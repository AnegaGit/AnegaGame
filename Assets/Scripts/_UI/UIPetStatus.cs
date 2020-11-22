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
public class UIPetStatus : MonoBehaviour
{
    public GameObject panel;
    public Slider healthSlider;
    public Text nameText;
    public Button autoAttackButton;
    public Button defendOwnerButton;
    public Button unsummonButton;
    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null && player.activePet != null)
        {
            Pet pet = player.activePet;
            panel.SetActive(true);
            healthSlider.value = pet.HealthPercent();
            healthSlider.GetComponent<UIShowToolTip>().text = "Health: " + pet.health + " / " + pet.healthMax;
            nameText.text = pet.name;
            autoAttackButton.GetComponentInChildren<Text>().fontStyle = pet.autoAttack ? FontStyle.Bold : FontStyle.Normal;
            autoAttackButton.onClick.SetListener(() => {
                player.CmdPetSetAutoAttack(!pet.autoAttack);
            });
            defendOwnerButton.GetComponentInChildren<Text>().fontStyle = pet.defendOwner ? FontStyle.Bold : FontStyle.Normal;
            defendOwnerButton.onClick.SetListener(() => {
                player.CmdPetSetDefendOwner(!pet.defendOwner);
            });
            //unsummonButton.interactable = player.CanUnsummonPet(); <- looks too annoying if button flashes rapidly
            unsummonButton.onClick.SetListener(() => {
                if (player.CanUnsummonPet()) player.CmdPetUnsummon();
            });
        }
        else panel.SetActive(false);
    }
}
