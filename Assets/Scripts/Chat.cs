/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// We implemented a chat system that works directly with UNET. The chat supports
// different channels that can be used to communicate with other players:
//
// - **Local Chat:** by default, all messages that don't start with a **/** are
// addressed to the local chat. If one player writes a local message, then all
// players around him _(all observers)_ will be able to see the message.
// - **Whisper Chat:** a player can write a private message to another player by
// using the **/ name message** format.
// - **Guild Chat:** we implemented guild chat support with the **/g message**
// - **Info Chat:** the info chat can be used by the server to notify all
// players about important news. The clients won't be able to write any info
// messages.
//
// _Note: the channel names, colors and commands can be edited in the Inspector_
using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
[Serializable]
public class ChannelInfo
{
    public string command; // /w etc.
    public string identifier; // identifyer text
    public Color color;
    public string nextCommand;
    public float distance;
    public ChannelInfo(string command, string identifier, Color color, string nextCommand, float distance)
    {
        this.command = command;
        this.identifier = identifier;
        this.color = color;
        this.nextCommand = nextCommand;
        this.distance = distance;
    }
}
[Serializable]
public class ChatMessage
{
    public string sender;
    public string message;
    public string type;
    public string identifier;
    public ChatMessage(string sender, string message, string type, string identifier = "")
    {
        this.sender = sender;
        this.identifier = identifier;
        this.message = message;
        this.type = type;
    }
}
public partial class Chat : NetworkBehaviour
{
    [Header("Components")] // to be assigned in inspector
    public Player player;
    [HideInInspector] public static Dictionary<string, ChannelInfo> channelInfos = new Dictionary<string, ChannelInfo>();
    public override void OnStartLocalPlayer()
    {
        // create channelInfos settings
        channelInfos.Add("whisper", new ChannelInfo(PlayerPreferences.chatPrefixWhisper, ":", PlayerPreferences.chatColorWhisper, PlayerPreferences.chatPrefixWhisper, player.distanceWhisper));
        channelInfos.Add("local", new ChannelInfo("", ":", PlayerPreferences.chatColorLocal, "", player.distanceNormal));
        channelInfos.Add("loud", new ChannelInfo(PlayerPreferences.chatPrefixLoud, ":", PlayerPreferences.chatColorLoud, PlayerPreferences.chatPrefixLoud, player.distanceLoud));
        channelInfos.Add("shout", new ChannelInfo(PlayerPreferences.chatPrefixShout, ":", PlayerPreferences.chatColorShout, "", player.distanceShout));
        channelInfos.Add("ooc", new ChannelInfo(PlayerPreferences.chatPrefixOoc, " (ooc):", PlayerPreferences.chatColorOoc, "", GlobalVar.chatDistanceOoc));
        channelInfos.Add("emotion", new ChannelInfo(PlayerPreferences.chatPrefixEmotion, "", PlayerPreferences.chatColorEmotion, "", GlobalVar.chatDistanceEmotion));
        channelInfos.Add("party", new ChannelInfo(PlayerPreferences.chatPrefixParty, " (Party):", PlayerPreferences.chatColorParty, "", GlobalVar.chatDistanceMax));
        channelInfos.Add("info", new ChannelInfo("", "(Info) ", PlayerPreferences.chatColorInfo, "", GlobalVar.chatDistanceMax));
        channelInfos.Add("gm", new ChannelInfo(PlayerPreferences.chatPrefixGm, "(GM) ", PlayerPreferences.chatColorGm, "", GlobalVar.chatDistanceMax));
        channelInfos.Add("introduce", new ChannelInfo(PlayerPreferences.chatPrefixIntroduce, "", PlayerPreferences.chatColorInfo, "", GlobalVar.chatDistanceIntroduce));
        // welcome messages
        UIChat.singleton.AddMessage(new ChatMessage("", GlobalVar.chatWelcomeText, "info"));
        // ensure Debug is recognized
        if (!GlobalVar.isProduction)
            UIChat.singleton.AddMessage(new ChatMessage("", "=== Test and debug version ===", "info"));
    }
    // submit tries to send the string and then returns the new input text
    [Client]
    public string OnSubmit(string text)
    {
        player.ActivityPerformed();
        // not empty and not only spaces?
        if (!Utils.IsNullOrWhiteSpace(text))
        {
            // command in the commands list?
            // note: we don't do 'break' so that one message could potentially
            //       be sent to multiple channels (see mmorpg local chat)
            string lastcommand = "";
            if (text.StartsWith(channelInfos["whisper"].command))
            {
                lastcommand = CallStandardMessage(text, "whisper");
            }
            else if (text.StartsWith(channelInfos["loud"].command))
            {
                lastcommand = CallStandardMessage(text, "loud");
            }
            else if (text.StartsWith(channelInfos["shout"].command))
            {
                // party
                string msg = ParseGeneral(channelInfos["shout"].command, text);
                if (!Utils.IsNullOrWhiteSpace(msg))
                {
                    lastcommand = channelInfos["shout"].nextCommand;
                    CmdMsgShout(msg, channelInfos["shout"].distance, channelInfos["loud"].distance, "shout");
                }
            }
            else if (text.StartsWith(channelInfos["ooc"].command))
            {
                lastcommand = CallStandardMessage(text, "ooc");
            }
            else if (text.StartsWith(channelInfos["emotion"].command))
            {
                lastcommand = CallStandardMessage(text, "emotion");
            }
            else if (text.StartsWith(channelInfos["party"].command))
            {
                // party
                string msg = ParseGeneral(channelInfos["party"].command, text);
                if (!Utils.IsNullOrWhiteSpace(msg))
                {
                    lastcommand = channelInfos["party"].nextCommand;
                    CmdMsgParty(msg);
                }
            }
            else if (text.StartsWith(channelInfos["gm"].command))
            {
                lastcommand = channelInfos["gm"].nextCommand;
                string msg = ParseGeneral(channelInfos["gm"].command, text);
                CmdMsgToGm(player.displayName, player.id, msg);
            }
            else if (text.StartsWith(channelInfos["introduce"].command))
            {
                lastcommand = channelInfos["introduce"].nextCommand;
                CmdMsgAction(player.displayName, channelInfos["introduce"].distance, "introduce");
            }
            else
            {
                //everything else as is
                lastcommand = CallStandardMessage(text, "local");
            }
            // input text should be set to lastcommand
            return lastcommand;
        }
        // input text should be cleared
        return "";
    }
    [Client]
    string CallStandardMessage(string text, string type)
    {
        string msg = ParseGeneral(channelInfos[type].command, text);
        if (!Utils.IsNullOrWhiteSpace(msg))
        {
            CmdMsgAll(msg, channelInfos[type].distance, type);
            return channelInfos[type].nextCommand;
        }
        else
        {
            return text;
        }
    }
    [Client]
    public void CallInformMessage(string message)
    {
        CmdMsgInfo(message);
    }
    // parse a message of form "/command message"
    static string ParseGeneral(string command, string msg)
    {
        // return message without command prefix (if any)
        return msg.StartsWith(command) ? msg.Substring(command.Length) : "";
    }
    // Trim max text length
    static string TrimMessageLenght(string msg)
    {
        //return first n chars
        if (msg.Length <= GlobalVar.chatMaxTextLength)
        {
            return msg;
        }
        else
        {
            return msg.Substring(0, GlobalVar.chatMaxTextLength - 4) + " ...";
        }
    }
    // networking //////////////////////////////////////////////////////////////
    [Command]
    void CmdMsgAll(string message, float maxDistance, string type)
    {
        message = TrimMessageLenght(message);
        // find all player in range
        foreach (KeyValuePair<string, Player> onlinePlayerKVP in Player.onlinePlayers)
        {
            // Player distance
            float distance = Vector3.Distance(player.transform.position, onlinePlayerKVP.Value.transform.position);
            if (distance <= maxDistance)
            {
                // call TargetRpc on that GameObject for that connection
                onlinePlayerKVP.Value.chat.TargetMsgAll(onlinePlayerKVP.Value.connectionToClient, name, message, type);
            }
        }
    }
    [Command]
    void CmdMsgShout(string message, float maxDistance, float maxDistanceIdentification, string type)
    {
        message = TrimMessageLenght(message);
        // find all player in range
        foreach (KeyValuePair<string, Player> onlinePlayerKVP in Player.onlinePlayers)
        {
            // Player distance
            float distance = Vector3.Distance(player.transform.position, onlinePlayerKVP.Value.transform.position);
            if (distance <= maxDistanceIdentification)
            {
                // call TargetRpc on that GameObject for that connection
                onlinePlayerKVP.Value.chat.TargetMsgAll(onlinePlayerKVP.Value.connectionToClient, name, message, type);
            }
            else if (distance <= maxDistance)
            {
                // call TargetRpc on that GameObject for that connection
                onlinePlayerKVP.Value.chat.TargetMsgAll(onlinePlayerKVP.Value.connectionToClient, GlobalVar.nameNotKnown, message, type);
            }
        }
    }
    [Command]
    void CmdMsgToGm(string sender, int senderId, string message)
    {
        LogFile.WriteGmLog(0, senderId, string.Format("Message from {0}: {1}", sender, message));
        message = TrimMessageLenght(message);
        // find all GM or sending player
        foreach (KeyValuePair<string, Player> onlinePlayerKVP in Player.onlinePlayers)
        {
            if (onlinePlayerKVP.Value.id == senderId || onlinePlayerKVP.Value.isGM)
            {
                // call TargetRpc on that GameObject for that connection
                onlinePlayerKVP.Value.chat.TargetMsgAll(onlinePlayerKVP.Value.connectionToClient, name, message, "gm");
            }
        }
    }
    [Command]
    public void CmdMsgGmToAll(string message)
    {
        message = TrimMessageLenght(message);
        player.GmLogAction(0, string.Format("Broadcast: {0}", message));
        // find all
        foreach (KeyValuePair<string, Player> onlinePlayerKVP in Player.onlinePlayers)
        {
            // call TargetRpc on that GameObject for that connection
            onlinePlayerKVP.Value.chat.TargetMsgAll(onlinePlayerKVP.Value.connectionToClient, name, message, "gm");
        }
    }
    [Command]
    public void CmdMsgGmToSingle(int receiverId, string message)
    {
        message = TrimMessageLenght(message);
        // find receiver
        foreach (KeyValuePair<string, Player> onlinePlayerKVP in Player.onlinePlayers)
        {
            if (onlinePlayerKVP.Value.id == receiverId)
            {
                // call TargetRpc on that GameObject for that connection
                onlinePlayerKVP.Value.chat.TargetMsgAll(onlinePlayerKVP.Value.connectionToClient, name, message, "gm");
            }
        }
    }
    [Command]
    void CmdMsgParty(string message)
    {
        message = TrimMessageLenght(message);
        // send message to all online party members
        if (player.InParty())
        {
            foreach (string member in player.party.members)
            {
                if (Player.onlinePlayers.ContainsKey(member))
                {
                    // call TargetRpc on that GameObject for that connection
                    Player onlinePlayer = Player.onlinePlayers[member];
                    onlinePlayer.chat.TargetMsgAll(onlinePlayer.connectionToClient, name, message, "party");
                }
            }
        }
    }
    [Command]
    void CmdMsgAction(string message, float maxDistance, string type)
    {
        message = TrimMessageLenght(message);
        // find all player in range
        foreach (KeyValuePair<string, Player> onlinePlayerKVP in Player.onlinePlayers)
        {
            // Player distance
            float distance = Vector3.Distance(player.transform.position, onlinePlayerKVP.Value.transform.position);
            if (distance <= maxDistance)
            {
                // call TargetRpc on that GameObject for that connection
                onlinePlayerKVP.Value.chat.TargetMsgAction(onlinePlayerKVP.Value.connectionToClient, name, message, type);
            }
        }
    }
    [Command]
    void CmdMsgInfo(string message)
    {
        TargetMsgInfo(connectionToClient, message);
    }
    // message handlers ////////////////////////////////////////////////////////
    [TargetRpc]
    public void TargetMsgAll(NetworkConnection target, string sender, string message, string type)
    {
        // add message
        UIChat.singleton.AddMessage(new ChatMessage(sender, message, type));
    }
    // Any source can create info
    [TargetRpc]
    public void TargetMsgInfo(NetworkConnection target, string message)
    {
        UIChat.singleton.AddMessage(new ChatMessage("", message, "info"));
    }
    // Action
    [TargetRpc]
    public void TargetMsgAction(NetworkConnection target, string sender, string message, string type)
    {
        UIChat.singleton.AddMessage(new ChatMessage(sender, message, type));
    }
}
