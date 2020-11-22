/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// The Entity class is rather simple. It contains a few basic entity properties
// like health and mana that all inheriting classes like Players and
// Monsters can use.
//
// Entities also have a _target_ Entity that can't be synchronized with a
// SyncVar. Instead we created a EntityTargetSync component that takes care of
// that for us.
//
// Entities use a deterministic finite state machine to handle IDLE/MOVING/DEAD/
// CASTING etc. states and events. Using a deterministic FSM means that we react
// to every single event that can happen in every state (as opposed to just
// taking care of the ones that we care about right now). This means a bit more
// code, but it also means that we avoid all kinds of weird situations like 'the
// monster doesn't react to a dead target when casting' etc.
// The next state is always set with the return value of the UpdateServer
// function. It can never be set outside of it, to make sure that all events are
// truly handled in the state machine and not outside of it. Otherwise we may be
// tempted to set a state in CmdBeingTrading etc., but would likely forget of
// special things to do depending on the current state.
//
// Entities also need a kinematic Rigidbody so that OnTrigger functions can be
// called. Note that there is currently a Unity bug that slows down the agent
// when having lots of FPS(300+) if the Rigidbody's Interpolate option is
// enabled. So for now it's important to disable Interpolation - which is a good
// idea in general to increase performance.
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

