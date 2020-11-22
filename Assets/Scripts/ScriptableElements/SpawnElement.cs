/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Text;
using System.Collections.Generic;
using UnityEngine;
public class SpawnElement : ScriptableElement
{
    List<Transform> spawnPositions = new List<Transform>();
    public GameObject positions;
    public string description;
    public bool standardSpawn = false;
    public bool specialSpawn = false;
    public bool gmOnly = false;
    public override string ToolTip()
    {
        // we use a StringBuilder so it is easy to modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{DESCRIPTION}", description);
        return tip.ToString();
    }


    public Transform RandomPosition()
    {
        spawnPositions.Clear();
        foreach (Transform pos in positions.transform)
        {
            spawnPositions.Add(pos);
        }
        return spawnPositions[Random.Range(0, spawnPositions.Count)];
    }
}
