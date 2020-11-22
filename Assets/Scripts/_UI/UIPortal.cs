/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class UIPortal : MonoBehaviour
{
    public GameObject panel;
    public Button button;
    public Dropdown targetSelection;
    public Slider delaySlider;
    public Transform portalObjects;
    private List<PortalElement> locations = new List<PortalElement>();
    private float teleportCastTime;
    private bool teleportActive = false;
    private PortalElement startPortal;
    private Player player;
    void Update()
    {
        if (panel.activeSelf)
        {
            // show while player is dead
            float distance = Vector3.Distance(player.transform.position, startPortal.transform.position);
            if (distance > startPortal.interactionRange)
            {
                delaySlider.gameObject.SetActive(false);
                teleportActive = false;
                panel.SetActive(false);
            }
            else if (teleportActive)
            {
                teleportCastTime -= Time.deltaTime;
                delaySlider.value = 1 - teleportCastTime / GlobalVar.teleportPortalCastTime;
                player.mana -= (int)(Time.deltaTime / GlobalVar.teleportPortalCastTime * GlobalVar.teleportPortalMana);
                if (player.mana < 0)
                {
                    teleportActive = false;
                    panel.SetActive(false);
                    player.Inform("You don't have enough mana for using this portal.");
                }
                else if (teleportCastTime < 0)
                {
                    PerformTeleport();
                }
            }
        }
    }
    public void InitializePanel(Player user, PortalElement start)
    {
        player = user;
        startPortal = start;
        if (player != null)
        {
            teleportActive = false;
            panel.SetActive(true);
            targetSelection.ClearOptions();
            locations.Clear();

            foreach (GameObject portalObject in Universal.AllPortals)
            {
                foreach (Transform subelement in portalObject.transform)
                {
                    PortalElement portalElement = subelement.gameObject.GetComponent<PortalElement>();
                    if (portalElement)
                    {
                        if (startPortal.portalName != portalElement.portalName)
                        {

                            if (!portalElement.gmOnly || GameMaster.enterGmIsland(player.gmState))
                            {
                                targetSelection.options.Add(new Dropdown.OptionData() { text = portalElement.portalName });
                                locations.Add(portalElement);
                            }
                        }
                        break;
                    }
                }
            }

            if (locations.Count == 0)
            {
                panel.SetActive(false);
                startPortal.ActivateEntrance(false);
            }
            else
            {
                startPortal.ActivateEntrance(true);
                targetSelection.value = 1;
                targetSelection.value = 0;
                delaySlider.gameObject.SetActive(false);
                button.gameObject.SetActive(true);
            }
        }
    }
    public void BeginTeleport()
    {
        if (Money.AvailableMoney(player) < GlobalVar.teleportPortalCost)
            player.Inform(string.Format("You don't have enough money. You need {0}.", Money.MoneyText(GlobalVar.teleportPortalCost)));
        else
        {
            teleportCastTime = GlobalVar.teleportPortalCastTime;
            delaySlider.gameObject.SetActive(true);
            button.gameObject.SetActive(false);
            teleportActive = true;
        }
    }
    private void PerformTeleport()
    {
        delaySlider.gameObject.SetActive(false);
        teleportActive = false;
        panel.SetActive(false);

        player.CmdChangeAvailableMoney(-GlobalVar.teleportPortalCost);
        Transform spawnTarget = locations[targetSelection.value].RandomPosition();
        player.TeleportTo(spawnTarget.position,spawnTarget.rotation.eulerAngles.y);
        startPortal.ActivateEntrance(false);
    }
}
