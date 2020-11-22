/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// only usable items need minLevel and usage functions
using System;
using System.Text;
using UnityEngine;
[CreateAssetMenu(menuName = "Anega/Item/General", order = 1)]
public class UsableItem : ScriptableItem
{
    [Header("Usage")]
    public float interactionRange; // distance for interaction if ElementSlot (requires prefab)
    public bool pickable; // can be picked from ElementSlot into ItemSlot (requires prefab)
    public bool usableAsElement; // can be used free rooming
    public bool usableFromInventory; // can be used in inventory
    public int decayTime; // how long does it take before the element rots. Items in bag don't rot
    // item cooldowns need to be global so we can't use the potion in slot 0 and
    // then the one in slot 1 immediately after.
    // -> cooldown buffs are the common solution in MMOs and they allow for
    //    heal-over-time if needed too
    // -> should use 'Health Potion Cooldown' buff for all health potions, etc.
    [Header("Animation")]
    public GameObject modelPrefab;
    public string animation;
    [Header("Cooldown Buff")]
    public TargetBuffSpell cooldownBuff;
    // usage ///////////////////////////////////////////////////////////////////
    // [Server] and [Client] CanUse check for UI, Commands, etc.
    // can it be used as element
    public virtual bool CanUse(Player player, ElementSlot element)
    {
        return usableAsElement;
    }
    // can it be picked into inventory
    public virtual bool CanPicked(ElementSlot element)
    {
        return pickable;
    }
    // can it be used as inventory item
    public virtual bool CanUse(Player player, ItemSlot itemSlot)
    {
        return usableFromInventory;
    }

    // [Server] Use logic: make sure to call base.Use() in overrides too.
    public virtual void Use(Player player, ElementSlot element)
    {
        ApplyCooldown(player);
    }
    public virtual void Use(Player player, int container, int slot)
    {
        ApplyCooldown(player);
    }
    private void ApplyCooldown(Player player)
    {
        // start cooldown buff (if any)
        if (cooldownBuff != null)
        {
            // set target to player before applying buff
            Entity oldTarget = player.target;
            player.target = player;
            // apply the buff with spell level 1
            cooldownBuff.Apply(player);
            // restore target
            player.target = oldTarget;
        }
    }

    // initialize
    public virtual void Initialize(ElementSlot element) { }
    // using over a long term
    public virtual void InUse(ElementSlot element) { }
    public virtual void InUse(Player player, int container, int slot) { }
    // [Client] OnUse Rpc callback for effects, sounds, etc.
    //dummy will be used in higher tiers
    public virtual void OnUsed(Player player, ElementSlot element) { }
    public virtual void OnUsed(Player player, int container, int slot) { }
    public virtual void OnUseAction(Player player, int container, int slot, int action) { }
    public virtual void OnUseAction(ElementSlot element, int data1, int data2, int data3) { }
    // update for elements
    public virtual void UpdateServer(ElementSlot element) { }
    public virtual void UpdateClient(ElementSlot element) { }

    // tooltip /////////////////////////////////////////////////////////////////
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{DECAY}", GlobalFunc.ExamineLimitText(decayTime, GlobalVar.spellCooldownTimeText));
        return tip.ToString();
    }
}
