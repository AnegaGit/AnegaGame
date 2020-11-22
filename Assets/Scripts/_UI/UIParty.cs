/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public partial class UIParty : MonoBehaviour
{
    public KeyCode hotKey = KeyCode.P;
    public GameObject panel;
    public Text currentCapacityText;
    public Text maximumCapacityText;
    public UIPartyMemberSlot slotPrefab;
    public Transform memberContent;
    public Toggle experienceShareToggle;
    public Toggle goldShareToggle;
    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            // hotkey (not while typing in chat, etc.)
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                panel.SetActive(!panel.activeSelf);
            // only update the panel if it's active
            if (panel.activeSelf)
            {
                Party party = player.party;
                int memberCount = party.members != null ? party.members.Length : 0;
                // properties
                currentCapacityText.text = memberCount.ToString();
                maximumCapacityText.text = Party.Capacity.ToString();
                // instantiate/destroy enough slots
                UIUtils.BalancePrefabs(slotPrefab.gameObject, memberCount, memberContent);
                // refresh all members
                for (int i = 0; i < memberCount; ++i)
                {
                    UIPartyMemberSlot slot = memberContent.GetChild(i).GetComponent<UIPartyMemberSlot>();
                    string memberName = party.members[i];
                    slot.nameText.text = memberName;
                    slot.masterIndicatorText.gameObject.SetActive(i == 0);
                    // party struct doesn't sync health, mana, level, etc. We find
                    // those from observers instead. Saves bandwidth and is good
                    // enough since another member's health is only really important
                    // to use when we are fighting the same monsters.
                    // => null if member not in observer range, in which case health
                    //    bars etc. should be grayed out!
                    // update some data only if around. otherwise keep previous data.
                    // update icon only if around. otherwise keep previous one.
                    if (Player.onlinePlayers.ContainsKey(memberName))
                    {
                        Player member = Player.onlinePlayers[memberName];
                        slot.icon.sprite = member.classIcon;
                        slot.guildText.text = member.guildName;
                        slot.healthSlider.value = member.HealthPercent();
                        slot.manaSlider.value = member.ManaPercent();
                    }
                    // action button:
                    // dismiss: if i=0 and member=self and master
                    // kick: if i > 0 and player=master
                    // leave: if member=self and not master
                    if (memberName == player.name && i == 0)
                    {
                        slot.actionButton.gameObject.SetActive(true);
                        slot.actionButton.GetComponentInChildren<Text>().text = "Dismiss";
                        slot.actionButton.onClick.SetListener(() => {
                            player.CmdPartyDismiss();
                        });
                    }
                    else if (memberName == player.name && i > 0)
                    {
                        slot.actionButton.gameObject.SetActive(true);
                        slot.actionButton.GetComponentInChildren<Text>().text = "Leave";
                        slot.actionButton.onClick.SetListener(() => {
                            player.CmdPartyLeave();
                        });
                    }
                    else if (party.members[0] == player.name && i > 0)
                    {
                        slot.actionButton.gameObject.SetActive(true);
                        slot.actionButton.GetComponentInChildren<Text>().text = "Kick";
                        int icopy = i;
                        slot.actionButton.onClick.SetListener(() => {
                            player.CmdPartyKick(icopy);
                        });
                    }
                    else
                    {
                        slot.actionButton.gameObject.SetActive(false);
                    }
                }
                // gold share toggle
                goldShareToggle.interactable = player.InParty() && party.members[0] == player.name;
                goldShareToggle.onValueChanged.SetListener((val) => {}); // avoid callback while setting .isOn via code
                goldShareToggle.isOn = party.shareMoney;
                goldShareToggle.onValueChanged.SetListener((val) => {
                    player.CmdPartySetMoneyShare(val);
                });
            }
        }
        else panel.SetActive(false);
    }
}
