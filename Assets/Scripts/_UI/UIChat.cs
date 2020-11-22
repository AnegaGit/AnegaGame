/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
public partial class UIChat : MonoBehaviour
{
    public static UIChat singleton;
    public GameObject panel;
    public InputField messageInput;
    public Button sendButton;
    public Transform content;
    public ScrollRect scrollRect;
    public GameObject textPrefab;
    public KeyCode[] activationKeys = { KeyCode.Return, KeyCode.KeypadEnter };
    bool eatActivation;
    public UIChat() { singleton = this; }
    void Start()
    {
        messageInput.characterLimit = GlobalVar.chatMaxTextLength;
    }
    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            panel.SetActive(!player.webcamActive);
            // character limit
            Chat chat = player.GetComponent<Chat>();
            // activation (ignored once after deselecting, so it doesn't immediately
            // activate again)
            if (Utils.AnyKeyDown(activationKeys) && !eatActivation)
            {
                messageInput.Select();
                StartCoroutine(MoveTextEnd_NextFrame());
            }
            eatActivation = false;
            // end edit listener
            messageInput.onEndEdit.SetListener((value) =>
            {
                // submit key pressed? then submit and set new input text
                if (Utils.AnyKeyDown(activationKeys))
                {
                    string newinput = chat.OnSubmit(value);
                    messageInput.text = newinput;
                    messageInput.MoveTextEnd(false);
                    if (value.Length == 0 || value == Chat.channelInfos["whisper"].command || value == Chat.channelInfos["loud"].command || !PlayerPreferences.stayInChat)
                    {
                        eatActivation = true;
                    }
                }
                // unfocus the whole chat in any case. otherwise we would scroll or
                // activate the chat window when doing wsad movement afterwards
                UIUtils.DeselectCarefully();
            });
            // send button
            sendButton.onClick.SetListener(() =>
            {
                // submit and set new input text
                string newinput = chat.OnSubmit(messageInput.text);
                messageInput.text = newinput;
                messageInput.MoveTextEnd(false);
                // unfocus the whole chat in any case. otherwise we would scroll or
                // activate the chat window when doing wsad movement afterwards
                UIUtils.DeselectCarefully();
            });
        }
        else panel.SetActive(false);
    }
    void AutoScroll()
    {
        // update first so we don't ignore recently added messages, then scroll
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
    }
    public void AddMessage(ChatMessage message)
    {
        // delete old messages so the UI doesn't eat too much performance.
        // => every Destroy call causes a lag because of a UI rebuild
        // => it's best to destroy a lot of messages at once so we don't
        //    experience that lag after every new chat message
        if (content.childCount >= GlobalVar.chatKeepHistory)
        {
            for (int i = 0; i < content.childCount / 2; ++i)
                Destroy(content.GetChild(i).gameObject);
        }
        // instantiate and initialize text prefab
        GameObject go = Instantiate(textPrefab);
        string senderText = "";
        int senderid = Player.IdFromName(message.sender);
        if (senderid > 0)
        {
            Player player = Player.localPlayer;
            if (message.type == "introduce")
            {
                if (senderid == player.id)
                    senderText = "You introduced yourself as: ";
                else
                {
                    player.UpdateKnownNames(senderid, message.message, player.KnownState(senderid));
                    return;
                }
            }
            else
            {
                if (senderid == player.id)
                    senderText = "You";
                else
                    senderText = player.KnownName(senderid);
            }
        }
        else
            senderText = message.sender;
        go.transform.SetParent(content.transform, false);
        string chatText = "";
        if (message.identifier.Length == 0)
            chatText = "<b>" + senderText + Chat.channelInfos[message.type].identifier + "</b> " + message.message;
        else
            chatText = "<b>" + senderText + message.identifier + "</b> " + message.message;
        go.GetComponent<Text>().text = chatText;
        go.GetComponent<Text>().color = Chat.channelInfos[message.type].color;
        chatText = chatText.Replace("<b>", "");
        chatText = chatText.Replace("</b>", "");
        LogFile.WriteLog(LogFile.LogLevel.Chat, chatText);
        AutoScroll();
    }
    IEnumerator MoveTextEnd_NextFrame()
    {
        yield return 0; // Skip the first frame in which this is called.
        messageInput.MoveTextEnd(false); // Do this during the next frame.
    }
}
