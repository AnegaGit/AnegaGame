/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using System;

public class AreaProperties : MonoBehaviour
{
    public string displayName;
    public bool skyIsVisible;
}

[Serializable]
public class AreaInfo
{
    public string displayName="";
    public bool skyIsVisible;

    //This part is load from the game on awake in Universal.cs
    public int zeroX, zeroY, zeroZ;
    public int widthX, widthY, widthZ;
    public int sizeOne;
    public bool hasVegetationMap;
    public Texture2D vegetationMap;
    public bool hasMagicMap;
    public Texture2D magicMap;



    public AreaInfo(string displayName, bool skyIsVisible, int zeroX, int zeroY, int zeroZ,int widthX, int widthY, int widthZ, int sizeOne, bool hasVegetationMap,Texture2D vegetationMap,bool hasMagicMap ,Texture2D magicMap)
    {
        this.displayName = displayName;
        this.skyIsVisible = skyIsVisible;
        this.zeroX = zeroX;
        this.zeroY = zeroY;
        this.zeroZ = zeroZ;
        this.widthX = widthX;
        this.widthY = widthY;
        this.widthZ = widthZ;
        this.sizeOne = sizeOne;
        this.hasMagicMap = hasMagicMap;
        this.magicMap = magicMap;
        this.hasVegetationMap = vegetationMap;
        this.vegetationMap = vegetationMap;
    }
}
