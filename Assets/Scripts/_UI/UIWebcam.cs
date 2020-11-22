
/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIWebcam : MonoBehaviour
{
    public GameObject panelWebcam;
    public GameObject panelLogo;
    public Text timeWebcam;
    public Text playerWebcam;
    public float webcamUpdateCycle;
    public GameObject[] hidePanels;

    private float lastWebcamUpdate = 0f;
    private float networkWaitStart;
    private Player player;
    private bool isActive;
    private bool takeScreenshotNow;


    void Update()
    {
        if (panelWebcam.activeSelf)
        {
            if (!isActive)
            {
                // first time
                isActive = true;
                int i = 0;
                while (i < hidePanels.Length)
                {
                    hidePanels[i].SetActive(false);
                    i++;
                }
            }
            if (Time.time > lastWebcamUpdate + webcamUpdateCycle)
            {
                player = Player.localPlayer;
                lastWebcamUpdate = Time.time;
                GameTime gt = new GameTime();
                timeWebcam.text = gt.DateTimeString;

                //player mut come from server
                networkWaitStart = Time.time;
                player.requestedPlayerList.Clear();
                player.CmdGmRequestPlayerList();
                takeScreenshotNow = false;
                StartCoroutine("WaitForPlayerList");
            }
        }
        else if (isActive)
        {
            isActive = false;
            int i = 0;
            while (i < hidePanels.Length)
            {
                hidePanels[i].SetActive(true);
                i++;
            }
        }
    }

    public bool WebcamControl()
    {
        panelWebcam.SetActive(!panelWebcam.activeSelf);
        panelLogo.SetActive(panelWebcam.activeSelf);
        return panelWebcam.activeSelf;
    }

    IEnumerator WaitForPlayerList()
    {
        //unitl we have a result or wait too long
        while (player.requestedPlayerList.Count == 0 && (Time.time - networkWaitStart) < GlobalVar.networkMaxWaitTime)
        {
            yield return null;
        }
        // we have a result
        if (player.requestedPlayerList.Count > 0)
        {
            playerWebcam.text = string.Format("{0} character online", player.requestedPlayerList.Count - 1);
        }
        //wait for screen update
        if (!takeScreenshotNow)
        {
            takeScreenshotNow = true;
            yield return null;
        }
        player.Screenshot(true);
    }
}
