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
public class UIPopup : MonoBehaviour
{
    public GameObject panel;
    public Text messageText;
    public void Show(string message)
    {
        // append error if visible, set otherwise. then show it.
        if (panel.activeSelf) messageText.text += ";\n" + message;
        else messageText.text = message;
        panel.SetActive(true);
    }
}
