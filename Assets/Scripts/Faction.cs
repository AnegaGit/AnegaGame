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
public class Faction : MonoBehaviour
{
    public static int None = 0;
    public static int Wisdom = 1;
    public static int Wealth = 2;
    public static int Honor = 3;
    public Transform[] respawnPosition = new Transform[4];
}
