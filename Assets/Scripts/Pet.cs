/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using Mirror;
using System;
using System.Linq;
using System.Collections.Generic;
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NetworkNavMeshAgent))]
public partial class Pet : Summonable
{
    [Header("Text Meshes")]
    public TextMesh ownerNameOverlay;
    [Header("Movement")]
    public float returnDistance = 25; // return to player if dist > ...
    // pets should follow their targets even if they run out of the movement
    // radius. the follow dist should always be bigger than the biggest archer's
    // attack range, so that archers will always pull aggro, even when attacking
    // from far away.
    public float followDistance = 20;
    // pet should teleport if the owner gets too far away for whatever reason
    public float teleportDistance = 30;
    [Range(0.1f, 1)] public float attackToMoveRangeRatio = 0.8f; // move as close as 0.8 * attackRange to a target
    public override float speed
    {
        get
        {
            // use owner's speed if found, so that the pet can still follow the
            // owner if he is riding a mount, etc.
            return owner != null ? owner.speed : base.speed;
        }
    }
    [Header("Death")]
    public float deathTime = 2; // enough for animation
    double deathTimeEnd; // double for long term precision
    [Header("Behaviour")]
    [SyncVar] public bool defendOwner = true; // attack what attacks the owner
    [SyncVar] public bool autoAttack = true; // attack what the owner attacks
    // the last spell that was casted, to decide which one to cast next
    int lastSpell = -1;
    // sync to item ////////////////////////////////////////////////////////////
    protected override ItemSlot SyncStateToItemSlot(ItemSlot slot)
    {
        // pet also has experience, unlike summonable. sync that too.
        slot = base.SyncStateToItemSlot(slot);
        return slot;
    }
    // networkbehaviour ////////////////////////////////////////////////////////
    protected override void Awake()
    {
        base.Awake();
    }
    public override void OnStartServer()
    {
        // call Entity's OnStartServer
        base.OnStartServer();
        // load spells based on spell templates
        foreach (ScriptableSpell spellData in spellTemplates)
            spells.Add(new Spell(spellData));
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (nameOverlay != null)
            nameOverlay.color = PlayerPreferences.nameOverlayPetColor;
    }
    protected override void Start()
    {
        base.Start();
    }
    void LateUpdate()
    {
        // pass parameters to animation state machine
        // => passing the states directly is the most reliable way to avoid all
        //    kinds of glitches like movement sliding, attack twitching, etc.
        // => make sure to import all looping animations like idle/run/attack
        //    with 'loop time' enabled, otherwise the client might only play it
        //    once
        // => only play moving animation while the agent is actually moving. the
        //    MOVING state might be delayed to due latency or we might be in
        //    MOVING while a path is still pending, etc.
        // => spell names are assumed to be boolean parameters in animator
        //    so we don't need to worry about an animation number etc.
        if (isClient) // no need for animations on the server
        {
            animator.SetBool("MOVING", state == GlobalVar.stateMoving && agent.velocity != Vector3.zero);
            animator.SetBool("CASTING", state == GlobalVar.stateCasting);
            animator.SetBool("STUNNED", state == GlobalVar.stateStunned);
            animator.SetBool("DEAD", state == GlobalVar.stateDead);
            foreach (Spell spell in spells)
                animator.SetBool(spell.name, spell.CastTimeRemaining() > 0);
        }
    }
    // OnDrawGizmos only happens while the Script is not collapsed
    void OnDrawGizmos()
    {
        // draw the movement area (around 'start' if game running,
        // or around current position if still editing)
        Vector3 startHelp = Application.isPlaying ? owner.petDestination : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(startHelp, returnDistance);
        // draw the follow dist
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(startHelp, followDistance);
    }
    void OnDestroy()
    {
        // Unity bug: isServer is false when called in host mode. only true when
        // called in dedicated mode. so we need a workaround:
        if (NetworkServer.active) // isServer
        {
            // keep player's pet item up to date
            SyncToOwnerItem();
        }
    }
    // always update pets. IsWorthUpdating otherwise only updates if has observers,
    // but pets should still be updated even if they are too far from any observers,
    // so that they continue to run to their owner.
    public override bool IsWorthUpdating() { return true; }
    // finite state machine events /////////////////////////////////////////////
    bool EventOwnerDisappeared()
    {
        return owner == null;
    }
    bool EventDied()
    {
        return health == 0;
    }
    bool EventDeathTimeElapsed()
    {
        return state == GlobalVar.stateDead && NetworkTime.time >= deathTimeEnd;
    }
    bool EventTargetDisappeared()
    {
        return target == null;
    }
    bool EventTargetDied()
    {
        return target != null && target.health == 0;
    }
    bool EventTargetTooFarToAttack()
    {
        Vector3 destination;
        return target != null &&
               0 <= currentSpell && currentSpell < spells.Count &&
               !CastCheckDistance(spells[currentSpell], out destination);
    }
    bool EventTargetTooFarToFollow()
    {
        return target != null &&
               Vector3.Distance(owner.petDestination, target.collider.ClosestPoint(transform.position)) > followDistance;
    }
    bool EventNeedReturnToOwner()
    {
        return Vector3.Distance(owner.petDestination, transform.position) > returnDistance;
    }
    bool EventNeedTeleportToOwner()
    {
        return Vector3.Distance(owner.petDestination, transform.position) > teleportDistance;
    }
    bool EventAggro()
    {
        return target != null && target.health > 0;
    }
    bool EventSpellRequest()
    {
        return 0 <= currentSpell && currentSpell < spells.Count;
    }
    bool EventSpellFinished()
    {
        return 0 <= currentSpell && currentSpell < spells.Count &&
               spells[currentSpell].CastTimeRemaining() == 0;
    }
    bool EventMoveEnd()
    {
        return state == GlobalVar.stateMoving && !IsMoving();
    }
    bool EventStunned()
    {
        return NetworkTime.time <= stunTimeEnd;
    }
    // finite state machine - server ///////////////////////////////////////////
    [Server]
    int UpdateServer_IDLE()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventOwnerDisappeared())
        {
            // owner might disconnect or get destroyed for some reason
            NetworkServer.Destroy(gameObject);
            return GlobalVar.stateIdle;
        }
        if (EventDied())
        {
            // we died.
            OnDeath();
            return GlobalVar.stateDead;
        }
        if (EventStunned())
        {
            agent.ResetMovement();
            return GlobalVar.stateStunned;
        }
        if (EventTargetDied())
        {
            // we had a target before, but it died now. clear it.
            target = null;
            currentSpell = -1;
            return GlobalVar.stateIdle;
        }
        if (EventNeedTeleportToOwner())
        {
            agent.Warp(owner.petDestination);
            return GlobalVar.stateIdle;
        }
        if (EventNeedReturnToOwner())
        {
            // return to owner only while IDLE
            target = null;
            currentSpell = -1;
            agent.stoppingDistance = 0;
            agent.destination = owner.petDestination;
            return GlobalVar.stateMoving;
        }
        if (EventTargetTooFarToFollow())
        {
            // we had a target before, but it's out of follow range now.
            // clear it and go back to start. don't stay here.
            target = null;
            currentSpell = -1;
            agent.stoppingDistance = 0;
            agent.destination = owner.petDestination;
            return GlobalVar.stateMoving;
        }
        if (EventTargetTooFarToAttack())
        {
            // we had a target before, but it's out of attack range now.
            // follow it. (use collider point(s) to also work with big entities)
            agent.stoppingDistance = CurrentCastRange() * attackToMoveRangeRatio;
            agent.destination = target.collider.ClosestPoint(transform.position);
            return GlobalVar.stateMoving;
        }
        if (EventSpellRequest())
        {
            // we had a target in attack range before and trying to cast a spell
            // on it. check self (alive, mana, weapon etc.) and target
            Spell spell = spells[currentSpell];
            if (CastCheckSelf(spell) && CastCheckTarget(spell))
            {
                // start casting
                StartCastSpell(spell);
                return GlobalVar.stateCasting;
            }
            else
            {
                // invalid target. stop trying to cast.
                target = null;
                currentSpell = -1;
                return GlobalVar.stateIdle;
            }
        }
        if (EventAggro())
        {
            // target in attack range. try to cast a first spell on it
            if (spells.Count > 0) currentSpell = NextSpell();
            else Debug.LogError(name + " has no spells to attack with.");
            return GlobalVar.stateIdle;
        }
        //if (EventMoveEnd()) { } // don't care
        //if (EventDeathTimeElapsed()) { } // don't care
        //if (EventSpellFinished()) { } // don't care
        //if (EventTargetDisappeared()) { } // don't care
        return GlobalVar.stateIdle; // nothing interesting happened
    }
    [Server]
    int UpdateServer_MOVING()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventOwnerDisappeared())
        {
            // owner might disconnect or get destroyed for some reason
            NetworkServer.Destroy(gameObject);
            return GlobalVar.stateIdle;
        }
        if (EventDied())
        {
            // we died.
            OnDeath();
            agent.ResetMovement();
            return GlobalVar.stateDead;
        }
        if (EventStunned())
        {
            agent.ResetMovement();
            return GlobalVar.stateStunned;
        }
        if (EventMoveEnd())
        {
            // we reached our destination.
            return GlobalVar.stateIdle;
        }
        if (EventTargetDied())
        {
            // we had a target before, but it died now. clear it.
            target = null;
            currentSpell = -1;
            agent.ResetMovement();
            return GlobalVar.stateIdle;
        }
        if (EventNeedTeleportToOwner())
        {
            agent.Warp(owner.petDestination);
            return GlobalVar.stateIdle;
        }
        if (EventTargetTooFarToFollow())
        {
            // we had a target before, but it's out of follow range now.
            // clear it and go back to start. don't stay here.
            target = null;
            currentSpell = -1;
            agent.stoppingDistance = 0;
            agent.destination = owner.petDestination;
            return GlobalVar.stateMoving;
        }
        if (EventTargetTooFarToAttack())
        {
            // we had a target before, but it's out of attack range now.
            // follow it. (use collider point(s) to also work with big entities)
            agent.stoppingDistance = CurrentCastRange() * attackToMoveRangeRatio;
            agent.destination = target.collider.ClosestPoint(transform.position);
            return GlobalVar.stateMoving;
        }
        if (EventAggro())
        {
            // target in attack range. try to cast a first spell on it
            // (we may get a target while randomly wandering around)
            if (spells.Count > 0) currentSpell = NextSpell();
            else Debug.LogError(name + " has no spells to attack with.");
            agent.ResetMovement();
            return GlobalVar.stateIdle;
        }
        //if (EventNeedReturnToOwner()) { } // don't care
        //if (EventDeathTimeElapsed()) { } // don't care
        //if (EventSpellFinished()) { } // don't care
        //if (EventTargetDisappeared()) { } // don't care
        //if (EventSpellRequest()) { } // don't care, finish movement first
        return GlobalVar.stateMoving; // nothing interesting happened
    }
    [Server]
    int UpdateServer_CASTING()
    {
        // keep looking at the target for server & clients (only Y rotation)
        if (target) LookAtY(target.transform.position);
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventOwnerDisappeared())
        {
            // owner might disconnect or get destroyed for some reason
            NetworkServer.Destroy(gameObject);
            return GlobalVar.stateIdle;
        }
        if (EventDied())
        {
            // we died.
            OnDeath();
            return GlobalVar.stateDead;
        }
        if (EventStunned())
        {
            currentSpell = -1;
            agent.ResetMovement();
            return GlobalVar.stateStunned;
        }
        if (EventTargetDisappeared())
        {
            // cancel if the target matters for this spell
            if (spells[currentSpell].cancelCastIfTargetDied)
            {
                currentSpell = -1;
                target = null;
                return GlobalVar.stateIdle;
            }
        }
        if (EventTargetDied())
        {
            // cancel if the target matters for this spell
            if (spells[currentSpell].cancelCastIfTargetDied)
            {
                currentSpell = -1;
                target = null;
                return GlobalVar.stateIdle;
            }
        }
        if (EventSpellFinished())
        {
            // finished casting. apply the spell on the target.
            FinishCastSpell(spells[currentSpell]);
            // did the target die? then clear it so that the monster doesn't
            // run towards it if the target respawned
            if (target.health == 0) target = null;
            // go back to IDLE
            lastSpell = currentSpell;
            currentSpell = -1;
            return GlobalVar.stateIdle;
        }
        //if (EventMoveEnd()) { } // don't care
        //if (EventDeathTimeElapsed()) { } // don't care
        //if (EventNeedTeleportToOwner()) { } // don't care
        //if (EventNeedReturnToOwner()) { } // don't care
        //if (EventTargetTooFarToAttack()) { } // don't care, we were close enough when starting to cast
        //if (EventTargetTooFarToFollow()) { } // don't care, we were close enough when starting to cast
        //if (EventAggro()) { } // don't care, always have aggro while casting
        //if (EventSpellRequest()) { } // don't care, that's why we are here
        return GlobalVar.stateCasting; // nothing interesting happened
    }
    [Server]
    int UpdateServer_STUNNED()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventOwnerDisappeared())
        {
            // owner might disconnect or get destroyed for some reason
            NetworkServer.Destroy(gameObject);
            return GlobalVar.stateIdle;
        }
        if (EventDied())
        {
            // we died.
            OnDeath();
            currentSpell = -1; // in case we died while trying to cast
            return GlobalVar.stateDead;
        }
        if (EventStunned())
        {
            return GlobalVar.stateStunned;
        }
        // go back to idle if we aren't stunned anymore and process all new
        // events there too
        return GlobalVar.stateIdle;
    }
    [Server]
    int UpdateServer_DEAD()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventOwnerDisappeared())
        {
            // owner might disconnect or get destroyed for some reason
            NetworkServer.Destroy(gameObject);
            return GlobalVar.stateDead;
        }
        if (EventDeathTimeElapsed())
        {
            // we were lying around dead for long enough now.
            // hide while respawning, or disappear forever
            NetworkServer.Destroy(gameObject);
            return GlobalVar.stateDead;
        }
        //if (EventSpellRequest()) { } // don't care
        //if (EventSpellFinished()) { } // don't care
        //if (EventMoveEnd()) { } // don't care
        //if (EventNeedTeleportToOwner()) { } // don't care
        //if (EventNeedReturnToOwner()) { } // don't care
        //if (EventTargetDisappeared()) { } // don't care
        //if (EventTargetDied()) { } // don't care
        //if (EventTargetTooFarToFollow()) { } // don't care
        //if (EventTargetTooFarToAttack()) { } // don't care
        //if (EventAggro()) { } // don't care
        //if (EventDied()) { } // don't care, of course we are dead
        return GlobalVar.stateDead; // nothing interesting happened
    }
    [Server]
    protected override int UpdateServer()
    {
        if (state == GlobalVar.stateIdle) return UpdateServer_IDLE();
        if (state == GlobalVar.stateMoving) return UpdateServer_MOVING();
        if (state == GlobalVar.stateCasting) return UpdateServer_CASTING();
        if (state == GlobalVar.stateStunned) return UpdateServer_STUNNED();
        if (state == GlobalVar.stateDead) return UpdateServer_DEAD();
        Debug.LogError("invalid state:" + state);
        return GlobalVar.stateIdle;
    }
    // finite state machine - client ///////////////////////////////////////////
    [Client]
    protected override void UpdateClient()
    {
        if (state == GlobalVar.stateCasting)
        {
            // keep looking at the target for server & clients (only Y rotation)
            if (target) LookAtY(target.transform.position);
        }
    }
    // overlays ////////////////////////////////////////////////////////////////
    protected override void UpdateOverlays()
    {
        base.UpdateOverlays();
        if (ownerNameOverlay != null)
        {
            if (owner != null)
            {
                Player player = Player.localPlayer;
                // find local player (null while in character selection)
                if (player != null)
                {
                    ownerNameOverlay.text = player.KnownName(owner.id);
                    ownerNameOverlay.color = owner.nameOverlay.color;
                }
            }
            else ownerNameOverlay.text = "?";
        }
    }
    // combat //////////////////////////////////////////////////////////////////
    // custom DealDamageAt function that also rewards experience if we killed
    // the monster
    [Server]
    public override void DealDamageAt(Entity entity, int amount,  float stunTime = 0)
    {
        // deal damage with the default function
        base.DealDamageAt(entity, amount,  stunTime);
        // a monster?
        if (entity is Monster)
        {
            // forward to owner to share rewards with everyone
            owner.OnDamageDealtToMonster((Monster)entity);
        }
        // a player?
        // (see murder code section comments to understand the system)
        else if (entity is Player)
        {
            // forward to owner for murderer detection etc.
            owner.OnDamageDealtToPlayer((Player)entity);
        }
        // a pet?
        // (see murder code section comments to understand the system)
        else if (entity is Pet)
        {
            owner.OnDamageDealtToPet((Pet)entity);
        }
    }

    // aggro ///////////////////////////////////////////////////////////////////
    // this function is called by entities that attack us
    [ServerCallback]
    public override void OnAggro(Entity entity)
    {
        // are we alive, and is the entity alive and of correct type?
        if (entity != null && CanAttack(entity))
        {
            // no target yet(==self), or closer than current target?
            // => has to be at least 20% closer to be worth it, otherwise we
            //    may end up nervously switching between two targets
            // => we do NOT use Utils.ClosestDistance, because then we often
            //    also end up nervously switching between two animated targets,
            //    since their collides moves with the animation.
            if (target == null)
            {
                target = entity;
            }
            else
            {
                float oldDistance = Vector3.Distance(transform.position, target.transform.position);
                float newDistance = Vector3.Distance(transform.position, entity.transform.position);
                if (newDistance < oldDistance * 0.8) target = entity;
            }
        }
    }
    // death ///////////////////////////////////////////////////////////////////
    [Server]
    protected override void OnDeath()
    {
        // take care of entity stuff
        base.OnDeath();
        // set death end time. we set it now to make sure that everything works
        // fine even if a pet isn't updated for a while. so as soon as it's
        // updated again, the death/respawn will happen immediately if current
        // time > end time.
        deathTimeEnd = NetworkTime.time + deathTime;
        // keep player's pet item up to date
        SyncToOwnerItem();
    }
    // spells //////////////////////////////////////////////////////////////////
    // monsters always have a weapon
    public override bool HasCastWeapon() { return true; }
    // CanAttack check
    // we use 'is' instead of 'GetType' so that it works for inherited types too
    public override bool CanAttack(Entity entity)
    {
        return base.CanAttack(entity) &&
               (entity is Monster ||
                (entity is Player && entity != owner) ||
                (entity is Pet && ((Pet)entity).owner != owner) ||
                (entity is Mount && ((Mount)entity).owner != owner));
    }
    // helper function to get the current cast range (if casting anything)
    public float CurrentCastRange()
    {
        return 0 <= currentSpell && currentSpell < spells.Count ? spells[currentSpell].CastRange(this) : 0;
    }
    // helper function to decide which spell to cast
    // => we got through spells one after another, this is better than selecting
    //    a random spell because it allows for some planning like:
    //    'strong skeleton always starts with a stun' etc.
    int NextSpell()
    {
        // find the next ready spell, starting at 'lastSpell+1' (= next one)
        // and looping at max once through them all (up to spell.Count)
        //  note: no spells.count == 0 check needed, this works with empty lists
        //  note: also works if lastSpell is still -1 from initialization
        for (int i = 0; i < spells.Count; ++i)
        {
            int index = (lastSpell + 1 + i) % spells.Count;
            // could we cast this spell right now? (enough mana, spell ready, etc.)
            if (CastCheckSelf(spells[index]))
                return index;
        }
        return -1;
    }
}
