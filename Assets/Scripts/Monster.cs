/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// The Monster class has a few different features that all aim to make monsters
// behave as realistically as possible.
//
// - **States:** first of all, the monster has several different states like
// IDLE, ATTACKING, MOVING and DEATH. The monster will randomly move around in
// a certain movement radius and try to attack any players in its aggro range.
// _Note: monsters use NavMeshAgents to move on the NavMesh._
//
// - **Aggro:** To save computations, we let Unity take care of finding players
// in the aggro range by simply adding a AggroArea _(see AggroArea.cs)_ sphere
// to the monster's children in the Hierarchy. We then use the OnTrigger
// functions to find players that are in the aggro area. The monster will always
// move to the nearest aggro player and then attack it as long as the player is
// in the follow radius. If the player happens to walk out of the follow
// radius then the monster will walk back to the start position quickly.
//
// - **Respawning:** The monsters have a _respawn_ property that can be set to
// true in order to make the monster respawn after it died. We developed the
// respawn system with simplicity in mind, there are no extra spawner objects
// needed. As soon as a monster dies, it will make itself invisible for a while
// and then go back to the starting position to respawn. This feature allows the
// developer to quickly drag monster Prefabs into the scene and place them
// anywhere, without worrying about spawners and spawn areas.
//
// - **Loot:** Dead monsters can also generate loot, based on the _lootItems_
// list. Each monster has a list of items with their dropchance, so that loot
// will always be generated randomly. Monsters can also randomly generate loot
// money between a minimum and a maximum amount.
using UnityEngine;
using Mirror;
using System.Linq;
using UMA;
using UMA.CharacterSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NetworkNavMeshAgent))]
public partial class Monster : Fighter
{
    [Header("Movement")]
    [Range(0, 1)] public float moveProbability = 0.1f; // chance per second
    public float moveDistance = 10;
    // monsters should follow their targets even if they run out of the movement
    // radius. the follow dist should always be bigger than the biggest archer's
    // attack range, so that archers will always pull aggro, even when attacking
    // from far away.
    public float followDistance = 20;
    [Range(0.1f, 1)] public float attackToMoveRangeRatio = 0.8f; // move as close as 0.8 * attackRange to a target
    [Header("Experience Reward")]
    public long rewardExperience = 10;
    public long rewardSpellExperience = 2;
    [Header("Apperance")]
    public Transform rightHandPosition;
    public ScriptableItem rightHandItem;
    public Transform leftHandPosition;
    public ScriptableItem leftHandItem;
    public RaceSpecification raceSpecification;
    [Header("Loot")]
    public int lootMoneyMin = 0;
    public int lootMoneyMax = 1000;
    public ItemDropChance[] dropChances;
    // note: Items have a .valid property that can be used to 'delete' an item.
    //       it's better than .RemoveAt() because we won't run into index-out-of
    //       range issues
    [Header("Respawn")]
    public float deathTime = 30f; // enough for animation & looting
    double deathTimeEnd; // double for long term precision
    public bool respawn = true;
    public float respawnTime = 10f;
    double respawnTimeEnd; // double for long term precision
    // save the start position for random movement distance and respawning
    Vector3 startPosition;
    // the last spell that was casted, to decide which one to cast next
    int lastSpell = -1;
    // Animation base values
    float lastButOneRotationY = 0;
    float lastButOneState = 0;
    float walkingSpeedLimit = GlobalVar.monsterWalkValue * GlobalVar.monsterWalkValue;
    // UMA exists
    bool isUMAExists = false;

