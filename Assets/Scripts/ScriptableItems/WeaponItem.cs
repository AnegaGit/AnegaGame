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
[CreateAssetMenu(menuName = "Anega/Item/Weapon", order = 102)]
public class WeaponItem : ParryItem
{
    [Header("Attack")]
    public AmmoItem requiredAmmo; // null if no ammo is required
    public StandardFightingSpell defaultSpell;
    public Skills.Skill skillWeapon;
    [Range(0, 100)] public int levelWeapon;
    [Range(0, 1)] public float luckPortionWeapon;
    public float luckMaxWeapon;
    public int maxDamage;
    public float attackTime;
    public float attackRange;

    // use it in inventory shall equip to spare position
    public override void Use(Player player, int containerId, int slotIndex)
    {
        // always call base function too
        base.Use(player, containerId, slotIndex);
        // Don't change with itself
        if (!(containerId==GlobalVar.containerEquipment&&slotIndex==GlobalVar.equipmentRightHand))
        {
            player.SwapInventoryEquip(containerId, slotIndex, GlobalVar.equipmentRightHand);
        }
    }   
    
    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        if (requiredAmmo != null)
            tip.Replace("{REQUIREDAMMO}", requiredAmmo.name);
        tip.Replace("{WEAPONSKILL}", Skills.Name(skillWeapon));
        tip.Replace("{WEAPONLEVEL}", GlobalFunc.ExamineLimitText(levelWeapon, GlobalVar.skillLevelText));
        tip.Replace("{WEAPONLUCKPORTION}", GlobalFunc.ExamineLimitText(luckPortionWeapon, GlobalVar.luckPortionText));
        tip.Replace("{WEAPONLUCKMAX}", luckMaxWeapon.ToString());
        tip.Replace("{WEAPONDAMAGE}", GlobalFunc.ExamineLimitText(maxDamage,GlobalVar.damageAndHealText));
        tip.Replace("{WEAPONFREQUENCY}", GlobalFunc.ExamineLimitText(60f/attackTime,GlobalVar.hitsPerMinuteText));
        return tip.ToString();
    }
}
