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
using UnityEngine.UI;
public class SignPost : ScriptableElement
{
    [Header("SignPost Properties")]
    [TextArea(1, 30)] public string displayText;
    public string illiterateText = "Whatever";
    public TextMesh boardText;

    Player player;

    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{DISPLAYTEXT}", shownText);
        return tip.ToString();
    }
    private void Start()
    {
        Invoke("WaitForLocalPlayer", GlobalVar.repeatInitializationAttempt);
    }
    string shownText
    {
        get
        {
            if (player.readAndWrite == Abilities.Nav)
                return illiterateText;
            else if (player.readAndWrite == Abilities.Poor && displayText.Length > GlobalVar.illiteratePoorMaxText)
            {
                int textLength = GlobalVar.illiteratePoorCutText + displayText.IndexOf(' ', GlobalVar.illiteratePoorCutText);
                if (textLength > displayText.Length)
                    return displayText;
                return displayText.Substring(0, textLength) + " ...";
            }
            else
                return displayText;
        }
    }
    void WaitForLocalPlayer()
    {
        player = Player.localPlayer;
        if (!player)
            Invoke("WaitForLocalPlayer", GlobalVar.repeatInitializationAttempt);
        else
        {
            boardText.text = shownText;
        }
    }
}
