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

[CreateAssetMenu(menuName = "Anega/Item/Portal", order = 901)]
public class PortalItem : UsableItem
{
   //[Header("Portal")]
    //data1:
    //data2:
    //data3:

    // usage
    // can it be used as element
    public override bool CanUse(Player player, ElementSlot element)
    {
        return true;
    }
    // can it be used from inventory
    public override bool CanUse(Player player, ItemSlot itemSlot)
    {
        return false;
    }
    // can it be picked into inventory
    public override bool CanPicked(ElementSlot element)
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
        GameObject go = elementSlot.transform.Find("Model").gameObject;
        PortalElement pe = go.GetComponent<PortalElement>();
        pe.UseBy(player);
        elementSlot.isInUse = true;
    }

    public override void UpdateClient(ElementSlot element)
    {
        if (element.isInUse)
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                float distance = Vector3.Distance(player.transform.position, element.transform.position);
                if (distance > interactionRange)
                {
                    GameObject go = GameObject.Find("Canvas/Teleport");
                    UIPortal ui = go.GetComponent<UIPortal>();
                    ui.panel.SetActive(false);
                    element.isInUse = false;
                }
            }
        }
    }
}

