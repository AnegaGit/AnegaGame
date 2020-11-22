/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhasedElement : MonoBehaviour
{
    public GameObject[] phases;

    private int currentPhase = 0;

    public int phase
    {
        get => currentPhase;
        set
        {
            int nextPhase = value;
            for (int i = 0; i < phases.Length; i++)
            {
                if (phases[i])
                {
                    phases[i].SetActive(i == nextPhase);
                }
            }
            currentPhase = nextPhase;
        }
    }
}
