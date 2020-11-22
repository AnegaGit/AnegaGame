/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System;
using System.Text;
using UnityEngine;
using Mirror;
[Serializable]
public partial struct Container
{
    public int id;
    public int type;
    public string name;
    public int slots;
    public int containers;
    public string miscellaneousSync;

    // constructors
    public Container(int id, int type, int slots, int containers, string name, string miscellaneous)
    {
        this.id = id;
        this.type = type;
        this.slots = Mathf.Max(0, slots);
        this.containers = Mathf.Max(0, containers);
        this.name = name + "";
        this.miscellaneousSync = miscellaneous + "";
    }
}
public class SyncListContainer : SyncListSTRUCT<Container>
{
    public int IndexOfType(int type)
    {
        return this.FindIndex(x => x.type == type);
    }

    public int IndexOfId(int id)
    {
        return this.FindIndex(x => x.id == id);
    }

    public Container Container (int id)
    {
        int index = IndexOfId(id);
        if(index== -1)
        {
            Container notFound = new Container();
            notFound.id = -1;
            return notFound;
        }
        else
        {
            return this[index];
        }
    }

    public int SlotsInId(int id)
    {
        int index = IndexOfId(id);
        if (index < 0)
            return 0;
        else
            return this[index].slots;
    }
}
