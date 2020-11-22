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
using System;

public class CreateStaticElement : MonoBehaviour
{
    public enum SubFolder { StaticElements }; // no semistatic elements

    [Header("Basic Information")]
    public GameObject prefabElement;
    public SubFolder subFolder;

    [Header("Item Specific")]
    public ScriptableItem item;
    public string targetElementName = "";
    public string specialName = "";
    [Tooltip("Life time in seconds\n0 for never decay\n-1 use default value")]
    public int liveTimeInSeconds = -1;
    [Header("Item SpecialProperties")]
    [Tooltip("Valid for the whole block")]
    public bool useSpecialProperties = false;
    public int itemData1 = 0;
    public int itemData2 = 0;
    public int itemData3 = 0;
    [Range(0f, 100)] public int itemDurability = GlobalVar.defaultDurability;
    [Range(0f, 100)] public int itemQuality = GlobalVar.defaultQuality;
    [Header("Rotation and Size")]
    [Range(0.01f, 10f)] public float creationSize = 1f;
    [Tooltip("Rotate around vertical axix")]
    public bool creationRotationY = false;
    [Tooltip("Roling upside down")]
    public bool creationRotationXZ = false;
    [Header("Editor Behaviour")]
    public bool stayInEditor = false;

    public void CreateElement(Vector3 position, Quaternion rotation = new Quaternion())
    {
        ScriptableItem itemData;
        if (ScriptableItem.dict.TryGetValue(item.name.GetStableHashCode(), out itemData))
        {
            GameObject element = Instantiate(prefabElement, position, rotation);
            Vector3 creationScale = new Vector3(creationSize, creationSize, creationSize);
            element.transform.localScale = creationScale;
            if (creationRotationY)
            {
                element.transform.Rotate(0, UnityEngine.Random.value * 360f, 0);
            }
            if (creationRotationXZ)
            {
                element.transform.Rotate(UnityEngine.Random.value * 360f, 0, UnityEngine.Random.value * 360f);
            }
            element.transform.SetParent(GameObject.Find("Areas/" + Universal.GetArea(position) + "/" + Enum.GetName(typeof(SubFolder), subFolder)).transform);
            if (targetElementName.Length > 0)
                element.name = targetElementName;

            Item itemToCreate = new Item(itemData);
            ElementSlot es = element.GetComponent<ElementSlot>();
            es.item = itemToCreate;
            es.isStatic = true;
            es.applyToGround = false;
            es.specialName = specialName;
            es.item.data.displayName = specialName;

            UsableItem usableItem = (UsableItem)itemToCreate.data;
            if (liveTimeInSeconds < 0)
            {
                liveTimeInSeconds = usableItem.decayTime;
            }
            if (liveTimeInSeconds > 0)
            {
                es.decayAfter = GameTime.SecondsSinceZero() + liveTimeInSeconds;
            }
            if (useSpecialProperties)
            {
                es.SetItemData(itemData1, itemData2, itemData3);
                es.SetItemDurability(itemDurability);
                es.SetItemQuality(itemQuality);
            }



            GameObject model;
            if (usableItem.modelPrefab)
            {
                model = Instantiate(usableItem.modelPrefab, element.transform);
                model.transform.SetParent(element.transform, true);
                model.name = "Model";
            }
            Debug.Log(string.Format("Item {0} created", element.name));
            if (!stayInEditor)
            {
                Selection.activeGameObject = element;
            }
        }
    }
}
#endif