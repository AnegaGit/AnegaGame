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
using UnityEngine.UI;
public partial class UIEquipment : MonoBehaviour
{
    public KeyCode hotKey = KeyCode.E;
    public GameObject panel;
    public Transform content;
    public Slider weightSlider;
    public Image weightBar;
    public UIInventory inventory;
    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            // hotkey (not while typing in chat, etc.)
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            {
                panel.SetActive(!panel.activeSelf);
            }
            // only update the panel if it's active
            if (panel.activeSelf)
            {
                // refresh all
                for (int i = 0; i < GlobalVar.equipmentSize; ++i)
                {
                    UIEquipmentSlot slot = content.Find("SlotEquipment" + i.ToString()).GetComponent<UIEquipmentSlot>();
                    slot.dragAndDropable.slot = i; // drag and drop slot
                    slot.dragAndDropable.container = GlobalVar.containerEquipment;

                    if (player.inventory.GetEquipment(i, out ItemSlot itemSlot))
                    {
                        // refresh valid item
                        int icopy = i; // needed for lambdas, otherwise i is Count
                        slot.button.onClick.SetListener(() =>
                        {
                            if (itemSlot.item.data is UsableItem &&
                                ((UsableItem)itemSlot.item.data).CanUse(player,itemSlot))
                            {
                                player.CmdUseInventoryItem(GlobalVar.containerEquipment, icopy);
                            }
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
                        slot.tooltip.enabled = false;
                        slot.dragAndDropable.dragable = false;
                        slot.image.color = Color.clear;
                        slot.image.sprite = null;
                        slot.amountOverlay.SetActive(false);
                    }
                }
                float weightPercent = player.WeightPercent();
                if (player.handscale != Abilities.Excellent)
                {
                    int accuracy = GlobalVar.weightBarAccuracy[player.handscale];
                    weightPercent = (float)((int)(weightPercent * accuracy)) / accuracy + (0.5f / accuracy);
                }
                weightSlider.value = weightPercent;
                if (weightPercent > PlayerPreferences.weightWarningLimit)
                    weightBar.color = Color.Lerp(Color.gray, PlayerPreferences.weightWarningColor, (float)GlobalFunc.ProportionFromValue(weightPercent, PlayerPreferences.weightWarningLimit, 1));
                else
                    weightBar.color = Color.gray;

            }
        }
        else panel.SetActive(false);
    }
}
