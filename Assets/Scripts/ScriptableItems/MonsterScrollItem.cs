/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// note: spawns should have a NetworkName component for name synchronization,
//       otherwise they keep the "(Clone)" suffix on clients
using System;
using System.Text;
using UnityEngine;
using Mirror;
using System.Collections.Generic;
[CreateAssetMenu(menuName="uMMORPG/Item/Monster Scroll", order=999)]
public class MonsterScrollItem : UsableItem
{
    [Serializable]
    public struct SpawnInfo
    {
        public Monster monster;
        public int amount;
        public float distanceMultiplier;
    }
    [Header("Spawn")]
    public SpawnInfo[] spawns;
    public override void Use(Player player, int containerId, int slotIndex)
    {
        // always call base function too
        base.Use(player,  containerId,  slotIndex);
        foreach (SpawnInfo spawn in spawns)
        {
            if (spawn.monster != null)
            {
                for (int i = 0; i < spawn.amount; ++i)
                {
                    // summon in random circle position around the player
                    Vector2 circle2D = UnityEngine.Random.insideUnitCircle * spawn.distanceMultiplier;
                    Vector3 position = player.transform.position + new Vector3(circle2D.x, 0, circle2D.y);
                    GameObject go = Instantiate(spawn.monster.gameObject, position, Quaternion.identity);
                    go.name = spawn.monster.name; // avoid "(Clone)"
                    NetworkServer.Spawn(go);
                }
            }
        }
        // decrease amount
        player.inventory.DecreaseAmount(containerId,  slotIndex, 1);
    }
}
