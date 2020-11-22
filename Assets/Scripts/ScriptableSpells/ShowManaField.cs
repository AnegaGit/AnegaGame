/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Shows mana density to player around player position
// No use for enitities
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Anega/Spell/Mana Field", order = 501)]
public class ShowManaField : ScriptableSpell
{
    [Header("Misc")]
    public GameObject manaFieldIndicatorPrefab;
    private const int displayDurationNoob = 70;
    private const int displayDurationMaster = 170;
    private const int displayDurationObfuscation = 50;

    public override bool CheckTarget(Entity caster)
    {
        // no target necessary
        return true;
    }
    public override bool CheckDistance(Entity caster, out Vector3 destination)
    {
        // can cast anywhere
        // cast at position of player
        destination = caster.transform.position;
        return true;
    }
    public override void Apply(Entity caster)
    {
        // finally learn a little bit;
        Player player = (Player)caster;
        player.LearnSkill(skill, skillLevel, CastTime(caster));
    }

    //[Client]
    public override void OnCastFinished(Entity caster)
    {
        int spellSize = Mathf.Max(1, (int)CastRange(caster));
        int framesAlive = (int)GlobalFunc.ValueFromProportion(NonLinearCurves.GetFloat0_1(GlobalVar.castRangeMasteryNonlinear, ((Player)caster).skills.LevelOfSkill(skill)), displayDurationNoob, displayDurationMaster);

        SetSingleIndicator(caster.transform.position, 0, 0, framesAlive);
        for (int range = 1; range <= spellSize; range++)
        {
            for (int i = -range; i <= range; i++)
            {
                SetSingleIndicator(caster.transform.position, i, -range, framesAlive);
                SetSingleIndicator(caster.transform.position, i, range, framesAlive);
                if (i > -range && i < range)
                {
                    SetSingleIndicator(caster.transform.position, -range, i, framesAlive);
                    SetSingleIndicator(caster.transform.position, range, i, framesAlive);
                }
            }
        }
    }

    //set Indicator
    void SetSingleIndicator(Vector3 position, float xDelta, float zDelta, int framesAlive)
    {
        position.x += xDelta;
        position.z += zDelta;

        float strength = Universal.GetAmbientMana(position);
        framesAlive = (int)GlobalFunc.RandomObfuscation(framesAlive, displayDurationObfuscation);

        GameObject go = Instantiate(manaFieldIndicatorPrefab);
        go.transform.position = position;
        go.GetComponent<ProjectorStrength>().Strength = strength;
        go.GetComponent<ShortTermProjector>().framesUntilDelete = framesAlive;
    }
}
