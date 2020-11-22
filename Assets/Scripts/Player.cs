/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// All player logic was put into this class. We could also split it into several
// smaller components, but this would result in many GetComponent calls and a
// more complex syntax.
//
// The class also takes care of selection handling, which detects 3D world
// clicks and then targets/navigates somewhere/interacts with someone.
//
// Animations are not handled by the NetworkAnimator because it's still very
// buggy and because it can't really react to movement stops fast enough, which
// results in moonwalking. Not synchronizing animations over the network will
// also save us bandwidth
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using System;
using System.Linq;
using System.Collections.Generic;
using UMA;
using UMA.CharacterSystem;

public enum TradeStatus { Free, Locked, Accepted }
public enum CraftingState { None, InProgress, Success, Failed }
[Serializable]
public struct SpellbarEntry
{
    public string reference;
    public KeyCode hotKey;
}
[Serializable]
public struct EquipmentInfo
{
    public string info;
    public string requiredCategory;
    public bool isVisible;
    public Transform location;
}
[Serializable]
public struct Colorset
{
    public Color visible;
    public Color usedForUMA;
}

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Chat))]
[RequireComponent(typeof(NetworkName))]
public partial class Player : Entity
{
    [Header("Components")]
    public Chat chat;
    public Camera avatarCamera;
    public NetworkNavMeshAgentRubberbanding rubberbanding;
    public GameObject minimapMarker;
    public GameObject body;
    [Header("Icons")]
    public Sprite classIcon; // for character selection

    [SyncVar] public string displayName = "";

    [Header("ChatDistances")]
    public float distanceWhisper = 2.5f;
    public float distanceNormal = 20.0f;
    public float distanceLoud = 40.0f;
    public float distanceShout = 80.0f;
    public float distanceDetectionPerson = 80.0f;
    [Header("Abilities & Attributes")]
    [SyncVar] public string abilitiesSync;
    public Abilities abilities = new Abilities();
    [SyncVar] public string attributesSync;
    public Attributes attributes = new Attributes();
    [SyncVar] public string apperanceSync;
    public Apperance apperance = new Apperance();
    public bool isGenderMale;
    [Header("Random Divergence")]
    public float divergenceCompass = 0.0f;
    public float divergenceClock = 0.0f;
    public float divergenceExamination = 0.0f;
    public float divergenceWeight = 1.0f;
    public float divergencePrice = 0.0f;
    [Header("Tests visible")]
    [SyncVar] public bool canSeeSky = true;
    [SyncVar] public string currentArea = "Start";
    private string lastButOneArea = "Start";
    [Header("Faction")]
    public int faction = 0;
    [Header("Apperance Limits")]
    public RaceSpecification raceSpecification;
    public float apperanceHeightMin = 0.5f;
    public float apperanceHeightMax = 0.9f;
    public float apperanceFatMin = 0.0f;
    public float apperanceFatMax = 1.0f;
    public float apperanceMusclesMin = 0.0f;
    public float apperanceMusclesMax = 1.0f;
    public float apperanceBreastSizeMin = 0.0f;
    public float apperanceBreastSizeMax = 1.0f;
    public float apperanceGluteusSizeMin = 0.0f;
    public float apperanceGluteusSizeMax = 1.0f;
    public float apperanceWaistMin = 0.0f;
    public float apperanceWaistMax = 1.0f;
    public List<Colorset> apperanceSkinColors;
    public List<UMATextRecipe> apperanceHairs;
    public List<UMATextRecipe> apperanceBeards;
    public List<Color> apperanceHairColors;
    public List<UMATextRecipe> apperanceFangs;
    public float apperanceHeadWidthMin = 0f;
    public float apperanceHeadWidthMax = 1f;
    public float apperanceChinSizeMin = 0f;
    public float apperanceChinSizeMax = 1f;
    public float apperanceChinPronouncedMin = 0f;
    public float apperanceChinPronouncedMax = 1f;
    public float apperanceEyeSizeMin = 0f;
    public float apperanceEyeSizeMax = 1f;
    public float apperanceMouthSizeMin = 0f;
    public float apperanceMouthSizeMax = 1f;
    public float apperanceLipsSizeMin = 0f;
    public float apperanceLipsSizeMax = 1f;
    public float apperanceEarSizeMin = 0f;
    public float apperanceEarSizeMax = 1f;
    public float apperanceNoseSizeMin = 0f;
    public float apperanceNoseSizeMax = 1f;
    public float apperanceNoseWidthMin = 0f;
    public float apperanceNoseWidthMax = 1f;
    public float apperanceNoseCurveMin = 0f;
    public float apperanceNoseCurveMax = 1f;
    //Specific factors
    private float[] speedOnArea = GlobalVar.areaSpeed;

    // invisible UMA content
    private DynamicCharacterAvatar UMAAvatar;
    private bool isUMAAvatarInitilized = false;

    // LastButOne
    private float lastButOneRotationY = 0;
    private int lastButOneState = 0;
    private string lastButOneAnimationType = "";

    // some meta info
    public int id
    {
        get
        {
            return IdFromName(name);
        }
    }
    public static string NameFromId(int id)
    {
        return "player" + id.ToString();
    }
    public static int IdFromName(string nameText)
    {
        if (nameText.Length < 7)
            return 0;
        else
            return nameText.Substring(6).ToInt();
    }
    [HideInInspector] public string account = "";
    [HideInInspector] public string className = "";
    [Header("Inventory Handling")]
    public int splitValue = 1;

    // localPlayer singleton for easier access from UI scripts etc.
    public static Player localPlayer;
    public void InitializeCharacter(bool equipPlayer = true)
    {
        apperance.ConvertFromString(apperanceSync);
        attributes.CreateFromString(attributesSync);
        ApplyAttributes();
        abilities.CreateFromString(abilitiesSync);
        ApplyAbilities();
        ApplyGM();
        UMAAvatar = GetComponent<DynamicCharacterAvatar>();
        if (equipPlayer)
        {
            if (isUMAAvatarInitilized)
            {
                InitializeEquipment();
            }
            else
            {
                UMAAvatar.CharacterCreated.AddListener(UMACharacterCreated);
            }
        }
    }
    void UMACharacterCreated(UMAData arg0)
    {
        isUMAAvatarInitilized = true;
        Invoke("UMACharacterCreatedFinished", 0.1f);
    }
    void UMACharacterCreatedFinished()
    {
        ApplyApperance();
        InitializeEquipment();
    }
    private void InitializeEquipment()
    {
        foreach (ItemSlot itemSlot in inventory)
        {
            if (itemSlot.amount > 0 && itemSlot.container == GlobalVar.containerEquipment)
            {
                // OnEquipmentChanged won't be called unless spawned, we
                // need to refresh manually
                RefreshLocation(itemSlot.slot);
            }
        }
    }

    public void InitializeLocalPlayer()
    {
        if (attributes.isDefault)
            Invoke("InitializeLocalPlayer", GlobalVar.repeatInitializationAttempt);
        else
        {
            distanceDetectionPerson = NonLinearCurves.FloatFromCurvePosition(GlobalVar.distanceDetectionPersonNonlinear, attributes.perception, 0, 20, GlobalVar.distanceDetectionPersonMin, GlobalVar.distanceDetectionPersonMax);
            if (!InitializeKnownNames())
                Invoke("InitializeLocalPlayer", GlobalVar.repeatInitializationAttempt);
        }
    }
    public void ApplyApperance(Apperance newApperance = null)
    {
        if (newApperance != null)
            apperance = newApperance;

        Dictionary<string, DnaSetter> avatarDNA;
        avatarDNA = UMAAvatar.GetDNA();

        avatarDNA["height"].Set(apperance.height);
        // fat influences a number of properties
        avatarDNA["belly"].Set(apperance.fat);
        avatarDNA["upperWeight"].Set(apperance.fat);
        avatarDNA["lowerWeight"].Set(apperance.fat);
        // muscles influences a number of properties
        avatarDNA["neckThickness"].Set(apperance.muscles);
        avatarDNA["upperMuscle"].Set(apperance.muscles);
        avatarDNA["lowerMuscle"].Set(apperance.muscles);
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
        UMAAvatar.ClearSlot("Hair");
        if (apperance.hair > 0)
        {
            UMAAvatar.SetSlot("Hair", apperanceHairs[apperance.hair].name);
        }
        UMAAvatar.ClearSlot("Beard");
        if (apperance.beard > 0)
        {
            UMAAvatar.SetSlot("Beard", apperanceBeards[apperance.beard].name);
        }
        if (apperanceFangs.Count > 0)
        {
            UMAAvatar.SetSlot("Face", apperanceFangs[apperance.fangs].name);
        }
        UMAAvatar.BuildCharacter();
        // colors
        UMAAvatar.SetColor("Skin", apperance.skinColor);
        UMAAvatar.SetColor("Hair", apperance.hairColor);
        UMAAvatar.SetColor("Beard", apperance.beardColor);
        UMAAvatar.UpdateColors(true);

    }
    public void ApplyAttributes()
    {
        healthMaxPlayer = NonLinearCurves.IntFromCurvePosition(GlobalVar.healtNonlinear, attributes.constitution, 0, 20, GlobalVar.healthMin, GlobalVar.healthMax);
        healthRecoveryPlayer = NonLinearCurves.IntFromCurvePosition(GlobalVar.healthRecoveryNonlinear, attributes.healthiness, 0, 20, GlobalVar.healthRecoveryMin, GlobalVar.healthRecoveryMax);
        manaMaxPlayer = NonLinearCurves.IntFromCurvePosition(GlobalVar.healtNonlinear, attributes.essence, 0, 20, GlobalVar.manaMin, GlobalVar.manaMax);
        manaRecoveryPlayer = NonLinearCurves.IntFromCurvePosition(GlobalVar.manaRecoveryNonlinear, attributes.magicAwareness, 0, 20, GlobalVar.manaRecoveryMin, GlobalVar.manaRecoveryMax);
        speedWalkPlayer = NonLinearCurves.FloatFromCurvePosition(GlobalVar.speedWalkNonlinear, attributes.agility, 0, 20, GlobalVar.speedWalkMin, GlobalVar.speedWalkMax);
        speedWalkPlayerByTwo = speedWalkPlayer * speedWalkPlayer;
        speedRunPlayer = NonLinearCurves.FloatFromCurvePosition(GlobalVar.speedRunNonlinear, attributes.agility, 0, 20, GlobalVar.speedRunMin, GlobalVar.speedRunMax);
        staminaMaxPlayer = NonLinearCurves.IntFromCurvePosition(GlobalVar.staminaNonlinear, attributes.endurance, 0, 20, GlobalVar.staminaMin, GlobalVar.staminaMax);
        staminaRecoveryPlayer = NonLinearCurves.IntFromCurvePosition(GlobalVar.staminaRecoveryNonlinear, attributes.fitness, 0, 20, GlobalVar.staminaRecoveryMin, GlobalVar.staminaRecoveryMax);
        staminaLimit = (speedWalkPlayer * speedWalkPlayer + speedRunPlayer * speedRunPlayer) / 2;
        weightMaxPlayer = NonLinearCurves.IntFromCurvePosition(GlobalVar.liftingCapacityNonlinear, attributes.strength, 0, 20, GlobalVar.liftingCapacityMin, GlobalVar.liftingCapacityMax);
        detailViewMin = NonLinearCurves.FloatFromCurvePosition(GlobalVar.cameraFieldOfViewNonLinear, attributes.perception, 0, 20, GlobalVar.cameraFieldOfViewMax, GlobalVar.cameraFieldOfViewMin);
        throwingDistanceMaxPlayer = NonLinearCurves.FloatFromCurvePosition(GlobalVar.throwingDistanceMaxInfluenceNonlinear, attributes.strength, 0, 20, GlobalVar.throwingDistanceMaxMin, GlobalVar.throwingDistanceMaxMax);
    }

    private void ApplyAbilities()
    {
        // Chat distances - Voice Control
        distanceWhisper = GlobalVar.voiceControl[abilities.voiceControl, 0];
        distanceNormal = GlobalVar.voiceControl[abilities.voiceControl, 1];
        distanceLoud = GlobalVar.voiceControl[abilities.voiceControl, 2];
        distanceShout = GlobalVar.voiceControl[abilities.voiceControl, 3];
        // inventorySize - Storekeeper
        inventorySize = GlobalVar.storekeeperInventorySize[abilities.storekeeper];
        // can walk in shallow water?
        agent.areaMask = GlobalFunc.SetBit(agent.areaMask, GlobalVar.navMeshAreaShallowWater, abilities.waterproof != Abilities.Nav);
        agent.areaMask = GlobalFunc.SetBit(agent.areaMask, GlobalVar.navMeshAreaDeepWater, abilities.waterproof >= Abilities.Good);
        speedOnArea[GlobalVar.navMeshAreaShallowWater] = GlobalVar.waterproofSpeed[abilities.waterproof];
        speedOnArea[GlobalVar.navMeshAreaDeepWater] = GlobalVar.deepWaterSpeed[abilities.waterproof];
        speedOnArea[GlobalVar.navMeshAreaRoad] = GlobalVar.roadRunnerSpeed[abilities.roadrunner];
    }

    private void ApplyGM()
    {
        isGM = GameMaster.isGM(gmState);
        if (isGM)
        {
            bool isGod = GameMaster.isGod(gmState);
            gmDisplay = GameMaster.showGmInOverlay(gmState);
            gmKnowNames = GameMaster.knowNames(gmState) || isGod;
            gmUnlimitedHealth = GameMaster.unlimitedHealth(gmState) || isGod; ;
            gmUnlimitedMana = GameMaster.unlimitedMana(gmState) || isGod; ;
            gmUnlimitedStamina = GameMaster.unlimitedStamina(gmState) || isGod; ;
            //>>> temporary for test
            gmUnlimitedHealth = false;
            gmUnlimitedMana = false;
            gmUnlimitedStamina = false;
            //>>> endtest
        }
        else
        {
            gmState = GameMaster.EmptySyncString;
            gmDisplay = false;
            gmKnowNames = false;
            gmUnlimitedHealth = false;
            gmUnlimitedMana = false;
            gmUnlimitedStamina = false;
        }
    }

    // health
    public override int healthMaxBase
    {
        get
        {
            return healthMaxPlayer;
        }
    }
    public override int healthMax
    {
        get
        {
            // calculate equipment bonus
            int equipmentBonus = 0;
            //(from slot in inventory
            //                      where GlobalFunc.IsEquipment(slot.container, slot.slot)
            //                      select ((EquipmentItem)slot.item.data).healthBonus).Sum();

            // player +  equip
            return base.healthMax + equipmentBonus;
        }
    }
    public override int healthRecoveryRateBase
    {
        get
        {
            return healthRecoveryPlayer;
        }
    }
    public override int healthRecoveryRate
    {
        get
        {
            // calculate equipment bonus //Anega// add related bonus to equipment
            int equipmentBonus = 0;
            //(from slot in equipment
            //                      where slot.amount > 0
            //                      select ((EquipmentItem)slot.item.data).healthBonus).Sum();

            // entity + equip
            return base.healthRecoveryRate + equipmentBonus;
        }
    }

    // mana
    public override int manaMaxBase
    {
        get
        {
            return manaMaxPlayer;
        }
    }
    public override int manaMax
    {
        get
        {
            // calculate equipment bonus
            int equipmentBonus = 0;
            //(from slot in inventory
            //                      where GlobalFunc.IsEquipment(slot.container, slot.slot)
            //                      select ((EquipmentItem)slot.item.data).manaBonus).Sum();
            float buffBonus = buffs.Sum(buff => buff.bonusMana);
            //            Debug.Log(">>> m buff:" + buffBonus);
            // player + buff + equip
            return (int)(manaMaxBase * manaWandBonus + (int)buffBonus + equipmentBonus);
        }
    }
    public override int manaRecoveryRateBase
    {
        get
        {
            return manaRecoveryPlayer;
        }
    }
    public override int manaRecoveryRate
    {
        get
        {
            // calculate equipment bonus //Anega// add related bonus to equipment
            int equipmentBonus = 0;
            //(from slot in equipment
            //                      where slot.amount > 0
            //                      select ((EquipmentItem)slot.item.data).healthBonus).Sum();
            float buffBonus = buffs.Sum(buff => buff.bonusManaPerSecond);
            // player + buff + equip
            return (int)((manaRecoveryRateBase * manaRecoveryWandBonus + buffBonus + equipmentBonus) * Universal.GetAmbientMana(transform.position, currentArea));
        }
    }

    // speed
    public bool isSwimming;

