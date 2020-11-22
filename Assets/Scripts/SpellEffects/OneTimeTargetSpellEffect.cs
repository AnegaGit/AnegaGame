/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using Mirror;
public class OneTimeTargetSpellEffect : SpellEffect
{
    public ParticleSystem leadParticleSystem;

    private void Awake()
    {
        if (!leadParticleSystem)
        {
            leadParticleSystem = GetComponent<ParticleSystem>();
        }
    }
    void Update()
    {
        // follow the target's position (because we can't make a NetworkIdentity
        // a child of another NetworkIdentity)
        if (target != null)
            transform.position = target.collider.bounds.center;
        // destroy self if target disappeared or particle ended
        if (isServer)
            if (target == null || !leadParticleSystem.IsAlive())
                NetworkServer.Destroy(gameObject);
    }
}
