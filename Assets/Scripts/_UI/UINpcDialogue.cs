/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public partial class UINpcDialogue : MonoBehaviour
{
    public static UINpcDialogue singleton;
    public GameObject panel;
    public Text welcomeText;
    public Button tradingButton;
    public Button teleportButton;
    public Button questsButton;
    public Button guildButton;
    public Button reviveButton;
    public GameObject npcTradingPanel;
    public GameObject npcQuestPanel;
    public GameObject npcGuildPanel;
    public GameObject npcRevivePanel;
    public GameObject inventoryPanel;
    public UINpcDialogue() { singleton = this; }
    void Update()
    {
        Player player = Player.localPlayer;
        // use collider point(s) to also work with big entities
        if (player != null &&
            panel.activeSelf &&
            player.target != null && player.target is Npc &&
            Utils.ClosestDistance(player.collider, player.target.collider) <= player.interactionRange)
        {
            Npc npc = (Npc)player.target;
            // welcome text
            welcomeText.text = npc.welcome;
            // trading button
            tradingButton.gameObject.SetActive(npc.buyItems.Count > 0 || npc.sellItems.Count>0);
            tradingButton.onClick.SetListener(() => {
                npcTradingPanel.SetActive(true);
                panel.SetActive(false);
            });
            // teleport button
            teleportButton.gameObject.SetActive(npc.teleportTo != null);
            if (npc.teleportTo != null)
                teleportButton.GetComponentInChildren<Text>().text = "Teleport: " + npc.teleportTo.name;
            teleportButton.onClick.SetListener(() => {
                player.CmdNpcTeleport();
            });
            // filter out the quests that are available for the player
            List<ScriptableQuest> questsAvailable = npc.QuestsVisibleFor(player);
            questsButton.gameObject.SetActive(questsAvailable.Count > 0);
            questsButton.onClick.SetListener(() => {
                npcQuestPanel.SetActive(true);
                panel.SetActive(false);
            });
            // guild
            guildButton.gameObject.SetActive(npc.offersGuildManagement);
            guildButton.onClick.SetListener(() => {
                npcGuildPanel.SetActive(true);
                panel.SetActive(false);
            });
            // summonable revive
            reviveButton.gameObject.SetActive(npc.offersSummonableRevive);
            reviveButton.onClick.SetListener(() => {
                npcRevivePanel.SetActive(true);
                inventoryPanel.SetActive(true); // better feedback
                panel.SetActive(false);
            });
        }
        else panel.SetActive(false);
    }
    public void Show() { panel.SetActive(true); }
}
