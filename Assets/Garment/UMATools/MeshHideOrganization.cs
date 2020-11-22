/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UMA;

[System.Serializable]
public struct BasicSlot
{
    [Tooltip("Group name")]
    public string name;
    [Tooltip("MeshHide assets valid for each recipe in this group.")]
    public List<UMA.MeshHideAsset> meshHides;
    [Tooltip("Slots to hide for each recipe in this group.")]
    public List<UMA.SlotDataAsset> slotsToHide;
    [Tooltip("Recipes of the group.\nA recipe can be part of multiple groups.")]
    public List<UMA.UMATextRecipe> recipes;
    [Tooltip("Recipes are parts of basic groups already.")]
    public bool specialGroup;
}


public struct TmpUMAOverlay
{
    public string id;
}

public class TmpUMASlot
{
    public string id;
    public TmpUMAOverlay[] overlays;
}

public class TmpUMARecipe
{
    public string version;
    public TmpUMASlot[] slotsV3;
}

public class MeshHideOrganization : MonoBehaviour
{
    public List<BasicSlot> basicSlots;
    [Tooltip("Remove only MeshHide assets, used in this list.\n\nAlready applied MeshHide assets will be removed before applying this list!")]
    public bool RemoveShownMeshHideAssetsOnly;

    public void ReorganizeMeshHide()
    {
        int countGroup = 0;
        int countRecipe = 0;
        int countMeshHide = 0;
        int countHideSlot = 0;
        int countError = 0;

        List<string> usedMeshHides;
        List<string> usedSlotHides;

        usedMeshHides = new List<string>();
        usedSlotHides = new List<string>();

        ClearLogConsole();

        //Check for error and build MeshHide Asset list and Slot list for removal if necessary
        foreach (BasicSlot basicSlot in basicSlots)
        {
            foreach (MeshHideAsset meshHide in basicSlot.meshHides)
            {
                if (!meshHide)
                {
                    countError++;
                    Debug.Log(string.Format("There is a not assigned MeshHide Asset in group '{0}'!", basicSlot.name));
                }
                else if (RemoveShownMeshHideAssetsOnly)
                {
                    if (!usedMeshHides.Contains(meshHide.name))
                    {
                        usedMeshHides.Add(meshHide.name);
                    }
                }
            }
            foreach (SlotDataAsset slotHide in basicSlot.slotsToHide)
            {
                if (!slotHide)
                {
                    countError++;
                    Debug.Log(string.Format("There is a not assigned Slot to hide in group '{0}'!", basicSlot.name));
                }
                else
                {
                    if (!usedSlotHides.Contains(slotHide.slotName))
                    {
                        usedSlotHides.Add(slotHide.slotName);
                    }
                }
            }
        }
        Debug.Log(string.Format("In total there are {0} different MeshHide assets and {1} different slots to hide.", usedMeshHides.Count, usedSlotHides.Count));

        //remove old unused MeshHide asstes and Slots to hide from all used recipes
        foreach (BasicSlot basicSlot in basicSlots)
        {
            foreach (UMATextRecipe recipe in basicSlot.recipes)
            {
                if (!recipe)
                {
                    countError++;
                    Debug.Log(string.Format("There is a not assigned recipe in slot '{0}'!", basicSlot.name));
                }
                else
                {
                    //remove MeshHide assets
                    if (RemoveShownMeshHideAssetsOnly)
                    {
                        foreach (MeshHideAsset existingMeshHide in recipe.MeshHideAssets.ToArray())
                        {
                            if (existingMeshHide)
                            {
                                if (usedMeshHides.Contains(existingMeshHide.name))
                                {
                                    recipe.MeshHideAssets.Remove(existingMeshHide);
                                }
                            }
                            else
                            {
                                Debug.Log(string.Format("Removed an empty meshHide in recipe '{0}'!", recipe.name));
                                recipe.MeshHideAssets.Remove(existingMeshHide);
                            }

                        }
                    }
                    else
                    {
                        recipe.MeshHideAssets.Clear();
                    }

                    //remove Slots to hide
                    foreach (string existingSlotName in recipe.Hides.ToArray())
                    {
                        if (usedSlotHides.Contains(existingSlotName))
                        {
                            recipe.Hides.Remove(existingSlotName);
                        }
                    }

                    // set recipe to be changed in Unity
                    EditorUtility.SetDirty(recipe);
                }
            }
        }

        //add MeshHide asstes and slots to hide for all used recipes
        foreach (BasicSlot basicSlot in basicSlots)
        {
            countGroup++;
            foreach (UMATextRecipe recipe in basicSlot.recipes)
            {
                if (recipe) // protect empty, error message was earlier
                {
                    countRecipe++;

                    // Add MeshHide Asset from list to recipe
                    foreach (MeshHideAsset meshHide in basicSlot.meshHides)
                    {
                        if (meshHide) // protect empty, error message was earlier
                        {
                            //if (!recipe.MeshHideAssets.Contains(meshHide))
                            //{
                            countMeshHide++;
                            recipe.MeshHideAssets.Add(meshHide);
                            //}
                        }
                    }

                    // Add Slot to hide from list to recipe
                    foreach (SlotDataAsset hideSlot in basicSlot.slotsToHide)
                    {
                        if (!recipe.Hides.Contains(hideSlot.slotName))
                        {
                            countHideSlot++;
                            recipe.Hides.Add(hideSlot.slotName);
                        }
                    }
                    // set recipe to be changed in Unity
                    EditorUtility.SetDirty(recipe);
                }
            }
        }

        // Save all changed recipes
        AssetDatabase.SaveAssets();

        // End message
        if (countError > 0)
        {
            Debug.Log(string.Format("In {0} groups with {1} recipes {2} mesh hide assets and {3} slots to hide applied. List contains {4} errors.", countGroup, countRecipe, countMeshHide, countHideSlot, countError));
        }
        else
        {
            Debug.Log(string.Format("In {0} groups with {1} recipes {2} mesh hide assets and {3} slots to hide applied.", countGroup, countRecipe, countMeshHide, countHideSlot));
        }
    }

