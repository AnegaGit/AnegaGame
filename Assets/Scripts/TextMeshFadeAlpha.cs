/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
[RequireComponent(typeof(TextMesh))]
public class TextMeshFadeAlpha : MonoBehaviour
{
    public TextMesh textMesh;
    public float delay = 0;
    public float duration = 1;
    float perSecond;
    float startTime;
    void Start()
    {
        // calculate by how much to fade per second
        perSecond = textMesh.color.a / duration;
        // calculate start time
        startTime = Time.time + delay;
    }
    void Update()
    {
        if (Time.time >= startTime)
        {
            // fade all text meshes (in children too in case of shadows etc.)
            Color color = textMesh.color;
            color.a -= perSecond * Time.deltaTime;
            textMesh.color = color;
        }
    }
}
