/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
//#define PRODUCTION //<<< activate for production
using System;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVar
{
    // Test and Debug
    // isProduction must be true for production
#if PRODUCTION
    public const bool isProduction = true;
    public const bool makeGmAutomatically = false;
#else
    public const bool isProduction = false;
    public const bool makeGmAutomatically = true;
#endif
    public static float testGameTimeOffset = 0f;
    public static float testGameTimeSpeed = 1f;

    // directories and files
    public const string gameDevDir = "C:\\Daten\\Anega\\AnegaGame";
    public const string garmentRecipesDir = "Assets\\Garment\\Recipes";
    public const string screenshotDefaultFolder = "\\Pictures";

    public const string semistaticItemsFile = "{0}\\ServerSave\\{1}_semistaticitems.json";
    // general
    public const int intFalse = 0;
    public const int intTrue = 1;
    public const ulong allDirty = 0xFFFFFFFF;
    // loading screen
    public const float loadingBlackSeconds = 2f;
    public const float loadingFadeSeconds = 1f;
    // Networking
    public const float networkMaxWaitTime = 10f;
    // Logging
    public const string logImpossibleErrorText = "This should not be possible. Please inform a developer!";
    // general
    public const string nameNotKnown = "Someone";
    public const int repeatInitializationAttempt = 2;
    public const int cycleCheckStartDelay = 5;
    public const int cycleCheckForDecay = 13;
    public const int cycleCheckSemistaticElements = 17;
    public const int cycleSaveSemistaticElements = 600; //>>> this must be much higher for productione
    public const float loadingPanelBlackSeconds = 2f;
    public const float loadingPanelFadeSeconds = 1f;
    // Layers
    public const int layerIgnoreRaycast = 2;
    public const int layerElement = 18;
    public const int layerMaskElement = 262144; //18
    public const int layerMaskGround = 458752; //16, 17, 18
    public const int layerMaskTerrain = 196608; // 16, 17
    public const int layerMaskTerrainAdd = 131072; //17
    public const int layerMaskWater = 16; //4
    // base colors
    public static Color colorText = new Color(0.23f, 0.23f, 0.23f, 1.0f);
    public static Color colorTextGood = new Color(0.0f, 0.7f, 0.0f, 1.0f);
    public static Color colorTextBad = new Color(0.7f, 0.0f, 0.0f, 1.0f);
    // Names
    public const int displayedNameMaxLength = 24;
    public const int displayedNameMaxLengthExtended = 35;
    // walking
    public const float walkBackRelativeSpeed = 0.2f;
    public const float walkInInteractionRange = 0.9f;
    public const float walkMinStoppingDistance = 1.1f; //hack necessary since distance <1.1 becomes 0
    public const float walkHighHangingElement = 2.0f;
    // Camera
    public const float cameraMinDistance = 3.0f;
    public const float cameraMaxDistance = 20;
    public const float cameraZoomSpeedTouch = 0.2f;
    public const float cameraXMinAngle = -40;
    public const float cameraXMaxAngle = 80;
    public const float cameraFieldOfViewDefault = 60;
    public const float cameraFieldOfViewMax = 57;
    public const float cameraFieldOfViewMin = 2;
    public const int cameraFieldOfViewNonLinear = 12;

    // Chat
    public const float chatDistanceOoc = 30.0f;
    public const float chatDistanceEmotion = 50.0f;
    public const float chatDistanceIntroduce = 5.0f;
    public const float chatDistanceMax = 5000.0f;
    public const int chatMaxTextLength = 70;
    public const int chatKeepHistory = 100;
    public const string chatWelcomeText = "Welcome to Anega - Have fun!";


    // states
    public const int stateIdle = 0;
    public const int stateMoving = 1;
    public const int stateStunned = 2;
    public const int stateCasting = 3;
    public const int stateAttacking = 4;
    public const int stateDead = 5;
    public const int stateTrading = 6;
    public const int stateCrafting = 7;
    public const int stateWorking = 8;

    // Ability states
    public const int abilityTotal = 18;
    public const int abilityMinMin = 1;
    public const int abilityMinMax = 1;

    // Attribute states
    public const int attributeMax = 20;
    public const int attributeTotal = 120;
    public const int attributeStartPenalty = 30;
    public const int attributeStartPenaltyDuration = 2; // in h
    public const int attributeStartPenaltyNonlinear = 0;

    // Unique start items
    public const int uniqueStartItemsMax = 3;
    public const int uniqueStartItemsMinNameLength = 3;

    // Adaptation chatacter apperance
    public const float charApperanceStartValue = 0.5f;
    public static Color charApperanceStartColor = new Color(1f, 1f, 1f, 1f);
    public const float charApperanceHeightStartValue = 0.5f;

    // Nonlinear curves
    public const string fileNonLinearCurves = "nonlinearcurves.json";
    public const int maxNonLinearCurves = 35; // a few more than exists in the file

    // Divergence
    public const float divergenceCalculationCycle = 113.0f; // seconds
    public const int divergenceBaseCompass = 4632; // seconds
    public const int divergenceBaseClock = 3294; // seconds
    public const int divergenceExamination = 2785; // seconds

    // Game Time
    public const string timeDate = "{0}. day {1} of {2}";

    // Unified Random
    public const int unifiedRandomAccuracy = 321; // in seconds how long a unified random value is the same

    // Animation control
    public const int numberOfDeadAnimations = 2;
    public const float animWalkForwardMultiplyer = 1f;
    public const float animWalkFBackwardMultiplyer = -1.5f;

    // Voice Control
    //[attribute,distance]
    //distances are: whisper, normal, loud, shout
    public static float[,] voiceControl = {
        { 0.5f, 4.0f, 8.0f, 12.0f },
        { 2.0f, 12.0f, 16.0f, 80.0f },
        { 2.5f, 20.0f, 30.0f, 80.0f },
        { 6.0f, 25.0f, 50.0f, 100.0f } };

    // inventorySize - Storekeeper
    public static int[] storekeeperInventorySize = { 100, 300, 750, 3000 };

    // cannot read / illiterate
    public const string illiterateBookName = "A Book";
    public const string illiterateBookAuthor = "???";
    public static string[] illiterateNavBookText = { "This looks interesting but doesn't make any sense to you." };
    public const string illiteratePoorBookText = "... further text becomes a jumbled mess of letters.";
    public const int illiteratePoorMaxText = 35; //shorter texts shown full
    public const int illiteratePoorCutText = 25; //other are cut at first space after

    // Light & Night Vision
    public const float ambientLightDay = 1f;
    public const float dayFog = 0.75f;
    public const float nightFog = 0.1f;
    public const float sunHeightAtNoonMin = 40f;
    public const float sunHeightAtNoonMax = 80f;
    public static float[] nightVisionAmbientLightSurface = { 0.1f, 0.3f, 0.5f, 0.8f };

    // Health
    public const int healthBaseValue = 1000;
    public const int healthRecoveryBaseValue = 50;
    public const int healthMin = 1000;
    public const int healthMax = 10000;
    public const int healtNonlinear = 4;
    public const int healthRecoveryMin = 16;
    public const int healthRecoveryMax = 100;
    public const int healthRecoveryNonlinear = 10;
    public const float healthMaxInjury = 0.9f;
    public const float healthLimitUnharmed = 0.96f;
    public const float healthLimitSlightlyWounded = 0.84f;
    public const float healthLimitWounded = 0.47f;
    public const float healthLimitBadlyWounded = 0.15f;
    public const string healthTextUnharmed = "unharmed";
    public const string healthTextSlightlyWounded = "slightly wounded";
    public const string healthTextWounded = "wounded";
    public const string healthTextBadlyWounded = "badly wounded";
    public const string healthTextNearDeath = "near death";
    public const string healthTextDeath = "dead";

    // Mana
    public const int manaBaseValue = 1000;
    public const int manaRecoveryBaseValue = 50;
    public const int manaMin = 1000;
    public const int manaMax = 10000;
    public const int manaNonlinear = 12;
    public const int manaRecoveryMin = 25;
    public const int manaRecoveryMax = 170;
    public const int manaRecoveryNonlinear = 3;

    // Spells
    public const int manaMasteryNonlinear = 29;
    public const int castTimeMasteryNonlinear = 29;
    public const int cooldownMasteryNonlinear = 29;
    public const int castRangeMasteryNonlinear =29;
    public const int bonusMasteryNonlinear =    29;

    // Stamnia
    public const int staminaConsumption = 50; // per second
    public const int staminaConsumptionSwimming = 55;
    public const int staminaMin = 100;
    public const int staminaMax = 10000;
    public const int staminaNonlinear = 4;
    public const int staminaRecoveryMin = 12;
    public const int staminaRecoveryMax = 40;
    public const int staminaRecoveryNonlinear = 12;
    public const int staminaNegaive = -1000;

    // Speed
    public const int speedBaseValue = 4;
    public const int monsterWalkValue = 3;
    public const float speedWalkMin = 2;
    public const float speedWalkMax = 6;
    public const int speedWalkNonlinear = 9;
    public const float speedRunMin = 8;
    public const float speedRunMax = 15;
    public const int speedRunNonlinear = 12;

    public static float[] areaSpeed = { 1f, 1f, 1f, 100f, 1f, 1f, 1.5f };
    public static float[] waterproofSpeed = { 0.01f, 0.05f, 0.4f, 0.95f };
    public static float[] deepWaterSpeed = { 0.001f, 0.001f, 0.07f, 0.3f };
    public static float[] roadRunnerSpeed = { 0.7f, 1f, 1.2f, 1.6f };

    // Lifting Capacity
    public const int liftingCapacityMin = 10000;
    public const int liftingCapacityMax = 100000;
    public const int liftingCapacityNonlinear = 3;
    public const int liftingCapacitySpeedInfluenceNonlinear = 18;

    // throwing distance
    public const float throwingDistanceMaxMin = 20f;
    public const float throwingDistanceMaxMax = 30f;
    public const int throwingDistanceMaxInfluenceNonlinear = 4;
    public const int throwingDistanceWeightMax = 5000;
    public const int throwingDistanceWeightInfluenceNonlinear = 19;

    // luck
    public const int luckLowNonlinear = 24;
    public const int luckHighNonlinear = 25;
    public const float luckHighMax = 5.0f;

    // Person detection
    public const float distanceDetectionPersonMin = 10.0f;
    public const float distanceDetectionPersonMax = 60.0f;
    public const int distanceDetectionPersonNonlinear = 8;

    // Playt time (in s)
    public const int playtimeAccuracy = 30;
    public const int playtimeInactivityLimit = 250;
    public const int playtimeMovmentReward = 120;
    public const int playhoursUntilNextAttributeChange = 2;

    // Revive
    public const float reviveStamina = 0.25f;
    public const int reviveDelay = 10; //s

    // Container
    public const int containerTypeNone = 0;
    public const int containerTypeEquipment = 1;
    public const int containerTypeMobile = 2;
    public const int containerTypePublic = 3;

    public const int containerEquipment = 1;
    public const int containerLoot = 2;
    public const int containerFirstPublic = 100;
    public const int containerLastPublic = 199;
    public const int containerFirstMobile = 1000;
    public const string containerDepotWrong = "useless empty chest";

    // Equipment
    public const int equipmentLeftHand = 0;
    public const int equipmentRightHand = 1;
    public const int equipmentHead = 2;
    public const int equipmentChest = 3;
    public const int equipmentHands = 4;
    public const int equipmentLegs = 5;
    public const int equipmentFoot = 6;
    public const int equipmentNeck = 7;
    public const int equipmentFinger1 = 8;
    public const int equipmentFinger2 = 9;
    public const int equipmentFinger3 = 10;
    public const int equipmentFinger4 = 11;
    public const int equipmentFinger5 = 12;
    public const int equipmentFinger6 = 13;
    public const int equipmentBelt1 = 14;
    public const int equipmentBelt2 = 15;
    public const int equipmentBelt3 = 16;
    public const int equipmentBelt4 = 17;
    public const int equipmentBelt5 = 18;
    public const int equipmentBelt6 = 19;
    public const int equipmentBackpack = 20;
    public const int equipmentSize = 21;

    // Skill
    public const int skillExperiencePerLevel = 10000;
    public const int skillDefaultLow = 1;
    public const int skillDefaultLowExp = (int)(5.25 * skillExperiencePerLevel);
    public const int skillDefaultMedium = 1;
    public const int skillDefaultMediumExp = (int)(14.5 * skillExperiencePerLevel);
    public const int skillDefaultHigh = 1;
    public const int skillDefaultHighExp = (int)(29.999 * skillExperiencePerLevel);
    public const int skillTotalTimeStart = 3600;
    public const int skillMaxExperiencePerAction = (int)(0.9 * skillExperiencePerLevel);
    public const float skillIdleRelation = 6;
    public const int skillBestWaitTime = 3600;
    public const int skillFitBestAt = 40;
    public const int skillFitInstationary = 20;
    public const int skillTotalLearnTimeDefault = 100000; //ca. 30h
    public const int skillLearnCurveDefault = 21;
    public const int skillExperienceLearnMin = 1;

    // Fighting
    public const int fightWeaponMasteryNonlinear = 26;
    public const int fightWeaponMasteryFitBestAt = 20;
    public const float fightAttackObfuscation = 0.5f;
    public const int fightAttributeNonlinear = 2;
    public const int fightDodgeSkillNonlinear = 1;
    public const int fightDodgeAttributesNonlinear = 4;
    public const int fightDodgeLoadNonlinear = 18;
    public const float fightDodgeMax = 0.5f;
    public const int fightBlockStunNonlinear = 0;
    public const int fightBlockStunMaxDamage = 10000;
    public const float fightBlockStunMaxTime = 10f;
    public const int fightArmorSkillRelationNonlinear = 3;
    public const int fightItemQualityNonlinear = 27;
    public const float fightItemQualityWorst = 0.25f;

    // damage
    public const int damageMaxBloodPermitted = 60;
    public const int damageBloodNonlinear = 9;

    public static int[,] fightArmorPortion =
    {
        {equipmentHead, 18},
        {equipmentChest, 63},
        {equipmentHands, 72},
        {equipmentLegs, 93},
        {equipmentFoot, 100}
    };

    // Spells
    public const int spellMasteryNonlinear = 26;
    public const int spellMasteryFitBestAt = 20;
    public const float spellObfuscation = 0.5f;
    public const int spellMaxRelativeEffect = 150;  //should be >100 to make a full effect with reductions possible
    public const float waitUntilTeleportRejected = 15f; // teleport
    public const float teleportPemittedMove = 2f;
    public const float minTeleportDistance = 0.8f;
    public const float groupTeleportCircle = 0.8f;
    // working
    public const int toolMasteryNonlinear = 26;
    public const int toolMasteryFitBestAt = 20;
    public const int toolItemQualityNonlinear = 27;
    public const int toolWorkingTimeNonlinear = 4; // reverse effect, higher is shorter!
    public const int toolAttributeNonlinear = 2;

    // gathering
    public const int gatheringItemsInResourceFitBestAt = 20;
    public const int gatheringItemsInResourceCurve = 26;
    public const float gatheringResourcePhaseTimeObfuscation = 0.6f;
    public const string gatheringTextNoResource = "There is nothing to pick here at the moment.";
    public const string gatheringTextHasResource = "It may contain something valuable.\nThe resource is {0} your skill level.";
    public static ExamineLimit[] gatheringSkillRange = {
        new ExamineLimit { limit= -5f, text= "far over" },
        new ExamineLimit { limit= -1f, text= "over" },
        new ExamineLimit { limit= 5f, text= "exact" },
        new ExamineLimit { limit= 25f, text= "below" },
        new ExamineLimit { limit= 100f, text= "far below" } };
    public const string gatheringTextNoTool = "";
    public const string gatheringTextTool = "You need a {0} to work here.";
    public const float gatheringLearnFromFailure = 0.15f;
    public const float gatheringToolLuckMin = 0.1f;
    public const float gatheringToolLuckMax = 100f;

    //items
    public const int defaultDurability = 50;
    public const int defaultQuality = 50;

    // Loot
    public const int lootMaxAmount = 10;

    // collect items
    public const float pickRange = 2f;

    // Examination
    public struct ExamineLimit
    {
        public float limit;
        public string text;
    }
    public static ExamineLimit[] generalExaminationSpeed = {
        new ExamineLimit { limit= 0.20f, text= "very slow" },
        new ExamineLimit { limit= 0.40f, text= "slow" },
        new ExamineLimit { limit= 0.60f, text= "normal" },
        new ExamineLimit { limit= 0.80f, text= "fast" },
        new ExamineLimit { limit= 10.0f, text= "very fast" } };
    public static ExamineLimit[] generalExaminationTime = {
        new ExamineLimit { limit= 0.10f, text= "a few seconds" },
        new ExamineLimit { limit= 0.33f, text= "a short time" },
        new ExamineLimit { limit= 0.56f, text= "some time" },
        new ExamineLimit { limit= 0.77f, text= "a little longer" },
        new ExamineLimit { limit= 10.0f, text= "a very long time" } };
    public const int ExaminationHealthDivergence = 750;
    public static ExamineLimit[] healthExaminationLow = {
        new ExamineLimit { limit= 0.52f ,text= "weak" },
        new ExamineLimit { limit= 10.0f ,text= "strong" }};
    public static ExamineLimit[] healthExaminationNormal = {
        new ExamineLimit { limit= 0.08f, text= "very weak" },
        new ExamineLimit { limit= 0.26f, text= "weak" },
        new ExamineLimit { limit= 0.52f, text= "medium strong" },
        new ExamineLimit { limit= 0.81f, text= "strong" },
        new ExamineLimit { limit= 0.93f, text= "powerful" },
        new ExamineLimit { limit= 10.0f, text= "overpowerd" } };
    public const int ExaminationManaDivergence = 750;
    public static ExamineLimit[] manaExaminationLow = {
        new ExamineLimit { limit= 0.42f ,text= "some" },
        new ExamineLimit { limit= 10.0f ,text= "a " }};
    public static ExamineLimit[] manaExaminationNormal = {
        new ExamineLimit { limit= 0.09f, text= "almost no" },
        new ExamineLimit { limit= 0.24f, text= "a small" },
        new ExamineLimit { limit= 0.49f, text= "a " },
        new ExamineLimit { limit= 0.78f, text= "a strong" },
        new ExamineLimit { limit= 0.92f, text= "a powerful" },
        new ExamineLimit { limit= 10.0f, text= "an unlimited" } };

    // Item Properties
    public const int itemQualityMax = 100;
    public static ExamineLimit[] itemQualityBase = {
        new ExamineLimit { limit= 0.07f, text= "trash" },
        new ExamineLimit { limit= 0.18f, text= "bad" },
        new ExamineLimit { limit= 0.29f, text= "poor" },
        new ExamineLimit { limit= 0.40f, text= "average" },
        new ExamineLimit { limit= 0.51f, text= "above average" },
        new ExamineLimit { limit= 0.62f, text= "good" },
        new ExamineLimit { limit= 0.73f, text= "exceptional" },
        new ExamineLimit { limit= 0.84f, text= "superb" },
        new ExamineLimit { limit= 0.95f, text= "exquisite" },
        new ExamineLimit { limit= 10.0f, text= "perfect" } };
    public const int itemDurabilityMax = 100;
    public static ExamineLimit[] itemDurabilityBase = {
        new ExamineLimit { limit= 0.06f, text= "decaying" },
        new ExamineLimit { limit= 0.17f, text= "ramshackly" },
        new ExamineLimit { limit= 0.30f, text= "damaged" },
        new ExamineLimit { limit= 0.55f, text= "normal" },
        new ExamineLimit { limit= 0.78f, text= "stable" },
        new ExamineLimit { limit= 0.95f, text= "new" },
        new ExamineLimit { limit= 10.0f, text= "brand-new" } };

    // Teleport
    public const float teleportPortalCastTime = 0.9f;
    public const int teleportPortalMana = 700;
    public const int teleportPortalCost = 500;

    // Wand bonus
    public const int wandBonusNonlinearCurve = 9;
    public const int wandBonusOffset = 20;

    // Price system
    public const float priceSellNormal = 0.1f;
    public const int priceStabilityTime = 19000; //about 5:15h
    public static float[] priceBestProbability = { 0.05f, 0.21f, 0.45f, 0.95f };
    public const float priceVariance = 0.2f;
    public const int priceCurveDurability = 22;
    public const float priceBestToAverageDurability = 2f;
    public const int priceCurveQuality = 23;
    public const float priceBestToAverageQuality = 10f;

    // light
    public const int lightIntensityReductionCurve = 18;
    public const float lightTimeAccuracy = 7.0f;
    public const int lightNeverLit = 0;
    public const string lightTimeNew = "It is new.";
    public static ExamineLimit[] lightTime = {
        new ExamineLimit { limit= 0.09f, text= "will burn down soon" },
        new ExamineLimit { limit= 0.41f, text= "will not last long" },
        new ExamineLimit { limit= 0.72f, text= "lasts for some time" },
        new ExamineLimit { limit= 10.0f, text= "lasts very long" } };

    // weight
    public const int weightTextLimit = 10000;
    public const int divergenceWeight = 983;
    public const float weightBaseSpread = 0.2f;
    public static ExamineLimit[] weightText = {
        new ExamineLimit { limit= 0.005f, text= "It is light as a feather." },
        new ExamineLimit { limit= 0.035f, text= "It is lightweight." },
        new ExamineLimit { limit= 0.092f, text= "It does not weigh much." },
        new ExamineLimit { limit= 1.000f, text= "It is heavy." },
        new ExamineLimit { limit= 100.0f, text= "It is extremly heavy." } };
    public static int[] weightBarAccuracy = { 3, 6, 12, 100 };

    // skill
    public static ExamineLimit[] skillLevelText = {
        new ExamineLimit { limit= 5f, text=    "noob" },
        new ExamineLimit { limit= 15f, text=   "beginner" },
        new ExamineLimit { limit= 25f, text=   "apprentice" },
        new ExamineLimit { limit= 35f, text=   "assistant" },
        new ExamineLimit { limit= 45f, text=   "journeyman" },
        new ExamineLimit { limit= 55f, text=   "practitioner" },
        new ExamineLimit { limit= 65f, text=   "adept" },
        new ExamineLimit { limit= 75f, text=   "authority" },
        new ExamineLimit { limit= 85f, text=   "expert" },
        new ExamineLimit { limit= 95f, text=   "master" },
        new ExamineLimit { limit= 1000f, text= "grand master" } };

    // damage (1.8*i)^e
    public static ExamineLimit[] damageAndHealText = {
        new ExamineLimit { limit= 0.001f, text= "nothing" },
        new ExamineLimit { limit= 5f, text= "almost nothing" },
        new ExamineLimit { limit= 33f, text= "insignificiant" },
        new ExamineLimit { limit= 98f, text= "few" },
        new ExamineLimit { limit= 214f, text= "little" },
        new ExamineLimit { limit= 393f, text= "some" },
        new ExamineLimit { limit= 644f, text= "considerably" },
        new ExamineLimit { limit= 980f, text= "some more" },
        new ExamineLimit { limit= 1408f, text= "much" },
        new ExamineLimit { limit= 1940f, text= "quite a bit" },
        new ExamineLimit { limit= 2583f, text= "huge amount" },
        new ExamineLimit { limit= 3347f, text= "incedible high" },
        new ExamineLimit { limit= 10000000f, text= "infinite" } };

    // stun time
    public static ExamineLimit[] stunTimeText = {
        new ExamineLimit { limit= 0.001f, text= "nothing" },
        new ExamineLimit { limit= 0.2f, text= "very short" },
        new ExamineLimit { limit= 0.6f, text= "short" },
        new ExamineLimit { limit= 1.2f, text= "considerably" },
        new ExamineLimit { limit= 2.4f, text= "long" },
        new ExamineLimit { limit= 4.8f, text= "very long" },
        new ExamineLimit { limit= 9.6f, text= "extrem long" },
        new ExamineLimit { limit= 10000000f, text= "infinite" } };

    // luck portion
    public static ExamineLimit[] luckPortionText = {
        new ExamineLimit { limit= 0.0001f, text= "nothing" },
        new ExamineLimit { limit= 0.023f, text= "insignificiant" },
        new ExamineLimit { limit= 0.057f, text= "feelable" },
        new ExamineLimit { limit= 0.11f, text= "significiant" },
        new ExamineLimit { limit= 0.22f, text= "much" },
        new ExamineLimit { limit= 0.34f, text= "huge" },
        new ExamineLimit { limit= 0.53f, text= "extraordinary" },
        new ExamineLimit { limit= 0.9991f, text= "unbelievable" },
        new ExamineLimit { limit= 10f, text= "everything" } };

    // hit frequency per minute
    public static ExamineLimit[] hitsPerMinuteText = {
        new ExamineLimit { limit= 1f, text=    "every now and then" },
        new ExamineLimit { limit= 3.1f, text=   "few" },
        new ExamineLimit { limit= 6.5f, text=   "some" },
        new ExamineLimit { limit= 10.2f, text=  "significiant amount" },
        new ExamineLimit { limit= 14.9f, text=  "a lot" },
        new ExamineLimit { limit= 21.3f, text=  "many" },
        new ExamineLimit { limit= 54.4f, text=  "very many" },
        new ExamineLimit { limit= 93.2f, text=  "incredible many" },
        new ExamineLimit { limit= 10000f, text= "countless" } };

    // master skill relation to noom
    public static ExamineLimit[] relationMasterNoobText = {
        new ExamineLimit { limit= 0.01f, text=    "amost drops to zero" },
        new ExamineLimit { limit= 0.025f, text=   "falls very strong" },
        new ExamineLimit { limit= 0.1f, text=     "falls a lot" },
        new ExamineLimit { limit= 0.41f, text=    "falls" },
        new ExamineLimit { limit= 0.67f, text=    "is halved" },
        new ExamineLimit { limit= 0.9f, text=     "drops a bit" },
        new ExamineLimit { limit= 1.1f, text=     "almost equal" },
        new ExamineLimit { limit= 93.2f, text=    "inreases a little" },
        new ExamineLimit { limit= 93.2f, text=    "doubles" },
        new ExamineLimit { limit= 93.2f, text=    "rises" },
        new ExamineLimit { limit= 93.2f, text=    "rises a lot" },
        new ExamineLimit { limit= 93.2f, text=    "rises very strongly" },
        new ExamineLimit { limit= 10000f, text=   "almost goes to infinity" } };
    // mana requirement for sill
    public static ExamineLimit[] manaConsumptionText = {
        new ExamineLimit { limit= 0.04f, text= "almost nothing" },
        new ExamineLimit { limit= 0.12f, text= "somewhat" },
        new ExamineLimit { limit= 0.24f, text= "sensible" },
        new ExamineLimit { limit= 0.49f, text= "noteworthy" },
        new ExamineLimit { limit= 0.78f, text= "very much" },
        new ExamineLimit { limit= 0.98f, text= "almost everything" },
        new ExamineLimit { limit= 1000.0f, text= "everything" } };
    // cast time (seconds)
    public static ExamineLimit[] spellCastTimeText = {
        new ExamineLimit { limit= 0.2f, text= "almost immediately" },
        new ExamineLimit { limit= 1.2f, text= "very fast" },
        new ExamineLimit { limit= 2.6f, text= "fast" },
        new ExamineLimit { limit= 4.9f, text= "not bad" },
        new ExamineLimit { limit= 8.1f, text= "long" },
        new ExamineLimit { limit= 21.3f, text= "very long" },
        new ExamineLimit { limit= 1000.0f, text= "extremly long" } }; 
    // cast time (seconds)
    public static ExamineLimit[] spellCooldownTimeText = {
        new ExamineLimit { limit= 0.5f, text= "almost immediately" },
        new ExamineLimit { limit= 4f, text= "very fast" },
        new ExamineLimit { limit= 60f, text= "seconds" },
        new ExamineLimit { limit= 600f, text= "few minutes" },
        new ExamineLimit { limit= 3000f, text= "some minutes" },
        new ExamineLimit { limit= 25000f, text= "hours" },
        new ExamineLimit { limit= 100000f, text= "days" },
        new ExamineLimit { limit= 10000000.0f, text= "ages" } }; 
    // cast time (seconds)
    public static ExamineLimit[] spellRangeText = {
        new ExamineLimit { limit= 0.05f, text= "non" },
        new ExamineLimit { limit= 1f, text= "immediate vicinity" },
        new ExamineLimit { limit= 2.5f, text= "nearby" },
        new ExamineLimit { limit= 10f, text= "some meters" },
        new ExamineLimit { limit= 30f, text= "not so far" },
        new ExamineLimit { limit= 90f, text= "far" },
        new ExamineLimit { limit= 1000f, text= "very far" },
        new ExamineLimit { limit= 10000000.0f, text= "everywhere" } };     
    // teleport distance (m)
    public static ExamineLimit[] teleportDistanceText = {
        new ExamineLimit { limit= 0.05f, text= "non" },
        new ExamineLimit { limit= 1f, text= "immediate vicinity" },
        new ExamineLimit { limit= 2.5f, text= "nearby" },
        new ExamineLimit { limit= 10f, text= "some meters" },
        new ExamineLimit { limit= 25f, text= "not so far" },
        new ExamineLimit { limit= 50f, text= "far" },
        new ExamineLimit { limit= 100f, text= "very far" },
        new ExamineLimit { limit= 10000000.0f, text= "extremly far" } };
    // wand activation time (seconds)
    public static ExamineLimit[] wandActivationeTimeText = {
        new ExamineLimit { limit= 0.5f, text=  "immediately" },
        new ExamineLimit { limit= 10f, text= "very fast" },
        new ExamineLimit { limit= 60f, text= "seconds" },
        new ExamineLimit { limit= 600f, text= "few minutes" },
        new ExamineLimit { limit= 3000f, text= "some minutes" },
        new ExamineLimit { limit= 25000f, text= "hours" },
        new ExamineLimit { limit= 100000f, text= "days" },
        new ExamineLimit { limit= 10000000.0f, text= "ages" } };
    // wand bonus effect
    public static ExamineLimit[] wandBonusEffectText = {
        new ExamineLimit { limit= 1f, text=  "negative" },
        new ExamineLimit { limit= 1.1f, text= "a little" },
        new ExamineLimit { limit= 1.4f, text= "some" },
        new ExamineLimit { limit= 1.8f, text= "almost doubles" },
        new ExamineLimit { limit= 2.5f, text= "doubles" },
        new ExamineLimit { limit= 5f, text= "very much" },
        new ExamineLimit { limit= 100f, text= "extremly" } };
    // Item creation in sight
    public const float maxCreationDistance = 25f;

    // GM definitions
    public const float gmTeleportDistance = 2.0f;
    public const KeyCode gmReleaseKey = KeyCode.F2;

    // GM abuse control
    // The abuse is checked every 5 minutes, Alarm if>limit, count reduced by reduction
    public const int gmNpcKillAlarmLimit = 5;
    public const int gmNpcKillReduction = 2;
    public const int gmNpcPullAlarmLimit = 5;
    public const int gmNpcPullReduction = 2;

    // Interface control
    public const float panelUpdateDelay = 0.3f;

    // Ambient control
    public const float ambientActionDistanceToPlayer = 5;
    public const float minSizeCreation = 0.1f;
    public const float maxSizeCreation = 10.0f;
    public const int totalSizeCompression = 1000;
#if PRODUCTION
    public const int vegetationCycleInSeconds =  GameTime.secondPerDay;
#else
    public const int vegetationCycleInSeconds = 300; //>>> change for production, this should be GameTime.secondPerDay (29700)
#endif
    public const float cycleSeedVegetration = 0.05f;
    public const int triesPerSeedCycle = 3;
    public const float waterDeepSeedMax = 5f;
    public const float waterShallow = 0.05f;
    public const float rockSizeMax = 10f;

    //bookk design
    public static int bookInitialWidth = 512;
    public static int bookInitialHeight = 384;
    public static int bookInitialTextSize = 20;
    public static float bookMinSize = 0.5f;
    public  static float bookMaxSize = 2f;
    public  static float bookSizeStep = 0.1f;
    public static string[] bookPageSplit = { "<p>" };

    //Camera
    public const float webcamDefaultCycle = 120f;

    //NavMesh
    public const string recalculateNavMeshWorkingFolder = "RecalculateNavMesh_Delete";
    public const float recalculateNavMeshMoveDown = 0.2f;
    public const float recalculateNavMeshMoveUp = 0.05f;
     
    public const float depthShallowWater = 0.01f;
    public const float depthDeepWater = 1.00f;
    public const float navMeshRayLength = 15;
    public const string navMeshRoadLayers = "TerrainLayer_Path|TerrainLayer_Cobblestones";
    
    public const int navMeshAreaWalkable = 0;
    public const int navMeshAreaShallowWater = 3;
    public const int navMeshAreaDeepWater = 4;
    public const int navMeshAreaRoad = 6;

    public enum VegetationType
    {
        NoVegetation = 0,
        ShallowWater = 2,
        PineForestLight = 10,
        PineForestDense = 11,
        BroadleafForestDense = 21,
        BroadleafForestLight = 20,
        ForestSwamp = 31,
        OpenSwamp = 32,
        RockyMountain = 42,
        HighMountain = 51,
        Prairie = 52,
        Meadow = 57
    }

    // MouseFollow
    public const float maxRaycastMouseFollow = 150;
    public static Color mouseFollowDanger = new Color32(226, 226, 97, 207);
    public static Color mouseFollowActive = new Color32(226, 97, 226, 207);
    public static Color mouseFollowInactive = new Color32(226, 97, 226, 70);

    // FlashEffects
    public const int flashEffectSwirly = 0;
    public const int flashEffectWandActive = 1; 
}