    public void CreateDocumentation()
    {
        string docuText = "====== Available Garment ======" + Environment.NewLine;
        int countRecipe = 0;
        List<string> usedRecipes = new List<string>();

        ClearLogConsole();

        docuText += "Tables contain femal names only.\\" + Environment.NewLine;
        docuText += "^slot^overlay^" + Environment.NewLine;
        docuText += "|replace last 'F' by 'M' for male|replace 'F_' by 'M_' for male|" + Environment.NewLine;

        foreach (BasicSlot basicSlot in basicSlots)
        {
            bool groupHeader = false;
            foreach (UMATextRecipe recipe in basicSlot.recipes)
            {
                if (!recipe)
                {
                    Debug.Log(string.Format("Error: There is a not assigned recipe in group {0}.", basicSlot.name));
                }
                else
                {
                    // do not document a recipe twice
                    if (!usedRecipes.Contains(recipe.name))
                    {
                        usedRecipes.Add(recipe.name);
                        // write only female recipes
                        if (recipe.name.Contains("F_Recipe"))
                        {
                            if (!groupHeader)
                            {
                                docuText += string.Format("==== {0} ====", basicSlot.name.Replace("Female", "")) + Environment.NewLine;
                                groupHeader = true;
                            }
                            countRecipe++;
                            docuText += string.Format("=== {0} ===", recipe.name.Replace("F_Recipe", "")) + Environment.NewLine;

                            TmpUMARecipe tmpRecipe = Newtonsoft.Json.JsonConvert.DeserializeObject<TmpUMARecipe>(recipe.recipeString);
                            foreach (TmpUMASlot tmpSlot in tmpRecipe.slotsV3)
                            {
                                bool firstOverlay = true;
                                foreach (TmpUMAOverlay tmpOverlay in tmpSlot.overlays)
                                {
                                    if (firstOverlay)
                                    {
                                        docuText += string.Format("|{0}|{1}|", tmpSlot.id, tmpOverlay.id) + Environment.NewLine;
                                        firstOverlay = false;
                                    }
                                    else
                                    {
                                        docuText += string.Format("|:::|{0}|", tmpOverlay.id) + Environment.NewLine;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        docuText += Environment.NewLine + string.Format("Total: {0} UMA garment pieces", countRecipe);
        Debug.Log("Copy the text from clipboard!" + Environment.NewLine + docuText);
        CopyToClipboard(docuText);

        // verify text errors and missing recipes
        foreach (string usedRecipe in usedRecipes)
        {
            if (usedRecipe.Contains("F_Recipe"))
            {
                if (!usedRecipes.Contains(usedRecipe.Replace("F_", "M_")))
                {
                    Debug.Log(string.Format("Missing: No male recipe for {0}", usedRecipe));
                }
            }
            else if (usedRecipe.Contains("M_Recipe"))
            {
                if (!usedRecipes.Contains(usedRecipe.Replace("M_", "F_")))
                {
                    Debug.Log(string.Format("Missing: No female recipe for {0}", usedRecipe));
                }
            }
            else
            {
                Debug.Log(string.Format("Wrong Naming: {0}", usedRecipe));
            }
        }

    }

    public void FindMissingFiles()
    {
        int coutMissing = 0;
        ClearLogConsole();

        //build list of used recipes
        List<string> usedRecipes = new List<string>();
        foreach (BasicSlot basicSlot in basicSlots)
        {
            foreach (UMATextRecipe recipe in basicSlot.recipes)
            {
                if (recipe)
                {
                    // do not document a recipe twice
                    if (!usedRecipes.Contains(recipe.name))
                    {
                        usedRecipes.Add(recipe.name);
                    }
                }
            }
        }


        string targetFilePath = GlobalVar.gameDevDir + "\\" + GlobalVar.garmentRecipesDir;
        string excluseFilesAndDirs = "\\_";

        DirectoryInfo dir = new DirectoryInfo(targetFilePath);
        FileInfo[] fileList = dir.GetFiles("*_Recipe.asset", SearchOption.AllDirectories);
        if (fileList.Length == 0)
        {
            //no files found, exit
            Debug.Log("No file found.");
            return;
        }

        foreach (FileInfo fileInList in fileList)
        {
            string fileInListName = fileInList.Name.Replace(".asset", "");
            if (!usedRecipes.Contains(fileInListName) && !fileInList.FullName.Contains(excluseFilesAndDirs))
            {
                coutMissing++;
                Debug.Log(String.Format("Missing recipe: {0}", fileInListName));
            }
        }
                Debug.Log(String.Format("Verification finished: {0} recipes foud which are tot taken into consideration.", coutMissing));

    }

    public void SortByName()
    {
        //sort the main list, special at the end
        basicSlots.Sort((x, y) => (x.specialGroup.ToString() + x.name).CompareTo(y.specialGroup.ToString() + y.name));
        //sort the  recipes
        foreach (BasicSlot basicSlot in basicSlots)
        {
            basicSlot.recipes.Sort((x, y) => x.name.CompareTo(y.name));
        }
        Debug.Log("List resorted");
    }

    /// <summary>
    /// Clear Debug log console
    /// </summary>
    /// <remarks>https://forum.unity.com/threads/solved-unity-2017-1-0f3-trouble-cleaning-console-via-code.484079/</remarks>
    static void ClearLogConsole()
    {
        var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");

        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

        clearMethod.Invoke(null, null);
    }

    /// <summary>
    /// Copy any text to clipboard
    /// </summary>
    /// <remarks>https://answers.unity.com/questions/1144378/copy-to-clipboard-with-a-button-unity-53-solution.html</remarks>
    static void CopyToClipboard(string textToClipboard)
    {
        TextEditor te = new TextEditor();
        te.text = textToClipboard;
        te.SelectAll();
        te.Copy();
    }
}
#endif