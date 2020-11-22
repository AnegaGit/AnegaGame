/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// a wand item improves the mana usage of the wearer regarding it's magic school class
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Anega/Item/Wand", order = 110)]
public class WandItem : UsableItem
{
    [Header("Wand")]
    public Skills.Skill magicSchool;
    public int skillLevel;
    public float maxManaIncrease;
    public float maxManaRegenerationIncrease;
    public int secondsToTakeEffect;
    public int nolinearDependency = GlobalVar.wandBonusNonlinearCurve;
    public int nolinearOffset = GlobalVar.wandBonusOffset;
    //data1: time the item was equipped
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
        return false;
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
        if (GlobalFunc.IsInBelt(equipmentIndex) || GlobalFunc.IsInHand(equipmentIndex))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // tooltip /////////////////////////////////////////////////////////////////
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{MAGICSCHOOL}", Skills.info[(int)magicSchool].name);
        tip.Replace("{MAGICSCHOOLLEVEL}", GlobalFunc.ExamineLimitText(skillLevel, GlobalVar.skillLevelText));
        tip.Replace("{TIMETOACTIVE}",GlobalFunc.ExamineLimitText(secondsToTakeEffect,GlobalVar.wandActivationeTimeText));
        tip.Replace("{MAXMANAINCREASE}", GlobalFunc.ExamineLimitText(maxManaIncrease, GlobalVar.wandBonusEffectText));
        tip.Replace("{MAXMANAREGENERATIONINCREASE}", GlobalFunc.ExamineLimitText(maxManaRegenerationIncrease, GlobalVar.wandBonusEffectText));
        return tip.ToString();
    }
}
