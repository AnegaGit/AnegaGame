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
using UnityEngine.UI;
public class UIContextMenu : MonoBehaviour
{
    private Vector3[] buttonPosition =
        {
        new Vector3(46, 24),
        new Vector3(46, -24),
        new Vector3(33, 72),
        new Vector3(33, -72),
        new Vector3(20, 120),
        new Vector3(20, -120),
        new Vector3(-24, 145),
        new Vector3(-24, -145),
        new Vector3(-72, 152),
        new Vector3(-72, -152)
    };
    public GameObject panel;
    public Button buttonSelect;
    public Button buttonGoto;
    public Button buttonInfo;
    public Button buttonFight;
    public Button buttonTake;
    public Button buttonUse;
    public Button buttonTalk;
    public Button buttonKill;
    public Button buttonTeleportToTarget;
    public Button buttonTeleportToEntity;
    public Button buttonPullPlayer;
    public Player player;
    private enum ObjectType { Nothing, Element, Item, Entity };
    private ObjectType objectType = ObjectType.Nothing;
    private ElementSlot _element;
    private UsableItem _item;
    private Entity _entity;
    private Vector3 _targetPosition;
    public ElementSlot element
    {
        set
        {
            _element = value;
            _item = null;
            _entity = null;
            objectType = ObjectType.Element;
            Initialize();
        }
    }
    public Entity entity
    {
        set
        {
            _element = null;
            _item = null;
            _entity = value;
            objectType = ObjectType.Entity;
            Initialize();
        }
    }
    public UsableItem item
    {
        set
        {
            _element = null;
            _item = value;
            _entity = null;
            objectType = ObjectType.Item;
            Initialize();
        }
    }
    public Vector3 targetPosition
    {
        set
        {
            _element = null;
            _item = null;
            _entity = null;
            _targetPosition = value;
            objectType = ObjectType.Nothing;
            Initialize();
        }
    }
    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }
    private void Initialize()
    {
        // verify action distance
        if (objectType == ObjectType.Entity)
        {
            if (Vector3.Distance(player.transform.position, _entity.transform.position) > player.distanceDetectionPerson)
                return;
        }
        else if (objectType == ObjectType.Element)
        {
            if (Vector3.Distance(player.transform.position, _element.transform.position) > player.distanceDetectionPerson)
                return;
        }

        panel.SetActive(true);
        panel.transform.position = Input.mousePosition;
        int iButton = 0;
        iButton += ButtonActivate(buttonSelect, (objectType == ObjectType.Element || objectType == ObjectType.Entity), iButton);
        iButton += ButtonActivate(buttonGoto, true, iButton);
        iButton += ButtonActivate(buttonInfo, (objectType == ObjectType.Entity), iButton);
        if (objectType == ObjectType.Element)
            iButton += ButtonActivate(buttonTake, (_element.pickable), iButton);
        else
            buttonTake.gameObject.SetActive(false);
        iButton += ButtonActivate(buttonFight, ((_entity is Monster || _entity is Player) && player.WeaponEquipped()), iButton);
        iButton += ButtonActivate(buttonTalk, (_entity is Npc), iButton);
        if (objectType == ObjectType.Element)
            iButton += ButtonActivate(buttonUse, (_element.CanUse(player)), iButton);
        else
            buttonUse.gameObject.SetActive(false);

        buttonKill.gameObject.SetActive(false);
        buttonPullPlayer.gameObject.SetActive(false);
        buttonTeleportToTarget.gameObject.SetActive(false);
        buttonTeleportToEntity.gameObject.SetActive(false);
        if (player.isGM)
        {
            if (objectType == ObjectType.Entity)
            {
                if (_entity is Monster && (GameMaster.killMonster(player.gmState)))
                    iButton += ButtonActivate(buttonKill, (_entity.health > 0), iButton);
                else if (_entity is Player && (GameMaster.hasKills(player.gmState) > 0))
                    iButton += ButtonActivate(buttonKill, (_entity.health > 0), iButton);
                iButton += ButtonActivate(buttonTeleportToEntity, true, iButton);
                if (_entity is Player && (GameMaster.hasTeleports(player.gmState) > 0))
                    iButton += ButtonActivate(buttonPullPlayer, (_entity.health > 0), iButton);
                else if (_entity is Monster && (GameMaster.pullMonster(player.gmState)))
                    iButton += ButtonActivate(buttonPullPlayer, true, iButton);
            }
            else if (objectType == ObjectType.Element)
            {
                if (_element.item.data is GatheringSourceItem && (GameMaster.buildEnvironment(player.gmState)))
                    iButton += ButtonActivate(buttonKill, true, iButton);
                iButton += ButtonActivate(buttonTeleportToTarget, true, iButton);
            }
            else if (objectType == ObjectType.Nothing)
            {
                iButton += ButtonActivate(buttonTeleportToTarget, true, iButton);
            }
        }
        //resize panel to keep in screen
        if (iButton <= 2)
            panel.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 110);
        else if (iButton <= 4)
            panel.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 210);
        else if (iButton <= 6)
            panel.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 300);
        else if (iButton <= 8)
            panel.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 350);
        else
            panel.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 350);
    }
    private int ButtonActivate(Button button, bool active, int pos = 0)
    {
        button.gameObject.SetActive(active);
        button.transform.localPosition = buttonPosition[pos];
        if (active)
            return 1;
        else
            return 0;
    }
    public void SelectTarget()
    {
        if (objectType == ObjectType.Entity)
        {
            player.CmdSetTarget(_entity.netIdentity);
        }
        if (objectType == ObjectType.Element)
        {
            player.CmdSetSelectedElement(_element.netIdentity);
        }
        panel.SetActive(false);
    }
    public void GotoTarget()
    {
        Vector3 targetPos = new Vector3();
        float stoppingDistance = 0;

        if (objectType == ObjectType.Entity)
        {
            targetPos = _entity.collider.ClosestPoint(player.transform.position);
            stoppingDistance = player.interactionRange * GlobalVar.walkInInteractionRange;
        }
        else if (objectType == ObjectType.Element)
        {
            targetPos = Universal.FindClosestNavMeshPosition(_element.transform.position, 4);
            float distanceOverMesh = Vector3.Distance(targetPos, _element.transform.position);
            float totalDistance = _element.interactionRange * GlobalVar.walkInInteractionRange;
            // player ignoeres any stopping distance below 1.1
            stoppingDistance = Mathf.Max(0, Mathf.Sqrt(totalDistance * totalDistance - distanceOverMesh * distanceOverMesh));
        }
        else if (objectType == ObjectType.Nothing)
        {
            targetPos = _targetPosition;
            stoppingDistance = 0;
        }
        else
        {
            panel.SetActive(false);
            return;
        }


        player.SetIndicatorViaPosition(targetPos);

        player.agent.stoppingDistance = stoppingDistance;
        player.agent.destination = targetPos;
        SelectTarget();
    }
    public void InfoTarget()
    {
        if (objectType == ObjectType.Entity)
        {
            UICharacterExamination characterExamination = GameObject.Find("Canvas/CharacterExamination").GetComponent<UICharacterExamination>();
            if (_entity is Player)
                characterExamination.InitializePlayer((Player)_entity);
            else
                characterExamination.InitializeEntity(_entity);
        }
        SelectTarget();
    }
    public void UseTarget()
    {
        if (objectType == ObjectType.Element)
        {
            SelectTarget();
            if (Vector3.Distance(player.transform.position, _element.transform.position) > _element.interactionRange)
            {
                player.useElementWhenCloser = _element;
                GotoTarget();
            }
            else
                player.UseSelectedElement(_element);
        }
        SelectTarget();
    }
    public void TalkToTarget()
    {
        if (_entity is Npc)
        {
            if (Vector3.Distance(player.transform.position, _entity.transform.position) > player.interactionRange)
                GotoTarget();
            else
                UINpcDialogue.singleton.Show();
        }
        SelectTarget();
    }
    public void TakeTarget()
    {
        if (objectType == ObjectType.Element)
        {
            SelectTarget();
            if (Vector3.Distance(player.transform.position, _element.transform.position) > _element.interactionRange)
            {
                player.pickElementWhenCloser = _element;
                GotoTarget();
            }
            else
                player.CmdPickSelectedElement();
        }
        panel.SetActive(false);
    }

    public void FightTarget()
    {
        if ((_entity is Monster || _entity is Player) && player.WeaponEquipped())
        {
            if (player.CanAttack(_entity))
            {
                int standardSpellId = player.spells.IdOfStandardFighting();
                if (standardSpellId >= 0)
                {
                    if (Vector3.Distance(player.transform.position, _entity.transform.position) > player.spells[standardSpellId].CastRange(player))
                    {
                        // GotoTarget
                        Vector3 targetPos = _entity.collider.ClosestPoint(player.transform.position);
                        player.agent.stoppingDistance = player.spells[standardSpellId].CastRange(player) * GlobalVar.walkInInteractionRange;
                        player.SetIndicatorViaPosition(targetPos);
                        player.agent.destination = targetPos;

                        player.useSpellWhenCloser = standardSpellId;
                    }
                    else

                    {
                        // then try to use that one
                        player.TryUseSpell(standardSpellId);
                    }
                }
            }
        }
        SelectTarget();
    }

    public void JumpToTarget()
    {
        Vector3 targetPos = new Vector3();
        float stoppingDistance = 0;

        if (objectType == ObjectType.Entity)
        {
            targetPos = _entity.collider.ClosestPoint(player.transform.position);
            stoppingDistance = GlobalVar.gmTeleportDistance;
        }
        else if (objectType == ObjectType.Element)
        {
            targetPos = _element.transform.position;
            stoppingDistance = _element.interactionRange * GlobalVar.walkInInteractionRange;
        }
        else if (objectType == ObjectType.Nothing)
        {

            targetPos = _targetPosition;
            stoppingDistance = 0;
        }
        else
        {
            panel.SetActive(false);
            return;
        }

        player.SetIndicatorViaPosition(targetPos);

        Vector3 direction = player.transform.position - targetPos;
        Vector3 spawnPos = Universal.FindPossiblePosition(targetPos + (direction.normalized * stoppingDistance), 2);

        //get view direction
        float yView;
        if (targetPos == spawnPos)
        {
            yView = Quaternion.LookRotation(spawnPos - player.transform.position, Vector3.up).eulerAngles.y;
        }
        else
        {
            yView = Quaternion.LookRotation(targetPos - spawnPos, Vector3.up).eulerAngles.y;
        }
        player.TeleportTo(spawnPos, yView);
        panel.SetActive(false);
    }

    public void PullTarget()
    {
        if (Input.GetKey(GlobalVar.gmReleaseKey))
        {
            Vector3 direction = player.transform.position - _entity.transform.position;
            Vector3 spawnPos = Universal.FindPossiblePositionAround(player.transform.position - (direction.normalized * GlobalVar.gmTeleportDistance), 2);
            float viewDirection = Quaternion.LookRotation(player.transform.position - spawnPos, Vector3.up).eulerAngles.y;
            player.CmdSetTarget(_entity.netIdentity);
            player.CmdTeleportPullTarget(spawnPos.x, spawnPos.y, spawnPos.z, 0, viewDirection);

            if (_entity is Player)
            {
                Player playerToPull = (Player)_entity;
                player.GmLogAction(playerToPull.id, string.Format("GM pulled player {0}", playerToPull.displayName));
            }
            else
            {
                player.gmNpcPullCount++;
            }
        }
        else
            player.Inform("Please hold GM Relese Key to pull or kill!");
        panel.SetActive(false);
    }

    public void KillTarget()
    {
        if (Input.GetKey(GlobalVar.gmReleaseKey))
        {
            if (_entity)
            {
                player.CmdSetTarget(_entity.netIdentity);
                player.CmdInstantKillTarget();
                if (_entity is Player)
                {
                    Player playerToKill = (Player)_entity;
                    player.GmLogAction(playerToKill.id, string.Format("GM killed player {0}", playerToKill.displayName));
                    player.chat.CmdMsgGmToSingle(playerToKill.id, string.Format("You was killed by GM {0} (ID:{1})", player.displayName, player.id));
                }
                else if (_entity is Monster)
                {
                    player.gmNpcKillCount++;
                }
            }
            else if (_element)
            {
                player.CmdSetSelectedElement(_element.netIdentity);
                player.CmdDeleteSelectedElement();
            }
        }
        else
            player.Inform("Please hold GM Relese Key to remove or kill!");
        panel.SetActive(false);
    }
}