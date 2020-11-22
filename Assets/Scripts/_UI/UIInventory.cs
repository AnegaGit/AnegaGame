/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Note: this script has to be on an always-active UI parent, so that we can
// always react to the hotkey.
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public partial class UIInventory : MonoBehaviour
{
    public KeyCode hotKey = KeyCode.B;
    public UIInventoryPanel panelPrefab;
    private Dictionary<int, Vector3> panelPositions=new Dictionary<int, Vector3>();
    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            // hotkey (not while typing in chat, etc.)
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            {
                int idOfBackpack = player.ContainerIdOfBackpack();
                if (idOfBackpack == -1)
                {
                    player.Inform("You realize too late that you have forgotten your backpack.");
                    return;
                }
                OpenCloseContainer(idOfBackpack);
            }
        }
    }
    public void OpenCloseContainer(int containerIndex)
    {
        if (!CloseContainer(containerIndex))
        {
            OpenContainer(containerIndex);
        }
    }
    public bool CloseContainer(int containerIndex)
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            foreach (Transform panel in this.transform)
            {
                UIInventoryPanel inventoryPanel = panel.gameObject.GetComponent<UIInventoryPanel>();
                if (inventoryPanel.containerId == containerIndex)
                {
                    Destroy(panel.gameObject);
                    return true;
                }
            }
        }
        return false;
    }
    public void CloseAllContainerButBackpack()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            int idBackpack = player.ContainerIdOfBackpack();
            foreach (Transform panel in this.transform)
            {
                UIInventoryPanel inventoryPanel = panel.gameObject.GetComponent<UIInventoryPanel>();
                if (inventoryPanel.containerId != idBackpack)
                {
                    Destroy(panel.gameObject);
                }
            }
        }

    }
    public bool OpenContainer(int containerIndex)
    {
        return OpenContainer(containerIndex, null);
    }

    public bool OpenContainer(int containerIndex, Sprite icon)
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            foreach (Transform panel in this.transform)
            {
                UIInventoryPanel inventoryPanel = panel.gameObject.GetComponent<UIInventoryPanel>();
                if (inventoryPanel.containerId == containerIndex)
                {
                    return false;
                }
            }

            GameObject go = GameObject.Instantiate(panelPrefab.gameObject);
            go.transform.SetParent(this.transform, false);
            UIInventoryPanel newPanel = go.GetComponent<UIInventoryPanel>();
            newPanel.containerId = containerIndex;
            newPanel.uiInventory = this;
            if (panelPositions.TryGetValue(containerIndex, out Vector3 panelPos))
                go.transform.position = panelPos;
            if (icon)
                newPanel.icon.sprite = icon;
            return true;
        }
        else
            return false;
    }
    public void SavePanelPosition(int containerIndex,Vector3 panelPos)
    {
        panelPositions[containerIndex] = panelPos;
    }
}
