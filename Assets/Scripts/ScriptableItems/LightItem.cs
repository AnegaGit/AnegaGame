/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Text;
using UnityEngine;
[CreateAssetMenu(menuName = "Anega/Item/Light", order = 903)]
public class LightItem : UsableItem
{
    [Header("Light")]
    public int maxLightSeconds;
    public bool canExtinguished;
    public bool multipleUse;
    public string extinguishFailureMessage;
    //data1: remainingLightSeconds
    //data2: isLightOn
    //data3:
    bool isLightOn = false;
    // remainingLightSeconds==0 is never lit
    // make sure a used light cannot become 0!
    int remainingLightSeconds = GlobalVar.lightNeverLit;

    // usage
    // can we pick it
    public override bool CanPicked(ElementSlot element)
    {
        ReadDynamicData(element);
        return !isLightOn;
    }
    // can it be used as element
    public override bool CanUse(Player player, ElementSlot element)
    {
        if (element.amount == 1)
            return usableAsElement;
        return false;
    }
    // can it be used from Inventory
    public override bool CanUse(Player player, ItemSlot itemSlot)
    {
        return GlobalFunc.IsInHand(itemSlot.container, itemSlot.slot) && itemSlot.amount == 1;
    }
    // can we equip this item into this specific equipment slot?
    public override bool CanEquip(Player player, int equipmentIndex)
    {
        if (GlobalFunc.IsInBelt(equipmentIndex) || GlobalFunc.IsInHand(equipmentIndex))
            return true;
        return false;
    }
    // initialize server side
    public override void Initialize(ElementSlot element)
    {
        ReadDynamicData(element);
        if (isLightOn)
        {
            // burn down
            element.UseOverTime(GlobalVar.lightTimeAccuracy);
        }
        SaveRemainingSeconds(element, false);
        base.Initialize(element);
    }

    // server side use
    public override void Use(Player player, ElementSlot element)
    {
        //function almots twice but different connector
        ReadDynamicData(element);

        if (!isLightOn && (remainingLightSeconds > 0 || remainingLightSeconds == GlobalVar.lightNeverLit))
        {
            isLightOn = true;
            if (remainingLightSeconds == GlobalVar.lightNeverLit)
                remainingLightSeconds = maxLightSeconds;
            remainingLightSeconds = (int)Mathf.Max(1, remainingLightSeconds - GlobalVar.lightTimeAccuracy); // at least one cycle remains!
            element.UseOverTime(GlobalVar.lightTimeAccuracy);
            SaveRemainingSeconds(element);
        }
        else if (isLightOn && canExtinguished)
        {
            isLightOn = false;
            remainingLightSeconds = (int)Mathf.Max(1, maxLightSeconds - GlobalVar.lightTimeAccuracy); // at least one cycle remains!                
            SaveRemainingSeconds(element);
        }
        else if (isLightOn)
            player.Inform(extinguishFailureMessage);
    }
    public override void Use(Player player, int containerId, int slotIndex)
    {
        if (GlobalFunc.IsInHand(containerId, slotIndex))
        {
            //function almost twice but different connector
            ReadDynamicData(player, containerId, slotIndex);

            if (!isLightOn && (remainingLightSeconds > 0 || remainingLightSeconds == GlobalVar.lightNeverLit))
            {
                isLightOn = true;
                if (remainingLightSeconds == GlobalVar.lightNeverLit)
                    remainingLightSeconds = maxLightSeconds;
                remainingLightSeconds = (int)Mathf.Max(1, remainingLightSeconds - GlobalVar.lightTimeAccuracy); // at least one cycle remains!
                player.UseOverTime(containerId, slotIndex, GlobalVar.lightTimeAccuracy);
                SaveRemainingSeconds(player, containerId, slotIndex);
            }
            else if (isLightOn && canExtinguished)
            {
                isLightOn = false;
                remainingLightSeconds = (int)Mathf.Max(1, maxLightSeconds - GlobalVar.lightTimeAccuracy); // at least one cycle remains!                
                SaveRemainingSeconds(player, containerId, slotIndex);
            }
            else if (isLightOn)
                player.Inform(extinguishFailureMessage);
        }
        else
        {
            player.Inform("It would certainly look spectacular if you burn off your inventory. But you hold yourself back.");
        }
    }

