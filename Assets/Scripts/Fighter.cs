/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// entity types that can attack or defend themself except player)
using UnityEngine;
using Mirror;
public abstract class Fighter : Entity
{
    // fighting parameter are used for no player character only
    [Header("Attack")]
    public Skills.Skill attackSkill;
    [Range(0, 100)] public int attackLevel;
    public int attackMin;
    public int attackMax;
    public float attackDistance;
    [Header("Dodge")]
    [Tooltip("0..1; larger will always dodge")]
    [Range(0, 10)] public float dodgeProbability = 10; // 0..1; 10 will always dodge, not attackable
    [Header("Parry")]
    [Range(0, 1)] public float blockProbability = 0; // 0..1
    public int blockDamageIgnore = 0; // if damage is larger ==> stun
    [Header("Armor")]
    public Skills.Skill armorSkill;
    [Range(0, 100)] public int defenseLevel;
    public int armorNotConsumed;
    public int armorMin = 0;
    public int armorMax = 1000;

}

