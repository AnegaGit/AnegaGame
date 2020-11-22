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

[CreateAssetMenu(menuName = "Anega/Item/Learn Spell", order = 402)]
public class LearnSpellItem : BookItem
{
    [Header("Spell")]
    public ScriptableSpell spell;
    public int minSkillLevel;
    //data1:
    //data2:
    //data3:

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
                uIBook.Initialize((BookItem)this, container, slot);
                uIBook.InitializeExecute(true, "Learn Spell");
            }
        }
    }

    public override void ExecuteBook(int containerId, int slotId)
    {
        Player player = Player.localPlayer;
        if (player)
        {
            if (player.skills.LevelOfId((int)spell.skill) >= minSkillLevel || minSkillLevel == 0)
            {
                //verify whether the book is still in hand
                if (player.inventory.GetItemSlot(containerId, slotId, out ItemSlot itemSlot))
                {
                    if (itemSlot.item.data == this)
                    {
                        if (player.CanLearnSpell(spell.name))
                        {
                            player.CmdLearnSpell(spell.name);
                            // remove item!
                            player.CmdRemoveItem(containerId, slotId, 1);
                        }
                    }
                    else
                    {
                        player.Inform("Nice try, but that's not the same book you opened!");
                    }
                }
                else
                {
                    player.Inform("You cannot use a book after it has been moved!");
                }
            }
            else
            {
                player.Inform(string.Format("You cannot learn that spell yet. Your skill {0} has to be {1} at least.",
                    Skills.info[(int)spell.skill].name
                    , GlobalFunc.ExamineLimitText(minSkillLevel, GlobalVar.skillLevelText)));
            }
        }
    }


}

