/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;
public class PanelAttributeInfo : MonoBehaviour
{
    public UICharacterInfo characterInfo;
    public UICharacterExamination characterExamination;
    public GameObject buttonMinus;
    public GameObject buttonPlus;
    public Text attributeName;
    public Text valueText;
    public int attributeValue
    {
        set
        {
            valueText.text = value.ToString();
            if (value == 0)
                buttonMinus.SetActive(false);
            else if (value == GlobalVar.attributeMax)
                buttonPlus.SetActive(false);
        }
    }
    public string tooltip
    {
        set
        {
            this.GetComponent<UIShowToolTip>().text = value;
        }
    }
    public void onClickButtoPlus()
    {
        if (characterExamination)
        {
            characterExamination.ChangeAttribute(attributeName.text, 1);
        }
        else
        {
            characterInfo.ChangeAttribute(attributeName.text, 1);
        }
    }
    public void onClickButtoMinus()
    {
        if (characterExamination)
        {
            characterExamination.ChangeAttribute(attributeName.text, -1);
        }
        else
        {
            characterInfo.ChangeAttribute(attributeName.text, -1);
        }
    }
}
