/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;
public class PanelAbility : MonoBehaviour
{
    public Text explanationPanel;
    public string explanation="";
    public UICharacterCreation characterCreation;
    private string abilityName;
    public void OnPointerEnter()
    {
        explanationPanel.text = explanation;
    }
    public void Name(string name)
    {
        abilityName = name;
        transform.Find("TextAbility").GetComponent<Text>().text = name + ":";
    }
    public void SetValue(int value)
    {
        transform.Find("Slider").GetComponent<Slider>().value = GlobalFunc.KeepInRange(value, 0, 3);
    }
    public void ValueChanged()
    {
        characterCreation.AbilityChanged(abilityName, (int)transform.Find("Slider").GetComponent<Slider>().value);
    }
}