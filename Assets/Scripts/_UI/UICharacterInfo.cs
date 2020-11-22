/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Note: this script has to be on an always-active UI parent, so that we can
// always react to the hotkey.
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
public partial class UICharacterInfo : MonoBehaviour
{
    public KeyCode hotKey = KeyCode.I;
    public GameObject panel;
    [Header("Stats")]
    public GameObject panelStats;
    public Button buttonStats;
    public Text statsText;
    private string staticText = "";
    [Header("Attributes")]
    public GameObject panelAttributes;
    public Button buttonAttributes;
    public GameObject listAttributes;
    public GameObject panelAttribute;
    public Text attributeBaseText;
    public GameObject iconNetworkingAttribute;
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
    [Header("GM")]
    public GameObject panelGM;
    public Button buttonGM;
    public Text GMInfoText;
    private bool isShown = false;
    bool isNetworking = false;
    void InitializeView()
    {
        isNetworking = false;
        Player player = Player.localPlayer;
        if (player)
        {
            SelectRegister(1);
            AttributesCreateList();
            AbilitiesCreateList();
            SkillCreateList();
            staticText = String.Format("Your name is <b>{7}</b>" + Environment.NewLine + Environment.NewLine
                + "A look in the mirror shows you are {0} and heal {1}." + Environment.NewLine + "There is {2} magic aura with {3} mana refill." + Environment.NewLine
                + "You walk with a {4} speed and can run for {5}." + Environment.NewLine + Environment.NewLine + "Unique character id: {6}",
                GlobalFunc.ExamineLimitText(1f * player.healthMax / GlobalVar.healthMax, GlobalVar.healthExaminationNormal),
                GlobalFunc.ExamineLimitText(1f * player.healthRecoveryRate / GlobalVar.healthRecoveryMax, GlobalVar.generalExaminationSpeed),
                GlobalFunc.ExamineLimitText(1f * player.manaMax / GlobalVar.manaMax, GlobalVar.manaExaminationNormal),
                GlobalFunc.ExamineLimitText(1f * player.manaRecoveryRate / GlobalVar.manaRecoveryMax, GlobalVar.generalExaminationSpeed),
                GlobalFunc.ExamineLimitText(player.speedWalkPlayer / GlobalVar.speedWalkMax, GlobalVar.generalExaminationSpeed),
                GlobalFunc.ExamineLimitText(player.staminaMaxPlayer / GlobalVar.staminaMax, GlobalVar.generalExaminationTime),
                player.id,
                player.displayName);
            buttonGM.gameObject.SetActive(player.isGM);

        }
        else
            panel.SetActive(false);
    }

    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            // hotkey (not while typing in chat, etc.)
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                panel.SetActive(!panel.activeSelf);
            // only refresh the panel while it's active
            if (panel.activeSelf)
            {
                if (!isShown)
                {
                    InitializeView();
                    isShown = true;
                }
                if (panelStats.activeSelf)
                {
                    statsText.text = string.Format("{0}" + Environment.NewLine + "You have {1} available."
                        , staticText
                        , Money.MoneyText(Money.AvailableMoney(player)));
                }
                if (panelGM.activeSelf)
                {
                    if (GameMaster.isGod(player.gmState))
                        GMInfoText.text = "<b>You are God.</b>" + Environment.NewLine + "Use your unlimited power for better role play for everyone.";
                    else
                        GMInfoText.text = "You are a GM." + Environment.NewLine + "Use your power for better role play for everyone." + Environment.NewLine + Environment.NewLine
                            + "<b>GM powers</b>" + Environment.NewLine
                            + "You can teleport yourself: Yes" + Environment.NewLine
                            + "You can enter GM island: " + (GameMaster.enterGmIsland(player.gmState) ? "yes" : "no") + Environment.NewLine
                            + "Player see you as GM: " + (GameMaster.showGmInOverlay(player.gmState) ? "yes" : "no") + Environment.NewLine
                            + "You see player names: " + (GameMaster.knowNames(player.gmState) ? "yes" : "no") + Environment.NewLine
                            + "You see advanced info: " + (GameMaster.isShowAdvancedInfo(player.gmState) ? "yes" : "no") + Environment.NewLine
                            + "You see all players list: " + (GameMaster.seeAllPlayer(player.gmState) ? "yes" : "no") + Environment.NewLine
                            + "You can broadcast: " + (GameMaster.broadcast(player.gmState) ? "yes" : "no") + Environment.NewLine
                            + "You can pull monster: " + (GameMaster.pullMonster(player.gmState) ? "yes" : "no") + Environment.NewLine
                            + "You can kill monster: " + (GameMaster.killMonster(player.gmState) ? "yes" : "no") + Environment.NewLine
                            + "You can pull player: " + GameMaster.hasTeleports(player.gmState).ToString() + " x" + Environment.NewLine
                            + "You can kill player: " + GameMaster.hasKills(player.gmState).ToString() + " x" + Environment.NewLine
                            + "You can " + (GameMaster.changeAbilitiesAndAttributes(player.gmState) ? "reduce" : "see") + " attributes and abilities: " + (GameMaster.seeAbilitiesAndAttributes(player.gmState) ? "yes" : "no") + Environment.NewLine
                            + "You have unlimited health: " + (GameMaster.unlimitedHealth(player.gmState) ? "yes" : "no") + Environment.NewLine
                            + "You have unlimited mana: " + (GameMaster.unlimitedMana(player.gmState) ? "yes" : "no") + Environment.NewLine
                            + "You have unlimited stamina: " + (GameMaster.unlimitedStamina(player.gmState) ? "yes" : "no") + Environment.NewLine
                            + "You have DarkVision: Yes" + Environment.NewLine
                            + "You can read and write everything: Yes" + Environment.NewLine;
                }
                iconNetworkingAttribute.SetActive(isNetworking);
            }
            else
            {
                isShown = false;
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
                panelAttributes.SetActive(true);
                buttonAttributes.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 27);
                break;
            case 3:
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
        bool canAddAttribuite = false;
        listAttributes.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25 * player.attributes.count + 25);
        int attributeTotal = player.AttributeTotalMax();

        attributeBaseText.text = player.attributes.allocatedTotal.ToString() + " of " + attributeTotal.ToString() + " attribute points distributed";
        if (player.attributes.allocatedTotal < attributeTotal)
        {
            attributeBaseText.color = Color.green;
            canAddAttribuite = true;
        }
        else
            attributeBaseText.color = Color.white;

        foreach (Transform child in listAttributes.transform)
        {
            if (child.gameObject.name != "PanelAttributeBaseInfo")
                Destroy(child.gameObject);
        }

        foreach (Attributes.Attribute attribute in player.attributes.listOfAttributes)
        {
            GameObject go = Instantiate(panelAttribute) as GameObject;
            go.SetActive(true);
            PanelAttributeInfo pa = go.GetComponent<PanelAttributeInfo>();
            pa.characterInfo = this;
            pa.attributeName.text = attribute.name;
            //>>> reduce always possible still
            //there is a quest needed with time limitation
            pa.buttonMinus.SetActive(true);
            pa.buttonPlus.SetActive(canAddAttribuite);
            pa.attributeValue = attribute.value;
            pa.tooltip = string.Format("<i>{0}</i>" + Environment.NewLine + "{1}", attribute.headline, attribute.description);
            go.transform.SetParent(listAttributes.transform);
            isNetworking = false;
        }
    }

    public void AbilitiesCreateList()
    {
        Player player = Player.localPlayer;
        bool canAddAbility = false;
        listAbilities.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25 * player.abilities.count + 25);

        abilityBaseText.text = player.abilities.allocatedTotal.ToString() + " of " + GlobalVar.abilityTotal.ToString() + " ability points distributed";
        if (player.abilities.allocatedTotal < GlobalVar.abilityTotal)
        {
            abilityBaseText.color = Color.green;
            canAddAbility = true;
        }
        else if (!player.abilities.IsCorrectAssigned())
            abilityBaseText.color = Color.red;
        else
            abilityBaseText.color = Color.white;

        foreach (Transform child in listAbilities.transform)
        {
            if (child.gameObject.name != "PanelAbilityBaseInfo")
                Destroy(child.gameObject);
        }

        foreach (Abilities.Ability ability in player.abilities.listOfAbilities)
        {
            GameObject go = Instantiate(panelAbilitiy) as GameObject;
            go.SetActive(true);
            PanelAbilityInfo pa = go.GetComponent<PanelAbilityInfo>();
            pa.characterInfo = this;
            pa.abilityeName.text = ability.name;
            //>>> reduce always possible still
            //there is a quest needed with time limitation
            pa.buttonMinus.SetActive(true);
            pa.buttonPlus.SetActive(canAddAbility);
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
        isNetworking = false;
    }

    void SkillCreateList()
    {
        Player player = Player.localPlayer;

        // update display
        listSkills.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25 * player.skills.Count + 25);
        // remove old
        foreach (Transform child in listSkills.transform)
        {
            if (child.gameObject.name != "PanelSkillBaseInfo")
                Destroy(child.gameObject);
        }
        // create new
        foreach (SkillExperience skill in player.skills)
        {
            GameObject go = Instantiate(panelSkill) as GameObject;
            go.SetActive(true);
            PanelSkillInfo pa = go.GetComponent<PanelSkillInfo>();
            pa.characterInfo = this;
            pa.player = player;
            pa.skillId = skill.id;
            go.transform.SetParent(listSkills.transform);
        }
    }

    public void ChangeAttribute(string attributeName, int value)
    {
        if (!isNetworking)
        {
            Player player = Player.localPlayer;
            player.CmdGmChangeAttributeFor(player.id, attributeName, value);
            player.Inform(string.Format("Try to {0} attribute {1}", (value > 0 ? "boost" : "reduce"), attributeName));
            //wait 2 seconds tosynchronize change
            isNetworking = true;
            Invoke("AttributesCreateList", 2);
        }
    }

    public void ChangeAbility(string abilityName, bool increase)
    {
        if (!isNetworking)
        {
            Player player = Player.localPlayer;
            Abilities tmpAbilities = new Abilities();
            tmpAbilities.CreateFromString( player.abilities.CreateString());
            tmpAbilities.ChangeValue(abilityName, increase);
            if (tmpAbilities.allocatedTotal > GlobalVar.abilityTotal || (tmpAbilities.allocatedTotal == GlobalVar.abilityTotal && !tmpAbilities.IsCorrectAssigned()))
            {
                player.Inform("This combination of abilities is not permitted. Please check the permissible settings based on the documentation.");
                return;
            }

            player.CmdChangeAbitity(abilityName, increase);
            player.Inform(string.Format("Try to {0} ability {1}", (increase ? "boost" : "reduce"), abilityName));

            isNetworking = true;
            Invoke("AbilitiesCreateList", 2);
        }
    }

    public void ReduceSkill(int skillId)
    {
        Player player = Player.localPlayer;
        int exp = Mathf.Max(0, player.skills.ExperienceOfId(skillId) - GlobalVar.skillExperiencePerLevel);
        player.CmdGmChangeSkillFor(player.id, skillId, exp);
        player.Inform(string.Format("You reduced your skill {0} by one full level.", Skills.info[skillId].name));
    }
}