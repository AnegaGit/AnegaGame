/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public partial class UINpcTrading : MonoBehaviour
{
    public static UINpcTrading singleton;
    public GameObject panel;
    public Text windowText;
    public GameObject prefabHeadline;
    [Header("NPC sell")]
    public Button buttonSell;
    public GameObject panelSell;
    public GameObject listSellItems;
    public GameObject prefabSellItem;
    [Header("NPC buy")]
    public Button buttonBuy;
    public GameObject panelBuy;
    public GameObject listBuyItems;
    public GameObject prefabBuyItem;
    [Header("Player sell")]
    public Button buttonPlayerSell;
    public GameObject panelPlayerSell;
    public GameObject listPlayerSellItems;
    public GameObject prefabPlayerSellItem;
    public Toggle filterEquipment;
    public Toggle filterBelt;
    public Toggle filterBackpack;


    private Player player;
    private Npc npc;
    private bool openBefore = false;

    private bool[] groupOpenSell;
    private bool[] groupOpenBuy;
    private bool[] groupOpenPlayerSell;

    public UINpcTrading() { singleton = this; }

    public void StartStopTrading()
    {
        panel.SetActive(!panel.activeSelf);
    }

    void Update()
    {
        player = Player.localPlayer;
        // use collider point(s) to also work with big entities
        if (player != null &&
            player.target != null && player.target is Npc &&
            Utils.ClosestDistance(player.collider, player.target.collider) <= player.interactionRange)
        {
            if (!openBefore)
            {
                npc = (Npc)player.target;
                groupOpenSell = new bool[npc.sellItems.Count];
                groupOpenBuy = new bool[npc.buyItems.Count + npc.sellItems.Count * (npc.buyAllSellItems ? 1 : 0)];
                groupOpenPlayerSell = new bool[3];
                filterEquipment.isOn = false;
                filterBelt.isOn = false;
                filterBackpack.isOn = true;
                RegisterSetDefault();
                SelectRegister(1);
                windowText.text = "Trade with " + npc.name;
                openBefore = true;
            }
        }
        else
        {
            panel.SetActive(false);
            openBefore = false;
        }
    }

    void RegisterSetDefault()
    {
        panelSell.SetActive(false);
        buttonSell.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25);
        panelBuy.SetActive(false);
        buttonBuy.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25);
        panelPlayerSell.SetActive(false);
        buttonPlayerSell.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25);
    }
    public void SelectRegister(int register)
    {
        RegisterSetDefault();
        switch (register)
        {
            case 2:
                panelBuy.SetActive(true);
                InitializeBuyList();
                buttonBuy.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 27);
                break;
            case 3:
                panelPlayerSell.SetActive(true);
                InitializePlayerSellList();
                buttonPlayerSell.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 27);
                break;
            default:
                panelSell.SetActive(true);
                InitializeSellList();
                buttonSell.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 27);
                break;
        }
    }

    private void InitializeBuyList()
    {
        foreach (Transform child in listBuyItems.transform)
        {
            Destroy(child.gameObject);
        }
        int totalSize = 0;
        int itemGroups = 0;

        if (groupOpenBuy.Length > 0)
        {
            for (int i = 0; i < npc.buyItems.Count; i++)
            {
                // headline first
                GameObject go = Instantiate(prefabHeadline) as GameObject;
                PanelNpcTradingHeadline pnth = go.GetComponent<PanelNpcTradingHeadline>();
                pnth.Initialize(i, 2, npc.buyItems[i].headline, this, !groupOpenBuy[i]);
                go.transform.SetParent(listBuyItems.transform);
                totalSize += 20;
                if (groupOpenBuy[i])
                {
                    // now the items
                    foreach (ScriptableItem scriptableItem in npc.buyItems[i].items)
                    {
                        long currentPrice = Money.MoneyRound((long)(scriptableItem.price * npc.buyItems[i].priceLevel * GlobalVar.priceSellNormal * (1.0f - player.divergencePrice)));
                        GameObject goItem = Instantiate(prefabBuyItem) as GameObject;
                        PanelNpcTradingBuy pntb = goItem.GetComponent<PanelNpcTradingBuy>();
                        pntb.Initialize(this, scriptableItem.displayName, scriptableItem.image, currentPrice);
                        goItem.transform.SetParent(listBuyItems.transform);
                        totalSize += 30;
                    }
                }
                itemGroups = i;
            }
        }
        if (npc.buyAllSellItems && groupOpenSell.Length > 0)
        {
            for (int i = 0; i < npc.sellItems.Count; i++)
            {
                // headline first
                GameObject go = Instantiate(prefabHeadline) as GameObject;
                PanelNpcTradingHeadline pnth = go.GetComponent<PanelNpcTradingHeadline>();
                pnth.Initialize(itemGroups + i, 2, npc.sellItems[i].headline, this, !groupOpenBuy[itemGroups + i]);
                go.transform.SetParent(listBuyItems.transform);
                totalSize += 20;
                if (groupOpenBuy[itemGroups + i])
                {
                    // now the items
                    foreach (SellItem sellItem in npc.sellItems[i].items)
                    {
                        ScriptableItem scriptableItem = sellItem.item;
                        long currentPrice = Money.MoneyRound((long)(scriptableItem.price * npc.sellItems[i].priceLevel * GlobalVar.priceSellNormal * (1.0f - player.divergencePrice)));
                        GameObject goItem = Instantiate(prefabBuyItem) as GameObject;
                        PanelNpcTradingBuy pntb = goItem.GetComponent<PanelNpcTradingBuy>();
                        pntb.Initialize(this, scriptableItem.displayName, scriptableItem.image, currentPrice);
                        goItem.transform.SetParent(listBuyItems.transform);
                        totalSize += 30;
                    }
                }
            }
        }
        if (groupOpenBuy.Length == 0 && (!npc.buyAllSellItems || groupOpenSell.Length == 0))
        {
            // nothing to buy
            GameObject go = Instantiate(prefabHeadline) as GameObject;
            PanelNpcTradingHeadline hdlno = go.GetComponent<PanelNpcTradingHeadline>();
            hdlno.Initialize(-1, 2, "Does not buy anything!", this, true);
            go.transform.SetParent(listBuyItems.transform);
            totalSize += 20;
        }
        listBuyItems.GetComponent<RectTransform>().sizeDelta = new Vector2(0, totalSize + 10);
    }

    private void InitializeSellList()
    {
        foreach (Transform child in listSellItems.transform)
        {
            Destroy(child.gameObject);
        }
        int totalSize = 0;
        if (groupOpenSell.Length == 0)
        {
            // nothing to Sell
            GameObject go = Instantiate(prefabHeadline) as GameObject;
            PanelNpcTradingHeadline hdlno = go.GetComponent<PanelNpcTradingHeadline>();
            hdlno.Initialize(-1, 1, "Has nothing to sell!", this, true);
            go.transform.SetParent(listSellItems.transform);
            totalSize += 20;
        }
        else
        {
            for (int i = 0; i < npc.sellItems.Count; i++)
            {
                // headline first
                GameObject go = Instantiate(prefabHeadline) as GameObject;
                PanelNpcTradingHeadline pnth = go.GetComponent<PanelNpcTradingHeadline>();
                pnth.Initialize(i, 1, npc.sellItems[i].headline, this, !groupOpenSell[i]);
                go.transform.SetParent(listSellItems.transform);
                totalSize += 20;
                if (groupOpenSell[i])
                {
                    // now the items
                    foreach (SellItem sellItem in npc.sellItems[i].items)
                    {
                        long currentPrice = Money.AdaptToDurabilityAndQuality((long)(sellItem.item.price * sellItem.priceLevel * (1.0f + player.divergencePrice)), sellItem.durability, sellItem.quality);
                        GameObject goItem = Instantiate(prefabSellItem) as GameObject;
                        PanelNpcTradingSell pntb = goItem.GetComponent<PanelNpcTradingSell>();

                        // build ToolTip
                        StringBuilder tip = new StringBuilder(sellItem.item.ToolTip());
                        tip.Replace("{QUALITY}", GlobalFunc.ExamineLimitText((float)sellItem.quality / GlobalVar.itemQualityMax, GlobalVar.itemQualityBase));
                        tip.Replace("{DURABILITY}", GlobalFunc.ExamineLimitText((float)sellItem.durability / GlobalVar.itemDurabilityMax, GlobalVar.itemDurabilityBase));
                        tip.Replace("{TOTALWEIGHT}", GlobalFunc.WeightText((int)(sellItem.item.weight * player.divergenceWeight), player.handscale));
                        tip.Replace("{SPECIALNAME}", sellItem.item.displayName);
                        tip.Replace("{AMOUNT}", "1");
                        tip.Replace("{TOTALPRICE}", Money.MoneyText(sellItem.item.price));
                        tip.Replace("{LIGHTTIME}", GlobalVar.lightTimeNew);

                        pntb.Initialize(this, sellItem.item.name, sellItem.item.displayName, sellItem.item.image, currentPrice, sellItem.quality, sellItem.durability, tip.ToString());
                        goItem.transform.SetParent(listSellItems.transform);
                        totalSize += 30;
                    }
                }
            }
        }
        listSellItems.GetComponent<RectTransform>().sizeDelta = new Vector2(0, totalSize + 10);
    }

    private void InitializePlayerSellList()
    {
        foreach (Transform child in listPlayerSellItems.transform)
        {
            Destroy(child.gameObject);
        }
        int totalSize = 0;
        int foundItems = 0;

        // equipment
        if (filterEquipment.isOn)
        {
            // headline first
            GameObject go = Instantiate(prefabHeadline) as GameObject;
            PanelNpcTradingHeadline pnth = go.GetComponent<PanelNpcTradingHeadline>();
            pnth.Initialize(0, 3, "Equipment", this, !groupOpenPlayerSell[0]);
            go.transform.SetParent(listPlayerSellItems.transform);
            totalSize += 20;

            int equipmentItems = 0;
            //all equipped items, not the backpack
            for (int i = 0; i < GlobalVar.equipmentBelt1; i++)
            {
                if (InitializePlayerSellItem(GlobalVar.containerEquipment, i, !groupOpenPlayerSell[0]))
                {
                    if (groupOpenPlayerSell[0])
                    {
                        totalSize += 30;
                    }
                    foundItems++;
                    equipmentItems++;
                }
            }
            pnth.headlineText.text = string.Format("Equipment ({0})", equipmentItems);
        }
        // belt
        if (filterBelt.isOn)
        {
            // headline first
            GameObject go = Instantiate(prefabHeadline) as GameObject;
            PanelNpcTradingHeadline pnth = go.GetComponent<PanelNpcTradingHeadline>();
            pnth.Initialize(1, 3, "Belt", this, !groupOpenPlayerSell[1]);
            go.transform.SetParent(listPlayerSellItems.transform);
            totalSize += 20;

            int beltItems = 0;
            //all items in belt
            for (int i = GlobalVar.equipmentBelt1; i <= GlobalVar.equipmentBelt6; i++)
            {
                if (InitializePlayerSellItem(GlobalVar.containerEquipment, i, !groupOpenPlayerSell[1]))
                {
                    if (groupOpenPlayerSell[1])
                    {
                        totalSize += 30;
                    }
                    foundItems++;
                    beltItems++;
                }
            }
            pnth.headlineText.text = string.Format("Belt ({0})", beltItems);
        }
        // backpack
        if (filterBackpack.isOn)
        {
            int idBackpack = player.ContainerIdOfBackpack();
            if (idBackpack < 0)
            {
                // no backpack, headline only
                GameObject gohlo = Instantiate(prefabHeadline) as GameObject;
                PanelNpcTradingHeadline pnthlo = gohlo.GetComponent<PanelNpcTradingHeadline>();
                pnthlo.Initialize(-1, 3, "No backpack equipped", this, false);
                gohlo.transform.SetParent(listPlayerSellItems.transform);
                totalSize += 20;
            }
            else
            {
                // headline first
                GameObject go = Instantiate(prefabHeadline) as GameObject;
                PanelNpcTradingHeadline pnth = go.GetComponent<PanelNpcTradingHeadline>();
                pnth.Initialize(2, 3, "Backpack", this, !groupOpenPlayerSell[2]);
                go.transform.SetParent(listPlayerSellItems.transform);
                totalSize += 20;

                int backpackItems = 0;
                //all items in the backpack
                for (int i = 0; i < player.containers.SlotsInId(idBackpack); i++)
                {
                    if (InitializePlayerSellItem(idBackpack, i, !groupOpenPlayerSell[2]))
                    {
                        if (groupOpenPlayerSell[2])
                        {
                            totalSize += 30;
                        }
                        foundItems++;
                        backpackItems++;
                    }
                }
                pnth.headlineText.text = string.Format("Backpack ({0})", backpackItems);
            }
        }
        if (foundItems == 0)
        {
            // nothing to Sell
            GameObject go = Instantiate(prefabHeadline) as GameObject;
            PanelNpcTradingHeadline hdlno = go.GetComponent<PanelNpcTradingHeadline>();
            hdlno.Initialize(-1, 3, "You have nothing to sell!", this, true);
            go.transform.SetParent(listPlayerSellItems.transform);
            totalSize += 20;
        }
        listPlayerSellItems.GetComponent<RectTransform>().sizeDelta = new Vector2(0, totalSize + 10);
    }
    private bool InitializePlayerSellItem(int containerId, int slotIndex, bool countOnly)
    {
        // get the item
        if (player.inventory.GetItemSlot(containerId, slotIndex, out ItemSlot itemSlot))
        {
            // does the npc buy this itsem?
            for (int i = 0; i < npc.buyItems.Count; i++)
            {
                foreach (ScriptableItem scriptableItem in npc.buyItems[i].items)
                {
                    bool skipThis = false;
                    if (itemSlot.item.data is ContainerItem)
                    {
                        // cannot sell filled bag
                        if (player.inventory.AllInContainer(itemSlot.item.data1).Count > 0)
                        {
                            skipThis = true;
                        }
                    }
                    if (itemSlot.item.data is LightItem)
                    {
                        //cannot sell burning torch etc.
                        if (itemSlot.item.data2 == 1)
                        {
                            skipThis = true;
                        }
                    }
                    // item found in NPC list?
                    if (scriptableItem.name == itemSlot.item.name && !skipThis)
                    {
                        if (!countOnly)
                        {
                            long currentPrice = Money.AdaptToDurabilityAndQuality((long)(scriptableItem.price * npc.buyItems[i].priceLevel * GlobalVar.priceSellNormal * (1.0f - player.divergencePrice) * itemSlot.amount), itemSlot.item.durability, itemSlot.item.quality);
                            GameObject goItem = Instantiate(prefabPlayerSellItem) as GameObject;
                            PanelNpcTradingPlayerSell pntb = goItem.GetComponent<PanelNpcTradingPlayerSell>();
                            pntb.Initialize(this, itemSlot, currentPrice);
                            goItem.transform.SetParent(listPlayerSellItems.transform);
                        }
                        return true;
                    }
                }
            }
            // does the npc sell and buy this itsem?
            if (npc.buyAllSellItems && groupOpenSell.Length > 0)
            {
                for (int i = 0; i < npc.sellItems.Count; i++)
                {
                    // now the items
                    foreach (SellItem sellItem in npc.sellItems[i].items)
                    {
                        ScriptableItem scriptableItem = sellItem.item;
                        bool skipThis = false;
                        if (itemSlot.item.data is ContainerItem)
                        {
                            // cannot sell filled bag
                            if (player.inventory.AllInContainer(itemSlot.item.data1).Count > 0)
                            {
                                skipThis = true;
                            }
                        }
                        if (itemSlot.item.data is LightItem)
                        {
                            //cannot sell burning torch etc.
                            if (itemSlot.item.data2 == 1)
                            {
                                skipThis = true;
                            }
                        }
                        // item found in NPC list?
                        if (scriptableItem.name == itemSlot.item.name && !skipThis)
                        {
                            if (!countOnly)
                            {
                                long currentPrice = Money.AdaptToDurabilityAndQuality((long)(scriptableItem.price * sellItem.priceLevel * GlobalVar.priceSellNormal * (1.0f - player.divergencePrice) * itemSlot.amount), itemSlot.item.durability, itemSlot.item.quality);
                                GameObject goItem = Instantiate(prefabPlayerSellItem) as GameObject;
                                PanelNpcTradingPlayerSell pntb = goItem.GetComponent<PanelNpcTradingPlayerSell>();
                                pntb.Initialize(this, itemSlot, currentPrice);
                                goItem.transform.SetParent(listPlayerSellItems.transform);
                            }
                            return true;
                        }
                    }
                }
            }
        }
        // nothing found
        return false;
    }

    public void ExpandBuyGroup(int index, bool expand)
    {
        groupOpenBuy[index] = expand;
        InitializeBuyList();
    }
    public void ExpandSellGroup(int index, bool expand)
    {
        groupOpenSell[index] = expand;
        InitializeSellList();
    }
    public void ExpandPlayerSellGroup(int index, bool expand)
    {
        groupOpenPlayerSell[index] = expand;
        InitializePlayerSellList();
    }
    public void ChangedPlayerSellList()
    {
        InitializePlayerSellList();
    }

    public void MouseEnterPanel()
    {
        if (panelPlayerSell.activeSelf)
        {
            InitializePlayerSellList();
        }
    }
}
