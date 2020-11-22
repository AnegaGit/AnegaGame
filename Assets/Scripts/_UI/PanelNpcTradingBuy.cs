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

public class PanelNpcTradingBuy : MonoBehaviour
{
    UINpcTrading uiNpcTrading;
    public Image image;
    public Text itemName;
    public Text priceText;

    public void Initialize(UINpcTrading uiNpcTrading, string name, Sprite icon, long price)
    {
        image.sprite = icon;
        itemName.text = name;
        priceText.text = Money.MoneyShortText(price);
    }
}
