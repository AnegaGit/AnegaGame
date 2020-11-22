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
public class DayNight : MonoBehaviour
{
    Light sunLight;
    //enum PartOfDay { night, dawn, day, dusk };
    //PartOfDay partOfDay = PartOfDay.night;
    GameTime gameTime = new GameTime();
    float dawnStart;
    float dawnEnd;
    float duskStart;
    float duskEnd;
    float dayLength;
    float twilightLength;
    float maxX;
    bool playerActive = false;

    Color ambientDayLight = new Color(GlobalVar.ambientLightDay, GlobalVar.ambientLightDay, GlobalVar.ambientLightDay, 1f);
    Color ambientNightLight = Color.black;
    Color fogColorDay = new Color(GlobalVar.dayFog, GlobalVar.dayFog, GlobalVar.dayFog);
    Color fogColorNight = new Color(GlobalVar.nightFog, GlobalVar.nightFog, GlobalVar.nightFog);
    // Start is called before the first frame update
    void Start()
    {
        sunLight = gameObject.GetComponent<Light>();
        Invoke("WaitForLocalPlayer", GlobalVar.repeatInitializationAttempt);
    }

    void WaitForLocalPlayer()
    {
        Player player = Player.localPlayer;
        if (!player)
            Invoke("WaitForLocalPlayer", GlobalVar.repeatInitializationAttempt);
        else
        {
            CalculateAmbientLight();
            playerActive = true;
            InvokeRepeating("BaseCalculation", 0, 600);
        }
    }

    public void CalculateAmbientLight()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            float greyScale = GlobalVar.nightVisionAmbientLightSurface[player.darkVision];
            ambientNightLight = new Color(greyScale, greyScale, greyScale, 1);
            if (player.canSeeSky)
            {
                ambientDayLight = new Color(GlobalVar.ambientLightDay, GlobalVar.ambientLightDay, GlobalVar.ambientLightDay, 1f);
            }
            else
            {
                ambientDayLight = ambientNightLight;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerActive)
        {
            if (Player.localPlayer.canSeeSky)
            {
                gameTime.Now();
                float timeOfDay = (float)gameTime.PartOfDay;
                if (timeOfDay < dawnStart || timeOfDay >= duskEnd)
                    ItIsNight();
                else if (timeOfDay < dawnEnd)
                    ItIsDawn(timeOfDay);
                else if (timeOfDay < duskStart)
                    ItIsDay(timeOfDay);
                else
                    ItIsDusk(timeOfDay);
            }
            else
                ItIsNight();
        }
        else
            BeforePlayer();
    }
    public void BaseCalculation()
    {
        gameTime.Now();
        dayLength = (float)gameTime.CurrentDayPortion;
        dawnStart = (float)gameTime.CurrentNightPortion / 2;
        dawnEnd = (1 - dayLength) / 2;
        duskStart = 1 - dawnEnd;
        duskEnd = 1 - dawnStart;
        twilightLength = dawnEnd - dawnStart;
        double proportion = GlobalFunc.ProportionFromValue(dayLength * 24, GameTime.timeMinDaylength, GameTime.timeMaxDaylength);
        maxX = (float)GlobalFunc.ValueFromProportion(proportion, GlobalVar.sunHeightAtNoonMin, GlobalVar.sunHeightAtNoonMax);
    }
    private void BeforePlayer()
    {
        sunLight.enabled = true;
        Quaternion direction = Quaternion.Euler(50, 64, 0);
        transform.rotation = direction;
        RenderSettings.ambientLight = ambientDayLight;
    }
    private void ItIsNight()
    {
        sunLight.enabled = false;
        // park sun in the underworld
        Quaternion direction = Quaternion.Euler(270, 0, 0);
        transform.rotation = direction;
        RenderSettings.ambientLight = ambientNightLight;
        RenderSettings.fogColor = fogColorNight;
    }

    private void ItIsDawn(float timeOfDay)
    {
        sunLight.enabled = true;
        float x = -10 * (dawnEnd - timeOfDay) / twilightLength;
        float y = 360 * timeOfDay;
        Quaternion direction = Quaternion.Euler(x, y, 0);
        transform.rotation = direction;
        RenderSettings.ambientLight = ambientNightLight + ((timeOfDay - dawnStart) / twilightLength * (ambientDayLight - ambientNightLight));
        RenderSettings.fogColor = fogColorNight + ((timeOfDay - dawnStart) / twilightLength * (fogColorDay - fogColorNight));
    }

    private void ItIsDay(float timeOfDay)
    {
        sunLight.enabled = true;
        //using nonlinear curve 7 to approximate a half sinus
        float x = (float)NonLinearCurves.GetInterimDouble0_1(7, 1 - (Mathf.Abs(timeOfDay - 0.5f) * 2 / dayLength)) * maxX;
        float y = 360 * timeOfDay;
        Quaternion direction = Quaternion.Euler(x, y, 0);
        transform.rotation = direction;
        RenderSettings.ambientLight = ambientDayLight;
        RenderSettings.fogColor = fogColorDay;
    }

    private void ItIsDusk(float timeOfDay)
    {
        sunLight.enabled = true;
        float x = -10 * (timeOfDay - duskStart) / twilightLength;
        float y = 360 * timeOfDay;
        Quaternion direction = Quaternion.Euler(x, y, 0);
        transform.rotation = direction;
        RenderSettings.ambientLight = ambientNightLight + ((duskEnd - timeOfDay) / twilightLength * (ambientDayLight - ambientNightLight));
        RenderSettings.fogColor = RenderSettings.ambientLight;
    }
}
