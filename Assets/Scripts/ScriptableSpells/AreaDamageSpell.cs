/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName = "Anega/Spell/Area Damage", order = 102)]
public class AreaDamageSpell : DamageSpell
{
    public float damageArea;
    public bool canDamageSelf = false;
    public bool canDamagePlayer = true;
    public bool canDamageMonster = true;

    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{DAMAGEAREA}", GlobalFunc.ExamineLimitText(damageArea, GlobalVar.spellRangeText));
        string canDamage = (canDamageSelf ? "self" : "");
        if (canDamagePlayer)
        {
            canDamage += (canDamage.Length > 0 ? ", " : "");
            canDamage += "chars";
        }
        if (canDamageMonster)
        {
            canDamage += (canDamage.Length > 0 ? ", " : "");
            canDamage += "monster";
        }
        tip.Replace("{CANDAMAGE}", canDamage);
        return tip.ToString();
    }

    public override bool CheckTarget(Entity caster)
    {
        // Can be applied into nothing
        return true;
    }
    public override bool CheckDistance(Entity caster, out Vector3 destination)
    {
        // Distance is givel later, so true
        destination = caster.transform.position;
        return true;
    }
    //[Server]
    // nothing happens when cast finished
    public override void Apply(Entity caster)
    {
        return;
    }

    //[Client]
    public override void OnCastFinished(Entity caster)
    {
        // Player only can execute an AOE damage, just verify
        Player player = (Player)caster;
        if (player == Player.localPlayer)
        {
            float maxDistance = CastRange(caster);
            UIFollowMouse uiFollowMouse = GameObject.Find("Canvas/FollowMouse").GetComponent<UIFollowMouse>();
            float minDistance = 0;
            if (canDamageSelf)
            {
                minDistance = damageArea;
            }
            uiFollowMouse.SetSpellTargetParameter(maxDistance, minDistance);
            // hand over and wait for AOE target
        }
    }

    //[Server]
    public override void ExecutePositionSpell(Entity caster, Vector3 targetPosition)
    {
        // Player only can execute an AOE 
        Player player = (Player)caster;
        if (player)
        {
            //we have a center target

            // candidates hashset to be 100% sure that we don't apply an area spell
            // to a candidate twice. this could happen if the candidate has more
            // than one collider (which it often has).
            HashSet<Entity> candidates = new HashSet<Entity>();
            // find all entities of same type in castRange around the caster
            Collider[] colliders = Physics.OverlapSphere(targetPosition, damageArea);
            foreach (Collider co in colliders)
            {
                Entity candidate = co.GetComponentInParent<Entity>();
                if (candidate != null &&
                    candidate.health > 0 && // can't damage dead people
                    ((candidate is Monster && canDamageMonster) || // the right type)
                     (candidate is Player && canDamagePlayer) ||
                     (candidate == player && canDamageSelf))
                    )
                {
                    candidates.Add(candidate);
                }
            }
            // apply to all candidates
            bool isFirstCandidate = true;
            foreach (Entity candidate in candidates)
            {
                CalculateDamage(out int currentDamage, out float currentStunTime, candidate, caster, isFirstCandidate);
                isFirstCandidate = false;
                float usedStunTime = 0;
                if (GlobalFunc.RandomLowerLimit0_1(stunChance))
                {
                    usedStunTime = currentStunTime;
                }
                if (currentDamage > 0 || usedStunTime > 0)
                {
                    caster.DealDamageAt(candidate, currentDamage, usedStunTime);
                    // show effect on target
                    SpawnEffect(caster, candidate);
                }

                if (isFirstCandidate)
                {
                    // learn skill  
                    float currentCastTime = CastTime(player);
                    player.LearnSkill(skill, skillLevel, currentCastTime);

                    // degrade wand
                    int slot = GlobalFunc.hasWandInHand(player);
                    if (slot != -1)
                    {
                        GlobalFunc.DegradeItem(player, GlobalVar.containerEquipment, slot, currentCastTime);
                    }
                }
                isFirstCandidate = false;
            }
        }
    }
}
