/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// If an item should be in the backpack it must be defined after the backpack item and get position -1
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Anega/Starter Set", order = 999)]
public class StarterSet : ScriptableObject
{
    public int identifier;
    public string headline;
    [TextArea(1, 30)] public string description;
    public int money;

    public List<StarterSetDefaultItem> defaultItems = new List<StarterSetDefaultItem>();
}

[Serializable]
public partial struct StarterSetDefaultItem
{
    public ScriptableItem item;
    public int amount;
    public int quality;
    public int durability;
    public int position;
}


public class StarterSets
{

    public int count
    {
        get { return listOfStarterSets.Count; }
    }


    public List<StarterSet> listOfStarterSets = new List<StarterSet>();

    public void Reload()
    {
        listOfStarterSets.Clear();
        listOfStarterSets = Resources.LoadAll<StarterSet>("StarterSets").ToList();
    }
    public string headline
    {
        get { return "It's not hard to decide what you want your life to be about. What's hard, is figuring out what you're willing to give up in order to do the things you really care about."; }
    }

    public string description
    {
        get
        {
            return "The starter set contains of some items and money. Additionally some properties such as health , stamnia and experience time might be influenced." + Environment.NewLine
            + "The different sets require a different role play to start with but are generally equal eventually.";
        }
    }
}
