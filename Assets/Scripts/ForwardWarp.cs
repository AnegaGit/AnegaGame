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

public class ForwardWarp : MonoBehaviour
{
    public Transform[] targetPositions;
    public string teleportMessage;

    // teleport straingt to the target on enter
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Player player = other.transform.parent.parent.gameObject.GetComponent<Player>();
            // only if there are targets
            if (targetPositions.Length > 0)
            {
                if (teleportMessage.Length > 0)
                    player.Inform(teleportMessage);
                int i = GlobalFunc.RandomInRange(0, targetPositions.Length - 1);
                Transform sendTo = targetPositions[i];
                player.TeleportTo(sendTo.position,sendTo.rotation.eulerAngles.y);
            }
        }
    }
}
