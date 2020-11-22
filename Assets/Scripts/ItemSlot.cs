/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Inventories need a slot type to hold Item + Amount. This is better than
// storing .amount in 'Item' because then we can use Item.Equals properly
// any workarounds to ignore the .amount.
using System;
using System.Text;
using UnityEngine;
using Mirror;
[Serializable]
public partial struct ItemSlot
{
    public Item item;
    public int amount;
    public int slot;
    public int container;
    // constructors
    public ItemSlot(Item item, int container, int slot, int amount = 1)
    {
        this.item = item;
        this.amount = amount;
        this.slot = slot;
        this.container = container;
    }

    // tooltip
    public string ToolTip()
    {
        if (amount == 0) return "";
        Player player = Player.localPlayer;
        // we use a StringBuilder so it is easy to modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(item.ToolTip());
        tip.Replace("{AMOUNT}", amount.ToString());
        tip.Replace("{TOTALPRICE}", Money.MoneyText(amount * item.price));
        tip.Replace("{TOTALWEIGHT}", GlobalFunc.WeightText((int)(amount * item.weight * player.divergenceWeight), player.handscale));

        // handling the dynamic part of items here
        if (item.data is LightItem)
        {
            LightItem lightItem = (LightItem)item.data;
            tip.Replace("{LIGHTTIME}", LightItem.RemainingLightTime(item.data1, lightItem.maxLightSeconds));
        }
        return tip.ToString();
    }
}
public class SyncListItemSlot : SyncListSTRUCT<ItemSlot>
{
    private int GetIndex(int container, int slot)
    {
        return this.FindIndex(x => x.container == container && x.slot == slot);
    }

    //for better understandability in call code
    public void AddOrReplace(ItemSlot itemSlot)
    {
        Add(itemSlot);
    }

    public new void Add(ItemSlot itemSlot)
    {
        int index = GetIndex(itemSlot.container, itemSlot.slot);
        if (index < 0)
            base.Add(itemSlot);
        else
            this[index] = itemSlot;
    }

    public void AddItem(Item item, int container, int slot, int amount = 1)
    {
        ItemSlot itemSlot = new ItemSlot(item, container, slot, amount);
        Add(itemSlot);
    }

    public void Remove(int container, int slot)
    {
        int index = GetIndex(container, slot);
        if (index >= 0)
        {
            Remove(this[index]);
        }
    }

    public bool IsSlotEmpty(int container, int slot)
    {
        return GetIndex(container, slot) < 0;
    }

    public bool GetItemSlot(int container, int slot, out ItemSlot itemSlot)
    {
        int index = GetIndex(container, slot);
        if (index < 0)
        {
            itemSlot = new ItemSlot();
            return false;
        }
        else
        {
            itemSlot = this[index];
            return true;
        }
    }

    public int DecreaseAmount(int container, int slot, int reduceBy)
    {
        int decrease = 0;
        if (reduceBy > 0)
        {
            if (GetItemSlot(container, slot, out ItemSlot itemSlot))
            {
                if (reduceBy >= itemSlot.amount)
                {
                    decrease = itemSlot.amount;
                    Remove(container, slot);
                }
                else
                {
                    itemSlot.amount -= reduceBy;
                    decrease = reduceBy;
                    AddOrReplace(itemSlot);
                }
            }
        }
        return decrease;
    }

    public int IncreaseAmount(int container, int slot, int increaseBy)
    {
        int increase = 0;
        if (GetItemSlot(container, slot, out ItemSlot itemSlot))
        {
            int maxStack = itemSlot.item.data.maxStack;
            if (itemSlot.amount < maxStack)
            {
                if (itemSlot.amount + increaseBy > maxStack)
                    increase = maxStack - itemSlot.amount;
                else
                    increase = increaseBy;
                if (increase > 0)
                {
                    itemSlot.amount += increase;
                    AddOrReplace(itemSlot);
                }
            }
        }
        return increase;
    }

    // increase always
    // necessary for special cases e.g. money when hand over different items at once
    public void IncreaseAmountLimitless(int container, int slot, int increaseBy)
    {
        if (GetItemSlot(container, slot, out ItemSlot itemSlot))
                {
                    itemSlot.amount += increaseBy;
                    AddOrReplace(itemSlot);
                }
    }

    public bool GetEquipment(int slot, out Item item, out int amount)
    {
        if (GetItemSlot(GlobalVar.containerEquipment, slot, out ItemSlot itemSlot))
        {
            item = itemSlot.item;
            amount = itemSlot.amount;
            return true;
        }
        else
        {
            item = new Item();
            amount = 0;
            return false;
        }
    }

    public bool GetEquipment(int slot, out ItemSlot itemSlot)
    {
        if (GetItemSlot(GlobalVar.containerEquipment, slot, out ItemSlot tmp))
        {
            itemSlot = tmp;
            return true;
        }
        else
        {
            itemSlot = new ItemSlot();
            return false;
        }
    }

    public bool SetEquipment(int slot, Item item, int amount)
    {
        if (IsSlotEmpty(GlobalVar.containerEquipment, slot))
        {
            AddItem(item, GlobalVar.containerEquipment, slot, amount);
            return true;
        }
        else
            return false;
    }

    public void RemoveEquipment(int slot)
    {
        Remove(GlobalVar.containerEquipment, slot);
    }

    public SyncListSTRUCT<ItemSlot> AllInContainer(int containerId)
    {
        SyncListSTRUCT<ItemSlot> tmp = new SyncListSTRUCT<ItemSlot>();
        foreach (ItemSlot itemSlot in this)
        {
            if (itemSlot.container == containerId)
                tmp.Add(itemSlot);
        }
        return tmp;
    }


    public int SpareContainerInContainer(int containerId, Player player)
    {
        int containerIndex = player.containers.IndexOfId(containerId);
        if (containerIndex == -1)
            return 0;
        int permittedContainer = player.containers[containerIndex].containers;
        if (permittedContainer == 0)
            return 0;
        foreach (ItemSlot itemSlot in AllInContainer(containerId))
        {
            if (itemSlot.item.data is ContainerItem)
                permittedContainer -= 1;
        }
        return Mathf.Max(0, permittedContainer);
    }

}
