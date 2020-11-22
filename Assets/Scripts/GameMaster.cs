/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// GameMaser control
// helper to understand GM sync string
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameMaster
{
    // definition
    // 1###################### is GM
    // #1##################### is God
    // ##11################### number of permitted teleports for player
    // ####11################# number of permitted kills
    // ######1################ show advanced info
    // #######1############### display GM in Overlay
    // ########1############## see all player names
    // #########1############# can kill monster
    // ##########1############ can pull monster
    // ###########1########### can enter GM island
    // ############1########## can see all player
    // #############1######### can broadcast
    // ##############1######## unlimited health
    // ###############1####### unlimited mana
    // ################1###### unlimited stamina
    // #################1##### can become invisible
    // ##################1#### 1 1 see, 2 edit attributes and abilities
    // ###################1### can create unlimited items
    // ####################1## can build unlimited environment
    // #####################1# can change skills
    // ######################1 can change health, mana and stamina

    public static string EmptySyncString =      "00000000000000000000000";
    public static string typeGod =              "11999910111011111121111";
    public static string typeSimplePlayerGm =   "10100001101090000000000";
    public static string typeAdvancedPlayerGm = "10350201111111101111100";

    public static string CorrectSyncString(string syncString)
    {
        if (syncString.Length < EmptySyncString.Length)
            syncString += EmptySyncString.Substring(0, EmptySyncString.Length - syncString.Length);
        return syncString;
    }

    public static bool isGM(string syncString)
    {
        return syncString.Substring(0, 1) == "1";
    }
    public static bool isGod(string syncString)
    {
        return syncString.Substring(1, 1) == "1";
    }
    public static int hasTeleports(string syncString)
    {
        if (isGod(syncString))
            return 99;
        else if (int.TryParse(syncString.Substring(2, 2), out int number))
            return number;
        return 0;
    }
    public static string useTeleport(string syncString)
    {
        if (int.TryParse(syncString.Substring(2, 2), out int number))
            syncString = syncString.Remove(2, 2).Insert(2, Mathf.Max(0, number - 1).ToString("00"));
        return syncString;
    }
    public static int hasKills(string syncString)
    {
        if (isGod(syncString))
            return 99;
        else if (int.TryParse(syncString.Substring(4, 2), out int number))
            return number;
        return 0;
    }
    public static string useKill(string syncString)
    {
        if (int.TryParse(syncString.Substring(4, 2), out int number))
            syncString = syncString.Remove(4, 2).Insert(4, Mathf.Max(0, number - 1).ToString("00"));
        return syncString;
    }
    public static bool isShowAdvancedInfo(string syncString)
    {
        return syncString.Substring(6, 1) == "1" || isGod(syncString);
    }
    public static bool showGmInOverlay(string syncString)
    {
        if (isGod(syncString))
            return false;
        else
            return syncString.Substring(7, 1) == "1";
    }

    public static bool knowNames(string syncString)
    {
        return syncString.Substring(8, 1) == "1" || isGod(syncString);
    }
    public static bool killMonster(string syncString)
    {
        return syncString.Substring(9, 1) == "1" || isGod(syncString);
    }
    public static bool pullMonster(string syncString)
    {
        return syncString.Substring(10, 1) == "1" || isGod(syncString);
    }
    public static bool enterGmIsland(string syncString)
    {
        return syncString.Substring(11, 1) == "1" || isGod(syncString);
    }
    public static bool seeAllPlayer(string syncString)
    {
        return syncString.Substring(12, 1) == "1" || isGod(syncString);
    }
    public static bool broadcast(string syncString)
    {
        return syncString.Substring(13, 1) == "1" || isGod(syncString);
    }
    public static bool unlimitedHealth(string syncString)
    {
        return syncString.Substring(14, 1) == "1" || isGod(syncString);
    }
    public static bool unlimitedMana(string syncString)
    {
        return syncString.Substring(15, 1) == "1" || isGod(syncString);
    }
    public static bool unlimitedStamina(string syncString)
    {
        return syncString.Substring(16, 1) == "1" || isGod(syncString);
    }
    public static bool canInvisibility(string syncString)
    {
        return syncString.Substring(17, 1) == "1" || isGod(syncString);
    }
    public static bool seeAbilitiesAndAttributes(string syncString)
    {
        return syncString.Substring(18, 1) == "1" || syncString.Substring(18, 1) == "2" || isGod(syncString);
    }
    public static bool changeAbilitiesAndAttributes(string syncString)
    {
        return syncString.Substring(18, 1) == "2" || isGod(syncString);
    }
    public static bool createItems(string syncString)
    {
        return syncString.Substring(19, 1) == "1" || isGod(syncString);
    }
    public static bool buildEnvironment(string syncString)
    {
        return syncString.Substring(20, 1) == "1" || isGod(syncString);
    }
    public static bool changeSkills(string syncString)
    {
        return syncString.Substring(21, 1) == "1" || isGod(syncString);
    }
    public static bool changeBasics(string syncString)
    {
        return syncString.Substring(22, 1) == "1" || isGod(syncString);
    }
}
