/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// player can change this
// file at %USERPROFILE%\AppData\Local\Anega\PlayerPreferences.json
using UnityEngine;
using System;
using System.IO;
public class PlayerPreferences : MonoBehaviour
{
    // Logging
    public static string logLevel = "11001111";
    public static float turningSpeed = 6f;
    public static float creepSpeed = 0.5f;
    //Camera
    public static float cameraDistance = 6.0f;
    public static float cameraAngle = 25.0f;
    public static float cameraRotationSpeed = 2.0f;
    public static float cameraZoomSpeed = 1.0f;
    public static string screenshotFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +  GlobalVar.screenshotDefaultFolder;
    public static int screenshotQuality= 75;
    public static float webcamCycle = GlobalVar.webcamDefaultCycle;
    //Keys
    public static KeyCode keyToggleCamera = KeyCode.Y;
    public static KeyCode keyRun1 = KeyCode.RightShift;
    public static KeyCode keyRun2 = KeyCode.LeftShift;
    public static KeyCode keyCreep1 = KeyCode.RightControl;
    public static KeyCode keyCreep2 = KeyCode.LeftControl;
    public static KeyCode keyDoNotWalk = KeyCode.LeftAlt;
    public static KeyCode keyReleaseAction = KeyCode.F2;
    public static KeyCode keyCancelAction = KeyCode.Escape;
    public static KeyCode keyClosestMonster = KeyCode.Tab;
    public static float keyPressedLongClick = 0.35f;

    //Chat behavior
    public static bool stayInChat = true;
    public static string chatPrefixWhisper = "/w";
    public static string chatPrefixLoud = "/l";
    public static string chatPrefixShout = "/s";
    public static string chatPrefixOoc = "/o";
    public static string chatPrefixEmotion = "/me";
    public static string chatPrefixParty = "/p";
    public static string chatPrefixGm = "/gm";
    public static string chatPrefixIntroduce = "#i";
    public static string overlayGmNameAdd = "GM:";
    //Colors
    public static Color chatColorWhisper = new Color(0.85f, 0.85f, 0.85f, 1.0f);
    public static Color chatColorLocal = Color.white;
    public static Color chatColorLoud = new Color(1.0f, 0.9f, 0.9f, 1.0f);
    public static Color chatColorShout = Color.red;
    public static Color chatColorOoc = Color.cyan;
    public static Color chatColorEmotion = Color.yellow;
    public static Color chatColorInfo = new Color(0.6f, 0.8f, 0.6f, 1.0f);
    public static Color chatColorParty = new Color(1.0f, 0.5f, 1.0f, 1.0f);
    public static Color chatColorGm = new Color(0.5f, 1.0f, 0.8f, 1.0f);

    // Display
    public static int framesUntilFade = 50;
    public static int maxBlood = 40;
    public static Color myBloodColor = new Color(0.95f, 0.15f, 0.15f, 1.0f);
    public static Color otherBloodColor = new Color(0.6f, 0.15f, 0.15f, 1.0f);

