/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// The Npc class is rather simple. It contains state Update functions that do
// nothing at the moment, because Npcs are supposed to stand around all day.
//
// Npcs first show the welcome text and then have options for item trading and
// quests.
using UnityEngine;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UMA;
using UMA.CharacterSystem;

[RequireComponent(typeof(NetworkNavMeshAgent))]
public partial class Npc : Entity
{
    [Header("Text Meshes")]
    public TextMesh questOverlay;
    [Header("Welcome Text")]
    [TextArea(1, 30)] public string welcome;
    [Header("NPC Trade")]
    public bool buyAllSellItems = false;
    public List<BuyItems> buyItems;
    public List<SellItems> sellItems;
    [Header("Quests")]
    public ScriptableQuest[] quests;
    [Header("Teleportation")]
    public Transform teleportTo;
    [Header("Guild Management")]
    public bool offersGuildManagement = true;
    [Header("Summonables")]
    public bool offersSummonableRevive = true;
    [Header("Apperance")]
    public RaceSpecification raceSpecification;
    // invisible UMA content
    private DynamicCharacterAvatar UMAAvatar;
    // UMA exists
    bool isUMAExists = false;

    // networkbehaviour ////////////////////////////////////////////////////////
    public override void OnStartServer()
    {
        base.OnStartServer();
        // all npcs should spawn with full health and mana
        health = healthMax;
        mana = manaMax;
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (nameOverlay != null)
            nameOverlay.color = PlayerPreferences.nameOverlayNpcColor;

        // apply apperance
        UMAAvatar = GetComponent<DynamicCharacterAvatar>();
        if (UMAAvatar && !isUMAExists)
        {
            foreach (StringFloat raceDef in raceSpecification.definitions)
            {
                UMAAvatar.predefinedDNA.AddDNA(raceDef.text, raceDef.value);
            }
            isUMAExists = true;
        }
    }
    // finite state machine states /////////////////////////////////////////////
    [Server] protected override int UpdateServer() { return state; }
    [Client] protected override void UpdateClient() { }
    // overlays ////////////////////////////////////////////////////////////////
    protected override void UpdateOverlays()
    {
        base.UpdateOverlays();
        if (questOverlay != null)
        {
            // find local player (null while in character selection)
            if (Player.localPlayer != null)
            {
                if (quests.Any(q => Player.localPlayer.CanCompleteQuest(q.name)))
                    questOverlay.text = "!";
                else if (quests.Any(Player.localPlayer.CanAcceptQuest))
                    questOverlay.text = "?";
                else
                    questOverlay.text = "";
            }
        }
    }
    // spells //////////////////////////////////////////////////////////////////
    public override bool HasCastWeapon() { return true; }
    public override bool CanAttack(Entity entity) { return false; }
    // quests //////////////////////////////////////////////////////////////////
    // helper function to filter the quests that are shown for a player
    // -> all quests that:
    //    - can be started by the player
    //    - or were already started but aren't completed yet
    public List<ScriptableQuest> QuestsVisibleFor(Player player)
    {
        return quests.Where(q => player.CanAcceptQuest(q) ||
                                 player.HasActiveQuest(q.name)).ToList();
    }
}

// NPC trade //////////////////////////////////////////////////////////////////
[Serializable]
public struct BuyItems
{
    public string headline;
    public float priceLevel;
    public List<ScriptableItem> items;
}

[Serializable]
public struct SellItem
{
    public ScriptableItem item;
    public int quality;
    public int durability;
    public float priceLevel;
}

[Serializable]
public struct SellItems
{
    public string headline;
    public float priceLevel;
    public List<SellItem> items;
}