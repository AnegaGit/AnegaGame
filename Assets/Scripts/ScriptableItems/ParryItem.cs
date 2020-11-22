/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Text;
using UnityEngine;
[CreateAssetMenu(menuName = "Anega/Item/Shield", order = 104)]
public class ParryItem : EquipmentItem
{
    [Header("Parry")]
    [Range(0, 100)] public int levelParry;
    [Range(0, 1)] public float luckPortionParry;
    public float luckMaxParry;
    public int damageIgnore;
    [Range(0, 1)] public float parryBestPortion;

    // use it in inventory shall equip to spare position
    public override void Use(Player player, int containerId, int slotIndex)
    {
        // always call base function too
        base.Use(player, containerId, slotIndex);
        // Don't change with itself
        // weapon items have parry too so don't use them
        if (!(containerId == GlobalVar.containerEquipment && slotIndex == GlobalVar.equipmentLeftHand) && !(this is WeaponItem) )
        {
            player.SwapInventoryEquip(containerId, slotIndex, GlobalVar.equipmentLeftHand);
        }
    }

    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{PARRYLEVEL}", GlobalFunc.ExamineLimitText(levelParry, GlobalVar.skillLevelText));
        tip.Replace("{PARRYLUCK}",  GlobalFunc.ExamineLimitText(luckPortionParry, GlobalVar.luckPortionText));
        tip.Replace("{PARRYPROBABILITY}", GlobalFunc.ExamineLimitText(parryBestPortion,GlobalVar.luckPortionText));
        tip.Replace("{PARRYDAMAGE}", GlobalFunc.ExamineLimitText(damageIgnore, GlobalVar.damageAndHealText));
        return tip.ToString();
    }
}