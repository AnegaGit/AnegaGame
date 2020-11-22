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
public partial class UINpcRevive : MonoBehaviour
{
    public static UINpcRevive singleton;
    public GameObject panel;
    public UIDragAndDropable itemSlot;
    public Text costsText;
    public Button reviveButton;
    [HideInInspector] public int itemIndex = -1;
    public UINpcRevive() { singleton = this; }
    void Update()
    {
        Player player = Player.localPlayer;
        // use collider point(s) to also work with big entities
        if (player != null &&
            player.target != null && player.target is Npc &&
            Utils.ClosestDistance(player.collider, player.target.collider) <= player.interactionRange)
        {
            Npc npc = (Npc)player.target;
            // revive
            if (itemIndex != -1 && itemIndex < player.inventory.Count &&
                player.inventory[itemIndex].amount > 0 &&
                player.inventory[itemIndex].item.data is SummonableItem)
            {
                ItemSlot slot = player.inventory[itemIndex];
                SummonableItem itemData = (SummonableItem)slot.item.data;
                itemSlot.GetComponent<Image>().color = Color.white;
                itemSlot.GetComponent<Image>().sprite = slot.item.image;
                itemSlot.GetComponent<UIShowToolTip>().enabled = true;
                itemSlot.GetComponent<UIShowToolTip>().text = slot.ToolTip();
                itemSlot.dragable = true;
                costsText.text = itemData.revivePrice.ToString();
                reviveButton.interactable = slot.item.data1 == 0 && Money.AvailableMoney(player) >= itemData.revivePrice;
                reviveButton.onClick.SetListener(() =>
                {
                    player.CmdNpcReviveSummonable(itemIndex);
                    itemIndex = -1;
                });
            }
            else
            {
                itemSlot.GetComponent<Image>().color = Color.clear;
                itemSlot.GetComponent<Image>().sprite = null;
                itemSlot.GetComponent<UIShowToolTip>().enabled = false;
                itemSlot.dragable = false;
                costsText.text = "0";
                reviveButton.interactable = false;
            }
        }
        else panel.SetActive(false);
    }
}
