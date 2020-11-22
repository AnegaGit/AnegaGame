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
public partial class UIRespawn : MonoBehaviour
{
    public GameObject panel;
    public Button button;
    public Dropdown respawnSelection;
    public Slider delaySlider;
    private bool isVisible = false;
    private List<Transform> locations = new List<Transform>();
    private float respawnDelay;
    void Update()
    {
        Player player = Player.localPlayer;
        // show while player is dead
        if (player != null && player.health == 0)
        {
            if (!isVisible)
            {
                panel.SetActive(true);
                isVisible = true;
                LogFile.WriteLog(LogFile.LogLevel.Info, String.Format("Character: {0} died.", player.displayName));
                respawnSelection.ClearOptions();
                locations.Clear();

                foreach (GameObject spawnObject in Universal.AllSpawns)
                {
                    SpawnElement spawnElement = spawnObject.GetComponent<SpawnElement>();
                    if (spawnElement)
                    {
                        if ((!spawnElement.gmOnly || GameMaster.enterGmIsland(player.gmState)) && !spawnElement.specialSpawn)
                        {
                            respawnSelection.options.Add(new Dropdown.OptionData() { text = spawnElement.description });
                            locations.Add(spawnElement.RandomPosition());
                        }
                    }
                }

                respawnSelection.value = 1;
                respawnSelection.value = 0;
                respawnDelay = GlobalVar.reviveDelay;
                delaySlider.gameObject.SetActive(true);
                button.gameObject.SetActive(false);
            }

            if (respawnDelay > 0)
            {
                respawnDelay -= Time.deltaTime;
                delaySlider.value = respawnDelay / GlobalVar.reviveDelay;
            }
            else
            {
                delaySlider.gameObject.SetActive(false);
                button.gameObject.SetActive(true);

                button.onClick.SetListener(() =>
                           {
                               player.CmdRespawn(locations[respawnSelection.value].position,locations[respawnSelection.value].rotation.eulerAngles.y);
                               player.ReviveClient();
                           });
            }
        }
        else if (isVisible)
        {
            panel.SetActive(false);
            isVisible = false;
        }
    }
}
