/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// A simple spell effect that follows the target until it ends.
// -> Can be used for buffs.
//
// Note: Particle Systems need Simulation Space = Local for it to work.
using UnityEngine;
using Mirror;
public class BuffSpellEffect : SpellEffect
{
    float lastRemainingTime = Mathf.Infinity;
    [SyncVar, HideInInspector] public string buffName;
    void Update()
    {
        // only while target still exists, buff still active and hasn't been
        // recasted
        if (target != null)
        {
            int index = target.buffs.FindIndex(s => s.name == buffName);
            if (index != -1)
            {
                Buff buff = target.buffs[index];
                if (lastRemainingTime >= buff.BuffTimeRemaining()) {
                    transform.position = target.collider.bounds.center;
                    lastRemainingTime = buff.BuffTimeRemaining();
                    return;
                }
            }
        }
        // if we got here then something wasn't good, let's destroy self
        if (isServer) NetworkServer.Destroy(gameObject);
    }
}
