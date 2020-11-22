/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEngine.EventSystems;

public class UINpcTradingPanel : MonoBehaviour, IPointerEnterHandler
{
    public UINpcTrading uiNpcTrading;
    private float mouseEnterNextTime;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Time.time > mouseEnterNextTime)
        {
            mouseEnterNextTime = Time.time + GlobalVar.panelUpdateDelay;
            uiNpcTrading.MouseEnterPanel();
        }
    }
}
