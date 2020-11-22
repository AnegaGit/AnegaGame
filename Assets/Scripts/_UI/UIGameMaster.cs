/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class UIGameMaster : MonoBehaviour
{
    public KeyCode hotKey = KeyCode.G;
    public GameObject panel;
    [Header("Misc")]
    public GameObject panelMisc;
    public Button buttonMisc;
    public Text textInfo;
    public Button buttonLogMessage;
    public InputField messageToLogFile;
    public Button buttonSwitchVisibility;
    public Text textButtonSwitchVisibility;
    public Button buttonWebcam;
    public InputField inputWebcamCycle;
    private float webcamCycle;
    [Header("Player")]
    public GameObject panelPlayer;
    public Button buttonPlayer;
    public GameObject listPlayer;
    public GameObject panelPlayerDetail;
    public Text baseTextPlayer;
    public Button buttonBroadcast;
    public InputField messageToPlayer;
    public GameObject networkingImage;
    [Header("Warp")]
    public GameObject panelWarp;
    public Button buttonWarp;
    public GameObject listWarp;
    public GameObject panelWarpDetail;
    public GameObject panelWarpHeadline;
    [Header("Item")]
    public GameObject panelItem;
    public Button buttonItem;
    public InputField inputMoney;
    public InputField itemName;
    public InputField itemAmount;
    public GameObject panelGathering;
    public Dropdown dropdownGatheringItems;
    public InputField gatheringSpecialName;
    public Toggle gatheringRotateY;
    public Toggle gatheringRotateXZ;
    public InputField inputGatheringSize;
    public InputField inputGatheringSizeRange;
    public GameObject mousePositionPanel;
    public Sprite imageOnMousePosition;
    private float gatheringSize = 1.0f;
    private float gatheringSizeRange = 0f;

    Player player;
    float networkWaitStart = 0f;
    void InitializeView()
    {
        player = Player.localPlayer;
        if (player)
        {
            SelectRegister(1);

            buttonPlayer.gameObject.SetActive(GameMaster.seeAllPlayer(player.gmState));
            buttonBroadcast.gameObject.SetActive(GameMaster.broadcast(player.gmState));
            buttonSwitchVisibility.gameObject.SetActive(GameMaster.canInvisibility(player.gmState));
            buttonItem.gameObject.SetActive(GameMaster.createItems(player.gmState));
            panelGathering.gameObject.SetActive(GameMaster.buildEnvironment(player.gmState));
            webcamCycle = PlayerPreferences.webcamCycle;
            inputWebcamCycle.text = webcamCycle.ToString();
        }
        else
            panel.SetActive(false);
    }

    void Update()
    {
        player = Player.localPlayer;
        if (player)
        {
            // hotkey (not while typing in chat, etc.)
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive() && player.isGM)
            {
                panel.SetActive(!panel.activeSelf);
                if (panel.activeSelf)
                    InitializeView();
            }
            if (buttonSwitchVisibility.IsActive())
                if (player.isGmInvisible)
                    textButtonSwitchVisibility.text = "Make visible again";
                else
                    textButtonSwitchVisibility.text = "Make invisible for player";
        }
    }

    void RegisterSetDefault()
    {
        panelMisc.SetActive(false);
        buttonMisc.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25);
        panelPlayer.SetActive(false);
        buttonPlayer.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25);
        panelWarp.SetActive(false);
        buttonWarp.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25);
        panelItem.SetActive(false);
        buttonItem.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25);
    }
    public void SelectRegister(int register)
    {
        RegisterSetDefault();
        switch (register)
        {
            case 2:
                panelPlayer.SetActive(true);
                UpdatePlayerList();
                buttonPlayer.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 27);
                break;
            case 3:
                panelWarp.SetActive(true);
                InitializeWarpList();
                buttonWarp.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 27);
                break;
            case 4:
                panelItem.SetActive(true);
                InitializeItemList();
                buttonItem.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 27);
                break;
            default:
                panelMisc.SetActive(true);
                buttonMisc.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 27);
                break;
        }
    }
    void InitializePlayerList()
    {
        foreach (Transform child in listPlayer.transform)
        {
            if (child.gameObject.name != "PanelPlayerBaseInfo")
                Destroy(child.gameObject);
        }
        int playerCount = 0;
        int closePlayerCount = 0;
        foreach (KeyValuePair<int, string> serverPlayer in player.requestedPlayerList.OrderBy(x => x.Value))
        {
            //Don't show the player itself
            if (serverPlayer.Key != player.id)
            {
                playerCount++;
                bool isNearby = false;
                //try to find the player nearby
                foreach (KeyValuePair<string, Player> onlinePlayerKVP in Player.onlinePlayers)
                {
                    if (onlinePlayerKVP.Value.id == serverPlayer.Key)
                    {
                        closePlayerCount++;
                        GameObject goNear = Instantiate(panelPlayerDetail) as GameObject;
                        goNear.SetActive(true);
                        UIGameMasterPlayerDetail gmpdNear = goNear.GetComponent<UIGameMasterPlayerDetail>();
                        gmpdNear.Initialize(player, onlinePlayerKVP.Value);
                        gmpdNear.uiGameMaster = this;
                        goNear.transform.SetParent(listPlayer.transform);
                        isNearby = true;
                        break;
                    }
                }
                // not found in client player list, must be far away
                if (!isNearby)
                {
                    GameObject goFar = Instantiate(panelPlayerDetail) as GameObject;
                    goFar.SetActive(true);
                    UIGameMasterPlayerDetail gmpdFar = goFar.GetComponent<UIGameMasterPlayerDetail>();
                    gmpdFar.Initialize(player, serverPlayer.Key, serverPlayer.Value);
                    gmpdFar.uiGameMaster = this;
                    goFar.transform.SetParent(listPlayer.transform);
                }
            }
        }

        networkingImage.SetActive(false);
        baseTextPlayer.text = string.Format("{0} player online, {1} nearby", playerCount, closePlayerCount);
    }

    public void UpdatePlayerList()
    {
        networkingImage.SetActive(true);
        networkWaitStart = Time.time;
        player.requestedPlayerList.Clear();
        player.CmdGmRequestPlayerList();
        StartCoroutine("WaitForPlayerList");
    }

    IEnumerator WaitForPlayerList()
    {
        //unitl we have a result or wait too long
        while (player.requestedPlayerList.Count == 0 && (Time.time - networkWaitStart) < GlobalVar.networkMaxWaitTime)
        {
            yield return null;
        }
        // we have a result
        if (player.requestedPlayerList.Count > 0)
        {
            InitializePlayerList();
        }
        else
        {
            player.Inform("Wait too long for Server, no update of the list!");
            networkingImage.SetActive(false);
            LogFile.WriteLog(LogFile.LogLevel.Error, string.Format("Request for GM player list takes more than {0} seconds.", GlobalVar.networkMaxWaitTime));
        }
    }

    public void InitializeWarpList()
    {
        GameObject go;
        UIGameMasterTargetDetails gmtd;

        foreach (Transform child in listWarp.transform)
        {
            Destroy(child.gameObject);
        }
        // new player start
        go = Instantiate(panelWarpDetail) as GameObject;
        go.SetActive(true);
        go.transform.SetParent(listWarp.transform);
        gmtd = go.GetComponent<UIGameMasterTargetDetails>();
        gmtd.targetText.text = "New player start";
        gmtd.targetPos = Universal.NewPlayerSpawn.position;

        // portals headline
        go = Instantiate(panelWarpHeadline) as GameObject;
        go.SetActive(true);
        go.transform.SetParent(listWarp.transform);
        gmtd = go.GetComponent<UIGameMasterTargetDetails>();
        gmtd.targetText.text = "<b>Stationary Portals</b>";
        // portals list
        foreach (GameObject portalObject in Universal.AllPortals)
        {
            foreach (Transform subelement in portalObject.transform)
            {
                PortalElement portalElement = subelement.gameObject.GetComponent<PortalElement>();
                if (portalElement)
                {
                    if (!portalElement.gmOnly || GameMaster.enterGmIsland(player.gmState))
                    {
                        go = Instantiate(panelWarpDetail) as GameObject;
                        go.SetActive(true);
                        go.transform.SetParent(listWarp.transform);
                        gmtd = go.GetComponent<UIGameMasterTargetDetails>();
                        gmtd.targetText.text = portalElement.portalName;
                        gmtd.targetPos = portalElement.RandomPosition().position;
                    }
                    break;
                }
            }
        }

        // respawns headline
        go = Instantiate(panelWarpHeadline) as GameObject;
        go.SetActive(true);
        go.transform.SetParent(listWarp.transform);
        gmtd = go.GetComponent<UIGameMasterTargetDetails>();
        gmtd.targetText.text = "<b>Respawns</b>";
        // respawn list
        foreach (GameObject spawnObject in Universal.AllSpawns)
        {
            SpawnElement spawnElement = spawnObject.GetComponent<SpawnElement>();
            if (spawnElement)
            {
                if (!spawnElement.gmOnly || GameMaster.enterGmIsland(player.gmState))
                {
                    go = Instantiate(panelWarpDetail) as GameObject;
                    go.SetActive(true);
                    go.transform.SetParent(listWarp.transform);
                    gmtd = go.GetComponent<UIGameMasterTargetDetails>();
                    gmtd.targetText.text = spawnElement.description;
                    gmtd.targetPos = spawnElement.RandomPosition().position;
                }
            }
        }
    }

    public void InitializeItemList()
    {
        inputMoney.text = "";

        dropdownGatheringItems.ClearOptions();
        foreach (GatheringSourceItem item in Universal.GatheringSourceItems)
        {
            dropdownGatheringItems.options.Add(new Dropdown.OptionData() { text = item.name });
        }
        dropdownGatheringItems.value = 1;
        dropdownGatheringItems.value = 0;
        gatheringSpecialName.text = "";
        gatheringRotateY.isOn = true;
        gatheringRotateXZ.isOn = false;
        inputGatheringSize.text = "1";
        inputGatheringSizeRange.text = "0";
    }


    public void BroadcastMessage()
    {
        player.chat.CmdMsgGmToAll(messageToPlayer.text);
    }

    public void SinglePlayerMessage(int targetId)
    {
        player.chat.CmdMsgGmToSingle(targetId, messageToPlayer.text);
    }

    public void MessageToGMLog()
    {
        string msg = messageToLogFile.text.Trim();
        if (msg.Length > 0)
            player.GmLogAction(0, "-->" + msg);
        messageToLogFile.text = "";
    }

    public void SwitchVisibility()
    {
        player.CmdGmSwitchVisibility();
    }

    public void GetPositionInfo()
    {
        UIFollowMouse uIFollowMouse = mousePositionPanel.GetComponent<UIFollowMouse>();
        uIFollowMouse.SetReadAmbientPosition();
    }

    public void CreateMoney()
    {
        if (int.TryParse(inputMoney.text, out int money))
        {
            player.CmdChangeAvailableMoney(money);
            if (money > 0)
                player.GmLogAction(player.id, string.Format("Create money: {0}", Money.MoneyText(money)));
        }
        inputMoney.text = "";
    }

    public void CreateItem()
    {
        int amount = 1;
        if (!int.TryParse(itemAmount.text, out amount))
        {
            itemAmount.text = "1";
            amount = 1;
        }
        // just to verify if item exists
        if (ScriptableItem.dict.TryGetValue(itemName.text.GetStableHashCode(), out ScriptableItem itemData))
        {
            UsableItem ue = (UsableItem)itemData;
            if (ue)
            {
                if (ue.pickable)
                {
                    player.CmdAddItemToAvailableInventory(itemName.text, amount, GlobalVar.defaultDurability, GlobalVar.defaultQuality, "");
                    player.GmLogAction(player.id, string.Format("Create item: {0} x {1}", amount, itemName.text));
                    player.Inform(string.Format("Create item: {0} x {1}", amount, itemName.text));
                    return;
                }
            }
            player.Inform("The item '" + itemName.text + "' can not be placed in your inventory.");
        }
        else
        {
            player.Inform("There is no item '" + itemName.text + "'");
        }
    }

    //Gathering item section
    public void GatheringItemSizeChanged()
    {
        if (float.TryParse(inputGatheringSize.text, out float newSize))
        {
            gatheringSize = Mathf.Clamp(newSize, GlobalVar.minSizeCreation, GlobalVar.maxSizeCreation);
        }
        inputGatheringSize.text = gatheringSize.ToString();
    }

    public void GatheringItemSizeRangeChanged()
    {
        if (float.TryParse(inputGatheringSizeRange.text, out float newSize))
        {
            gatheringSizeRange = Mathf.Clamp(newSize, 0, GlobalVar.maxSizeCreation / 2);
        }
        inputGatheringSizeRange.text = gatheringSizeRange.ToString();
    }

    public void PlaceGatheringItem()
    {
        UIFollowMouse uIFollowMouse = mousePositionPanel.GetComponent<UIFollowMouse>();
        uIFollowMouse.SetTargetImage(imageOnMousePosition);
        uIFollowMouse.SetItemCreationParameter(dropdownGatheringItems.options[dropdownGatheringItems.value].text, GlobalFunc.RandomObfuscation(gatheringSize, gatheringSizeRange), gatheringRotateY.isOn, gatheringRotateXZ.isOn, gatheringSpecialName.text);
    }

    public void TargetYourself()
    {
        //target self
        player.CmdSetTarget(player.netIdentity);
    }

    public void WebcamControl()
    {
        UIWebcam uIWebcam = GameObject.Find("Canvas/Webcam").GetComponent<UIWebcam>();
        DayNight dayNight = GameObject.Find("SunLight").GetComponent<DayNight>();
        uIWebcam.webcamUpdateCycle = webcamCycle;
        player.webcamActive = uIWebcam.WebcamControl();
        dayNight.CalculateAmbientLight();
        if (player.webcamActive)
        {
            panel.SetActive(false);
        }
    }
    public void WebcamCycleChanged()
    {
        if (float.TryParse(inputWebcamCycle.text, out float newValue))
        {
            webcamCycle = newValue;
        }
        else
        {
            inputWebcamCycle.text = webcamCycle.ToString();
        }
    }
}
