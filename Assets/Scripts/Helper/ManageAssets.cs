/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text.RegularExpressions;


public class ManageAssets : MonoBehaviour
{
    public void ShowExplorer()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Anega";
        System.Diagnostics.Process.Start("explorer.exe", path);
    }

    // Items
    public void CountItems()
    {
        Debug.Log(ScriptableItem.dict.Count + " items found");
    }

    public void SaveListItems()
    {
        string itemList = "item;display name;tooltip;max stack;price;weight;durability;"
                            + "decay time;"
                            + "PARRY;level;best portion;luck portion;luck max;damage ignore;"
                            + "ATTACK;skill;level;max damage;time;range;luck portion;luck max;"
                            + "ARMOR;skill;level;max protection;min passing damage;luck portion;luck max;"
                            + "TOOL;skill;level;distance;minTime;maxTime;luck portion;luck max;"
                            + "SOURCE;phases;decay visible;"
                            + "BOOK;author;title;pages;letters;type;"
                            + "LEARNSPELL;spell;skill level" + Environment.NewLine
                        + " ; ; ;count;copper;gram;seconds;"
                            + "seconds;"
                            + "#####;0-100;0-1;0-1;float;HP;"
                            + "######;name;0-100;HP;s;m;0-1;float;"
                            + "#####;name;0-100;HP;HP;0-1;float;"
                            + "Gathering;name;0-100;m;s;s;0-1;float;"
                            + "Gathering;int;0-1;"
                            + "#####;name;name;int;int;type;"
                            + "#####;name;0-100" + Environment.NewLine;

        foreach (KeyValuePair<int, ScriptableItem> kvp in ScriptableItem.dict)
        {
            ScriptableItem scriptableItem = kvp.Value;
            itemList += string.Format("{0};{1};{2};{3};{4};{5};{6}"
                , scriptableItem.name
                , scriptableItem.itemName
                , scriptableItem.toolTipText
                , scriptableItem.maxStack
                , scriptableItem.price
                , scriptableItem.weight
                , scriptableItem.maxDurability
                );
            if (scriptableItem is UsableItem)
            {
                UsableItem usableItem = (UsableItem)scriptableItem;
                itemList += string.Format(";{0}"
                    , usableItem.decayTime
                    );
            }
            else
            {
                itemList += ";";
            }
            if (scriptableItem is ParryItem)
            {
                ParryItem parryItem = (ParryItem)scriptableItem;
                itemList += string.Format(";1;{0};{1};{2};{3};{4}"
                    , parryItem.levelParry
                    , parryItem.parryBestPortion
                    , parryItem.luckPortionParry
                    , parryItem.luckMaxParry
                    , parryItem.damageIgnore
                    );
            }
            else
            {
                itemList += ";0;;;;;";
            }
            if (scriptableItem is WeaponItem)
            {
                WeaponItem weaponItem = (WeaponItem)scriptableItem;
                itemList += string.Format(";1;{0};{1};{2};{3};{4};{5};{6}"
                    , Skills.Name(weaponItem.skillWeapon)
                    , weaponItem.levelWeapon
                    , weaponItem.maxDamage
                    , weaponItem.attackTime
                    , weaponItem.attackRange
                    , weaponItem.luckPortionWeapon
                    , weaponItem.luckMaxWeapon

                    );
            }
            else
            {
                itemList += ";0;;;;;;;";
            }
            if (scriptableItem is ClothingItem)
            {
                ClothingItem clothingItem = (ClothingItem)scriptableItem;
                itemList += string.Format(";1;{0};{1};{2};{3};{4};{5}"
                    , Skills.Name(clothingItem.skillArmor)
                    , clothingItem.levelArmor
                    , clothingItem.maxProtection
                    , clothingItem.minPassingDamage
                    , clothingItem.luckPortionDefense
                    , clothingItem.luckMaxDefense
                    );
            }
            else
            {
                itemList += ";0;;;;;;";
            }
            if (scriptableItem is GatheringToolItem)
            {
                GatheringToolItem gatheringToolItem = (GatheringToolItem)scriptableItem;
                itemList += string.Format(";1;{0};{1};{2};{3};{4};{5};{6}"
                    , Skills.Name(gatheringToolItem.skillTool)
                    , gatheringToolItem.levelTool
                    , gatheringToolItem.workDistance
                    , gatheringToolItem.workTimeMin
                    , gatheringToolItem.workTimeMax
                    , gatheringToolItem.luckPortionTool
                    , gatheringToolItem.luckMaxTool
                    );
            }
            else
            {
                itemList += ";0;;;;;;;";
            }
            if (scriptableItem is GatheringSourceItem)
            {
                GatheringSourceItem gatheringSourceItem = (GatheringSourceItem)scriptableItem;
                itemList += string.Format(";1;{0};{1}"
                    , gatheringSourceItem.lifePhases.Length
                    , gatheringSourceItem.changeInvisibleOnly
                    );
            }
            else
            {
                itemList += ";0;;";
            }
            if (scriptableItem is BookItem)
            {
                BookItem bookItem = (BookItem)scriptableItem;
                itemList += string.Format(";1;{0};{1};{2};{3};{4}"
                    , bookItem.author
                    , bookItem.title
                    , Regex.Matches(bookItem.bookText, "<p>").Count + 1
                    , bookItem.bookText.Length
                    , bookItem.bookType
                    );
            }
            else
            {
                itemList += ";0;;;;;";
            }
            if (scriptableItem is LearnSpellItem)
            {
                LearnSpellItem learnSpellItem = (LearnSpellItem)scriptableItem;
                itemList += string.Format(";1;{0};{1}"
                    , learnSpellItem.spell.displayName
                    , learnSpellItem.minSkillLevel
                    );
            }
            else
            {
                itemList += ";0;;";
            }
            itemList += Environment.NewLine;
        }

        string fileName = ItemListFileName();
        StreamWriter fileStream;
        fileStream = File.CreateText(fileName);
        fileStream.WriteLine(itemList);
        fileStream.Close();
        Debug.Log("Item List created: " + fileName);
    }

    public void LoadListItems()
    {
        string fileName = ItemListFileName();
        if (File.Exists(fileName))
        {
            int countChanges = 0;
            StreamReader stream = new StreamReader(fileName);
            string line;
            string changedItems = "";
            int counter = 0;
            while ((line = stream.ReadLine()) != null)
            {
                // ignore empty lines, usually the last one
                if (!Utils.IsNullOrWhiteSpace(line))
                {
                    // ignore first 2 lines. This is the header!
                    if (counter > 1)
                    {
                        string[] columns = line.Split(';');
                        if (columns.Length >= 37)
                        {
                            int column = 0;
                            bool itemChanged = false;
                            ScriptableItem scriptableItem = Resources.Load<ScriptableItem>("Items/" + columns[0]);
                            if (scriptableItem)
                            {
                                column = 1;
                                scriptableItem.itemName = GlobalFunc.EqualAndConvert(ref itemChanged, scriptableItem.itemName, columns[column++]);
                                scriptableItem.toolTipText = GlobalFunc.EqualAndConvert(ref itemChanged, scriptableItem.toolTipText, columns[column++]);
                                scriptableItem.maxStack = GlobalFunc.EqualAndConvert(ref itemChanged, scriptableItem.maxStack, columns[column++]);
                                scriptableItem.price = GlobalFunc.EqualAndConvert(ref itemChanged, scriptableItem.price, columns[column++]);
                                scriptableItem.weight = GlobalFunc.EqualAndConvert(ref itemChanged, scriptableItem.weight, columns[column++]);
                                scriptableItem.maxDurability = GlobalFunc.EqualAndConvert(ref itemChanged, scriptableItem.maxDurability, columns[column++]);

                                // usable item part
                                if (int.TryParse(columns[6], out int decayTime))
                                {
                                    if (scriptableItem is UsableItem)
                                    {
                                        UsableItem usableItem = (UsableItem)scriptableItem;
                                        usableItem.decayTime = GlobalFunc.EqualAndConvert(ref itemChanged, usableItem.decayTime, columns[6]);
                                    }
                                }

                                // parry part
                                column = 7;
                                if (columns[column++] == "1")
                                {
                                    if (scriptableItem is ParryItem)
                                    {
                                        ParryItem parryItem = (ParryItem)scriptableItem;
                                        parryItem.levelParry = GlobalFunc.EqualAndConvert(ref itemChanged, parryItem.levelParry, columns[column++]);
                                        parryItem.parryBestPortion = GlobalFunc.EqualAndConvert(ref itemChanged, parryItem.parryBestPortion, columns[column++]);
                                        parryItem.luckPortionParry = GlobalFunc.EqualAndConvert(ref itemChanged, parryItem.luckPortionParry, columns[column++]);
                                        parryItem.luckMaxParry = GlobalFunc.EqualAndConvert(ref itemChanged, parryItem.luckMaxParry, columns[column++]);
                                        parryItem.damageIgnore = GlobalFunc.EqualAndConvert(ref itemChanged, parryItem.damageIgnore, columns[column++]);
                                    }
                                    else
                                    {
                                        Debug.Log("Parry item changes cannot be applied to the non parry item " + scriptableItem.name);
                                    }
                                }

                                // attack part
                                column = 13;
                                if (columns[column++] == "1")
                                {
                                    if (scriptableItem is WeaponItem)
                                    {
                                        WeaponItem weaponItem = (WeaponItem)scriptableItem;

                                        weaponItem.skillWeapon = GlobalFunc.EqualAndConvert(ref itemChanged, weaponItem.skillWeapon, columns[column++]);
                                        weaponItem.levelWeapon = GlobalFunc.EqualAndConvert(ref itemChanged, weaponItem.levelWeapon, columns[column++]);
                                        weaponItem.maxDamage = GlobalFunc.EqualAndConvert(ref itemChanged, weaponItem.maxDamage, columns[column++]); ;
                                        weaponItem.attackTime = GlobalFunc.EqualAndConvert(ref itemChanged, weaponItem.attackTime, columns[column++]);
                                        weaponItem.attackRange = GlobalFunc.EqualAndConvert(ref itemChanged, weaponItem.attackRange, columns[column++]);
                                        weaponItem.luckPortionWeapon = GlobalFunc.EqualAndConvert(ref itemChanged, weaponItem.luckPortionWeapon, columns[column++]);
                                        weaponItem.luckMaxWeapon = GlobalFunc.EqualAndConvert(ref itemChanged, weaponItem.luckMaxWeapon, columns[column++]);
                                    }
                                    else
                                    {
                                        Debug.Log("Attack parameter changes cannot be applied to the non weapon item " + scriptableItem.name);
                                    }
                                }

                                // armor part
                                column = 21;
                                if (columns[column++] == "1")
                                {
                                    if (scriptableItem is ClothingItem)
                                    {
                                        ClothingItem clothingItem = (ClothingItem)scriptableItem;
                                        clothingItem.skillArmor = GlobalFunc.EqualAndConvert(ref itemChanged, clothingItem.skillArmor, columns[column++]);
                                        clothingItem.levelArmor = GlobalFunc.EqualAndConvert(ref itemChanged, clothingItem.levelArmor, columns[column++]);
                                        clothingItem.maxProtection = GlobalFunc.EqualAndConvert(ref itemChanged, clothingItem.maxProtection, columns[column++]);
                                        clothingItem.minPassingDamage = GlobalFunc.EqualAndConvert(ref itemChanged, clothingItem.minPassingDamage, columns[column++]);
                                        clothingItem.luckPortionDefense = GlobalFunc.EqualAndConvert(ref itemChanged, clothingItem.luckPortionDefense, columns[column++]);
                                        clothingItem.luckMaxDefense = GlobalFunc.EqualAndConvert(ref itemChanged, clothingItem.luckMaxDefense, columns[column++]);

                                    }
                                    else
                                    {
                                        Debug.Log("Armor parameter changes cannot be applied to the non clothing item " + scriptableItem.name);
                                    }
                                }

                                // gathering tool part
                                column = 28;
                                if (columns[column++] == "1")
                                {
                                    if (scriptableItem is GatheringToolItem)
                                    {
                                        GatheringToolItem gatheringToolItem = (GatheringToolItem)scriptableItem;
                                        gatheringToolItem.skillTool = GlobalFunc.EqualAndConvert(ref itemChanged, gatheringToolItem.skillTool, columns[column++]);
                                        gatheringToolItem.levelTool = GlobalFunc.EqualAndConvert(ref itemChanged, gatheringToolItem.levelTool, columns[column++]);
                                        gatheringToolItem.workDistance = GlobalFunc.EqualAndConvert(ref itemChanged, gatheringToolItem.workDistance, columns[column++]);
                                        gatheringToolItem.workTimeMin = GlobalFunc.EqualAndConvert(ref itemChanged, gatheringToolItem.workTimeMin, columns[column++]);
                                        gatheringToolItem.workTimeMax = GlobalFunc.EqualAndConvert(ref itemChanged, gatheringToolItem.workTimeMax, columns[column++]);
                                        gatheringToolItem.luckPortionTool = GlobalFunc.EqualAndConvert(ref itemChanged, gatheringToolItem.luckPortionTool, columns[column++]);
                                        gatheringToolItem.luckMaxTool = GlobalFunc.EqualAndConvert(ref itemChanged, gatheringToolItem.luckMaxTool, columns[column++]);

                                    }
                                    else
                                    {
                                        Debug.Log("Gathering tool parameter changes cannot be applied to the non gathering tool item " + scriptableItem.name);
                                    }
                                }

                                // gathering source part
                                column = 36;
                                if (columns[column++] == "1")
                                {
                                    if (scriptableItem is GatheringSourceItem)
                                    {
                                        GatheringSourceItem gatheringSourceItem = (GatheringSourceItem)scriptableItem;

                                        column++; //gatheringSourceItem.phases.Length
                                        gatheringSourceItem.changeInvisibleOnly = GlobalFunc.EqualAndConvert(ref itemChanged, gatheringSourceItem.changeInvisibleOnly, columns[column++]);
                                    }
                                    else
                                    {
                                        Debug.Log("Gathering source parameter changes cannot be applied to the non gathering source item " + scriptableItem.name);
                                    }
                                }

                                // update if any change
                                if (itemChanged)
                                {
                                    EditorUtility.SetDirty(scriptableItem);
                                    changedItems += Environment.NewLine + columns[0];
                                    countChanges++;
                                }
                            }
                            else
                            {
                                Debug.Log("Item with name >" + columns[0] + "< does not exist. Maybe you have to create the item first.");
                            }
                        }
                        else
                        {
                            Debug.Log("Columns missing in line: " + line);
                        }
                    }
                    counter++;
                }
            }
            stream.Close();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(string.Format("{0} items out of {1} items changed.{2}", countChanges, counter - 2, changedItems));

        }
        else
        {
            Debug.Log("File " + fileName + " does not exists.");
        }
    }

    private string ItemListFileName()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Anega\\ItemList.csv";
    }

    // Gathering Items
    public void CountGatheringItems()
    {
        int count = 0;
        foreach (KeyValuePair<int, ScriptableItem> kvp in ScriptableItem.dict)
        {
            if (kvp.Value is GatheringSourceItem)
            {
                count++;
            }
        }
        Debug.Log(count + " gathering sourece items found");
    }

    public void SaveListGatheringItemsPhases()
    {
        string itemList = "item;model name;free range;change invisible;"
                        + "phase;model phase;skill;best skill;total tries;void try min;void try max;extra stuff;next used probability;next used default; next used special;effect;timeout days;next timeout probability;next timeout default;next timeout special;"
                        + "item;amount;probability;durbility;quality;message" + Environment.NewLine
                        + " ; ;m;0/1;"
                        + "#;#;skill;0-100;int;0-1;0-1;0-1;0-1;phase;phase; ;day;0-1;phase;phase;"
                        + " ;int;int;0-100;0-100; " + Environment.NewLine;

        foreach (KeyValuePair<int, ScriptableItem> kvp in ScriptableItem.dict)
        {
            ScriptableItem scriptableItem = kvp.Value;
            if (scriptableItem is GatheringSourceItem)
            {
                GatheringSourceItem gatheringSourceItem = (GatheringSourceItem)scriptableItem;

                itemList += string.Format("{0};{1};{2};{3}" + Environment.NewLine
                    , scriptableItem.name
                    , gatheringSourceItem.modelPrefab.name
                    , gatheringSourceItem.freeRange
                    , gatheringSourceItem.changeInvisibleOnly
                    );

                int phase = 0;
                while (phase < gatheringSourceItem.lifePhases.Length)
                {
                    bool phaseHeader = false;
                    GatheringLifePhase lifePhase = gatheringSourceItem.lifePhases[phase];

                    if (lifePhase.content.Length == 0)
                    {
                        itemList += GatheringItemPhase(lifePhase, phase) + Environment.NewLine;
                    }
                    else
                    {
                        foreach (GatheringContent item in lifePhase.content)
                        {
                            if (phaseHeader)
                            {
                                itemList += GatheringItemPhase();
                            }
                            else
                            {
                                itemList += GatheringItemPhase(lifePhase, phase);
                                phaseHeader = true;
                            }
                            itemList += string.Format(";{0};{1};{2};{3};{4};{5}"
                                    , item.item.itemName
                                    , item.amount
                                    , item.probability
                                    , item.durability
                                    , item.quality
                                    , item.infoText);
                            itemList += Environment.NewLine;
                        }
                    }
                    phase++;
                }
                if (gatheringSourceItem.lifePhases.Length == 0)
                {
                    itemList += ";;;;Life phases missing" + Environment.NewLine;
                }
            }
        }

        string fileName = GatheringItemListFileName("Phases");
        StreamWriter fileStream;
        fileStream = File.CreateText(fileName);
        fileStream.WriteLine(itemList);
        fileStream.Close();
        Debug.Log("Gathering item list for life phases created: " + fileName);
    }
    private string GatheringItemPhase()
    {
        return ";;;;;;;;;;;;;;;;;;;";
    }
    private string GatheringItemPhase(GatheringLifePhase lifePhase, int phaseNumber)
    {
        string returnText = string.Format(";;;;{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15}"
            , phaseNumber
            , lifePhase.modelPhase
            , lifePhase.gatheringSkill.ToString()
            , lifePhase.bestSkillLevel
            , lifePhase.itemsInResource
            , lifePhase.voidTriesMin
            , lifePhase.voidTriesMax
            , lifePhase.probabilityExtraStuff
            , lifePhase.nextPhaseUsedDefaultProbability
            , lifePhase.nextPhaseUsedDefault
            , lifePhase.nextPhaseUsedSpecial
            , lifePhase.onUseEffect.ToString()
            , lifePhase.dayInPhase
            , lifePhase.nextPhaseTimeoutDefaultProbability
            , lifePhase.nextPhaseTimeoutDefault
            , lifePhase.nextPhaseTimeoutSpecial);
        return returnText;
    }

    public void SaveListGatheringItemsSeed()
    {
        string itemList = "item;model name;free range;change invisible;phases;"
                        + "action;vegetation;frequency;size min;size max;"
                        + "items;min distance;max distance;min amount;max amount" + Environment.NewLine
                        + "#; ;m;0/1;#;"
                        + " ; ;int;float;float;"
                        + " ;float;float;float;float" + Environment.NewLine;

        foreach (KeyValuePair<int, ScriptableItem> kvp in ScriptableItem.dict)
        {
            ScriptableItem scriptableItem = kvp.Value;
            if (scriptableItem is GatheringSourceItem)
            {
                GatheringSourceItem gatheringSourceItem = (GatheringSourceItem)scriptableItem;

                itemList += string.Format("{0};{1};{2};{3};{4}" + Environment.NewLine
                    , scriptableItem.name
                    , gatheringSourceItem.modelPrefab.name
                    , gatheringSourceItem.freeRange
                    , gatheringSourceItem.changeInvisibleOnly
                    , gatheringSourceItem.lifePhases.Length
                    );

                int action = 0;
                while (action < gatheringSourceItem.seedActions.Length)
                {
                    bool phaseHeader = false;
                    GatheringSeedRegion seedAction = gatheringSourceItem.seedActions[action];

                    if (seedAction.rules.Length == 0)
                    {
                        itemList += GatheringItemSeedAction(seedAction, action) + Environment.NewLine;
                    }
                    else
                    {
                        foreach (GatheringSeedRule rule in seedAction.rules)
                        {
                            if (phaseHeader)
                            {
                                itemList += GatheringItemSeedAction();
                            }
                            else
                            {
                                itemList += GatheringItemSeedAction(seedAction, action);
                                phaseHeader = true;
                            }
                            itemList += string.Format(";{0};{1};{2};{3};{4}"
                                    , rule.relatedItems.name
                                    , rule.minDistance
                                    , rule.maxDistance
                                    , rule.minAmount
                                    , rule.maxAmount);
                            itemList += Environment.NewLine;
                        }
                    }
                    action++;
                }
                if (gatheringSourceItem.seedActions.Length == 0)
                {
                    itemList += ";;;;;Seed actions missing" + Environment.NewLine;
                }
            }
        }

        string fileName = GatheringItemListFileName("SeedActions");
        StreamWriter fileStream;
        fileStream = File.CreateText(fileName);
        fileStream.WriteLine(itemList);
        fileStream.Close();
        Debug.Log("Gathering item list for seed actions created: " + fileName);
    }
    private string GatheringItemSeedAction()
    {
        return ";;;;;;;;;";
    }
    private string GatheringItemSeedAction(GatheringSeedRegion action, int actionNumber)
    {
        string returnText = string.Format(";;;;;{0};{1};{2};{3};{4}"
            , actionNumber
            , action.VegetationType.ToString()
            , action.frequency
            , action.sizeMin
            , action.sizeMax);
        return returnText;
    }

    private string GatheringItemListFileName(string type)
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Anega\\GatheringItem" + type.Trim() + "List.csv";
    }

    // fighting NPC
    public Fighter[] fightingNPC;
    public void CountFightingNPC()
    {
        int countPets = 0;
        int countMounts = 0;
        int countMonster = 0;
        foreach (Fighter fighter in fightingNPC)
        {
            if (fighter is Monster)
            {
                countMonster++;
            }
            else if (fighter is Pet)
            {
                countPets++;
            }
            else if (fighter is Mount)
            {
                countMounts++;
            }
        }
        Debug.Log(string.Format("{0} monster; {1} pets; {2} mounts", countMonster, countPets, countMounts));
    }

    public void SaveListFightingNPC()
    {
        string npcList = "NPC;GENERAL;displayed name;level;health;health recovery;active;mana;mana recovery;active;speed;"
                            + "ATTACK;skill;level;min damage;max damage;distance;"
                            + "DODGE;probability;"
                            + "PARRY;probability;damage ignore;"
                            + "ARMOR;skill;min protection;max protection;min passing damage;"
                            + "MOVE;probability;range move;range follow;attack distance" + Environment.NewLine
                        + " ;#######;0-100;%;%;bool;%;%;bool;%;"
                            + "#####;name;0-100;HP;HP;m;"
                            + "#####;0-1;"
                            + "######;0-1;HP;"
                            + "#####;name;HP;HP;HP;"
                            + "####;0-1(/s);m;m;0.1-1" + Environment.NewLine;

        foreach (Fighter fighter in fightingNPC)
        {
            string textType = "unknown";
            if (fighter is Monster)
            {
                textType = "monster";
            }
            else if (fighter is Pet)
            {
                textType = "pet";
            }
            else if (fighter is Mount)
            {
                textType = "mount";
            }
            npcList += string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};;{10};{11};{12};{13};{14};;{15};;{16};{17};;{18};{19};{20};{21};{22}",
                fighter.name,
                textType,
                fighter.displayedName,
                fighter.defenseLevel,
                fighter.HealthLevel,
                fighter.HealthRecoveryLevel,
                fighter.healthRecovery.ToString(),
                fighter.ManaLevel,
                fighter.ManaRecoveryLevel,
                fighter.manaRecovery.ToString(),
                fighter.SpeedLevel,
                Skills.Name(fighter.attackSkill),
                fighter.attackLevel,
                fighter.attackMin,
                fighter.attackMax,
                fighter.attackDistance,
                fighter.dodgeProbability,
                fighter.blockProbability,
                fighter.blockDamageIgnore,
                Skills.Name(fighter.armorSkill),
                fighter.armorMin,
                fighter.armorMax,
                fighter.armorNotConsumed
                );
            if (fighter is Monster)
            {
                Monster monster = (Monster)fighter;
                npcList += string.Format(";1;{0};{1};{2};{3}"
                    , monster.moveProbability
                    , monster.moveDistance
                    , monster.followDistance
                    , monster.attackToMoveRangeRatio
                    );
            }
            else
            {
                npcList += ";0;;;;";
            }
            npcList += Environment.NewLine;
        }

        string fileName = FightingNPCListFileName();
        StreamWriter fileStream;
        fileStream = File.CreateText(fileName);
        fileStream.WriteLine(npcList);
        fileStream.Close();
        Debug.Log("Fighting NPC List created: " + fileName);
    }

    public void LoadListFightingNPC()
    {
        string fileName = FightingNPCListFileName();
        if (File.Exists(fileName))
        {
            int countChanges = 0;
            StreamReader stream = new StreamReader(fileName);
            string line;
            int counter = 0;
            while ((line = stream.ReadLine()) != null)
            {
                // ignore empty lines, usually the last one
                if (!Utils.IsNullOrWhiteSpace(line))
                {
                    // ignore first 2 lines. This is the header!
                    if (counter > 1)
                    {
                        string[] columns = line.Split(';');
                        if (columns.Length >= 27)
                        {
                            bool npcChanged = false;
                            Fighter fighter = null;
                            foreach (Fighter fighterTmp in fightingNPC)
                            {
                                if (fighterTmp.name == columns[0])
                                {
                                    fighter = fighterTmp;
                                    break;
                                }
                            }

                            if (fighter)
                            {
                                int column = 2;
                                fighter.displayedName = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.displayedName, columns[column++]);
                                fighter.defenseLevel = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.defenseLevel, columns[column++]);
                                fighter.HealthLevel = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.HealthLevel, columns[column++]);
                                fighter.HealthRecoveryLevel = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.HealthRecoveryLevel, columns[column++]);
                                fighter.healthRecovery = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.healthRecovery, columns[column++]);
                                fighter.ManaLevel = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.ManaLevel, columns[column++]);
                                fighter.ManaRecoveryLevel = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.ManaRecoveryLevel, columns[column++]);
                                fighter.manaRecovery = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.manaRecovery, columns[column++]);
                                fighter.SpeedLevel = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.SpeedLevel, columns[column++]);
                                fighter.attackSkill = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.attackSkill, columns[column++]);
                                fighter.attackLevel = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.attackLevel, columns[column++]);
                                fighter.attackMin = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.attackMin, columns[column++]);
                                fighter.attackMax = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.attackMax, columns[column++]);
                                fighter.attackDistance = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.attackDistance, columns[column++]);
                                fighter.dodgeProbability = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.dodgeProbability, columns[column++]);
                                column = 19;
                                fighter.blockProbability = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.blockProbability, columns[column++]);
                                fighter.blockDamageIgnore = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.blockDamageIgnore, columns[column++]);
                                column = 22;
                                fighter.armorSkill = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.armorSkill, columns[column++]);
                                fighter.armorMin = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.armorMin, columns[column++]);
                                fighter.armorMax = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.armorMax, columns[column++]);
                                fighter.armorNotConsumed = GlobalFunc.EqualAndConvert(ref npcChanged, fighter.armorNotConsumed, columns[column++]);


                                // monster move part
                                column = 26;
                                if (columns[column++] == "1")
                                {
                                    if (fighter is Monster)
                                    {
                                        Monster monster = (Monster)fighter;

                                        monster.moveProbability = GlobalFunc.EqualAndConvert(ref npcChanged, monster.moveProbability, columns[column++]);
                                        monster.moveDistance = GlobalFunc.EqualAndConvert(ref npcChanged, monster.moveDistance, columns[column++]);
                                        monster.followDistance = GlobalFunc.EqualAndConvert(ref npcChanged, monster.followDistance, columns[column++]);
                                        monster.attackToMoveRangeRatio = GlobalFunc.EqualAndConvert(ref npcChanged, monster.attackToMoveRangeRatio, columns[column++]);

                                    }
                                    else
                                    {
                                        Debug.Log("Monster parameter changes cannot be applied to the non monster NPC " + fighter.name);
                                    }
                                }

                                // update if any change
                                if (npcChanged)
                                {
                                    EditorUtility.SetDirty(fighter);
                                    countChanges++;
                                }
                            }
                            else
                            {
                                Debug.Log("NPC with name >" + columns[0] + "< does not exist. Maybe you have to create the prefab first or add it to the editor list.");
                            }
                        }
                        else
                        {
                            Debug.Log("Columns missing in line: " + line);
                        }
                    }
                    counter++;
                }
            }
            stream.Close();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(string.Format("{0} fighting NPC out of {1} fighting NPC changed.", countChanges, counter - 2));

        }
        else
        {
            Debug.Log("File " + fileName + " does not exists.");
        }
    }

    private string FightingNPCListFileName()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Anega\\FightingNPCList.csv";
    }

    [Header("NPC Merchants")]
    public List<Npc> tradingNPC;

    public void UpdateDatabaseNpcTrading()
    {
        // external item list in database
        Database.FillItemList();
        Database.FillNpcMerchants(tradingNPC);
    }
    public void ApplyDatabaseNpcTradingToNPC()
    {
        // database to game
        Database.ApplyNPCTrading(tradingNPC, false);
    }
    public void ApplyDatabaseNpcTradingToNPCTest()
    {
        // Test database to game
        Database.ApplyNPCTrading(tradingNPC, true);
    }

    private string SpellListFileName()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Anega\\SpellList.csv";
    }

    public void SaveSpellList()
    {
        string spellList = "spell;display name;tooltip;image;animation;in spell list;cast bar;cancel on death;skill;skill level;need weapon;mana min;mana max;cast time min;cast time max;cooldown min;cooldown max;range min;range max"
                        + ";DAMAGE;damage;relative damage;stun chance;stun time;luck portion;luck max;effect" 
                        + ";HEAL;health;mana;relative;luck portion;luck max;effect"+ Environment.NewLine
                        + " ; ; ;png;name;bool;bool;bool;name;0-100;bool;MP;MP;s;s;s;s;m;m"
                        + ";#####;HP;bool;0-1;s;0-1;float;name"
                        + ";#####;HP;MP;bool;0-1;float;name"+ Environment.NewLine;

        foreach (KeyValuePair<int, ScriptableSpell> kvp in ScriptableSpell.dict)
        {
            ScriptableSpell scriptableSpell = kvp.Value;
            spellList += string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16};{17};{18}"
                , scriptableSpell.name
                , scriptableSpell.displayName
                , scriptableSpell.toolTipText
                , scriptableSpell.image
                , scriptableSpell.castAnimation

                , scriptableSpell.showInSpellList
                , scriptableSpell.showCastBar
                , scriptableSpell.cancelCastIfTargetDied
                , scriptableSpell.skill.ToString()
                , scriptableSpell.skillLevel
                , scriptableSpell.requiresWeapon
                , scriptableSpell.manaCostsNewbe
                , scriptableSpell.manaCostsMaster
                , scriptableSpell.castTimeNewbe
                , scriptableSpell.castTimeMaster
                , scriptableSpell.cooldownNewbe
                , scriptableSpell.cooldownMaster
                , scriptableSpell.castRangeNewbe
                , scriptableSpell.castRangeMaster
                );
            if (scriptableSpell is DamageSpell)
            {
                DamageSpell damageSpell = (DamageSpell)scriptableSpell;
                spellList += string.Format(";1;{0};{1};{2};{3};{4};{5};{6}"
                    , damageSpell.maxDamage
                    , damageSpell.isRelativeDamage
                    , damageSpell.stunChance
                    , damageSpell.stunTimeMax
                    , damageSpell.luckPortion
                    , damageSpell.luckMax
                    , (damageSpell.effect == null?"":damageSpell.effect.name)
                    );
            }
            else
            {
                spellList += ";;;;;;;;";
            }

            if (scriptableSpell is HealSpell)
            {
                HealSpell healSpell = (HealSpell)scriptableSpell;
                spellList += string.Format(";1;{0};{1};{2};{3};{4};{5}"
                    , healSpell.healsHealth
                    , healSpell.healsMana
                    , healSpell.isRelativeHeals
                    , healSpell.luckPortion
                    , healSpell.luckMax
                    , healSpell.effect.name
                    );
            }
            else
            {
                spellList += ";;;;;;;";
            }
            spellList += Environment.NewLine;
        }

        string fileName = SpellListFileName();
        StreamWriter fileStream;
        fileStream = File.CreateText(fileName);
        fileStream.WriteLine(spellList);
        fileStream.Close();
        Debug.Log("Spell List created: " + fileName);
    }
}
#endif