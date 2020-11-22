/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Base type for heal spell templates.
// => there may be target heal, targetless heal, aoe heal, etc.
using System.Text;
using UnityEngine;
using Mirror;

public abstract class HealSpell : ScriptableSpell
{
    public int healsHealth;
    public int healsMana;
    public bool isRelativeHeals;
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
    // tooltip /////////////////////////////////////////////////////////////////
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{HEALSHEALTH}", GlobalFunc.ExamineLimitText(healsHealth / 2, GlobalVar.damageAndHealText));
        tip.Replace("{HEALSMANA}", GlobalFunc.ExamineLimitText(healsMana / 2, GlobalVar.damageAndHealText));
        return tip.ToString();
    }

    public void CalculateHeal(out int currentHealHealth, out int currentHealMana, Entity target, Entity caster = null, bool firstCall = true)
    {
        if (caster is Player)
        {
            Player player = (Player)caster;
            // apply luck
            float luckFactor = GlobalFunc.LuckFactor(player, luckPortion, luckMax);
            float spellMastery = NonLinearCurves.GetFloat0_1(GlobalVar.spellMasteryNonlinear, player.skills.LevelOfSkill(skill) - skillLevel + GlobalVar.spellMasteryFitBestAt);
            if (spellMastery <= 0 && firstCall)
            {
                player.InformNoRepeat(string.Format("You are not skilled enough to use the spell {0}.", DisplayName), 5f);
            }
            float attributeFactor = NonLinearCurves.GetFloat0_1(GlobalVar.fightAttributeNonlinear, player.attributes.CombinedAction(skill) * 5);
            int heal = 0;
            int mana = 0;
            float maxHeal = healsHealth;
            float maxMana = healsMana;
            if (isRelativeHeals)
            {
                    maxHeal = Mathf.Clamp(healsHealth, 0, GlobalVar.spellMaxRelativeEffect) * target.healthMax / 100;
                    maxMana = Mathf.Clamp(healsMana, 0, GlobalVar.spellMaxRelativeEffect) * target.manaMax / 100;
            }

            heal = (int)(luckFactor * spellMastery * attributeFactor * maxHeal * GlobalFunc.RandomObfuscation(GlobalVar.spellObfuscation));
            mana = (int)(luckFactor * spellMastery * attributeFactor * maxMana * GlobalFunc.RandomObfuscation(GlobalVar.spellObfuscation));
            if (firstCall)
            {
                LogFile.WriteDebug(string.Format("Player heals health {0}HP-{5}MP factors: luck:{1}; mastery:{2}; attributes:{3} max:{4}HP-{6}MP"
                                                  , heal, luckFactor, spellMastery, attributeFactor, maxHeal, mana, maxMana));
            }
            else
            {
                LogFile.WriteDebug(string.Format("Player heals next {0}HP-{1}MP.", heal, mana));
            }
            if ((heal > 0 || mana > 0) && firstCall)
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
            currentHealHealth = Mathf.Max(0, heal);
            currentHealMana = Mathf.Max(0, mana);
            return;
        }
        else
        {
            currentHealHealth = healsHealth / 2;
            currentHealMana = healsMana / 2;
            return;
        }
    }
}