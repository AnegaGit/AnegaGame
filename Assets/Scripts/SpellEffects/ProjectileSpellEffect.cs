/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Projectile spell effects like arrows, flaming fire balls, etc. that deal
// damage on the target.
//
// Note: we could move it on the server and use NetworkTransform to synchronize
// the position to all clients, which is the easy method. But we just move it on
// the server and the on the client to save bandwidth. Same result.
using UnityEngine;
using UnityEngine.Events;
using Mirror;
public class ProjectileSpellEffect : SpellEffect
{
    public float speed = 35;
    [HideInInspector] public int damage = 1; // set by spell
    [HideInInspector] public float stunChance; // set by spell
    [HideInInspector] public float stunTime; // set by spell
    // effects like a trail or particles need to have their initial positions
    // corrected too. simply connect their .Clear() functions to the event.
    public UnityEvent onSetInitialPosition;
    public override void OnStartClient()
    {
        SetInitialPosition();
    }
    void SetInitialPosition()
    {
        // the projectile should always start at the effectMount position.
        // -> server doesn't run animations, so it will never spawn it exactly
        //    where the effectMount is on the client by the time the packet
        //    reaches the client.
        // -> the best solution is to correct it here once
        if (target != null && caster != null)
        {
            transform.position = caster.effectMount.position;
            transform.LookAt(target.collider.bounds.center);
            onSetInitialPosition.Invoke();
        }
    }
    // fixedupdate on client and server to simulate the same effect without
    // using a NetworkTransform
    void FixedUpdate()
    {
        // target and caster still around?
        // note: we keep flying towards it even if it died already, because
        //       it looks weird if fireballs would be canceled inbetween.
        if (target != null && caster != null)
        {
            // move closer and look at the target
            Vector3 goal = target.collider.bounds.center;
            transform.position = Vector3.MoveTowards(transform.position, goal, speed * Time.fixedDeltaTime);
            transform.LookAt(goal);
            // server: reached it? apply spell and destroy self
            if (isServer && transform.position == goal)
            {
                if (target.health > 0)
                {
                    float usedStunTime = 0;
                    if (GlobalFunc.RandomLowerLimit0_1(stunChance))
                    {
                        usedStunTime = stunTime;
                    }
                    // find the spell that we casted this effect with
                    caster.DealDamageAt(target, damage, usedStunTime);
                }
                NetworkServer.Destroy(gameObject);
            }
        }
        else if (isServer) NetworkServer.Destroy(gameObject);
    }
}
