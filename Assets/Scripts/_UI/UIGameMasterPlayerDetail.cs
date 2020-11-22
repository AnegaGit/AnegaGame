/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
public class UIGameMasterPlayerDetail : MonoBehaviour
{
    Player _gmPlayer;
    Player _userPlayer;
    int _userPlayerId;
    public UIGameMaster uiGameMaster;
    public Text playerNameAndId;
    public Button buttonExamination;
    public Button buttonGoto;
    public Button buttonPull;
    public Button buttonKill;

    public void Initialize(Player gmPlayer, Player userPlayer)
    {
        _gmPlayer = gmPlayer;
        _userPlayer = userPlayer;
        _userPlayerId = _userPlayer.id;
        playerNameAndId.text = string.Format("{0} ({1})", _userPlayer.displayName, _userPlayer.id);
        ButtonInitialize(true);
    }
    public void Initialize(Player gmPlayer, int playerId, string displayName)
    {
        _gmPlayer = gmPlayer;
        _userPlayer = null;
        _userPlayerId = playerId;
        playerNameAndId.text = string.Format("{0} ({1})", displayName, playerId);
        ButtonInitialize(false);
    }
    void ButtonInitialize(bool isVisible)
    {
        if (GameMaster.hasTeleports(_gmPlayer.gmState) > 0)
            buttonPull.gameObject.SetActive(true);
        else
            buttonPull.gameObject.SetActive(false);
        if (GameMaster.hasKills(_gmPlayer.gmState) > 0 && isVisible)
            buttonKill.gameObject.SetActive(true);
        else
            buttonKill.gameObject.SetActive(false);
        if (isVisible)
            buttonExamination.gameObject.SetActive(true);
        else
            buttonExamination.gameObject.SetActive(false);
    }

    public void GotoPlayer()
    {
        _gmPlayer.CmdTeleportToPlayer(_userPlayerId);
    }
    public void PullPlayer()
    {
        if (Input.GetKey(GlobalVar.gmReleaseKey))
        {
            Vector3 spawnPos = Universal.FindPossiblePositionAround(_gmPlayer.transform.position, GlobalVar.gmTeleportDistance);
            float viewDirection = Quaternion.LookRotation(_gmPlayer.transform.position - spawnPos, Vector3.up).eulerAngles.y;
            _gmPlayer.CmdTeleportPullTarget(spawnPos.x, spawnPos.y, spawnPos.z, _userPlayerId, viewDirection);
            _gmPlayer.GmLogAction(_userPlayer.id, string.Format("GM pulled player {0}", _userPlayer.displayName));
            uiGameMaster.UpdatePlayerList();
        }
        else
            _gmPlayer.Inform("Please hold GM Relese Key to pull a player!");
    }
    public void ExaminePlayer()
    {
        UICharacterExamination characterExamination = GameObject.Find("Canvas/CharacterExamination").GetComponent<UICharacterExamination>();
        characterExamination.InitializePlayer(_userPlayer);
    }
    public void MessagePlayer()
    {
        uiGameMaster.SinglePlayerMessage(_userPlayer.id);
    }
    public void KillPlayer()
    {
        if (Input.GetKey(GlobalVar.gmReleaseKey))
        {
            _gmPlayer.CmdSetTarget(_userPlayer.netIdentity);
            _gmPlayer.CmdInstantKillTarget();
            _gmPlayer.GmLogAction(_userPlayer.id, string.Format("GM killed player {0}", _userPlayer.displayName));
            _gmPlayer.chat.CmdMsgGmToSingle(_userPlayer.id, string.Format("You was killed by GM {0} (ID:{1})", _gmPlayer.displayName, _gmPlayer.id));
            uiGameMaster.UpdatePlayerList();
        }
        else
            _gmPlayer.Inform("Please hold GM Relese Key to pull or kill a player!");
    }
}
