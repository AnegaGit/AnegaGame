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

public class ShortTermProjector : MonoBehaviour
{
    public int framesUntilDelete = PlayerPreferences.framesUntilFade;
    public Projector projector;

    private int framesRemaining = int.MaxValue;

    void Start()
    {
        framesRemaining = framesUntilDelete;
    }

    public float size { set { projector.orthographicSize = value; } }

    // Update is called once per frame
    void Update()
    {
        framesRemaining--;
        if (framesRemaining <= 0)
            Destroy(gameObject);
    }
}
