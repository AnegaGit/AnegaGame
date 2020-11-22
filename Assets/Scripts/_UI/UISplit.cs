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
using UnityEngine.UI;

public class UISplit : MonoBehaviour
{
    public KeyCode hotKey = KeyCode.KeypadDivide;
    public GameObject panel;
    public Slider sliderAbsolute;
    public InputField inputAbsolute;
    bool isActive = false;
    bool blockChanges = false;
    Player player;


    void Update()
    {
        // hotkey (not while typing in chat, etc.)
        if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
        {
            panel.SetActive(!panel.activeSelf);
        }
        if (panel.activeSelf && isActive == false)
        {
            player = Player.localPlayer;
            ApplyDisplay();
            isActive = true;
        }
        if (!panel.activeSelf)
            isActive = false;
    }

    public void InputFieldChanged()
    {
        if (player && !blockChanges)
        {
            if (int.TryParse(inputAbsolute.text, out int newValue))
            {
                if (player)
                {
                    // max Split = 1000, Slider cannot handle more!
                    player.splitValue = Mathf.Clamp(newValue, 1, 1000);
                }
            }
            ApplyDisplay();
        }
    }

    public void SliderChanged()
    {
        if (player && !blockChanges)
        {
            if (sliderAbsolute.value <= 10)
                player.splitValue = (int)sliderAbsolute.value;
            else if (sliderAbsolute.value <= 19)
                player.splitValue = (int)(sliderAbsolute.value - 9) * 10;
            else
                player.splitValue = (int)(sliderAbsolute.value - 18) * 100;
            ApplyDisplay();
        }
    }

    void ApplyDisplay()
    {
        if (player)
        {
            blockChanges = true;
            inputAbsolute.text = player.splitValue.ToString();
            if (player.splitValue <= 10)
                sliderAbsolute.value = player.splitValue;
            else if (player.splitValue <= 100)
                sliderAbsolute.value = player.splitValue / 10 + 9;
            else
                sliderAbsolute.value = player.splitValue / 100 + 18;
            blockChanges = false;
        }
    }
}

