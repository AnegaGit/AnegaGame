/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Debug.Log("<<<"); we use reverse start text so search doesn't find this
// debug can remain for test cases
using System;
using System.Numerics;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class UITestAndDebug : MonoBehaviour
{
    public KeyCode hotKey = KeyCode.T;
    public GameObject panel;

    public InputField inputGameTimeOffset;
    public InputField inputGameTimeSpeed;
    public Text DebugPlayerText;

    // Use this for special tests
    public InputField testInput;
    public GameObject testGameObject;
    public GameObject testPositionTest;

    private void Awake()
    {
        // initialize some test and debug values
        if (!GlobalVar.isProduction)
        {

        }
    }
    void InitializeView()
    {
        inputGameTimeOffset.text = GlobalVar.testGameTimeOffset.ToString("0.0");
        inputGameTimeSpeed.text = GlobalVar.testGameTimeSpeed.ToString("0.0");
    }

    void Update()
    {
        Player player = Player.localPlayer;
        // Use in test only
        if (player && !GlobalVar.isProduction)
        {
            // hotkey (not while typing in chat, etc.)
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            {
                panel.SetActive(!panel.activeSelf);
                if (panel.activeSelf)
                    InitializeView();
            }
            if (panel.activeSelf)
            {
                DebugPlayerText.text = "Health :" + player.health + " (" + player.healthMax + " + " + player.healthRecoveryRate + "/s)" + Environment.NewLine
                    + "Mana :" + player.mana + " (" + player.manaMax + " + " + player.manaRecoveryRate + "/s)" + Environment.NewLine
                    + "Play time: " + player.playtime + "s" + Environment.NewLine
                    + "Stamina: " + player.stamina + " (" + player.staminaMaxPlayer + " + " + player.staminaRecoveryPlayer + "/s)" + Environment.NewLine
                    + "Speed: " + player.speed + " (" + player.speedWalkPlayer + " / " + player.speedRunPlayer + Environment.NewLine
                    + "Weight: " + player.weight + " (" + player.weightMaxPlayer + " g)" + Environment.NewLine
                    + "Attributes: " + player.attributesSync + Environment.NewLine
                    + "Available money: " + Money.MoneyText(Money.AvailableMoney(player)) + Environment.NewLine
                    + "State: " + player.state + Environment.NewLine;
            }
        }
    }
    public void UpdateGameTimeOffset()
    {
        if (float.TryParse(inputGameTimeOffset.text, out float result))
        {
            GlobalVar.testGameTimeOffset = result;
        }
        else
            inputGameTimeOffset.text = "error";
    }
    public void UpdateGameTimeSpeed()
    {
        if (float.TryParse(inputGameTimeSpeed.text, out float result))
        {
            GlobalVar.testGameTimeSpeed = result;
        }
        else
            inputGameTimeSpeed.text = "error";
    }

    /// <summary>
    /// Use this function for any special tests
    /// </summary>
    public void SpecialTest()
    {
        Player player = Player.localPlayer;

        // call server side fuction SpecialServerTest see below
        player.CmdTestAndDebug(testInput.text);

        // create a prefab at player position
        //Instantiate(testGameObject, player.transform.position, player.transform.rotation);
    }

    // do anything on server
    // call player.CmdTestAndDebug from client
    public void SpecialServerTest(Player player, string input)
    {
        Debug.Log("<<< Server position >>>" + player.transform.position.ToString());
    }

    // do anything with an item
    // use item found in first belt slot
    private void SpecialTestItem()
    {
        Player player = Player.localPlayer;
        if (player.inventory.GetEquipment(GlobalVar.equipmentBelt1, out Item item, out int amount))
        {
            ElementSlot es = testGameObject.GetComponent<ElementSlot>();
            es.item = item;
        }
    }

    // do anything on client
    // triggered by F11
    public void ClientActionOnF11(Player player)
    {
        player.Inform("Position:" + player.transform.position.ToString());
        Debug.Log(">>> 1.2345678 (2) =" + GlobalFunc.RoundToSignificantDigits(1.2345678, 2));
        Debug.Log(">>> 1.2345678 (3) =" + GlobalFunc.RoundToSignificantDigits(1.2345678, 3));
        Debug.Log(">>> 1.2345678 (4) =" + GlobalFunc.RoundToSignificantDigits(1.2345678, 4));
        Debug.Log(">>> 1.2345678 (5) =" + GlobalFunc.RoundToSignificantDigits(1.2345678, 5));
        Debug.Log(">>> 12345678 (2) =" + GlobalFunc.RoundToSignificantDigits(12345678, 2));
        Debug.Log(">>> 0.0012345678 (2) =" + GlobalFunc.RoundToSignificantDigits(0.0012345678, 2));
    }


}
