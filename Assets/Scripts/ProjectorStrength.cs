/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectorStrength : MonoBehaviour
{
    public float strength;
    public Projector projector;


    private void Awake()
    {
       Material specialMaterial = new Material (projector.material);
        projector.material = specialMaterial;
        Strength = strength;
    }

    public float Strength
    {
        set
        {
            strength = Mathf.Clamp(value, 0, 1);
            projector.material.color = Color.Lerp(Color.black,Color.white,  strength);
        }
    }
}
