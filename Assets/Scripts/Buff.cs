/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Buffs are like Spells, but for the Buffs list.
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mirror;
[Serializable]
public partial struct Buff
{
    // hashcode used to reference the real ScriptableSpell (can't link to data
    // directly because synclist only supports simple types). and syncing a
    // string's hashcode instead of the string takes WAY less bandwidth.
    public int hash;
    // dynamic stats (cooldowns etc.)
    public float level; // value related to max
    public double buffTimeEnd; // server time. double for long term precision.
    // constructors
    public Buff(BuffSpell data,int level)
    {
        hash = data.name.GetStableHashCode();
        this.level = level;
        buffTimeEnd = NetworkTime.time + data.buffTime; // start buff immediately
    }
    public Buff(BuffSpell data,float level,float buffTime)
    {
        hash = data.name.GetStableHashCode();
        this.level = level;
        buffTimeEnd = NetworkTime.time + buffTime; // start buff immediately
    }    // wrappers for easier access
    public BuffSpell data
    {
        get
        {
            // show a useful error message if the key can't be found
            // note: ScriptableSpell.OnValidate 'is in resource folder' check
            //       causes Unity SendMessage warnings and false positives.
            //       this solution is a lot better.
            if (!ScriptableSpell.dict.ContainsKey(hash))
                throw new KeyNotFoundException("There is no ScriptableSpell with hash=" + hash + ". Make sure that all ScriptableSpells are in the Resources folder so they are loaded properly.");
            return (BuffSpell)ScriptableSpell.dict[hash];
        }
    }
    public string name { get { return data.name; } }
    public Sprite image { get { return data.image; } }
    public float buffTime { get { return data.buffTime; } }
    public int bonusHealth { get { return (int)(data.bonusHealthMax*level); } }
    public int bonusMana { get { return (int)(data.bonusManaMax*level); } }
    public float bonusHealthPerSecond { get { return data.bonusHealthPerSecondMax*level; } }
    public float bonusManaPerSecond { get { return data.bonusManaPerSecondMax*level; } }
    public float bonusSpeed { get { return data.bonusSpeedMax*level; } }
    // tooltip - runtime part
    public string ToolTip()
    {
        // we use a StringBuilder so it is easy to modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(data.ToolTip());
        return tip.ToString();
    }
    public float BuffTimeRemaining()
    {
        // how much time remaining until the buff ends? (using server time)
        return NetworkTime.time >= buffTimeEnd ? 0 : (float)(buffTimeEnd - NetworkTime.time);
    }
}
public class SyncListBuff : SyncListSTRUCT<Buff> {}
