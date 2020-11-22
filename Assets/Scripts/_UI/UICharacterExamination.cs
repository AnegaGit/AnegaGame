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
using System;
public class UICharacterExamination : MonoBehaviour
{
    public GameObject panel;
    [Header("Stats")]
    public GameObject panelStats;
    public Button buttonStats;
    public InputField nameText;
    public Dropdown relationStat;
    public Text statText;
    [Header("Attributes")]
    public GameObject panelAttributes;
    public Button buttonAttributes;
    public GameObject listAttributes;
    public GameObject panelAttribute;
    public Text attributeBaseText;
    [Header("Abilities")]
    public GameObject panelAbilities;
    public Button buttonAbilities;
    public GameObject listAbilities;
    public GameObject panelAbilitiy;
    public Text abilityBaseText;
    [Header("Skills")]
    public GameObject panelSkills;
    public Button buttonSkills;
    public GameObject listSkills;
    public GameObject panelSkill;
    public Dropdown skillNameDropdown;
    [Header("GM")]
    public GameObject panelGM;
    public Button buttonGM;
    public Text GMText;
    public Text buttonStopText;
    public Slider GMHealth;
    public Slider GMMana;
    public Slider GMStamina;
    public GameObject buttonSliderUpdate;
    private Player targetPlayer;
    private Entity targetEntity;
    private Entity lastTarget;
    private bool isPlayer;
    private bool isInitialized = false;
    private bool isSliderUpdate = true;

