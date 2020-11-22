/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;
public class PanelSkillInfo : MonoBehaviour
{
    public UICharacterInfo characterInfo;
    public GameObject buttonMinus;
    public Text skillName;
    public Text skillLevelText;
    public Slider skillLevel;
    public Image skillLevelBar;
    public Slider skillProgress;
    public Player player;
    private int _skillId = 0;
    public int skillId
    {
        set
        {
            _skillId = value;
            skillName.text = Skills.info[_skillId].name;
            if (value == 0)
                buttonMinus.SetActive(false);
            else
                buttonMinus.SetActive(true);
            UpdateValues();
        }
    }

    private void UpdateValues()
    {
        if (player)
        {
            int level = player.skills.LevelOfId(_skillId);
            skillLevel.value = level;
            skillLevelText.text = GlobalFunc.FirstToUpper( GlobalFunc.ExamineLimitText(level, GlobalVar.skillLevelText));
            skillLevelBar.color = Color.Lerp(Color.blue, Color.green, level / 100f);
            skillProgress.value = player.skills.ExperienceOfId(_skillId) % GlobalVar.skillExperiencePerLevel;
            if (player.CanSkill(_skillId))
                skillName.color = GlobalVar.colorText;
            else
                skillName.color = GlobalVar.colorTextBad;
        }
    }

    private void Update()
    {
        UpdateValues();
    }

    public void onClickButtoMinus()
    {
        if (Input.GetKey(PlayerPreferences.keyReleaseAction))
        {
            characterInfo.ReduceSkill(_skillId);
        }
        else
        {
            player.Inform(string.Format("You try to reduce your skill {0}. Push the release button F2 while pressing the button for this action.", Skills.info[_skillId].name));
        }
    }
}

