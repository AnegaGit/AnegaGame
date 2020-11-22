/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;
using System.Collections.Generic;
using UMA;
using UMA.CharacterSystem;

public partial class UICharacterCreation : MonoBehaviour
{
    public Sprite testSprite;

    public NetworkManagerMMO manager; // singleton is null until update
    public GameObject panel;
    public Transform cameraBodyLocation;
    public Transform cameraHeadLocation;
    private bool isHeadCamera = false;
    public float headCameraYMin;
    public float headCameraYMax;
    public GameObject panelExplanation;
    public Text explanation;
    [Header("Panel Name")]
    public GameObject panelName;
    public InputField displayedNameInput;
    [Header("Panel Apperance")]
    public GameObject panelApperance;
    public Dropdown classDropdown;
    public Slider turningSpeed;
    public Slider headCameraHeight;
    public Button buttonHeadCamera;
    public Button buttonBodyCamera;
    public Slider characterHeigth;
    public Slider characterFat;
    public Slider characterMuscles;
    public Slider characterBreastSize;
    public Slider characterGluteusSize;
    public Slider characterWaist;
    public Slider characterHeadWidth;
    public Slider characterChinSize;
    public Slider characterchinPronounced;
    public Slider characterEyeSize;
    public Slider characterMouthSize;
    public Slider characterLipsSize;
    public Slider characterEarSize;
    public Slider characterNoseSize;
    public Slider characterNoseWidth;
    public Slider characterNoseCurve;
    public Dropdown characterSkinColor;
    public Slider characterHair;
    public Dropdown characterHairColor;
    public Slider characterBeard;
    public Dropdown characterBeardColor;
    public Slider characterFangs;
    public GameObject exampleChar;
    private bool isAvatarReady = false;
    private DynamicCharacterAvatar avatarUMA;
    private Dictionary<string, DnaSetter> avatarDNA;
    [Header("Panel Default Character")]
    public GameObject panelDefaultChars;
    public GameObject panelDefaultChar;
    public GameObject listDefaultChars;
    public Button defaultCharCreate;
    [Header("Panel Attributes")]
    public GameObject panelAbttributes;
    public GameObject panelAttribute;
    public GameObject listAttributes;
    public Button verifyAttributes;
    [Header("Panel Abilities")]
    public GameObject panelAbilities;
    public GameObject panelAbility;
    public GameObject listAbilities;
    public Button verifyAbilities;
    [Header("Panel Skills")]
    public GameObject panelSkills;
    public GameObject panelSkill;
    public GameObject listSkills;
    public Button verifySkills;
    [Header("Panel StarterSet")]
    public GameObject panelStarterSets;
    public GameObject panelStarterSet;
    public GameObject listStarterSets;
    [Header("Panel UniqueItems")]
    public GameObject panelUniqueItems;
    public GameObject panelUniqueItem;
    public GameObject listUniqueItems;
    public Dropdown uniqueItemsDropdown;
    public Button buttonAddUniqueItem;
    [Header("Panel Verification")]
    public GameObject panelVerification;
    public Text verifyTextDisplayedName;
    public Text verifyTextApperance;
    public Text verifyTextAttributes;
    public Text verifyTextAbilities;
    public Text verifyTextSkill;
    public Text verifyTextStarterSet;
    public Text verifyTextSpecialItems;
    public Button createButton;
    [Header("Buttons")]
    public Button nextButton;
    public Button cancelButton;
    public Button previousButton;
    private bool isInitialized = false;
    private bool isFirstInitialized = false;
    private GameObject[] panels = new GameObject[1];
    private int currentPanel = 0;
    private DefaultChars defaultChars = new DefaultChars();
    private int defaultCharSelected = 0;
    private List<PanelDefaultChar> defaultCharPanels = new List<PanelDefaultChar>();
    private Abilities abilities = new Abilities();
    private Attributes attributes = new Attributes();
    private StarterSets starterSets = new StarterSets();
    private int starterSetSelected = 1;
    private List<PanelStarterSet> starterSetPanels = new List<PanelStarterSet>();
    private GameObject previewChar;
    private UniqueStarterItems uniqueStarterItems = new UniqueStarterItems();
    private Apperance apperance = new Apperance();
    private int[] defaultSkills = new int[Skills.maxSkills];


    private Color colorTrue = Color.green;
    private Color colorFalse = Color.red;
    private Color colorMaybe = Color.yellow;

    private void Start()
    {
        panels = new GameObject[9] { panelName, panelApperance, panelDefaultChars, panelAbttributes, panelAbilities, panelSkills, panelStarterSets, panelUniqueItems, panelVerification };
        starterSets.Reload();
    }

