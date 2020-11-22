/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Base type for buff spell templates.
// => there may be target buffs, targetless buffs, aoe buffs, etc.
//    but they all have to fit into the buffs list
using System.Text;
using UnityEngine;
using Mirror;
public abstract class BuffSpell : BonusSpell
{
    public float buffTime = 60f;
    public float luckTimePortion = 0;
    public float luckTimeMax = 1;
    public BuffSpellEffect effect;
    // helper function to spawn the spell effect on someone
    // (used by all the buff implementations and to load them after saving)
    public void SpawnEffect(Entity caster, Entity spawnTarget)
    {
        if (effect != null)
        {
            GameObject go = Instantiate(effect.gameObject, spawnTarget.transform.position, Quaternion.identity);
            go.GetComponent<BuffSpellEffect>().caster = caster;
            go.GetComponent<BuffSpellEffect>().target = spawnTarget;
            go.GetComponent<BuffSpellEffect>().buffName = name;
            NetworkServer.Spawn(go);
        }
    }
    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{BUFFTIME}", Utils.PrettySeconds(buffTime));
        return tip.ToString();
    }
    //effects
    public void CalculateBuffEffects(out float buffLevel, out float buffLuckTime, Entity castTarget, Entity caster = null, bool isFirst = true)
    {
        if (caster is Player)
        {
            Player player = (Player)caster;
            // apply luck
            float luckFactor = GlobalFunc.LuckFactor(player, luckTimePortion, luckTimeMax);
            buffLuckTime = buffTime * luckFactor;

            float spellMastery = NonLinearCurves.GetFloat0_1(GlobalVar.spellMasteryNonlinear, player.skills.LevelOfSkill(skill) - skillLevel + GlobalVar.spellMasteryFitBestAt);
            if (spellMastery <= 0)
            {
                player.InformNoRepeat(string.Format("You are not skilled enough to use the spell {0}.", DisplayName), 5f);
            }
            float attributeFactor = NonLinearCurves.GetFloat0_1(GlobalVar.fightAttributeNonlinear, player.attributes.CombinedAction(skill) * 5);

            buffLevel = 0;
            if (bonusHealthMax != 0)
            {
                float bonusHealth = GlobalFunc.ValueFromProportion(spellMastery * attributeFactor, bonusHealthMin, bonusHealthMax);
                if (bonusRelative)
                {
                    bonusHealth = bonusHealth / 100 * castTarget.healthMax;
                }
                buffLevel = bonusHealth / bonusHealthMax;
            }
            else if (bonusManaMax != 0)
            {
                float bonusMana = GlobalFunc.ValueFromProportion(spellMastery * attributeFactor, bonusManaMin, bonusManaMax);
                if (bonusRelative)
                {
                    bonusMana = bonusMana / 100 * castTarget.manaMax;
                }
                buffLevel = bonusMana / bonusManaMax;
            }
            else if (bonusHealthPerSecondMax != 0)
            {
                float bonusHealthPerSecond = GlobalFunc.ValueFromProportion(spellMastery * attributeFactor, bonusHealthPerSecondMin, bonusHealthPerSecondMax);
                if (bonusRelative)
                {
                    bonusHealthPerSecond = bonusHealthPerSecond / 100 * castTarget.healthRecoveryRateBase;
                }
                buffLevel = bonusHealthPerSecond / bonusHealthPerSecondMax;
            }
            else if (bonusManaPerSecondMax != 0)
            {
                float bonusManaPerSecond = GlobalFunc.ValueFromProportion(spellMastery * attributeFactor, bonusManaPerSecondMin, bonusManaPerSecondMax);
                if (bonusRelative)
                {
                    bonusManaPerSecond = bonusManaPerSecond / 100 * castTarget.manaRecoveryRateBase;
                }
                buffLevel = bonusManaPerSecond / bonusManaPerSecondMax;
            }
            else if (bonusSpeedMax != 0)
            {
                float bonusSpeed = GlobalFunc.ValueFromProportion(spellMastery * attributeFactor, bonusSpeedMin, bonusSpeedMax);
                if (bonusRelative)
                {
                    bonusSpeed = bonusSpeed / 100 * castTarget.speedBase;
                }
                buffLevel = bonusSpeed / bonusSpeedMax;
            }

            LogFile.WriteDebug(string.Format("Player applies buff with spell {5} level {0} for time: {1}::: factors: luck:{2}; mastery:{3}; attributes:{4}"
                , buffLevel, buffLuckTime, luckFactor, spellMastery, attributeFactor, name));
            if (buffLevel != 0 && buffLuckTime > 0)
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
            return;
        }
        else
        {
            buffLevel = 1;
            buffLuckTime = buffTime;
            return;
        }
    }
}
