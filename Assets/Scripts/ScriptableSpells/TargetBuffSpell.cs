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
[CreateAssetMenu(menuName = "Anega/Spell/Target Buff", order = 201)]
public class TargetBuffSpell : BuffSpell
{
    public bool canBuffSelf = true;
    public bool canBuffOthers = false; // so that players can buff other players
    public bool canBuffEnemies = false; // so that players can buff monsters
    // helper function to determine the target that the spell will be cast on
    // (e.g. cast on self if targeting a monster that can't be buffed)
    Entity CorrectedTarget(Entity caster)
    {
        // targeting nothing? then try to cast on self
        if (caster.target == null)
            return canBuffSelf ? caster : null;
        // targeting self?
        if (caster.target == caster)
            return canBuffSelf ? caster : null;
        // targeting someone of same type? buff them or self
        if (caster.target.GetType() == caster.GetType())
        {
            if (canBuffOthers)
                return caster.target;
            else if (canBuffSelf)
                return caster;
            else
                return null;
        }
        // targeting an enemy? buff them or self
        if (caster.CanAttack(caster.target))
        {
            if (canBuffEnemies)
                return caster.target;
            else if (canBuffSelf)
                return caster;
            else
                return null;
        }
        // no valid target? try to cast on self or don't cast at all
        return canBuffSelf ? caster : null;
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
        // note: caster already has the corrected target because we returned it in StartCast
        // can't buff dead people
        if (caster.target != null && caster.target.health > 0)
        {
            CalculateBuffEffects(out float buffLevel, out float buffLuckTime, caster.target, caster);
            // add buff or replace if already in there
            Debug.Log(">>> lvl:" + buffLevel + "  lt:" + buffLuckTime);
            caster.target.AddOrRefreshBuff(new Buff(this, buffLevel, buffLuckTime));
            // show effect on target
            SpawnEffect(caster, caster.target);
        }
    }
}
