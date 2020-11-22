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
public partial class UISpellbar : MonoBehaviour
{
    public GameObject panel;
    public UISpellbarSlot slotPrefab;
    public Transform content;
    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            panel.SetActive(!player.webcamActive);
            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.spellbar.Length, content);
            // refresh all
            for (int i = 0; i < player.spellbar.Length; ++i)
            {
                UISpellbarSlot slot = content.GetChild(i).GetComponent<UISpellbarSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index
                slot.dragAndDropable.slot = i;
                // hotkey overlay (without 'Alpha' etc.)
                string pretty = player.spellbar[i].hotKey.ToString().Replace("Alpha", "");
                slot.hotkeyText.text = pretty;
                // spell, inventory item or equipment item?
                int spellIndex = player.spells.IdByName(player.spellbar[i].reference);
                if (spellIndex != -1)
                {
                    Spell spell = player.spells[spellIndex];
                    bool canCast = player.CastCheckSelf(spell);
                    // hotkey pressed and not typing in any input right now?
                    if (Input.GetKeyDown(player.spellbar[i].hotKey) &&
                        !UIUtils.AnyInputActive() &&
                        canCast) // checks mana, cooldowns, etc.) {
                    {
                        // try use the spell or walk closer if needed
                        player.TryUseSpell(spellIndex);
                    }
                    // refresh spell slot
                    slot.button.interactable = canCast; // check mana, cooldowns, etc.
                    slot.button.onClick.SetListener(() => {
                        // try use the spell or walk closer if needed
                        player.TryUseSpell(spellIndex);
                    });
                    slot.tooltip.enabled = true;
                    slot.tooltip.text = spell.displayName;
                    slot.dragAndDropable.dragable = true;
                    slot.image.color = Color.white;
                    slot.image.sprite = spell.image;
                    float cooldown = spell.CooldownRemaining();
                    slot.cooldownOverlay.SetActive(cooldown > 0);
                    slot.cooldownText.text = cooldown.ToString("F0");
                    slot.cooldownCircle.fillAmount = spell.Cooldown(player) > 0 ? cooldown / spell.Cooldown(player) : 0;
                    slot.amountOverlay.SetActive(false);
                }
                else
                {
                    // clear the outdated reference
                    player.spellbar[i].reference = "";
                    // refresh empty slot
                    slot.button.onClick.RemoveAllListeners();
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.cooldownOverlay.SetActive(false);
                    slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(false);
                }
            }
        }
        else panel.SetActive(false);
    }
}
