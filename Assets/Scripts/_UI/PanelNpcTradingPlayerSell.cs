/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelNpcTradingPlayerSell : MonoBehaviour
{
    UINpcTrading uiNpcTrading;
    public Image image;
    public Text itemName;
    public Text priceText;
    public Image barDurability;
    public Image barQuality;
    public Button buttonSellItem;

    ItemSlot itemSlot;
    long price;

    public void Initialize(UINpcTrading uiNpcTrading, ItemSlot itemSlot, long basePrice)
    {
        image.sprite = itemSlot.item.data.image;
        itemName.text = (itemSlot.amount > 1 ? itemSlot.amount.ToString() + " x " : "") + itemSlot.item.data.displayName;
        this.itemSlot = itemSlot;
        price = Money.AdaptToDurabilityAndQuality(basePrice,itemSlot.item.durability,itemSlot.item.quality);
        priceText.text = Money.MoneyShortText(price);
        barDurability.rectTransform.sizeDelta = new Vector2(Mathf.Floor(itemSlot.item.durability / 15f) / 6 * 22, 6);
        barQuality.rectTransform.sizeDelta = new Vector2(Mathf.Floor(itemSlot.item.quality / 15f) / 6 * 22, 6);
        buttonSellItem.gameObject.SetActive(true);
    }

    public void SellItem()
    {
        Player player = Player.localPlayer;
        if (Input.GetKey(PlayerPreferences.keyReleaseAction))
        {
            player.CmdNpcSellItem(itemSlot.container,itemSlot.slot, price);
            player.Inform(string.Format("You sold {0} for {1}.", itemName.text, Money.MoneyText(price)));
            buttonSellItem.gameObject.SetActive(false);
        }
        else
        {
            player.Inform(string.Format("To sell the item {0} for {1} hold the relase key.", itemName.text, Money.MoneyText(price)));
        }
    }
}