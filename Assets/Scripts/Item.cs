/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// The Item struct only contains the dynamic item properties, so that the static
// properties can be read from the scriptable object.
//
// Items have to be structs in order to work with SyncLists.
//
// Use .Equals to compare two items. Comparing the name is NOT enough for cases
// where dynamic stats differ. E.g. two pets with different levels shouldn't be
// merged.
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mirror;
[Serializable]
public partial struct Item
{
    // hashcode used to reference the real ScriptableItem (can't link to data
    // directly because synclist only supports simple types). and syncing a
    // string's hashcode instead of the string takes WAY less bandwidth.
    public int hash;
    // dynamic stats (cooldowns etc. later)
    public GameObject objectInGame; // used in: summonableItems
    public int data1; //  used in: summonableItems as health; containerItems as containerID
    public int data2; //  used in: summonableItems as level
    public int data3; //  used in: summonableItems as experience
    public int durability;
    public int quality;
    public string miscellaneousSync
    {
        get { return data.GetMiscellaneousAll(); }
        set { data.InitializeMiscellaneous(value); }
    }
    // constructors
    public Item(ScriptableItem data)
    {
        hash = data.name.GetStableHashCode();
        objectInGame = null;
        data1 = data is SummonableItem ? ((SummonableItem)data).summonPrefab.healthMax : 0;
        data2 = data is SummonableItem ? 1 : 0;
        data3 = 0;
        durability = 100;
        quality = 33;
        miscellaneousSync = "";
    }
    // wrappers for easier access
    public ScriptableItem data
    {
        get
        {
            // show a useful error message if the key can't be found
            // note: ScriptableItem.OnValidate 'is in resource folder' check
            //       causes Unity SendMessage warnings and false positives.
            //       this solution is a lot better.
            if (!ScriptableItem.dict.ContainsKey(hash))
                throw new KeyNotFoundException("There is no ScriptableItem with hash=" + hash + ". Make sure that all ScriptableItems are in the Resources folder so they are loaded properly.");
            return ScriptableItem.dict[hash];
        }
    }
    public string name { get { return data.displayName; } }
    public string itemName { get { return data.name; } }
    public int maxStack { get { return data.maxStack; } }
    public long price { get { return data.price; } }
    public int weight { get { return data.weight; } }
    public Sprite image { get { return data.image; } }
    // tooltip
    public string ToolTip()
    {
        // we use a StringBuilder so it is easy to modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(data.ToolTip());
        tip.Replace("{DATA1}", data1.ToString());
        tip.Replace("{DATA2}", data2.ToString());
        tip.Replace("{DATA3}", data3.ToString());
        tip.Replace("{QUALITY}", GlobalFunc.ExamineLimitText((float)quality / GlobalVar.itemQualityMax, GlobalVar.itemQualityBase));
        tip.Replace("{DURABILITY}", GlobalFunc.ExamineLimitText((float)durability / GlobalVar.itemDurabilityMax, GlobalVar.itemDurabilityBase));
        return tip.ToString();
    }

    //miscellaneous
    //change in list changes sync string too
    public void SetMiscellaneous(string key, string value)
    {
        data.SetMiscellaneous(key, value);
        miscellaneousSync = data.GetMiscellaneousAll();
    }


}
