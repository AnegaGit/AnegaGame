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
[CreateAssetMenu(menuName = "Anega/Item/Container", order = 905)]
public class ContainerItem : UsableItem
{
    [Header("Container")]
    public int minSlots;
    public int maxSlots;
    public int minContainer;
    public int maxContainer;
    //data1: containerID
    //data2:
    //data3:

    public List<ItemSlot> itemInElement = new List<ItemSlot>();
    public Container containerInElement = new Container();

    // usage
    // free rooming only if not pickable
    public override bool CanUse(Player player,ElementSlot element)
    {
        return !pickable;
    }
    // -> can we equip this into any slot?
    public override bool CanUse(Player player, ItemSlot itemSlot)
    {
        return itemSlot.amount == 1;
    }
    // can we equip this item into this specific equipment slot?
    public override bool CanEquip(Player player, int equipmentIndex)
    {
        if (player.weightMaxPlayer > weight && equipmentIndex == GlobalVar.equipmentBackpack)
            return true;
        return false;
    }

    public override void Use(Player player, int containerId, int slotIndex)
    {
        if (player.inventory.GetItemSlot(containerId, slotIndex, out ItemSlot itemSlot))
        {
            if (itemSlot.item.data is ContainerItem)
            {
                //create entry in containerlist if necessary
                if (itemSlot.item.data1 == 0)
                {
                    itemSlot.item.data1 = player.AddNewMobileContainer(minSlots, minContainer, name, "");
                    player.inventory.AddOrReplace(itemSlot);
                }
            }
        }
    }

    // client side use
    public override void OnUsed(Player player, ElementSlot elementSlot)
    {
        int depotId = elementSlot.item.data1;
        string depotName = elementSlot.displayName;
        if (player.containers.IndexOfId(depotId) < 0)
        {
            player.AddNewContainer(depotId, GlobalVar.containerTypePublic, minSlots, minContainer, depotName, "");
        }
        GameObject go = GameObject.Find("Canvas/Inventory");
        UIInventory ui = go.GetComponent<UIInventory>();
        ui.OpenContainer(depotId, elementSlot.item.data.image);
        elementSlot.isInUse = true;
    }
    public override void OnUsed(Player player, int containerId, int slotIndex)
    {
        if (player == Player.localPlayer)
        {
            if (player.inventory.GetItemSlot(containerId, slotIndex, out ItemSlot itemSlot))
            {
                if (itemSlot.item.data is ContainerItem)
                {
                    //open or wait until sync
                    if (itemSlot.item.data1 != 0)
                    {
                        GameObject go = GameObject.Find("Canvas/Inventory");
                        UIInventory ui = go.GetComponent<UIInventory>();
                        ui.OpenContainer(itemSlot.item.data1);
                    }
                    else
                        player.Inform("The lock is stuck. Try again.");
                }
            }
        }
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
                    GameObject go = GameObject.Find("Canvas/Inventory");
                    UIInventory ui = go.GetComponent<UIInventory>();
                    ui.CloseAllContainerButBackpack(); ;
                    element.isInUse = false;
                }
            }
        }
    }

    // container special for item <==> element conversion
    // move container content from inventory to item
    public void PullFromInventory(Player player, int containerId)
    {
        int containerIndex = player.containers.IndexOfId(containerId);
        if (containerIndex >= 0)
        {
            containerInElement = player.containers[containerIndex];
            itemInElement.Clear();
            for (int i = 0; i < containerInElement.slots; ++i)
            {
                // all slots, take slots with items only
                if (player.inventory.GetItemSlot(containerInElement.id, i, out ItemSlot itemSlot))
                {
                    itemInElement.Add(itemSlot);
                    player.inventory.Remove(containerInElement.id, i);
                }
            }
            player.containers.RemoveAt(containerIndex);
        }
    }

    // move container content from item to inventory
    public int PushToInventory(Player player)
    {
        // create a containerID for the content
        int containerId = player.AddNewMobileContainer(containerInElement.slots, containerInElement.containers, containerInElement.name, containerInElement.miscellaneousSync, true);

        int containerIndex = player.containers.IndexOfId(containerId);
        if (containerIndex >= 0)
        {
            foreach (ItemSlot itemSlot in itemInElement)
            {
                player.inventory.Add(new ItemSlot(itemSlot.item, containerId, itemSlot.slot, itemSlot.amount));
            }
            itemInElement.Clear();
            containerInElement = new Container();
        }
        return containerId;
    }

    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{MAXSLOTS}", maxSlots.ToString());
        tip.Replace("{MAXCONTAINER}", maxContainer.ToString());
        return tip.ToString();
    }
}
