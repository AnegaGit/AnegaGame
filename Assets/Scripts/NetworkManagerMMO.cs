/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// We use a custom NetworkManager that also takes care of login, character
// selection, character creation and more.
//
// We don't use the playerPrefab, instead all available player classes should be
// dragged into the spawnable objects property.
//
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using Mirror;
#if UNITY_EDITOR
using UnityEditor;
#endif
// we need a clearly defined state to know if we are offline/in world/in lobby
// otherwise UICharacterSelection etc. never know 100% if they should be visible
// or not.
public enum NetworkState { Offline, Handshake, Lobby, World }
public partial class NetworkManagerMMO : NetworkManager
{
    // current network manager state on client
    public NetworkState state = NetworkState.Offline;
    // <conn, account> dict for the lobby
    // (people that are still creating or selecting characters)
    Dictionary<NetworkConnection, string> lobby = new Dictionary<NetworkConnection, string>();
    // UI components to avoid FindObjectOfType
    [Header("UI")]
    public UIPopup uiPopup;
    // login info for the local player
    // we don't just name it 'account' to avoid collisions in handshake
    [Header("Login")]
    public string loginAccount = "";
    public string loginPassword = "";
    // we may want to add another game server if the first one gets too crowded.
    // the server list allows people to choose a server.
    //
    // note: we use one port for all servers, so that a headless server knows
    // which port to bind to. otherwise it would have to know which one to
    // choose from the list, which is far too complicated. one port for all
    // servers will do just fine for an Indie MMORPG.
    [Serializable]
    public class ServerInfo
    {
        public string name;
        public string ip;
    }
    public List<ServerInfo> serverList = new List<ServerInfo>() {
        new ServerInfo{name="Local", ip="localhost"}
    };
    [Header("Character Selection")]
    public int selection = -1;
    public Transform[] selectionLocations;
    public Transform selectionCameraLocation;
    [Header("Database")]
    public int characterLimit = 4;
    public int characterNameMaxLength = 16;
    public int accountMaxLength = 16;
    public float saveInterval = 60f; // in seconds
    // store characters available message on client so that UI can access it
    [HideInInspector] public CharactersAvailableMsg charactersAvailableMsg;
    // name checks /////////////////////////////////////////////////////////////
    public bool IsAllowedAccountName(string account)
    {
        // not too long?
        // only contains letters, number and underscore and not empty (+)?
        // (important for database safety etc.)
        return account.Length <= accountMaxLength &&
               Regex.IsMatch(account, @"^[a-zA-Z0-9_]+$");
    }
    // events //////////////////////////////////////////////////////////////////
    void Start()
    {
        // empty
    }
    void Update()
    {
        // any valid local player? then set state to world
        if (ClientScene.localPlayer != null)
            state = NetworkState.World;
    }
    // client popup messages ///////////////////////////////////////////////////
    void ClientSendPopup(NetworkConnection conn, string error, bool disconnect)
    {
        ErrorMsg message = new ErrorMsg { text = error, causesDisconnect = disconnect };
        conn.Send(ErrorMsg.MsgId, message);
    }
    void OnClientReceivePopup(NetworkMessage netMsg)
    {
        ErrorMsg message = netMsg.ReadMessage<ErrorMsg>();
        print("OnClientReceivePopup: " + message.text);
        // show a popup
        uiPopup.Show(message.text);
        // disconnect if it was an important network error
        // (this is needed because the login failure message doesn't disconnect
        //  the client immediately (only after timeout))
        if (message.causesDisconnect)
        {
            netMsg.conn.Disconnect();
            // also stop the host if running as host
            // (host shouldn't start server but disconnect client for invalid
            //  login, which would be pointless)
            if (NetworkServer.active) StopHost();
        }
    }
    // start & stop ////////////////////////////////////////////////////////////
    public override void OnStartServer()
    {
        // handshake packet handlers (in OnStartServer so that reconnecting works)
        NetworkServer.RegisterHandler(LoginMsg.MsgId, OnServerLogin);
        NetworkServer.RegisterHandler(CharacterCreateMsg.MsgId, OnServerCharacterCreate);
        NetworkServer.RegisterHandler(CharacterDeleteMsg.MsgId, OnServerCharacterDelete);
        NetworkServer.RegisterHandler(CharacterVerifyMsg.MsgId, OnServerCharacterVerify);
#if !UNITY_EDITOR
        // server only? not host mode?
        if (!NetworkClient.active)
        {
            // set a fixed tick rate instead of updating as often as possible
            // -> updating more than 50x/s is just a waste of CPU power that can
            //    be used by other threads like network transport instead
            // -> note: doesn't work in the editor
            Application.targetFrameRate = Mathf.RoundToInt(1f / Time.fixedDeltaTime);
            print("server tick rate set to: " + Application.targetFrameRate + " (1 / Edit->Project Settings->Time->Fixed Time Step)");
        }
#endif
        // invoke saving
        InvokeRepeating("SavePlayers", saveInterval, saveInterval);
        // call base function to guarantee proper functionality
        base.OnStartServer();
    }
    public override void OnStopServer()
    {
        print("OnStopServer");
        CancelInvoke("SavePlayers");
        // call base function to guarantee proper functionality
        base.OnStopServer();
    }
    // handshake: login ////////////////////////////////////////////////////////
    public bool IsConnecting()
    {
        return NetworkClient.active && !ClientScene.ready;
    }
    public override void OnClientConnect(NetworkConnection conn)
    {
        LogFile.WriteLog(LogFile.LogLevel.Server, string.Format("Try to connect from {0}", loginAccount));
        // setup handlers
        client.RegisterHandler(CharactersAvailableMsg.MsgId, OnClientCharactersAvailable);
        client.RegisterHandler(ErrorMsg.MsgId, OnClientReceivePopup);
        // send login packet with hashed password, so that the original one
        // never leaves the player's computer.
        //
        // it's recommended to use a different salt for each hash. ideally we
        // would store each user's salt in the database. to not overcomplicate
        // things, we will use the account name as salt (at least 16 bytes)
        //
        // Application.version can be modified under:
        // Edit -> Project Settings -> Player -> Bundle Version
        string hash = Utils.PBKDF2Hash(loginPassword, "at_least_16_byte" + loginAccount);
        LoginMsg message = new LoginMsg { account = loginAccount, password = hash, version = Application.version };
        conn.Send(LoginMsg.MsgId, message);
        //Debug.Log(">>> login message was sent");
        // set state
        state = NetworkState.Handshake;
        // call base function to make sure that client becomes "ready"
        //base.OnClientConnect(conn);
        ClientScene.Ready(conn); // from bitbucket OnClientConnect source
    }
    // the default OnClientSceneChanged sets the client as ready automatically,
    // which makes no sense for MMORPG situations. this was more for situations
    // where the server tells all clients to load a new scene.
    // -> setting client as ready will cause 'already set as ready' errors if
    //    we call StartClient before loading a new scene (e.g. for zones)
    // -> it's best to just overwrite this with an empty function
    public override void OnClientSceneChanged(NetworkConnection conn) { }
    bool AccountLoggedIn(string account)
    {
        // in lobby or in world?
        return lobby.ContainsValue(account) ||
               Player.onlinePlayers.Values.Any(p => p.account == account);
    }
    // helper function to make a CharactersAvailableMsg from all characters in
    // an account
    CharactersAvailableMsg MakeCharactersAvailableMessage(string account)
    {
        // load from database
        List<Player> characters = Database.CharactersForAccount(account)
                                    .Select(character => Database.CharacterLoad(character, GetPlayerClasses()))
                                    .Select(go => go.GetComponent<Player>())
                                    .ToList();
        // construct the message
        CharactersAvailableMsg message = new CharactersAvailableMsg();
        message.Load(characters);
        // destroy the temporary players again and return the result
        characters.ForEach(player => Destroy(player.gameObject));
        return message;
    }
    void OnServerLogin(NetworkMessage netMsg)
    {
        //print("OnServerLogin " + netMsg.conn);
        LoginMsg message = netMsg.ReadMessage<LoginMsg>();
        // correct version?
        if (message.version == Application.version)
        {
            // allowed account name?
            if (IsAllowedAccountName(message.account))
            {
                // validate account info
                if (Database.IsValidAccount(message.account, message.password))
                {
                    // not in lobby and not in world yet?
                    if (!AccountLoggedIn(message.account))
                    {
                        LogFile.WriteLog(LogFile.LogLevel.Server, string.Format("Login successful: {0}", message.account));
                        // add to logged in accounts
                        lobby[netMsg.conn] = message.account;
                        // send necessary data to client
                        CharactersAvailableMsg reply = MakeCharactersAvailableMessage(message.account);
                        netMsg.conn.Send(CharactersAvailableMsg.MsgId, reply);
                    }
                    else
                    {
                        LogFile.WriteLog(LogFile.LogLevel.Server, string.Format("Account already logged in: {0}", message.account));
                        ClientSendPopup(netMsg.conn, "already logged in", true);
                        // note: we should disconnect the client here, but we can't as
                        // long as unity has no "SendAllAndThenDisconnect" function,
                        // because then the error message would never be sent.
                        //netMsg.conn.Disconnect();
                    }
                }
                else
                {
                    LogFile.WriteLog(LogFile.LogLevel.Server, string.Format("Invalid account or password for: {0}", message.account));
                    ClientSendPopup(netMsg.conn, "invalid account", true);
                }
            }
            else
            {
                LogFile.WriteLog(LogFile.LogLevel.Server, string.Format("Account name not allowed: {0}", message.account));
                ClientSendPopup(netMsg.conn, "account name not allowed", true);
            }
        }
        else
        {
            LogFile.WriteLog(LogFile.LogLevel.Server, string.Format("Version mismatch: {0} expected:{1} received: {2}", message.account, Application.version, message.version));
            ClientSendPopup(netMsg.conn, "outdated version", true);
        }
    }
    // handshake: character selection //////////////////////////////////////////
    Player LoadPreview(GameObject prefab, Transform location, int selectionIndex, CharactersAvailableMsg.CharacterPreview character)
    {
        // instantiate the prefab
        GameObject preview = Instantiate(prefab.gameObject, location.position, location.rotation);
        preview.transform.parent = location;
        Player player = preview.GetComponent<Player>();
        // assign basic preview values like name and equipment
        player.name = character.name;
        player.displayName = character.displayName;
        player.apperanceSync = character.appreanceSync;
        player.InitializeCharacter();

        foreach (ItemSlot itemSlot in character.inventory)
        {
            if (itemSlot.amount > 0 && itemSlot.container == GlobalVar.containerEquipment)
            {
                player.inventory.Add(itemSlot);
            }
        }
        // add selection script
        preview.AddComponent<SelectableCharacter>();
        preview.GetComponent<SelectableCharacter>().index = selectionIndex;
        return player;
    }