    void Update()
    {
        // only update while visible (after character selection made it visible)
        if (panel.activeSelf)
        {
            // still in lobby?
            if (manager.state == NetworkState.Lobby)
            {
                Show();
                if (!isInitialized)
                {
                    // prepare view
                    currentPanel = 0;
                    if (!isFirstInitialized)
                    {
                        InitializeApperance();
                        InitializeDefaultChars();
                        //InitializeAbilities();
                        //InitializeAttributes();
                        //InitializeStarterSets();
                        InitializeUniqueItems();
                        isFirstInitialized = true;
                    }
                    PrepareView();

                    isInitialized = true;
                    // setup camera
                    Camera.main.transform.position = cameraBodyLocation.position;
                    Camera.main.transform.rotation = cameraBodyLocation.rotation;
                    headCameraHeight.minValue = headCameraYMin;
                    headCameraHeight.maxValue = headCameraYMax;
                    headCameraHeight.value = (headCameraYMin + headCameraYMax) / 2;
                    isHeadCamera = false;
                    buttonHeadCamera.gameObject.SetActive(true);
                    buttonBodyCamera.gameObject.SetActive(false);
                }
                //previous
                previousButton.onClick.SetListener(() =>
                {
                    currentPanel--;
                    PrepareView();
                });
                //next
                nextButton.onClick.SetListener(() =>
                {
                    currentPanel++;
                    PrepareView();
                });
                // cancel
                cancelButton.onClick.SetListener(() =>
                {
                    Hide();
                });
            }
            else Hide();
        }
        else Hide();
    }
    public void Hide()
    {
        panel.SetActive(false);
        isInitialized = false;
    }
    public void Show() { panel.SetActive(true); }
    public bool IsVisible() { return panel.activeSelf; }
    private void PrepareView()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (i == currentPanel)
                panels[i].SetActive(true);
            else
                panels[i].SetActive(false);
        }
        if (currentPanel == 0)
        {
            previousButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(true);
        }
        else if (currentPanel == panels.Length - 1)
        {
            previousButton.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(false);
        }
        else
        {
            previousButton.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(true);
        }
        panelExplanation.SetActive(true);
        switch (currentPanel)
        {
            case 0:
                NameAndRaceExplanation();
                break;
            case 1:
                panelExplanation.SetActive(false);
                break;
            case 2:
                DefaultCharExplanation();
                break;
            case 3:
                AttributeExplanation();
                break;
            case 4:
                AbilityExplanation();
                break;
            case 5:
                SkillExplanation();
                break;
            case 6:
                StarterSetExplanation();
                break;
            case 7:
                UniqueItemExplanation();
                break;
            case 8:
                VerificationExplanation();
                InitializeVerification();
                break;
        }
    }

    // -------- Code section ---------------
    // Name selection
    public void DisplayNameInputChanged()
    {
        Image text = displayedNameInput.GetComponent<Image>();
        if (GlobalFunc.IsAllowedDisplayedName(displayedNameInput.text))
            text.color = Color.white;
        else
            text.color = Color.red;
        panelExplanation.SetActive(true);
    }

    private void NameAndRaceExplanation()
    {
        explanation.text = "<b>Displayed Name</b>" + Environment.NewLine
        + "This name is shown to other character. You can change it later." + Environment.NewLine + Environment.NewLine
        + "The name should correspond to a medieval fantasy world." + Environment.NewLine
        + "It must not be a person of public interest." + Environment.NewLine
        + "It must not interfer political, religious or economic interests." + Environment.NewLine
        + "It must be unique.";
    }
    public void NameVerification()
    {
        CharacterVerifyMsg message = new CharacterVerifyMsg
        {
            displayedName = displayedNameInput.text
        };
        manager.client.Send(CharacterVerifyMsg.MsgId, message);
    }

    // -------- Code section ---------------
    //Default Characters
    public void DefaultCharacterChanged(int id)
    {
        defaultCharSelected = id;
        SetCurrentDefaultChar();
    }
    private void InitializeDefaultChars()
    {
        defaultCharPanels.Clear();
        listDefaultChars.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 40 * defaultChars.count);
        defaultChars.Reload();
        foreach (DefaultChar set in defaultChars.listOfDefaultChars)
        {
            GameObject go = Instantiate(panelDefaultChar) as GameObject;
            go.SetActive(true);
            PanelDefaultChar pa = go.GetComponent<PanelDefaultChar>();
            pa.characterCreation = this;
            pa.explanationPanel = explanation;
            pa.Name(set.name);
            pa.explanation = string.Format("<b>{0}</b>" + Environment.NewLine + Environment.NewLine + "<i>{1}</i>" + Environment.NewLine + Environment.NewLine + "{2}"
                , set.name
                , set.headline
                , set.description);
            pa.SetValue(0);
            pa.id = set.identifier;
            go.transform.SetParent(listDefaultChars.transform);
            defaultCharPanels.Add(pa);
        }
        SetCurrentDefaultChar();
    }
    private void SetCurrentDefaultChar()
    {
        foreach (PanelDefaultChar pa in defaultCharPanels)
        {
            if (pa.id == defaultCharSelected)
                pa.SetValue(1);
            else
                pa.SetValue(0);
        }
        foreach (DefaultChar set in defaultChars.listOfDefaultChars)
        {
            if (set.identifier == defaultCharSelected)
            {
                if (set.attributes.Length < attributes.count)
                    attributes.CreateRandom(GlobalVar.attributeTotal - GlobalVar.attributeStartPenalty);
                else
                    attributes.CreateFromString(set.attributes);
                InitializeAttributes();

                if (set.abilities.Length < abilities.count)
                    abilities.CreateRandom();
                else
                    abilities.CreateFromString(set.abilities);
                InitializeAbilities();

                if (set.skills.Length < 4)
                    CreateRandomSkills();
                else
                    defaultSkills = Skills.DeserializeDefaultSkills(set.skills);
                InitializeSkills();

                if (set.starterSet)
                {
                    starterSetSelected = set.starterSet.identifier;
                }
                else
                {
                    starterSetSelected = GlobalFunc.RandomInRange(0, starterSets.listOfStarterSets.Count-1);
                }
                InitializeStarterSets();
            }
        }
    }

    public void DefaultCharExplanation()
    {
        explanation.text = string.Format("<i>{0}</i>" + Environment.NewLine + Environment.NewLine + "{1}",
            defaultChars.headline,
            defaultChars.description);
    }
    public void GoToCreation()
    {
        currentPanel = panels.Length - 1;
        PrepareView();
    }

    // -------- Code section ---------------
    // Apperance selection
    public void InitializeApperance()
    {
        // copy player classes to class selection
        classDropdown.options = manager.GetPlayerClasses().Select(
            p => new Dropdown.OptionData(p.displayName)
        ).ToList();
        classDropdown.options.Add(new Dropdown.OptionData() { text = "nothing" });
        classDropdown.value = classDropdown.options.Count;
        characterHeigth.minValue = 0f;
        characterHeigth.maxValue = 1f;
        characterFat.minValue = 0f;
        characterFat.maxValue = 1f;
        characterMuscles.minValue = 0f;
        characterMuscles.maxValue = 1f;
        characterBreastSize.minValue = 0f;
        characterBreastSize.maxValue = 1f;
        characterSkinColor.ClearOptions();
        characterHair.minValue = 0;
        characterHair.maxValue = 0;
        characterHairColor.ClearOptions();
        characterBeard.minValue = 0;
        characterBeard.maxValue = 0;
        characterBeardColor.ClearOptions();
        characterFangs.minValue = 0;
        characterFangs.maxValue = 0;
        UpdateApperanceValues();
    }
    void UpdateApperanceValues()
    {
        characterHeigth.value = apperance.height;
        characterFat.value = apperance.fat;
        characterMuscles.value = apperance.muscles;
        characterBreastSize.value = apperance.breastSize;
        characterGluteusSize.value = apperance.gluteusSize;
        characterWaist.value = apperance.waist;
        characterHair.value = apperance.hair;
        characterBeard.value = apperance.beard;
        characterHeadWidth.value = apperance.headWidth;
        characterChinSize.value = apperance.chinSize;
        characterchinPronounced.value = apperance.chinPronounced;
        characterEyeSize.value = apperance.eyeSize;
        characterMouthSize.value = apperance.mouthSize;
        characterLipsSize.value = apperance.lipsSize;
        characterEarSize.value = apperance.earsSize;
        characterNoseSize.value = apperance.noseSize;
        characterNoseWidth.value = apperance.noseWidth;
        characterNoseCurve.value = apperance.noseCurve;
        characterFangs.value = apperance.fangs;
    }
    private void CreateRandomApperance()
    {
        apperance.height = UnityEngine.Random.Range(characterHeigth.minValue, characterHeigth.maxValue);
        apperance.fat = UnityEngine.Random.Range(characterFat.minValue, characterFat.maxValue);
        apperance.muscles = UnityEngine.Random.Range(characterMuscles.minValue, characterMuscles.maxValue);
        apperance.breastSize = UnityEngine.Random.Range(characterBreastSize.minValue, characterBreastSize.maxValue);
        apperance.gluteusSize = UnityEngine.Random.Range(characterGluteusSize.minValue, characterGluteusSize.maxValue);
        apperance.waist = UnityEngine.Random.Range(characterWaist.minValue, characterWaist.maxValue);
        apperance.hair = UnityEngine.Random.Range((int)characterHair.minValue, (int)characterHair.maxValue);
        apperance.beard = UnityEngine.Random.Range((int)characterBeard.minValue, (int)characterBeard.maxValue);
        apperance.headWidth = UnityEngine.Random.Range(characterHeadWidth.minValue, characterHeadWidth.maxValue);
        apperance.chinSize = UnityEngine.Random.Range(characterChinSize.minValue, characterChinSize.maxValue);
        apperance.chinPronounced = UnityEngine.Random.Range(characterchinPronounced.minValue, characterchinPronounced.maxValue);
        apperance.eyeSize = UnityEngine.Random.Range(characterEyeSize.minValue, characterEyeSize.maxValue);
        apperance.mouthSize = UnityEngine.Random.Range(characterMouthSize.minValue, characterMouthSize.maxValue);
        apperance.lipsSize = UnityEngine.Random.Range(characterLipsSize.minValue, characterLipsSize.maxValue);
        apperance.earsSize = UnityEngine.Random.Range(characterEarSize.minValue, characterEarSize.maxValue);
        apperance.noseSize = UnityEngine.Random.Range(characterNoseSize.minValue, characterNoseSize.maxValue);
        apperance.noseWidth = UnityEngine.Random.Range(characterNoseWidth.minValue, characterNoseWidth.maxValue);
        apperance.noseCurve = UnityEngine.Random.Range(characterNoseCurve.minValue, characterNoseCurve.maxValue);
        apperance.fangs = UnityEngine.Random.Range((int)characterFangs.minValue, (int)characterFangs.maxValue);

        Player currentSelected = previewChar.GetComponent<Player>();
        characterSkinColor.value = UnityEngine.Random.Range(0, characterSkinColor.options.Count - 1);
        apperance.skinColor = currentSelected.apperanceSkinColors[characterSkinColor.value].usedForUMA;
        characterHairColor.value = UnityEngine.Random.Range(0, characterHairColor.options.Count - 1);
        apperance.hairColor = currentSelected.apperanceHairColors[characterHairColor.value];
        characterBeardColor.value = UnityEngine.Random.Range(0, characterBeardColor.options.Count - 1);
        apperance.beardColor = currentSelected.apperanceHairColors[characterBeardColor.value];
        UpdateApperanceValues();
    }
    public void RaceAndGenderChanged()
    {
        // find the prefab for that class
        Destroy(previewChar);
        Player prefab = manager.GetPlayerClasses().Find(p => p.displayName == classDropdown.options[classDropdown.value].text);
        if (prefab == null)
        {
            isAvatarReady = false;
            return;
        }
        isAvatarReady = false;
        previewChar = Instantiate(prefab.gameObject, exampleChar.transform.position, exampleChar.transform.rotation, exampleChar.transform);
        avatarUMA = previewChar.GetComponent<DynamicCharacterAvatar>();
        avatarUMA.CharacterCreated.AddListener(RaceAndGenderChangeFinished);
        previewChar.name = displayedNameInput.text;
        // create base values for slider from prefab
        characterHeigth.minValue = prefab.apperanceHeightMin;
        characterHeigth.maxValue = prefab.apperanceHeightMax;
        characterHeigth.value = (characterHeigth.maxValue + characterHeigth.minValue) / 2;
        characterFat.minValue = prefab.apperanceFatMin;
        characterFat.maxValue = prefab.apperanceFatMax;
        characterFat.value = (characterFat.maxValue + characterFat.minValue) / 2;
        characterMuscles.maxValue = prefab.apperanceMusclesMax;
        characterMuscles.minValue = prefab.apperanceMusclesMin;
        characterMuscles.value = (characterMuscles.maxValue + characterMuscles.minValue) / 2;
        characterBreastSize.minValue = prefab.apperanceBreastSizeMin;
        characterBreastSize.maxValue = prefab.apperanceBreastSizeMax;
        characterBreastSize.value = (characterBreastSize.maxValue + characterBreastSize.minValue) / 2;
        characterGluteusSize.minValue = prefab.apperanceGluteusSizeMin;
        characterGluteusSize.maxValue = prefab.apperanceGluteusSizeMax;
        characterGluteusSize.value = (characterGluteusSize.maxValue + characterGluteusSize.minValue) / 2;
        characterWaist.minValue = prefab.apperanceWaistMin;
        characterWaist.maxValue = prefab.apperanceWaistMax;
        characterWaist.value = (characterWaist.maxValue + characterWaist.minValue) / 2;
        characterHeadWidth.minValue = prefab.apperanceHeadWidthMin;
        characterHeadWidth.maxValue = prefab.apperanceHeadWidthMax;
        characterHeadWidth.value = (characterHeadWidth.maxValue + characterHeadWidth.minValue) / 2;
        characterChinSize.minValue = prefab.apperanceChinSizeMin;
        characterChinSize.maxValue = prefab.apperanceChinSizeMax;
        characterChinSize.value = (characterChinSize.maxValue + characterChinSize.minValue) / 2;
        characterchinPronounced.minValue = prefab.apperanceChinPronouncedMin;
        characterchinPronounced.maxValue = prefab.apperanceChinPronouncedMax;
        characterchinPronounced.value = (characterchinPronounced.maxValue + characterchinPronounced.minValue) / 2;
        characterEyeSize.minValue = prefab.apperanceEyeSizeMin;
        characterEyeSize.maxValue = prefab.apperanceEyeSizeMax;
        characterEyeSize.value = (characterEyeSize.maxValue + characterEyeSize.minValue) / 2;
        characterMouthSize.minValue = prefab.apperanceMouthSizeMin;
        characterMouthSize.maxValue = prefab.apperanceMouthSizeMax;
        characterMouthSize.value = (characterMouthSize.maxValue + characterMouthSize.minValue) / 2;
        characterLipsSize.minValue = prefab.apperanceLipsSizeMin;
        characterLipsSize.maxValue = prefab.apperanceLipsSizeMax;
        characterLipsSize.value = (characterLipsSize.maxValue + characterLipsSize.minValue) / 2;
        characterEarSize.minValue = prefab.apperanceEarSizeMin;
        characterEarSize.maxValue = prefab.apperanceEarSizeMax;
        characterEarSize.value = (characterEarSize.maxValue + characterEarSize.minValue) / 2;
        characterNoseSize.minValue = prefab.apperanceNoseSizeMin;
        characterNoseSize.maxValue = prefab.apperanceNoseSizeMax;
        characterNoseSize.value = (characterNoseSize.maxValue + characterNoseSize.minValue) / 2;
        characterNoseWidth.minValue = prefab.apperanceNoseWidthMin;
        characterNoseWidth.maxValue = prefab.apperanceNoseWidthMax;
        characterNoseWidth.value = (characterNoseWidth.maxValue + characterNoseWidth.minValue) / 2;
        characterNoseCurve.minValue = prefab.apperanceNoseCurveMin;
        characterNoseCurve.maxValue = prefab.apperanceNoseCurveMax;
        characterNoseCurve.value = (characterNoseCurve.maxValue + characterNoseCurve.minValue) / 2;

        // color dropdown
        characterSkinColor.ClearOptions();
        foreach (Colorset colorset in prefab.apperanceSkinColors)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, colorset.visible);
            texture.Apply();
            Sprite shownImage = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
            characterSkinColor.options.Add(new Dropdown.OptionData() { image = shownImage });
        }
        characterSkinColor.value = 1;
        characterSkinColor.value = 0;
        // hair
        characterHair.maxValue = Math.Max(0, prefab.apperanceHairs.Count - 1);
        characterHairColor.ClearOptions();
        foreach (Color haircolor in prefab.apperanceHairColors)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, haircolor);
            texture.Apply();
            Sprite shownImage = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
            characterHairColor.options.Add(new Dropdown.OptionData() { image = shownImage });
        }
        characterBeard.maxValue = Math.Max(0, prefab.apperanceBeards.Count - 1);
        characterBeardColor.ClearOptions();
        foreach (Color beardcolor in prefab.apperanceHairColors)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, beardcolor);
            texture.Apply();
            Sprite shownImage = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
            characterBeardColor.options.Add(new Dropdown.OptionData() { image = shownImage });
        }
        characterFangs.maxValue = Math.Max(0, prefab.apperanceFangs.Count - 1);
    }
    private void RaceAndGenderChangeFinished(UMAData arg0)
    {
        avatarUMA.CharacterCreated.RemoveListener(RaceAndGenderChangeFinished);
        Invoke("RaceAndGenderChangeReady", 0.1f);
    }
    private void RaceAndGenderChangeReady()
    {
        avatarDNA = avatarUMA.GetDNA();
        isAvatarReady = true;
        CreateRandomApperance();
        ApplyCharacterApperance();
    }

    public void TurningSpeedChanged()
    {
        MagicVibration mv = exampleChar.GetComponent<MagicVibration>();
        mv.spinY = turningSpeed.value;
        mv.ParameterUpdated();
    }
    public void BodyCameraSelected()
    {
        Camera.main.transform.position = cameraBodyLocation.position;
        Camera.main.transform.rotation = cameraBodyLocation.rotation;
        isHeadCamera = false;
        buttonHeadCamera.gameObject.SetActive(true);
        buttonBodyCamera.gameObject.SetActive(false);
    }
    public void HeadCameraSelected()
    {
        Vector3 cameraPos = cameraHeadLocation.position;
        Player currentSelected = previewChar.GetComponent<Player>();
        if (currentSelected)
        {
            if (currentSelected.isGenderMale)
            {
                headCameraHeight.value = 1.60f * characterHeigth.value + 1004.5f;
            }
            else
            {
                headCameraHeight.value = 1.72f * characterHeigth.value + 1004.2f;
            }
        }
        cameraPos.y = headCameraHeight.value;
        Camera.main.transform.position = cameraPos;
        Camera.main.transform.rotation = cameraHeadLocation.rotation;
        buttonHeadCamera.gameObject.SetActive(false);
        buttonBodyCamera.gameObject.SetActive(true);
        isHeadCamera = true;
    }
    public void HeadCameraHeightChanged()
    {
        if (isHeadCamera)
        {
            Vector3 cameraPos = cameraHeadLocation.position;
            cameraPos.y = headCameraHeight.value;
            Camera.main.transform.position = cameraPos;
        }
    }
    public void RandomizeApperance()
    {
        if (isAvatarReady)
        {
            CreateRandomApperance();
            ApplyCharacterApperance();
        }
    }

    public void CharacterHeightChanged()
    {
        apperance.height = characterHeigth.value;
        ApplyCharacterApperance();
    }
    public void CharacterFatChanged()
    {
        apperance.fat = characterFat.value;
        ApplyCharacterApperance();
    }
    public void CharacterMusclesChanged()
    {
        apperance.muscles = characterMuscles.value;
        ApplyCharacterApperance();
    }
    public void CharacterBreastSizeChanged()
    {
        apperance.breastSize = characterBreastSize.value;
        ApplyCharacterApperance();
    }
    public void CharacterGluteusSizeChanged()
    {
        apperance.gluteusSize = characterGluteusSize.value;
        ApplyCharacterApperance();
    }
    public void CharacterWaistChanged()
    {
        apperance.waist = characterWaist.value;
        ApplyCharacterApperance();
    }
    public void CharacterHeadWidthChanged()
    {
        apperance.headWidth = characterHeadWidth.value;
        ApplyCharacterApperance();
    }
    public void CharacterChinSizeChanged()
    {
        apperance.chinSize = characterChinSize.value;
        ApplyCharacterApperance();
    }
    public void CharacterchinPronouncationChanged()
    {
        apperance.chinPronounced = characterchinPronounced.value;
        ApplyCharacterApperance();
    }
    public void CharacterEyeSizeChanged()
    {
        apperance.eyeSize = characterEyeSize.value;
        ApplyCharacterApperance();
    }
    public void CharacterMouthSizeChanged()
    {
        apperance.mouthSize = characterMouthSize.value;
        ApplyCharacterApperance();
    }
    public void CharacterLipSizeChanged()
    {
        apperance.lipsSize = characterLipsSize.value;
        ApplyCharacterApperance();
    }
    public void CharacterEarSizeChanged()
    {
        apperance.earsSize = characterEarSize.value;
        ApplyCharacterApperance();
    }
    public void CharacterNoseSizeChanged()
    {
        apperance.noseSize = characterNoseSize.value;
        ApplyCharacterApperance();
    }
    public void CharacterNoseWidthChanged()
    {
        apperance.noseWidth = characterNoseWidth.value;
        ApplyCharacterApperance();
    }
    public void CharacterNoseCurveChanged()
    {
        apperance.noseCurve = characterNoseCurve.value;
        ApplyCharacterApperance();
    }
    public void CharacterSkinColorChanged()
    {
        Player currentSelected = previewChar.GetComponent<Player>();
        apperance.skinColor = currentSelected.apperanceSkinColors[characterSkinColor.value].usedForUMA;
        ApplyCharacterApperance();
    }
    public void CharacterHairChanged()
    {
        apperance.hair = (int)characterHair.value;
        ApplyCharacterApperance();
    }
    public void CharacterHairColorChanged()
    {
        Player currentSelected = previewChar.GetComponent<Player>();
        apperance.hairColor = currentSelected.apperanceHairColors[characterHairColor.value];
        ApplyCharacterApperance();
    }
    public void CharacterBeardChanged()
    {
        apperance.beard = (int)characterBeard.value;
        ApplyCharacterApperance();
    }
    public void CharacterBeardColorChanged()
    {
        Player currentSelected = previewChar.GetComponent<Player>();
        apperance.beardColor = currentSelected.apperanceHairColors[characterBeardColor.value];
        ApplyCharacterApperance();
    }
    public void CharacterFangsChanged()
    {
        apperance.fangs = (int)characterFangs.value;
        ApplyCharacterApperance();
    }

    private void ApplyCharacterApperance()
    {
        if (isAvatarReady)
        {
            // this function is almost identical tp Player.ApplyApperance
            // Change both fuctions simultaneously
            avatarDNA["height"].Set(apperance.height);
            // fat influences a number of properties
            avatarDNA["belly"].Set(apperance.fat);
            avatarDNA["upperWeight"].Set(apperance.fat);
            avatarDNA["lowerWeight"].Set(apperance.fat);
            // muscles influences a number of properties
            avatarDNA["neckThickness"].Set(apperance.muscles);
            avatarDNA["upperMuscle"].Set(apperance.muscles);
            avatarDNA["lowerMuscle"].Set(apperance.muscles);
            //arms depends on muscles & fat
            avatarDNA["armWidth"].Set(Mathf.Max(apperance.fat, apperance.muscles));
            avatarDNA["forearmWidth"].Set(Mathf.Max(apperance.fat, apperance.muscles));
            avatarDNA["breastSize"].Set(apperance.breastSize);
            avatarDNA["gluteusSize"].Set(apperance.gluteusSize);
            avatarDNA["waist"].Set(apperance.waist);
            avatarDNA["headWidth"].Set(apperance.headWidth);
            avatarDNA["chinSize"].Set(apperance.chinSize);
            avatarDNA["chinPronounced"].Set(apperance.chinPronounced);
            avatarDNA["eyeSize"].Set(apperance.eyeSize);
            avatarDNA["mouthSize"].Set(apperance.mouthSize);
            avatarDNA["lipsSize"].Set(apperance.lipsSize);
            avatarDNA["earsSize"].Set(apperance.earsSize);
            avatarDNA["noseSize"].Set(apperance.noseSize);
            avatarDNA["noseWidth"].Set(apperance.noseWidth);
            avatarDNA["noseCurve"].Set(apperance.noseCurve);

            Player currentSelected = previewChar.GetComponent<Player>();
            avatarUMA.ClearSlot("Hair");
            if (apperance.hair > 0)
            {
                avatarUMA.SetSlot("Hair", currentSelected.apperanceHairs[apperance.hair].name);
            }
            avatarUMA.ClearSlot("Beard");
            if (apperance.beard > 0)
            {
                avatarUMA.SetSlot("Beard", currentSelected.apperanceBeards[apperance.beard].name);
            }
            avatarUMA.ClearSlot("Face");
            if (currentSelected.apperanceFangs.Count > 0)
            {
                avatarUMA.SetSlot("Face", currentSelected.apperanceFangs[apperance.fangs].name);
            }
            avatarUMA.BuildCharacter();
            // colors
            avatarUMA.SetColor("Skin", apperance.skinColor);
            avatarUMA.SetColor("Hair", apperance.hairColor);
            avatarUMA.SetColor("Beard", apperance.beardColor);
            avatarUMA.UpdateColors(true);
        }
    }

    // -------- Code section ---------------
    // Abilities
    public void AbilityChanged(string name, int value)
    {
        for (int i = 0; i < abilities.count; i++)
        {
            if (name == abilities.listOfAbilities[i].name)
            {
                abilities.listOfAbilities[i].value = value;
                VerifyAbilities();
                break;
            }
        }
    }
    private void VerifyAbilities()
    {
        if (abilities.IsCorrectAssigned())
        {
            verifyAbilities.GetComponent<Image>().color = Color.green;
            verifyAbilities.transform.Find("Text").GetComponent<Text>().text = "Settings permitted!";
        }
        else
        {
            verifyAbilities.GetComponent<Image>().color = Color.red;
            verifyAbilities.transform.Find("Text").GetComponent<Text>().text = string.Format("Settings not permitted! ({0} of {1})", abilities.allocatedTotal, GlobalVar.abilityTotal);
        }
    }
    private void InitializeAbilities()
    {
        listAbilities.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 40 * abilities.count);
        foreach (Transform child in listAbilities.transform)
            Destroy(child.gameObject);

        foreach (Abilities.Ability ability in abilities.listOfAbilities)
        {
            GameObject go = Instantiate(panelAbility) as GameObject;
            go.SetActive(true);
            PanelAbility pa = go.GetComponent<PanelAbility>();
            pa.characterCreation = this;
            pa.explanationPanel = explanation;
            pa.Name(ability.name);
            pa.explanation = string.Format("<b>{0}</b>" + Environment.NewLine + Environment.NewLine + "<i>{1}</i>" + Environment.NewLine + Environment.NewLine
                + "<b>Not Available</b>" + Environment.NewLine + "{2}" + Environment.NewLine + Environment.NewLine
                + "<b>Poor</b>" + Environment.NewLine + "{3}" + Environment.NewLine + Environment.NewLine
                + "<b>Good</b>" + Environment.NewLine + "{4}" + Environment.NewLine + Environment.NewLine
                + "<b>Excellent</b>" + Environment.NewLine + "{5}"
                , ability.name
                , ability.headline
               , ability.descriptionNav
               , ability.descriptionPoor
               , ability.descriptionGood
               , ability.descriptionExcellent);
            pa.SetValue(ability.value);
            go.transform.SetParent(listAbilities.transform);
        }
        VerifyAbilities();
    }
    public void AbilityExplanation()
    {
        explanation.text = string.Format("Abilities define certain capabilities of the character." + Environment.NewLine + Environment.NewLine
            + "Every character has to have advantages and disadvantages." + Environment.NewLine + Environment.NewLine
            + "The total sum of all abilities has to be {0}" + Environment.NewLine
            + "There must be at least {1} abilities Not Available." + Environment.NewLine
            + "There must be at least {2} abilities Excellent" + Environment.NewLine + Environment.NewLine
            + "Currently there are {3} of {0} points allocated.",
            GlobalVar.abilityTotal,
            GlobalVar.abilityMinMin,
            GlobalVar.abilityMinMax,
            abilities.allocatedTotal);
    }

    // -------- Code section ---------------
    // Skills
    public void SkillChanged(int skillId, int value)
    {
        defaultSkills[skillId] = value;
        VerifyButtonSkill();

    }
    int skillCountLow = 0;
    int skillCountMedium = 0;
    int skillCountHigh = 0;
    private bool VerifySkills()
    {
        skillCountLow = 0;
        skillCountMedium = 0;
        skillCountHigh = 0;
        foreach (int value in defaultSkills)
        {
            if (value == 1)
                skillCountLow++;
            else if (value == 2)
                skillCountMedium++;
            else if (value == 3)
                skillCountHigh++;
        }
        return (skillCountLow == GlobalVar.skillDefaultLow && skillCountMedium == GlobalVar.skillDefaultMedium && skillCountHigh == GlobalVar.skillDefaultHigh);
    }
    private void VerifyButtonSkill()
    {
        if (VerifySkills())
        {
            verifySkills.GetComponent<Image>().color = Color.green;
            verifySkills.transform.Find("Text").GetComponent<Text>().text = "Settings permitted!";
        }
        else
        {
            verifySkills.GetComponent<Image>().color = Color.red;
            verifySkills.transform.Find("Text").GetComponent<Text>().text = string.Format("Settings not permitted! low:{0}({1}) medium:{2}({3}) high:{4}({5})"
                , skillCountLow, GlobalVar.skillDefaultLow, skillCountMedium, GlobalVar.skillDefaultMedium, skillCountHigh, GlobalVar.skillDefaultHigh);
        }
    }
    private void InitializeSkills()
    {
        listSkills.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 40 * defaultSkills.Length);
        foreach (Transform child in listSkills.transform)
            Destroy(child.gameObject);

        foreach (Skills.SkillInfo skill in Skills.info)
        {
            GameObject go = Instantiate(panelSkill) as GameObject;
            go.SetActive(true);
            PanelSkill pa = go.GetComponent<PanelSkill>();
            pa.characterCreation = this;
            pa.explanationPanel = explanation;
            pa.Id(skill.id);
            pa.SetValue(defaultSkills[skill.id]);
            go.transform.SetParent(listSkills.transform);
        }
        VerifyButtonSkill();
    }
    private void CreateRandomSkills()
    {
        int[] shuffled = GlobalFunc.Shuffle(Skills.maxSkills);
        int i = 0;
        while (i < GlobalVar.skillDefaultLow)
        {
            defaultSkills[shuffled[i]] = 1;
            i++;
        }
        while (i < (GlobalVar.skillDefaultLow + GlobalVar.skillDefaultMedium))
        {
            defaultSkills[shuffled[i]] = 2;
            i++;
        }
        while (i < (GlobalVar.skillDefaultLow + GlobalVar.skillDefaultMedium + GlobalVar.skillDefaultHigh))
        {
            defaultSkills[shuffled[i]] = 3;
            i++;
        }
        while (i < Skills.maxSkills)
        {
            defaultSkills[shuffled[i]] = 0;
            i++;
        }
    }
    public void SkillExplanation()
    {
        VerifySkills();
        explanation.text = string.Format("Skills define how much a character had already learned." + Environment.NewLine + Environment.NewLine
            + "Define the starting point for the character. Be aware, learning will never end." + Environment.NewLine + Environment.NewLine
            + "There must be {0} (current{3}) advanced skills where the character might even already be a journeyman." + Environment.NewLine
            + "There must be {1} (current{4}) skills at advanced apprentice level." + Environment.NewLine
            + "There must be {2} (current{5}) skills where the character know to hold the handle of the tool the correct way.",
            GlobalVar.skillDefaultHigh,
            GlobalVar.skillDefaultMedium,
            GlobalVar.skillDefaultLow,
            skillCountHigh, skillCountMedium, skillCountLow);
    }

    // -------- Code section ---------------
    //Attributes
    public void AttributeChanged(string name, int value)
    {
        for (int i = 0; i < attributes.count; i++)
        {
            if (name == attributes.listOfAttributes[i].name)
            {
                attributes.listOfAttributes[i].value = value;
                VerifyAttributes();
                break;
            }
        }
    }
    private void VerifyAttributes()
    {
        if (attributes.IsCorrectAssigned(GlobalVar.attributeTotal - GlobalVar.attributeStartPenalty))
        {
            verifyAttributes.GetComponent<Image>().color = Color.green;
            verifyAttributes.transform.Find("Text").GetComponent<Text>().text = "Settings permitted!";
        }
        else
        {
            verifyAttributes.GetComponent<Image>().color = Color.red;
            verifyAttributes.transform.Find("Text").GetComponent<Text>().text = string.Format("Settings not permitted! {0} of {1}", attributes.allocatedTotal, GlobalVar.attributeTotal - GlobalVar.attributeStartPenalty);
        }
    }
    private void InitializeAttributes()
    {
        listAttributes.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 40 * attributes.count);
        foreach (Transform child in listAttributes.transform)
            Destroy(child.gameObject);

        foreach (Attributes.Attribute attribute in attributes.listOfAttributes)
        {
            GameObject go = Instantiate(panelAttribute) as GameObject;
            go.SetActive(true);
            PanelAttribute pa = go.GetComponent<PanelAttribute>();
            pa.characterCreation = this;
            pa.explanationPanel = explanation;
            pa.Name(attribute.name);
            pa.explanation = string.Format("<b>{0}</b>" + Environment.NewLine + Environment.NewLine + "<i>{1}</i>" + Environment.NewLine + Environment.NewLine + "{2}"
                , attribute.name
                , attribute.headline
               , attribute.description);
            pa.SetValue(attribute.value);
            go.transform.SetParent(listAttributes.transform);
        }
        VerifyAttributes();
    }
    public void AttributeExplanation()
    {
        explanation.text = string.Format("Attributes control the behaviour of the character." + Environment.NewLine + Environment.NewLine
            + "The max sum of all attributes will become {0}." + Environment.NewLine
            + "Attributes are influenced neither by experience nor skills." + Environment.NewLine + Environment.NewLine
            + "A new character starts with {2} attribute points less. By playing the character for a certain amount of time, you win the extra points and can use them at will." + Environment.NewLine + Environment.NewLine
            + "Currently there are {1} of {3} points allocated.",
            GlobalVar.attributeTotal,
            attributes.allocatedTotal,
            GlobalVar.attributeStartPenalty,
            GlobalVar.attributeTotal - GlobalVar.attributeStartPenalty);
    }
    // -------- Code section ---------------
    //Starter sets
    public void StarterSetChanged(int id)
    {
        starterSetSelected = id;
        SetCurrentStarterSet();
    }
    private void InitializeStarterSets()
    {
        starterSetPanels.Clear();
        listStarterSets.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 40 * starterSets.count);
        starterSets.Reload();
        foreach (Transform child in listStarterSets.transform)
            Destroy(child.gameObject);

        foreach (StarterSet set in starterSets.listOfStarterSets)
        {
            GameObject go = Instantiate(panelStarterSet) as GameObject;
            go.SetActive(true);
            PanelStarterSet pa = go.GetComponent<PanelStarterSet>();
            pa.characterCreation = this;
            pa.explanationPanel = explanation;
            pa.Name(set.name);
            pa.explanation = string.Format("<b>{0}</b>" + Environment.NewLine + Environment.NewLine + "<i>{1}</i>" + Environment.NewLine + Environment.NewLine + "{2}"
                + Environment.NewLine + Environment.NewLine + "The purse has {3}."
                + Environment.NewLine + Environment.NewLine + "{4} equipped items including the backpack."
                , set.name
                , set.headline
                , set.description
                , Money.MoneyText(set.money)
                , set.defaultItems.Count);
            pa.SetValue(0);
            pa.id = set.identifier;
            go.transform.SetParent(listStarterSets.transform);
            starterSetPanels.Add(pa);
        }
        SetCurrentStarterSet();
    }
    private void SetCurrentStarterSet()
    {
        foreach (PanelStarterSet pa in starterSetPanels)
        {
            if (pa.id == starterSetSelected)
                pa.SetValue(1);
            else
                pa.SetValue(0);
        }
    }
    public void StarterSetExplanation()
    {
        explanation.text = string.Format("<i>{0}</i>" + Environment.NewLine + Environment.NewLine + "{1}",
            starterSets.headline,
            starterSets.description);
    }

    // -------- Code section ---------------
    //UinqueItems
    private void InitializeUniqueItems()
    {
        uniqueItemsDropdown.options.Clear();
        uniqueItemsDropdown.options.Add(new Dropdown.OptionData() { text = "nothing" });
        foreach (UniqueStarterItems.UniqueStarterItem usi in uniqueStarterItems.listOfItems)
        {
            uniqueItemsDropdown.options.Add(new Dropdown.OptionData() { text = usi.name });
        }
        uniqueItemsDropdown.value = 1;
        uniqueItemsDropdown.value = 0;
    }
    public void UniqueItemDropdownSelected()
    {
        string selectedItem = uniqueItemsDropdown.options[uniqueItemsDropdown.value].text;
        foreach (UniqueStarterItems.UniqueStarterItem usi in uniqueStarterItems.listOfItems)
        {
            if (selectedItem == usi.name)
            {
                explanation.text = string.Format("<i>{0}</i>" + Environment.NewLine + Environment.NewLine + "{1}",
                    usi.headline,
                    usi.description);
                return;
            }
        }
        UniqueItemExplanation();
    }
    public void UniqueItemCreate()
    {
        string selectedItem = uniqueItemsDropdown.options[uniqueItemsDropdown.value].text;
        foreach (UniqueStarterItems.UniqueStarterItem usi in uniqueStarterItems.listOfItems)
        {
            if (selectedItem == usi.name)
            {
                UniqueItemPanelCreate(usi);
                if (UniqueItemsCount() < GlobalVar.uniqueStartItemsMax)
                    buttonAddUniqueItem.interactable = true;
                else
                    buttonAddUniqueItem.interactable = false;
                return;
            }
        }

    }

    public void UniqueItemRemoved(int id)
    {
        buttonAddUniqueItem.interactable = true;
    }
    private int UniqueItemsCount()
    {
        int iCount = 0;
        foreach (Transform child in listUniqueItems.transform)
        {
            iCount++;
        }
        return iCount;
    }
    private bool UniqeItemVerification()
    {
        bool isVerified = true;
        foreach (Transform child in listUniqueItems.transform)
        {
            GameObject go = child.gameObject;
            PanelUniqueItem pa = go.GetComponent<PanelUniqueItem>();
            if (pa.GetItemName().Length < GlobalVar.uniqueStartItemsMinNameLength)
                isVerified = false;
        }
        return isVerified;
    }

    private List<UniqueStarterItems.UniqueStarterItem> UniqueItemsContent()
    {
        List<UniqueStarterItems.UniqueStarterItem> uniqueStarterItemsSelected = new List<UniqueStarterItems.UniqueStarterItem>();

        foreach (Transform child in listUniqueItems.transform)
        {
            GameObject go = child.gameObject;
            PanelUniqueItem pa = go.GetComponent<PanelUniqueItem>();
            UniqueStarterItems.UniqueStarterItem usi = new UniqueStarterItems.UniqueStarterItem()
            {
                identifier = pa.id,
                itemName = pa.GetItemName(),
                itemDescription = pa.GetItemDescription()
            };
            uniqueStarterItemsSelected.Add(usi);
        }
        return uniqueStarterItemsSelected;
    }
    private void UniqueItemPanelCreate(UniqueStarterItems.UniqueStarterItem item)
    {
        GameObject go = Instantiate(panelUniqueItem) as GameObject;
        go.SetActive(true);
        PanelUniqueItem pa = go.GetComponent<PanelUniqueItem>();
        pa.characterCreation = this;
        pa.explanationPanel = explanation;
        pa.ItemType(item.name);
        pa.id = item.identifier;
        pa.explanation = string.Format("<b>{0}</b>" + Environment.NewLine + Environment.NewLine + "<i>{1}</i>" + Environment.NewLine + Environment.NewLine + "{2}"
            , item.name
            , item.headline
           , item.description);
        go.transform.SetParent(listUniqueItems.transform);
        UniqueStarterItems.UniqueStarterItem nus = new UniqueStarterItems.UniqueStarterItem
        {
            identifier = item.identifier,
            name = item.name,
            headline = item.headline,
            description = item.description
        };
    }
    public void UniqueItemExplanation()
    {
        explanation.text = string.Format("<i>{0}</i>" + Environment.NewLine + Environment.NewLine + "{1}",
            uniqueStarterItems.headline,
            uniqueStarterItems.description);
    }

    // -------- Code section ---------------
    //Verification

    private void InitializeVerification()
    {
        bool isSuccessVerificaton = true;
        if (GlobalFunc.IsAllowedDisplayedName(displayedNameInput.text))
        {
            verifyTextDisplayedName.text = "Displayed name: " + displayedNameInput.text;
            verifyTextDisplayedName.color = colorTrue;
        }
        else
        {
            verifyTextDisplayedName.text = "Displayed name not permitted: \"" + displayedNameInput.text + "\"";
            verifyTextDisplayedName.color = colorFalse;
            isSuccessVerificaton = false;
        }
        if (classDropdown.options[classDropdown.value].text == "nothing")
        {
            verifyTextApperance.text = "No apperance selected.";
            verifyTextApperance.color = colorFalse;
        }
        else
        {
            verifyTextApperance.text = "Apperance \"" + classDropdown.options[classDropdown.value].text + "\" selected.";
            verifyTextApperance.color = colorTrue;
        }
        if (attributes.IsCorrectAssigned(GlobalVar.attributeTotal - GlobalVar.attributeStartPenalty))
        {
            verifyTextAttributes.text = string.Format("Attribute settings ok! ({0} of {0} assigned)", attributes.allocatedTotal);
            verifyTextAttributes.color = colorTrue;
        }
        else
        {
            verifyTextAttributes.text = string.Format("Attribute settings not permitted! ({0} of {1} assigned)", attributes.allocatedTotal, GlobalVar.attributeTotal - GlobalVar.attributeStartPenalty);
            verifyTextAttributes.color = colorFalse;
            isSuccessVerificaton = false;
        }
        if (abilities.IsCorrectAssigned())
        {
            verifyTextAbilities.text = string.Format("Abilities settings ok! ({0} of {0} assigned)", abilities.allocatedTotal);
            verifyTextAbilities.color = colorTrue;
        }
        else
        {
            verifyTextAbilities.text = string.Format("Ablility settings not permitted! ({0} of {1} assigned)", abilities.allocatedTotal, GlobalVar.abilityTotal);
            verifyTextAbilities.color = colorFalse;
            isSuccessVerificaton = false;
        }
        if (VerifySkills())
        {
            verifyTextSkill.text = "Skill settings ok!";
            verifyTextSkill.color = colorTrue;
        }
        else
        {
            verifyTextSkill.text = string.Format("Skill settings not permitted! low:{0}({1}) medium:{2}({3}) high:{4}({5})"
                , skillCountLow, GlobalVar.skillDefaultLow, skillCountMedium, GlobalVar.skillDefaultMedium, skillCountHigh, GlobalVar.skillDefaultHigh);
            verifyTextSkill.color = colorFalse;
            isSuccessVerificaton = false;
        }
        int starterSetId = starterSets.listOfStarterSets.FindIndex(x => x.identifier == starterSetSelected);
        verifyTextStarterSet.text = string.Format("Starter equipment set \"{0}\" selected.", starterSets.listOfStarterSets[starterSetId].name);
        if (!UniqeItemVerification())
        {
            verifyTextSpecialItems.text = string.Format("At least one unique missed a name with at least {0} characters.", GlobalVar.uniqueStartItemsMinNameLength);
            verifyTextSpecialItems.color = colorFalse;
            isSuccessVerificaton = false;
        }
        else if (UniqueItemsCount() < GlobalVar.uniqueStartItemsMax)
        {
            verifyTextSpecialItems.text = string.Format("{0} of {1} possible unique items selected.", UniqueItemsCount(), GlobalVar.uniqueStartItemsMax);
            verifyTextSpecialItems.color = colorMaybe;
        }
        else
        {
            verifyTextSpecialItems.text = string.Format("All ({1}) possible unique items selected.", UniqueItemsCount(), GlobalVar.uniqueStartItemsMax);
            verifyTextSpecialItems.color = colorTrue;
        }

        createButton.interactable = isSuccessVerificaton;
    }

    public void CreateCharacterSelected()
    {
        CharacterCreateMsg message = new CharacterCreateMsg
        {
            displayedName = displayedNameInput.text,
            classIndex = classDropdown.value,
            attributes = this.attributes.CreateString(),
            abilities = this.abilities.CreateString(),
            apperance = this.apperance.CreateString(),
            starterSet = this.starterSetSelected,
            skills = Skills.SerializeDefaultSkills(defaultSkills),
            uniqueItems = uniqueStarterItems.Serialize(UniqueItemsContent())
        };
        manager.client.Send(CharacterCreateMsg.MsgId, message);
        Hide();
    }
    public void VerificationExplanation()
    {
        explanation.text = "<b>You are almost done</b>" + Environment.NewLine + Environment.NewLine
            + "If some of the settings are not permitted please go back and correct accordingly." + Environment.NewLine
            + "Currently it is possible the character name or displayed character name exists in the data base already. If you get that message please recall character creation. your settings are available." + Environment.NewLine + Environment.NewLine
            + "<b>Enjoy the role-play on Anega.</b>";
    }
}