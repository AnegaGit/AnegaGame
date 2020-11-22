/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;
public partial class UIHealthMana : MonoBehaviour
{
    public GameObject panelHealth;
    public GameObject panelWeight;
    public Slider healthSlider;
    public Slider manaSlider;
    public Slider staminaSlider;
    public Image staminaBar;
    public Slider weightSlider;
    public Image weightBar;
    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            panelHealth.SetActive(!player.webcamActive);
            panelWeight.SetActive(!player.webcamActive);
            healthSlider.value = player.HealthPercent();
            manaSlider.value = player.ManaPercent();
            if (player.stamina >= 0)
                staminaBar.color = PlayerPreferences.staminaBarPlus;
            else
                staminaBar.color = PlayerPreferences.staminaBarMinus;
            staminaSlider.value = player.StaminaPercent();
            float weightPercent = player.WeightPercent();
            if (player.handscale!=Abilities.Excellent)
            {
                int accuracy = GlobalVar.weightBarAccuracy[player.handscale];
                weightPercent = (float)((int)(weightPercent * accuracy)) / accuracy + (0.5f / accuracy);
            }
            weightSlider.value = weightPercent;
            if (weightPercent > PlayerPreferences.weightWarningLimit)
                weightBar.color = Color.Lerp(Color.gray, PlayerPreferences.weightWarningColor, (float)GlobalFunc.ProportionFromValue(weightPercent, PlayerPreferences.weightWarningLimit, 1));
            else
                weightBar.color = Color.gray;

        }
        else
        {
            panelHealth.SetActive(false);
            panelWeight.SetActive(false);
        }
    }
}
