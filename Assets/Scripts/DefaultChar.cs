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

[CreateAssetMenu(menuName = "Anega/Default Char", order = 901)]
public class DefaultChar : ScriptableObject
{
    public int identifier;
    public string headline;
    [TextArea(1, 30)] public string description;
    public string apperance;
    public string attributes;
    public string abilities;
    public string skills;
    public StarterSet starterSet;
}


public class DefaultChars
{

    public int count
    {
        get { return listOfDefaultChars.Count; }
    }


    public List<DefaultChar> listOfDefaultChars = new List<DefaultChar>();

    public void Reload()
    {
        listOfDefaultChars.Clear();
        listOfDefaultChars = Resources.LoadAll<DefaultChar>("DefaultChars").ToList();
    }
    public string headline
    {
        get { return "Don't worry about being worried. You're heading out on an adventure and you can always change your mind along the way and try something else."; }
    }

    public string description
    {
        get
        {
            return "The game provides some predefined character settings. The  settings cannot be perfect since nobody know how your perfect beeing within the given limitation looks like." + Environment.NewLine
            + "You can choose a char and play directly. This bypasses the extensive creation of a special character." + Environment.NewLine
            + "Of course you are free to customize this character in the following six steps.";
        }
    }
}
