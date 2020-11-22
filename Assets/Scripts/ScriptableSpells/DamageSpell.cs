/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Base type for damage spell templates.
// => there may be target damage, targetless damage, aoe damage, etc.
using System.Text;
using UnityEngine;
using Mirror;

public abstract class DamageSpell : ScriptableSpell
{
    [Header("Damage")]
    public int maxDamage = 1;
    public bool isRelativeDamage;
    [Range(0f, 1f)] public float stunChance; // range [0,1]
    public float stunTimeMax; // in seconds

    [Range(0f, 1f)] public float luckPortion;
    public float luckMax;

    public OneTimeTargetSpellEffect effect;

    // helper function to spawn the spell effect on someone
    // (used by all the buff implementations and to load them after saving)
    public void SpawnEffect(Entity caster, Entity spawnTarget)
    {
        if (effect != null)
        {
            GameObject go = Instantiate(effect.gameObject, spawnTarget.transform.position, Quaternion.identity);
            go.GetComponent<OneTimeTargetSpellEffect>().caster = caster;
            go.GetComponent<OneTimeTargetSpellEffect>().target = spawnTarget;
            NetworkServer.Spawn(go);
        }
    }
    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{DAMAGE}", GlobalFunc.ExamineLimitText(maxDamage / 2, GlobalVar.damageAndHealText));
        tip.Replace("{STUNCHANCE}", GlobalFunc.ExamineLimitText(stunChance, GlobalVar.luckPortionText));
        tip.Replace("{STUNTIME}", GlobalFunc.ExamineLimitText(stunTimeMax / 2, GlobalVar.stunTimeText));
        tip.Replace("{LUCKPORTION}", GlobalFunc.ExamineLimitText(luckPortion, GlobalVar.luckPortionText));
        return tip.ToString();
    }

    public void CalculateDamage(out int damage, out float stunTime, Entity castTarget, Entity caster = null, bool isFirst = true)
    {
        if (caster is Player)
        {
            Player player = (Player)caster;
            // apply luck
            float luckFactor = GlobalFunc.LuckFactor(player, luckPortion, luckMax);
            //attackSkill = weaponItem.skillWeapon;
            //attackLevel = player.skills.LevelOfSkill(attackSkill);
            float spellMastery = NonLinearCurves.GetFloat0_1(GlobalVar.spellMasteryNonlinear, player.skills.LevelOfSkill(skill) - skillLevel + GlobalVar.spellMasteryFitBestAt);
            if (spellMastery <= 0)
            {
                player.InformNoRepeat(string.Format("You are not skilled enough to use the spell {0}.", DisplayName), 5f);
            }
            float attributeFactor = NonLinearCurves.GetFloat0_1(GlobalVar.fightAttributeNonlinear, player.attributes.CombinedAction(skill) * 5);
            int calculatedDamage = 0;
            float damageMax = maxDamage;
            if (isRelativeDamage)
            {
                damageMax = Mathf.Clamp(maxDamage, 0, GlobalVar.spellMaxRelativeEffect) * castTarget.healthMax / 100;
            }
            calculatedDamage = (int)(luckFactor * spellMastery * attributeFactor * damageMax * GlobalFunc.RandomObfuscation(GlobalVar.spellObfuscation));
            float calculatedStuntime = luckFactor * spellMastery * attributeFactor * stunTimeMax * GlobalFunc.RandomObfuscation(GlobalVar.spellObfuscation);

            LogFile.WriteDebug(string.Format("Player damages target {0}HP factors: luck:{1}; mastery:{2}; attributes:{3} max:{4}HP stun time: {5}"
                , calculatedDamage, luckFactor, spellMastery, attributeFactor, maxDamage, calculatedStuntime));
            if (calculatedDamage > 0 || calculatedStuntime > 0)
            {
                float currentCastTime = CastTime(player);
                // learn skill
                player.LearnSkill(skill, skillLevel, currentCastTime);
                // degrade wand
                int slot = GlobalFunc.hasWandInHand(player);
                if (slot != -1)
                {
                    GlobalFunc.DegradeItem(player, GlobalVar.containerEquipment, slot, currentCastTime);
                }
            }
            damage = Mathf.Max(0, calculatedDamage);
            stunTime = Mathf.Max(0, calculatedStuntime);
            return;
        }
        else
        {
            damage = maxDamage / 2;
            stunTime = stunTimeMax / 2;
            return;
        }
    }
}
