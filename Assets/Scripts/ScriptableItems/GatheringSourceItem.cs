/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Anega/Item/Gathering Source", order = 201)]
public class GatheringSourceItem : UsableItem
{
    [Header("Gathering")]
    //data1: current phase
    //data2: utc seconds when next phase
    //data3:
    public float freeRange;
    public GatheringSeedRegion[] seedActions;
    public GatheringLifePhase[] lifePhases;

    public bool changeInvisibleOnly;

    // usage
    // can it be used as element
    public override bool CanUse(Player player, ElementSlot element)
    {
        // only if there is any chance to get anything from this item
        if (lifePhases[element.item.data1].itemsInResource > 0)
        {
            if (player.skills.LevelOfSkill(lifePhases[element.item.data1].gatheringSkill) >= lifePhases[element.item.data1].bestSkillLevel - GlobalVar.gatheringItemsInResourceFitBestAt)
                return true;
            else
                return false;
        }
        else
            return false;
    }
    // can it be picked into inventory
    public override bool CanPicked(ElementSlot element)
    {
        return false;
    }
    // can it be used as inventory item
    public override bool CanUse(Player player, ItemSlot itemSlot)
    {
        return false;
    }
    // can we equip this item into this specific equipment slot?
    public override bool CanEquip(Player player, int equipmentIndex)
    {
        return false;
    }

    public override void Use(Player player, ElementSlot elementSlot)
    {
        // player should be idle to work here
        if (player.state == GlobalVar.stateIdle || player.state == GlobalVar.stateMoving)
        {
            // verify if the player has a tool
            if (player.GatheringToolEquipped(out GatheringToolItem toolItem))
            {
                // search first right hand
                if (player.inventory.GetEquipment(GlobalVar.equipmentRightHand, out ItemSlot itemSlot))
                {
                    if (itemSlot.item.data is GatheringToolItem)
                    {
                        player.UseInventoryItem(GlobalVar.containerEquipment, GlobalVar.equipmentRightHand);
                    }
                }
                else
                {
                    player.UseInventoryItem(GlobalVar.containerEquipment, GlobalVar.equipmentLeftHand);
                }
            }
        }
    }
}

// struct seed actiona
// used for Item
[Serializable]
public struct GatheringSeedRegion
{
    public GlobalVar.VegetationType VegetationType;
    public int frequency;
    public float sizeMin;
    public float sizeMax;
    public GatheringSeedRule[] rules;
}
// used for creator
[Serializable]
public struct GatheringSeedItem
{
    public string itemName;
    public int frequency;
    public float sizeMin;
    public float sizeMax;
    public float daysInPhase0;
    public List <GatheringSeedRule> rules;
}
// used for item and creator
[Serializable]
public struct GatheringSeedRule
{
    public GatheringSourceItemList relatedItems;
    public float minDistance;
    public float maxDistance;
    public float minAmount;
    public float maxAmount;
}

// struct live phases
[Serializable]
public struct GatheringLifePhase
{
    public Skills.Skill gatheringSkill;
    public int modelPhase;
    public int bestSkillLevel;
    public float itemsInResource;
    [Range(0f, 1f)] public float voidTriesMin;
    [Range(0f, 1f)] public float voidTriesMax;
    public GatheringContent[] content;
    [Range(0f, 0.1f)] public float probabilityExtraStuff;
    [Range(0f, 1f)] public float nextPhaseUsedDefaultProbability;
    public int nextPhaseUsedDefault;
    public int nextPhaseUsedSpecial;
    public GameObject onUseEffect;
    [Tooltip("0 = never\n1 = always\n0.9 = 10 time in average\n0.997 = 333 in average")]
    [Range(0f, 1f)] public float nextPhaseTimeoutDefaultProbability;
    public int nextPhaseTimeoutDefault;
    public int nextPhaseTimeoutSpecial;
    public float dayInPhase;
}

// struct findable items
[Serializable]
public struct GatheringContent
{
    public ScriptableItem item;
    public int amount;
    public float probability;
    public string infoText;
    public int durability;
    public int quality;

    public GatheringContent(ScriptableItem item, int amount, float probability, int durability, int quality, string infoText = "")
    {
        this.item = item;
        this.amount = amount;
        this.probability = probability;
        this.durability = durability;
        this.quality = quality;
        this.infoText = infoText;
    }
}



