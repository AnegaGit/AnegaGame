/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Text;
using System.Collections.Generic;
using UnityEngine;
public class PortalElement : MonoBehaviour
{
    [Header("Portal Properties")]
    public string portalName;
    public GameObject positions;
    public TextMesh hoverText;
    public bool gmOnly = false;
    public GameObject sendMagic;
    public float interactionRange = 1f;

    List<Transform> spawnPositions = new List<Transform>();
    Player player;

    //public override string ToolTip()
    //{
    //    StringBuilder tip = new StringBuilder(base.ToolTip());
    //    tip.Replace("{Portal}", shownPortalName);
    //    return tip.ToString();
    //}
    private void Start()
    {
        Invoke("WaitForLocalPlayer", GlobalVar.repeatInitializationAttempt);
    }
    string shownPortalName
    {
        get
        {
            if (player)
            {
                if (player.readAndWrite == Abilities.Nav)
                    return "Anywhere";
                else
                    return portalName;
            }
            else
                return "";
        }
    }
    void WaitForLocalPlayer()
    {
        player = Player.localPlayer;
        if (!player)
            Invoke("WaitForLocalPlayer", GlobalVar.repeatInitializationAttempt);
        else
        {
            hoverText.text = shownPortalName;
        }
    }


    public Transform RandomPosition()
    {
        spawnPositions.Clear();
        foreach (Transform pos in positions.transform)
        {
            spawnPositions.Add(pos);
        }
        return spawnPositions[Random.Range(0, spawnPositions.Count)];
    }
    public void ActivateEntrance(bool active)
    {
        sendMagic.SetActive(active);
    }
    public void UseBy(Player player)
    {
        UIPortal uiPortal = GameObject.Find("Canvas/Teleport").GetComponent<UIPortal>();
        uiPortal.InitializePanel(player, this);
    }
}
