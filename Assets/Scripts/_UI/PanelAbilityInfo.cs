/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;
public class PanelAbilityInfo : MonoBehaviour
{
    public UICharacterInfo characterInfo;
    public UICharacterExamination characterExamination;
    public GameObject buttonMinus;
    public GameObject buttonPlus;
    public Text abilityeName;
    public Text valueText;
    public int abilityValue
    {
        set
        {
            switch (value)
            {
                case 0:
                    valueText.text = "none";
                    buttonMinus.SetActive(false);
                    break;
                case 1:
                    valueText.text = "poor";
                    break;
                case 2:
                    valueText.text = "good";
                    break;
                case 3:
                    valueText.text = "excellent";
                    buttonPlus.SetActive(false);
                    break;
            }
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
        if (characterInfo)
            characterInfo.ChangeAbility(abilityeName.text, true);
        if (characterExamination)
            characterExamination.ChangeAbility(abilityeName.text, true);
    }
    public void onClickButtoMinus()
    {
        if (characterInfo)
            characterInfo.ChangeAbility(abilityeName.text, false);
        if (characterExamination)
            characterExamination.ChangeAbility(abilityeName.text, false);
    }
}
