/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Text;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Anega/Item/GM", order = 904)]
public class GMItem : UsableItem
{
    //[Header("GM")]
    //data1:
    //data2:
    //data3:

    // usage
    // can it be used as element
    public override bool CanUse(Player player, ElementSlot element)
    {
        if (player.isGM)
            return true;
        else
            return false;
    }
    // can it be used from inventory
    public override bool CanUse(Player player, ItemSlot itemSlot)
    {
        return false;
    }
    // can we equip this item into this specific equipment slot?
    public override bool CanEquip(Player player, int equipmentIndex)
    {
        return false;
    }

    // client side use
    public override void OnUsed(Player player, ElementSlot elementSlot)
    {
        // fist application: target yourself
        player.CmdSetTarget(player.netIdentity);
    }
}

