/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Group heal that heals all entities of same type in cast range
// => player heals players in cast range
// => monster heals monsters in cast range
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mirror;
[CreateAssetMenu(menuName = "Anega/Spell/Area Heal", order = 002)]
public class AreaHealSpell : HealSpell
{
    public override bool CheckTarget(Entity caster)
    {
        // no target necessary, but still set to self so that LookAt(target)
        // doesn't cause the player to look at a target that doesn't even matter
        caster.target = caster;
        return true;
    }
    public override bool CheckDistance(Entity caster, out Vector3 destination)
    {
        // can cast anywhere
        destination = caster.transform.position;
        return true;
    }
    public override void Apply(Entity caster)
    {
        // candidates hashset to be 100% sure that we don't apply an area spell
        // to a candidate twice. this could happen if the candidate has more
        // than one collider (which it often has).
        HashSet<Entity> candidates = new HashSet<Entity>();
        // find all entities of same type in castRange around the caster
        Collider[] colliders = Physics.OverlapSphere(caster.transform.position, CastRange(caster));
        foreach (Collider co in colliders)
        {
            Entity candidate = co.GetComponentInParent<Entity>();
            if (candidate != null &&
                candidate.health > 0 && // can't heal dead people
                candidate.GetType() == caster.GetType()) // only on same type
            {
                candidates.Add(candidate);
            }
        }
        // apply to all candidates
        bool isFirstCandidate = true;
        foreach (Entity candidate in candidates)
        {
            CalculateHeal(out int currentHealHealth, out int currentHealMana, candidate, caster, isFirstCandidate);
            isFirstCandidate = false;
            if (currentHealHealth > 0)
            {
                candidate.health += currentHealHealth;
            }
            if (currentHealMana > 0)
            {
                candidate.mana += currentHealMana;
            }
            if (currentHealHealth > 0 || currentHealMana > 0)
            {
                // show effect on target
                SpawnEffect(caster, candidate);
            }
        }
    }
}
