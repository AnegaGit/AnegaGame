/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;
public class PanelAttribute : MonoBehaviour
{
    public Text explanationPanel;
    public string explanation = "";
    public UICharacterCreation characterCreation;
    private string attributeName;
    public void OnPointerEnter()
    {
        explanationPanel.text = explanation;
    }
    public void Name(string name)
    {
        attributeName = name;
        transform.Find("TextAttribute").GetComponent<Text>().text = name + ":";
    }
    public void SetValue(int value)
    {
        value =GlobalFunc.KeepInRange(value, 0, 20);
        transform.Find("Slider").GetComponent<Slider>().value = value;
        transform.Find("TextValue").GetComponent<Text>().text = value.ToString();
    }
    public void ValueChanged()
    {
        int value = (int)transform.Find("Slider").GetComponent<Slider>().value;
        characterCreation.AttributeChanged(attributeName, value);
        transform.Find("TextValue").GetComponent<Text>().text = value.ToString();
    }
}