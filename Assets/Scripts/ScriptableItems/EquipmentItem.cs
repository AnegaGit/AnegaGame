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
[CreateAssetMenu(menuName = "Anega/Item/Equipment", order = 100)]
public class EquipmentItem : UsableItem
{
    [Header("Equipment")]
    public string category;
    public int healthBonus;
    public int manaBonus;
    public int staminaBonus;

    // usage
    // -> can we equip this into any slot?
    public override bool CanUse(Player player, ItemSlot itemSlot)
    {
        return FindEquipableSlotFor(player) != -1 && itemSlot.amount == 1;
    }
    // can we equip this item into this specific equipment slot?
    public override bool CanEquip(Player player, int equipmentIndex)
    {
        if (equipmentIndex >= GlobalVar.equipmentBelt1 && equipmentIndex <= GlobalVar.equipmentBelt6)
            return true;
        else if (equipmentIndex == GlobalVar.equipmentBackpack)
            return false;
        else
        {
            string requiredCategory = player.equipmentInfo[equipmentIndex].requiredCategory;
            return requiredCategory != "" &&
                   category.StartsWith(requiredCategory);
        }
    }
    int FindEquipableSlotFor(Player player)
    {
        for (int i = 0; i < GlobalVar.equipmentSize; ++i)
            if (CanEquip(player, i))
                return i;
        return -1;
    }

    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{CATEGORY}", category);
        tip.Replace("{HEALTHBONUS}", healthBonus.ToString());
        tip.Replace("{MANABONUS}", manaBonus.ToString());
        tip.Replace("{STAMINABONUS}", staminaBonus.ToString());
        return tip.ToString();
    }
}
