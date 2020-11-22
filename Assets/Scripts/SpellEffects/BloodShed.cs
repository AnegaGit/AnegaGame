/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;

public class BloodShed : MonoBehaviour
{
    public void BloodAmount(int damage, int maxDamange,bool isLocalPlayer)
    {
        // particle amount in between 1 and playerPrefence.maxBlood
        // max blood limited
        // nonlinearity applied
        var main = GetComponent<ParticleSystem>().main;
        main.maxParticles = NonLinearCurves.IntFromCurvePosition(GlobalVar.damageBloodNonlinear, 1.0 * damage / maxDamange, 0, 1, 1, Mathf.Clamp(PlayerPreferences.maxBlood, 1, GlobalVar.damageMaxBloodPermitted));
        if (isLocalPlayer)
        {
            main.startColor = PlayerPreferences.myBloodColor;
        }
        else
        {
           main.startColor = PlayerPreferences.otherBloodColor;
        }
    }
}
