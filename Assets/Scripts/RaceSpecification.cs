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
using UMA;


[CreateAssetMenu(menuName = "Anega/Race Specification", order = 903)]
public class RaceSpecification : ScriptableObject
{
    [Header("Dedicated Apperance")]
    public List<StringFloat> definitions = new List<StringFloat>();
}
