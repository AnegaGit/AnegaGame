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
using System.Linq;
public partial class UISpells : MonoBehaviour
{
    private KeyCode hotKey = KeyCode.M;
    public GameObject panel;
    public GameObject slotPrefab;
    public Transform content;

    bool panelActiveLast;
    int spellCountLast = 0;
    float slotheight;
    float slotWidth;

    private void Awake()
    {
        slotheight = slotPrefab.GetComponent<RectTransform>().sizeDelta.y;
        slotWidth = content.gameObject.GetComponent<RectTransform>().sizeDelta.x;
    }

    private void InitializePanel(Player player)
    {
        int spellCount = 0;
        // refresh all
        // remove old
        foreach (Transform child in content)
        {
                Destroy(child.gameObject);
        }
        if (player.spells.Count > 0)
        {
            for (int i = 0; i < player.spells.Count; i++)
            {
                Spell spell = player.spells[i];
                // only listable spells
                if (spell.data.showInSpellList)
                {
                    spellCount++;
                    GameObject go = Instantiate(slotPrefab, content);
                    UISpellSlot slot = go.GetComponent<UISpellSlot>();
                    // set state
                    slot.dragAndDropable.slot = i;
                    slot.dragAndDropable.name = i.ToString();
                    slot.dragAndDropable.dragable = true;

                    slot.playerSpellNo = i;

                    // image
                        slot.image.color = Color.white;
                        slot.image.sprite = spell.image;

                    // description
                    slot.nameText.text = spell.displayName;
                    slot.tooltip.text = spell.ToolTip();
                }
            }
            content.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(slotWidth, spellCount * slotheight);
        }
    }

    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            // hotkey (not while typing in chat, etc.)
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                panel.SetActive(!panel.activeSelf);
            // rebuild the panel if it's activated or spell No changed
            if (panel.activeSelf && (!panelActiveLast || player.spells.Count != spellCountLast))
            {
                panelActiveLast = panel.activeSelf;
                spellCountLast = player.spells.Count;
                InitializePanel(player);
            }
        }
        else panel.SetActive(false);
    }
}
