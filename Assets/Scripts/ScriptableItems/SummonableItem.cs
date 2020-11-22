/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// summons a 'Summonable' entity type.
// not to be confused with Monster Scrolls, that simply spawn monsters.
// (summonables are entities that belong to the player, like pets and mounts)
using UnityEngine;
using Mirror;
public abstract class SummonableItem : UsableItem
{
    [Header("Summonable")]
    public Summonable summonPrefab;
    public long revivePrice = 10;
    public bool removeItemIfDied;
    // usage
    public override bool CanUse(Player player, ItemSlot itemSlot)
    {
        // summon only if:
        //  summonable not dead (dead summonable item has to be revived first)
        //  not while fighting, trading, stunned, dead, etc
        //  player level at least summonable level to avoid power leveling
        //    with someone else's high level summonable
        //  -> also use riskyActionTime to avoid spamming. we don't want someone
        //     to spawn and destroy a pet 1000x/second
        //>>> Inventory action macht keinen Sinn, �berarbeiten
        return base.CanUse(player, itemSlot) &&
               (player.state == GlobalVar.stateIdle || player.state == GlobalVar.stateMoving) &&
               NetworkTime.time >= player.nextRiskyActionTime &&
               summonPrefab != null &&
               player.inventory[1].item.data1 > 0;
    }
    public override void Use(Player player, int containerId, int slotIndex)
    {
        // always call base function too
        base.Use(player, containerId, slotIndex);
        // set risky action time (1s should be okay)
        player.nextRiskyActionTime = NetworkTime.time + 1;
    }
}
