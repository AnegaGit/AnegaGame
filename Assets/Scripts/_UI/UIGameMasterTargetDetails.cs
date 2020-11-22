/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;
public class UIGameMasterTargetDetails : MonoBehaviour
{
    public Vector3 targetPos;
    public Text targetText;

    public void WarpToPosition()
    {
        Player player = Player.localPlayer;
        player.TeleportTo(Universal.FindPossiblePosition(targetPos, GlobalVar.gmTeleportDistance));
    }
}
