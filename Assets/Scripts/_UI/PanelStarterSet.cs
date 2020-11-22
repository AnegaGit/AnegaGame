/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;
public class PanelStarterSet : MonoBehaviour
{
    public Text explanationPanel;
    public string explanation = "";
    public UICharacterCreation characterCreation;
    public int id;
    private string starterSetName;
    private bool externSet = false;
    public void OnPointerEnter()
    {
        explanationPanel.text = explanation;
    }
    public void Name(string name)
    {
        starterSetName = name;
        transform.Find("TextStarterSet").GetComponent<Text>().text = name + ":";
    }
    public void SetValue(int value)
    {
        if (transform.Find("Slider").GetComponent<Slider>().value != value)
        {
            externSet = true;
            transform.Find("Slider").GetComponent<Slider>().value = GlobalFunc.KeepInRange(value, 0, 1);
        }
    }
    public void ValueChanged()
    {
        if (!externSet)
        {
            if (transform.Find("Slider").GetComponent<Slider>().value == 0)
            {
                externSet = true;
                transform.Find("Slider").GetComponent<Slider>().value = 1;
            }
            else
                characterCreation.StarterSetChanged(id);
        }
        else
        {
            externSet = false;
        }
    }
}
