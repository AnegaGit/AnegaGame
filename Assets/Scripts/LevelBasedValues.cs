/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// level based values for spell levels, player level based health, etc.
// -> easier than managing huge arrays of level stats
// -> easier than abstract GetManaForLevel functions etc.
//
// note: levels are 1-based. we use level-1 in the calculations so that level 1
//       has 0 bonus
using System;
[Serializable]
public struct LevelBasedInt
{
    public int baseValue;
    public int bonusPerLevel;
    public int Get(int level) { return baseValue + bonusPerLevel * (level - 1); }
}
[Serializable]
public struct LevelBasedLong
{
    public long baseValue;
    public long bonusPerLevel;
    public long Get(int level) { return baseValue + bonusPerLevel * (level - 1); }
}
[Serializable]
public struct LevelBasedFloat
{
    public float baseValue;
    public float bonusPerLevel;
    public float Get(int level) { return baseValue + bonusPerLevel * (level - 1); }
}