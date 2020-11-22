/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;
public partial class UIInventoryPanel : MonoBehaviour
{
    public UIInventory uiInventory;
    public UIInventorySlot slotPrefab;
    public Transform content;
    public int containerId = -1;
    public Text titleText;
    public Image icon;
    private Container container;
    private bool isInitialized = false;
    private int containerIndex = -1;
    private int width0 = 26;
    private int height0 = 34;
    private int slotSize = 38;
    private int widthMin = 5;
    private int heightMin = 1;
    private void InitializePanel()
    {
        Player player = Player.localPlayer;
        containerIndex = player.containers.IndexOfId(containerId);
        if (containerIndex == -1)
        {
            LogFile.WriteLog(LogFile.LogLevel.Error, string.Format("Try to open container id: {0} for player {1}. Container does not exists in container list", containerId, player.name));
            return;
        }
        container = player.containers[containerIndex];

        int widthThis = Mathf.Max(widthMin, PlayerPreferences.inventoryDefaultWidth);
        int heightThis = Mathf.Max(heightMin, PlayerPreferences.inventoryDefaultHeight);

        if (PlayerPreferences.inventoryDynamic)
        {
            int preferedSize = (int)Mathf.Sqrt((float)container.slots - 0.1f) + 1;
            widthThis = Mathf.Clamp(preferedSize, widthMin, PlayerPreferences.inventoryDynamicMax);
            heightThis = Mathf.Clamp(preferedSize, heightMin, PlayerPreferences.inventoryDynamicMax);
        }
        RectTransform rt = this.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width0 + widthThis * slotSize, height0 + heightThis * slotSize);
        GridLayoutGroup glg = content.gameObject.GetComponent<GridLayoutGroup>();
        glg.constraintCount = widthThis;

        titleText.text = container.name;
    }
    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            if (!isInitialized && containerId >= 0)
            {
                InitializePanel();
            }
            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, container.slots, content);

            // refresh all items
            for (int i = 0; i < container.slots; ++i)
            {
                UIInventorySlot slot = content.GetChild(i).GetComponent<UIInventorySlot>();
                // drag and drop index
                slot.dragAndDropable.container = containerId;
                slot.dragAndDropable.slot = i;

                if (player.inventory.GetItemSlot(container.id, i, out ItemSlot itemSlot))
                {
                    // refresh valid item
                    int icopy = i; // needed for lambdas, otherwise i is Count
                    slot.button.onClick.SetListener(() =>
                    {
                        if (itemSlot.item.data is UsableItem &&
                            ((UsableItem)itemSlot.item.data).CanUse(player,itemSlot))
                            player.CmdUseInventoryItem(container.id, icopy);
                    });
                    slot.tooltip.enabled = true;
                    slot.tooltip.text = itemSlot.ToolTip();
                    slot.dragAndDropable.dragable = true;
                    slot.image.color = Color.white;
                    slot.image.sprite = itemSlot.item.image;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
                }
                else
                {
                    // refresh invalid item
                    slot.button.onClick.RemoveAllListeners();
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.amountOverlay.SetActive(false);
                }
            }
        }
    }
    private void OnDestroy()
    {
        uiInventory.SavePanelPosition(containerId, transform.position);
    }
}
