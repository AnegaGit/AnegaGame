/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Anega/Item/Book", order = 401)]
public class BookItem : UsableItem
{
    [Header("Book")]
    public string author;
    public string title;
    [TextArea(1, 30)] public string bookText;
    public UIBook.BookType bookType;
    //data1:
    //data2:
    //data3:

    // usage
    // can it be used as element
    public override bool CanUse(Player player, ElementSlot element)
    {
        return false;
    }
    // can it be used from inventory
    // must hold in hand or belt
    public override bool CanUse(Player player, ItemSlot itemSlot)
    {
        return GlobalFunc.IsInHand(itemSlot.container, itemSlot.slot) || GlobalFunc.IsInBelt(itemSlot.container, itemSlot.slot);
    }
    // can it be picked into inventory
    public override bool CanPicked(ElementSlot element)
    {
        return true;
    }
    // can we equip this item into this specific equipment slot?
    // can be hold in hand
    public override bool CanEquip(Player player, int equipmentIndex)
    {
        return GlobalFunc.IsInHand(equipmentIndex) || GlobalFunc.IsInBelt(equipmentIndex);
    }

    // use it in inventory start usage
    // client side use only
    public override void OnUsed(Player player, int container, int slot)
    {
        if (player == Player.localPlayer)
        {
            GameObject go = GameObject.Find("Canvas/Book");
            UIBook uIBook = go.GetComponent<UIBook>();
            if (uIBook.isShown)
            {
                uIBook.isShown = false;
            }
            else
            {
                uIBook.Initialize(this, container, slot);
            }
        }
    }

    // tooltip
    public override string ToolTip()
    {
        Player player = Player.localPlayer;
        StringBuilder tip = new StringBuilder(base.ToolTip());
        if (player.abilities.readAndWrite == Abilities.Nav)
        {
            tip.Replace("{BOOKTITLE}", GlobalVar.illiterateBookName);
        }
        else
        {
            tip.Replace("{BOOKTITLE}", string.Format("{0}" + Environment.NewLine + "by {1}", title, author));
        }


        return tip.ToString();
    }

    public virtual void ExecuteBook(int container, int slot)
    {
        return;
    }
}

