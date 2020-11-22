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
public partial class UINpcGuildManagement : MonoBehaviour
{
    public GameObject panel;
    public Text createPriceText;
    public InputField createNameInput;
    public Button createButton;
    public Button terminateButton;
    void Update()
    {
        Player player = Player.localPlayer;
        // use collider point(s) to also work with big entities
        if (player != null &&
            player.target != null && player.target is Npc &&
            Utils.ClosestDistance(player.collider, player.target.collider) <= player.interactionRange)
        {
            createNameInput.interactable = !player.InGuild() &&
                                          Money.AvailableMoney(player) >= Guild.CreationPrice;
            createNameInput.characterLimit = Guild.NameMaxLength;
            createPriceText.text = Guild.CreationPrice.ToString();
            createButton.interactable = !player.InGuild() && Guild.IsValidGuildName(createNameInput.text);
            createButton.onClick.SetListener(() => {
                player.CmdCreateGuild(createNameInput.text);
                createNameInput.text = ""; // clear the input afterwards
            });
            terminateButton.interactable = player.guild.CanTerminate(player.name);
            terminateButton.onClick.SetListener(() => {
                player.CmdTerminateGuild();
            });
        }
        else panel.SetActive(false);
    }
}
