/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;
public class PanelUniqueItem : MonoBehaviour
{
    public Text explanationPanel;
    public string explanation = "";
    public UICharacterCreation characterCreation;
    public InputField inputName;
    public InputField inputDescription;
    public int id;
    private string itemType;
    public void OnPointerEnter()
    {
        explanationPanel.text = explanation;
    }
    public void ItemType(string type)
    {
        itemType = type;
        transform.Find("TextType").GetComponent<Text>().text = type + ":";
    }
    public string GetItemName()
    {
        return inputName.text;
    }
    public string GetItemDescription()
    {
        return inputDescription.text;
    }
    public void ItemDeleted()
    {
        characterCreation.UniqueItemRemoved(2);
        Destroy(gameObject);
    }
    public void ItemChanged()
    {
//        characterCreation.UniqueItemChanged();
    }
}
