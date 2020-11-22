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

public class UIFollowMouse : MonoBehaviour
{
    public GameObject mousePositionPanel;
    public Image hairCross;
    public Image targetImage;

    enum ActionType { createSemistaticElement, readAmbientPosition, getSpellTarget };
    private ActionType actionType;

    private Vector3 selectedPosition;

    private string creationItemName;
    private float creationItemSize;
    private bool creationItemRotationY;
    private bool creationItemRotationXZ;
    private string creationItemSpecialName;
    private float _maxDistance = GlobalVar.maxCreationDistance;
    private float _minDistance = 0;

    private Vector3 posCorr;

    private void Awake()
    {
        posCorr = new Vector3(mousePositionPanel.transform.GetComponent<RectTransform>().sizeDelta.x / 2, mousePositionPanel.transform.GetComponent<RectTransform>().sizeDelta.y / 2);
    }


    void Update()
    {
        if (mousePositionPanel.activeSelf)
        {
            Vector3 mousePos = Input.mousePosition;
            Ray r = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(r, out hit, GlobalVar.maxRaycastMouseFollow, GlobalVar.layerMaskTerrain))
            {
                float distance = Vector3.Distance(hit.point, Player.localPlayer.transform.position);
                if (Input.GetMouseButtonDown(0))
                {
                    if (distance <= _maxDistance)
                    {
                        selectedPosition = hit.point;
                        ActionAtPosition();
                    }
                }
                if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
                {
                    CloseFunction();
                }

                if (distance <= _minDistance)
                {
                    hairCross.color = GlobalVar.mouseFollowDanger;
                }
                else if (distance <= _maxDistance)
                {
                    hairCross.color = GlobalVar.mouseFollowActive;
                }
                else
                {
                    hairCross.color = GlobalVar.mouseFollowInactive;
                }
            }
            else
            {
                hairCross.color = GlobalVar.mouseFollowInactive;
            }
            mousePositionPanel.transform.position = mousePos - posCorr;
        }
    }

    void CloseFunction()
    {
        targetImage.gameObject.SetActive(false); // for recall with no image
        mousePositionPanel.SetActive(false);
    }

    public void SetTargetImage(Sprite sprite)
    {
        targetImage.sprite = sprite;
        targetImage.gameObject.SetActive(true);
    }

    public void SetReadAmbientPosition()
    {
        actionType = ActionType.readAmbientPosition;
        _maxDistance = GlobalVar.maxCreationDistance;
        _minDistance = 0;
        mousePositionPanel.SetActive(true);
    }

    public void SetItemCreationParameter(string itemName, float creationSize, bool creationRotationY, bool creationRotationXZ, string specialName)
    {
        actionType = ActionType.createSemistaticElement;
        creationItemName = itemName;
        creationItemSize = creationSize;
        creationItemRotationY = creationRotationY;
        creationItemRotationXZ = creationRotationXZ;
        creationItemSpecialName = specialName;
        _maxDistance = GlobalVar.maxCreationDistance;
        _minDistance = 0;
        mousePositionPanel.SetActive(true);
    }

    public void SetSpellTargetParameter(float maxDistance, float minDistance=0)
    {
        actionType = ActionType.getSpellTarget;
        _maxDistance = maxDistance;
        _minDistance = minDistance;
        mousePositionPanel.SetActive(true);
    }

    public void ActionAtPosition()
    {
        Player player = Player.localPlayer;
        switch (actionType)
        {
            case ActionType.createSemistaticElement:
                {
                    player.CmdCreateAmbientElement(creationItemName, selectedPosition, creationItemSize, creationItemRotationY, creationItemRotationXZ, creationItemSpecialName, "", "");
                    CloseFunction(); break;
                }
            case ActionType.readAmbientPosition:
                {   // the info is on the server only, therefor transfer the request to the server
                    player.CmdShowAmbientPosition(selectedPosition);
                    break;
                }
            case ActionType.getSpellTarget:
                {
                    player.CmdExecutePositionSpell(selectedPosition.x, selectedPosition.y, selectedPosition.z);
                    CloseFunction();
                    break;
                }
            default:
                {
                    CloseFunction();
                    break;
                }
        }
    }
}