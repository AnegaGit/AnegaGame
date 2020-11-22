/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Text;
using UnityEngine;
using Mirror;
[CreateAssetMenu(menuName = "uMMORPG/Item/Mount", order = 999)]
public class MountItem : SummonableItem
{
    // usage
    public override bool CanUse(Player player, ItemSlot itemSlot)
    {
        // summonable checks if we can summon it already,
        // we just need to check if we have no active mount summoned yet
        // OR if this is the active mount, so we unsummon it
        // >>>Inventory action macht keinen Sinn
        return base.CanUse(player, itemSlot) &&
               (player.activeMount == null || player.activeMount.gameObject == player.inventory[1].item.objectInGame);
    }
    public override void Use(Player player, int containerId, int slotIndex)
    {
        // always call base function too
        base.Use(player, containerId, slotIndex);
        // summon
        if (player.activeMount == null)
        {
            // summon at player position
            if (player.inventory.GetItemSlot(containerId, slotIndex, out ItemSlot slot))
            {
                GameObject go = Instantiate(summonPrefab.gameObject, player.transform.position, player.transform.rotation);
                Mount mount = go.GetComponent<Mount>();
                mount.name = summonPrefab.name; // avoid "(Clone)"
                mount.owner = player;
                mount.health = slot.item.data1;

                NetworkServer.Spawn(go);
                player.activeMount = go.GetComponent<Mount>(); // set syncvar to go after spawning

                // set item summoned pet reference so we know it can't be sold etc.
                slot.item.objectInGame = go;
                player.inventory.AddOrReplace(slot);
            }
        }
        // unsummon
        else
        {
            // destroy from world. item.summoned and activePet will be null.
            NetworkServer.Destroy(player.activeMount.gameObject);
        }
    }
}
