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
public partial class UICastBar : MonoBehaviour
{
    public GameObject panel;
    public Slider slider;
    public Text spellNameText;
    public Text progressText;
    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null &&
            player.state == GlobalVar.stateCasting && player.currentSpell != -1 &&
            player.spells[player.currentSpell].showCastBar)
        {
            panel.SetActive(true);
            Spell spell = player.spells[player.currentSpell];
            float ratio = (spell.CastTime(player) - spell.CastTimeRemaining()) / spell.CastTime(player);
            slider.value = ratio;
            spellNameText.text = spell.name;
            progressText.text = spell.CastTimeRemaining().ToString("F1") + "s";
        }
        else panel.SetActive(false);
    }
}
