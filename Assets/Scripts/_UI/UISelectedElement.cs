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
public class UISelectedElement : MonoBehaviour
{
    public GameObject panel;
    public Button nameButton;
    public Text nameText;
    public Button useButton;
    public Button takeButton;
    private ElementSlot selectedElement;
    private void Update()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            selectedElement = player.selectedElement;
            if (selectedElement != null)
            {
                float distance = Vector3.Distance(player.transform.position, selectedElement.transform.position);
                if (distance <= player.distanceDetectionPerson)
                {
                    // show
                    panel.SetActive(true);
                    // name button
                    nameText.text = selectedElement.displayName;

                    ElementSlot usableElement = (selectedElement is ElementSlot ? (ElementSlot)selectedElement : null);
                    // take button
                    if (usableElement)
                    {
                        takeButton.gameObject.SetActive(usableElement.pickable);
                    }
                    else takeButton.gameObject.SetActive(false);

                    //use button
                    if (usableElement)
                    {
                        useButton.gameObject.SetActive(usableElement.CanUse(player));
                    }
                    else useButton.gameObject.SetActive(false);
                }
                else
                {
                    // out of sight out of mind, do not remember
                    player.CmdClearSelectedElement();
                    panel.SetActive(false);
                }
            }
            else panel.SetActive(false);
        }
        else panel.SetActive(false);
    }

    public void PickSelectedElement()
    {
        Player player = Player.localPlayer;
        selectedElement = player.selectedElement;
        float distance = Vector3.Distance(player.transform.position, selectedElement.transform.position);

        if (distance <= selectedElement.interactionRange)
        {
            player.CmdPickSelectedElement();
        }
        else
        {
            player.pickElementWhenCloser = selectedElement;
            GotoTarget();
        }
    }

    public void UseSelectedElement()
    {
        Player player = Player.localPlayer;
        selectedElement = player.selectedElement;
        float distance = Vector3.Distance(player.transform.position, selectedElement.transform.position);

        if (distance <= selectedElement.interactionRange)
        {
            player.UseSelectedElement(selectedElement);
        }
        else
        {
            player.useElementWhenCloser = selectedElement;
            GotoTarget();
        }
    }

    public void GotoTarget()
    {
        Player player = Player.localPlayer;
        selectedElement = player.selectedElement;
        Vector3 targetPos = Universal.FindClosestNavMeshPosition(selectedElement.transform.position, 4);
        float distanceOverMesh = Vector3.Distance(targetPos, selectedElement.transform.position);
        float totalDistance = selectedElement.interactionRange * GlobalVar.walkInInteractionRange;
        // player ignoeres any stopping distance below 1.1
        float stoppingDistance = Mathf.Max(0, Mathf.Sqrt(totalDistance * totalDistance - distanceOverMesh * distanceOverMesh));
        player.SetIndicatorViaPosition(targetPos);

        player.agent.stoppingDistance = stoppingDistance;
        player.agent.destination = targetPos;
    }
}