// note: no animator required, towers, dummies etc. may not have one
[RequireComponent(typeof(Rigidbody))] // kinematic, only needed for OnTrigger
[RequireComponent(typeof(NetworkProximityGridChecker))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public abstract partial class Entity : NetworkBehaviour
{
    [Header("Components")]
    public NavMeshAgent agent;
    public NetworkProximityGridChecker proxchecker;
    public Animator animator;
    new public Collider collider;
    public AudioSource audioSource;
    // finite state machine
    // -> state only writable by entity class to avoid all kinds of confusion
    [Header("State")]
    [SyncVar, SerializeField] int _state = GlobalVar.stateIdle;
    public int state { get { return _state; } }
    // 'Entity' can't be SyncVar and NetworkIdentity causes errors when null,
    // so we use [SyncVar] GameObject and wrap it for simplicity
    [Header("Target")]
    [SyncVar] GameObject _target;
    public Entity target
    {
        get { return _target != null ? _target.GetComponent<Entity>() : null; }
        set { _target = value != null ? value.gameObject : null; }
    }
    [Header("Level")]
    [SyncVar] public int HealthLevel = 100;
    [SyncVar] public int HealthRecoveryLevel = 100;
    [SyncVar] public int ManaLevel = 100;
    [SyncVar] public int ManaRecoveryLevel = 100;
    [SyncVar] public int SpeedLevel = 100;
    [Header("Health")]
    [SyncVar] int _health = 1000000;
    public virtual int healthMaxBase
    {
        get
        {
            return Convert.ToInt32(GlobalVar.healthBaseValue* HealthLevel / 100);
        }
    }
    public virtual int healthMax
    {
        get
        {
            // base + buffs
            int buffBonus = buffs.Sum(buff => buff.bonusHealth);
            return healthMaxBase  + buffBonus;
        }
    }
    public bool invincible = false; // GMs, Npcs, ...
    [SyncVar] int _injury = 0;
    public int injury
    {
        get { return _injury; }
        set { _injury = Mathf.Clamp(value, 0, (int)(healthMax * GlobalVar.healthMaxInjury)); }
    }
    public int health
    {
        get { return Mathf.Min(_health, healthMax - injury); } // min in case hp>hpmax after buff ends etc.
        set { _health = Mathf.Clamp(value, 0, healthMax - injury); }
    }
    public bool healthRecovery = true; // can be disabled in combat etc.
    public virtual int healthRecoveryRateBase
    {
        get
        {
           return Convert.ToInt32(GlobalVar.healthRecoveryBaseValue* HealthRecoveryLevel / 100);
        }
    }
    public virtual int healthRecoveryRate
    {
        get
        {
            // base + buffs
            float buffBonus = buffs.Sum(buff => buff.bonusHealthPerSecond);
            return Convert.ToInt32(healthRecoveryRateBase  + buffBonus);
        }
    }
    [Command]
    public void CmdSetTargetHealth(int value)
    {
        if (target)
        {
            target.health = value;
        }
    }

    [Header("Mana")]
    [SyncVar] int _mana = 100000;
    public virtual int manaMaxBase
    {
        get
        {
            return Convert.ToInt32(GlobalVar.manaBaseValue * ManaLevel / 100);
        }
    }
    public virtual int manaMax
    {
        get
        {
            // base + buffs
            int buffBonus = buffs.Sum(buff => buff.bonusMana);
            return manaMaxBase + buffBonus;
        }
    }
    public int mana
    {
        get { return Mathf.Min(_mana, manaMax); } // min in case hp>hpmax after buff ends etc.
        set { _mana = Mathf.Clamp(value, 0, manaMax); }
    }
    public bool manaRecovery = true; // can be disabled in combat etc.
   public virtual int manaRecoveryRateBase
    {
        get
        {
            return Convert.ToInt32(GlobalVar.manaBaseValue * ManaRecoveryLevel / 100);
        }
    }
    public virtual int manaRecoveryRate
    {
        get
        {
            // base + buffs
            int buffBonus = (int) buffs.Sum(buff => buff.bonusManaPerSecond);
            return manaRecoveryRateBase + buffBonus;
        }
    }
    [Command]
    public void CmdSetTargetMana(int value)
    {
        // validate
        if (target)
        {
            target.mana = value;
        }
    }

    public virtual float speedBase
    {
        get
        {
            return GlobalVar.speedBaseValue * SpeedLevel / 100;
        }
    }
    public virtual float speed
    {
        get
        {
            // base +  buffs
            float buffBonus = buffs.Sum(buff => buff.bonusSpeed);
            return speedBase + buffBonus;
        }
    }


    [Header("Damage Popup")]
    public GameObject damagePopupPrefab;  //>>> sollt es nicht mehr geben
    // spell system for all entities (players, monsters, npcs, towers, ...)
    // 'spellTemplates' are the available spells (first one is default attack)
    // 'spells' are the loaded spells with cooldowns etc.
    [Header("Spells & Buffs")]
    public ScriptableSpell[] spellTemplates;
    public SyncListSpell spells = new SyncListSpell();
    public SyncListBuff buffs = new SyncListBuff(); // active buffs
    // current spell (synced because we need it as an animation parameter)
    [SyncVar, HideInInspector] public int currentSpell = -1;
    // current working hand (synced because we need it as an animation parameter)
    [SyncVar, HideInInspector] public int workingHand = -1;
    // effect mount is where the arrows/fireballs/etc. are spawned
    // -> can be overwritten, e.g. for mages to set it to the weapon's effect
    //    mount
    // -> assign to right hand if in doubt!
#pragma warning disable 0649 //warning CS0649: Field 'Entity._effectMount' is never assigned to, and will always have its default value null
    [SerializeField] Transform _effectMount;
    public virtual Transform effectMount { get { return _effectMount; } }
#pragma warning restore 0649
    // all entities should have an inventory, not just the player.
    // useful for monster loot, chests, etc.
    [Header("Inventory")]
    public SyncListItemSlot inventory = new SyncListItemSlot();
    public SyncListContainer containers = new SyncListContainer();

    // 3D text mesh for name above the entity's head
    [Header("Text Meshes")]
    public bool showOverlay = true;
    public string displayedName;
    public TextMesh nameOverlay;
    public TextMesh healthOverlay;
    public TextMesh stunnedOverlay;
    // every entity can be stunned by setting stunEndTime
    protected double stunTimeEnd;
    // safe zone flag
    // -> needs to be in Entity because both player and pet need it
    [HideInInspector] public bool inSafeZone;
    // networkbehaviour ////////////////////////////////////////////////////////
    protected virtual void Awake()
    {
        // empty, may be overwritten
    }
    public override void OnStartServer()
    {
        // health recovery every second
        InvokeRepeating("Recover", 1, 1);
        // dead if spawned without health
        if (health == 0) _state = GlobalVar.stateDead;
    }
    protected virtual void Start()
    {
        // disable animator on server. this is a huge performance boost and
        // definitely worth one line of code (1000 monsters: 22 fps => 32 fps)
        // (!isClient because we don't want to do it in host mode either)
        // (OnStartServer doesn't know isClient yet, Start is the only option)
        if (!isClient) animator.enabled = false;

        if (isClient)
        {
            nameOverlay.gameObject.SetActive(false);
            Renderer rendName = nameOverlay.gameObject.GetComponent<Renderer>();
            rendName.enabled = true;
            if (healthOverlay)
            {
                Renderer rendHealth = healthOverlay.gameObject.GetComponent<Renderer>();
                healthOverlay.gameObject.SetActive(false);
                rendHealth.enabled = true;
            }
            if (stunnedOverlay)
            {
                Renderer rendStun = stunnedOverlay.gameObject.GetComponent<Renderer>();
                rendStun.enabled = true;
                stunnedOverlay.gameObject.SetActive(false);
            }
        }
    }
    // monsters, npcs etc. don't have to be updated if no player is around
    // checking observers is enough, because lonely players have at least
    // themselves as observers, so players will always be updated
    // and dead monsters will respawn immediately in the first update call
    // even if we didn't update them in a long time (because of the 'end'
    // times)
    // -> update only if:
    //    - observers are null (they are null in clients)
    //    - if they are not null, then only if at least one (on server)
    //    - if the entity is hidden, otherwise it would never be updated again
    //      because it would never get new observers
    // -> can be overwritten if necessary (e.g. pets might be too far from
    //    observers but should still be updated to run to owner)
    public virtual bool IsWorthUpdating()
    {
        return netIdentity.observers == null ||
               netIdentity.observers.Count > 0 ||
               IsHidden();
    }
    // entity logic will be implemented with a finite state machine
    // -> we should react to every state and to every event for correctness
    // -> we keep it functional for simplicity
    // note: can still use LateUpdate for Updates that should happen in any case
    void Update()
    {
        // only update if it's worth updating (see IsWorthUpdating comments)
        // -> we also clear the target if it's hidden, so that players don't
        //    keep hidden (respawning) monsters as target, hence don't show them
        //    as target again when they are shown again
        if (IsWorthUpdating())
        {
            // always apply speed to agent
            agent.speed = speed;
            if (isClient)
            {
                UpdateClient();
            }
            if (isServer)
            {
                CleanupBuffs();
                if (target != null && target.IsHidden()) target = null;
                _state = UpdateServer();
            }
        }
        // update overlays in any case, except on server-only mode
        // (also update for character selection previews etc. then)
        if (!isServerOnly) UpdateOverlays();
    }
    // update for server. should return the new state.
    protected abstract int UpdateServer();
    // update for client.
    protected abstract void UpdateClient();
    // can be overwritten for more overlays
    protected virtual void UpdateOverlays()
    {
        if (showOverlay)
        {
            bool isDisplayOverlay = false;
            Player player = Player.localPlayer;
            if (player != null)
            {
                float distance = Vector3.Distance(player.transform.position, transform.position);
                if (distance < player.distanceDetectionPerson && !isLocalPlayer)
                    isDisplayOverlay = true;

                if (nameOverlay != null)
                {
                    nameOverlay.gameObject.SetActive(isDisplayOverlay);
                    if (isDisplayOverlay && !(this is Player))
                    {
                        if (displayedName.Length > 0)
                        {
                            nameOverlay.text = displayedName;
                        }
                        else
                        {
                            nameOverlay.text = name;
                        }
                    }
                }

                if (healthOverlay != null)
                {
                    healthOverlay.gameObject.SetActive(isDisplayOverlay);
                    if (isDisplayOverlay)
                    {

                        if (player.abilities.diagnosis == 0)
                        {
                            if (health == 0)
                            {
                                healthOverlay.color = PlayerPreferences.healthColorDeath;
                                healthOverlay.text = GlobalVar.healthTextDeath;
                            }
                            else
                            {
                                healthOverlay.color = PlayerPreferences.healthColorUnharmed;
                                healthOverlay.text = GlobalVar.healthTextUnharmed;
                            }
                        }
                        else
                        {
                            float healthPercent = HealthPercent();
                            if (healthPercent > GlobalVar.healthLimitUnharmed)
                            {
                                healthOverlay.color = PlayerPreferences.healthColorUnharmed;
                                healthOverlay.text = GlobalVar.healthTextUnharmed;
                            }
                            else if (healthPercent > GlobalVar.healthLimitSlightlyWounded)
                            {
                                healthOverlay.color = PlayerPreferences.healthColorSlightlyWounded;
                                healthOverlay.text = GlobalVar.healthTextSlightlyWounded;
                            }
                            else if (healthPercent > GlobalVar.healthLimitWounded)
                            {
                                healthOverlay.color = PlayerPreferences.healthColorWounded;
                                healthOverlay.text = GlobalVar.healthTextWounded;
                            }
                            else if (healthPercent > GlobalVar.healthLimitBadlyWounded)
                            {
                                healthOverlay.color = PlayerPreferences.healthColorBadlyWounded;
                                healthOverlay.text = GlobalVar.healthTextBadlyWounded;
                            }
                            else if (healthPercent > 0)
                            {
                                healthOverlay.color = PlayerPreferences.healthColorNearDeath;
                                healthOverlay.text = GlobalVar.healthTextNearDeath;
                            }
                            else
                            {
                                healthOverlay.color = PlayerPreferences.healthColorDeath;
                                healthOverlay.text = GlobalVar.healthTextDeath;
                            }
                        }
                    }


                    if (stunnedOverlay != null)
                    {
                        if (state == GlobalVar.stateStunned && isDisplayOverlay)
                        {
                            stunnedOverlay.gameObject.SetActive(true);
                            stunnedOverlay.text = string.Format("stunned {0:F1} s", stunTimeEnd - NetworkTime.time);
                        }
                        else
                        {
                            stunnedOverlay.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    // visibility //////////////////////////////////////////////////////////////
    // hide a entity
    // note: using SetActive won't work because its not synced and it would
    //       cause inactive objects to not receive any info anymore
    // note: this won't be visible on the server as it always sees everything.
    [Server]
    public void Hide()
    {
        proxchecker.forceHidden = true;
    }
    [Server]
    public void Show()
    {
        proxchecker.forceHidden = false;
    }
    // is the entity currently hidden?
    // note: usually the server is the only one who uses forceHidden, the
    //       client usually doesn't know about it and simply doesn't see the
    //       GameObject.
    public bool IsHidden()
    {
        return proxchecker.forceHidden;
    }
    public float VisRange()
    {
        return NetworkProximityGridChecker.visRange;
    }
    // look at a transform while only rotating on the Y axis (to avoid weird
    // tilts)
    public void LookAtY(Vector3 position)
    {
        transform.LookAt(new Vector3(position.x, transform.position.y, position.z));
    }
    public bool IsMoving()
    {
        // -> agent.hasPath will be true if stopping distance > 0, so we can't
        //    really rely on that.
        // -> pathPending is true while calculating the path, which is good
        // -> remainingDistance is the distance to the last path point, so it
        //    also works when clicking somewhere onto a obstacle that isn't
        //    directly reachable.
        // -> velocity is the best way to detect WASD movement
        return agent.pathPending ||
               agent.remainingDistance > agent.stoppingDistance ||
               agent.velocity != Vector3.zero;
    }
    // health & mana ///////////////////////////////////////////////////////////
    public float HealthPercent()
    {
        return (health != 0 && healthMax != 0) ? (float)health / (float)healthMax : 0;
    }
    [Server]
    public virtual void Revive(float healthPercentage = 1)
    {
        health = Mathf.RoundToInt(healthMax * healthPercentage);
    }

    [Command]
    public void CmdInstantKill()
    {
        InstantKill();
    }
    [Server]
    public virtual void InstantKill()
    {
        health = 0;
    }
    public float ManaPercent()
    {
        return (mana != 0 && manaMax != 0) ? (float)mana / (float)manaMax : 0;
    }
    // combat //////////////////////////////////////////////////////////////////
    // deal damage at another entity
    // (can be overwritten for players etc. that need custom functionality)
    [Server]
    public virtual void DealDamageAt(Entity entity, int damage, float stunTime = 0)
    {
        // don't deal any damage if entity is invincible
        if (!entity.invincible)
        {
            // deal the damage
            if (damage > 0)
            {
                entity.health -= damage;
            }
            // stun
            if (stunTime > 0)
            {
                // stun don't stack
                entity.stunTimeEnd = Math.Max(entity.stunTimeEnd, NetworkTime.time + stunTime);
            }
        }
        // let's make sure to pull aggro in any case so that archers
        // are still attacked if they are outside of the aggro range
        entity.OnAggro(this);
        // show effects on clients
        entity.RpcOnDamageReceived(damage);
    }
    // no need to instantiate damage popups on the server
    // -> calculating the position on the client saves server computations and
    //    takes less bandwidth (4 instead of 12 byte)
    [Client]
    void ShowDamagePopup(int damage)
    {
        // spawn the damage popup (if any) and set the text
        if (Universal.EffectBloodShed != null)
        {
            // showing it above their head looks best, and we don't have to use
            Bounds bounds = collider.bounds;
            Vector3 position = new Vector3(bounds.center.x, 0.9f * (bounds.max.y - bounds.min.y) + bounds.min.y, bounds.center.z);
            GameObject popup = Instantiate(Universal.EffectBloodShed, position, Quaternion.identity);

            popup.GetComponent<BloodShed>().BloodAmount(damage, healthMax, isLocalPlayer);
        }
    }
    [ClientRpc]
    void RpcOnDamageReceived(int amount)
    {
        // show popup above receiver's head in all observers via ClientRpc
        ShowDamagePopup(amount);
    }
    // recovery ////////////////////////////////////////////////////////////////
    // recover health and mana once a second
    // note: when stopping the server with the networkmanager gui, it will
    //       generate warnings that Recover was called on client because some
    //       entites will only be disabled but not destroyed. let's not worry
    //       about that for now.
    [Server]
    public virtual void Recover()
    {
        if (enabled && health > 0)
        {
            if (healthRecovery) health += healthRecoveryRate;
            if (manaRecovery) mana += manaRecoveryRate;
        }
    }
    // aggro ///////////////////////////////////////////////////////////////////
    // this function is called by the AggroArea (if any) on clients and server
    public virtual void OnAggro(Entity entity) { }
    // spell system ////////////////////////////////////////////////////////////

    // helper function to find a buff index
    public int GetBuffIndexByName(string buffName)
    {
        return buffs.FindIndex(buff => buff.name == buffName);
    }
    // fist fights are virtually pointless because they overcomplicate the code
    // and they don't add any value to the game. so we need a check to find out
    // if the entity currently has a weapon equipped, otherwise casting a spell
    // shouldn't be possible. this may always return true for monsters, towers
    // etc.
    public abstract bool HasCastWeapon();
    // we need a function to check if an entity can attack another.
    // => overwrite to add more cases like 'monsters can only attack players'
    //    or 'player can attack pets but not own pet' etc.
    public virtual bool CanAttack(Entity entity)
    {
        return health > 0 &&
               entity.health > 0 &&
               entity != this &&
               !inSafeZone && !entity.inSafeZone;
    }
    // the first check validates the caster
    // (the spell won't be ready if we check self while casting it. so the
    //  checkSpellReady variable can be used to ignore that if needed)
    public bool CastCheckSelf(Spell spell, bool checkSpellReady = true)
    {
        // has a weapon (important for projectiles etc.), no cooldown, hp, mp?
        return spell.CheckSelf(this, checkSpellReady);
    }
    // the second check validates the target and corrects it for the spell if
    // necessary (e.g. when trying to heal an npc, it sets target to self first)
    // (spell shots that don't need a target will just return true if the user
    //  wants to cast them at a valid position)
    public bool CastCheckTarget(Spell spell)
    {
        return spell.CheckTarget(this);
    }
    // the third check validates the distance between the caster and the target
    // (target entity or target position in case of spell shots)
    // note: castchecktarget already corrected the target (if any), so we don't
    //       have to worry about that anymore here
    public bool CastCheckDistance(Spell spell, out Vector3 destination)
    {
        return spell.CheckDistance(this, out destination);
    }
    // starts casting
    public void StartCastSpell(Spell spell)
    {
        // start casting and set the casting end time
        spell.castTimeEnd = NetworkTime.time + spell.CastTime(this);
        // save modifications
        spells[currentSpell] = spell;
        // rpc for client sided effects
        RpcSpellCastStarted();
    }
    private int lastCastedSpell;
    // finishes casting. casting and waiting has to be done in the state machine
    public void FinishCastSpell(Spell spell)
    {
        // * check if we can currently cast a spell (enough mana etc.)
        // * check if we can cast THAT spell on THAT target
        // note: we don't check the distance again. the spell will be cast even
        //   if the target walked a bit while we casted it (it's simply better
        //   gameplay and less frustrating)

        if (CastCheckSelf(spell, false) && CastCheckTarget(spell))
        {
            // let the spell template handle the action
            spell.Apply(this);
            // rpc for client sided effects
            // -> pass that spell because spellIndex might be reset in the mean
            //    time, we never know
            RpcSpellCastFinished(spell);
            // decrease mana in any case
            // >>> some spells cannot be applied, maybe move it to apply
            mana -= spell.ManaCosts(this);
            // start the cooldown (and save it in the struct)
            spell.cooldownEnd = NetworkTime.time + spell.Cooldown(this);
            // save any spell modifications in any case
            spells[currentSpell] = spell;

        }
        else
        {
            // not all requirements met. no need to cast the same spell again
            currentSpell = -1;
        }
        // spells with delays
        lastCastedSpell = currentSpell;
    }
    // cast a spell waiting for a position input
    [Command]
    public void CmdExecutePositionSpell(float x, float y, float z)
    {
        if (lastCastedSpell != -1)
        {
            Spell spell = spells[lastCastedSpell];
            spell.ExecutePositionSpell(this, new Vector3(x, y, z));
        }
        lastCastedSpell = -1;
    }
    // helper function to add or refresh a buff
    public void AddOrRefreshBuff(Buff buff)
    {
        // reset if already in buffs list, otherwise add
        int index = buffs.FindIndex(b => b.name == buff.name);
        if (index != -1) buffs[index] = buff;
        else buffs.Add(buff);
    }
    // helper function to remove all buffs that ended
    void CleanupBuffs()
    {
        for (int i = 0; i < buffs.Count; ++i)
        {
            if (buffs[i].BuffTimeRemaining() == 0)
            {
                buffs.RemoveAt(i);
                --i;
            }
        }
    }
    // spell cast started rpc for client sided effects
    // note: no need to pass spellIndex, currentSpell is synced anyway
    [ClientRpc]
    public void RpcSpellCastStarted()
    {
        // validate: still alive and valid spell?
        if (health > 0 && 0 <= currentSpell && currentSpell < spells.Count)
        {
            spells[currentSpell].data.OnCastStarted(this);
        }
    }
    // spell cast finished rpc for client sided effects
    // note: no need to pass spellIndex, currentSpell is synced anyway
    [ClientRpc]
    public void RpcSpellCastFinished(Spell spell)
    {
        // validate: still alive?
        if (health > 0)
        {
            // call scriptablespell event
            spell.data.OnCastFinished(this);
            // maybe some other component needs to know about it too
            SendMessage("OnSpellCastFinished", spell, SendMessageOptions.DontRequireReceiver);
        }
    }
    // inventory ///////////////////////////////////////////////////////////////
    // helper function to count the free slots
    public int InventorySlotsFree(int containerIndex)
    {
        return containers[containerIndex].slots - inventory.Count(slot => slot.container == containerIndex);
    }
    // helper function to calculate the total amount of an item type in inventory
    // note: .Equals because name AND dynamic variables matter (petLevel etc.)
    public int InventoryCount(Item item, int containerIndex)
    {
        return (from slot in inventory
                where slot.container == containerIndex && slot.item.Equals(item)
                select slot.amount).Sum();
    }
    // helper function to remove 'n' items from the inventory
    public int InventoryRemove(Item item, int amount, int containerIndex)
    {
        foreach (ItemSlot itemSlot in inventory)
        {
            // note: .Equals because name AND dynamic variables matter (petLevel etc.)
            if (itemSlot.container == containerIndex)
            {
                if (itemSlot.item.Equals(item))
                {
                    // take as many as possible
                    amount -= inventory.DecreaseAmount(containerIndex, itemSlot.slot, amount);

                    // are we done?
                    if (amount == 0) return 0;
                }
            }
        }
        // if we got here, then we didn't remove enough items
        return amount;
    }
    // helper function to check if the inventory has space for 'n' items of type
    // -> the easiest solution would be to check for enough free item slots
    // -> it's better to try to add it onto existing stacks of the same type
    //    first though
    // -> it could easily take more than one slot too
    // note: this checks for one item type once. we can't use this function to
    //       check if we can add 10 potions and then 10 potions again (e.g. when
    //       doing player to player trading), because it will be the same result
    public bool InventoryCanAdd(Item item, int amount, int containerIndex)
    {
        // go through each slot
        for (int i = 0; i < containers[containerIndex].slots; ++i)
        {
            // empty? then subtract maxstack
            if (!inventory.GetItemSlot(containerIndex, i, out ItemSlot itemSlot))
                amount -= item.maxStack;
            // not empty. same type too? then subtract free amount (max-amount)
            // note: .Equals because name AND dynamic variables matter (petLevel etc.)
            else if (itemSlot.item.Equals(item))
                amount -= (item.maxStack - itemSlot.amount);
            // were we able to fit the whole amount already?
            if (amount <= 0) return true;
        }
        // if we got here than amount was never <= 0
        return false;
    }
    // helper function to put 'n' items of a type into the inventory, while
    // trying to put them onto existing item stacks first
    // -> this is better than always adding items to the first free slot
    // -> function will only add them if there is enough space for all of them
    public bool InventoryAdd(Item item, int amount, int containerIndex)
    //>>> die ganze Funktion ist Quatsch, anpassen wenn Quest, Loot oder Crafting gemmacht, nur bedeutsam für Player!
    {
        // we only want to add them if there is enough space for all of them, so
        // let's double check 
        if (InventoryCanAdd(item, amount, containerIndex))
        {
            // add to same item stacks first (if any)
            // (otherwise we add to first empty even if there is an existing
            //  stack afterwards)
            for (int i = 0; i < containers[containerIndex].slots; ++i)
            {
                if (inventory.GetItemSlot(containerIndex, i, out ItemSlot itemSlot))
                    // not empty and same type? then add free amount (max-amount)                                                                                                                        
                    // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                    if (itemSlot.item.Equals(item))
                    {
                        //>>> das mact keinen Sinn amount -= itemSlot.IncreaseAmount(amount);
                    }
                // were we able to fit the whole amount already? then stop loop
                if (amount <= 0) return true;
            }
            // add to empty slots (if any)
            for (int i = 0; i < containers[containerIndex].slots; ++i)
            {
                // empty? then fill slot with as many as possible
                if (!inventory.GetItemSlot(containerIndex, i, out ItemSlot itemSlot))
                {
                    int add = Mathf.Min(amount, item.maxStack);
                    inventory.AddItem(item, containerIndex, i, add);
                    amount -= add;
                }
                // were we able to fit the whole amount already? then stop loop
                if (amount <= 0) return true;
            }
            // we should have been able to add all of them
            if (amount != 0) LogFile.WriteLog(LogFile.LogLevel.Error, "Inventory add failed: " + amount + " " + item.name + " remains. " + GlobalVar.logImpossibleErrorText);
        }
        return false;
    }

    // death ///////////////////////////////////////////////////////////////////
    // universal OnDeath function that takes care of all the Entity stuff.
    // should be called by inheriting classes' finite state machine on death.
    [Server]
    protected virtual void OnDeath()
    {
        // clear movement/buffs/target/cast
        agent.ResetMovement();
        buffs.Clear();
        target = null;
        currentSpell = -1;
        workingHand = -1;
    }
    // ontrigger ///////////////////////////////////////////////////////////////
    // protected so that inheriting classes can use OnTrigger too, while also
    // calling those here via base.OnTriggerEnter/Exit
    protected virtual void OnTriggerEnter(Collider col)
    {
        // check if trigger first to avoid GetComponent tests for environment
        if (col.isTrigger && col.GetComponent<SafeZone>())
            inSafeZone = true;
    }
    protected virtual void OnTriggerExit(Collider col)
    {
        // check if trigger first to avoid GetComponent tests for environment
        if (col.isTrigger && col.GetComponent<SafeZone>())
            inSafeZone = false;
    }

}
