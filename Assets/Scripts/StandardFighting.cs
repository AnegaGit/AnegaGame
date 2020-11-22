/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardFighting
{
    // All functions pure server side
    // standard fighting procedure
    // 1. calculate damage
    // depend on luck, attributes and mastery of weapon
    // in StandardFightingSpell

    // 2. Defender try to use Dodge
    // NPC use a fix value
    // player is influenced by
    // skill level, attributes, attacking weapon and carried load
    // if everything is perfect (100% skill, 3x20 attributes, slow weapon and 0 load you may even dodge every blow!
    public static bool DefenderDodge(Entity defender, Skills.Skill attackingSkill, float attackTime)
    {
        bool success = false;
        if (defender is Player)
        {
            Player player = (Player)defender;
            int defenderSkillLevel = (player).skills.LevelOfSkill(Skills.Skill.Dodge);
            float skillFactor = NonLinearCurves.GetFloat0_1(GlobalVar.fightDodgeSkillNonlinear, defenderSkillLevel);
            float attributeFactor = NonLinearCurves.GetFloat0_1(GlobalVar.fightDodgeAttributesNonlinear, (player).attributes.CombinedAction(Skills.Skill.Dodge) * 5);
            float difficultyFactor = Skills.GetRelation(attackingSkill, Skills.Skill.Dodge);
            float overloaded = NonLinearCurves.GetInterimFloat0_1(GlobalVar.fightDodgeLoadNonlinear, (player).WeightPercent());
            float baseDodge = skillFactor * attributeFactor * difficultyFactor * overloaded * GlobalVar.fightDodgeMax;
            success = GlobalFunc.RandomLowerLimit0_1(baseDodge);
            LogFile.WriteDebug(string.Format("Player dodge: {0}  limit: {1}  factors skill: {2}; attributes: {3}; difficulty: {4}; load: {5}"
                , success.ToString(), baseDodge, skillFactor, attributeFactor, difficultyFactor, overloaded));
            if (success)
            {
                // learn dodge
                player.LearnSkill(Skills.Skill.Dodge, defenderSkillLevel, attackTime);
            }
        }
        else if (defender is Fighter)
        {
            Fighter fighter = (Fighter)defender;
            success = GlobalFunc.RandomLowerLimit0_1(fighter.dodgeProbability);
            LogFile.WriteDebug(string.Format("NPC dodge: {0}  limit: {1}"
                , success.ToString(), fighter.dodgeProbability));
        }
        return success;
    }

    // 3. Defender try to parry
    // requires blocking weapon such as shield or sword
    // player
    // block probability depend on luck, attributes and mastery of shield
    // mastery of shield
    // - if player skill < shield level -20 => no block
    // - if player skill = shield level => 50% block probability
    // - if player skill >= shield level +80 => 100% probability
    //
    // NPC
    // fix proportion
    //
    // if damage>DamageIgnore (Player * luck) stun relativ to remaining damage
    public static bool DefenderParry(Entity defender, Skills.Skill attackingSkill, int damage, float attackTime, out float stun)
    {
        stun = 0f;
        if (defender is Player)
        {
            Player player = (Player)defender;
            // find parry item
            // search first left hand
            ItemSlot itemSlot;
            if (player.inventory.GetEquipment(GlobalVar.equipmentLeftHand, out itemSlot))
            {
                if (!(itemSlot.item.data is ParryItem))
                {
                    //no item at all
                    if (!player.inventory.GetEquipment(GlobalVar.equipmentRightHand, out itemSlot))
                    {
                        return false;
                    }
                }
            }
            else
            {
                //no item at all
                if (!player.inventory.GetEquipment(GlobalVar.equipmentRightHand, out itemSlot))
                {
                    return false;
                }
            }
            // only items without parry or no items
            if (!(itemSlot.item.data is ParryItem))
            {
                return false;
            }
            ParryItem parryItem = (ParryItem)itemSlot.item.data;

            // now we apply this item
            int defenderSkillLevel = player.skills.LevelOfSkill(Skills.Skill.Parry);
            float shieldMastery = NonLinearCurves.GetFloat0_1(GlobalVar.fightWeaponMasteryNonlinear, defenderSkillLevel - parryItem.levelParry + GlobalVar.fightWeaponMasteryFitBestAt);
            if (shieldMastery <= 0)
            {
                player.InformNoRepeat(string.Format("You are not skilled enough to block your enemy with {0}.", parryItem.displayName), 5f);
                return false;
            }
            float luckFactor = GlobalFunc.LuckFactor(player, parryItem.luckPortionParry, parryItem.luckMaxParry);
            float attributeFactor = NonLinearCurves.GetFloat0_1(GlobalVar.fightAttributeNonlinear, player.attributes.CombinedAction(Skills.Skill.Parry) * 5) ;
            float qualityFactor = NonLinearCurves.GetFloat0_1(GlobalVar.fightItemQualityNonlinear, itemSlot.item.quality);
            LogFile.WriteDebug(string.Format("Player try parry: factors: shieldMaster: {0}; luck: {1}; attribute: {2}; quality: {4}; item: {3}"
                , shieldMastery, luckFactor, attributeFactor, parryItem.parryBestPortion, qualityFactor));
            if (GlobalFunc.RandomLowerLimit0_1(shieldMastery * luckFactor * attributeFactor * parryItem.parryBestPortion * qualityFactor))
            {
                int damageIgnore = (int)(parryItem.damageIgnore * luckFactor / GlobalVar.fightItemQualityWorst * qualityFactor);
                stun = NonLinearCurves.GetInterimFloat0_1(GlobalVar.fightBlockStunNonlinear, (float)(damage - damageIgnore) / GlobalVar.fightBlockStunMaxDamage) * GlobalVar.fightBlockStunMaxTime;
                // learn skill
                player.LearnSkill(Skills.Skill.Parry, defenderSkillLevel, attackTime);
                // degrade shield
                GlobalFunc.DegradeItem(player, GlobalVar.containerEquipment, itemSlot.slot, attackTime);
                LogFile.WriteDebug(string.Format("Player parried with stun: {0}s", stun));
                return true;
            }
        }
        else if (defender is Fighter)
        {
            Fighter fighter = (Fighter)defender;
            if (GlobalFunc.RandomLowerLimit0_1(fighter.blockProbability))
            {
                stun = NonLinearCurves.GetInterimFloat0_1(GlobalVar.fightBlockStunNonlinear, (float)(damage - fighter.blockDamageIgnore) / GlobalVar.fightBlockStunMaxDamage) * GlobalVar.fightBlockStunMaxTime;
                LogFile.WriteDebug(string.Format("NPC parried with probalility {1} Stunned for: {0}s", stun, fighter.blockProbability));
                return true;
            }
        }
        return false;
    }

    //4. armor absorb damage
    // Player
    // find attacked position and related armor
    // consumed damage depends on depend on luck, attributes, skill relation of attack and defense, type of attack and defense and mastery of armor
    // mastery of armor
    // - if player skill < armor level -20 => no block
    // - if player skill = armor level => 50% block probability
    // - if player skill >= armor level +80 => 100% probability 
    // depending on the armor there might remain a min damage
    //
    // NPC
    // skill relation of attack and defense, type of attack and defense and random in range
    public static int RemainingDamageAfterArmory(Entity defender, Skills.Skill attackingSkill, int attackingLevel, int damage, float attackTime)
    {
        if (defender is Player)
        {
            Player player = (Player)defender;
            //find attacked position
            int i;
            int pos = GlobalFunc.RandomInRange(0, 99);
            for (i = 0; i < GlobalVar.fightArmorPortion.GetLength(0); i++)
            {
                if (pos < GlobalVar.fightArmorPortion[i, 1])
                {
                    break;
                }
            }
            if (player.inventory.GetItemSlot(GlobalVar.containerEquipment, GlobalVar.fightArmorPortion[i, 0], out ItemSlot itemSlot))
            {
                if (itemSlot.item.data is ClothingItem)
                {
                    ClothingItem armorItem = (ClothingItem)itemSlot.item.data;
                    int defenderSkillLevel = player.skills.LevelOfSkill(armorItem.skillArmor);
                    float armorMastery = NonLinearCurves.GetFloat0_1(GlobalVar.fightWeaponMasteryNonlinear, defenderSkillLevel - armorItem.levelArmor + GlobalVar.fightWeaponMasteryFitBestAt) ;
                    if (armorMastery <= 0)
                    {
                        player.InformNoRepeat(string.Format("You are not skilled enough to get any protection from {0}.", armorItem.displayName), 5f);
                        return damage;
                    }
                    float luckFactor = GlobalFunc.LuckFactor(player, armorItem.luckPortionDefense, armorItem.luckMaxDefense);
                    float attributeFactor = NonLinearCurves.GetFloat0_1(GlobalVar.fightAttributeNonlinear, player.attributes.CombinedAction(armorItem.skillArmor) * 5) ;
                    float skillRelationFactor = NonLinearCurves.GetInterimFloat0_1(GlobalVar.fightArmorSkillRelationNonlinear, (armorItem.levelArmor - attackingLevel + 100) / 200f);
                    float difficultyFactor = Skills.GetRelation(attackingSkill, armorItem.skillArmor);
                    float qualityFactor = NonLinearCurves.GetFloat0_1(GlobalVar.fightItemQualityNonlinear, itemSlot.item.quality);
                    int protection = (int)(luckFactor * attributeFactor * skillRelationFactor * difficultyFactor * armorMastery * qualityFactor * armorItem.maxProtection);
                    LogFile.WriteDebug(string.Format("Player protected by armor at {7} with {0} HP Factors: luck: {1}; attributes: {2}; skillRelation: {3}; difficulty: {4}; mastery: {5}; quality: {8}; Best value: {6}HP"
                        , protection, luckFactor, attributeFactor, skillRelationFactor, difficultyFactor, armorMastery, armorItem.maxProtection, GlobalVar.fightArmorPortion[i, 0], qualityFactor));
                    // learn skill
                    player.LearnSkill(armorItem.skillArmor, defenderSkillLevel, attackTime);
                    // degrade shield
                    GlobalFunc.DegradeItem(player, GlobalVar.containerEquipment, itemSlot.slot, attackTime);
                    return Mathf.Max(Mathf.Min(damage, armorItem.minPassingDamage), damage - protection);
                }
                else
                {
                    LogFile.WriteDebug(string.Format("Player has item without protection at position {0}", GlobalVar.fightArmorPortion[i, 0]));
                    // item has no protection
                    return damage;
                }
            }
            else
            {
                LogFile.WriteDebug(string.Format("Player not protected at position {0}", GlobalVar.fightArmorPortion[i, 0]));
                // no protection
                return damage;
            }
        }
        else if (defender is Fighter)
        {
            Fighter fighter = (Fighter)defender;
            float skillRelationFactor = NonLinearCurves.GetInterimFloat0_1(GlobalVar.fightArmorSkillRelationNonlinear, (fighter.defenseLevel - attackingLevel + 100) / 200f);
            float difficultyFactor = Skills.GetRelation(attackingSkill, fighter.armorSkill);
            int defense = (int)(GlobalFunc.RandomInRange(fighter.armorMin, fighter.armorMax) * skillRelationFactor * difficultyFactor);
            LogFile.WriteDebug(string.Format("NPC protected by armor with {0}HP Factors: skillRelation: {1}; difficulty: {2}"
                , defense, skillRelationFactor, difficultyFactor));
            return Mathf.Max(Mathf.Min(0, fighter.armorNotConsumed), damage - defense);
        }
        else
        {
            // non fighting entity
            return damage;
        }
    }
}
