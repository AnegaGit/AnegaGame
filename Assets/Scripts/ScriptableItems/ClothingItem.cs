/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Text;
using UnityEngine;
[CreateAssetMenu(menuName = "Anega/Item/Clothing", order = 101)]
public class ClothingItem : EquipmentItem
{
    [Header("Damage Protection")]
    public Skills.Skill skillArmor;
    [Range(0, 100)] public int levelArmor;
    [Range(0, 1)] public float luckPortionDefense;
    public float luckMaxDefense;
    public int maxProtection;
    public int minPassingDamage;
    [Header("UMA Reference")]
    public UMA.UMATextRecipe UMAClothingRecipeMale;
    public UMA.UMATextRecipe UMAClothingRecipeFemale;



    // use it in inventory shall equip to spare position
    public override void Use(Player player, int containerId, int slotIndex)
    {
        // always call base function too
        base.Use(player, containerId, slotIndex);
        // find a slot that accepts this category, then equip it
        int targetSlot = FindClothingSlotFor(player);
        if (targetSlot != -1)
        {
            player.SwapInventoryEquip(containerId, slotIndex, targetSlot);
        }
    }
    int FindClothingSlotFor(Player player)
    {
        for (int i = GlobalVar.equipmentHead; i <= GlobalVar.equipmentFoot; ++i)
            if (CanEquip(player, i))
                return i;
        return -1;
    } 
    
    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{ARMORSKILL}", Skills.Name(skillArmor));
        tip.Replace("{ARMORLEVEL}", GlobalFunc.ExamineLimitText(levelArmor, GlobalVar.skillLevelText));
        tip.Replace("{ARMORLUCKPORTION}", GlobalFunc.ExamineLimitText(luckPortionDefense, GlobalVar.luckPortionText));
        tip.Replace("{ARMORLUCKMAX}", luckMaxDefense.ToString("F1"));
        tip.Replace("{ARMORMAXPROTECTION}", GlobalFunc.ExamineLimitText(maxProtection, GlobalVar.damageAndHealText));
        tip.Replace("{ARMORDAMAGEGAP}", GlobalFunc.ExamineLimitText(minPassingDamage, GlobalVar.damageAndHealText));
        return tip.ToString();
    }
}
