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
[CreateAssetMenu(menuName = "uMMORPG/Item/Pet", order = 999)]
public class PetItem : SummonableItem
{
    // usage
    public override bool CanUse(Player player, ItemSlot itemSlot)
    {
        // summonable checks if we can summon it already,
        // we just need to check if we have no active pet summoned yet
        return base.CanUse(player, itemSlot) && player.activePet == null;
    }
    public override void Use(Player player, int containerId, int slotIndex)
    {
        // always call base function too
        base.Use(player, containerId, slotIndex);
        // summon right next to the player
        if (player.inventory.GetItemSlot (containerId, slotIndex, out ItemSlot slot))
        {
            GameObject go = Instantiate(summonPrefab.gameObject, player.petDestination, Quaternion.identity);
            Pet pet = go.GetComponent<Pet>();
            pet.name = summonPrefab.name; // avoid "(Clone)"
            pet.owner = player;
            pet.health = slot.item.data1;

            NetworkServer.Spawn(go);
            player.activePet = go.GetComponent<Pet>(); // set syncvar to go after spawning

            // set item summoned pet reference so we know it can't be sold etc.
            slot.item.objectInGame = go;
            player.inventory.AddOrReplace(slot);
        }
    }
}