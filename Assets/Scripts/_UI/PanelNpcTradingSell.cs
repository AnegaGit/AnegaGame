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

public class PanelNpcTradingSell : MonoBehaviour
{
    UINpcTrading uiNpcTrading;
    private string itemName;
    public Image image;
    public Text itemDisplayName;
    public Text priceText;
    public Image barDurability;
    public Image barQuality;

    int quality;
        int durability;
    long price;

    public void Initialize(UINpcTrading uiNpcTrading,string name,  string displayName, Sprite icon, long price, int quality, int durability,string toolTip="")
    {
        image.sprite = icon;
        itemName = name;
        itemDisplayName.text = displayName;
        this.price = price;
        priceText.text = Money.MoneyShortText(price);
        this.durability = durability;
        barDurability.rectTransform.sizeDelta = new Vector2(Mathf.Floor(durability / 15f) / 6 * 22, 6);
        this.quality = quality;
        barQuality.rectTransform.sizeDelta = new Vector2(Mathf.Floor(quality / 15f) / 6 * 22, 6);
        this.GetComponent<UIShowToolTip>().text = toolTip;
    }

    public void SellItem()
    {
        Player player = Player.localPlayer;
        if (Input.GetKey(PlayerPreferences.keyReleaseAction))
        {
            if (Money.AvailableMoney(player) >= price)
            {
                player.CmdNpcBuyItem(itemName, durability, quality, price);
                player.Inform(string.Format("You bought {0} for {1}.", itemDisplayName.text, Money.MoneyText(price)));
            }
            else
            {
                player.Inform(string.Format("Maybe you should make some more money. Otherwise, you can not afford the item {0} for {1}.", itemDisplayName.text, Money.MoneyText(price)));
            }
        }
        else
        {
            player.Inform(string.Format("To buy the item {0} for {1} hold the relase key.", itemDisplayName.text, Money.MoneyText(price)));
        }
    }
}