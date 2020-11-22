/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AlwaysUpright : MonoBehaviour
{
    float defaultX;
    float defaultZ;
    Vector3 reverseScale;
    // Update is called once per frame
    private void Awake()
    {
        Vector3 rotation = transform.rotation.eulerAngles;
        defaultX = rotation.x;
        defaultZ = rotation.z;
        reverseScale = new Vector3(transform.localScale.x, transform.localScale.y * -1, transform.localScale.z);
    }
    void Update()
    {
        Vector3 rotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(defaultX, rotation.y, defaultZ);
        if(transform.parent.localScale.y<0)
        transform.localScale = reverseScale;
    }
}
