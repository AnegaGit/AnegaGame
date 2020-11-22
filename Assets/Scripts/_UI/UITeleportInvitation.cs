/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public partial class UITeleportInvitation : MonoBehaviour
{
    public GameObject panel;
    public Slider delaySlider;
    public Text InviteText;
    private Vector3 _targetLocation;
    private Player player;
    private float rejectDelay = GlobalVar.waitUntilTeleportRejected;

    public void Initialize(Player sender, Vector3 targetLocation, bool askForPermission)
    {
        // we cannot teleport anybody with an already open invitation
        if (panel.activeSelf == false)
        {
            player = Player.localPlayer;
            if (!askForPermission || player == sender)
            {
                player.TeleportTo(targetLocation,player.transform.rotation.eulerAngles.y);
            }
            else
            {
                InviteText.text = string.Format("{1} " + Environment.NewLine + "want to teleport you over {0:0} m."
                    , Vector3.Distance(targetLocation, player.transform.position)
                    , player.KnownName(sender.id));
                rejectDelay = GlobalVar.waitUntilTeleportRejected;
                _targetLocation = targetLocation;
                delaySlider.value = 1;
                panel.SetActive(true);
            }
        }
    }

    void Update()
    {
        if (panel.activeSelf)
        {
            // show not while player is dead
            if (player != null)
            {
                if (rejectDelay > 0 && player.health > 0)
                {
                    rejectDelay -= Time.deltaTime;
                    delaySlider.value = rejectDelay / GlobalVar.waitUntilTeleportRejected;
                }
                else
                {
                    panel.SetActive(false);
                }
            }
        }
    }

    public void RejectTeleport()
    {
        panel.SetActive(false);
    }

    public void AcceptTeleport()
    {
        player.TeleportTo(_targetLocation, player.transform.rotation.eulerAngles.y);
        panel.SetActive(false);
    }
}