    public void InitializePlayer(Player target)
    {
        if (panel.activeSelf)
            panel.SetActive(false);
        else
        {
            targetPlayer = target;
            targetEntity = null;
            lastTarget = target;
            isPlayer = true;
            InitializeView();
        }
    }
    public void InitializeEntity(Entity target)
    {
        if (panel.activeSelf)
            panel.SetActive(false);
        else
        {
            targetPlayer = null;
            targetEntity = target;
            lastTarget = target;
            isPlayer = false;
            InitializeView();
        }
    }
    void InitializeView()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            if (isPlayer)
            {
                nameText.readOnly = false;
                nameText.text = player.KnownName(targetPlayer.id);
                relationStat.gameObject.SetActive(true);
                relationStat.options.Clear();
                foreach (string state in PlayerPreferences.characterStateText)
                {
                    relationStat.options.Add(new Dropdown.OptionData() { text = state });
                }
                relationStat.value = 2;
                relationStat.value = (int)player.KnownState(targetPlayer.id);
                buttonStats.gameObject.SetActive(true);
                buttonSkills.gameObject.SetActive(true);
                isInitialized = true;
                if (player.isGM)
                {
                    GMStamina.gameObject.SetActive(true);
                    buttonAttributes.gameObject.SetActive(GameMaster.seeAbilitiesAndAttributes(player.gmState));
                    buttonAbilities.gameObject.SetActive(GameMaster.seeAbilitiesAndAttributes(player.gmState));
                    buttonSkills.gameObject.SetActive(GameMaster.seeAbilitiesAndAttributes(player.gmState));
                    GMHealth.maxValue = targetPlayer.healthMax;
                    GMMana.maxValue = targetPlayer.manaMax;
                    GMStamina.maxValue = targetPlayer.staminaMaxPlayer;
                    GMHealth.interactable = GameMaster.changeBasics(player.gmState);
                    GMMana.interactable = GameMaster.changeBasics(player.gmState);
                    GMStamina.interactable = GameMaster.changeBasics(player.gmState);
                    buttonSliderUpdate.SetActive(GameMaster.changeBasics(player.gmState));
                    isSliderUpdate = true;
                    SetSliderText();

                    skillNameDropdown.ClearOptions();
                    foreach (Skills.SkillInfo skillInfo in Skills.info)
                    {
                        if (skillInfo.skill != Skills.Skill.NoSkill)
                        {
                            skillNameDropdown.options.Add(new Dropdown.OptionData() { text = skillInfo.name });
                        }
                    }
                }
                else
                {
                    buttonAttributes.gameObject.SetActive(false);
                    buttonAbilities.gameObject.SetActive(false);
                    buttonSkills.gameObject.SetActive(false);
                }
            }
            else
            {
                nameText.text = targetEntity.name;
                nameText.readOnly = true;
                relationStat.gameObject.SetActive(false);
                buttonStats.gameObject.SetActive(true);
                buttonAttributes.gameObject.SetActive(false);
                buttonAbilities.gameObject.SetActive(false);
                buttonSkills.gameObject.SetActive(false);
                GMHealth.maxValue = targetEntity.healthMax;
                GMMana.maxValue = targetEntity.manaMax;
                GMHealth.interactable = true;
                GMMana.interactable = true;
                GMStamina.gameObject.SetActive(false);
            }
            if (player.isGM)
            {
                buttonGM.gameObject.SetActive(GameMaster.isShowAdvancedInfo(player.gmState));
                isSliderUpdate = true;
                SetSliderText();
            }
            else
            {
                buttonGM.gameObject.SetActive(false);
            }
            SelectRegister(1);
            panel.SetActive(true);
        }
        else
            panel.SetActive(false);
    }

    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            // only refresh the panel while it's active
            if (panel.activeSelf)
            {
                if (player.target != lastTarget)
                {
                    panel.SetActive(false);
                    return;
                }
                string tmpText = "";
                if (isPlayer)
                {
                    if (panelStats.activeSelf)
                    {
                        if (player.abilities.diagnosis == Abilities.Nav)
                        {
                            tmpText += String.Format("The char is {0} with {1} magic aura.",
                                GlobalFunc.ExamineLimitText((targetPlayer.healthMax + player.divergenceExamination * GlobalVar.ExaminationHealthDivergence) / GlobalVar.healthMax, GlobalVar.healthExaminationLow),
                                GlobalFunc.ExamineLimitText((targetPlayer.manaMax + player.divergenceExamination * GlobalVar.ExaminationManaDivergence) / GlobalVar.manaMax, GlobalVar.manaExaminationLow));
                        }
                        else
                        {
                            tmpText = String.Format("The char is {0} with {1} magic aura.",
                                GlobalFunc.ExamineLimitText((targetPlayer.healthMax + player.divergenceExamination * GlobalVar.ExaminationHealthDivergence) / GlobalVar.healthMax, GlobalVar.healthExaminationNormal),
                                GlobalFunc.ExamineLimitText((targetPlayer.manaMax + player.divergenceExamination * GlobalVar.ExaminationManaDivergence) / GlobalVar.manaMax, GlobalVar.manaExaminationNormal));
                        }
                        if (player.abilities.diagnosis == Abilities.Nav && targetPlayer.isGM)
                        {
                            tmpText += Environment.NewLine + Environment.NewLine + "The char is a GM (Game Master).";
                        }
                        statText.text = tmpText;
                    }
                    else if (panelGM.activeSelf)
                    {
                        if (isSliderUpdate)
                        {
                            GMHealth.value = targetPlayer.health;
                            GMMana.value = targetPlayer.mana;
                            GMStamina.value = targetPlayer.stamina;

                        }
                        GMText.text = "Health :" + targetPlayer.health + " (" + targetPlayer.healthMax + " + " + targetPlayer.healthRecoveryRate + "/s)" + Environment.NewLine
                            + "Mana :" + targetPlayer.mana + " (" + targetPlayer.manaMax + " + " + targetPlayer.manaRecoveryRate + "/s)" + Environment.NewLine
                            + "Play time: " + targetPlayer.playtime + "s" + Environment.NewLine
                            + "Total skill time: " + targetPlayer.skillTotalTime + "s" + Environment.NewLine
                            + "Stamina: " + targetPlayer.stamina + " (" + targetPlayer.staminaMaxPlayer + " + " + targetPlayer.staminaRecoveryPlayer + "/s)" + Environment.NewLine
                            + "Speed: " + targetPlayer.speed + " (" + targetPlayer.speedWalkPlayer + " / " + targetPlayer.speedRunPlayer + ")" + Environment.NewLine
                            + "Weight: " + targetPlayer.weight + " (<" + targetPlayer.weightMaxPlayer / 1000 + " kg)" + Environment.NewLine;
                        GMHealth.maxValue = targetPlayer.healthMax;
                        GMMana.maxValue = targetPlayer.manaMax;
                    }
                }
                else //NPC
                {
                    if (panelStats.activeSelf)
                    {
                        if (player.abilities.diagnosis == 0)
                        {
                            tmpText += String.Format("The target is {0} with {1} magic aura.",
                                GlobalFunc.ExamineLimitText((targetEntity.healthMax + player.divergenceExamination * GlobalVar.ExaminationHealthDivergence) / GlobalVar.healthMax, GlobalVar.healthExaminationLow),
                                GlobalFunc.ExamineLimitText((targetEntity.manaMax + player.divergenceExamination * GlobalVar.ExaminationManaDivergence) / GlobalVar.manaMax, GlobalVar.manaExaminationLow));
                        }
                        else
                        {
                            tmpText = String.Format("The target is {0} with {1} magic aura.",
                                GlobalFunc.ExamineLimitText((targetEntity.healthMax + player.divergenceExamination * GlobalVar.ExaminationHealthDivergence) / GlobalVar.healthMax, GlobalVar.healthExaminationNormal),
                                GlobalFunc.ExamineLimitText((targetEntity.manaMax + player.divergenceExamination * GlobalVar.ExaminationManaDivergence) / GlobalVar.manaMax, GlobalVar.manaExaminationNormal));
                        }
                        statText.text = tmpText;
                    }
                    else if (panelGM.activeSelf)
                    {
                        if (isSliderUpdate)
                        {
                            GMHealth.value = targetEntity.health;
                            GMMana.value = targetEntity.mana;
                        }
                        GMText.text = "Health :" + targetEntity.health + " (" + targetEntity.healthMax + " + " + targetEntity.healthRecoveryRate + "/s)" + Environment.NewLine
                            + "Mana :" + targetEntity.mana + " (" + targetEntity.manaMax + " + " + targetEntity.manaRecoveryRate + "/s)" + Environment.NewLine
                            + "Speed: " + targetEntity.speed + Environment.NewLine;
                    }
                }
            }
        }
        else
            panel.SetActive(false);
    }

    void RegisterSetDefault()
    {
        panelStats.SetActive(false);
        buttonStats.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25);
        panelAttributes.SetActive(false);
        buttonAttributes.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25);
        panelAbilities.SetActive(false);
        buttonAbilities.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25);
        panelSkills.SetActive(false);
        buttonSkills.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25);
        panelGM.SetActive(false);
        buttonGM.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25);
    }
    public void SelectRegister(int register)
    {
        RegisterSetDefault();
        switch (register)
        {
            case 2:
                AttributesCreateList();
                panelAttributes.SetActive(true);
                buttonAttributes.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 27);
                break;
            case 3:
                AbilitiesCreateList();
                panelAbilities.SetActive(true);
                buttonAbilities.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 27);
                break;
            case 4:
                SkillCreateList();
                panelSkills.SetActive(true);
                buttonSkills.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 27);
                break;
            case 5:
                panelGM.SetActive(true);
                buttonGM.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 27);
                break;
            default:
                panelStats.SetActive(true);
                buttonStats.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 27);
                break;
        }
    }

    void AttributesCreateList()
    {
        Player player = Player.localPlayer;
        // update attributes from sync
        targetPlayer.attributes.CreateFromString(targetPlayer.attributesSync);
        // apply todisplay
        listAttributes.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25 * targetPlayer.attributes.count + 25);

        attributeBaseText.text = targetPlayer.attributes.allocatedTotal.ToString() + " attribute points distributed";
        attributeBaseText.color = Color.white;

        foreach (Transform child in listAttributes.transform)
        {
            if (child.gameObject.name != "PanelAttributeBaseInfo")
                Destroy(child.gameObject);
        }

        foreach (Attributes.Attribute attribute in targetPlayer.attributes.listOfAttributes)
        {
            GameObject go = Instantiate(panelAttribute) as GameObject;
            go.SetActive(true);
            PanelAttributeInfo pa = go.GetComponent<PanelAttributeInfo>();
            pa.characterExamination = this;
            pa.attributeName.text = attribute.name;
            pa.buttonPlus.SetActive(false);
            pa.buttonMinus.SetActive(GameMaster.changeAbilitiesAndAttributes(player.gmState));
            pa.attributeValue = attribute.value;
            pa.tooltip = string.Format("<i>{0}</i>" + Environment.NewLine + "{1}", attribute.headline, attribute.description);
            go.transform.SetParent(listAttributes.transform);
        }
    }

    void AbilitiesCreateList()
    {
        Player player = Player.localPlayer;
        // update abilities from sync
        targetPlayer.abilities.CreateFromString(targetPlayer.abilitiesSync);
        // update display
        listAbilities.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25 * targetPlayer.abilities.count + 25);

        abilityBaseText.text = targetPlayer.abilities.allocatedTotal.ToString() + " of " + GlobalVar.abilityTotal.ToString() + " ability points distributed";
        if (targetPlayer.abilities.allocatedTotal < GlobalVar.abilityTotal)
        {
            abilityBaseText.color = Color.green;
        }
        else if (!targetPlayer.abilities.IsCorrectAssigned())
            abilityBaseText.color = Color.red;
        else
            abilityBaseText.color = Color.white;

        foreach (Transform child in listAbilities.transform)
        {
            if (child.gameObject.name != "PanelAbilityBaseInfo")
                Destroy(child.gameObject);
        }

        foreach (Abilities.Ability ability in targetPlayer.abilities.listOfAbilities)
        {
            GameObject go = Instantiate(panelAbilitiy) as GameObject;
            go.SetActive(true);
            PanelAbilityInfo pa = go.GetComponent<PanelAbilityInfo>();
            pa.characterExamination = this;
            pa.abilityeName.text = ability.name;
            pa.buttonPlus.SetActive(GameMaster.changeAbilitiesAndAttributes(player.gmState) && targetPlayer.abilities.allocatedTotal < GlobalVar.abilityTotal);
            pa.buttonMinus.SetActive(GameMaster.changeAbilitiesAndAttributes(player.gmState));
            pa.abilityValue = ability.value;
            switch (ability.value)
            {
                case Abilities.Nav:
                    pa.tooltip = ability.descriptionNav;
                    break;
                case Abilities.Poor:
                    pa.tooltip = ability.descriptionPoor;
                    break;
                case Abilities.Good:
                    pa.tooltip = ability.descriptionGood;
                    break;
                default:
                    pa.tooltip = ability.descriptionExcellent;
                    break;
            }
            go.transform.SetParent(listAbilities.transform);
        }
    }

    void SkillCreateList()
    {
        Player player = Player.localPlayer;

        // update display
        listSkills.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25 * targetPlayer.skills.Count + 25);
        // remove old
        foreach (Transform child in listSkills.transform)
        {
            if (child.gameObject.name != "PanelSkillBaseInfo")
                Destroy(child.gameObject);
        }

        foreach (SkillExperience skill in targetPlayer.skills)
        {
            GameObject go = Instantiate(panelSkill) as GameObject;
            go.SetActive(true);
            PanelSkillControl pa = go.GetComponent<PanelSkillControl>();
            pa.characterExamination = this;
            pa.targetPlayer = targetPlayer;
            pa.skillId = skill.id;
            go.transform.SetParent(listSkills.transform);
        }
    }

    public void NameInputChanged()
    {
        Image text = nameText.GetComponent<Image>();
        if (GlobalFunc.IsAllowedDisplayedNameExtended(nameText.text))
            text.color = Color.white;
        else
            text.color = Color.red;
    }
    public void NameOrStateChanged()
    {
        if (isInitialized)
        {
            if (GlobalFunc.IsAllowedDisplayedNameExtended(nameText.text))
            {
                Player player = Player.localPlayer;
                player.UpdateKnownNames(targetPlayer.id, nameText.text, (Player.CharacterState)relationStat.value);
            }
        }
    }


    public void ChangeAbility(string abilityName, bool increase)
    {
        Player player = Player.localPlayer;
        Abilities tmpAbilities = new Abilities();
        tmpAbilities.CreateFromString(targetPlayer.abilities.CreateString());
        tmpAbilities.ChangeValue(abilityName, increase);
        if (tmpAbilities.allocatedTotal > GlobalVar.abilityTotal || (tmpAbilities.allocatedTotal == GlobalVar.abilityTotal && !tmpAbilities.IsCorrectAssigned()))
        {
            player.Inform("This combination of abilities is not permitted. Please check the permissible settings based on the documentation.");
            return;
        }

        targetPlayer.CmdChangeAbitity(abilityName, increase);
        player.Inform(string.Format("You {1} ability {0} of {2}", abilityName, (increase ? "increase" : "decrease"), targetPlayer.name));
        player.CmdGmLogAction(targetPlayer.id, string.Format("Ability {0} set {1}", abilityName, (increase ? "higher" : "lower")));
        player.chat.CmdMsgGmToSingle(targetPlayer.id, string.Format("GM try to {1} your ability {0}", abilityName, (increase ? "increase" : "decrease")));

        Invoke("AbilitiesCreateList", 2);
    }

    public void ChangeAttribute(string attributeName, int value)
    {
        Player player = Player.localPlayer;

        player.CmdGmChangeAttributeFor(targetPlayer.id, attributeName, value);
        player.Inform(string.Format("Try to {0} attribute {1} for player {2}", (value > 0 ? "boost" : "reduce"), attributeName, targetPlayer.displayName));
        player.CmdGmLogAction(targetPlayer.id, string.Format("Change attribute {0} by {1}", attributeName, value));
        player.chat.CmdMsgGmToSingle(targetPlayer.id, string.Format("GM {1} attribute {0}", (value > 0 ? "boost" : "reduce"), attributeName));

        Invoke("AttributesCreateList", 2);
    }


    public void ChangeSkill(int skillId, int experience)
    {
        Player player = Player.localPlayer;

        player.CmdGmChangeSkillFor(targetPlayer.id, skillId, experience);
        player.Inform(string.Format("Set skill {1} for player {2} to {0}", experience / GlobalVar.skillExperiencePerLevel, Skills.info[skillId].name, targetPlayer.displayName));
        player.CmdGmLogAction(targetPlayer.id, string.Format("Change skill {0} to {1}", Skills.info[skillId].name, experience / GlobalVar.skillExperiencePerLevel));
        player.chat.CmdMsgGmToSingle(targetPlayer.id, string.Format("GM changed skill {0} to {1}", Skills.info[skillId].name, experience / GlobalVar.skillExperiencePerLevel));
    }
    public void RemoveSkill()
    {
        Player player = Player.localPlayer;

        int skillId = Skills.IdFromName(skillNameDropdown.options[skillNameDropdown.value].text);
        if (skillId >= 0)
        {
            if (targetPlayer.skills.ExperienceOfId(skillId) > 0)
            {
                player.CmdGmChangeSkillFor(targetPlayer.id, skillId, 0);
                player.Inform(string.Format("Remove skill {0} for player {1}", Skills.info[skillId].name, targetPlayer.displayName));
                player.CmdGmLogAction(targetPlayer.id, string.Format("Remove skill {0}", Skills.info[skillId].name));
                player.chat.CmdMsgGmToSingle(targetPlayer.id, string.Format("GM removed skill {0}", Skills.info[skillId].name));
            }
            else
            {
                player.Inform("You cannot remove a skill, ther player don't have.");
            }
        }
        else
        {
            player.Inform(string.Format("The skill '{0}' does not exists.", skillNameDropdown.options[skillNameDropdown.value].text));
        }
    }

    public void AddSkill()
    {
        Player player = Player.localPlayer;

        int skillId = Skills.IdFromName(skillNameDropdown.options[skillNameDropdown.value].text);
        if (skillId >= 0)
        {
            if (targetPlayer.skills.ExperienceOfId(skillId) <= 0)
            {
                player.CmdGmChangeSkillFor(targetPlayer.id, skillId, 1);
                player.Inform(string.Format("Create skill {0} for player {1}", Skills.info[skillId].name, targetPlayer.displayName));
                player.CmdGmLogAction(targetPlayer.id, string.Format("Create skill {0}", Skills.info[skillId].name));
                player.chat.CmdMsgGmToSingle(targetPlayer.id, string.Format("GM created skill {0}", Skills.info[skillId].name));
            }
            else
            {
                player.Inform("You cannot add a skill the player has already.");
            }
        }
        else
        {
            player.Inform(string.Format("The skill '{0}' does not exists.", skillNameDropdown.options[skillNameDropdown.value].text));
        }
    }

    public void GMHealthChanged()
    {
        if (!isSliderUpdate)
        {
            Player player = Player.localPlayer;
            player.CmdSetTargetHealth((int)Mathf.Max(GMHealth.value, 1));
        }
    }
    public void GMManaChanged()
    {
        if (!isSliderUpdate)
        {
            Player player = Player.localPlayer;
            player.CmdSetTargetMana((int)GMMana.value);
        }
    }
    public void GMStaminaChanged()
    {
        if (!isSliderUpdate)
        {
            Player player = Player.localPlayer;
            player.CmdSetTargetStamina((int)Mathf.Clamp(GMStamina.value, -1, targetPlayer.staminaMaxPlayer));
        }
    }
    public void ChangeSliderUpdate()
    {
        isSliderUpdate = !isSliderUpdate;
        SetSliderText();
    }
    private void SetSliderText()
    {
        if (isSliderUpdate)
        {
            buttonStopText.text = "A" + Environment.NewLine + "d" + Environment.NewLine + "a" + Environment.NewLine + "p" + Environment.NewLine + "t";
        }
        else
        {
            buttonStopText.text = "S" + Environment.NewLine + "h" + Environment.NewLine + "o" + Environment.NewLine + "w";
        }
    }
}