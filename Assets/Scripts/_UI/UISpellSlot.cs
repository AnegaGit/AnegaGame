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
public class UISpellSlot : MonoBehaviour
{
    public UIShowToolTip tooltip;
    public UIDragAndDropable dragAndDropable;
    public Image image;
    public Button button;
    public GameObject cooldownOverlay;
    public Text cooldownText;
    public Image cooldownCircle;
    public Text nameText;
    public int playerSpellNo;

    public void OnClickButton()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            // try use the spell or walk closer if needed
            player.TryUseSpell(playerSpellNo);
        }
    }

    private void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            Spell spell = player.spells[playerSpellNo];
            // cooldown overlay
            float cooldown = spell.CooldownRemaining();
            cooldownOverlay.SetActive(cooldown > 0);
            cooldownText.text = cooldown.ToString("F0");
            cooldownCircle.fillAmount = spell.Cooldown(player) > 0 ? cooldown / spell.Cooldown(player) : 0;

            // click event possible
            button.interactable = player.CastCheckSelf(spell); // checks mana, cooldown etc.
        }
    }
}
