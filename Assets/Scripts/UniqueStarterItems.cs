/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System;
using System.Collections.Generic;
public class UniqueStarterItems
{
    private char seperator = '#';
    public int count
    {
        get { return listOfItems.Count; }
    }

    public struct UniqueStarterItem
    {
        public int identifier;
        public string name;
        public string headline;
        public string description;
        public string itemName;
        public string itemDescription;
    }
    public List<UniqueStarterItem> listOfItems = new List<UniqueStarterItem>();


    public UniqueStarterItems()
    {
        listOfItems.Add(new UniqueStarterItem
        {
            identifier = 1,
            itemName = "",
            itemDescription = "",
            name = "Parchment",
            headline = "Parchment isn't important. It's the words on them that are important.",
            description = "This could be any  document important fpr your character." + Environment.NewLine + Environment.NewLine
            + "Think about a certificate for excellent service as maid, apprentice or mercenary." + Environment.NewLine
            + "Think about a childrens dawing, love letter or last will." + Environment.NewLine
            + "Think about cut off from a book with a hint about an unknown spell."
        });
        listOfItems.Add(new UniqueStarterItem
        {
            identifier = 2,
            itemName = "",
            itemDescription = "",
            name = "Carved piece of wood",
            headline = "Memories are what warm you up from the inside.",
            description = "This could be any gift you got in the past." + Environment.NewLine + Environment.NewLine
            + "Think about a toy you got from your parents." + Environment.NewLine
            + "Think about a nippes you saved from your burned down home." + Environment.NewLine
            + "Think about a souvenir you fond on one of your journeys."
        });
        listOfItems.Add(new UniqueStarterItem
        {
            identifier = 3,
            itemName = "",
            itemDescription = "",
            name = "Simple engraved dagger",
            headline = "Friends are the family you choose.",
            description = "This could be any reward you got in the past." + Environment.NewLine + Environment.NewLine
            + "Think about a ceremonial dagger recieved at the end of an apprenticeship." + Environment.NewLine
            + "Think about a engraved gift you got from your company of mercenaries." + Environment.NewLine
            + "Think about a leftover from your fathers time as soldier."
        });
        listOfItems.Add(new UniqueStarterItem
        {
            identifier = 4,
            itemName = "",
            itemDescription = "",
            name = "Simple engraved ring",
            headline = "Rings try to find their way back to their owner.",
            description = "This could be a membership sign." + Environment.NewLine + Environment.NewLine
            + "Think about a wedding ring." + Environment.NewLine
            + "Think about a religious order." + Environment.NewLine
            + "Think about a leftover from your parents."
        });
    }

    public string headline
    {
        get { return "Your memories are your jewels!"; }
    }

    public string description
    {
        get
        {
            return "The game is primarily about role play. These unique items shall help to define a history of your character." + Environment.NewLine
                + "You can create up to 3 unique items. You do not need such unique items, but be aware that there are quests associated with these items." + Environment.NewLine
                + "The items can only be sold for a few pieces of copper and are barely worth the storage capacity if you only consider the monetary value." + Environment.NewLine + Environment.NewLine
                + "<b>The limit is your imagination.</b>";
        }
    }

    public string Serialize(List<UniqueStarterItem> itemList)
    {
        string result = "";
        foreach (UniqueStarterItem item in itemList)
        {
            item.itemName.Replace(seperator, '_');
            item.itemDescription.Replace(seperator, '_');
            result += string.Format("{0}{1}{0}{2}{0}{3}{0}", seperator
                    , item.identifier
                    , item.itemName
                    , item.itemDescription);
        }
        return result;
    }
    public List<UniqueStarterItem> Deserialize(string text)
    {
        List<UniqueStarterItem> itemList = new List<UniqueStarterItem>();
        string[] splitted = text.Split(seperator);
        for (int i = 0; i < splitted.Length - 1; i = i + 3)
        {
            UniqueStarterItem item = new UniqueStarterItem();
            item.identifier = splitted[0].ToInt();
            item.itemName = splitted[1];
            item.itemDescription = splitted[2];
            itemList.Add(item);
        }
        return itemList;
    }
}
