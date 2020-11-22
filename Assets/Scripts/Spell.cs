/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// The Spell struct only contains the dynamic spell properties, so that the
// static properties can be read from the scriptable object. The benefits are
// low bandwidth and easy Player database saving (saves always refer to the
// scriptable spell, so we can change that any time).
//
// Spells have to be structs in order to work with SyncLists.
//
// We implemented the cooldowns in a non-traditional way. Instead of counting
// and increasing the elapsed time since the last cast, we simply set the
// 'end' Time variable to NetworkTime.time + cooldown after casting each time.
// This way we don't need an extra Update method that increases the elapsed time
// for each spell all the time.
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mirror;
[Serializable]
public partial struct Spell
{
    // hashcode used to reference the real ItemTemplate (can't link to template
    // directly because synclist only supports simple types). and syncing a
    // string's hashcode instead of the string takes WAY less bandwidth.
    public int hash;
    // dynamic stats (cooldowns etc.)
    public double castTimeEnd; // server time. double for long term precision.
    public double cooldownEnd; // server time. double for long term precision.
    // constructors
    public Spell(ScriptableSpell data)
    {
        hash = data.name.GetStableHashCode();
        // ready immediately
        castTimeEnd = cooldownEnd = NetworkTime.time;
    }
    // wrappers for easier access
    public ScriptableSpell data
    {
        get
        {
            // show a useful error message if the key can't be found
            // note: ScriptableSpell.OnValidate 'is in resource folder' check
            //       causes Unity SendMessage warnings and false positives.
            //       this solution is a lot better.
            if (!ScriptableSpell.dict.ContainsKey(hash))
                throw new KeyNotFoundException("There is no ScriptableSpell with hash=" + hash + ". Make sure that all ScriptableSpells are in the Resources folder so they are loaded properly.");
            return ScriptableSpell.dict[hash];
        }
    }
    public string name { get { return data.name; } }
    public string displayName { get { return data.displayName; } }
    public float CastTime(Entity caster = null)
    {
        return data.CastTime(caster);
    }
    public float Cooldown(Entity caster = null)
    {
        return data.Cooldown(caster);
    }
    public float CastRange(Entity caster = null)
    {
        return data.CastRange(caster);
    }
    public int ManaCosts(Entity caster = null)
    {
        return data.ManaCosts(caster);
    }
    public bool followupDefaultAttack { get { return data.followupDefaultAttack; } }
    public Sprite image { get { return data.image; } }
    public bool showCastBar { get { return data.showCastBar; } }
    public bool cancelCastIfTargetDied { get { return data.cancelCastIfTargetDied; } }
    public int skill { get { return (int)data.skill; } }
    public int skillLevel { get { return data.skillLevel; } }
    public ScriptableSpell predecessor { get { return data.predecessor; } }
    //public int predecessorLevel { get { return data.predecessorLevel; } }
    public bool requiresWeapon { get { return data.requiresWeapon; } }
    // events
    public bool CheckSelf(Entity caster, bool checkSpellReady = true)
    {
        return (!checkSpellReady || IsReady()) &&
               data.CheckSelf(caster);
    }
    public bool CheckTarget(Entity caster) { return data.CheckTarget(caster); }
    public bool CheckDistance(Entity caster, out Vector3 destination) { return data.CheckDistance(caster, out destination); }
    public void Apply(Entity caster) { data.Apply(caster); }
    public void ExecutePositionSpell(Entity caster, Vector3 targetPosition) { data.ExecutePositionSpell(caster, targetPosition); }
    // tooltip - dynamic part
    public string ToolTip()
    {
        // we use a StringBuilder so it is easy to modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(data.ToolTip());
        return tip.ToString();
    }
    public float CastTimeRemaining()
    {
        // how much time remaining until the casttime ends? (using server time)
        return NetworkTime.time >= castTimeEnd ? 0 : (float)(castTimeEnd - NetworkTime.time);
    }
    public bool IsCasting()
    {
        // we are casting a spell if the casttime remaining is > 0
        return CastTimeRemaining() > 0;
    }
    public float CooldownRemaining()
    {
        // how much time remaining until the cooldown ends? (using server time)
        return NetworkTime.time >= cooldownEnd ? 0 : (float)(cooldownEnd - NetworkTime.time);
    }
    public bool IsReady()
    {
        return CooldownRemaining() == 0;
    }
}
public class SyncListSpell : SyncListSTRUCT<Spell>
{
    public int IdByName(string spellName)
    {
        return this.FindIndex(spell => spell.name == spellName);
    }

    /// <summary>
    /// returns empty spell if nothing found
    /// </summary>
    public Spell SpellByName(string spellName)
    {
        int id = IdByName(spellName);
        if (id < 0)
        {
            return new Spell();
        }
        else
        {
            return this[id];
        }
    }

    /// <summary>
    /// first StandardFighting, there should be not more than one
    /// </summary>
    public int IdOfStandardFighting()
    {
        return this.FindIndex(spell => spell.data is StandardFightingSpell);
    }
}
