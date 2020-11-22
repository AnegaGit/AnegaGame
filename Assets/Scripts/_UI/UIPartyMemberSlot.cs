/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Attach to the prefab for easier component access by the UI Scripts.
// Otherwise we would need slot.GetChild(0).GetComponentInChildren<Text> etc.
using UnityEngine;
using UnityEngine.UI;
public class UIPartyMemberSlot : MonoBehaviour
{
    public Image icon;
    public Text nameText;
    public Text masterIndicatorText;
    public Text levelText;
    public Text guildText;
    public Button actionButton;
    public Slider healthSlider;
    public Slider manaSlider;
}
