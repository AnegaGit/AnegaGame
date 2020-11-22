/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;
public class PanelSkillControl : MonoBehaviour
{
    public UICharacterExamination characterExamination;
    public InputField experienceInput;
    public Text skillName;
    public Player targetPlayer;
    private int _skillId = 0;
    private int lastValue = 0;
    public int skillId
    {
        set
        {
            _skillId = value;
            skillName.text = Skills.info[_skillId].name;
            UpdateValues();
        }
    }

    private void UpdateValues()
    {
        if (targetPlayer)
        {
            if (lastValue != targetPlayer.skills.ExperienceOfId(_skillId))
            {
                lastValue = targetPlayer.skills.ExperienceOfId(_skillId);
                experienceInput.text = string.Format("<b>{0}</b>.{1:####}", targetPlayer.skills.LevelOfId(_skillId), lastValue % GlobalVar.skillExperiencePerLevel);
            }
        }
    }

    private void Update()
    {
        UpdateValues();
    }

    public void onChangeExperience()
    {
        Player player = Player.localPlayer;
        if (GameMaster.changeSkills(player.gmState))
        {
            if (float.TryParse(experienceInput.text, out float newValue))
            {
                int exp = (int)((newValue - (int)newValue) * GlobalVar.skillExperiencePerLevel);
                int level = Mathf.Clamp((int)newValue, 0, 100);
                characterExamination.ChangeSkill(_skillId, level * GlobalVar.skillExperiencePerLevel + exp);
            }
            else
            {
                lastValue = 0;
            }
        }
        else
        {
            lastValue = 0;
        }
    }
}

