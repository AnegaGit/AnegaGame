/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
public class UnityVersionCheck : MonoBehaviour
{
    void Awake()
    {
        LogFile.WriteLog(LogFile.LogLevel.Always,string.Format("Unity version: " + Application.unityVersion));
    }
}