    public void ClearPreviews()
    {
        selection = -1;
        foreach (Transform location in selectionLocations)
            if (location.childCount > 0)
                Destroy(location.GetChild(0).gameObject);
    }
    void OnClientCharactersAvailable(NetworkMessage netMsg)
    {
        charactersAvailableMsg = netMsg.ReadMessage<CharactersAvailableMsg>();
        //        print("characters available:" + charactersAvailableMsg.characters.Length);
        // set state
        state = NetworkState.Lobby;
        // clear previous previews in any case
        ClearPreviews();
        // load previews for 3D character selection
        for (int i = 0; i < charactersAvailableMsg.characters.Length; ++i)
        {
            CharactersAvailableMsg.CharacterPreview character = charactersAvailableMsg.characters[i];
            // find the prefab for that class
            Player prefab = GetPlayerClasses().Find(p => p.name == character.className);
            if (prefab != null)
            {
                Player previewPlayer = LoadPreview(prefab.gameObject, selectionLocations[i], i, character);
            }
            else
                Debug.LogError("Character Selection: no prefab found for class " + character.className);
        }
    }
    // called after the client calls ClientScene.AddPlayer with a msg parameter
    public override void OnServerAddPlayer(NetworkConnection conn, NetworkMessage extraMsg)
    {
        //print("OnServerAddPlayer extra");
        if (extraMsg != null)
        {
            // only while in lobby (aka after handshake and not ingame)
            if (lobby.ContainsKey(conn))
            {
                // read the index and find the n-th character
                // (only if we know that he is not ingame, otherwise lobby has
                //  no netMsg.conn key)
                CharacterSelectMsg message = extraMsg.ReadMessage<CharacterSelectMsg>();
                string account = lobby[conn];
                List<int> characters = Database.CharactersForAccount(account);
                // validate index
                if (0 <= message.index && message.index < characters.Count)
                {
                    LogFile.WriteLog(LogFile.LogLevel.Server, string.Format("{0} selected character {1}", account, characters[message.index]));
                    // load character data
                    GameObject go = Database.CharacterLoad(characters[message.index], GetPlayerClasses());
                    // add to client
                    NetworkServer.AddPlayerForConnection(conn, go);
                    // remove from lobby
                    lobby.Remove(conn);
                }
                else
                {
                    LogFile.WriteLog(LogFile.LogLevel.Server, string.Format("Character selection: invalid character index: {0} {1}", account, message.index));
                    ClientSendPopup(conn, "invalid character index", false);
                }
            }
            else
            {
                LogFile.WriteLog(LogFile.LogLevel.Server, string.Format("Character selection: AddPlayer: not in lobby {0}", conn));
                ClientSendPopup(conn, "AddPlayer: not in lobby", true);
            }
        }
        else
        {
            print("missing extraMessageReader");
            ClientSendPopup(conn, "missing parameter", true);
        }
    }
    // handshake: character creation ///////////////////////////////////////////
    // find all available player classes
    public List<Player> GetPlayerClasses()
    {
        List<Player> playerClasses = new List<Player>();
        foreach (GameObject go in spawnPrefabs)
        {
            if (go.GetComponent<Player>() != null)
            {
                playerClasses.Add(go.GetComponent<Player>());
            }
        }
        return playerClasses;
    }
    void OnServerCharacterVerify(NetworkMessage netMsg)
    {
        CharacterVerifyMsg message = netMsg.ReadMessage<CharacterVerifyMsg>();
        string messageText = "";


        // only while in lobby (aka after handshake and not ingame)
        if (lobby.ContainsKey(netMsg.conn))
        {
            // allowed character name?
            if (GlobalFunc.IsAllowedDisplayedName(message.displayedName))
            {
                // not existant yet?
                string account = lobby[netMsg.conn];
                if (Database.DisplayNameExists(message.displayedName))
                {
                    if (messageText.Length > 0)
                        messageText += Environment.NewLine;
                    messageText += "Displayed name already exists";
                }
            }
            if (messageText.Length == 0)
            {
                messageText += "Name is available";
            }
            ClientSendPopup(netMsg.conn, messageText, false);

        }
        else
        {
            print("CharacterCreate: not in lobby");
            ClientSendPopup(netMsg.conn, "CharacterCreate: not in lobby", true);
        }
    }
    void OnServerCharacterCreate(NetworkMessage netMsg)
    {
        //print("OnServerCharacterCreate " + netMsg.conn);
        CharacterCreateMsg message = netMsg.ReadMessage<CharacterCreateMsg>();
        // only while in lobby (aka after handshake and not ingame)
        if (lobby.ContainsKey(netMsg.conn))
        {
            // allowed character name?
            if (GlobalFunc.IsAllowedDisplayedName(message.displayedName))
            {
                string account = lobby[netMsg.conn];
                // not too may characters created yet?
                if (Database.CharactersForAccount(account).Count < characterLimit)
                {
                    // valid class index?
                    List<Player> classes = GetPlayerClasses();
                    if (0 <= message.classIndex && message.classIndex < classes.Count)
                    {
                        // create new character based on the prefab.
                        // -> we also assign default items and equipment for new characters
                        // -> spells are handled in Database.CharacterLoad every time. if we
                        //    add new ones to a prefab, all existing players should get them
                        // (instantiate temporary player)
                        print("creating character: " + message.classIndex);
                        Player prefab = GameObject.Instantiate(classes[message.classIndex]).GetComponent<Player>();
                        prefab.name = "playertmp";
                        prefab.displayName = message.displayedName;
                        prefab.account = account;
                        prefab.className = classes[message.classIndex].name;
                        prefab.transform.position = Universal.NewPlayerSpawn.position;

                        // manage skills
                        int[] defaultSkills = Skills.DeserializeDefaultSkills(message.skills);
                        for (int i = 0; i < defaultSkills.Length; i++)
                        {
                            if (defaultSkills[i] > 0)
                            {
                                int experience = 0;
                                if (defaultSkills[i] == 1)
                                    experience = GlobalVar.skillDefaultLowExp;
                                else if (defaultSkills[i] == 2)
                                    experience = GlobalVar.skillDefaultMediumExp;
                                else if (defaultSkills[i] == 3)
                                    experience = GlobalVar.skillDefaultHighExp;
                                prefab.skills.AddExperience(i, experience);
                            }
                        }

                        // manage start equipment
                        StarterSets starterSets = new StarterSets();
                        starterSets.Reload();
                        StarterSet selectedSet = starterSets.listOfStarterSets.Find(x => x.identifier == message.starterSet);
                        int backpackPosition = 0;
                        foreach (StarterSetDefaultItem defaultItem in selectedSet.defaultItems)
                        {
                            if (GlobalFunc.CreateNewItem(defaultItem.item.name, 0, 0, defaultItem.durability, defaultItem.quality, "", out Item newItem))
                            {
                                if (defaultItem.position < 0)
                                    prefab.inventory.AddItem(newItem, prefab.ContainerIdOfBackpack(), backpackPosition++, defaultItem.amount);
                                else
                                {
                                    if (defaultItem.position == GlobalVar.equipmentBackpack && newItem.data is ContainerItem)
                                    {
                                        ContainerItem containerItem = (ContainerItem)newItem.data;
                                        int containerId = prefab.AddNewMobileContainer(containerItem.minSlots, containerItem.minContainer, defaultItem.item.name, "", true);
                                        newItem.data1 = containerId;
                                    }
                                    prefab.inventory.AddItem(newItem, GlobalVar.containerEquipment, defaultItem.position, defaultItem.amount);
                                }
                            }
                            else
                                LogFile.WriteLog(LogFile.LogLevel.Error, "Default item '" + defaultItem.item.name + "' doesn't exists. Verify StarterSets.");
                        }
                        Money.AddToInventory(prefab, selectedSet.money);
                        prefab.health = prefab.healthMax; // after equipment in case of boni
                        prefab.mana = prefab.manaMax; // after equipment in case of boni
                        prefab.attributes.CreateFromString(message.attributes);
                        prefab.abilities.CreateFromString(message.abilities);
                        prefab.apperance.ConvertFromString(message.apperance);
                        prefab.skillTotalTime = GlobalVar.skillTotalTimeStart;

                        if (!GlobalVar.isProduction && GlobalVar.makeGmAutomatically)
                        {
                            prefab.gmState = GameMaster.typeGod;
                        }

                        // create the player and receive id
                        int charid = Database.CharacterCreate(prefab);
                        prefab.name = Player.NameFromId(charid);
                        // save the player
                        Database.CharacterSave(prefab, false);
                        GameObject.Destroy(prefab.gameObject);

                        // send available characters list again, causing
                        // the client to switch to the character
                        // selection scene again
                        CharactersAvailableMsg reply = MakeCharactersAvailableMessage(account);
                        netMsg.conn.Send(CharactersAvailableMsg.MsgId, reply);
                    }
                    else
                    {
                        print("character invalid class: " + message.classIndex);
                        ClientSendPopup(netMsg.conn, "character invalid class", false);
                    }
                }
                else
                {
                    print("character limit reached: " + account);
                    ClientSendPopup(netMsg.conn, "character limit reached", false);
                }
            }
            else
            {
                print("character name not allowed: " + message.displayedName);
                ClientSendPopup(netMsg.conn, "character name not allowed", false);
            }
        }
        else
        {
            print("CharacterCreate: not in lobby");
            ClientSendPopup(netMsg.conn, "CharacterCreate: not in lobby", true);
        }
    }
    void OnServerCharacterDelete(NetworkMessage netMsg)
    {
        //print("OnServerCharacterDelete " + netMsg.conn);
        CharacterDeleteMsg message = netMsg.ReadMessage<CharacterDeleteMsg>();
        // only while in lobby (aka after handshake and not ingame)
        if (lobby.ContainsKey(netMsg.conn))
        {
            string account = lobby[netMsg.conn];
            List<int> characters = Database.CharactersForAccount(account);
            // validate index
            if (0 <= message.index && message.index < characters.Count)
            {
                // delete the character
                print("delete character: " + characters[message.index]);
                Database.CharacterDelete(characters[message.index]);
                // send the new character list to client
                characters = Database.CharactersForAccount(account);
                CharactersAvailableMsg reply = MakeCharactersAvailableMessage(account);
                netMsg.conn.Send(CharactersAvailableMsg.MsgId, reply);
            }
            else
            {
                print("invalid character index: " + account + " " + message.index);
                ClientSendPopup(netMsg.conn, "invalid character index", false);
            }
        }
        else
        {
            print("CharacterDelete: not in lobby: " + netMsg.conn);
            ClientSendPopup(netMsg.conn, "CharacterDelete: not in lobby", true);
        }
    }
    // player saving ///////////////////////////////////////////////////////////
    // we have to save all players at once to make sure that item trading is
    // perfectly save. if we would invoke a save function every few minutes on
    // each player seperately then it could happen that two players trade items
    // and only one of them is saved before a server crash - hence causing item
    // duplicates.
    void SavePlayers()
    {
        List<Player> players = Player.onlinePlayers.Values.ToList();
        Database.CharacterSaveMany(players);
        if (players.Count > 0) LogFile.WriteLog(LogFile.LogLevel.Server, string.Format("{0} players saved to database", players.Count));
    }
    // stop/disconnect /////////////////////////////////////////////////////////
    // called on the server when a client disconnects
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        //print("OnServerDisconnect " + conn);
        // save player (if any)
        if (conn.playerController != null)
        {
            Database.CharacterSave(conn.playerController.GetComponent<Player>(), false);
            print("saved:" + conn.playerController.name);
        }
        else print("no player to save for: " + conn);
        // remove logged in account after everything else was done
        lobby.Remove(conn); // just returns false if not found
        // do base function logic (removes the player for the connection)
        base.OnServerDisconnect(conn);
    }
    // called on the client if he disconnects
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        // show a popup so that users know what happened
        uiPopup.Show("Disconnected.");
        // call base function to guarantee proper functionality
        base.OnClientDisconnect(conn);
        // call StopClient to clean everything up properly (otherwise
        // NetworkClient.active remains false after next login)
        StopClient();
        // set state
        state = NetworkState.Offline;
    }
    // universal quit function for editor & build
    public static void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    // called when quitting the application by closing the window / pressing
    // stop in the editor
    // -> we want to send the quit packet to the server instead of waiting for a
    //    timeout
    // -> this also avoids the OnDisconnectError UNET bug (#838689) more often
    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        if (IsClientConnected())
        {
            LogFile.WriteLog(LogFile.LogLevel.Always, "Client stopped!");
            StopClient();
        }
    }
    public override void OnValidate()
    {
        base.OnValidate();
        // ip has to be changed in the server list. make it obvious to users.
        if (!Application.isPlaying && networkAddress != "")
            networkAddress = "Use the Server List below!";
        // need enough character selection locations for character limit
        if (selectionLocations.Length != characterLimit)
        {
            // create new array with proper size
            Transform[] newArray = new Transform[characterLimit];
            // copy old values
            for (int i = 0; i < Mathf.Min(characterLimit, selectionLocations.Length); ++i)
                newArray[i] = selectionLocations[i];
            // use new array
            selectionLocations = newArray;
        }
    }
}
