/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
public class StaticFlame : MonoBehaviour
{
    public Vector3 rotation = new Vector3(-90, 0, 0);

    private void Awake()
    {
        // Flame always vertical
        transform.rotation = Quaternion.Euler(rotation);
    }
}
