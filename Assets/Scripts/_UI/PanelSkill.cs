/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;
using System;

public class PanelSkill : MonoBehaviour
{
    public Text explanationPanel;
    public UICharacterCreation characterCreation;
    private Skills.SkillInfo skillInfo;
    private int skillId;
    public void OnPointerEnter()
    {
        explanationPanel.text = string.Format("<b>{0}</b>" + Environment.NewLine + Environment.NewLine
                + "<i>{1}</i>" + Environment.NewLine + Environment.NewLine
                + "{2}" + Environment.NewLine + Environment.NewLine
                + "<b>Group: </b>{3}"
                , skillInfo.name
                , skillInfo.headline
                , skillInfo.description
                , skillInfo.group);
    }
    public void Id(int id)
    {
        skillId = id;
        skillInfo = Skills.info[skillId];
        transform.Find("TextSkill").GetComponent<Text>().text = skillInfo.name + ":";
    }
    public void SetValue(int value)
    {
        transform.Find("Slider").GetComponent<Slider>().value = GlobalFunc.KeepInRange(value, 0, 3);
    }
    public void ValueChanged()
    {
        characterCreation.SkillChanged(skillId, (int)transform.Find("Slider").GetComponent<Slider>().value);
    }
}