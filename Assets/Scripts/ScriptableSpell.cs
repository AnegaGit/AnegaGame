/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Saves the spell info in a ScriptableObject that can be used ingame by
// referencing it from a MonoBehaviour. It only stores an spell's static data.
//
// We also add each one to a dictionary automatically, so that all of them can
// be found by name without having to put them all in a database. Note that we
// have to put them all into the Resources folder and use Resources.LoadAll to
// load them. This is important because some spells may not be referenced by any
// entity ingame (e.g. after a special event). But all spells should still be
// loadable from the database, even if they are not referenced by anyone
// anymore. So we have to use Resources.Load. (before we added them to the dict
// in OnEnable, but that's only called for those that are referenced in the
// game. All others will be ignored by Unity.)
//
// Entity animation controllers will need one bool parameter for each spell name
// and they can use the same animation for different spell templates by using
// multiple transitions. (this is way easier than keeping track of a spellindex)
//
// A Spell can be created by right clicking the Resources folder and selecting
// Create -> Anega Spell. Existing spells can be found in the Resources folder
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
public abstract partial class ScriptableSpell : ScriptableObject
{
    [Header("Info")]
    public bool followupDefaultAttack;
    public string displayName;
    [SerializeField, TextArea(1, 30)] protected string toolTip; // not public, use ToolTip()
    public Sprite image;
    public bool showInSpellList = true;
    public bool showCastBar;
    public bool cancelCastIfTargetDied; // direct hit may want to cancel if target died. buffs doesn't care. etc.
    [Header("Skill")]
    public Skills.Skill skill;
    public int skillLevel;
    [Header("Requirements")]
    public ScriptableSpell predecessor; // this spell has to be learned first
    public bool requiresWeapon; // some might need empty-handed casting
    [Header("Properties")]
    public int manaCostsNewbe;
    public int manaCostsMaster;
    public float castTimeNewbe;
    public float castTimeMaster;
    public float cooldownNewbe;
    public float cooldownMaster;
    public float castRangeNewbe;
    public float castRangeMaster;
    [Header("Animation")]
    public string castAnimation;
    [Header("Sound")]
    public AudioClip castSound;

// standard values
public string DisplayName
    {
        get
        {
            if (displayName.Length > 0)
            {
                return displayName;
            }
            else
            {
                return name;
            }
        }
    }
    // standard values, may be overwritten in higher instances
    public virtual int ManaCosts(Entity caster = null)
    {
        if (caster is Player)
        {
            Player player = (Player)caster;
            return (int)GlobalFunc.ValueFromProportion(NonLinearCurves.GetFloat0_1(GlobalVar.manaMasteryNonlinear, player.skills.LevelOfSkill(skill)), manaCostsNewbe, manaCostsMaster);
        }
        else
        {
            return manaCostsNewbe;
        }
    }
    public virtual float CastTime(Entity caster = null)
    {
        if (caster is Player)
        {
            Player player = (Player)caster;
            return GlobalFunc.ValueFromProportion(NonLinearCurves.GetFloat0_1(GlobalVar.castTimeMasteryNonlinear, player.skills.LevelOfSkill(skill)), castTimeNewbe, castTimeMaster);
        }
        else
        {
            return castTimeNewbe;
        }
    }
    public virtual float Cooldown(Entity caster = null)
    {
        if (caster is Player)
        {
            Player player = (Player)caster;
            return GlobalFunc.ValueFromProportion(NonLinearCurves.GetFloat0_1(GlobalVar.cooldownMasteryNonlinear, player.skills.LevelOfSkill(skill)), cooldownNewbe, cooldownMaster);
        }
        else
        {
            return cooldownNewbe;
        }
    }
    public virtual float CastRange(Entity caster = null)
    {
        if (caster is Player)
        {
            Player player = (Player)caster;
            return GlobalFunc.ValueFromProportion(NonLinearCurves.GetFloat0_1(GlobalVar.castRangeMasteryNonlinear, player.skills.LevelOfSkill(skill)), castRangeNewbe, castRangeMaster);
        }
        else
        {
            return castRangeNewbe;
        }
    }

