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
using UnityEngine.EventSystems;
public class UIImageMouseoverColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image image;
    public Color highlightColor = Color.white;
    Color defaultColor;
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        defaultColor = image.color;
        image.color = highlightColor;
    }
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        image.color = defaultColor;
    }
}