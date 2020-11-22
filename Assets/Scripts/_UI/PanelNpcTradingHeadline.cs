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

public class PanelNpcTradingHeadline : MonoBehaviour
{
    UINpcTrading uiNpcTrading;
    int register;
    int index;
    bool isCollapsed;
    public Text headlineText;
    public Button buttonExpand;
    public Image buttonImage;
    public Sprite iconExpand;
    public Sprite iconCollaps;

    public void Initialize(int index, int register, string headline, UINpcTrading uiNpcTrading, bool isCollapsed)
    {
        this.index = index;
        this.register = register;
        headlineText.text = headline;
        this.uiNpcTrading = uiNpcTrading;
        if (index < 0)
            buttonExpand.gameObject.SetActive(false);
        this.isCollapsed = isCollapsed;
        if (isCollapsed)
            buttonImage.sprite = iconExpand;
        else
            buttonImage.sprite = iconCollaps;
    }

    public void ExpandButtonClicked()
    {
        switch (register)
        {
            case 2:
                uiNpcTrading.ExpandBuyGroup(index, isCollapsed);
                break;
            case 3:
                uiNpcTrading.ExpandPlayerSellGroup(index, isCollapsed);
                break;
            default:
                uiNpcTrading.ExpandSellGroup(index, isCollapsed);
                break;
        }
    }
}