    public override float speedBase
    {
        get
        {
            return speedWalkPlayer;
        }
    }
    public override float speed
    {
        get
        {
            float currentSpeed;
            int areaIndex = GlobalVar.navMeshAreaWalkable;
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                areaIndex = GlobalFunc.FirstBitPosition(hit.mask);
            }
            if ((Input.GetKey(PlayerPreferences.keyRun1) || Input.GetKey(PlayerPreferences.keyRun2)) && stamina >= 0)
            {
                // speed buff on running and walking only
                float buffBonus = buffs.Sum(buff => buff.bonusSpeed);

                currentSpeed = speedRunPlayer * speedWeightInfluence + buffBonus;
            }
            else if (Input.GetKey(PlayerPreferences.keyCreep1) || Input.GetKey(PlayerPreferences.keyCreep2))
            {
                currentSpeed = PlayerPreferences.creepSpeed * speedWalkPlayer * speedWeightInfluence;
            }
            else
            {
                // speed buff on running and walking only
                float buffBonus = buffs.Sum(buff => buff.bonusSpeed);

                currentSpeed = speedWalkPlayer * speedWeightInfluence + buffBonus;
            }
            currentSpeed = currentSpeed * speedOnArea[areaIndex];
            isSwimming = areaIndex == GlobalVar.navMeshAreaDeepWater;

            return currentSpeed;
            //// mount speed if mounted, regular speed otherwise
            //return activeMount != null && activeMount.health > 0 ? activeMount.speed : base.speed;
        }
    }

    [Header("Attribute driven values")]
    [SyncVar] public int healthMaxPlayer = 100000;
    [SyncVar] public int healthRecoveryPlayer = 1;
    [SyncVar] public int manaMaxPlayer = 100000;
    [SyncVar] public int manaRecoveryPlayer = 1;
    [SyncVar] public float speedWalkPlayer = 2;
    float speedWalkPlayerByTwo = 4;
    [SyncVar] public float speedRunPlayer = 6;
    [SyncVar] public int staminaMaxPlayer = 100000;
    [SyncVar] public int staminaRecoveryPlayer = 1;
    float staminaLast = 0;
    float staminaLimit = 40;
    float staminaConsumption = 0;
    [SyncVar] public int weightMaxPlayer = 50000;
    float speedWeightInfluence = 1;
    [SyncVar] public float detailViewMin = 58;
    float throwingDistanceMaxPlayer = 2;

    [Header("Bonus values")]
    [SyncVar] public float manaWandBonus = 1;
    [SyncVar] public float manaRecoveryWandBonus = 1;

    //GM can read and write always
    public int readAndWrite
    {
        get { return (isGM ? Abilities.Excellent : abilities.readAndWrite); }
    }

    //GM can see in dark always
    public int darkVision
    {
        get { return ((isGM && !webcamActive) ? Abilities.Excellent : abilities.darkVision); }
    }
    //GM can see in dark always
    public int handscale
    {
        get { return (isGM ? Abilities.Excellent : abilities.handscale); }
    }

    [Header("Skill")]
    public SyncListSkill skills = new SyncListSkill();
    // this value is needed on server only, GM needs to know, so we must sync
    [SyncVar, SerializeField] long _skillTotalTime = 0;
    public long skillTotalTime
    {
        get { return _skillTotalTime; }
        set { _skillTotalTime = Math.Max(value, 1); }
    }

    [Header("Play time")] // note: int is not enough (can have > 2 mil. easily)
    [SyncVar, SerializeField] long _playtime = 0;
    [SerializeField] int activityLevel = 0;
    [SerializeField] Vector3 lastPosition = new Vector3(0, 0, 0);
    [SerializeField] Quaternion lastRotation = new Quaternion(0, 0, 0, 0);

    public long playtime
    {
        get { return _playtime; }
        set { _playtime = Math.Max(value, 0); }
    }

    [Header("GM")]
    [SyncVar] public string gmState = GameMaster.EmptySyncString;
    // seperate variables for part of gmState since they are used in update cycle
    // reduce load
    public bool isGM = false;
    public bool gmDisplay = false;
    public bool gmKnowNames = false;
    public int gmNpcKillCount = 0;
    public int gmNpcPullCount = 0;
    public bool gmUnlimitedHealth = false;
    public bool gmUnlimitedMana = false;
    public bool gmUnlimitedStamina = false;
    [SyncVar] public bool isGmInvisible = false;

    [Header("Indicator")]
    public GameObject indicatorPrefab;
    public GameObject rangeDisplayPrefab;
    [HideInInspector] public GameObject indicator;

    [Header("Inventory")]
    public int inventorySize = 30;
    //public ScriptableItem[] defaultItems;
    public KeyCode[] inventorySplitKeys = { KeyCode.LeftShift, KeyCode.RightShift };

    [Header("Equipment Info")]
    public EquipmentInfo[] equipmentInfo = {
        new EquipmentInfo{info="left Hand",requiredCategory="Hand", isVisible=true,  location=null},
        new EquipmentInfo{info="right Hand",requiredCategory="Hand", isVisible=true,  location=null},
        new EquipmentInfo{info="Head",requiredCategory="Head", isVisible=true,  location=null},
        new EquipmentInfo{info="Shoulder",requiredCategory="Shoulder", isVisible=true,  location=null},
        new EquipmentInfo{info="Chest",requiredCategory="Chest", isVisible=true,  location=null},
        new EquipmentInfo{info="Arms",requiredCategory="Arms", isVisible=true,  location=null},
        new EquipmentInfo{info="Legs",requiredCategory="Legs", isVisible=true,  location=null},
        new EquipmentInfo{info="Feet",requiredCategory="Feet", isVisible=true,  location=null},
        new EquipmentInfo{info="Neck",requiredCategory="Neck", isVisible=false,  location=null},
        new EquipmentInfo{info="Finger1",requiredCategory="Finger", isVisible=false, location=null},
        new EquipmentInfo{info="Finger2",requiredCategory="Finger", isVisible=false, location=null},
        new EquipmentInfo{info="Finger3",requiredCategory="Finger", isVisible=false, location=null},
        new EquipmentInfo{info="Finger4",requiredCategory="Finger", isVisible=false, location=null},
        new EquipmentInfo{info="Finger5",requiredCategory="Finger", isVisible=false, location=null},
        new EquipmentInfo{info="Finger6",requiredCategory="Finger", isVisible=false, location=null},
        new EquipmentInfo{info="Belt1",requiredCategory="", isVisible=false, location=null},
        new EquipmentInfo{info="Belt2",requiredCategory="", isVisible=false, location=null},
        new EquipmentInfo{info="Belt3",requiredCategory="", isVisible=false, location=null},
        new EquipmentInfo{info="Belt4",requiredCategory="", isVisible=false, location=null},
        new EquipmentInfo{info="Belt5",requiredCategory="", isVisible=false, location=null},
        new EquipmentInfo{info="Belt6",requiredCategory="", isVisible=false, location=null},
        new EquipmentInfo{info="Backpack",requiredCategory="", isVisible=false, location=null}
    };

    [Header("Spellbar")]
    public SpellbarEntry[] spellbar = {
        new SpellbarEntry{reference="", hotKey=KeyCode.Alpha1},
        new SpellbarEntry{reference="", hotKey=KeyCode.Alpha2},
        new SpellbarEntry{reference="", hotKey=KeyCode.Alpha3},
        new SpellbarEntry{reference="", hotKey=KeyCode.Alpha4},
        new SpellbarEntry{reference="", hotKey=KeyCode.Alpha5},
        new SpellbarEntry{reference="", hotKey=KeyCode.Alpha6},
        new SpellbarEntry{reference="", hotKey=KeyCode.Alpha7},
        new SpellbarEntry{reference="", hotKey=KeyCode.Alpha8},
        new SpellbarEntry{reference="", hotKey=KeyCode.Alpha9},
        new SpellbarEntry{reference="", hotKey=KeyCode.Alpha0},
    };

    [Header("Quests")] // contains active and completed quests (=all)
    public int activeQuestLimit = 10;
    public SyncListQuest quests = new SyncListQuest();

    [Header("Interaction")]
    public float interactionRange = 4;
    public bool localPlayerClickThrough = true; // click selection goes through localplayer. feels best.

    [Header("PvP")]
    public BuffSpell offenderBuff;
    public BuffSpell murdererBuff;

    [Header("Trading")]
    [SyncVar, HideInInspector] public string tradeRequestFrom = "";
    [SyncVar, HideInInspector] public TradeStatus tradeStatus = TradeStatus.Free;
    public SyncListInt tradeOfferItems = new SyncListInt(); // inventory indices

    [Header("Crafting")]
    public List<int> craftingIndices = Enumerable.Repeat(-1, ScriptableRecipe.recipeSize).ToList();
    [HideInInspector] public CraftingState craftingState = CraftingState.None; // // client sided
    [SyncVar, HideInInspector] public double craftingTimeEnd; // double for long term precision

    [Header("Guild")]
    [SyncVar, HideInInspector] public string guildName = ""; // syncvar so that all observers see it
    [SyncVar, HideInInspector] public string guildInviteFrom = "";
    [SyncVar, HideInInspector] public Guild guild; // TODO SyncToOwner later
    public float guildInviteWaitSeconds = 3;

    [Header("Party")]
    [HideInInspector] public Party party;
    [SyncVar, HideInInspector] public string partyInviteFrom = "";
    public float partyInviteWaitSeconds = 3;


    // 'Pet' can't be SyncVar so we use [SyncVar] GameObject and wrap it
    [Header("Pet")]
    [SyncVar] GameObject _activePet;
    public Pet activePet
    {
        get { return _activePet != null ? _activePet.GetComponent<Pet>() : null; }
        set { _activePet = value != null ? value.gameObject : null; }
    }

    // pet's destination should always be right next to player, not inside him
    // -> we use a helper property so we don't have to recalculate it each time
    // -> we offset the position by exactly 1 x bounds to the left because dogs
    //    are usually trained to walk on the left of the owner. looks natural.
    public Vector3 petDestination
    {
        get
        {
            Bounds bounds = collider.bounds;
            return transform.position - transform.right * bounds.size.x;
        }
    }

    [Header("Mount")]
    public Transform meshToOffsetWhenMounted;
    public float seatOffsetY = -1;

    // 'Mount' can't be SyncVar so we use [SyncVar] GameObject and wrap it
    [SyncVar] GameObject _activeMount;
    public Mount activeMount
    {
        get { return _activeMount != null ? _activeMount.GetComponent<Mount>() : null; }
        set { _activeMount = value != null ? value.gameObject : null; }
    }

    // when moving into attack range of a target, we always want to move a
    // little bit closer than necessary to tolerate for latency and other
    // situations where the target might have moved away a little bit already.
    [Header("Movement")]
    [Range(0.1f, 1)] public float attackToMoveRangeRatio = 0.8f;

    // some commands should have delays to avoid DDOS, too much database usage
    // or brute forcing coupons etc. we use one riskyAction timer for all.
    [SyncVar, HideInInspector] public double nextRiskyActionTime = 0; // double for long term precision

    // the next target to be set if we try to set it while casting
    // 'Entity' can't be SyncVar and NetworkIdentity causes errors when null,
    // so we use [SyncVar] GameObject and wrap it for simplicity
    [SyncVar] GameObject _nextTarget;
    public Entity nextTarget
    {
        get { return _nextTarget != null ? _nextTarget.GetComponent<Entity>() : null; }
        set { _nextTarget = value != null ? value.gameObject : null; }
    }

    // 'ElementSlot' can't be SyncVar and NetworkIdentity causes errors when null,
    // so we use [SyncVar] GameObject and wrap it for simplicity
    [SyncVar] GameObject _selectedElement;
    public ElementSlot selectedElement
    {
        get { return _selectedElement != null ? _selectedElement.GetComponent<ElementSlot>() : null; }
        set
        {
            _selectedElement = value != null ? value.gameObject : null;
        }
    }

    // keep list of known names
    public enum CharacterState
    {
        friend, ally, neutral, dubious, outlaw, enemy, murder
    }
    public struct ForeignName
    {
        public string displayName;
        public CharacterState state;
    }
    [SyncVar] public string knownNamesSnyc;
    public Dictionary<int, ForeignName> knownNames = new Dictionary<int, ForeignName>();

    bool InitializeKnownNames()
    {
        if (knownNamesSnyc.Length == 0)
            return false;
        else
        {
            string[] tmp = knownNamesSnyc.Split('#');
            int i = 0;
            knownNames.Clear();
            while (i + 2 <= tmp.Length)
            {
                ForeignName foreignName;
                foreignName.displayName = tmp[i + 1];
                foreignName.state = (CharacterState)tmp[i + 2].ToInt();
                knownNames.Add(tmp[i].ToInt(), foreignName);
                i = i + 3;
            }
            // not needed after iniotialization
            knownNamesSnyc = "";
            return true;
        }
    }

    public string KnownName(int namedCharid)
    {
        ForeignName foreignName;
        if (knownNames.TryGetValue(namedCharid, out foreignName))
        {
            return foreignName.displayName;
        }
        else
        {
            return GlobalVar.nameNotKnown;
        }
    }

    public CharacterState KnownState(int namedCharid)
    {
        ForeignName foreignName;
        if (knownNames.TryGetValue(namedCharid, out foreignName))
        {
            return foreignName.state;
        }
        else
        {
            return CharacterState.neutral;
        }
    }

    [Client]
    public void UpdateKnownNames(int namedCharid, string displayName, CharacterState state)
    {
        ForeignName foreignName;
        foreignName.displayName = displayName;
        foreignName.state = state;
        knownNames[namedCharid] = foreignName;
        CmdUpdateKnownNames(namedCharid, displayName, (int)state);
    }

    [Command]
    private void CmdUpdateKnownNames(int namedCharid, string displayName, int state)
    {
        applyKnownNames(namedCharid, displayName, state);
    }

    [Server]
    private void applyKnownNames(int namedCharid, string displayName, int state)
    {
        Database.SaveNames(this.id, namedCharid, displayName, state);
    }


    // cache players to save lots of computations
    // (otherwise we'd have to iterate NetworkServer.objects all the time)
    // => on server: all online players
    // => on client: all observed players
    public static Dictionary<string, Player> onlinePlayers = new Dictionary<string, Player>();

    // helper variable to remember which spell / element to use when we walked close enough
    public int useSpellWhenCloser = -1;
    public ElementSlot useElementWhenCloser = null;
    public ElementSlot pickElementWhenCloser = null;

    // networkbehaviour ////////////////////////////////////////////////////////
    protected override void Awake()
    {
        if (isClient)
        {
            // apply race
            UMAAvatar = GetComponent<DynamicCharacterAvatar>();
            foreach (StringFloat raceDef in raceSpecification.definitions)
            {
                UMAAvatar.predefinedDNA.AddDNA(raceDef.text, raceDef.value);
            }
        }
        // cache base components
        base.Awake();
    }

    public override void OnStartLocalPlayer()
    {
        // set singleton
        localPlayer = this;

        // initialize player from sync values
        InitializeCharacter();

        // setup camera targets
        Camera.main.GetComponent<CameraMMO>().target = transform;
        GameObject.FindWithTag("MinimapCamera").GetComponent<CopyPosition>().target = transform;
        if (avatarCamera) avatarCamera.enabled = true; // avatar camera for local player

        // load spellbar after player data was loaded
        LoadSpellbar();

        // no name shown
        showOverlay = false;
        nameOverlay.gameObject.SetActive(false);
        healthOverlay.gameObject.SetActive(false);

        // run local initialization from server load
        // delay due to communication delay
        Invoke("InitializeLocalPlayer", GlobalVar.repeatInitializationAttempt);

        LogFile.WriteLog(LogFile.LogLevel.Always, String.Format("Character {0} with ID: {1} started.", displayName, id.ToString()));
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // setup synclist callbacks on client. no need to update and show and
        // animate equipment on server
        inventory.Callback += OnEquipmentChanged;

        // initialize player from sync values
        InitializeCharacter();

        InvokeRepeating("RefreshDivergence", 0, GlobalVar.divergenceCalculationCycle);
        InvokeRepeating("RefreshPlaytime", 0, GlobalVar.playtimeAccuracy);
        InvokeRepeating("OneSecondCycleClient", 1, 1);
        InvokeRepeating("FiveMinuteCycle", 300, 300);

    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        InvokeRepeating("OneSecondCycleServer", 1, 1);
        // initialize trade item indices
        for (int i = 0; i < 6; ++i) tradeOfferItems.Add(-1);
    }

    protected override void Start()
    {
        // do nothing if not spawned (=for character selection previews)
        if (!isServer && !isClient) return;

        base.Start();
        onlinePlayers[name] = this;

        // spawn effects for any buffs that might still be active after loading
        // (OnStartServer is too early)
        // note: no need to do that in Entity.Start because we don't load them
        //       with previously casted spells
        if (isServer)
            for (int i = 0; i < buffs.Count; ++i)
                if (buffs[i].BuffTimeRemaining() > 0)
                    buffs[i].data.SpawnEffect(this, this);

        // notify guild members that we are online. this also updates the client's
        // own guild info via targetrpc automatically
        // -> OnStartServer is too early because it's not spawned there yet
        if (isServer)
            SetGuildOnline(true);
    }

    void LateUpdate()
    {
        // pass parameters to animation state machine
        // => passing the states directly is the most reliable way to avoid all
        //    kinds of glitches like movement sliding, attack twitching, etc.
        // => make sure to import all looping animations like idle/run/attack
        //    with 'loop time' enabled, otherwise the client might only play it
        //    once
        // => MOVING state is set to local IsMovement result directly. otherwise
        //    we would see animation latencies for rubberband movement if we
        //    have to wait for MOVING state to be received from the server
        // => spell names are assumed to be boolean parameters in animator
        //    so we don't need to worry about an animation number etc.
        if (isClient) // no need for animations or key checks on the server
        {
            // calculate some basics
            float lastTurning = lastButOneRotationY - transform.rotation.eulerAngles.y;
            lastButOneRotationY = transform.rotation.eulerAngles.y;
            // positionCheck make environment visible
            if (isLocalPlayer)
            {
                if (lastButOneArea != currentArea)
                {
                    lastButOneArea = currentArea;
                    foreach (Transform area in Universal.AllAreas.transform)
                    {
                        area.gameObject.SetActive(area.name == currentArea);
                    }
                }
            }
            // now pass parameters after any possible rebinds
            // get current animation type once for all
            string animationType = "";
            if (state == GlobalVar.stateWorking)
            {
                if (inventory.GetEquipment(workingHand, out ItemSlot itemSlot))
                {
                    animationType = ((UsableItem)itemSlot.item.data).animation;
                }
            }

            foreach (Animator anim in GetComponentsInChildren<Animator>())
            {
                if ((IsMoving() || state == GlobalVar.stateIdle) && !IsMounted())
                {
                    // We use for backward a higher speed since Unity animation has issues to recognize very slow motions
                    // A higher speed is not overlayed by the idle animation that much
                    anim.SetBool("LOCOMOTION", true);
                    anim.SetBool("SWIMMING", isSwimming);
                    anim.SetFloat("Turning", lastTurning);
                    anim.SetFloat("Speed", agent.velocity.sqrMagnitude / speedWalkPlayerByTwo * (hasMovedBackward ? GlobalVar.animWalkFBackwardMultiplyer : GlobalVar.animWalkForwardMultiplyer));
                }
                else
                {
                    anim.SetBool("LOCOMOTION", false);
                }
                anim.SetBool("CASTING", state == GlobalVar.stateCasting);
                anim.SetBool("STUNNED", state == GlobalVar.stateStunned);
                anim.SetBool("MOUNTED", IsMounted()); // for seated animation
                anim.SetBool("DEAD", state == GlobalVar.stateDead);
                anim.SetBool("WORKING", state == GlobalVar.stateWorking);
                // state change ... for long lasting animations and interruptions
                if (state != lastButOneState)
                {
                    if (state == GlobalVar.stateDead)
                    {
                        anim.SetInteger("ClipNumber", GlobalFunc.UnifiedRandom(GlobalVar.numberOfDeadAnimations, id));
                    }
                    if (state == GlobalVar.stateWorking)
                    {
                        if (animationType.Length > 0)
                        {
                            anim.SetBool(animationType, true);
                        }
                    }
                    else
                    {
                        if (lastButOneAnimationType.Length > 0)
                        {
                            anim.SetBool(lastButOneAnimationType, false);
                        }
                    }
                }

                if (currentSpell >= 0)
                {
                    if (spells[currentSpell].data.castAnimation.Length > 0)
                    {
                        anim.SetBool(spells[currentSpell].data.castAnimation, state == GlobalVar.stateCasting);
                    }
                }
            }
            // remember last states
            lastButOneState = state;
            lastButOneAnimationType = animationType;

            if (!UIUtils.AnyInputActive())
            {
                if (Input.GetKeyDown(KeyCode.P))
                {
                    // plunder surounding
                    PickElementsInRange();
                    SetPickRangeIndicator(transform.position);
                }
                if (isGM)
                {
                    if (Input.GetKey(KeyCode.K) && Input.GetKey(GlobalVar.gmReleaseKey))
                    {
                        LogFile.WriteLog(LogFile.LogLevel.Info, "GM action performed: Suicide");
                        CmdInstantKill();
                    }
                }
                if (Input.GetKeyDown(KeyCode.F9))
                {
                    Screenshot();
                }
                if (Input.GetKeyDown(KeyCode.F10) && !GlobalVar.isProduction)
                {
                    //<<< test and debug action on server, has to remain in code
                    CmdTestAndDebug("testtext");
                }
                if (Input.GetKeyDown(KeyCode.F11) && !GlobalVar.isProduction)
                {
                    //<<< test and debug action, has to remain in code
                    GameObject go = GameObject.Find("Canvas/TestAndDebug");
                    UITestAndDebug testAndDebug = go.GetComponent<UITestAndDebug>();
                    testAndDebug.ClientActionOnF11(this);
                }
            }

        }

        // follow mount's seat position if mounted
        // (on server too, for correct collider position and calculations)
        ApplyMountSeatOffset();
    }

    [Command]
    public void CmdInstantKillTarget()
    {
        if (target)
        {
            target.health = 0;
            if (target is Player)
            {
                gmState = GameMaster.useKill(gmState);
            }
        }
    }

    [Command]
    public void CmdDeleteSelectedElement()
    {
        if (selectedElement)
        {
            Destroy(selectedElement.gameObject);
            selectedElement = null;
        }
    }

    [Server]
    public override void InstantKill()
    {
        health = 0;
        Database.CharacterSave(this, true);
    }

    [Client]
    public void ReviveClient()
    {
        stamina = (int)(staminaMaxPlayer * GlobalVar.reviveStamina);
    }

    [Server]
    public override void Revive(float healthPercentage = 1)
    {
        health = 1;
        mana = 1;
        Database.CharacterSave(this, true);
    }

    // Stamina handling for player only
    [Header("Stamina")]
    [SyncVar] int _stamina = 1000000;
    public int stamina
    {
        get { return _stamina; }
        set { _stamina = Mathf.Clamp(value, GlobalVar.staminaNegaive, staminaMaxPlayer); }
    }

    public float StaminaPercent()
    {
        return (stamina > 0) ? (float)stamina / (float)staminaMaxPlayer : (float)stamina / GlobalVar.staminaNegaive;
    }
    [Command]
    public void CmdSetTargetStamina(int value)
    {
        // validate
        if (target is Player)
        {
            ((Player)target).stamina = value;
        }
    }
    // Weight handling for player only
    public int weight
    {
        get
        {
            int foundWeight = 0;
            int idOfBackpack = ContainerIdOfBackpack();
            foreach (ItemSlot slot in inventory)
            {
                if (slot.container == GlobalVar.containerEquipment || slot.container == idOfBackpack)
                    foundWeight += slot.item.weight * slot.amount;
            }
            return foundWeight;
        }
    }

    /// <summary>
    /// relative weight 0..1
    /// </summary>
    public float WeightPercent()
    {
        return (float)weight / weightMaxPlayer;
    }

    void OnDestroy()
    {
        // do nothing if not spawned (=for character selection previews)
        if (!isServer && !isClient) return;

        // Unity bug: isServer is false when called in host mode. only true when
        // called in dedicated mode. so we need a workaround:
        if (NetworkServer.active) // isServer
        {
            // leave party (if any)
            if (InParty())
            {
                // dismiss if master, leave otherwise
                if (party.members[0] == name)
                    PartyDismiss();
                else
                    PartyLeave();
            }

            // notify guild members that we are offline
            SetGuildOnline(false);
        }

        if (isLocalPlayer) // requires at least Unity 5.5.1 bugfix to work
        {
            Destroy(indicator);
            SaveSpellbar();
            localPlayer = null;
        }

        onlinePlayers.Remove(name);
    }

    // finite state machine events /////////////////////////////////////////////
    bool EventDied()
    {
        return health == 0;
    }

    bool EventTargetDisappeared()
    {
        return target == null;
    }

    bool EventTargetDied()
    {
        return target != null && target.health == 0;
    }

    bool EventSpellRequest()
    {
        return 0 <= currentSpell && currentSpell < spells.Count;
    }

    bool EventSpellFinished()
    {
        return 0 <= currentSpell && currentSpell < spells.Count &&
               spells[currentSpell].CastTimeRemaining() == 0;
    }

    bool EventMoveStart()
    {
        return state != GlobalVar.stateMoving && IsMoving(); // only fire when started moving
    }

    bool EventMoveEnd()
    {
        return state == GlobalVar.stateMoving && !IsMoving(); // only fire when stopped moving
    }

    bool EventTradeStarted()
    {
        // did someone request a trade? and did we request a trade with him too?
        Player player = FindPlayerFromTradeInvitation();
        return player != null && player.tradeRequestFrom == name;
    }

    bool EventTradeDone()
    {
        // trade canceled or finished?
        return state == GlobalVar.stateTrading && tradeRequestFrom == "";
    }

    bool craftingRequested;
    bool EventCraftingStarted()
    {
        bool result = craftingRequested;
        craftingRequested = false;
        return result;
    }

    bool EventCraftingDone()
    {
        return state == GlobalVar.stateCrafting && NetworkTime.time > craftingTimeEnd;
    }

    bool EventWorkingStart()
    {
        return state != GlobalVar.stateWorking && workingHand != -1;
    }

    bool EventWorkingEnd()
    {
        return state == GlobalVar.stateWorking && workingHand == -1;
    }
    bool EventStunned()
    {
        return NetworkTime.time <= stunTimeEnd;
    }

    Vector3 respawnPosition = new Vector3();
    float respawnDirection = 0;
    bool respawnRequested;
    [Command]
    public void CmdRespawn(Vector3 pos, float yDir)
    {
        respawnRequested = true;
        respawnPosition = pos;
        respawnDirection = yDir;
    }
    bool EventRespawn()
    {
        bool result = respawnRequested;
        respawnRequested = false; // reset
        return result;
    }

    [Command]
    public void CmdCancelAction() { cancelActionRequested = true; }
    bool cancelActionRequested;
    bool EventCancelAction()
    {
        bool result = cancelActionRequested;
        cancelActionRequested = false; // reset
        return result;
    }

    // finite state machine - server ///////////////////////////////////////////
    [Server]
    int UpdateServer_IDLE()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died.
            OnDeath();
            return GlobalVar.stateDead;
        }
        if (EventStunned())
        {
            rubberbanding.ResetMovement();
            return GlobalVar.stateStunned;
        }
        if (EventCancelAction())
        {
            // the only thing that we can cancel is the target
            target = null;
            selectedElement = null;
            return GlobalVar.stateIdle;
        }
        if (EventTradeStarted())
        {
            // cancel casting (if any), set target, go to trading
            currentSpell = -1; // just in case
            target = FindPlayerFromTradeInvitation();
            return GlobalVar.stateTrading;
        }
        if (EventCraftingStarted())
        {
            // cancel casting (if any), go to crafting
            currentSpell = -1; // just in case
            return GlobalVar.stateCrafting;
        }
        if (EventMoveStart())
        {
            // cancel casting (if any)
            currentSpell = -1;
            return GlobalVar.stateMoving;
        }
        if (EventSpellRequest())
        {
            // don't cast while mounted
            // (no MOUNTED state because we'd need MOUNTED_STUNNED, etc. too)
            if (!IsMounted())
            {
                // user wants to cast a spell.
                // check self (alive, mana, weapon etc.) and target and distance
                Spell spell = spells[currentSpell];
                nextTarget = target; // return to this one after any corrections by CastCheckTarget
                Vector3 destination;
                if (CastCheckSelf(spell) && CastCheckTarget(spell) && CastCheckDistance(spell, out destination))
                {
                    // start casting and cancel movement in any case
                    // (player might move into attack range * 0.8 but as soon as we
                    //  are close enough to cast, we fully commit to the cast.)
                    rubberbanding.ResetMovement();
                    StartCastSpell(spell);
                    return GlobalVar.stateCasting;
                }
                else
                {
                    // checks failed. stop trying to cast.
                    currentSpell = -1;
                    nextTarget = null; // nevermind, clear again (otherwise it's shown in UITarget)
                    return GlobalVar.stateIdle;
                }
            }
        }
        if (EventWorkingStart())
        {
            // cancel casting (if any), go to working
            currentSpell = -1; // just in case
            return GlobalVar.stateWorking;
        }

        //if (EventSpellFinished()) { } // don't care
        //if (EventMoveEnd()) { } // don't care
        //if (EventTradeDone()) { } // don't care
        //if (EventCraftingDone()) { } // don't care
        //if (EventRespawn()) { } // don't care
        //if (EventTargetDied()) { } // don't care
        //if (EventWorkingEnd()) { } // don't care
        //if (EventTargetDisappeared()) { } // don't care

        return GlobalVar.stateIdle; // nothing interesting happened
    }

    [Server]
    int UpdateServer_MOVING()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died.
            OnDeath();
            return GlobalVar.stateDead;
        }
        if (EventStunned())
        {
            rubberbanding.ResetMovement();
            return GlobalVar.stateStunned;
        }
        if (EventWorkingStart())
        {
            // cancel casting (if any), stop moving, go to working
            currentSpell = -1;
            //            rubberbanding.ResetMovement();
            return GlobalVar.stateWorking;
        }
        if (EventMoveEnd())
        {
            // finished moving. do whatever we did before.
            return GlobalVar.stateIdle;
        }
        if (EventCancelAction())
        {
            // cancel casting (if any) and stop moving
            currentSpell = -1;
            //rubberbanding.ResetMovement(); <- done locally. doing it here would reset localplayer to the slightly behind server position otherwise
            return GlobalVar.stateIdle;
        }
        if (EventTradeStarted())
        {
            // cancel casting (if any), stop moving, set target, go to trading
            currentSpell = -1;
            rubberbanding.ResetMovement();
            target = FindPlayerFromTradeInvitation();
            return GlobalVar.stateTrading;
        }
        if (EventCraftingStarted())
        {
            // cancel casting (if any), stop moving, go to crafting
            currentSpell = -1;
            rubberbanding.ResetMovement();
            return GlobalVar.stateCrafting;
        }

        // SPECIAL CASE: Spell Request while doing rubberband movement
        // -> we don't really need to react to it
        // -> we could just wait for move to end, then react to request in IDLE
        // -> BUT player position on server always lags behind in rubberband movement
        // -> SO there would be a noticeable delay before we start to cast
        //
        // SOLUTION:
        // -> start casting as soon as we are in range
        // -> BUT don't ResetMovement. instead let it slide to the final position
        //    while already starting to cast
        // -> NavMeshAgentRubberbanding won't accept new positions while casting
        //    anyway, so this is fine
        if (EventSpellRequest())
        {
            // don't cast while mounted
            // (no MOUNTED state because we'd need MOUNTED_STUNNED, etc. too)
            if (!IsMounted())
            {
                Vector3 destination;
                Spell spell = spells[currentSpell];
                if (CastCheckSelf(spell) && CastCheckTarget(spell) && CastCheckDistance(spell, out destination))
                {
                    //Debug.Log("MOVING->EventSpellRequest: early cast started while sliding to destination...");
                    // rubberbanding.ResetMovement(); <- DO NOT DO THIS.
                    StartCastSpell(spell);
                    return GlobalVar.stateCasting;
                }
            }
        }

        //if (EventMoveStart()) { } // don't care
        //if (EventSpellFinished()) { } // don't care
        //if (EventTradeDone()) { } // don't care
        //if (EventCraftingDone()) { } // don't care
        //if (EventWorkingEnd()) { } // don't care
        //if (EventRespawn()) { } // don't care
        //if (EventTargetDied()) { } // don't care
        //if (EventTargetDisappeared()) { } // don't care

        return GlobalVar.stateMoving; // nothing interesting happened
    }

    void UseNextTargetIfAny()
    {
        // use next target if the user tried to target another while casting
        // (target is locked while casting so spell isn't applied to an invalid
        //  target accidentally)
        if (nextTarget != null)
        {
            target = nextTarget;
            nextTarget = null;
        }
    }

    [Server]
    int UpdateServer_CASTING()
    {
        // keep looking at the target for server & clients (only Y rotation)
        if (target) LookAtY(target.transform.position);

        // events sorted by priority (e.g. target doesn't matter if we died)
        //
        // IMPORTANT: nextTarget might have been set while casting, so make sure
        // to handle it in any case here. it should definitely be null again
        // after casting was finished.
        // => this way we can reliably display nextTarget on the client if it's
        //    != null, so that UITarget always shows nextTarget>target
        //    (this just feels better)
        if (EventDied())
        {
            // we died.
            OnDeath();
            UseNextTargetIfAny(); // if user selected a new target while casting
            return GlobalVar.stateDead;
        }
        if (EventStunned())
        {
            currentSpell = -1;
            rubberbanding.ResetMovement();
            return GlobalVar.stateStunned;
        }
        if (EventMoveStart())
        {
            // we do NOT cancel the cast if the player moved, and here is why:
            // * local player might move into cast range and then try to cast.
            // * server then receives the Cmd, goes to CASTING state, then
            //   receives one of the last movement updates from the local player
            //   which would cause EventMoveStart and cancel the cast.
            // * this is the price for rubberband movement.
            // => if the player wants to cast and got close enough, then we have
            //    to fully commit to it. there is no more way out except via
            //    cancel action. any movement in here is to be rejected.
            //    (many popular MMOs have the same behaviour too)
            //
            // we do NOT reset movement either. allow sliding to final position.
            // (NavMeshAgentRubberbanding doesn't accept new ones while CASTING)
            //rubberbanding.ResetMovement(); <- DO NOT DO THIS
            return GlobalVar.stateCasting;
        }
        if (EventCancelAction())
        {
            // cancel casting
            currentSpell = -1;
            UseNextTargetIfAny(); // if user selected a new target while casting
            return GlobalVar.stateIdle;
        }
        if (EventTradeStarted())
        {
            // cancel casting (if any), stop moving, set target, go to trading
            currentSpell = -1;
            rubberbanding.ResetMovement();

            // set target to trade target instead of next target (clear that)
            target = FindPlayerFromTradeInvitation();
            nextTarget = null;
            return GlobalVar.stateTrading;
        }
        if (EventTargetDisappeared())
        {
            // cancel if the target matters for this spell
            if (spells[currentSpell].cancelCastIfTargetDied)
            {
                currentSpell = -1;
                UseNextTargetIfAny(); // if user selected a new target while casting
                return GlobalVar.stateIdle;
            }
        }
        if (EventTargetDied())
        {
            // cancel if the target matters for this spell
            if (spells[currentSpell].cancelCastIfTargetDied)
            {
                currentSpell = -1;
                UseNextTargetIfAny(); // if user selected a new target while casting
                return GlobalVar.stateIdle;
            }
        }
        if (EventSpellFinished())
        {
            // apply the spell after casting is finished
            // note: we don't check the distance again. it's more fun if players
            //       still cast the spell if the target ran a few steps away
            Spell spell = spells[currentSpell];

            // apply the spell on the target
            FinishCastSpell(spell);

            // clear current spell for now
            currentSpell = -1;

            // target-based spell and no more valid target? then clear
            // (otherwise IDLE will get an unnecessary spell request and mess
            //  with targeting)
            bool validTarget = target != null && target.health > 0;
            if (currentSpell != -1 && spells[currentSpell].cancelCastIfTargetDied && !validTarget)
                currentSpell = -1;

            // use next target if the user tried to target another while casting
            UseNextTargetIfAny();

            // go back to IDLE
            return GlobalVar.stateIdle;
        }
        //if (EventMoveEnd()) { } // don't care
        //if (EventTradeDone()) { } // don't care
        //if (EventCraftingStarted()) { } // don't care
        //if (EventCraftingDone()) { } // don't care
        //if (EventWorkingStart()) { } // don't care
        //if (EventWorkingEnd()) { } // don't care
        //if (EventRespawn()) { } // don't care
        //if (EventSpellRequest()) { } // don't care

        return GlobalVar.stateCasting; // nothing interesting happened
    }

    [Server]
    int UpdateServer_STUNNED()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died.
            OnDeath();
            return GlobalVar.stateDead;
        }
        if (EventStunned())
        {
            return GlobalVar.stateStunned;
        }

        // go back to idle if we aren't stunned anymore and process all new
        // events there too
        return GlobalVar.stateIdle;
    }

    [Server]
    int UpdateServer_TRADING()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died, stop trading. other guy will receive targetdied event.
            OnDeath();
            TradeCleanup();
            return GlobalVar.stateDead;
        }
        if (EventStunned())
        {
            // stop trading
            currentSpell = -1;
            rubberbanding.ResetMovement();
            TradeCleanup();
            return GlobalVar.stateStunned;
        }
        if (EventMoveStart())
        {
            // reject movement while trading
            rubberbanding.ResetMovement();
            return GlobalVar.stateTrading;
        }
        if (EventCancelAction())
        {
            // stop trading
            TradeCleanup();
            return GlobalVar.stateIdle;
        }
        if (EventTargetDisappeared())
        {
            // target disconnected, stop trading
            TradeCleanup();
            return GlobalVar.stateIdle;
        }
        if (EventTargetDied())
        {
            // target died, stop trading
            TradeCleanup();
            return GlobalVar.stateIdle;
        }
        if (EventTradeDone())
        {
            // someone canceled or we finished the trade. stop trading
            TradeCleanup();
            return GlobalVar.stateIdle;
        }
        //if (EventMoveEnd()) { } // don't care
        //if (EventSpellFinished()) { } // don't care
        //if (EventCraftingStarted()) { } // don't care
        //if (EventCraftingDone()) { } // don't care
        //if (EventRespawn()) { } // don't care
        //if (EventTradeStarted()) { } // don't care
        //if (EventWorkingStart()) { } // don't care
        //if (EventWorkingEnd()) { } // don't care
        //if (EventSpellRequest()) { } // don't care

        return GlobalVar.stateTrading; // nothing interesting happened
    }

    [Server]
    int UpdateServer_CRAFTING()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died, stop crafting
            OnDeath();
            return GlobalVar.stateDead;
        }
        if (EventStunned())
        {
            // stop crafting
            rubberbanding.ResetMovement();
            return GlobalVar.stateStunned;
        }
        if (EventMoveStart())
        {
            // reject movement while crafting
            Debug.Log("Shoud not happen");
            rubberbanding.ResetMovement();
            return GlobalVar.stateCrafting;
        }
        if (EventCraftingDone())
        {
            // finish crafting
            Craft();
            return GlobalVar.stateIdle;
        }
        if (EventWorkingStart())
        {
            // reject working while crafting
            Debug.Log("Shoud not happen");
            StopWorking();
            return GlobalVar.stateCrafting;
        }
        //if (EventCancelAction()) { } // don't care. user pressed craft, we craft.
        //if (EventTargetDisappeared()) { } // don't care
        //if (EventTargetDied()) { } // don't care
        //if (EventMoveEnd()) { } // don't care
        //if (EventSpellFinished()) { } // don't care
        //if (EventRespawn()) { } // don't care
        //if (EventTradeStarted()) { } // don't care
        //if (EventTradeDone()) { } // don't care
        //if (EventCraftingStarted()) { } // don't care
        //if (EventWorkingEnd()) { } // don't care
        //if (EventSpellRequest()) { } // don't care

        return GlobalVar.stateCrafting; // nothing interesting happened
    }

    [Server]
    int UpdateServer_WORKING()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died, stop working
            OnDeath();
            return GlobalVar.stateDead;
        }
        if (EventStunned())
        {
            // stop working
            workingHand = -1;
            return GlobalVar.stateStunned;
        }
        if (EventMoveStart())
        {
            // stop working
            StopWorking();
            return GlobalVar.stateMoving;
        }
        if (EventWorkingEnd())
        {
            // finish work
            return GlobalVar.stateIdle;
        }
        if (EventCancelAction())
        {
            // stop working
            workingHand = -1;
            return GlobalVar.stateIdle;
        }
        //if (EventTargetDisappeared()) { } // don't care
        //if (EventTargetDied()) { } // don't care
        //if (EventMoveEnd()) { } // don't care
        //if (EventSpellFinished()) { } // don't care
        //if (EventRespawn()) { } // don't care
        //if (EventTradeStarted()) { } // don't care
        //if (EventTradeDone()) { } // don't care
        //if (EventCraftingStarted()) { } // don't care
        //if (EventWorkingEnd()) { } // don't care
        //if (EventSpellRequest()) { } // don't care

        return GlobalVar.stateWorking; // nothing interesting happened
    }

    [Server]
    int UpdateServer_DEAD()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventRespawn())
        {
            // respawn
            TeleportToPosition(respawnPosition, respawnDirection);
            health = 1;
            return GlobalVar.stateIdle;
        }
        //if (EventMoveStart()) { } // don't care
        //if (EventMoveEnd()) { } // don't care
        //if (EventSpellFinished()) { } // don't care
        //if (EventDied()) { } // don't care
        //if (EventCancelAction()) { } // don't care
        //if (EventTradeStarted()) { } // don't care
        //if (EventTradeDone()) { } // don't care
        //if (EventCraftingStarted()) { } // don't care
        //if (EventCraftingDone()) { } // don't care
        //if (EventWorkingStart()) { } // don't care
        //if (EventWorkingEnd()) { } // don't care
        //if (EventTargetDisappeared()) { } // don't care
        //if (EventTargetDied()) { } // don't care
        //if (EventSpellRequest()) { } // don't care

        return GlobalVar.stateDead; // nothing interesting happened
    }

    [Server]
    protected override int UpdateServer()
    {
        if (state == GlobalVar.stateIdle) return UpdateServer_IDLE();
        if (state == GlobalVar.stateMoving) return UpdateServer_MOVING();
        if (state == GlobalVar.stateCasting) return UpdateServer_CASTING();
        if (state == GlobalVar.stateStunned) return UpdateServer_STUNNED();
        if (state == GlobalVar.stateTrading) return UpdateServer_TRADING();
        if (state == GlobalVar.stateCrafting) return UpdateServer_CRAFTING();
        if (state == GlobalVar.stateWorking) return UpdateServer_WORKING();
        if (state == GlobalVar.stateDead) return UpdateServer_DEAD();
        LogFile.WriteLog(LogFile.LogLevel.Error, "Found invalid state: " + state);
        return GlobalVar.stateIdle;
    }

    // finite state machine - client ///////////////////////////////////////////
    [Client]
    protected override void UpdateClient()
    {
        if (state == GlobalVar.stateIdle || state == GlobalVar.stateMoving)
        {
            if (isLocalPlayer)
            {
                // simply accept input
                SelectionHandling();
                WASDHandling();
                TargetNearest();
                if (state == GlobalVar.stateMoving)
                    StaminaHandling();

                // cancel action if escape key was pressed
                if (Input.GetKeyDown(PlayerPreferences.keyCancelAction))
                {
                    agent.ResetPath(); // reset locally because we use rubberband movement
                    CmdCancelAction();
                }

                // trying to cast a spell on a monster that wasn't in range?
                // then check if we walked into attack range by now
                if (useSpellWhenCloser != -1)
                {
                    // can we still attack the target? maybe it was switched.
                    if (CanAttack(target))
                    {
                        // in range already?
                        // -> we don't use CastCheckDistance because we want to
                        // move a bit closer (attackToMoveRangeRatio)
                        float range = spells[useSpellWhenCloser].CastRange(this) * attackToMoveRangeRatio;
                        if (Utils.ClosestDistance(collider, target.collider) <= range)
                        {
                            // then stop moving and start attacking
                            CmdUseSpell(useSpellWhenCloser);

                            // reset
                            useSpellWhenCloser = -1;
                        }
                        // otherwise keep walking there. the target might move
                        // around or run away, so we need to keep adjusting the
                        // destination all the time
                        else
                        {
                            //Debug.Log("walking closer to target...");
                            agent.stoppingDistance = range;
                            agent.destination = target.collider.ClosestPoint(transform.position);
                        }
                    }
                    // otherwise reset
                    else useSpellWhenCloser = -1;
                }
                if (useElementWhenCloser)
                {
                    if (Vector3.Distance(transform.position, useElementWhenCloser.transform.position) < useElementWhenCloser.interactionRange)
                    {
                        // try to use or pick the element and reset
                        if (useElementWhenCloser.CanUse(this))
                        {
                            UseSelectedElement(useElementWhenCloser);
                        }
                        else if (useElementWhenCloser.pickable)
                            CmdPickSelectedElement();
                        agent.ResetMovement();
                        useElementWhenCloser = null;
                        pickElementWhenCloser = null;
                    }
                }
                if (pickElementWhenCloser)
                    if (Vector3.Distance(transform.position, pickElementWhenCloser.transform.position) < pickElementWhenCloser.interactionRange)
                    {
                        // try pick the element and reset
                        if (pickElementWhenCloser.pickable)
                            CmdPickSelectedElement();
                        useElementWhenCloser = null;
                        pickElementWhenCloser = null;
                    }
            }
        }
        else if (state == GlobalVar.stateCasting)
        {
            // keep looking at the target for server & clients (only Y rotation)
            if (target) LookAtY(target.transform.position);

            if (isLocalPlayer)
            {
                // simply accept input and reset any client sided movement
                SelectionHandling();
                WASDHandling(); // still call this to set pendingVelocity for after cast
                TargetNearest();
                agent.ResetMovement();

                // cancel action if escape key was pressed
                if (Input.GetKeyDown(PlayerPreferences.keyCancelAction)) CmdCancelAction();
            }
        }
        else if (state == GlobalVar.stateWorking)
        {
            // keep looking at the target for server & clients (only Y rotation)
            if (selectedElement)
            {
                LookAtY(selectedElement.transform.position);
            }

            if (isLocalPlayer)
            {
                // simply accept input and allow walking away
                SelectionHandling();
                WASDHandling();
                TargetNearest();

                // cancel action if escape key was pressed
                if (Input.GetKeyDown(PlayerPreferences.keyCancelAction))
                {
                    CmdStopWorking();
                }
            }
        }
        else if (state == GlobalVar.stateStunned)
        {
            if (isLocalPlayer)
            {
                // simply accept input and reset any client sided movement
                SelectionHandling();
                TargetNearest();
                agent.ResetMovement();

                // cancel action if escape key was pressed
                if (Input.GetKeyDown(PlayerPreferences.keyCancelAction)) CmdCancelAction();
            }
        }
        else if (state == GlobalVar.stateTrading) { }
        else if (state == GlobalVar.stateCrafting) { }
        else if (state == GlobalVar.stateDead) { }
        else Debug.LogError("invalid state:" + state);
    }

    // overlays ////////////////////////////////////////////////////////////////
    protected override void UpdateOverlays()
    {

        base.UpdateOverlays();

        if (showOverlay)
        {
            if (nameOverlay != null && nameOverlay.gameObject.activeSelf)
            {
                // to show name in character selection
                nameOverlay.text = displayName;
                // find local player (null while in character selection)
                if (localPlayer != null)
                {
                    if (localPlayer.gmKnowNames)
                    {
                        nameOverlay.text = displayName;
                    }
                    else
                    {
                        string addText = "";
                        if (gmDisplay)
                            addText = PlayerPreferences.overlayGmNameAdd;
                        nameOverlay.text = addText + localPlayer.KnownName(id);
                    }

                    // note: murderer has higher priority (a player can be a murderer and an
                    // offender at the same time)
                    if (IsMurderer())
                        nameOverlay.color = PlayerPreferences.nameOverlayMurdererColor;
                    else if (IsOffender())
                        nameOverlay.color = PlayerPreferences.nameOverlayOffenderColor;
                    // otherwise default
                    else
                        nameOverlay.color = PlayerPreferences.characterStateColor[(int)localPlayer.KnownState(id)];
                }
            }
        }
        // helthOverlay is active for currrent player  if he is GM with active Invisibility
        else if (isLocalPlayer)
        {
            if (isGmInvisible)
            {
                healthOverlay.gameObject.SetActive(true);
                healthOverlay.text = "Invisible";
            }
            else
            {
                healthOverlay.gameObject.SetActive(false);
            }
        }

    }

    // spell finished event & pending actions //////////////////////////////////
    // pending actions while casting. to be applied after cast.
    int pendingSpell = -1;
    Vector3 pendingDestination;
    bool pendingDestinationValid;
    Vector3 pendingVelocity;
    bool pendingVelocityValid;

    // client event when spell cast finished on server
    // -> useful for follow up attacks etc.
    //    (doing those on server won't really work because the target might have
    //     moved, in which case we need to follow, which we need to do on the
    //     client)
    [Client]
    void OnSpellCastFinished(Spell spell)
    {
        if (!isLocalPlayer) return;

        // tried to click move somewhere?
        if (pendingDestinationValid)
        {
            agent.stoppingDistance = 0;
            agent.destination = pendingDestination;
        }
        // tried to wasd move somewhere?
        else if (pendingVelocityValid)
        {
            agent.velocity = pendingVelocity;
        }

        // user pressed another spell button?
        if (pendingSpell != -1)
        {
            TryUseSpell(pendingSpell, true);
        }
        // otherwise do follow up attack if no interruptions happened
        else if (spell.followupDefaultAttack)
        {
            TryAttackStandardFight(true);
        }
        // clear pending actions in any case
        pendingSpell = -1;
        pendingDestinationValid = false;
        pendingVelocityValid = false;

    }

    // combat //////////////////////////////////////////////////////////////////
    [Server]
    public void OnDamageDealtToMonster(Monster monster)
    {
        // did we kill it?
        if (monster.health == 0)
        {
            // share kill rewards with party or only for self
            // increase quest kill counter for all party members
            if (InParty())
            {
                List<Player> closeMembers = InParty() ? GetPartyMembersInProximity() : new List<Player>();

                foreach (Player member in closeMembers)
                    member.QuestsOnKilled(monster);
            }
            else QuestsOnKilled(monster);
        }
    }

    [Server]
    public void OnDamageDealtToPlayer(Player player)
    {
        // was he innocent?
        if (!player.IsOffender() && !player.IsMurderer())
        {
            // did we kill him? then start/reset murder status
            // did we just attack him? then start/reset offender status
            // (unless we are already a murderer)
            if (player.health == 0) StartMurderer();
            else if (!IsMurderer()) StartOffender();
        }
    }

    [Server]
    public void OnDamageDealtToPet(Pet pet)
    {
        // was he innocent?
        if (!pet.owner.IsOffender() && !pet.owner.IsMurderer())
        {
            // did we kill him? then start/reset murder status
            // did we just attack him? then start/reset offender status
            // (unless we are already a murderer)
            if (pet.health == 0) StartMurderer();
            else if (!IsMurderer()) StartOffender();
        }
    }

    // custom DealDamageAt function if we killed the monster
    [Server]
    public override void DealDamageAt(Entity entity, int damage, float stunTime = 0)
    {
        // deal damage with the default function
        base.DealDamageAt(entity, damage, stunTime);

        // a monster?
        if (entity is Monster)
        {
            OnDamageDealtToMonster((Monster)entity);
        }
        // a player?
        // (see murder code section comments to understand the system)
        else if (entity is Player)
        {
            OnDamageDealtToPlayer((Player)entity);
        }
        // a pet?
        // (see murder code section comments to understand the system)
        else if (entity is Pet)
        {
            OnDamageDealtToPet((Pet)entity);
        }

        // let pet know that we attacked something
        if (activePet != null && activePet.autoAttack)
            activePet.OnAggro(entity);
    }

    // aggro ///////////////////////////////////////////////////////////////////
    // this function is called by entities that attack us
    [ServerCallback]
    public override void OnAggro(Entity entity)
    {
        // forward to pet if it's supposed to defend us
        if (activePet != null && activePet.defendOwner)
            activePet.OnAggro(entity);
    }

    // death ///////////////////////////////////////////////////////////////////
    [Server]
    protected override void OnDeath()
    {
        // take care of entity stuff
        base.OnDeath();

        // rubberbanding needs a custom reset
        rubberbanding.ResetMovement();

        // send an info chat message
        string message = "You died.";
        chat.TargetMsgInfo(connectionToClient, message);
    }

    // loot ////////////////////////////////////////////////////////////////////
    [Command]
    public void CmdTakeLootItem(int slotIndex)
    {
        // validate: dead monster and close enough and valid loot index?
        // use collider point(s) to also work with big entities
        if (IsInventoryActionPermitted() &&
            target != null && target is Monster && target.health == 0 &&
            Utils.ClosestDistance(collider, target.collider) <= interactionRange)
        {
            if (target.inventory.GetItemSlot(GlobalVar.containerLoot, slotIndex, out ItemSlot itemSlot))//Loot always from special loot container
            {
                // try to add it to the inventory, clear monster slot if it worked
                if (InventoryAdd(itemSlot.item, itemSlot.amount, ContainerIdOfBackpack()))
                {
                    target.inventory.Remove(GlobalVar.containerLoot, slotIndex);
                }
            }
        }
    }

    // inventory ///////////////////////////////////////////////////////////////
    [Command]
    public void CmdSwapInventoryInventory(int fromContainer, int fromIndex, int toContainer, int toIndex)
    {
        // note: should never send a command with complex types!
        // validate: make sure that the slots actually exist in the inventory
        // and that they are not equal
        if (IsInventoryActionPermitted(fromContainer, fromIndex, toContainer, toIndex))
        {
            if (inventory.GetItemSlot(fromContainer, fromIndex, out ItemSlot itemSlotFrom))
            {
                if (!IsContainerMovePermitted(itemSlotFrom, toContainer))
                    return;
                if (inventory.GetItemSlot(toContainer, toIndex, out ItemSlot itemSlotTo))
                {
                    if (!IsContainerMovePermitted(itemSlotTo, fromContainer))
                        return;
                    inventory.Remove(fromContainer, fromIndex);
                    inventory.AddItem(itemSlotTo.item, fromContainer, fromIndex, itemSlotTo.amount);
                    inventory.Remove(toContainer, toIndex);
                    inventory.AddItem(itemSlotFrom.item, toContainer, toIndex, itemSlotFrom.amount);
                }
                else
                {
                    inventory.Remove(fromContainer, fromIndex);
                    inventory.AddItem(itemSlotFrom.item, toContainer, toIndex, itemSlotFrom.amount);
                }
            }
            else
            {
                if (inventory.GetItemSlot(toContainer, toIndex, out ItemSlot itemSlotTo))
                {
                    if (!IsContainerMovePermitted(itemSlotFrom, toContainer))
                        return;
                    inventory.AddItem(itemSlotTo.item, fromContainer, fromIndex, itemSlotTo.amount);
                    inventory.Remove(toContainer, toIndex);
                }
            }
        }
    }

    [Command]
    public void CmdInventorySplit(int fromContainer, int fromIndex, int toContainer, int toIndex, int splitNumber)
    {
        // note: should never send a command with complex types!
        // validate: make sure that the slots actually exist in the inventory
        // and that they are not equal
        if (IsInventoryActionPermitted(fromContainer, fromIndex, toContainer, toIndex))
        {
            //slotTo has to be empty
            if (inventory.IsSlotEmpty(toContainer, toIndex))
            {
                if (inventory.GetItemSlot(fromContainer, fromIndex, out ItemSlot itemSlotFrom))
                {
                    // slotFrom needs at least two to split, 
                    if (itemSlotFrom.amount > splitNumber)
                    {
                        inventory.AddItem(itemSlotFrom.item, toContainer, toIndex, splitNumber);
                        inventory.DecreaseAmount(fromContainer, fromIndex, splitNumber);
                    }
                    else
                        Inform(string.Format("You canot split a stack of {0} items by taking {1} items. You think it over again.", itemSlotFrom.amount, splitNumber));
                }
            }
        }
    }

    [Command]
    public void CmdInventoryMerge(int fromContainer, int fromIndex, int toContainer, int toIndex)
    {
        if (IsInventoryActionPermitted(fromContainer, fromIndex, toContainer, toIndex))
        {
            // both items have to be valid 
            if (inventory.GetItemSlot(fromContainer, fromIndex, out ItemSlot itemSlotFrom)
                &&
                inventory.GetItemSlot(toContainer, toIndex, out ItemSlot itemSlotTo))
            {
                if (itemSlotFrom.item.Equals(itemSlotTo.item))
                {
                    // merge from -> to
                    // put as many as possible into 'To' slot
                    int put = inventory.IncreaseAmount(toContainer, toIndex, itemSlotFrom.amount);
                    inventory.DecreaseAmount(fromContainer, fromIndex, put);
                }
            }
        }
    }

    // add a new item to first spare available inventory slot
    [Command]
    public void CmdAddItemToAvailableInventory(string itemName, int amount, int durability, int quality, string miscellaneous)
    {
        AddItemToAvailableInventory(itemName, amount, durability, quality, miscellaneous);
    }
    public void AddItemToAvailableInventory(string itemName, int amount, int durability, int quality, string miscellaneous)
    {
        // just to verify if item exists, has to verified in advance
        if (ScriptableItem.dict.TryGetValue(itemName.GetStableHashCode(), out ScriptableItem itemData))
        {
            // create the item
            Item item = new Item(itemData);
            item.durability = durability;
            item.quality = quality;
            item.miscellaneousSync = miscellaneous;

            if (FindItemInAvailableInventory(item, 1, out int containerId, out int slotIndex, out bool isEmpty))
            {
                if (isEmpty)
                {
                    // create in inventory
                    inventory.AddItem(item, containerId, slotIndex, amount);
                }
                else
                {
                    // increase amout
                    inventory.IncreaseAmount(containerId, slotIndex, amount);
                }
            }
            else
            {
                // create on ground
                CreateItemOnGround(item, transform.position.x, transform.position.y, transform.position.z, amount);
            }
        }
    }

    // find empty slot in backpack or belt
    public bool FindEmptySlotInAvailableInventrory(out int containerId, out int slotIndex)
    {
        // find first empty item slot
        // try first in the backpack
        int containerIndexBackpack = containers.IndexOfId(ContainerIdOfBackpack());
        if (containerIndexBackpack != -1)
        {
            for (int i = 0; i < containers[containerIndexBackpack].slots; i++)
            {
                if (inventory.IsSlotEmpty(ContainerIdOfBackpack(), i))
                {
                    containerId = ContainerIdOfBackpack();
                    slotIndex = i;
                    return true;
                }
            }
        }
        // maybe more luck in the belt / container in backpack
        for (int i = GlobalVar.equipmentBelt1; i <= GlobalVar.equipmentBelt6; i++)
        {
            if (inventory.IsSlotEmpty(GlobalVar.containerEquipment, i))
            {
                containerId = GlobalVar.containerEquipment;
                slotIndex = i;
                return true;
            }
        }
        // no sucess at all
        containerId = 0;
        slotIndex = 0;
        return false;
    }

    // find any slot that can take new created items
    public bool FindItemInAvailableInventory(string itemName, int amount, out int containerId, out int slotIndex, out bool isEmpty)
    {
        // create new item for comparision
        ScriptableItem itemData;
        if (ScriptableItem.dict.TryGetValue(itemName.GetStableHashCode(), out itemData))
        {
            Item item = new Item(itemData);
            return FindItemInAvailableInventory(item, amount, out containerId, out slotIndex, out isEmpty);
        }
        // no sucess at all
        containerId = 0;
        slotIndex = 0;
        isEmpty = false;
        return false;
    }
    // find any slot that can take items
    public bool FindItemInAvailableInventory(Item item, int amount, out int containerId, out int slotIndex, out bool isEmpty)
    {
        int containerIndexBackpack = containers.IndexOfId(ContainerIdOfBackpack());
        ItemSlot itemSlot;
        // serach for slot with same item first
        // try first in the backpack
        if (containerIndexBackpack != -1)
        {
            for (int i = 0; i < containers[containerIndexBackpack].slots; i++)
            {
                if (inventory.GetItemSlot(ContainerIdOfBackpack(), i, out itemSlot))
                {
                    // same Element and enough stack place
                    if (itemSlot.item.Equals(item) && itemSlot.item.data.maxStack - itemSlot.amount - amount > 0)
                    {
                        containerId = ContainerIdOfBackpack();
                        slotIndex = i;
                        isEmpty = false;
                        return true;
                    }
                }
            }
        }
        // maybe more luck in the belt
        for (int i = GlobalVar.equipmentBelt1; i <= GlobalVar.equipmentBelt6; i++)
        {
            if (inventory.GetItemSlot(GlobalVar.containerEquipment, i, out itemSlot))
            {
                // same Element and enough stack place
                if (itemSlot.item.Equals(item) && itemSlot.item.data.maxStack - itemSlot.amount - amount > 0)
                {
                    containerId = GlobalVar.containerEquipment;
                    slotIndex = i;
                    isEmpty = false;
                    return true;
                }
            }
        }
        // let's take the first empty item slot
        if (FindEmptySlotInAvailableInventrory(out containerId, out slotIndex))
        {
            isEmpty = true;
            return true;
        }
        // no sucess at all
        containerId = 0;
        slotIndex = 0;
        isEmpty = false;
        return false;
    }

    // verify any item move action
    // player not in an action and alive
    // target and source is not identical
    private bool IsInventoryActionPermitted(int fromContainer, int fromIndex, int toContainer, int toIndex)
    {
        return (!(fromContainer == toContainer && fromIndex == toIndex)
                &&
               IsInventoryActionPermitted());
    }
    private bool IsInventoryActionPermitted()
    {
        return (state == GlobalVar.stateIdle || state == GlobalVar.stateMoving || state == GlobalVar.stateCasting)
                &&
               health > 0;
    }

    // verify whether the target can have the current container
    private bool IsContainerMovePermitted(ItemSlot sourceSlot, int toContainer)
    {
        if (sourceSlot.item.data is ContainerItem)
        {
            //cannot move into itself
            if (sourceSlot.item.data1 == toContainer)
            {
                Inform("You try unsuccessfully to stuff the bag into itself.");
                return false;
            }
            //target cannot take containers
            if (!IsContainerMovePermitted(toContainer, true))
            {
                return false;
            }
        }
        // cannot move a currently open container so close it first
        RpcCloseContainer(sourceSlot.item.data1);
        return true;
    }
    private bool IsContainerMovePermitted(int toContainer, bool messageToPlayer)
    {
        //target cannot take container
        if (inventory.SpareContainerInContainer(toContainer, this) == 0)
        {
            if (messageToPlayer)
                Inform("There is no room for another bag here.");
            return false;
        }
        return true;
    }

    [ClientRpc]
    public void RpcCloseContainer(int container)
    {
        if (isLocalPlayer)
        {
            UIInventory uiInventory = GameObject.Find("Canvas/Inventory").GetComponent<UIInventory>();
            uiInventory.CloseContainer(container);
        }
    }

    // useable item from inventory
    //   Player                      useable item
    //
    //   from client
    //        ||
    // CmdUseInventoryItem
    //         ------------------->     Use   -->---       First or single use on server
    //        ||              ----<------          |
    //    RpcUsedItem         |                    |
    //         ---------------)--->    OnUsed      |       First or single use on client
    //                        |                    |
    //    UseOverTime <-------o                    |                    
    //        ||              |                    |
    //   UseOverTimeEnd ------)------>   InUse     |       Repeated use on server
    //                        |            |       |
    //                        ---<---------o       |
    //                                     |       |
    //     RPCUseAction <------------------o--------
    //         |
    //         ---------------------->  OnUseAction        Action on client
    //

    [Command]
    public void CmdUseInventoryItem(int container, int index)
    {
        UseInventoryItem(container, index);
    }

    public void UseInventoryItem(int container, int index)
    {
        // validate
        if (IsInventoryActionPermitted())
        {
            if (inventory.GetItemSlot(container, index, out ItemSlot itemSlot))
            {
                if (itemSlot.item.data is UsableItem)
                {
                    // use item
                    // note: we don't decrease amount / destroy in all cases because
                    // some items may swap to other slots in .Use()
                    UsableItem itemData = (UsableItem)itemSlot.item.data;
                    if (itemData.CanUse(this, itemSlot))
                    {
                        // .Use might clear the slot, so we backup the Item first for the Rpc
                        itemData.Use(this, container, index);
                        RpcUsedItem(itemSlot.item, container, index);
                    }
                }
            }
        }
    }

    [ClientRpc]
    public void RpcUsedItem(Item item, int container, int slot)
    {
        // validate
        if (item.data is UsableItem)
        {
            UsableItem itemData = (UsableItem)item.data;
            itemData.OnUsed(this, container, slot);
        }
    }

    private bool useOverTimeStoped = false;
    //[Server]
    public void UseOverTime(int container, int slot, float waitTime)
    {
        if (container == GlobalVar.containerEquipment)
        {
            if (slot == GlobalVar.equipmentLeftHand)
            {
                useOverTimeStoped = false;
                Invoke("UseOverTimeEndLeftHand", waitTime);
            }
            else if (slot == GlobalVar.equipmentRightHand)
            {
                useOverTimeStoped = false;
                Invoke("UseOverTimeEndRightHand", waitTime);
            }
        }
    }
    //[Server]
    public void UseOverTimeEndLeftHand()
    {
        UseOverTimeEnd(GlobalVar.equipmentLeftHand);
    }
    //[Server]
    public void UseOverTimeEndRightHand()
    {
        UseOverTimeEnd(GlobalVar.equipmentRightHand);
    }
    //[Server]
    public void UseOverTimeEnd(int slot)
    {
        if (!useOverTimeStoped)
        {
            if (inventory.GetEquipment(slot, out ItemSlot itemSlot))
            {
                if (itemSlot.item.data is UsableItem)
                {
                    UsableItem itemData = (UsableItem)itemSlot.item.data;
                    itemData.InUse(this, GlobalVar.containerEquipment, slot);
                }
            }
        }
    }

    [ClientRpc]
    public void RpcUseAction(int container, int slot, int action)
    {
        // validate
        if (inventory.GetItemSlot(container, slot, out ItemSlot itemSlot))
        {
            if (itemSlot.item.data is UsableItem)
            {
                UsableItem itemData = (UsableItem)itemSlot.item.data;
                itemData.OnUseAction(this, container, slot, action);
            }
        }
    }

    // use element
    //Client
    public virtual void UseSelectedElement(ElementSlot elementSlot)
    {
        //verify
        if (selectedElement == elementSlot)
        {
            // Player distance
            float distance = Vector3.Distance(transform.position, selectedElement.transform.position);
            if (distance <= selectedElement.interactionRange)
            {
                CmdUseElement();
            }
        }
    }

    [Command]
    public void CmdUseElement()
    {
        UseElement();
    }
    public void UseElement()
    {
        // validate
        if (selectedElement.item.data is UsableItem)
        {
            // use item
            UsableItem itemData = (UsableItem)selectedElement.item.data;
            if (itemData.CanUse(this, selectedElement))
            {
                itemData.Use(this, selectedElement);
                RpcUsedSelectedElement();
            }
        }
    }
    [ClientRpc]
    public void RpcUsedSelectedElement()
    {
        // validate
        if (selectedElement.item.data is UsableItem)
        {
            UsableItem itemData = (UsableItem)selectedElement.item.data;
            itemData.OnUsed(this, selectedElement);
        }
    }

    // equipment ///////////////////////////////////////////////////////////////
    void OnEquipmentChanged(SyncListItemSlot.Operation op, int index, ItemSlot slot)
    {
        // update the model if equipment
        if (slot.container == GlobalVar.containerEquipment)
        {
            RefreshLocation(slot.slot);
        }
    }

    void RebindAnimators()
    {
        foreach (Animator anim in GetComponentsInChildren<Animator>())
            anim.Rebind();
    }

    // first creation should depend on UMAData.CharacterCreated
    public void RefreshLocation(int index)
    {
        if (!isUMAAvatarInitilized)
        {
            LogFile.WriteLog(LogFile.LogLevel.Error, "The program try to initialize equipmenmt before the character was created properly.");
            return;
        }
        EquipmentInfo info = equipmentInfo[index];

        // visible element?
        if (info.isVisible)
        {
            // dockable location?
            if (info.location != null)
            {
                //  valid item?
                if (inventory.GetEquipment(index, out ItemSlot itemSlot))
                {
                    // has a model? then set it
                    UsableItem itemData = (UsableItem)itemSlot.item.data;
                    if (itemData.modelPrefab != null)
                    {
                        GameObject go;
                        string existName = "";
                        if (info.location.childCount > 0) existName = info.location.GetChild(0).name;

                        if (existName == itemData.modelPrefab.name + "(Clone)")
                        {
                            //same prefab, no change
                            go = info.location.GetChild(0).gameObject;
                        }
                        else
                        {
                            // delete existing and and load the new model
                            if (info.location.childCount > 0) Destroy(info.location.GetChild(0).gameObject);
                            go = Instantiate(itemData.modelPrefab);
                            go.transform.SetParent(info.location, false);

                            // is it a skinned mesh with an animator?
                            Animator anim = go.GetComponent<Animator>();
                            if (anim != null)
                            {
                                // assign main animation controller to it
                                anim.runtimeAnimatorController = animator.runtimeAnimatorController;

                                // restart all animators, so that skinned mesh equipment will be
                                // in sync with the main animation
                                RebindAnimators();
                            }
                        }
                        // apply dynamic states of items e.g. light is burning
                        if (itemSlot.item.data is LightItem)
                        {
                            LightElement le = go.GetComponent<LightElement>();
                            le.SwitchDirect(itemSlot.item.data2 == 1);
                        }
                    }
                    else
                    {
                        // invisible, empty equipment slot
                        if (info.location.childCount > 0) Destroy(info.location.GetChild(0).gameObject);
                    }
                }
                else
                {
                    // empty equipment slot
                    if (info.location.childCount > 0) Destroy(info.location.GetChild(0).gameObject);
                }
            }
            else
            {
                // no dockable location, it's a wardrobe recipe
                //  valid item?
                if (inventory.GetEquipment(index, out ItemSlot itemSlot))
                {
                    // has a recipe? then set it
                    ClothingItem itemData = (ClothingItem)itemSlot.item.data;
                    if (itemData.UMAClothingRecipeMale != null)
                    {
                        if (isGenderMale)
                            UMAAvatar.SetSlot(info.requiredCategory, itemData.UMAClothingRecipeMale.name);
                        else
                            UMAAvatar.SetSlot(info.requiredCategory, itemData.UMAClothingRecipeFemale.name);

                        UMAAvatar.BuildCharacter();
                    }
                    else
                    {
                        // that's no wardrobe in the slot, clear the slot
                        UMAAvatar.ClearSlot(info.requiredCategory);
                        UMAAvatar.BuildCharacter();
                    }
                }
                else
                {
                    // no item, clear related slot
                    UMAAvatar.ClearSlot(info.requiredCategory);
                    UMAAvatar.BuildCharacter();
                }
            }
        }
    }

    [Command]
    // in fact we do not swap, the target has to be empty for the action
    public void CmdSplitInventoryEquip(int containerId, int slotIndex, int equipmentIndex)
    {
        SwapInventoryEquip(containerId, slotIndex, equipmentIndex, true);
    }
    [Command]
    // in fact we do not swap, the target has to be empty for the action
    public void CmdSwapInventoryEquip(int containerId, int slotIndex, int equipmentIndex)
    {
        SwapInventoryEquip(containerId, slotIndex, equipmentIndex);
    }
    public void SwapInventoryEquip(int containerId, int slotIndex, int equipmentIndex, bool isSplit = false)
    {
        // only if not dead
        if (health > 0)
        {
            if (!inventory.GetItemSlot(containerId, slotIndex, out ItemSlot inventorySlot))
            {
                // target is inventory
                if (inventory.GetItemSlot(GlobalVar.containerEquipment, equipmentIndex, out ItemSlot equipmentSlot))
                // equipment contains item
                {
                    if (equipmentIndex == GlobalVar.equipmentBackpack)
                    {
                        if (!IsContainerMovePermitted(equipmentSlot, containerId))
                            return;
                    }
                    // make sure it is not an activated item like a burning torch
                    if (!CanMoveItemTo(equipmentSlot.item, containerId, slotIndex))
                    {
                        return;
                    }

                    inventory.AddItem(equipmentSlot.item, containerId, slotIndex, equipmentSlot.amount);
                    inventory.RemoveEquipment(equipmentIndex);
                    // item in hand changed?
                    if (GlobalFunc.IsInHand(GlobalVar.containerEquipment, equipmentIndex))
                    {
                        //Rebuild default spell
                        RebuildDefaultAttackSpell();
                        //Stop working
                        StopWorking();
                    }
                }
            }
            else
            {
                // target is equipment
                //slot empty
                if (inventory.IsSlotEmpty(GlobalVar.containerEquipment, equipmentIndex))
                {
                    //slot can take item
                    if (inventorySlot.item.data.CanEquip(this, equipmentIndex))
                    {
                        // make sure it is not an activated item like a burning torch to belt
                        if (!CanMoveItemTo(inventorySlot.item, GlobalVar.containerEquipment, equipmentIndex))
                        {
                            return;
                        }
                        // splitValue into hand always 1 item!
                        if (GlobalFunc.IsInHand(GlobalVar.containerEquipment, equipmentIndex) && isSplit)
                        {
                            inventory.AddItem(inventorySlot.item, GlobalVar.containerEquipment, equipmentIndex, 1);
                            inventory.DecreaseAmount(containerId, slotIndex, 1);
                        }
                        else
                        {
                            inventory.AddItem(inventorySlot.item, GlobalVar.containerEquipment, equipmentIndex, inventorySlot.amount);
                            inventory.Remove(containerId, slotIndex);
                        }

                        // item in hand changed?
                        if (GlobalFunc.IsInHand(GlobalVar.containerEquipment, equipmentIndex))
                        {
                            //Rebuild default spell
                            RebuildDefaultAttackSpell();
                            //Restart effect delay
                            RestartEffectDelay(equipmentIndex);
                            //Stop working
                            StopWorking();
                        }
                    }

                }
            }
            // source is hand
            if (GlobalFunc.IsInHand(containerId, slotIndex))
            {
                //Stop working
                StopWorking();
            }
        }
    }
    private bool CanMoveItemTo(Item sourceItem, int targetContainerId, int targetSlotIndex)
    {
        if (GlobalFunc.IsInHand(targetContainerId, targetSlotIndex))
        {
            // no problem into hand
            return true;
        }
        // item is not an activated item like a burning torch to belt
        if (sourceItem.data is LightItem && sourceItem.data2 == 1)
        {
            Inform("It would certainly look spectacular if you burn off your inventory. But you hold yourself back.");
            return false;
        }
        // currently no further limitation
        return true;
    }

    // trow the item to a distant location and remove from inventory
    [Command]
    void CmdPutItemAway(int containerId, int slotIndex, float x, float y, float z, int numberOfItems)
    {
        if (inventory.GetItemSlot(containerId, slotIndex, out ItemSlot itemSlot))
        {
            //close an open container when moved and put items in
            if (itemSlot.item.data is ContainerItem)
            {
                RpcCloseContainer(itemSlot.item.data1);
                ContainerItem ci = (ContainerItem)itemSlot.item.data;
                ci.PullFromInventory(this, itemSlot.item.data1);
            }
            //create only the amount in the slot, don't create additional items
            CreateItemOnGround(itemSlot.item, x, y, z, Mathf.Min(itemSlot.amount, numberOfItems));
            inventory.DecreaseAmount(containerId, slotIndex, numberOfItems);
            // item in hand changed?
            if (GlobalFunc.IsInHand(containerId, slotIndex))
            {
                //Rebuild default spell
                RebuildDefaultAttackSpell();
            }
        }
    }

    // Rebuild default spell
    public void RebuildDefaultAttackSpell()
    {
        // find current defaultSpell
        StandardFightingSpell activeSpell = null;
        int spellId = -1;
        if (WeaponEquipped(out WeaponItem weapon))
        {
            activeSpell = weapon.defaultSpell;
            spellId = spells.IdByName(activeSpell.name);
        }
        // rebuild if current active spell not found 
        if (spellId < 0)
        {
            // cancel casting
            currentSpell = -1;
            // remove all standard fighting spells
            foreach (Spell spell in spells.ToList())
            {
                if (spell.data is StandardFightingSpell)
                {
                    spells.Remove(spell);
                }
            }
            // add current spell if any
            if (activeSpell)
            {
                spells.Add(new Spell(activeSpell));
            }
        }
    }

    //restart effect delay for certain items
    void RestartEffectDelay(int equipmentIndex)
    {
        if (inventory.GetItemSlot(GlobalVar.containerEquipment, equipmentIndex, out ItemSlot itemSlot))
        {
            int currentSeconds = GameTime.SecondsSinceZero();
            //check fo delay items
            if (itemSlot.item.data is WandItem)
            {
                itemSlot.item.data1 = currentSeconds;
                inventory.AddOrReplace(itemSlot);
            }
        }
    }

    // create an item at a position on ground
    public void CreateItemOnGround(Item item, float x, float y, float z, int numberOfItems)
    {
        Vector3 pos = new Vector3(x, y, z);
        string area = Universal.GetArea(pos);
        Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
        GameObject element = Instantiate(Universal.EmptyElementPrefab, pos, targetRotation);
        element.transform.SetParent(GameObject.Find("Areas/" + area + "/DynamicElements").transform);
        ElementSlot es = element.GetComponent<ElementSlot>();
        es.item = item;
        es.amount = numberOfItems;
        es.applyToGround = true;
        NetworkServer.Spawn(element);
    }

    // find all elements in range and pick to inventory
    public void PickElementsInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, GlobalVar.pickRange, GlobalVar.layerMaskElement);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            // traverse down to find ElementSlot
            Transform t = hitColliders[i].transform;
            // tis is a hack since I found no proper while exit and want to avoid a while(true)
            for (int j = 0; j < 20; j++)
            {
                ElementSlot hitElement = t.gameObject.GetComponent<ElementSlot>();
                if (hitElement)
                {
                    if (hitElement.pickable)
                    {
                        CmdPickArbitraryElement(hitElement.netIdentity);
                        break;
                    }
                }
                if (t.parent == null)
                    break;
                t = t.parent.transform;
            }
        }
    }

    [Command]
    public void CmdPickArbitraryElement(NetworkIdentity ni)
    {
        // validate
        if (ni != null)
        {
            // store selected element while picking
            ElementSlot savedSelectedElement = selectedElement;

            selectedElement = ni.GetComponent<ElementSlot>();
            if (!PickSelectedElement())
                InformNoRepeat("Your greed was bigger than your bag. You have to leave something.", 1);

            selectedElement = savedSelectedElement;
        }
    }

    // move selected element into inventory
    [Command]
    public void CmdPickSelectedElement()
    {
        // it must be a selected element
        if (selectedElement)
        {
            // it must be pickable, we don't want a depot picked
            if (selectedElement.pickable)
            {
                if (!PickSelectedElement())
                    Inform("There is no room left in your bag. You really have to cleaning out the pig sty.");
            }
            else
                Inform("Even with brute force you do not get the thing in your bag.");
        }
    }

    // pick the element, check pickable before calling!
    [Server]
    private bool PickSelectedElement()
    {
        int containerIndexBackpack = containers.IndexOfId(ContainerIdOfBackpack());
        ContainerItem containerItem = null;
        // prepare special management of container
        if (selectedElement.item.data is ContainerItem)
        {
            containerItem = (ContainerItem)selectedElement.item.data;
            // if the backpack cannot take a container we do as if there is no backpack
            if (!IsContainerMovePermitted(containerIndexBackpack, false))
                containerIndexBackpack = -1;
        }
        ItemSlot itemSlot;
        // serach for slot with same item first, don't stack container
        // try first in the backpack
        if (containerIndexBackpack != -1 && !containerItem)
        {
            for (int i = 0; i < containers[containerIndexBackpack].slots; i++)
            {
                if (inventory.GetItemSlot(ContainerIdOfBackpack(), i, out itemSlot))
                {
                    // same Element and enough stack place
                    if (itemSlot.item.Equals(selectedElement.item) && itemSlot.item.data.maxStack - itemSlot.amount - selectedElement.amount > 0)
                    {
                        MergeSelectedElementItoSlot(ContainerIdOfBackpack(), i);
                        return true;
                    }
                }
            }
        }
        // maybe more luck in the belt, but not for containers
        if (!containerItem)
        {
            for (int i = GlobalVar.equipmentBelt1; i <= GlobalVar.equipmentBelt6; i++)
            {
                if (inventory.GetItemSlot(GlobalVar.containerEquipment, i, out itemSlot))
                {
                    // same Element and enough stack place
                    if (itemSlot.item.Equals(selectedElement.item) && itemSlot.item.data.maxStack - itemSlot.amount - selectedElement.amount > 0)
                    {
                        MergeSelectedElementItoSlot(GlobalVar.containerEquipment, i);
                        return true;
                    }
                }
            }
        }
        // find first empty item slot
        // try first in the backpack
        if (containerIndexBackpack != -1)
        {
            for (int i = 0; i < containers[containerIndexBackpack].slots; i++)
            {
                if (inventory.IsSlotEmpty(ContainerIdOfBackpack(), i))
                {
                    PickSelectedElementIntoSlot(ContainerIdOfBackpack(), i);
                    return true;
                }
            }
        }
        // maybe more luck in the belt / container in backpack
        if (containerItem)
        {
            if (inventory.IsSlotEmpty(GlobalVar.containerEquipment, GlobalVar.equipmentBackpack))
            {
                PickSelectedElementIntoSlot(GlobalVar.containerEquipment, GlobalVar.equipmentBackpack);
                return true;
            }
        }
        else
        {
            for (int i = GlobalVar.equipmentBelt1; i <= GlobalVar.equipmentBelt6; i++)
            {
                if (inventory.IsSlotEmpty(GlobalVar.containerEquipment, i))
                {
                    PickSelectedElementIntoSlot(GlobalVar.containerEquipment, i);
                    return true;
                }
            }
        }
        // no space for item
        return false;
    }
    // add selected element to existing stack
    [Server]
    private void MergeSelectedElementItoSlot(int containerId, int slotId)
    {
        inventory.IncreaseAmount(containerId, slotId, selectedElement.amount);
        Destroy(selectedElement.gameObject);

        selectedElement = null;
    }
    // put selected element into empty slot
    [Server]
    private void PickSelectedElementIntoSlot(int containerId, int slotId)
    {
        // special management of container
        if (selectedElement.item.data is ContainerItem)
        {
            ContainerItem containerItem = (ContainerItem)selectedElement.item.data;
            int newId = containerItem.PushToInventory(this);
            selectedElement.SetItemData(newId);
        }
        inventory.AddItem(selectedElement.item, containerId, slotId, selectedElement.amount);
        Destroy(selectedElement.gameObject);

        selectedElement = null;
    }

    // money //////////////////////////////////////////////////////////////////
    [Command]
    public void CmdChangeAvailableMoney(int money)
    {
        if (money == 0)
        {
            return;
        }
        if (money < 0)
        {
            //remove, verify in advance if possible
            if (!Money.TakeFromInventory(this, -money))
                Money.TakeAllFromInventory(this);
        }
        else
        {
            Money.AddToInventory(this, money);
        }
    }

    // working with phased element //////////////////////////////////////////////////////////////////
    [Server]
    public void selectedElementChangePhase(int phase)
    {
        // change display for phased elements on server and each client
        PhasedElement[] phaseElements = selectedElement.transform.GetComponentsInChildren<PhasedElement>();
        if (phaseElements.Length > 0)
        {
            phaseElements[0].phase = phase;
        }
        RpcSelectedElementChangePhase(selectedElement.netIdentity, phase);
    }
    [ClientRpc]
    public void RpcSelectedElementChangePhase(NetworkIdentity ni, int phase)
    {
        PhasedElement[] phaseElements = ni.transform.GetComponentsInChildren<PhasedElement>();
        if (phaseElements.Length > 0)
        {
            phaseElements[0].phase = phase;
        }
    }

    // skills //////////////////////////////////////////////////////////////////
    [Command]
    public void CmdLearnSkill(int skillToLearn, int levelOfAction, float timeSeconds)
    {
        LearnSkill(Skills.SkillFromId(skillToLearn), levelOfAction, timeSeconds);
    }
    public void LearnSkill(Skills.Skill skillToLearn, int levelOfAction, float timeSeconds)
    {
        if (CanSkill(skillToLearn))
        {
            int currentSkillLevel = skills.LevelOfSkill(skillToLearn);
            //as more time you have, as faster you learn
            float availableTimeRelation = Mathf.Sqrt((float)(1f * skillTotalTime / GlobalVar.skillBestWaitTime));
            //You learn best with actions related to your skill
            //killing easy prey a master learns nothing
            int levelDifference = levelOfAction - currentSkillLevel + GlobalVar.skillFitBestAt;
            float skillLevelRelation = NonLinearCurves.GetFloat0_1(GlobalVar.skillFitInstationary, levelDifference);
            // combine everything
            float appliedSkillTime = timeSeconds * availableTimeRelation * skillLevelRelation * GlobalVar.skillIdleRelation;
            int skillGain = Mathf.Min(GlobalVar.skillMaxExperiencePerAction, (int)(appliedSkillTime / Skills.TimePerLevel((Skills.Skill)skillToLearn, currentSkillLevel) * GlobalVar.skillExperiencePerLevel));
            if (skillGain > 0)
            {
                skills.AddExperience(skillToLearn, skillGain);
                skillTotalTime -= (int)appliedSkillTime;
            }
            if (skills.LevelOfSkill(skillToLearn) > currentSkillLevel)
            {
                // inform player by a TargetRpc
                Inform(string.Format("You feel a little bit more skilled in {0}.", Skills.Name(skillToLearn)));
                TargetFlashEffect(this.connectionToClient, GlobalVar.flashEffectSwirly);
            }
        }
    }
    //can the player gain any skill anymore?
    public bool CanSkill(int skillId)
    {
        if (skills.LevelOfId(skillId) >= 100)
            return false;
        if (skills.PointsInGroup(skillId) > Skills.MaxPointGroup(skillId))
            return false;
        return true;
    }
    public bool CanSkill(Skills.Skill skill)
    {
        return CanSkill(Skills.IdFromSkill(skill));
    }

    // inform player with a local only effect
    [TargetRpc]
    void TargetFlashEffect(NetworkConnection target, int effectId)
    {
        // flash a local effect
        Instantiate(Universal.LocalEffect(effectId), transform.position, transform.rotation);
    }

    // spells //////////////////////////////////////////////////////////////////
    public bool WeaponEquipped()
    {
        return WeaponEquipped(out WeaponItem notUsed);
    }
    public bool WeaponEquipped(out WeaponItem weaponItem)
    {
        // search first right hand
        if (inventory.GetEquipment(GlobalVar.equipmentRightHand, out ItemSlot itemSlot))
        {
            if (itemSlot.item.data is WeaponItem)
            {
                weaponItem = (WeaponItem)itemSlot.item.data;
                return true;
            }
        }
        // else left hand
        if (inventory.GetEquipment(GlobalVar.equipmentLeftHand, out itemSlot))
        {
            if (itemSlot.item.data is WeaponItem)
            {
                weaponItem = (WeaponItem)itemSlot.item.data;
                return true;
            }
        }
        weaponItem = null;
        return false;
    }
    public bool GatheringToolEquipped(out GatheringToolItem toolItem)
    {
        // search first right hand
        if (inventory.GetEquipment(GlobalVar.equipmentRightHand, out ItemSlot itemSlot))
        {
            if (itemSlot.item.data is GatheringToolItem)
            {
                toolItem = (GatheringToolItem)itemSlot.item.data;
                return true;
            }
        }
        // else left hand
        if (inventory.GetEquipment(GlobalVar.equipmentLeftHand, out itemSlot))
        {
            if (itemSlot.item.data is GatheringToolItem)
            {
                toolItem = (GatheringToolItem)itemSlot.item.data;
                return true;
            }
        }
        toolItem = null;
        return false;
    }
    public override bool HasCastWeapon()
    {
        return WeaponEquipped();
    }

    // CanAttack check
    // we use 'is' instead of 'GetType' so that it works for inherited types too
    public override bool CanAttack(Entity entity)
    {
        return base.CanAttack(entity) &&
               (entity is Monster ||
                entity is Player ||
                (entity is Pet && entity != activePet) ||
                (entity is Mount && entity != activeMount));
    }

    [Command]
    public void CmdUseSpell(int spellIndex)
    {
        // validate
        if ((state == GlobalVar.stateIdle || state == GlobalVar.stateMoving || state == GlobalVar.stateCasting) &&
            0 <= spellIndex && spellIndex < spells.Count)
        {
            // spell can be casted?
            if (spells[spellIndex].IsReady())
            {
                currentSpell = spellIndex;
            }
        }
    }

    // helper function: try attack with standard fighting
    [Client]
    public void TryAttackStandardFight(bool ignoreState = false)
    {
        int spellIndex = spells.IdOfStandardFighting();
        if (spellIndex >= 0)
        {
            TryUseSpell(spellIndex, ignoreState);
        }
    }
    // helper function: try to use a spell and walk into range if necessary
    [Client]
    public void TryUseSpell(int spellIndex, bool ignoreState = false)
    {
        // only if not casting already
        // (might need to ignore that when coming from pending spell where
        //  CASTING is still true)
        if (state != GlobalVar.stateCasting || ignoreState)
        {
            Spell spell = spells[spellIndex];
            if (CastCheckSelf(spell) && CastCheckTarget(spell))
            {
                // check distance between self and target
                Vector3 destination;
                if (CastCheckDistance(spell, out destination))
                {
                    // cast
                    CmdUseSpell(spellIndex);
                }
                else
                {
                    // move to the target first
                    // (use collider point(s) to also work with big entities)
                    agent.stoppingDistance = spell.CastRange(this) * attackToMoveRangeRatio;
                    agent.destination = destination;

                    // use spell when there
                    useSpellWhenCloser = spellIndex;
                }
            }
        }
        else
        {
            pendingSpell = spellIndex;
        }
    }

    public bool HasLearnedSpell(string spellName)
    {
        return spells.Any(spell => spell.name == spellName);
    }

    public bool CanLearnSpell(string spellName)
    {
        return ((state == GlobalVar.stateIdle || state == GlobalVar.stateMoving || state == GlobalVar.stateCasting) &&
            !HasLearnedSpell(spellName));
    }
    // -> this is for learning!
    [Command]
    public void CmdLearnSpell(string spellName)
    {
        // validate
        if (CanLearnSpell(spellName))
        {
            // can be learned?
            ScriptableSpell scriptableSpell = ScriptableSpell.dict[spellName.GetStableHashCode()];
            Spell spell = new Spell(scriptableSpell);
            spells.Add(spell);
            Inform(string.Format("You learned the spell: '{0}'", spell.displayName));
            TargetFlashEffect(this.connectionToClient, GlobalVar.flashEffectSwirly);
        }
    }

    // spellbar ////////////////////////////////////////////////////////////////
    //[Client] <- disabled while UNET OnDestroy isLocalPlayer bug exists
    void SaveSpellbar()
    {
        // save spellbar to player prefs (based on player name, so that
        // each character can have a different spellbar)
        for (int i = 0; i < spellbar.Length; ++i)
            PlayerPrefs.SetString(name + "_spellbar_" + i, spellbar[i].reference);

        // force saving playerprefs, otherwise they aren't saved for some reason
        PlayerPrefs.Save();
    }

    [Client]
    void LoadSpellbar()
    {
        List<Spell> learned = spells.ToList();
        for (int i = 0; i < spellbar.Length; ++i)
        {
            // try loading an existing entry
            if (PlayerPrefs.HasKey(name + "_spellbar_" + i))
            {
                string entry = PlayerPrefs.GetString(name + "_spellbar_" + i, "");

                // is this a valid learned spell?
                // (might be an old character's playerprefs)
                // => only allow learned spells (in case it's an old character's
                //    spell that we also have, but haven't learned yet)
                if (HasLearnedSpell(entry))
                {
                    spellbar[i].reference = entry;
                }
            }
            // otherwise fill with default spells for a better first impression
            else if (i < learned.Count)
            {
                spellbar[i].reference = learned[i].name;
            }
        }
    }

    // quests //////////////////////////////////////////////////////////////////
    public int GetQuestIndexByName(string questName)
    {
        return quests.FindIndex(quest => quest.name == questName);
    }

    // helper function to check if the player has completed a quest before
    public bool HasCompletedQuest(string questName)
    {
        return quests.Any(q => q.name == questName && q.completed);
    }

    // helper function to check if a player has an active (not completed) quest
    public bool HasActiveQuest(string questName)
    {
        return quests.Any(q => q.name == questName && !q.completed);
    }

    [Server]
    public void QuestsOnKilled(Entity victim)
    {
        // call OnKilled in all active (not completed) quests
        for (int i = 0; i < quests.Count; ++i)
            if (!quests[i].completed)
                quests[i].OnKilled(this, i, victim);
    }

    [Server]
    public void QuestsOnLocation(Collider location)
    {
        // call OnLocation in all active (not completed) quests
        for (int i = 0; i < quests.Count; ++i)
            if (!quests[i].completed)
                quests[i].OnLocation(this, i, location);
    }

    // helper function to check if the player can accept a new quest
    // note: no quest.completed check needed because we have a'not accepted yet'
    //       check
    public bool CanAcceptQuest(ScriptableQuest quest)
    {
        // not too many quests yet?
        // has required level?
        // not accepted yet?
        // has finished predecessor quest (if any)?
        return quests.Count(q => !q.completed) < activeQuestLimit &&
               GetQuestIndexByName(quest.name) == -1 && // not accepted yet?
               (quest.predecessor == null || HasCompletedQuest(quest.predecessor.name));
    }

    [Command]
    public void CmdAcceptQuest(int npcQuestIndex)
    {
        // validate
        // use collider point(s) to also work with big entities
        if (state == GlobalVar.stateIdle &&
            target != null &&
            target.health > 0 &&
            target is Npc &&
            0 <= npcQuestIndex && npcQuestIndex < ((Npc)target).quests.Length &&
            Utils.ClosestDistance(collider, target.collider) <= interactionRange &&
            CanAcceptQuest(((Npc)target).quests[npcQuestIndex]))
        {
            ScriptableQuest npcQuest = ((Npc)target).quests[npcQuestIndex];
            quests.Add(new Quest(npcQuest));
        }
    }

    // helper function to check if the player can complete a quest
    public bool CanCompleteQuest(string questName)
    {
        // has the quest and not completed yet?
        int index = GetQuestIndexByName(questName);
        if (index != -1 && !quests[index].completed)
        {
            // fulfilled?
            Quest quest = quests[index];
            if (quest.IsFulfilled(this))
            {
                // enough space for reward item (if any)?
                return quest.rewardItem == null || InventoryCanAdd(new Item(quest.rewardItem), 1, ContainerIdOfBackpack());
            }
        }
        return false;
    }

    [Command]
    public void CmdCompleteQuest(int npcQuestIndex)
    {
        // validate
        // use collider point(s) to also work with big entities
        if (state == GlobalVar.stateIdle &&
            target != null &&
            target.health > 0 &&
            target is Npc &&
            0 <= npcQuestIndex && npcQuestIndex < ((Npc)target).quests.Length &&
            Utils.ClosestDistance(collider, target.collider) <= interactionRange)
        {
            ScriptableQuest npcQuest = ((Npc)target).quests[npcQuestIndex];
            int index = GetQuestIndexByName(npcQuest.name);
            if (index != -1)
            {
                // can complete it? (also checks inventory space for reward, if any)
                Quest quest = quests[index];
                if (CanCompleteQuest(quest.name))
                {
                    // call quest.OnCompleted to remove quest items from
                    // inventory, etc.
                    quest.OnCompleted(this);

                    // gain rewards
                    Money.AddToInventory(this, quest.rewardMoney);
                    if (quest.rewardItem != null)
                        InventoryAdd(new Item(quest.rewardItem), 1, ContainerIdOfBackpack());

                    // complete quest
                    quest.completed = true;
                    quests[index] = quest;
                }
            }
        }
    }

    // npc trading /////////////////////////////////////////////////////////////
    // player bought item from NPC
    [Command]
    public void CmdNpcBuyItem(string itemName, int durability, int quality, long price)
    {
        // pay
        Money.TakeFromInventory(this, (int)price);
        // create item
        AddItemToAvailableInventory(itemName, 1, durability, quality, "");
    }

    // player sold item to NPC
    [Command]
    public void CmdNpcSellItem(int containerId, int slotIndex, long price)
    {
        // make sure the item has not been sold already
        if (!inventory.IsSlotEmpty(containerId, slotIndex))
        {
            // money to player
            Money.AddToInventory(this, (int)price);
            // remove item from inventory
            inventory.Remove(containerId, slotIndex);
        }
    }

    // player removed item
    [Command]
    public void CmdRemoveItem(int containerId, int slotIndex, int amount)
    {
        // make sure the item exists
        if (!inventory.IsSlotEmpty(containerId, slotIndex))
        {
            // remove item from inventory
            inventory.DecreaseAmount(containerId, slotIndex, amount);
        }
    }

    // npc teleport ////////////////////////////////////////////////////////////
    [Command]
    public void CmdNpcTeleport()
    {
        // validate
        if (state == GlobalVar.stateIdle &&
            target != null &&
            target.health > 0 &&
            target is Npc &&
            Utils.ClosestDistance(collider, target.collider) <= interactionRange &&
            ((Npc)target).teleportTo != null)
        {
            TeleportToPosition(((Npc)target).teleportTo.position);
        }
    }

    // teleport ////////////////////////////////////////////////////////////////
    [Client]
    public void TeleportTo(Vector3 targetPos, float direction = 0)
    {
        agent.ResetMovement();
        agent.Warp(targetPos);
        transform.rotation = Quaternion.Euler(new Vector3(0, direction, 0));
        CmdTeleportTo(targetPos.x, targetPos.y, targetPos.z, direction);
    }
    [Command]
    void CmdTeleportTo(float x, float y, float z, float direction)
    {
        TeleportToPosition(new Vector3(x, y, z), direction);
    }
    [Server]
    public void TeleportToPosition(Vector3 targetPos, float direction = 0, bool global = true)
    {
        Vector3 targetRotation = new Vector3(0, direction, 0);
        agent.ResetMovement();
        agent.Warp(targetPos);
        currentArea = Universal.GetArea(targetPos);
        canSeeSky = Universal.GetAreaSky(currentArea);

        // change direction and inform all
        transform.rotation = Quaternion.Euler(targetRotation);
        // some teleports durin character creation cause trouble since the character is not known everywhere
        if (global)
        {
            RpcUpdatePosition(targetPos.x, targetPos.y, targetPos.z, direction);
        }
    }

    [Command]
    public void CmdTeleportToPlayer(int targetId)
    {
        Player targetPlayer = GetPlayerById(targetId);
        if (targetPlayer)
        {
            Vector3 targetPos = Universal.FindPossiblePositionAround(targetPlayer.transform.position, GlobalVar.gmTeleportDistance);
            float direction = Quaternion.LookRotation(targetPlayer.transform.position - targetPos, Vector3.up).eulerAngles.y;
            TeleportToPosition(targetPos, direction);
        }
    }
    [Command]
    public void CmdTeleportPullTarget(float x, float y, float z, int playerId, float direction)
    {
        Vector3 targetPos = new Vector3(x, y, z);
        if (target && playerId == 0)
        {
            // short distance transfer - use target
            if (target is Player)
            {
                ((Player)target).TeleportToPosition(targetPos, direction);
                gmState = GameMaster.useTeleport(gmState);
            }
            else
            {
                // move NPC
                target.agent.ResetMovement();
                target.agent.Warp(targetPos);
            }
        }
        else if (playerId != 0)
        {
            // long distance transfer
            Player targetPlayer = GetPlayerById(playerId);
            if (targetPlayer)
            {
                targetPlayer.TeleportToPosition(targetPos, direction);
                gmState = GameMaster.useTeleport(gmState);
            }
        }
    }


    public void AskForTeleport(int senderId, float x, float y, float z, float range)
    {
        Player sender = GetPlayerById(senderId);
        float dist = Vector3.Distance(this.transform.position, sender.transform.position) - GlobalVar.teleportPemittedMove;
        // make sure Teleport target has not moved far away
        if (dist <= range)
        {
            TargetAskForTeleport(connectionToClient, senderId, x, y, z, range);
        }
    }
    [TargetRpc] // ask the one client
    public void TargetAskForTeleport(NetworkConnection target, int senderId, float x, float y, float z, float range)
    {
        Player sender = GetPlayerById(senderId);
        UITeleportInvitation teleportInvitation = GameObject.Find("Canvas/TeleportInvite").GetComponent<UITeleportInvitation>();
        teleportInvitation.Initialize(sender, new Vector3(x, y, z), true);
    }


    // player to player trading ////////////////////////////////////////////////
    // how trading works:
    // 1. A invites his target with CmdTradeRequest()
    //    -> sets B.tradeInvitationFrom = A;
    // 2. B sees a UI window and accepts (= invites A too)
    //    -> sets A.tradeInvitationFrom = B;
    // 3. the TradeStart event is fired, both go to 'TRADING' state
    // 4. they lock the trades
    // 5. they accept, then items are swapped

    public bool CanStartTrade()
    {
        // a player can only trade if he is not trading already and alive
        return health > 0 && state != GlobalVar.stateTrading;
    }

    public bool CanStartTradeWith(Entity entity)
    {
        // can we trade? can the target trade? are we close enough?
        return entity != null && entity is Player && entity != this &&
               CanStartTrade() && ((Player)entity).CanStartTrade() &&
               Utils.ClosestDistance(collider, entity.collider) <= interactionRange;
    }

    // request a trade with the target player.
    [Command]
    public void CmdTradeRequestSend()
    {
        // validate
        if (CanStartTradeWith(target))
        {
            // send a trade request to target
            ((Player)target).tradeRequestFrom = name;
            print(name + " invited " + target.name + " to trade");
        }
    }

    // helper function to find the guy who sent us a trade invitation
    [Server]
    Player FindPlayerFromTradeInvitation()
    {
        if (tradeRequestFrom != "" && onlinePlayers.ContainsKey(tradeRequestFrom))
            return onlinePlayers[tradeRequestFrom];
        return null;
    }

    // accept a trade invitation by simply setting 'requestFrom' for the other
    // person to self
    [Command]
    public void CmdTradeRequestAccept()
    {
        Player sender = FindPlayerFromTradeInvitation();
        if (sender != null)
        {
            if (CanStartTradeWith(sender))
            {
                // also send a trade request to the person that invited us
                sender.tradeRequestFrom = name;
                print(name + " accepted " + sender.name + "'s trade request");
            }
        }
    }

    // decline a trade invitation
    [Command]
    public void CmdTradeRequestDecline()
    {
        tradeRequestFrom = "";
    }

    [Server]
    void TradeCleanup()
    {
        // clear all trade related properties
        for (int i = 0; i < tradeOfferItems.Count; ++i) tradeOfferItems[i] = -1;
        tradeStatus = TradeStatus.Free;
        tradeRequestFrom = "";
    }

    [Command]
    public void CmdTradeCancel()
    {
        // validate
        if (state == GlobalVar.stateTrading)
        {
            // clear trade request for both guys. the FSM event will do the rest
            Player player = FindPlayerFromTradeInvitation();
            if (player != null) player.tradeRequestFrom = "";
            tradeRequestFrom = "";
        }
    }

    [Command]
    public void CmdTradeOfferLock()
    {
        // validate
        if (state == GlobalVar.stateTrading)
            tradeStatus = TradeStatus.Locked;
    }

    [Command]
    public void CmdTradeOfferItem(int inventoryIndex, int offerIndex)
    {
        // validate
        if (state == GlobalVar.stateTrading && tradeStatus == TradeStatus.Free &&
            0 <= offerIndex && offerIndex < tradeOfferItems.Count &&
            !tradeOfferItems.Contains(inventoryIndex) && // only one reference
            0 <= inventoryIndex && inventoryIndex < inventory.Count)
        {
            ItemSlot slot = inventory[inventoryIndex];
            if (slot.amount > 0 && !slot.item.objectInGame)
                tradeOfferItems[offerIndex] = inventoryIndex;
        }
    }

    [Command]
    public void CmdTradeOfferItemClear(int offerIndex)
    {
        // validate
        if (state == GlobalVar.stateTrading && tradeStatus == TradeStatus.Free &&
            0 <= offerIndex && offerIndex < tradeOfferItems.Count)
            tradeOfferItems[offerIndex] = -1;
    }

    [Server]
    bool IsTradeOfferStillValid()
    {
        // all offered items are -1 or valid?
        return tradeOfferItems.All(index => index == -1 ||
                             (0 <= index && index < inventory.Count && inventory[index].amount > 0));
    }

    [Server]
    int TradeOfferItemSlotAmount()
    {
        return tradeOfferItems.Count(i => i != -1);
    }

    [Server]
    int InventorySlotsNeededForTrade()
    {
        // if other guy offers 2 items and we offer 1 item then we only need
        // 2-1 = 1 slots. and the other guy would need 1-2 slots and at least 0.
        if (target != null && target is Player)
        {
            Player other = (Player)target;
            int otherAmount = other.TradeOfferItemSlotAmount();
            int myAmount = TradeOfferItemSlotAmount();
            return Mathf.Max(otherAmount - myAmount, 0);
        }
        return 0;
    }

    [Command]
    public void CmdTradeOfferAccept()
    {
        // validate
        // note: distance check already done when starting the trade
        if (state == GlobalVar.stateTrading && tradeStatus == TradeStatus.Locked &&
            target != null && target is Player)
        {
            Player other = (Player)target;

            // other has locked?
            if (other.tradeStatus == TradeStatus.Locked)
            {
                //  simply accept and wait for the other guy to accept too
                tradeStatus = TradeStatus.Accepted;
                print("first accept by " + name);
            }
            // other has accepted already? then both accepted now, start trade.
            else if (other.tradeStatus == TradeStatus.Accepted)
            {
                // accept
                tradeStatus = TradeStatus.Accepted;
                print("second accept by " + name);

                // both offers still valid?
                if (IsTradeOfferStillValid() && other.IsTradeOfferStillValid())
                {
                    // both have enough inventory slots?
                    // note: we don't use InventoryCanAdd here because:
                    // - current solution works if both have full inventories
                    // - InventoryCanAdd only checks one slot. here we have
                    //   multiple slots though (it could happen that we can
                    //   not add slot 2 after we did add slot 1's items etc)
                    if (InventorySlotsFree(ContainerIdOfBackpack()) >= InventorySlotsNeededForTrade() &&
                        other.InventorySlotsFree(other.ContainerIdOfBackpack()) >= other.InventorySlotsNeededForTrade())
                    {
                        // exchange the items by first taking them out
                        // into a temporary list and then putting them
                        // in. this guarantees that exchanging even
                        // works with full inventories

                        // take them out
                        Queue<ItemSlot> tempMy = new Queue<ItemSlot>();
                        foreach (int index in tradeOfferItems)
                        {
                            if (index != -1)
                            {
                                ItemSlot slot = inventory[index];
                                tempMy.Enqueue(slot);
                                slot.amount = 0;
                                inventory[index] = slot;
                            }
                        }

                        Queue<ItemSlot> tempOther = new Queue<ItemSlot>();
                        foreach (int index in other.tradeOfferItems)
                        {
                            if (index != -1)
                            {
                                ItemSlot slot = other.inventory[index];
                                tempOther.Enqueue(slot);
                                slot.amount = 0;
                                other.inventory[index] = slot;
                            }
                        }

                        // put them into the free slots
                        for (int i = 0; i < inventory.Count; ++i)
                            if (inventory[i].amount == 0 && tempOther.Count > 0)
                                inventory[i] = tempOther.Dequeue();

                        for (int i = 0; i < other.inventory.Count; ++i)
                            if (other.inventory[i].amount == 0 && tempMy.Count > 0)
                                other.inventory[i] = tempMy.Dequeue();

                        // did it all work?
                        if (tempMy.Count > 0 || tempOther.Count > 0)
                            Debug.LogWarning("item trade problem");
                    }
                }
                else print("trade canceled (invalid offer)");

                // clear trade request for both guys. the FSM event will do the
                // rest
                tradeRequestFrom = "";
                other.tradeRequestFrom = "";
            }
        }
    }

    // crafting ////////////////////////////////////////////////////////////////
    // the crafting system is designed to work with all kinds of commonly known
    // crafting options:
    // - item combinations: wood + stone = axe
    // - weapon upgrading: axe + gem = strong axe
    // - recipe items: axerecipe(item) + wood(item) + stone(item) = axe(item)
    //
    // players can craft at all times, not just at npcs, because that's the most
    // realistic option

    // craft the current combination of items and put result into inventory
    [Command]
    public void CmdCraft(int[] indices)
    {
        // validate: between 1 and 6, all valid, no duplicates?
        // -> can be IDLE or MOVING (in which case we reset the movement)
        if ((state == GlobalVar.stateIdle || state == GlobalVar.stateMoving) &&
            indices.Length == ScriptableRecipe.recipeSize)
        {
            // find valid indices that are not '-1' and make sure there are no
            // duplicates
            List<int> validIndices = indices.Where(index => 0 <= index && index < inventory.Count && inventory[index].amount > 0).ToList();
            if (validIndices.Count > 0 && !validIndices.HasDuplicates())
            {
                // build list of item templates from valid indices
                List<ScriptableItem> items = validIndices.Select(index => inventory[index].item.data).ToList();

                // find recipe
                ScriptableRecipe recipe = ScriptableRecipe.dict.Values.ToList().Find(r => r.CanCraftWith(items)); // good enough for now
                if (recipe != null && recipe.result != null)
                {
                    // enough space?
                    Item result = new Item(recipe.result);
                    if (InventoryCanAdd(result, 1, ContainerIdOfBackpack()))
                    {
                        // store the crafting indices on the server. no need for
                        // a SyncList and unnecessary broadcasting.
                        // we already have a 'craftingIndices' variable anyway.
                        craftingIndices = indices.ToList();

                        // start crafting
                        craftingRequested = true;
                        craftingTimeEnd = NetworkTime.time + recipe.craftingTime;
                    }
                }
            }
        }
    }

    // finish the crafting
    void Craft()
    {
        // should only be called while CRAFTING
        // -> we already validated everything in CmdCraft. let's just craft.
        if (state == GlobalVar.stateCrafting)
        {
            // build list of item templates from indices
            List<int> validIndices = craftingIndices.Where(index => 0 <= index && index < inventory.Count && inventory[index].amount > 0).ToList();
            List<ScriptableItem> items = validIndices.Select(index => inventory[index].item.data).ToList();

            // find recipe
            ScriptableRecipe recipe = ScriptableRecipe.dict.Values.ToList().Find(r => r.CanCraftWith(items)); // good enough for now
            if (recipe != null && recipe.result != null)
            {
                // enough space?
                Item result = new Item(recipe.result);
                if (InventoryCanAdd(result, 1, ContainerIdOfBackpack()))
                {
                    // remove the ingredients from inventory in any case
                    foreach (int index in validIndices)
                    {
                        // decrease item amount
                        ItemSlot slot = inventory[index];
                        //>>> geht so nicht CraftingSystem slot.DecreaseAmount(1);
                        inventory[index] = slot;
                    }

                    // roll the dice to decide if we add the result or not
                    // IMPORTANT: we use rand() < probability to decide.
                    // => UnityEngine.Random.value is [0,1] inclusive:
                    //    for 0% probability it's fine because it's never '< 0'
                    //    for 100% probability it's not because it's not always '< 1', it might be == 1
                    //    and if we use '<=' instead then it won't work for 0%
                    // => C#'s Random value is [0,1) exclusive like most random
                    //    functions. this works fine.
                    if (new System.Random().NextDouble() < recipe.probability)
                    {
                        // add result item to inventory
                        InventoryAdd(new Item(recipe.result), 1, ContainerIdOfBackpack());
                        TargetCraftingSuccess(connectionToClient);
                    }
                    else
                    {
                        TargetCraftingFailed(connectionToClient);
                    }

                    // clear indices afterwards
                    // note: we set all to -1 instead of calling .Clear because
                    //       that would clear all the slots in host mode.
                    for (int i = 0; i < ScriptableRecipe.recipeSize; ++i)
                        craftingIndices[i] = -1;
                }
            }
        }
    }

    // two rpcs for results to save 1 byte for the actual result
    [TargetRpc] // only send to one client
    public void TargetCraftingSuccess(NetworkConnection target)
    {
        craftingState = CraftingState.Success;
    }

    [TargetRpc] // only send to one client
    public void TargetCraftingFailed(NetworkConnection target)
    {
        craftingState = CraftingState.Failed;
    }

    // pvp murder system ///////////////////////////////////////////////////////
    // attacking someone innocent results in Offender status
    //   (can be attacked without penalty for a short time)
    // killing someone innocent results in Murderer status
    //   (can be attacked without penalty for a long time + negative buffs)
    // attacking/killing a Offender/Murderer has no penalty
    //
    // we use buffs for the offender/status because buffs have all the features
    // that we need here.
    public bool IsOffender()
    {
        return offenderBuff != null && buffs.Any(buff => buff.name == offenderBuff.name);
    }

    public bool IsMurderer()
    {
        return murdererBuff != null && buffs.Any(buff => buff.name == murdererBuff.name);
    }

    public void StartOffender()
    {
        if (offenderBuff != null) AddOrRefreshBuff(new Buff(offenderBuff, 1));
    }

    public void StartMurderer()
    {
        if (murdererBuff != null) AddOrRefreshBuff(new Buff(murdererBuff, 1));
    }

    // guild ///////////////////////////////////////////////////////////////////
    public bool InGuild()
    {
        // only if both are true, otherwise we might be in the middle of leaving
        return guildName != "" && guild.GetMemberIndex(name) != -1;
    }

    [Server]
    static void BroadcastGuildChanges(string guildName, Guild guild)
    {
        // save in database
        Database.SaveGuild(guildName, guild.notice, guild.members.ToList());

        // copy to every online member. we don't just reload from db because the
        // online status is only available in the members list
        foreach (GuildMember member in guild.members)
        {
            if (onlinePlayers.ContainsKey(member.name))
            {
                Player player = onlinePlayers[member.name];
                player.guildName = guildName;
                player.guild = guild;
            }
        }
    }

    // helper function to clear guild variables sync for kick/leave/terminate
    [Server]
    void ClearGuild()
    {
        guildName = "";
        guild = new Guild();
    }

    [Server]
    public void SetGuildOnline(bool online)
    {
        if (InGuild())
        {
            guild.SetOnline(name, online);
            BroadcastGuildChanges(guildName, guild);
        }
    }

    [Command]
    public void CmdGuildInviteTarget()
    {
        // validate
        if (target != null && target is Player &&
            InGuild() && !((Player)target).InGuild() &&
            guild.CanInvite(name, target.name) &&
            NetworkTime.time >= nextRiskyActionTime &&
            Utils.ClosestDistance(collider, target.collider) <= interactionRange)
        {
            // send a invite and reset risky time
            ((Player)target).guildInviteFrom = name;
            nextRiskyActionTime = NetworkTime.time + guildInviteWaitSeconds;
            print(name + " invited " + target.name + " to guild");
        }
    }

    [Command]
    public void CmdGuildInviteAccept()
    {
        // valid invitation?
        // note: no distance check because sender might be far away already
        if (!InGuild() && guildInviteFrom != "" &&
            onlinePlayers.ContainsKey(guildInviteFrom))
        {
            // can sender actually invite us?
            Player sender = onlinePlayers[guildInviteFrom];
            if (sender.InGuild() && sender.guild.CanInvite(sender.name, name))
            {
                // add self to sender's guild members list
                sender.guild.AddMember(name);

                // broadcast and save changes from sender to everyone
                BroadcastGuildChanges(sender.guildName, sender.guild);
                print(sender.name + " added " + name + " to guild: " + guildName);
            }
        }

        // reset guild invite in any case
        guildInviteFrom = "";
    }

    [Command]
    public void CmdGuildInviteDecline()
    {
        guildInviteFrom = "";
    }

    [Command]
    public void CmdGuildKick(string memberName)
    {
        // validate
        if (InGuild() && guild.CanKick(name, memberName))
        {
            // remove from member list
            guild.RemoveMember(memberName);

            // broadcast and save changes
            BroadcastGuildChanges(guildName, guild);

            // clear variables for the kicked person
            if (onlinePlayers.ContainsKey(memberName))
                onlinePlayers[memberName].ClearGuild();
            print(name + " kicked " + memberName + " from guild: " + guildName);
        }
    }

    [Command]
    public void CmdGuildPromote(string memberName)
    {
        // validate
        if (InGuild() && guild.CanPromote(name, memberName))
        {
            // promote the member
            guild.PromoteMember(memberName);

            // broadcast and save changes
            BroadcastGuildChanges(guildName, guild);
            print(name + " promoted " + memberName + " in guild: " + guildName);
        }
    }

    [Command]
    public void CmdGuildDemote(string memberName)
    {
        // validate
        if (InGuild() && guild.CanDemote(name, memberName))
        {
            // demote the member
            guild.DemoteMember(memberName);

            // broadcast and save changes
            BroadcastGuildChanges(guildName, guild);
            print(name + " demoted " + memberName + " in guild: " + guildName);
        }
    }

    [Command]
    public void CmdSetGuildNotice(string notice)
    {
        // validate
        // (only allow changes every few seconds to avoid bandwidth issues)
        if (InGuild() && guild.CanNotify(name) &&
            notice.Length < Guild.NoticeMaxLength &&
            NetworkTime.time >= nextRiskyActionTime)
        {
            // set notice and reset next time
            guild.notice = notice;
            nextRiskyActionTime = NetworkTime.time + Guild.NoticeWaitSeconds;

            // broadcast and save changes
            BroadcastGuildChanges(guildName, guild);
            print(name + " changed guild notice to: " + guild.notice);
        }
    }

    // helper function to check if we are near a guild manager npc
    public bool IsGuildManagerNear()
    {
        return target != null &&
               target is Npc &&
               ((Npc)target).offersGuildManagement &&
               Utils.ClosestDistance(collider, target.collider) <= interactionRange;
    }

    [Command]
    public void CmdTerminateGuild()
    {
        // validate
        if (InGuild() && IsGuildManagerNear() && guild.CanTerminate(name))
        {
            // remove guild from database
            Database.RemoveGuild(guildName);

            // clear player variables
            ClearGuild();
        }
    }

    [Command]
    public void CmdCreateGuild(string newGuildName)
    {
        // validate
        if (health > 0 && IsGuildManagerNear() && !InGuild() && Money.AvailableMoney(this) >= Guild.CreationPrice)
        {
            if (Guild.IsValidGuildName(newGuildName) &&
                !Database.GuildExists(newGuildName)) // db check only on server, no Guild.CanCreate function because client has no DB.
            {
                // pay
                Money.TakeFromInventory(this, Guild.CreationPrice);

                // set guild and add self to members list as highest rank
                guildName = newGuildName;
                guild.notice = ""; // avoid null
                guild.AddMember(name, GuildRank.Master);

                // (broadcast and) save changes
                BroadcastGuildChanges(guildName, guild);
                print(name + " created guild: " + guildName);
            }
            else
            {
                string message = "Guild name invalid!"; // exists or invalid regex
                chat.TargetMsgInfo(connectionToClient, message);
            }
        }
    }

    [Command]
    public void CmdLeaveGuild()
    {
        // validate
        if (InGuild() && guild.CanLeave(name))
        {
            // remove self from members list
            guild.RemoveMember(name);

            // broadcast and save changes
            BroadcastGuildChanges(guildName, guild);

            // reset guild info and members list for the person that left
            ClearGuild();
        }
    }

    // party ///////////////////////////////////////////////////////////////////
    public bool InParty()
    {
        return party.members != null && party.members.Length > 0;
    }

    [Server]
    static void BroadcastPartyChanges(Party party)
    {
        // copy to every online member. we don't just reload from db because the
        // online status is only available in the members list
        // (call TargetRpc on that GameObject for that connection)
        foreach (string member in party.members)
        {
            if (onlinePlayers.ContainsKey(member))
            {
                Player player = onlinePlayers[member];
                player.party = party;
                player.TargetPartySync(player.connectionToClient, party);
            }
        }
    }

    // sending party info to all observers would be bandwidth overkill, so we
    // use a targetrpc
    [TargetRpc] // only send to one client
    public void TargetPartySync(NetworkConnection target, Party party)
    {
        this.party = party;
    }

    // helper function to clear party variables sync for kick/leave/dismiss
    [Server]
    void ClearParty()
    {
        party = new Party();
        TargetPartySync(connectionToClient, party);
    }

    // find party members in proximity for item/exp sharing etc.
    public List<Player> GetPartyMembersInProximity()
    {
        if (InParty())
        {
            return netIdentity.observers.Values
                                        .Select(conn => conn.playerController.GetComponent<Player>())
                                        .Where(p => party.GetMemberIndex(p.name) != -1)
                                        .ToList();
        }
        return new List<Player>();
    }

    // party invite by name (not by target) so that chat commands are possible
    // if needed
    [Command]
    public void CmdPartyInvite(string otherName)
    {
        // validate: is there someone with that name, and not self?
        if (otherName != name && onlinePlayers.ContainsKey(otherName) &&
            NetworkTime.time >= nextRiskyActionTime)
        {
            Player other = onlinePlayers[otherName];

            // can only send invite if no party yet or party isn't full and
            // have invite rights and other guy isn't in party yet
            if ((!InParty() || party.CanInvite(name)) && !other.InParty())
            {
                // send a invite and reset risky time
                other.partyInviteFrom = name;
                nextRiskyActionTime = NetworkTime.time + partyInviteWaitSeconds;
                print(name + " invited " + other.name + " to party");
            }
        }
    }

    [Command]
    public void CmdPartyInviteAccept()
    {
        // valid invitation?
        // note: no distance check because sender might be far away already
        if (!InParty() && partyInviteFrom != "" &&
            onlinePlayers.ContainsKey(partyInviteFrom))
        {
            // can sender actually invite us?
            Player sender = onlinePlayers[partyInviteFrom];

            // -> either he is in a party and can still invite someone
            if (sender.InParty() && sender.party.CanInvite(sender.name))
            {
                sender.party.AddMember(name);
                BroadcastPartyChanges(sender.party);
                print(sender.name + " added " + name + " to " + sender.party.members[0] + "'s party");
                // -> or he is not in a party and forms a new one
            }
            else if (!sender.InParty())
            {
                sender.party.AddMember(sender.name); // master
                sender.party.AddMember(name);
                BroadcastPartyChanges(sender.party);
                print(sender.name + " formed a new party with " + name);
            }
        }

        // reset party invite in any case
        partyInviteFrom = "";
    }

    [Command]
    public void CmdPartyInviteDecline()
    {
        partyInviteFrom = "";
    }

    [Command]
    public void CmdPartyKick(int memberIndex)
    {
        // validate: party master and index exists and not master?
        if (InParty() && party.members[0] == name &&
            0 < memberIndex && memberIndex < party.members.Length)
        {
            string member = party.members[memberIndex];

            // kick
            party.RemoveMember(member);

            // still enough people in it for a party?
            if (party.members.Length > 1)
            {
                BroadcastPartyChanges(party);
            }
            else
            {
                // a party requires at least two people, otherwise it's not
                // really a party anymore. if we'd keep it alive with one player
                // then he can't be invited to another party until he dismisses
                // the empty one.
                ClearParty();
            }

            // clear for the kicked person too
            if (onlinePlayers.ContainsKey(member))
                onlinePlayers[member].ClearParty();

            print(name + " kicked " + member + " from party");
        }
    }

    // version without cmd because we need to call it from the server too
    public void PartyLeave()
    {
        // validate: in party and not master?
        if (InParty() && party.members[0] != name)
        {
            // remove self from party
            party.RemoveMember(name);

            // still enough people in it for a party?
            if (party.members.Length > 1)
            {
                BroadcastPartyChanges(party);
            }
            else
            {
                // a party requires at least two people, otherwise it's not
                // really a party anymore. if we'd keep it alive with one player
                // then he can't be invited to another party until he dismisses
                // the empty one.
                if (onlinePlayers.ContainsKey(party.members[0]))
                    onlinePlayers[party.members[0]].ClearParty();
            }

            // clear for self
            ClearParty();
            print(name + " left the party");
        }
    }
    [Command]
    public void CmdPartyLeave() { PartyLeave(); }

    // version without cmd because we need to call it from the server too
    public void PartyDismiss()
    {
        // validate: is master?
        if (InParty() && party.members[0] == name)
        {
            // clear party for everyone
            foreach (string member in party.members)
            {
                if (onlinePlayers.ContainsKey(member))
                    onlinePlayers[member].ClearParty();
            }
            print(name + " dismissed the party");
        }
    }
    [Command]
    public void CmdPartyDismiss() { PartyDismiss(); }

    [Command]
    public void CmdPartySetMoneyShare(bool value)
    {
        // validate: is party master?
        if (InParty() && party.members[0] == name)
        {
            // set new value, sync to everyone else
            party.shareMoney = value;
            BroadcastPartyChanges(party);
        }
    }

    // pet /////////////////////////////////////////////////////////////////////
    [Command]
    public void CmdPetSetAutoAttack(bool value)
    {
        // validate
        if (activePet != null)
            activePet.autoAttack = value;
    }

    [Command]
    public void CmdPetSetDefendOwner(bool value)
    {
        // validate
        if (activePet != null)
            activePet.defendOwner = value;
    }

    // helper function for command and UI
    public bool CanUnsummonPet()
    {
        // only while pet and owner aren't fighting
        return activePet != null &&
               (state == GlobalVar.stateIdle || state == GlobalVar.stateMoving) &&
               (activePet.state == GlobalVar.stateIdle || activePet.state == GlobalVar.stateMoving);
    }

    [Command]
    public void CmdPetUnsummon()
    {
        // validate
        if (CanUnsummonPet())
        {
            // destroy from world. item.summoned and activePet will be null.
            NetworkServer.Destroy(activePet.gameObject);
        }
    }

    [Command]
    public void CmdNpcReviveSummonable(int index)
    {
        // validate: close enough, npc alive and valid index and valid item?
        // use collider point(s) to also work with big entities
        if (state == GlobalVar.stateIdle &&
            target != null &&
            target.health > 0 &&
            target is Npc &&
            ((Npc)target).offersSummonableRevive &&
            Utils.ClosestDistance(collider, target.collider) <= interactionRange &&
            0 <= index && index < inventory.Count)
        {
            ItemSlot slot = inventory[index];
            if (slot.amount > 0 && slot.item.data is SummonableItem)
            {
                // verify the pet status
                SummonableItem itemData = (SummonableItem)slot.item.data;
                if (slot.item.data1 == 0 && itemData.summonPrefab != null)
                {
                    // enough money?
                    if (Money.AvailableMoney(this) >= itemData.revivePrice)
                    {
                        // pay for it, revive it
                        Money.TakeFromInventory(this, itemData.revivePrice);
                        slot.item.data1 = itemData.summonPrefab.healthMax;
                        inventory[index] = slot;
                    }
                }
            }
        }
    }

    // mounts //////////////////////////////////////////////////////////////////
    public bool IsMounted()
    {
        return activeMount != null && activeMount.health > 0;
    }

    void ApplyMountSeatOffset()
    {
        if (meshToOffsetWhenMounted != null)
        {
            // apply seat offset if on mount (not a dead one), reset otherwise
            if (activeMount != null && activeMount.health > 0)
                meshToOffsetWhenMounted.transform.position = activeMount.seat.position + Vector3.up * seatOffsetY;
            else
                meshToOffsetWhenMounted.transform.localPosition = Vector3.zero;
        }
    }

    // marker setting ///////////////////////////////////////////////////////////
    public void SetPickRangeIndicator(Vector3 position)
    {
        GameObject go = Instantiate(rangeDisplayPrefab);
        go.transform.position = position;
        go.GetComponent<ShortTermProjector>().size = GlobalVar.pickRange;
    }

    public void SetIndicatorViaParent(Transform parent)
    {
        if (!indicator) indicator = Instantiate(indicatorPrefab);
        indicator.transform.SetParent(parent, true);
        indicator.transform.position = parent.position;
    }

    public void SetIndicatorViaPosition(Vector3 position)
    {
        if (!indicator) indicator = Instantiate(indicatorPrefab);
        indicator.transform.parent = null;
        indicator.transform.position = position;
    }

    // selection handling //////////////////////////////////////////////////////
    [Command]
    public void CmdSetTarget(NetworkIdentity ni)
    {
        // validate
        if (ni != null)
        {
            // can directly change it, or change it after casting?
            if (state == GlobalVar.stateIdle || state == GlobalVar.stateMoving || state == GlobalVar.stateStunned)
            {
                target = ni.GetComponent<Entity>();
                selectedElement = null;
            }
            else if (state == GlobalVar.stateCasting)
                nextTarget = ni.GetComponent<Entity>();
        }
    }
    [Command]
    public void CmdClearTarget()
    {
        target = null;
    }

    [Command]
    public void CmdSetSelectedElement(NetworkIdentity ni)
    {
        // validate
        if (ni != null)
        {
            // can directly change it
            if (state == GlobalVar.stateIdle || state == GlobalVar.stateMoving || state == GlobalVar.stateStunned)
            {
                target = null;
                selectedElement = ni.GetComponent<ElementSlot>();
            }
        }
    }
    [Command]
    public void CmdClearSelectedElement()
    {
        selectedElement = null;
    }

    private float clickTimeStart = 0f;
    private bool clickMouseIsDown = false;

    [Client]
    void SelectionHandling()
    {
        // click raycasting if not over a UI element & not pinching on mobile
        // note: this only works if the UI's CanvasGroup blocks Raycasts
        if (Input.GetMouseButtonUp(0))
        {
            clickMouseIsDown = false;
            // short left mouse click
            if (!Utils.IsCursorOverUserInterface() && Input.touchCount <= 1 && ((Time.time - clickTimeStart) < PlayerPreferences.keyPressedLongClick))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // raycast with local player ignore option
                RaycastHit hit;
                bool cast = localPlayerClickThrough ? Utils.RaycastWithout(ray, out hit, gameObject) : Physics.Raycast(ray, out hit);
                if (cast)
                {
                    // clear requested spell in any case because if we clicked
                    // somewhere else then we don't care about it anymore
                    useSpellWhenCloser = -1;
                    useElementWhenCloser = null;
                    pickElementWhenCloser = null;

                    // valid target?
                    Entity entity = hit.transform.GetComponent<Entity>();
                    ElementSlot usableElement = hit.transform.GetComponent<ElementSlot>();
                    if (entity)
                    {
                        // set indicator
                        SetIndicatorViaParent(hit.transform);

                        // clicked selected target again? and is not self or pet?
                        if (entity == target && entity != this && entity != activePet)
                        {
                            // attackable? => attack
                            if (CanAttack(entity))
                            {
                                int standardSpellId = spells.IdOfStandardFighting();
                                if (standardSpellId >= 0)
                                {
                                    if (Vector3.Distance(transform.position, entity.transform.position) > spells[standardSpellId].CastRange(this))
                                    {
                                        // otherwise just walk there
                                        // use collider point(s) to also work with big entities  
                                        agent.stoppingDistance = 0;
                                        agent.destination = entity.collider.ClosestPoint(transform.position);
                                        useSpellWhenCloser = standardSpellId;
                                    }
                                    else
                                    {
                                        // then try to use that one
                                        TryUseSpell(standardSpellId);

                                    }
                                }
                            }
                            // npc & alive => talk
                            else if (entity is Npc && entity.health > 0)
                            {
                                // close enough to talk?
                                // use collider point(s) to also work with big entities
                                if (Utils.ClosestDistance(collider, entity.collider) <= interactionRange)
                                {
                                    UINpcDialogue.singleton.Show();
                                }
                                // otherwise walk there
                                // use collider point(s) to also work with big entities
                                else
                                {
                                    agent.stoppingDistance = interactionRange;
                                    agent.destination = entity.collider.ClosestPoint(transform.position);
                                }
                            }
                            // monster & dead => loot
                            else if (entity is Monster && entity.health == 0)
                            {
                                // has loot? and close enough?
                                // use collider point(s) to also work with big entities
                                if (((Monster)entity).HasLoot() &&
                                    Utils.ClosestDistance(collider, entity.collider) <= interactionRange)
                                {
                                    UILoot.singleton.Show();
                                }
                                // otherwise walk there
                                // use collider point(s) to also work with big entities
                                else
                                {
                                    agent.stoppingDistance = interactionRange;
                                    agent.destination = entity.collider.ClosestPoint(transform.position);
                                }
                            }
                        }
                        // clicked a new target
                        else
                        {
                            // target it if close enough
                            float distance = Vector3.Distance(transform.position, entity.transform.position);
                            if (distance <= distanceDetectionPerson)
                            {
                                CmdSetTarget(entity.netIdentity);
                            }
                        }
                    }
                    // a useable element
                    else if (usableElement)
                    {
                        // set indicator
                        SetIndicatorViaParent(hit.transform);

                        // clicked last target again?
                        if (usableElement == selectedElement)
                        {
                            // click on selected: use it, pick or go to!
                            if (Vector3.Distance(transform.position, usableElement.transform.position) < usableElement.interactionRange)
                            {
                                if (usableElement.CanUse(this))
                                    UseSelectedElement(usableElement);
                                else if (usableElement.pickable)
                                    CmdPickSelectedElement();
                            }
                            else
                            {
                                Vector3 targetPos = usableElement.transform.position;
                                float stoppingDistance = usableElement.interactionRange * GlobalVar.walkInInteractionRange;

                                SetIndicatorViaPosition(targetPos);

                                agent.stoppingDistance = stoppingDistance;
                                agent.destination = targetPos;
                                //initialize useage for possible items only
                                if (usableElement.CanUse(this) || usableElement.pickable)
                                    useElementWhenCloser = usableElement;
                            }
                        }
                        else
                        {
                            float distance = Vector3.Distance(transform.position, usableElement.transform.position);
                            // target it if close enough
                            if (distance <= distanceDetectionPerson)
                            {
                                CmdSetSelectedElement(usableElement.netIdentity);
                            }
                        }
                    }
                    // otherwise it's a movement target
                    else
                    {
                        // set indicator and navigate to the nearest walkable
                        // destination. this prevents twitching when destination is
                        // accidentally in a room without a door etc.
                        Vector3 bestDestination = agent.NearestValidDestination(hit.point);
                        SetIndicatorViaPosition(bestDestination);

                        // casting? then set pending destination
                        if (state == GlobalVar.stateCasting)
                        {
                            pendingDestination = bestDestination;
                            pendingDestinationValid = true;
                        }
                        else
                        {
                            if (state == GlobalVar.stateWorking)
                            {
                                // end work on move
                                CmdStopWorking();
                            }
                            agent.stoppingDistance = 0;
                            agent.destination = bestDestination;
                        }
                    }
                }
            }
        }
        else if (!Utils.IsCursorOverUserInterface() && Input.touchCount <= 1)
        {
            // left mouse down 
            if (Input.GetMouseButtonDown(0))
            {
                clickTimeStart = Time.time;
                clickMouseIsDown = true;
            }
            // right mouse down
            else if (Input.GetMouseButtonDown(1))
            {

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // raycast with local player ignore option
                RaycastHit hit;
                bool cast = localPlayerClickThrough ? Utils.RaycastWithout(ray, out hit, gameObject) : Physics.Raycast(ray, out hit);
                if (cast)
                {
                    useSpellWhenCloser = -1;
                    useElementWhenCloser = null;
                    pickElementWhenCloser = null;

                    // valid target?
                    Entity entity = hit.transform.GetComponent<Entity>();
                    ElementSlot usableElement = hit.transform.GetComponent<ElementSlot>();
                    UIContextMenu contextMenu = GameObject.Find("Canvas/ContextMenu").GetComponent<UIContextMenu>();
                    if (entity)
                    {
                        contextMenu.player = this;
                        contextMenu.entity = entity;
                    }
                    else if (usableElement)
                    {
                        contextMenu.player = this;
                        contextMenu.element = usableElement;
                    }
                    else
                    {
                        Vector3 bestDestination = agent.NearestValidDestination(hit.point);
                        SetIndicatorViaPosition(bestDestination);
                        contextMenu.player = this;
                        contextMenu.targetPosition = bestDestination;
                    }
                }
            }
            // long mouse click
            else if (clickMouseIsDown && (Time.time - clickTimeStart) > PlayerPreferences.keyPressedLongClick)
            {
                if (state == GlobalVar.stateWorking)
                {
                    // end work on move
                    CmdStopWorking();
                }
                // hold mouse move into that direction
                float relativeMousePosX = (2f * Input.mousePosition.x / Screen.width) - 1;
                transform.Rotate(0, relativeMousePosX * PlayerPreferences.turningSpeed, 0);

                // get player direction
                Vector3 angles = transform.rotation.eulerAngles;

                if (Input.GetKey(PlayerPreferences.keyDoNotWalk))
                {
                    // tell everybody
                    CmdUpdateRotation(angles.y);
                }
                else
                {
                    // convert into new direction
                    angles.x = 0;
                    Quaternion rotation = Quaternion.Euler(angles); // back to quaternion
                    Vector3 direction = rotation * new Vector3(0, 0, 1);

                    agent.ResetMovement();
                    agent.velocity = speed * direction;
                    if (indicator != null)
                        Destroy(indicator);
                }
            }
        }
    }

    [Client]
    void WASDHandling()
    {
        float currentSpeed = 0;
        // don't move if currently typing in an input
        // we check this after checking h and v to save computations

        if (!UIUtils.AnyInputActive())
        {
            // get horizontal and vertical input
            // note: no != 0 check because it's 0 when we stop moving rapidly
            float playerrotation = Input.GetAxis("Horizontal"); //left/right
            float playermovement = Input.GetAxis("Vertical"); //forward/backward

            if (playerrotation != 0 || playermovement != 0)
            {
                currentSpeed = speed;

                if (playerrotation != 0)
                {
                    float currentRotation = playerrotation * PlayerPreferences.turningSpeed;
                    if (Input.GetKey(PlayerPreferences.keyCreep1) || Input.GetKey(PlayerPreferences.keyCreep2))
                        currentRotation = currentRotation * PlayerPreferences.creepSpeed;
                    transform.Rotate(0, currentRotation, 0);
                }
                if (playermovement < 0)
                {
                    currentSpeed = speedWalkPlayer * GlobalVar.walkBackRelativeSpeed;
                    hasMovedBackward = true;
                }
                else
                {
                    hasMovedBackward = false;
                }

                // create input vector, normalize
                Vector3 input = new Vector3(0, 0, playermovement);
                if (input.magnitude > 1) input = input.normalized;

                // get player direction
                Vector3 angles = transform.rotation.eulerAngles;
                angles.x = 0;
                Quaternion rotation = Quaternion.Euler(angles); // back to quaternion

                if (playermovement == 0)
                {
                    //just turn and inform other clients
                    CmdUpdateRotation(angles.y);
                }

                // calculate input direction relative to player rotation
                Vector3 direction = rotation * input;

                // clear indicator if there is one, and if it's not on a target
                // (simply looks better)
                if (direction != Vector3.zero && indicator != null && indicator.transform.parent == null)
                    Destroy(indicator);

                // cancel path if we are already doing click movement, otherwise
                // we will slide
                agent.ResetMovement();

                // casting? then set pending velocity
                if (state == GlobalVar.stateCasting)
                {
                    pendingVelocity = direction * currentSpeed;
                    pendingVelocityValid = true;
                }
                else
                {
                    // set velocity
                    agent.velocity = direction * currentSpeed;

                    // moving with velocity doesn't look at the direction, do it manually
                    // not good for walking backward
                    // LookAtY(transform.position + direction);
                }

                // clear requested spell and current work in any case because if we clicked
                // somewhere else then we don't care about it anymore
                useSpellWhenCloser = -1;
                useElementWhenCloser = null;
                pickElementWhenCloser = null;
                if (state == GlobalVar.stateWorking)
                {
                    // end work on move
                    CmdStopWorking();
                }
            }
            else if (Input.GetKey(PlayerPreferences.keyDoNotWalk) && !clickMouseIsDown)
            {
                agent.ResetMovement();
                if (indicator != null)
                    Destroy(indicator);
            }

        }
    }

    [Client]
    void StaminaHandling()
    {
        if (isSwimming)
        {
            staminaConsumption += GlobalVar.staminaConsumptionSwimming * Time.deltaTime;
        }
        else if (agent.velocity.sqrMagnitude > staminaLimit)
        {
            staminaConsumption += GlobalVar.staminaConsumption * Time.deltaTime;
        }
    }

    // simple tab targeting
    [Client]
    void TargetNearest()
    {
        if (Input.GetKeyDown(PlayerPreferences.keyClosestMonster))
        {
            // find all monsters that are alive, sort by distance
            GameObject[] objects = GameObject.FindGameObjectsWithTag("Monster");
            List<Monster> monsters = objects.Select(go => go.GetComponent<Monster>()).Where(m => m.health > 0).ToList();
            List<Monster> sorted = monsters.OrderBy(m => Vector3.Distance(transform.position, m.transform.position)).ToList();

            // target nearest one
            if (sorted.Count > 0)
            {
                SetIndicatorViaParent(sorted[0].transform);
                CmdSetTarget(sorted[0].netIdentity);
            }
        }
    }

    // ontrigger ///////////////////////////////////////////////////////////////
    protected override void OnTriggerEnter(Collider col)
    {
        // call base function too
        base.OnTriggerEnter(col);

        // quest location?
        if (isServer && col.tag == "QuestLocation")
            QuestsOnLocation(col);
    }

    // drag and drop ///////////////////////////////////////////////////////////
    void OnDragAndDrop_InventorySlot_InventorySlot(int[,] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        // only if any in from
        if (inventory.GetItemSlot(slotIndices[0, 0], slotIndices[0, 1], out ItemSlot itemSlotFrom))
        {
            // any in target?
            if (inventory.GetItemSlot(slotIndices[1, 0], slotIndices[1, 1], out ItemSlot itemSlotTo))
            {
                // merge? check Equals because name AND dynamic variables matter (petLevel etc.)
                if (itemSlotFrom.item.Equals(itemSlotTo.item))
                {
                    CmdInventoryMerge(slotIndices[0, 0], slotIndices[0, 1], slotIndices[1, 0], slotIndices[1, 1]);
                    return;
                }
            }
            // split?
            else if (Utils.AnyKeyPressed(inventorySplitKeys))
            {
                CmdInventorySplit(slotIndices[0, 0], slotIndices[0, 1], slotIndices[1, 0], slotIndices[1, 1], splitValue);
                return;
            }

            CmdSwapInventoryInventory(slotIndices[0, 0], slotIndices[0, 1], slotIndices[1, 0], slotIndices[1, 1]);
        }
    }

    void OnDragAndDrop_InventorySlot_EquipmentSlot(int[,] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        if (GlobalFunc.IsInBelt(slotIndices[1, 1]))
        {
            OnDragAndDrop_InventorySlot_InventorySlot(slotIndices);
            return;
        }
        // split?
        else if (Utils.AnyKeyPressed(inventorySplitKeys))
        {
            CmdSplitInventoryEquip(slotIndices[0, 0], slotIndices[0, 1], slotIndices[1, 1]);
            return;
        }
        CmdSwapInventoryEquip(slotIndices[0, 0], slotIndices[0, 1], slotIndices[1, 1]);
    }

    void OnDragAndDrop_InventorySlot_TradingSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        //if (inventory[slotIndices[0]].item.tradable) >>> neu machen
        CmdTradeOfferItem(slotIndices[0], slotIndices[1]);
    }

    void OnDragAndDrop_InventorySlot_CraftingIngredientSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        // only if not crafting right now
        if (craftingState != CraftingState.InProgress)
        {
            if (!craftingIndices.Contains(slotIndices[0]))
            {
                craftingIndices[slotIndices[1]] = slotIndices[0];
                craftingState = CraftingState.None; // reset state
            }
        }
    }

    void OnDragAndDrop_EquipmentSlot_EquipmentSlot(int[,] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        if (GlobalFunc.IsInBelt(slotIndices[0, 1]) && GlobalFunc.IsInBelt(slotIndices[1, 1]))
        {
            OnDragAndDrop_InventorySlot_InventorySlot(slotIndices);
            return;
        }
        // split?
        else if (Utils.AnyKeyPressed(inventorySplitKeys))
        {
            CmdSplitInventoryEquip(slotIndices[0, 0], slotIndices[0, 1], slotIndices[1, 1]);
            return;
        }
        CmdSwapInventoryEquip(slotIndices[0, 0], slotIndices[0, 1], slotIndices[1, 1]);
    }

    void OnDragAndDrop_EquipmentSlot_InventorySlot(int[,] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        if (GlobalFunc.IsInBelt(slotIndices[0, 1]))
        {
            OnDragAndDrop_InventorySlot_InventorySlot(slotIndices);
            return;
        }
        CmdSwapInventoryEquip(slotIndices[1, 0], slotIndices[1, 1], slotIndices[0, 1]); // reversed
    }

    void OnDragAndDrop_SpellSlot_SpellbarSlot(int[,] slotIndices)
    {
        // slotIndices[0] = player.spells index; slotIndices[1] = slotTo
        spellbar[slotIndices[1, 1]].reference = spells[slotIndices[0, 1]].name; // just save it clientsided
    }

    void OnDragAndDrop_SpellbarSlot_SpellbarSlot(int[,] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        // just swap them clientsided
        string temp = spellbar[slotIndices[0, 1]].reference;
        spellbar[slotIndices[0, 1]].reference = spellbar[slotIndices[1, 1]].reference;
        spellbar[slotIndices[1, 1]].reference = temp;
    }

    void OnDragAndDrop_CraftingIngredientSlot_CraftingIngredientSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        // only if not crafting right now
        if (craftingState != CraftingState.InProgress)
        {
            // just swap them clientsided
            int temp = craftingIndices[slotIndices[0]];
            craftingIndices[slotIndices[0]] = craftingIndices[slotIndices[1]];
            craftingIndices[slotIndices[1]] = temp;
            craftingState = CraftingState.None; // reset state
        }
    }

    void OnDragAndDrop_InventorySlot_NpcReviveSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        if (inventory[slotIndices[0]].item.data is SummonableItem)
            UINpcRevive.singleton.itemIndex = slotIndices[0];
    }

    void OnDragAndDrop_PutAway(int[,] slotIndices)
    {
        if (inventory.GetItemSlot(slotIndices[0, 0], slotIndices[0, 1], out ItemSlot itemSlot))
        {
            if (itemSlot.item.data is UsableItem)
            {
                UsableItem ui = (UsableItem)itemSlot.item.data;
                //has the slot a prefab that could be put away
                if (ui.modelPrefab)
                {
                    // find position and define action
                    float targetX = slotIndices[1, 0];
                    float targetY = slotIndices[1, 1];
                    Ray ray = Camera.main.ScreenPointToRay(new Vector3(targetX, targetY));

                    // raycast with local player ignore option
                    RaycastHit hit;
                    bool cast = localPlayerClickThrough ? Utils.RaycastWithout(ray, out hit, gameObject) : Physics.Raycast(ray, out hit);
                    if (cast)
                    {
                        // clear requested spell in any case because if we clicked
                        // somewhere else then we don't care about it anymore
                        useSpellWhenCloser = -1;
                        useElementWhenCloser = null;
                        pickElementWhenCloser = null;

                        float td = NonLinearCurves.FloatFromCurvePosition(GlobalVar.throwingDistanceWeightInfluenceNonlinear, ui.weight, 0, GlobalVar.throwingDistanceWeightMax, 0, throwingDistanceMaxPlayer);
                        if (Vector3.Distance(transform.position, hit.point) < td)
                        {
                            // valid target?
                            Entity entity = hit.transform.GetComponent<Entity>();
                            ElementSlot elementSlot = hit.transform.GetComponent<ElementSlot>();
                            if (entity)
                            {
                                // >>> tbd hit an entity
                            }
                            else if (elementSlot)
                            {
                                // >>> tbd hit an element (torch to holder ...)
                            }
                            else
                            {
                                // anywhere in the wild
                                int amount = int.MaxValue;
                                if (Utils.AnyKeyPressed(inventorySplitKeys))
                                    amount = splitValue;
                                CmdPutItemAway(slotIndices[0, 0], slotIndices[0, 1], hit.point.x, hit.point.y, hit.point.z, amount);
                            }
                        }
                        else
                            Inform("So far you can not throw that with the best will.");
                    }
                }
            }
        }
    }

    void OnDragAndClear_SpellbarSlot(int slotIndex)
    {
        spellbar[slotIndex].reference = "";
    }

    void OnDragAndClear_TradingSlot(int slotIndex)
    {
        CmdTradeOfferItemClear(slotIndex);
    }

    void OnDragAndClear_CraftingIngredientSlot(int slotIndex)
    {
        // only if not crafting right now
        if (craftingState != CraftingState.InProgress)
        {
            craftingIndices[slotIndex] = -1;
            craftingState = CraftingState.None; // reset state
        }
    }

    void OnDragAndClear_NpcReviveSlot(int slotIndex)
    {
        UINpcRevive.singleton.itemIndex = -1;
    }

    // validation //////////////////////////////////////////////////////////////
    void OnValidate()
    {
        // make sure that the NetworkNavMeshAgentRubberbanding2D component is
        // ABOVE the player component, so that it gets updated before Player.cs.
        // -> otherwise it overwrites player's WASD velocity for local player
        //    hosts
        // -> there might be away around it, but a warning is good for now
        Component[] components = GetComponents<Component>();
        if (Array.IndexOf(components, GetComponent<NetworkNavMeshAgentRubberbanding>()) >
            Array.IndexOf(components, this))
            Debug.LogWarning(name + "'s NetworkNavMeshAgentRubberbanding component is below the Player component. Please drag it above the Player component in the Inspector, otherwise there might be WASD movement issues due to the Update order.");
    }

    // variables to make sure the character turns back in the right direction after walking backward
    bool hasMovedBackward = false;
    Quaternion rotationLastSecond;
    Vector3 positionLastSecond;

    // 1 / second
    [Client]
    public void OneSecondCycleClient()
    {
        // stamina control
        if (gmUnlimitedStamina)
            stamina = staminaMaxPlayer;
        else
            stamina += staminaRecoveryPlayer - (int)staminaConsumption;

        staminaConsumption = 0;
        // penalty when falling below 0
        if (stamina < 0 && staminaLast >= 0)
            stamina = GlobalVar.staminaNegaive;
        staminaLast = stamina;
        speedWeightInfluence = NonLinearCurves.FloatFromCurvePosition(GlobalVar.liftingCapacitySpeedInfluenceNonlinear, WeightPercent(), 0f, 1f, 0f, 1f);

        if (isLocalPlayer && hasMovedBackward)
        {
            // direction correction
            // turning the char is partially not synchronized, we synchronize after char moved backward and idle
            // this seems to be a unity bug to work around
            if (rotationLastSecond == transform.rotation && positionLastSecond == transform.position)
            {
                // correct rotation
                CmdUpdateRotation(transform.rotation.eulerAngles.y);
                hasMovedBackward = false;
            }
            else
            {
                rotationLastSecond = transform.rotation;
                positionLastSecond = transform.position;
            }
        }

        // GM is invisible
        if (!isLocalPlayer)
        {
            if (isGmInvisible)
            {
                showOverlay = false;
                nameOverlay.gameObject.SetActive(false);
                healthOverlay.gameObject.SetActive(false);
                minimapMarker.SetActive(false);
                body.SetActive(false);
            }
            else
            {
                showOverlay = true;
                nameOverlay.gameObject.SetActive(true);
                healthOverlay.gameObject.SetActive(true);
                minimapMarker.SetActive(true);
                body.SetActive(true);
            }
        }
        else
        {
            if (isGmInvisible)
            {
                minimapMarker.SetActive(false);
            }
            else
            {
                minimapMarker.SetActive(true);
            }
        }
    }

    //Calculte advanced bonuses with high calculation effort
    [Server]
    public void OneSecondCycleServer()
    {
        // calculate wand bonus
        //wand in right hand
        if (CalculateWandBonus(GlobalVar.equipmentRightHand))
        {
            return;
        }
        else
        {
            // optional left hand
            if (CalculateWandBonus(GlobalVar.equipmentLeftHand))
            {
                return;
            }
            // no wand
            else
            {
                manaWandBonus = 1f;
                manaRecoveryWandBonus = 1f;
            }
        }
    }
    //returns true if wand found
    bool wandBonusMessageRequest = false;
    bool CalculateWandBonus(int hand)
    {
        if (inventory.GetItemSlot(GlobalVar.containerEquipment, hand, out ItemSlot itemSlot))
        {
            if (itemSlot.item.data is WandItem)
            {
                WandItem wandItem = (WandItem)itemSlot.item.data;
                if (itemSlot.item.data1 + wandItem.secondsToTakeEffect < GameTime.SecondsSinceZero())
                {
                    if (wandBonusMessageRequest == true)
                    {
                        wandBonusMessageRequest = false;
                        Inform("You notice how your wand begins to have an effect.");
                        TargetFlashEffect(this.connectionToClient, GlobalVar.flashEffectWandActive);
                    }
                    int relativeSkillLevel = skills.LevelOfSkill(wandItem.magicSchool) - wandItem.skillLevel + wandItem.nolinearOffset;
                    manaWandBonus = NonLinearCurves.GetFloat0_1(wandItem.nolinearDependency, relativeSkillLevel) * wandItem.maxManaIncrease;
                    manaRecoveryWandBonus = NonLinearCurves.GetFloat0_1(wandItem.nolinearDependency, relativeSkillLevel) * wandItem.maxManaRegenerationIncrease;
                }
                else
                {
                    wandBonusMessageRequest = true;
                    manaWandBonus = 1f;
                    manaRecoveryWandBonus = 1f;
                }
                return true;
            }
        }
        // No wand
        return false;
    }


    [Command]
    void CmdUpdateRotation(float y)
    {
        transform.rotation = Quaternion.Euler(0, y, 0);
        RpcUpdateRotation(y);
    }

    [ClientRpc]
    public void RpcUpdateRotation(float y)
    {
        transform.rotation = Quaternion.Euler(0, y, 0);
    }

    [ClientRpc]
    public void RpcUpdatePosition(float x, float y, float z, float direction)
    {
        agent.ResetMovement();
        agent.Warp(new Vector3(x, y, z));
        transform.rotation = Quaternion.Euler(0, direction, 0);
    }

    [Server]
    public override void Recover()
    {
        //>>> tmp limited recovery for tests
        if (health > 0 && enabled)
        {
            health += healthRecoveryRate / 10;
            mana += manaRecoveryRate / 10;
        }

        //if (gmUnlimitedHealth && health > 0)
        //    health = healthMax;
        //else if (enabled && health > 0 && healthRecovery)
        //    health += healthRecoveryRate;
        //if (gmUnlimitedMana && health > 0)
        //    mana = manaMax;
        //else if (enabled && health > 0 && manaRecovery)
        //    mana += manaRecoveryRate;

    }

    // 1 / 5 minutes
    [Client]
    public void FiveMinuteCycle()
    {
        if (isGM)
        {
            // verify GM abuse
            if (gmNpcKillCount >= GlobalVar.gmNpcKillAlarmLimit)
            {
                GmLogAction(0, string.Format("GM killed {0} NPC. (Empty bucket counter)", gmNpcKillCount));
                gmNpcKillCount = 0;
            }
            else
                gmNpcKillCount = Mathf.Max(0, gmNpcKillCount - GlobalVar.gmNpcKillReduction);
            if (gmNpcPullCount >= GlobalVar.gmNpcPullAlarmLimit)
            {
                GmLogAction(0, string.Format("GM pulled {0} NPC. (Empty bucket counter)", gmNpcPullCount));
                gmNpcPullCount = 0;
            }
            else
                gmNpcPullCount = Mathf.Max(0, gmNpcPullCount - GlobalVar.gmNpcPullReduction);
        }
    }

    // divergence //////////////////////////////////////////////////////////////
    public void RefreshDivergence()
    {
        if (!isGM)
        {
            divergenceClock = CalculateDivergence(GlobalVar.divergenceBaseClock);
            divergenceCompass = CalculateDivergence(GlobalVar.divergenceBaseCompass);
            divergenceExamination = CalculateDivergence(GlobalVar.divergenceExamination);
            divergenceWeight = 1 + (GlobalVar.weightBaseSpread / (handscale + 1) * CalculateDivergence(GlobalVar.divergenceWeight));
            divergencePrice = CalculatePriceDivergence(GlobalVar.priceStabilityTime, abilities.bestprice);
        }
        else
        {
            divergenceCompass = 0.0f;
            divergenceClock = 0.0f;
            divergenceExamination = 0.0f;
            divergenceWeight = 1.0f;
            divergencePrice = 0.0f;
        }
    }

    // obfuscation and divergences
    private float CalculateDivergence(int seedCycle)
    {
        // divergence is always in between -1 - 1
        GameTime gt = new GameTime();
        int secSinceStart = gt.SecondsSinceStart();
        int seed = secSinceStart / seedCycle;
        System.Random rand = new System.Random(seed);
        return (float)rand.NextDouble() * 2 - 1;
    }
    private float CalculatePriceDivergence(int seedCycle, int abilityLevel)
    {
        GameTime gt = new GameTime();
        int secSinceStart = gt.SecondsSinceStart();
        int seed = secSinceStart / seedCycle;
        System.Random rand = new System.Random(seed);
        double stable = rand.NextDouble();
        double deviation = rand.NextDouble();
        if (stable < GlobalVar.priceBestProbability[abilityLevel])
            return 0f;
        else
            return (float)deviation * GlobalVar.priceVariance;
    }
    // playtime //////////////////////////////////////////////////////////////
    public void RefreshPlaytime()
    {
        if (isLocalPlayer)
        {
            if (lastPosition != transform.position || lastRotation != transform.rotation)
            {
                ActivityPerformed(GlobalVar.playtimeMovmentReward);
                lastPosition = transform.position;
                lastRotation = transform.rotation;
            }
            else
            {
                activityLevel += GlobalVar.playtimeAccuracy;
            }
            if (activityLevel < GlobalVar.playtimeInactivityLimit)
            {
                playtime += GlobalVar.playtimeAccuracy;
                // add skill time
                skillTotalTime += GlobalVar.playtimeAccuracy;
            }
        }
    }

    public int AttributeTotalMax()
    {
        return GlobalVar.attributeTotal - (int)(GlobalFunc.ProportionFromValue(playtime, GlobalVar.attributeStartPenaltyDuration * 3600, 0) * GlobalVar.attributeStartPenalty);
    }

    public void ActivityPerformed()
    {
        activityLevel = 0;
    }
    public void ActivityPerformed(int reward)
    {
        activityLevel = Mathf.Min(activityLevel, Mathf.Max(0, GlobalVar.playtimeInactivityLimit - reward));
    }

    public void Inform(string message)
    {
        if (isServer)
        {
            chat.TargetMsgInfo(connectionToClient, message);
        }
        else
        {
            chat.CallInformMessage(message);
        }
    }

    // no repeat message
    // we expect spam from the same source, either client or server, so blocking is seperated to save network traffic
    struct NoRepeat
    {
        public string msg;
        public float endTime;
    }
    List<NoRepeat> messageNotToRepeat = new List<NoRepeat>();

    public void InformNoRepeat(string message, float timeLimit)
    {
        // this message is not already in the buffer
        if (messageNotToRepeat.FindIndex(x => x.msg == message) < 0)
        {
            // perform the message
            if (isServer)
            {
                chat.TargetMsgInfo(connectionToClient, message);
            }
            else
            {
                chat.CallInformMessage(message);
            }
            // enter to list
            messageNotToRepeat.Add(new NoRepeat { msg = message, endTime = Time.time + timeLimit });
            // invoke Cleanup
            Invoke("ClearMessageNotToRepeat", timeLimit + 0.1f);
        }
    }
    public void ClearMessageNotToRepeat()
    {
        if (messageNotToRepeat.Count > 0)
        {
            for (int i = messageNotToRepeat.Count - 1; i >= 0; i--)
            {
                if (messageNotToRepeat[i].endTime < Time.time)
                {
                    messageNotToRepeat.RemoveAt(i);
                }
            }
        }
    }

    // container helper
    // have to be in networkbehaviour
    public int ContainerIdOfBackpack()
    {
        if (inventory.GetEquipment(GlobalVar.equipmentBackpack, out ItemSlot itemSlot))
        {
            return itemSlot.item.data1;
        }
        return -1;
    }

    public int AddNewMobileContainer(int noOfSlots, int noOfContainers, string name, string miscellaneous, bool isServerCall = false)
    {
        //find free id
        int id = GlobalVar.containerFirstMobile;
        while (containers.IndexOfId(id) != -1)
        {
            id++;
        }
        AddNewContainer(id, GlobalVar.containerTypeMobile, noOfSlots, noOfContainers, name, miscellaneous, isServerCall);
        return id;
    }

    public void AddNewContainer(int id, int type, int noOfSlots, int noOfContainers, string name, string miscellaneous, bool isServerCall = false)
    {
        if (isServerCall)
        {
            containers.Add(new Container(id, type, noOfSlots, noOfContainers, name, miscellaneous));
        }
        else
        {
            CmdAddContainer(id, type, noOfSlots, noOfContainers, name, miscellaneous);
        }
    }

    [Command]
    public void CmdAddContainer(int id, int type, int noOfSlots, int noOfContainers, string name, string miscellaneous)
    {
        containers.Add(new Container(id, type, noOfSlots, noOfContainers, name, miscellaneous));
    }

    public void RemoveContainer(int id)
    {
        CmdRemoveContainer(id);
    }

    [Command]
    public void CmdRemoveContainer(int id)
    {
        containers.RemoveAt(containers.IndexOfId(id));
    }

    // GM Helper
    // GM log file always on server!
    public void GmLogAction(int involvedPlayer, string logText)
    {
        CmdGmLogAction(involvedPlayer, logText);
    }

    [Command]
    public void CmdGmLogAction(int involvedPlayer, string logText)
    {
        LogFile.WriteGmLog(this.id, involvedPlayer, logText);
    }

    // switch visibility
    [Command]
    public void CmdGmSwitchVisibility()
    {
        isGmInvisible = !isGmInvisible;
    }

    // change attribute for self or other player
    [Command]
    public void CmdGmChangeAttributeFor(int playerId, string attributeName, int value)
    {
        Player targetPlayer = GetPlayerById(playerId);
        if (targetPlayer == null)
            Inform("Player not found!");
        else
        {
            targetPlayer.attributes.ChangeValue(attributeName, value);
            targetPlayer.attributesSync = targetPlayer.attributes.CreateString();
            targetPlayer.ApplyAttributes();
            targetPlayer.TargetAttributeChanged(targetPlayer.connectionToClient, targetPlayer.attributesSync);
        }
    }

    //Inform client and perform adaptation
    [TargetRpc]
    public void TargetAttributeChanged(NetworkConnection target, string attributesSync)
    {
        attributes.CreateFromString(attributesSync);
        ApplyAttributes();
    }

    //change ability for self or other player
    [Command]
    public void CmdChangeAbitity(string abilityName, bool increase)
    {
        if (abilities.allocatedTotal >= GlobalVar.abilityTotal && increase)
        {
            Inform("You cannot add more abilities. Maybe you have to update your display.");
            return;
        }
        abilities.ChangeValue(abilityName, increase);
        abilitiesSync = abilities.CreateString();
        ApplyAbilities();
        TargetAbilityChanged(connectionToClient, abilitiesSync);
    }
    //Inform client and perform adaptation
    [TargetRpc]
    public void TargetAbilityChanged(NetworkConnection target, string abilitiesSync)
    {
        abilities.CreateFromString(abilitiesSync);
        ApplyAbilities();
    }

    // change skill for self or other player
    [Command]
    public void CmdGmChangeSkillFor(int playerId, int skillId, int experience)
    {
        Player targetPlayer = GetPlayerById(playerId);
        if (targetPlayer == null)
            Inform("Player not found!");
        else
        {
            targetPlayer.skills.SetExperience(skillId, experience);
        }
    }

    // working /////////////////////////////////////////////
    public void StartWorking(int usedHand)
    {
        if (GlobalFunc.IsInHand(usedHand))
        {
            workingHand = usedHand;
        }
        else
        {
            StopWorking();
        }
    }
    [Command]
    public void CmdStopWorking()
    {
        StopWorking();
    }
    public void StopWorking()
    {
        workingHand = -1;
        useOverTimeStoped = true;
    }

    // Get position parameter from server to inform
    [Command]
    public void CmdShowAmbientPosition(Vector3 position)
    {
        AmbientController.ShowAmbientPositionToPlayer(position, this);
    }

    // Create an ambient (semistatic) element
    [Command]
    public void CmdCreateAmbientElement(string itemName, Vector3 position, float size, bool rotateY, bool rotateXZ, string specialName, string textCanRead, string textCannotRead)
    {
        AmbientElement element = new AmbientElement();
        element.n = itemName + "-" + id.ToString() + "-" + GameTime.SecondsSinceZero().ToString();
        element.px = position.x;
        element.py = position.y;
        element.pz = position.z;
        element.rx = rotateXZ ? UnityEngine.Random.value * 360f : 0f;
        element.ry = rotateY ? UnityEngine.Random.value * 360f : 0f;
        element.rz = rotateXZ ? UnityEngine.Random.value * 360f : 0f;
        element.sx = size;
        element.sy = size;
        element.sz = size;
        int phaseEndTime = int.MaxValue;
        ScriptableItem itemData;
        if (ScriptableItem.dict.TryGetValue(itemName.GetStableHashCode(), out itemData))
        {
            if (itemData is GatheringSourceItem)
            {
                GatheringSourceItem gsi = (GatheringSourceItem)itemData;
                float days = gsi.lifePhases[0].dayInPhase;
                if (days > 0)
                {
                    phaseEndTime = GameTime.SecondsSinceZero() + (int)(GlobalFunc.RandomObfuscation(GlobalVar.gatheringResourcePhaseTimeObfuscation) * days * GlobalVar.vegetationCycleInSeconds);
                }
            }
        }
        element.da = phaseEndTime;
        element.i = itemName;
        element.d1 = 0;
        element.d2 = phaseEndTime;
        element.d3 = 0;
        element.id = GlobalVar.defaultDurability;
        element.iq = GlobalVar.defaultQuality;
        element.sn = specialName;
        element.tr = textCanRead;
        element.ti = textCannotRead;
        AmbientController.CreateAmbientElement(element);
    }

    //Screenshot and webcam
    public bool webcamActive = false;
    public void Screenshot(bool showUI = true)
    {
        string fileName = string.Format("{0}\\Anega{1:yyyy-MM-dd_HH_mm_ss}.jpg", PlayerPreferences.screenshotFolder, DateTime.Now);

        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        if (showUI)
        {
            screenShot = ScreenCapture.CaptureScreenshotAsTexture();
        }
        else
        {
            RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);

            Camera.main.targetTexture = rt;
            Camera.main.Render();
            RenderTexture.active = rt;

            screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            Camera.main.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
        }

        byte[] bytes = screenShot.EncodeToJPG(PlayerPreferences.screenshotQuality);
        System.IO.File.WriteAllBytes(fileName, bytes);

        UIScreenInfo screenInfo = GameObject.Find("Canvas/ScreenInfo").GetComponent<UIScreenInfo>();
        screenInfo.Screenshot();
    }

    // List of all player from Server view
    public Dictionary<int, string> requestedPlayerList = new Dictionary<int, string>();
    // Client request list
    [Command]
    public void CmdGmRequestPlayerList()
    {
        string tmp = "";
        List<Player> players = Player.onlinePlayers.Values.ToList();
        foreach (Player iPlayer in players)
        {
            // create a string with all active player
            //id#displayName#
            tmp += string.Format("{0}#{1}#", iPlayer.id, iPlayer.displayName);
        }
        TargetGmReceivePlayerList(this.connectionToClient, tmp);
    }

    //receive value and apply, used for GM only
    [TargetRpc]
    void TargetGmReceivePlayerList(NetworkConnection target, string playerList)
    {
        string[] tmp = playerList.Split('#');
        int i = 0;
        while (i < (tmp.Length - 1))
        {
            int id = int.Parse(tmp[i]);
            string dispName = tmp[i + 1];
            requestedPlayerList.Add(id, dispName);
            i = i + 2;
        }
    }

    // find player by given ID
    private Player GetPlayerById(int playerId)
    {
        foreach (KeyValuePair<string, Player> onlinePlayerKVP in Player.onlinePlayers)
        {
            if (onlinePlayerKVP.Value.id == playerId)
                return onlinePlayerKVP.Value;
        }
        return null;
    }

    //-- Test & Debug --------------------------------
    // call server procedure
    [Command]
    public void CmdTestAndDebug(string input)
    {
        GameObject go = GameObject.Find("Canvas/TestAndDebug");
        UITestAndDebug testAndDebug = go.GetComponent<UITestAndDebug>();
        testAndDebug.SpecialServerTest(this, input);
    }
}