    // server side repeated use
    public override void InUse(ElementSlot element)
    {
        ReadDynamicData(element);
        if (isLightOn)
        {
            remainingLightSeconds = remainingLightSeconds - (int)GlobalVar.lightTimeAccuracy;
            // a used light cannot have 0!
            if (remainingLightSeconds == 0)
                remainingLightSeconds--;
            if (remainingLightSeconds <= 0 && multipleUse)
            {
                isLightOn = false;
                SaveRemainingSeconds(element);
            }
            else if (remainingLightSeconds < 0)
            {
                Destroy(element.gameObject);
            }
            else
            {
                element.UseOverTime(GlobalVar.lightTimeAccuracy);
                SaveRemainingSeconds(element);
            }
        }
    }
    public override void InUse(Player player, int containerId, int slotIndex)
    {
        // just make sure it was not moved away
        if (GlobalFunc.IsInHand(containerId, slotIndex))
        {
            ReadDynamicData(player, containerId, slotIndex);
            if (isLightOn)
            {
                remainingLightSeconds = remainingLightSeconds - (int)GlobalVar.lightTimeAccuracy;
                // a used light cannot have 0!
                if (remainingLightSeconds == 0)
                    remainingLightSeconds--;
                if (remainingLightSeconds <= 0 && multipleUse)
                {
                    SaveRemainingSeconds(player, containerId, slotIndex);
                }
                else if (remainingLightSeconds < 0)
                {
                    player.inventory.Remove(containerId, slotIndex);
                }
                else
                {
                    player.UseOverTime(containerId, slotIndex, GlobalVar.lightTimeAccuracy);
                    SaveRemainingSeconds(player, containerId, slotIndex);
                }
            }
        }
    }

    public override void OnUseAction(ElementSlot element, int data1, int data2, int data3)
    {
        remainingLightSeconds = data1;
        isLightOn = (data2 == 1);
        LightElement lightElement = element.model.GetComponent<LightElement>();
        if (lightElement)
        {
            lightElement.SwitchDirect(isLightOn, (float)remainingLightSeconds / maxLightSeconds);
        }
    }


    // read dynamic data from slot
    private void ReadDynamicData(ElementSlot elementSlot)
    {
        remainingLightSeconds = elementSlot.item.data1;
        isLightOn = elementSlot.item.data2 == 1;
    }
    private void ReadDynamicData(Player player, int containerId, int slotIndex)
    {
        if (player.inventory.GetItemSlot(containerId, slotIndex, out ItemSlot itemSlot))
        {
            // just to make sure, maybe not necessary
            if (itemSlot.item.data is LightItem)
            {
                remainingLightSeconds = itemSlot.item.data1;
                isLightOn = itemSlot.item.data2 == 1;
            }
        }
    }

    // save changed data
    private void SaveRemainingSeconds(ElementSlot elementSlot, bool callClient = true)
    {
        // just to make sure, maybe not necessary
        if (elementSlot.item.data is LightItem)
        {
            elementSlot.SetItemData(remainingLightSeconds, (isLightOn ? 1 : 0));
            // inform clients except on initialization
            // give client time to initialize
            if (callClient)
                elementSlot.RpcUseAction(remainingLightSeconds, (isLightOn ? 1 : 0), 0);
        }
    }
    private void SaveRemainingSeconds(Player player, int containerId, int slotIndex)
    {
        if (player.inventory.GetItemSlot(containerId, slotIndex, out ItemSlot itemSlot))
        {
            // just to make sure, maybe not necessary
            if (itemSlot.item.data is LightItem)
            {
                itemSlot.item.data1 = remainingLightSeconds;
                itemSlot.item.data2 = (isLightOn ? 1 : 0);
                player.inventory.AddOrReplace(itemSlot);
            }
        }
    }

    public static string RemainingLightTime(int remainingSeconds, int maxSeconds)
    {
        if (remainingSeconds == GlobalVar.lightNeverLit)
            return "is new";
        else if (remainingSeconds < 0)
            return "is empty and cannot be lit";
        else
        {
            float relativeLightTime = (float)remainingSeconds / maxSeconds;
            return GlobalFunc.ExamineLimitText(relativeLightTime, GlobalVar.lightTime);
        }
    }
}
