/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
public class Blinking : MonoBehaviour
{
    public GameObject blinkingObject;
    public float speedSeconds=0.2f;
    float lastChange;

    // Start is called before the first frame update
    void Start()
    {
        lastChange = Time.time;
        blinkingObject.SetActive(true);
    }
    void Update()
    {
        if (Time.time - lastChange > speedSeconds)
        {
            blinkingObject.SetActive(!blinkingObject.activeSelf);
            lastChange = Time.time;
        }
    }
}
