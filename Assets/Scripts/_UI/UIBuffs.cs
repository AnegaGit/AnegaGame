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
using System.Linq;
public partial class UIBuffs : MonoBehaviour
{
    public GameObject panel;
    public UIBuffSlot slotPrefab;
    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            panel.SetActive(true);
            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.buffs.Count, panel.transform);
            // refresh all
            for (int i = 0; i < player.buffs.Count; ++i)
            {
                UIBuffSlot slot = panel.transform.GetChild(i).GetComponent<UIBuffSlot>();
                // refresh
                slot.image.color = Color.white;
                slot.image.sprite = player.buffs[i].image;
                slot.tooltip.text = player.buffs[i].name;
                slot.slider.maxValue = player.buffs[i].buffTime;
                slot.slider.value = player.buffs[i].BuffTimeRemaining();
            }
        }
        else panel.SetActive(false);
    }
}