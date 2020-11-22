/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Contains all the network messages that we need.
using System.Collections.Generic;
using System.Linq;
using Mirror;
// client to server ////////////////////////////////////////////////////////////
public partial class LoginMsg : MessageBase
{
    public static short MsgId = 1000;
    public string account;
    public string password;
    public string version;
}
public partial class CharacterSelectMsg : MessageBase
{
    public static short MsgId = 1001;
    public int index;
}
public partial class CharacterDeleteMsg : MessageBase
{
    public static short MsgId = 1002;
    public int index;
}
public partial class CharacterCreateMsg : MessageBase
{
    public static short MsgId = 1003;
    public string displayedName;
    public int classIndex;
    public string attributes;
    public string abilities;
    public int starterSet;
    public string skills;
    public string uniqueItems;
    public string apperance;
}
public partial class CharacterVerifyMsg : MessageBase
{
    public static short MsgId = 1004;
    public string displayedName;
}
// server to client ////////////////////////////////////////////////////////////
// we need an error msg packet because we can't use TargetRpc with the Network-
// Manager, since it's not a MonoBehaviour.
public partial class ErrorMsg : MessageBase
{
    public static short MsgId = 2000;
    public string text;
    public bool causesDisconnect;
}
public partial class CharactersAvailableMsg : MessageBase
{
    public static short MsgId = 2001;
    public partial struct CharacterPreview
    {
        public string name;
        public string className; // = the prefab name
        public string displayName;
        public string appreanceSync;
        public ItemSlot[] inventory;
    }
    public CharacterPreview[] characters;
    // load method in this class so we can still modify the characters structs
    public void Load(List<Player> players)
    {
        // we only need name, class, equipment for our UI
        characters = players.Select(
            player => new CharacterPreview {
                name = player.name,
                className = player.className,
                displayName = player.displayName,
                appreanceSync = player.apperanceSync,
                inventory = player.inventory.AllInContainer(GlobalVar.containerEquipment).ToArray()
            }
        ).ToArray();
    }
}