/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Saves the item info in a ScriptableObject that can be used ingame by
// referencing it from a MonoBehaviour. It only stores an item's static data.
//
// We also add each one to a dictionary automatically, so that all of them can
// be found by name without having to put them all in a database. Note that we
// have to put them all into the Resources folder and use Resources.LoadAll to
// load them. This is important because some items may not be referenced by any
// entity ingame (e.g. when a special event item isn't dropped anymore after the
// event). But all items should still be loadable from the database, even if
// they are not referenced by anyone anymore. So we have to use Resources.Load.
// (before we added them to the dict in OnEnable, but that's only called for
//  those that are referenced in the game. All others will be ignored be Unity.)
//
// An Item can be created by right clicking the Resources folder and selecting
// Create -> uMMORPG Item. Existing items can be found in the Resources folder.
//
// Note: this class is not abstract so we can create 'useless' items like recipe
// ingredients, etc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public partial class ScriptableItem : ScriptableObject
{
    [Header("Base Stats")]
    public string itemName;
    public int maxStack;
    public int price;
    public int weight;
    public float maxDurability;
    [SerializeField, TextArea(1, 30)] protected string toolTip; // not public, use ToolTip()
    public Sprite image;
    // tooltip /////////////////////////////////////////////////////////////////
    public virtual string ToolTip()
    {
        // we use a StringBuilder so it is easy to modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(toolTip);
        tip.Replace("{NAME}", displayName);
        tip.Replace("{PRICE}", Money.MoneyText(price));
        return tip.ToString();
    }
    public string toolTipText
    {
        get { return toolTip.Replace("\n", "</br>"); }
        set { toolTip = value.Replace("</br>", "\n"); }
    }

    // can we equip this item into this specific equipment slot?
    // every item can be hold in the belt
    public virtual bool CanEquip(Player player, int equipmentIndex)
    {
        if (GlobalFunc.IsInBelt(equipmentIndex))
            return true;
        return false;
    }
    // caching /////////////////////////////////////////////////////////////////
    // we can only use Resources.Load in the main thread. we can't use it when
    // declaring static variables. so we have to use it as soon as 'dict' is
    // accessed for the first time from the main thread.
    // -> we save the hash so the dynamic item part doesn't have to contain and
    //    sync the whole name over the network
    static Dictionary<int, ScriptableItem> cache;
    public static Dictionary<int, ScriptableItem> dict
    {
        get
        {
            // load if not loaded yet
            if (cache is null)
            {
                cache = Resources.LoadAll<ScriptableItem>("Items").ToDictionary(
                    item => item.name.GetStableHashCode(), item => item); //<<< here the warning appears
            }
            return cache;
        }
    }
    public Dictionary<string, string> miscellaneous = new Dictionary<string, string>();
    public void InitializeMiscellaneous(string saveString)
    {
        miscellaneous.Clear();
        if (saveString.Length > 1)
        {
            string[] tmp = saveString.Split('#');
            for (int i = 0; i < tmp.Length - 1; i += 2)
            {
                miscellaneous.Add(tmp[i], tmp[i + 1]);
            }
        }
    }
    public string GetMiscellaneousAll()
    {
        string tmp = "";
        if (miscellaneous.Count > 0)
        {
            foreach (KeyValuePair<string, string> item in miscellaneous)
            {
                tmp += item.Key + "#" + item.Value.Replace('#', '_') + "#";
            }
        }
        return tmp;
    }
    public bool GetMiscellaneous(string key, out string value)
    {
        if (miscellaneous.TryGetValue(key, out string tmp))
        {
            value = tmp;
            return true;
        }
        else
        {
            value = "";
            return false;
        }
    }
    public bool GetMiscellaneous(string key, out long value)
    {
        if (GetMiscellaneous(key, out string tmp))
            if (long.TryParse(tmp, out long result))
            {
                value = result;
                return true;
            }

        value = 0;
        return false;
    }

    public bool GetMiscellaneous(string key, out float value)
    {
        if (GetMiscellaneous(key, out string tmp))
            if (float.TryParse(tmp, out float result))
            {
                value = result;
                return true;
            }

        value = 0;
        return false;
    }

    public string displayName
    {
        get
        {
            if (miscellaneous.TryGetValue("name", out string tmp))
            {
                if (!Utils.IsNullOrWhiteSpace(tmp))
                {
                    return tmp;
                }
            }
            if (Utils.IsNullOrWhiteSpace(itemName))
            {
                return this.name;
            }
            else
            {
                return itemName;
            }
        }
        set
        {
            if (value == this.name)
                SetMiscellaneous("name", "");
            else
                SetMiscellaneous("name", value);
        }
    }

    //>>>This might be dangerous sice the change is not propageted to all instances!
    public void SetMiscellaneous(string key, string value)
    {
        if (miscellaneous.ContainsKey(key))
        {
            if (value.Length > 0)
                miscellaneous[key] = value;
            else
                miscellaneous.Remove(key);
        }
        else
        {
            miscellaneous.Add(key, value);
        }
    }
}
