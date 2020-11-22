/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LightElement : MonoBehaviour
{
    [Header("Light Properties")]
    [Header("Flame")]
    public bool hasFlame;
    public GameObject flame;
    public Light lightSource;
    [Header("Glow")]
    public bool hasGlow;
    public GameObject glowPart;
    Color glowColor;
    public bool _isLightOn = false;
    bool _isLightOnLastButOne;
    float lightIntensityMax = 1;


    bool isActivated
    {
        get { return _isLightOn; }
        set { _isLightOn = value; }
    }

    void Awake()
    {
        if (hasFlame)
        {
            lightIntensityMax = lightSource.intensity;
        }
        if (hasGlow)
        {
            glowColor = glowPart.GetComponent<Renderer>().material.GetColor("_EmissionColor");
        }
    }

    // set initial value
    // Start is called before the first frame update
    void Start()
    {
        ChangeLight();
    }
    // Update is called once per frame
    void Update()
    {
        if (_isLightOn != _isLightOnLastButOne)
        {
            ChangeLight();
            _isLightOnLastButOne = _isLightOn;
        }
    }

    // Change light state
    void ChangeLight()
    {
        if (hasFlame)
        {
            flame.SetActive(_isLightOn);
            lightSource.gameObject.SetActive(_isLightOn);
        }
        if (hasGlow)
        {
            glowPart.SetActive(_isLightOn);
        }
    }

    // light item handle parameter
    public void SwitchDirect(bool lightOn)
    {
        _isLightOn = lightOn;
    }
    public void SwitchDirect(bool lightOn, float relativeTime)
    {
        SwitchDirect(lightOn);
        if (hasFlame)
        {
            lightSource.intensity = NonLinearCurves.GetInterimFloat0_1(GlobalVar.lightIntensityReductionCurve, 1 - relativeTime) * lightIntensityMax;
        }
        if (hasGlow)
        {
            glowPart.GetComponent<Renderer>().material.SetColor("_EmissionColor", glowColor * NonLinearCurves.GetInterimFloat0_1(GlobalVar.lightIntensityReductionCurve, 1 - relativeTime));
        }
    }
}
