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
[CreateAssetMenu(menuName = "Anega/Spell/Target Heal", order = 001)]
public class TargetHealSpell : HealSpell
{
    public bool canHealSelf = true;
    public bool canHealOthers = false;
    // helper function to determine the target that the spell will be cast on
    // (e.g. cast on self if targeting a monster that isn't healable)
    Entity CorrectedTarget(Entity caster)
    {
        // targeting nothing? then try to cast on self
        if (caster.target == null)
            return canHealSelf ? caster : null;
        // targeting self?
        if (caster.target == caster)
            return canHealSelf ? caster : null;
        // targeting someone of same type? buff them or self
        if (caster.target.GetType() == caster.GetType())
        {
            if (canHealOthers)
                return caster.target;
            else if (canHealSelf)
                return caster;
            else
                return null;
        }
        // no valid target? try to cast on self or don't cast at all
        return canHealSelf ? caster : null;
    }
    public override bool CheckTarget(Entity caster)
    {
        // correct the target
        caster.target = CorrectedTarget(caster);
        // can only buff the target if it's not dead
        return caster.target != null && caster.target.health > 0;
    }
    // (has corrected target already)
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
    // (has corrected target already)
    public override void Apply(Entity caster)
    {
        // can't heal dead people
        if (caster.target != null && caster.target.health > 0)
        {
            CalculateHeal(out int currentHealHealth, out int currentHealMana, caster.target, caster);
            if (currentHealHealth > 0)
            {
                caster.target.health += currentHealHealth;
            }
            if (currentHealMana > 0)
            {
                caster.target.mana += currentHealMana;
            }
            if (currentHealHealth > 0 || currentHealMana > 0)
            {
                // show effect on target
                SpawnEffect(caster, caster.target);
            }
        }
    }
}
