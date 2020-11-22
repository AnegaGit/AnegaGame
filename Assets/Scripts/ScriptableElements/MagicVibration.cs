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
public class MagicVibration : MonoBehaviour
{
    public float spinX = 0f;
    public float spinY = 0f;
    public float spinZ = 0f;
    public float vibrationSpeed = 0f;
    public float vibrationX = 0f;
    public float vibrationY = 0f;
    public float vibrationZ = 0f;

    private bool isSpin = false;
    private Vector3 spin;
    private bool isVibration = false;
    private Vector3 basePosition;

    private void Start()
    {
        if (Mathf.Abs(spinX) + Mathf.Abs(spinY) + Mathf.Abs(spinZ) > 0)
            isSpin = true;
        else
            isSpin = false;
        spin = new Vector3(spinX, spinY, spinZ);

        if ((Mathf.Abs(vibrationX) + Mathf.Abs(vibrationY) + Mathf.Abs(vibrationZ)) > 0)
        {
            isVibration = true;
            vibrationX = Mathf.Abs(vibrationX) / 2;
            vibrationY = Mathf.Abs(vibrationY) / 2;
            vibrationZ = Mathf.Abs(vibrationZ) / 2;
        }
        else
            isVibration = false;

        basePosition = transform.localPosition;
    }
    private void Update()
    {
        if (isSpin)
            transform.Rotate(spin);
        if (isVibration)
        {
            transform.localPosition = basePosition + new Vector3(Random.value * vibrationX, Random.value * vibrationY, Random.value * vibrationZ);
        }
    }
    public void ParameterUpdated()
    {
        Start();
    }
}