    // Stamina and weight
    public static Color staminaBarPlus = new Color(1.0f, 0.85f, 0.0f, 1.0f);
    public static Color staminaBarMinus = new Color(1.0f, 0.35f, 0.0f, 1.0f);
    public static float weightWarningLimit = 0.9f;
    public static Color weightWarningColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);

    // Name Overlay colors
    public static Color nameOverlayNpcColor = Color.cyan;
    public static Color nameOverlayMonsterColor = Color.red;
    public static Color nameOverlayOffenderColor = Color.magenta;
    public static Color nameOverlayMurdererColor = new Color(0.7f, 0.0f, 0.0f, 1.0f);
    public static Color nameOverlayPetColor = new Color(0.7f, 0.0f, 0.0f, 1.0f);
    public static Color healthColorUnharmed = new Color(0.0f, 0.85f, 0.0f, 1.0f);
    public static Color healthColorSlightlyWounded = new Color(0.5f, 0.85f, 0.0f, 1.0f);
    public static Color healthColorWounded = new Color(1.0f, 0.95f, 0.0f, 1.0f);
    public static Color healthColorBadlyWounded = new Color(1.0f, 0.5f, 0.0f, 1.0f);
    public static Color healthColorNearDeath = new Color(0.9f, 0.0f, 0.0f, 1.0f);
    public static Color healthColorDeath = Color.grey;



    // Character states
    // CharacterState= friend, ally, neutral, dubious, outlaw, enemy, murder
    public static Color[] characterStateColor = {
        new Color(0.0f, 0.9f, 0.0f, 1.0f),
        new Color(0.0f, 0.9f, 0.9f, 1.0f),
        new Color(1.0f, 1.0f, 1.0f, 1.0f),
        new Color(1.0f, 0.7f, 0.5f, 1.0f),
        new Color(1.0f, 0.0f, 1.0f, 1.0f),
        new Color(1.0f, 0.0f, 0.5f, 1.0f),
        new Color(0.8f, 0.0f, 0.0f, 1.0f)};
    public static string[] characterStateText =    {
        "Friend","Ally","neutral","dubious","Outlaw","Enemy","Murder" };

    // Inventory Layout
    public static int inventoryDefaultWidth = 5;
    public static int inventoryDefaultHeight = 4;
    public static bool inventoryDynamic = true;
    public static int inventoryDynamicMax = 10;

    // Warnings
    public static int warningDurabilityLow = 9;


    // helper, never called!
    // create a PlayerPreferences example file
    private static void HelpCreateJSONFile()
    {
        string fileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Anega\\PlayerPreferencesExample.json";
        PlayerPreferencesParameter pp = new PlayerPreferencesParameter();
        string fileData = JsonUtility.ToJson(pp, true);
        StreamWriter fileStream;
        fileStream = File.CreateText(fileName);
        fileStream.WriteLine(fileData);
        fileStream.Close();
    }

    public static void LoadParameter()
    {
        string fileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Anega\\PlayerPreferences.json";
        if (File.Exists(fileName))
        {
            try
            {
                StreamReader stream = new StreamReader(fileName);
                string jsonFile = stream.ReadToEnd();
                stream.Close();
                PlayerPreferencesParameter ppp = new PlayerPreferencesParameter();
                ppp = JsonUtility.FromJson<PlayerPreferencesParameter>(jsonFile);

                if (ppp.logLevel != null) logLevel = ppp.logLevel + "11111111";
                if (ppp.turningSpeed > 0) turningSpeed = ppp.turningSpeed;
                if (ppp.creepSpeed > 0) creepSpeed = ppp.creepSpeed;
                if (creepSpeed > 1) creepSpeed = 0.8f; //do NotFiniteNumberException allow creep for running

                if (ppp.cameraDistance > 0) cameraDistance = ppp.cameraDistance;
                if (ppp.cameraAngle > 0) cameraAngle = ppp.cameraAngle;
                if (ppp.cameraRotationSpeed > 0) cameraRotationSpeed = ppp.cameraRotationSpeed;
                if (ppp.cameraZoomSpeed > 0) cameraZoomSpeed = ppp.cameraZoomSpeed;
                if (ppp.screenshotFolder != null) screenshotFolder = ppp.screenshotFolder;
                if (ppp.screenshotQuality >-1) screenshotQuality = ppp.screenshotQuality;
                if (ppp.webcamCycle > 0) webcamCycle = ppp.webcamCycle;

                if (ppp.keyToggleCamera > 0) keyToggleCamera = ppp.keyToggleCamera;
                if (ppp.keyRun1 > 0) keyRun1 = ppp.keyRun1;
                if (ppp.keyRun2 > 0) keyRun2 = ppp.keyRun2;
                if (ppp.keyCreep1 > 0) keyCreep1 = ppp.keyCreep1;
                if (ppp.keyCreep2 > 0) keyCreep2 = ppp.keyCreep2;
                if (ppp.keyDoNotWalk > 0) keyDoNotWalk = ppp.keyDoNotWalk;
                if (ppp.keyReleaseAction > 0) keyReleaseAction = ppp.keyReleaseAction;
                if (ppp.keyCancelAction > 0) keyCancelAction = ppp.keyCancelAction;
                if (ppp.keyClosestMonster > 0) keyClosestMonster = ppp.keyClosestMonster;
                if (ppp.keyPressedLongClick > 0) keyPressedLongClick = ppp.keyPressedLongClick;

                if (ppp.stayInChat > 0) stayInChat = ppp.stayInChat != 0;
                if (ppp.chatPrefixWhisper != null) chatPrefixWhisper = ppp.chatPrefixWhisper;
                if (ppp.chatPrefixLoud != null) chatPrefixLoud = ppp.chatPrefixLoud;
                if (ppp.chatPrefixShout != null) chatPrefixShout = ppp.chatPrefixShout;
                if (ppp.chatPrefixOoc != null) chatPrefixOoc = ppp.chatPrefixOoc;
                if (ppp.chatPrefixEmotion != null) chatPrefixEmotion = ppp.chatPrefixEmotion;
                if (ppp.chatPrefixParty != null) chatPrefixParty = ppp.chatPrefixParty;
                if (ppp.chatPrefixGm != null) chatPrefixGm = ppp.chatPrefixGm;
                if (ppp.chatPrefixIntroduce != null) chatPrefixIntroduce = ppp.chatPrefixIntroduce;

                if (ppp.chatColorWhisper != null) chatColorWhisper = ppp.chatColorWhisper;
                if (ppp.chatColorLocal != null) chatColorLocal = ppp.chatColorLocal;
                if (ppp.chatColorLoud != null) chatColorLoud = ppp.chatColorLoud;
                if (ppp.chatColorShout != null) chatColorShout = ppp.chatColorShout;
                if (ppp.chatColorOoc != null) chatColorOoc = ppp.chatColorOoc;
                if (ppp.chatColorEmotion != null) chatColorEmotion = ppp.chatColorEmotion;
                if (ppp.chatColorInfo != null) chatColorInfo = ppp.chatColorInfo;
                if (ppp.chatColorParty != null) chatColorParty = ppp.chatColorParty;
                if (ppp.chatColorGm != null) chatColorGm = ppp.chatColorGm;

                if (ppp.framesUntilFade > 0) framesUntilFade = ppp.framesUntilFade;
                if (ppp.maxBlood > -1) maxBlood = ppp.maxBlood;
                if (ppp.otherBloodColor != null) otherBloodColor = ppp.otherBloodColor;
                if (ppp.myBloodColor != null) myBloodColor = ppp.myBloodColor;

                if (ppp.staminaBarPlus != null) staminaBarPlus = ppp.staminaBarPlus;
                if (ppp.staminaBarMinus != null) staminaBarMinus = ppp.staminaBarMinus;
                if (ppp.weightWarningLimit > 0) weightWarningLimit = ppp.weightWarningLimit;
                if (ppp.weightWarningColor != null) weightWarningColor = ppp.weightWarningColor;

                if (ppp.nameOverlayNpcColor != null) nameOverlayNpcColor = ppp.nameOverlayNpcColor;
                if (ppp.nameOverlayMonsterColor != null) nameOverlayMonsterColor = ppp.nameOverlayMonsterColor;
                if (ppp.nameOverlayOffenderColor != null) nameOverlayOffenderColor = ppp.nameOverlayOffenderColor;
                if (ppp.nameOverlayMurdererColor != null) nameOverlayMurdererColor = ppp.nameOverlayMurdererColor;
                if (ppp.nameOverlayPetColor != null) nameOverlayPetColor = ppp.nameOverlayPetColor;
                if (ppp.healthColorUnharmed != null) healthColorUnharmed = ppp.healthColorUnharmed;
                if (ppp.healthColorSlightlyWounded != null) healthColorSlightlyWounded = ppp.healthColorSlightlyWounded;
                if (ppp.healthColorWounded != null) healthColorWounded = ppp.healthColorWounded;
                if (ppp.healthColorBadlyWounded != null) healthColorBadlyWounded = ppp.healthColorBadlyWounded;
                if (ppp.healthColorNearDeath != null) healthColorNearDeath = ppp.healthColorNearDeath;
                if (ppp.healthColorDeath != null) healthColorDeath = ppp.healthColorDeath;

                if (ppp.characterStateColor != null) characterStateColor = ppp.characterStateColor;
                if (ppp.characterStateText != null) characterStateText = ppp.characterStateText;

                if (ppp.inventoryDefaultWidth > 0) inventoryDefaultWidth = ppp.inventoryDefaultWidth;
                if (ppp.inventoryDefaultHeight > 0) inventoryDefaultHeight = ppp.inventoryDefaultHeight;
                if (ppp.inventoryDynamic > 0) inventoryDynamic = ppp.inventoryDynamic != 0;
                if (ppp.inventoryDynamicMax > 0) inventoryDynamicMax = ppp.inventoryDynamicMax;

                if (ppp.warningDurabilityLow > -1) warningDurabilityLow = ppp.warningDurabilityLow;

                //verify some parameter so it has to be checked once only
                if (!Directory.Exists(screenshotFolder))
                {
                    screenshotFolder= Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + GlobalVar.screenshotDefaultFolder;
                }
                screenshotQuality = Mathf.Clamp(screenshotQuality, 1, 100);

                LogFile.WriteLog(LogFile.LogLevel.Info, "PlayerPreferences loaded from file 'PlayerPreferences.json'.");
            }
            catch (Exception ex)
            {
                LogFile.WriteException(LogFile.LogLevel.Error, ex, fileName + " not loaded. Most likely a JSON format issue.");
            }

        }
    }


    class PlayerPreferencesParameter
    {
#pragma warning disable 0649
        // disable warning "warning CS0649: Field '...' is never assigned to, and will always have its default value 

        public string logLevel;
        public float turningSpeed = -1f;
        public float creepSpeed = -1f;

        public float cameraDistance = -1f;
        public float cameraAngle = -1f;
        public float cameraRotationSpeed = -1f;
        public float cameraZoomSpeed = -1f;
        public string screenshotFolder;
        public int screenshotQuality = -1;
        public float webcamCycle = -1f;

        public KeyCode keyToggleCamera = KeyCode.None;
        public KeyCode keyRun1 = KeyCode.None;
        public KeyCode keyRun2 = KeyCode.None;
        public KeyCode keyCreep1 = KeyCode.None;
        public KeyCode keyCreep2 = KeyCode.None;
        public KeyCode keyDoNotWalk = KeyCode.None;
        public KeyCode keyReleaseAction = KeyCode.None;
        public KeyCode keyCancelAction = KeyCode.None;
        public KeyCode keyClosestMonster = KeyCode.None;
        public float keyPressedLongClick = -1f;

        public int stayInChat = -1;
        public string chatPrefixWhisper;
        public string chatPrefixLoud;
        public string chatPrefixShout;
        public string chatPrefixOoc;
        public string chatPrefixEmotion;
        public string chatPrefixParty;
        public string chatPrefixGm;
        public string chatPrefixIntroduce;

        public Color chatColorWhisper;
        public Color chatColorLocal;
        public Color chatColorLoud;
        public Color chatColorShout;
        public Color chatColorOoc;
        public Color chatColorEmotion;
        public Color chatColorInfo;
        public Color chatColorParty;
        public Color chatColorGm;

        public int framesUntilFade = -1;
        public int maxBlood = -1;
        public Color myBloodColor;
        public Color otherBloodColor;

        public Color staminaBarPlus;
        public Color staminaBarMinus;
        public float weightWarningLimit = -1f;
        public Color weightWarningColor;

        public Color nameOverlayNpcColor;
        public Color nameOverlayMonsterColor;
        public Color nameOverlayOffenderColor;
        public Color nameOverlayMurdererColor;
        public Color nameOverlayPetColor;
        public Color healthColorUnharmed;
        public Color healthColorSlightlyWounded;
        public Color healthColorWounded;
        public Color healthColorBadlyWounded;
        public Color healthColorNearDeath;
        public Color healthColorDeath;

        public Color[] characterStateColor;
        public string[] characterStateText;

        public int inventoryDefaultWidth = -1;
        public int inventoryDefaultHeight = -1;
        public int inventoryDynamic = -1;
        public int inventoryDynamicMax = -1;

        public int warningDurabilityLow = -1;
#pragma warning restore 0649
    }
}
