/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Text;
using UnityEngine;
using Mirror;
[CreateAssetMenu(menuName = "Anega/Spell/Target Damage", order = 101)]
public class TargetDamageSpell : DamageSpell
{
    public override bool CheckTarget(Entity caster)
    {
        // target exists, alive, not self, oktype?
        return caster.target != null && caster.CanAttack(caster.target);
    }
    public override bool CheckDistance(Entity caster, out Vector3 destination)
    {
        // target still around?
        if (caster.target != null)
        {
            destination = caster.target.collider.ClosestPoint(caster.transform.position);
            return Utils.ClosestDistance(caster.collider, caster.target.collider) <= CastRange(caster);
        }
        destination = caster.transform.position;
        return false;
    }
    public override void Apply(Entity caster)
    {
        // can't attack dead people
        if (caster.target != null && caster.target.health > 0)
        {
            CalculateDamage(out int currentDamage, out float currentStunTime, caster.target,caster);
            // deal damage directly with base damage + spell damage
            float usedStunTime = 0;
            if (GlobalFunc.RandomLowerLimit0_1(stunChance))
            {
                usedStunTime = currentStunTime;
            }
            if (currentDamage > 0 || usedStunTime > 0)
            {
                caster.DealDamageAt(caster.target, currentDamage, usedStunTime);
                // show effect on target
                SpawnEffect(caster, caster.target);
            }
        }
    }
}
