/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ManageAssets))]
public class EditorManageAssets : Editor
{
    public override void OnInspectorGUI()
    {
        ManageAssets myScript = (ManageAssets)target;

        EditorGUILayout.LabelField("Files location", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Anega\\");
        if (GUILayout.Button("Open Folder"))
        {
            myScript.ShowExplorer();
        }

        EditorGUILayout.LabelField("Item Lists", EditorStyles.boldLabel);
        if (GUILayout.Button("Count Items"))
        {
            myScript.CountItems();
        }
        if (GUILayout.Button("Save all items to ItemList.csv"))
        {
            myScript.SaveListItems();
        }
        if (GUILayout.Button("Load ItemList.csv and update items"))
        {
            myScript.LoadListItems();
        }

        EditorGUILayout.LabelField("Vegetation and Gathering Lists", EditorStyles.boldLabel);
        if (GUILayout.Button("Count Gathering Items"))
        {
            myScript.CountGatheringItems();
        }
        if (GUILayout.Button("Save gathering item phases to GatheringItemPhaseList.csv"))
        {
            myScript.SaveListGatheringItemsPhases();
        }
        if (GUILayout.Button("Save gathering item seed settings to GatheringItemSeedList.csv"))
        {
            myScript.SaveListGatheringItemsSeed();
        }

        EditorGUILayout.LabelField("NPC Merchants", EditorStyles.boldLabel);
        if (GUILayout.Button("Load NPC and items into database"))
        {
            myScript.UpdateDatabaseNpcTrading();
        }
        if (GUILayout.Button("Test groups and items from database to NPC"))
        {
            myScript.ApplyDatabaseNpcTradingToNPCTest();
        }
        if (GUILayout.Button("Apply groups and items from database to NPC"))
        {
            myScript.ApplyDatabaseNpcTradingToNPC();
        }

        EditorGUILayout.LabelField("Fighting NPC Lists", EditorStyles.boldLabel);
        if (GUILayout.Button("Count fighting NPC"))
        {
            myScript.CountFightingNPC();
        }
        if (GUILayout.Button("Save all fighting NPC to FightingNPCList.csv"))
        {
            myScript.SaveListFightingNPC();
        }
        if (GUILayout.Button("Load FightingNPCList.csv and update fighting NPC"))
        {
            myScript.LoadListFightingNPC();
        }

        EditorGUILayout.LabelField("Spells", EditorStyles.boldLabel);
        if (GUILayout.Button("Save all spells to SpellList.csv"))
        {
            myScript.SaveSpellList();
        }
        DrawDefaultInspector();
    }
}
#endif