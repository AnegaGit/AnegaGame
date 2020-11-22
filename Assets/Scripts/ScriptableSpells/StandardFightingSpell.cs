/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Base type for damage spell templates.
// => there may be target damage, targetless damage, aoe damage, etc.
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Anega/Spell/Standard Figthing", order = 902)]
public class StandardFightingSpell : ScriptableSpell
{

    [HideInInspector] public WeaponItem weaponItem;

    // values not level but equipment based
    public override int ManaCosts(Entity caster=null)
    {
        // no mana for standard attack
        return 0;
    }
    public override float CastTime(Entity caster = null)
    {
        if (weaponItem)
        {
            return weaponItem.attackTime;
        }
        else
        {
            return base.CastTime();
        }
    }
    public override float Cooldown(Entity caster = null)
    {
        // no cooldown, immediate follow up attack
        return 0;
    }
    public override float CastRange(Entity caster = null)
    {
        if (weaponItem)
        {
            return weaponItem.attackRange;
        }
        else
        {
            return base.CastRange();
        }
    }


    public override bool CheckSelf(Entity caster)
    {
        if (caster is Player)
        {
            // find weapon first
            if (((Player)caster).WeaponEquipped(out weaponItem))
            {
                return caster.health > 0;
            }
            return false;
        }
        else
        {
            return caster.health > 0;
        }
    }

    public override bool CheckTarget(Entity caster)
    {
        // target exists, alive, not self, oktype?
        return caster.target != null && caster.CanAttack(caster.target);
    }

    public override bool CheckDistance(Entity caster, out Vector3 destination)
    {
        // target still around?
        if (caster.target != null)
        {
            destination = caster.target.collider.ClosestPoint(caster.transform.position);
            if (caster is Player)
            {
                return Utils.ClosestDistance(caster.collider, caster.target.collider) <= weaponItem.attackRange;
            }
            else if (caster is Fighter)
            {
                return Utils.ClosestDistance(caster.collider, caster.target.collider) <= ((Fighter)caster).attackDistance;
            }
        }
        destination = caster.transform.position;
        return false;
    }

    public override void Apply(Entity caster)
    {
        int damage = CalculateAttackDamage(caster, out Skills.Skill attackSkill, CastTime(), out int attackLevel);
        if (damage > 0)
        {
            // apply damage now to the target
            // Can the target dodge?
            if (StandardFighting.DefenderDodge(caster.target, attackSkill, CastTime()))
            {
                Debug.Log(">>> " + caster.target.name + " dodged " + damage + "HP attack by " + caster.name);
                // return without damage
                return;
            }

            //can the target parry?
            if (StandardFighting.DefenderParry(caster.target, attackSkill, damage, CastTime(), out float stunTime))
            {
                //>>> stun the target and return without damage
                caster.DealDamageAt(caster.target, 0, stunTime);
                Debug.Log(">>> " + caster.target.name + " parried " + damage + "HP attack by " + caster.name + "  stun for :" + stunTime + "s");
                return;
            }

            // let armor adsorb some damage
            int remainingDamage = StandardFighting.RemainingDamageAfterArmory(caster.target, attackSkill, attackLevel, damage, CastTime());
            Debug.Log(">>> " + caster.target.name + " took damage of " + remainingDamage + "HP of " + damage + "HP by " + caster.name);
            if (remainingDamage > 0)
            {
                // target takes damage
                caster.DealDamageAt(caster.target, remainingDamage);
            }
        }
    }

    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{XXX}", "xxx");
        return tip.ToString();
    }

    // 1. calculate damage
    // depend on luck, attributes and mastery of weapon
    private static int CalculateAttackDamage(Entity attacker, out Skills.Skill attackSkill, float attackTime, out int attackLevel)
    {
        attackSkill = Skills.Skill.NoSkill;
        attackLevel = 0;

        if (attacker is Player)
        {
            Player player = (Player)attacker;
            // find weapon first
            // search first right hand
            ItemSlot itemSlot;
            if (player.inventory.GetEquipment(GlobalVar.equipmentRightHand, out itemSlot))
            {
                if (!(itemSlot.item.data is WeaponItem))
                {
                    player.inventory.GetEquipment(GlobalVar.equipmentLeftHand, out itemSlot);
                }
            }
            else
            {
                player.inventory.GetEquipment(GlobalVar.equipmentLeftHand, out itemSlot);
            }
            // only non weapon items or no items
            if (!(itemSlot.item.data is WeaponItem))
            {
                return 0;
            }
            WeaponItem weaponItem = (WeaponItem)itemSlot.item.data;

            // apply weapon
            float luckFactor = GlobalFunc.LuckFactor(player, weaponItem.luckPortionWeapon, weaponItem.luckMaxWeapon);
            attackSkill = weaponItem.skillWeapon;
            attackLevel = player.skills.LevelOfSkill(attackSkill);
            float weaponMastery = NonLinearCurves.GetFloat0_1(GlobalVar.fightWeaponMasteryNonlinear, attackLevel - weaponItem.levelWeapon + GlobalVar.fightWeaponMasteryFitBestAt);
            if (weaponMastery <= 0)
            {
                player.InformNoRepeat(string.Format("You are not skilled enough to damage your enemy with {0}.", weaponItem.displayName), 5f);
            }
            float attributeFactor = NonLinearCurves.GetFloat0_1(GlobalVar.fightAttributeNonlinear, player.attributes.CombinedAction(attackSkill) * 5);
            float qualityFactor = NonLinearCurves.GetFloat0_1(GlobalVar.fightItemQualityNonlinear, itemSlot.item.quality);
            int damage = (int)(luckFactor * weaponMastery * attributeFactor * weaponItem.maxDamage * qualityFactor * GlobalFunc.RandomObfuscation(GlobalVar.fightAttackObfuscation));
            LogFile.WriteDebug(string.Format("Player attacks with damage {0}HP factors: luck: {1}; mastery: {2}; attributes: {3} quaity: {5} max: {4}HP "
                , damage, luckFactor, weaponMastery, attributeFactor, weaponItem.maxDamage, qualityFactor));
            if (damage > 0)
            {
                // learn skill
                player.LearnSkill(attackSkill, attackLevel, attackTime);
                // degrade weapon
                GlobalFunc.DegradeItem(player, GlobalVar.containerEquipment, itemSlot.slot, attackTime);
            }
            return damage;
        }
        // it is a NPC with fighting capabilities
        else if (attacker is Fighter)
        {
            Fighter fighter = (Fighter)attacker;
            // NPC
            attackSkill = fighter.attackSkill;
            attackLevel = fighter.attackLevel;
            int damage = GlobalFunc.RandomInRange(fighter.attackMin, fighter.attackMax);
            LogFile.WriteDebug(string.Format("NPC attacks with damage {0}HP", damage));
            return damage;
        }
        // everybody else
        return 0;
    }

}
