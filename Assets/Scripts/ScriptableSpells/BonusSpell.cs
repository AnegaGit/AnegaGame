/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Base type for bonus spell templates.
// => can be used for passive spells, buffs, etc.
using System.Text;
using UnityEngine;
using Mirror;
public abstract class BonusSpell : ScriptableSpell
{
    [Header("Bonus")]
    public bool bonusRelative = false;
    public int bonusHealthMin;
    public int bonusHealthMax;
    public int bonusManaMin;
    public int bonusManaMax;
    public float bonusHealthPerSecondMin; // can be negative too
    public float bonusHealthPerSecondMax; // can be negative too
    public float bonusManaPerSecondMin; // can be negative too
    public float bonusManaPerSecondMax; // can be negative too
    public float bonusSpeedMin; // can be negative too
    public float bonusSpeedMax; // can be negative too
    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{BONUSHEALTHMAX}", bonusHealthMax.ToString());
        tip.Replace("{BONUSMANAMAX}", bonusManaMax.ToString());
        tip.Replace("{BONUSHEALTHPERSECOND}", Mathf.RoundToInt(bonusHealthPerSecondMax * 100).ToString());
        tip.Replace("{BONUSMANAPERSECOND}", Mathf.RoundToInt(bonusManaPerSecondMax * 100).ToString());
        tip.Replace("{BONUSSPEED}", bonusSpeedMax.ToString("F2"));
        return tip.ToString();
    }
    public float CalculateLevel(int value)
    {
        return CalculateLevel((float)value);
    }
    public float CalculateLevel(float value)
    {
        float level = 0;
        if (bonusHealthMax != 0)
        {
            level = value / bonusHealthMax;
        }
        else if (bonusManaMax != 0)
        {
            level = value / bonusManaMax;
        }
        else if (bonusHealthPerSecondMax != 0)
        {
            level = value / bonusHealthPerSecondMax;
        }
        else if (bonusManaPerSecondMax != 0)
        {
            level = value / bonusManaPerSecondMax;
        }
        else if(bonusSpeedMax!=0)
        {
            level = value / bonusSpeedMax;
        }
        return level;
    }
}
