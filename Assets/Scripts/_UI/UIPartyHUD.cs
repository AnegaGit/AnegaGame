/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public partial class UIPartyHUD : MonoBehaviour
{
    public GameObject panel;
    public UIPartyHUDMemberSlot slotPrefab;
    public Transform memberContent;
    //[Range(0,1)] public float visiblityAlphaRange = 0.5f;
    public AnimationCurve alphaCurve;
    void Update()
    {
        Player player = Player.localPlayer;
        // only show and update while there are party members
        if (player != null && player.InParty())
        {
            panel.SetActive(true);
            Party party = player.party;
            // get party members without self. no need to show self in HUD too.
            List<string> members = player.InParty() ? party.members.Where(m => m != player.name).ToList() : new List<string>();
            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, members.Count, memberContent);
            // refresh all members
            for (int i = 0; i < members.Count; ++i)
            {
                UIPartyHUDMemberSlot slot = memberContent.GetChild(i).GetComponent<UIPartyHUDMemberSlot>();
                string memberName = members[i];
                float distance = Mathf.Infinity;
                float visRange = player.VisRange();
                slot.nameText.text = memberName;
                slot.masterIndicatorText.gameObject.SetActive(party.GetMemberIndex(memberName) == 0);
                // pull health, mana, etc. from observers so that party struct
                // doesn't have to send all that data around. people will only
                // see health of party members that are near them, which is the
                // only time that it's important anyway.
                if (Player.onlinePlayers.ContainsKey(memberName))
                {
                    Player member = Player.onlinePlayers[memberName];
                    slot.icon.sprite = member.classIcon;
                    slot.healthSlider.value = member.HealthPercent();
                    slot.manaSlider.value = member.ManaPercent();
                    slot.backgroundButton.onClick.SetListener(() => {
                        player.CmdSetTarget(member.netIdentity);
                    });
                    // distance color based on visRange ratio
                    distance = Vector3.Distance(player.transform.position, member.transform.position);
                    visRange = member.VisRange(); // visRange is always based on the other guy
                }
                // distance overlay alpha based on visRange ratio
                // (because values are only up to date for members in observer
                //  range)
                float ratio = visRange > 0 ? distance / visRange : 1f;
                float alpha = alphaCurve.Evaluate(ratio);
                // icon alpha
                Color iconColor = slot.icon.color;
                iconColor.a = alpha;
                slot.icon.color = iconColor;
                // health bar alpha
                foreach (Image image in slot.healthSlider.GetComponentsInChildren<Image>())
                {
                    Color color = image.color;
                    color.a = alpha;
                    image.color = color;
                }
                // mana bar alpha
                foreach (Image image in slot.manaSlider.GetComponentsInChildren<Image>())
                {
                    Color color = image.color;
                    color.a = alpha;
                    image.color = color;
                }
            }
        }
        else panel.SetActive(false);
    }
}