    // the spell casting process ///////////////////////////////////////////////
    // 1. self check: alive, enough mana, cooldown ready etc.?
    // (most spells can only be cast while alive. some maybe while dead or only
    //  if we have ammo, etc.)
    public virtual bool CheckSelf(Entity caster)
    {
        // has a weapon (important for projectiles etc.), no cooldown, hp, mp?
        return caster.health > 0 &&
               caster.mana >= ManaCosts(caster) &&
               (!requiresWeapon || caster.HasCastWeapon());
    }
    // 2. target check: can we cast this spell 'here' or on this 'target'?
    // => e.g. sword hit checks if target can be attacked
    //         spell shot checks if the position under the mouse is valid etc.
    //         buff checks if it's a friendly player, etc.
    // ===> IMPORTANT: this function HAS TO correct the target if necessary,
    //      e.g. for a buff that is cast on 'self' even though we target a NPC
    //      while casting it
    public abstract bool CheckTarget(Entity caster);
    // 3. distance check: do we need to walk somewhere to cast it?
    //    e.g. on a monster that's far away
    //    => returns 'true' if distance is fine, 'false' if we need to move
    // (has corrected target already)
    public abstract bool CheckDistance(Entity caster, out Vector3 destination);
    // 4. apply spell: deal damage, heal, launch projectiles, etc.
    // (has corrected target already)
    //[Server]
    public abstract void Apply(Entity caster);
    // 5. execute in some cases where we need additional UI activities like teleport
    //[Server]
    public virtual void ExecutePositionSpell(Entity caster, Vector3 targetPosition)
    {
        return;
    }
    // events for client sided effects /////////////////////////////////////////
    // [Client]
    public virtual void OnCastStarted(Entity caster)
    {
        if (caster.audioSource != null && castSound != null)
            caster.audioSource.PlayOneShot(castSound);
    }
    // [Client]
    public virtual void OnCastFinished(Entity caster) { }
    // OnCastCanceled doesn't seem worth the Rpc bandwidth, since spell effects
    // can check if caster.currentSpell == -1
    // tooltip /////////////////////////////////////////////////////////////////
    // fill in all variables into the tooltip
    // this saves us lots of ugly string concatenation code.
    // (dynamic ones are filled in Spell.cs)
    // -> note: each tooltip can have any variables, or none if needed
    // -> example usage:
    /*
    <b>{NAME}</b>
    Description here...
    Damage: {DAMAGE}
    Cast Time: {CASTTIME}
    Cooldown: {COOLDOWN}
    Cast Range: {CASTRANGE}
    AoE Radius: {AOERADIUS}
    Mana Costs: {MANACOSTS}
    */
    public virtual string ToolTip()
    {
        Player player = Player.localPlayer;
        StringBuilder tip = new StringBuilder(toolTip);
        tip.Replace("{NAME}", DisplayName);
        tip.Replace("{CASTTIME}", string.Format("{0} {1} with higher skill", GlobalFunc.ExamineLimitText(castTimeNewbe, GlobalVar.spellCastTimeText), GlobalFunc.ExamineLimitText((castTimeMaster+0.1f)/(castTimeNewbe+0.1f),GlobalVar.relationMasterNoobText)));
        tip.Replace("{COOLDOWN}", string.Format("{0} {1} with higher skill", GlobalFunc.ExamineLimitText(cooldownNewbe, GlobalVar.spellCooldownTimeText), GlobalFunc.ExamineLimitText((cooldownMaster + 0.1f) / (cooldownNewbe + 0.1f), GlobalVar.relationMasterNoobText)));
        tip.Replace("{CASTRANGE}", string.Format("{0} {1} with higher skill", GlobalFunc.ExamineLimitText(castRangeNewbe, GlobalVar.spellRangeText), GlobalFunc.ExamineLimitText((castRangeMaster + 0.1f) / (castRangeNewbe + 0.1f), GlobalVar.relationMasterNoobText)));
        tip.Replace("{MANACOSTS}", string.Format("{0} {1} with higher skill", GlobalFunc.ExamineLimitText(manaCostsNewbe/player.manaMax, GlobalVar.manaConsumptionText), GlobalFunc.ExamineLimitText((manaCostsMaster + 0.1f) / (manaCostsNewbe + 0.1f), GlobalVar.relationMasterNoobText)));
        tip.Replace("{SKILL}", Skills.Name(skill));
        tip.Replace("SKILLLEVEL", GlobalFunc.FirstToUpper(GlobalFunc.ExamineLimitText(skillLevel, GlobalVar.skillLevelText)));
        return tip.ToString();
    }
    public string toolTipText
    {
        get { return toolTip.Replace("\n", "</br>"); }
        set { toolTip = value.Replace("</br>", "\n"); }
    }
    // caching /////////////////////////////////////////////////////////////////
    // we can only use Resources.Load in the main thread. we can't use it when
    // declaring static variables. so we have to use it as soon as 'dict' is
    // accessed for the first time from the main thread.
    // -> we save the hash so the dynamic item part doesn't have to contain and
    //    sync the whole name over the network
    static Dictionary<int, ScriptableSpell> cache;
    public static Dictionary<int, ScriptableSpell> dict
    {
        get
        {
            // load if not loaded yet
            return cache ?? (cache = Resources.LoadAll<ScriptableSpell>("SpellsBuffsStatusEffects").ToDictionary(
                spell => spell.name.GetStableHashCode(), spell => spell)
            );
        }
    }
}