    // networkbehaviour ////////////////////////////////////////////////////////
    protected override void Awake()
    {
        base.Awake();
        startPosition = transform.position;
    }
    public override void OnStartServer()
    {
        // call Entity's OnStartServer
        base.OnStartServer();
        // all monsters should spawn with full health and mana
        health = healthMax;
        mana = manaMax;
        // load spells based on spell templates
        foreach (ScriptableSpell spellData in spellTemplates)
            spells.Add(new Spell(spellData));
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        // apply apperance
        DynamicCharacterAvatar UMAAvatar = GetComponent<DynamicCharacterAvatar>();
        if (UMAAvatar && !isUMAExists)
        {
            foreach (StringFloat raceDef in raceSpecification.definitions)
            {
                UMAAvatar.predefinedDNA.AddDNA(raceDef.text, raceDef.value);
            }
            UMAAvatar.CharacterCreated.AddListener(UMACharacterCreated);
        }
        else
        {
            InitializeEquipment();
        }

        if (nameOverlay != null)
        {
            nameOverlay.color = PlayerPreferences.nameOverlayMonsterColor;
        }


    }
    void UMACharacterCreated(UMAData arg0)
    {
        Invoke("UMACharacterCreatedFinished", 0.1f);
    }
    void UMACharacterCreatedFinished()
    {
        isUMAExists = true;
        InitializeEquipment();
    }
    void InitializeEquipment()
    {
        //equip left and right hand
        UsableItem rightHandItemData = (UsableItem)rightHandItem;
        if (rightHandItemData.modelPrefab != null)
        {
            GameObject go;
            // load the new model
            go = Instantiate(rightHandItemData.modelPrefab);
            go.transform.SetParent(rightHandPosition, false);


            // apply dynamic states of items e.g. light is burning
            if (rightHandItem is LightItem)
            {
                LightElement le = go.GetComponent<LightElement>();
                le.SwitchDirect(true);
            }
        }
        UsableItem leftHandItemData = (UsableItem)leftHandItem;
        if (leftHandItemData.modelPrefab != null)
        {
            GameObject go;
            // load the new model
            go = Instantiate(leftHandItemData.modelPrefab);
            go.transform.SetParent(leftHandPosition, false);


            // apply dynamic states of items e.g. light is burning
            if (leftHandItem is LightItem)
            {
                LightElement le = go.GetComponent<LightElement>();
                le.SwitchDirect(true);
            }
        }
    }
    protected override void Start()
    {
        base.Start();
    }
    void LateUpdate()
    {
        // only if worth updating right now (e.g. a player is around)
        if (!IsWorthUpdating()) return;
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
            // calculate some basics
            float lastTurning = lastButOneRotationY - transform.rotation.eulerAngles.y;
            lastButOneRotationY = transform.rotation.eulerAngles.y;

            if (state == GlobalVar.stateMoving || state == GlobalVar.stateIdle)
            {
                animator.SetBool("LOCOMOTION", true);
                animator.SetFloat("Turning", lastTurning);
                animator.SetFloat("Speed", agent.velocity.sqrMagnitude / walkingSpeedLimit);
            }
            else
            {
                animator.SetBool("LOCOMOTION", false);
            }
            animator.SetBool("CASTING", state == GlobalVar.stateCasting);
            animator.SetBool("STUNNED", state == GlobalVar.stateStunned);
            animator.SetBool("DEAD", state == GlobalVar.stateDead);
            if (lastButOneState != state)
            {
                if (state == GlobalVar.stateDead)
                {
                    animator.SetInteger("ClipNumber", GlobalFunc.UnifiedRandom(GlobalVar.numberOfDeadAnimations, name.Length));
                }
                lastButOneState = state;
            }
            if (currentSpell >= 0)
            {
                animator.SetBool(spells[currentSpell].data.castAnimation, state == GlobalVar.stateCasting);
            }
        }
    }
    // OnDrawGizmos only happens while the Script is not collapsed
    void OnDrawGizmos()
    {
        // draw the movement area (around 'start' if game running,
        // or around current position if still editing)
        Vector3 startHelp = Application.isPlaying ? startPosition : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(startHelp, moveDistance);
        // draw the follow dist
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(startHelp, followDistance);
    }
    // finite state machine events /////////////////////////////////////////////
    bool EventDied()
    {
        return health == 0;
    }
    bool EventDeathTimeElapsed()
    {
        return state == GlobalVar.stateDead && NetworkTime.time >= deathTimeEnd;
    }
    bool EventRespawnTimeElapsed()
    {
        return state == GlobalVar.stateDead && respawn && NetworkTime.time >= respawnTimeEnd;
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
               Vector3.Distance(startPosition, target.collider.ClosestPoint(transform.position)) > followDistance;
    }
    bool EventTargetEnteredSafeZone()
    {
        return target != null && target.inSafeZone;
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
    bool EventMoveRandomly()
    {
        return Random.value <= moveProbability * Time.deltaTime;
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
        if (EventTargetTooFarToFollow())
        {
            // we had a target before, but it's out of follow range now.
            // clear it and go back to start. don't stay here.
            target = null;
            currentSpell = -1;
            agent.stoppingDistance = 0;
            agent.destination = startPosition;
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
        if (EventTargetEnteredSafeZone())
        {
            // if our target entered the safe zone, we need to be really careful
            // to avoid kiting.
            // -> players could pull a monster near a safe zone and then step in
            //    and out of it before/after attacks without ever getting hit by
            //    the monster
            // -> running back to start won't help, can still kit while running
            // -> warping back to start won't help, we might accidentally placed
            //    a monster in attack range of a safe zone
            // -> the 100% secure way is to die and hide it immediately. many
            //    popular MMOs do it the same way to avoid exploits.
            // => call Entity.OnDeath without rewards etc. and hide immediately
            base.OnDeath(); // no looting
            respawnTimeEnd = NetworkTime.time + respawnTime; // respawn in a while
            return GlobalVar.stateDead;
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
        if (EventMoveRandomly())
        {
            // walk to a random position in movement radius (from 'start')
            // note: circle y is 0 because we add it to start.y
            Vector2 circle2D = Random.insideUnitCircle * moveDistance;
            agent.stoppingDistance = 0;
            agent.destination = startPosition + new Vector3(circle2D.x, 0, circle2D.y);
            return GlobalVar.stateMoving;
        }
        //if (EventDeathTimeElapsed()) { } // don't care
        //if (EventRespawnTimeElapsed()) { } // don't care
        //if (EventMoveEnd()) { } // don't care
        //if (EventSpellFinished()) { } // don't care
        //if (EventTargetDisappeared()) { } // don't care
        return GlobalVar.stateIdle; // nothing interesting happened
    }
    [Server]
    int UpdateServer_MOVING()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
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
        if (EventTargetTooFarToFollow())
        {
            // we had a target before, but it's out of follow range now.
            // clear it and go back to start. don't stay here.
            target = null;
            currentSpell = -1;
            agent.stoppingDistance = 0;
            agent.destination = startPosition;
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
        if (EventTargetEnteredSafeZone())
        {
            // if our target entered the safe zone, we need to be really careful
            // to avoid kiting.
            // -> players could pull a monster near a safe zone and then step in
            //    and out of it before/after attacks without ever getting hit by
            //    the monster
            // -> running back to start won't help, can still kit while running
            // -> warping back to start won't help, we might accidentally placed
            //    a monster in attack range of a safe zone
            // -> the 100% secure way is to die and hide it immediately. many
            //    popular MMOs do it the same way to avoid exploits.
            // => call Entity.OnDeath without rewards etc. and hide immediately
            base.OnDeath(); // no looting
            respawnTimeEnd = NetworkTime.time + respawnTime; // respawn in a while
            return GlobalVar.stateDead;
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
        //if (EventDeathTimeElapsed()) { } // don't care
        //if (EventRespawnTimeElapsed()) { } // don't care
        //if (EventSpellFinished()) { } // don't care
        //if (EventTargetDisappeared()) { } // don't care
        //if (EventSpellRequest()) { } // don't care, finish movement first
        //if (EventMoveRandomly()) { } // don't care
        return GlobalVar.stateMoving; // nothing interesting happened
    }
    [Server]
    int UpdateServer_CASTING()
    {
        // keep looking at the target for server & clients (only Y rotation)
        if (target) LookAtY(target.transform.position);
        // events sorted by priority (e.g. target doesn't matter if we died)
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
        if (EventTargetEnteredSafeZone())
        {
            // cancel if the target matters for this spell
            if (spells[currentSpell].cancelCastIfTargetDied)
            {
                // if our target entered the safe zone, we need to be really careful
                // to avoid kiting.
                // -> players could pull a monster near a safe zone and then step in
                //    and out of it before/after attacks without ever getting hit by
                //    the monster
                // -> running back to start won't help, can still kit while running
                // -> warping back to start won't help, we might accidentally placed
                //    a monster in attack range of a safe zone
                // -> the 100% secure way is to die and hide it immediately. many
                //    popular MMOs do it the same way to avoid exploits.
                // => call Entity.OnDeath without rewards etc. and hide immediately
                base.OnDeath(); // no looting
                respawnTimeEnd = NetworkTime.time + respawnTime; // respawn in a while
                return GlobalVar.stateDead;
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
        //if (EventDeathTimeElapsed()) { } // don't care
        //if (EventRespawnTimeElapsed()) { } // don't care
        //if (EventMoveEnd()) { } // don't care
        //if (EventTargetTooFarToAttack()) { } // don't care, we were close enough when starting to cast
        //if (EventTargetTooFarToFollow()) { } // don't care, we were close enough when starting to cast
        //if (EventAggro()) { } // don't care, always have aggro while casting
        //if (EventSpellRequest()) { } // don't care, that's why we are here
        //if (EventMoveRandomly()) { } // don't care
        return GlobalVar.stateCasting; // nothing interesting happened
    }
    [Server]
    int UpdateServer_STUNNED()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died.
            OnDeath();
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
        if (EventRespawnTimeElapsed())
        {
            // respawn at the start position with full health, visibility, no loot
            inventory.Clear();
            Show();
            agent.Warp(startPosition); // recommended over transform.position
            Revive();
            return GlobalVar.stateIdle;
        }
        if (EventDeathTimeElapsed())
        {
            // we were lying around dead for long enough now.
            // hide while respawning, or disappear forever
            if (respawn) Hide();
            else NetworkServer.Destroy(gameObject);
            return GlobalVar.stateDead;
        }
        //if (EventSpellRequest()) { } // don't care
        //if (EventSpellFinished()) { } // don't care
        //if (EventMoveEnd()) { } // don't care
        //if (EventTargetDisappeared()) { } // don't care
        //if (EventTargetDied()) { } // don't care
        //if (EventTargetTooFarToFollow()) { } // don't care
        //if (EventTargetTooFarToAttack()) { } // don't care
        //if (EventTargetEnteredSafeZone()) { } // don't care
        //if (EventAggro()) { } // don't care
        //if (EventMoveRandomly()) { } // don't care
        //if (EventStunned()) { } // don't care
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
    // aggro ///////////////////////////////////////////////////////////////////
    // this function is called by entities that attack us and by AggroArea
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
            //    => we don't even need closestdistance here because they are in
            //       the aggro area anyway. transform.position is perfectly fine
            if (target == null)
            {
                target = entity;
            }
            else if (entity != target) // no need to check dist for same target
            {
                float oldDistance = Vector3.Distance(transform.position, target.transform.position);
                float newDistance = Vector3.Distance(transform.position, entity.transform.position);
                if (newDistance < oldDistance * 0.8) target = entity;
            }
        }
    }
    // loot ////////////////////////////////////////////////////////////////////
    // other scripts need to know if it still has valid loot (to show UI etc.)
    public bool HasLoot()
    {
        // any valid items?
        return inventory.Any(slot => slot.amount > 0);
    }
    // death ///////////////////////////////////////////////////////////////////
    [Server]
    protected override void OnDeath()
    {
        // take care of entity stuff
        base.OnDeath();
        // set death and respawn end times. we set both of them now to make sure
        // that everything works fine even if a monster isn't updated for a
        // while. so as soon as it's updated again, the death/respawn will
        // happen immediately if current time > end time.
        deathTimeEnd = NetworkTime.time + deathTime;
        respawnTimeEnd = deathTimeEnd + respawnTime; // after death time ended
        // generate items (note: can't use Linq because of SyncList)
        // first create a backpack
        containers.Add(new Container(GlobalVar.containerLoot, GlobalVar.containerTypeMobile, GlobalVar.lootMaxAmount, 0, "Loot", ""));
        int lastLootSlot = 0;
        // generate loot money
        long lootMoney = Money.MoneyRound((long)Random.Range(lootMoneyMin, lootMoneyMax));
        int lootGold = (int)Money.MoneyGold(lootMoney);
        int lootSilver = (int)Money.MoneySilver(lootMoney);
        int lootCopper = (int)Money.MoneyCopper(lootMoney);
        if (lootGold > 0)
        {
            ScriptableItem itemData;
            if (ScriptableItem.dict.TryGetValue(Money.itemNameGold.GetStableHashCode(), out itemData))
            {
                Item item = new Item(itemData);
                inventory.AddItem(item, GlobalVar.containerLoot, lastLootSlot, lootGold);
                lastLootSlot++;
            }
        }
        if (lootSilver > 0)
        {
            ScriptableItem itemData;
            if (ScriptableItem.dict.TryGetValue(Money.itemNameSilver.GetStableHashCode(), out itemData))
            {
                Item item = new Item(itemData);
                inventory.AddItem(item, GlobalVar.containerLoot, lastLootSlot, lootSilver);
                lastLootSlot++;
            }
        }
        if (lootCopper > 0)
        {
            ScriptableItem itemData;
            if (ScriptableItem.dict.TryGetValue(Money.itemNameCopper.GetStableHashCode(), out itemData))
            {
                Item item = new Item(itemData);
                inventory.AddItem(item, GlobalVar.containerLoot, lastLootSlot, lootCopper);
                lastLootSlot++;
            }
        }
        // now the oter items
        foreach (ItemDropChance itemChance in dropChances)
            if (Random.value <= itemChance.probability)
            {
                inventory.AddItem(new Item(itemChance.item), GlobalVar.containerLoot, lastLootSlot, 1);
                if (lastLootSlot++ >= GlobalVar.lootMaxAmount)
                    break;
            }
    }
    // spells //////////////////////////////////////////////////////////////////
    // monsters always have a weapon
    public override bool HasCastWeapon() { return true; }
    // CanAttack check
    // we use 'is' instead of 'GetType' so that it works for inherited types too
    public override bool CanAttack(Entity entity)
    {
        return base.CanAttack(entity) &&
               (entity is Player ||
                entity is Pet ||
                entity is Mount);
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
