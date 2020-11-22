/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// This component copies a Transform's position to automatically follow it,
// which is useful for the camera.
using UnityEngine;
public class CopyPosition : MonoBehaviour
{
    public bool x, y, z;
    public Transform target;
    public float yOffset = 25;
    void Update()
    {
        if (!target) return;
        transform.position = new Vector3(
            (x ? target.position.x : transform.position.x),
            (y ? target.position.y + yOffset : transform.position.y),
            (z ? target.position.z : transform.position.z));
    }
}
