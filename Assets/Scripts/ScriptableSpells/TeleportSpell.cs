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

[CreateAssetMenu(menuName = "Anega/Spell/Teleport Spell", order = 601)]
public class TeleportSpell : ScriptableSpell
{
    public int teleportDistance;
    public bool canTeleportSelf = true;
    public bool canTeleportPlayer = false;
    public bool canTeleportMonster = false;
    public float area = 0;

    public OneTimeTargetSpellEffect effect;
    private float maxDistance;
    private Entity spellTarget;




    // helper function to spawn the spell effect on someone
    // (used by all the buff implementations and to load them after saving)
    public void SpawnEffect(Entity caster, Entity spawnTarget)
    {
        if (effect != null)
        {
            GameObject go = Instantiate(effect.gameObject, spawnTarget.transform.position, Quaternion.identity);
            go.GetComponent<OneTimeTargetSpellEffect>().caster = caster;
            go.GetComponent<OneTimeTargetSpellEffect>().target = spawnTarget;
            NetworkServer.Spawn(go);
        }
    }
    // tooltip /////////////////////////////////////////////////////////////////
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{TELEPORTDISTANCE}", GlobalFunc.ExamineLimitText(teleportDistance / 2, GlobalVar.teleportDistanceText));
        string canTeleport = (canTeleportSelf ? "self" : "");
        if (canTeleportPlayer)
        {
            canTeleport += (canTeleport.Length > 0 ? ", " : "");
            canTeleport += "chars";
        }
        if (canTeleportMonster)
        {
            canTeleport += (canTeleport.Length > 0 ? ", " : "");
            canTeleport += "monster";
        }
        tip.Replace("{CANTELEPORT}", canTeleport);
        return tip.ToString();
    }

    public float CalculateDistance(Entity caster = null)
    {
        if (caster is Player)
        {
            Player player = (Player)caster;
            float spellMastery = NonLinearCurves.GetFloat0_1(GlobalVar.spellMasteryNonlinear, player.skills.LevelOfSkill(skill) - skillLevel + GlobalVar.spellMasteryFitBestAt);
            if (spellMastery <= 0)
            {
                player.InformNoRepeat(string.Format("You are not skilled enough to use the spell {0}.", DisplayName), 5f);
            }
            float attributeFactor = NonLinearCurves.GetFloat0_1(GlobalVar.fightAttributeNonlinear, player.attributes.CombinedAction(skill) * 5);

            float distance = spellMastery * attributeFactor * teleportDistance;
            LogFile.WriteDebug(string.Format("Player try teleport over {0} m factors: mastery:{1}; attributes:{2} max:{3} m"
                , distance, spellMastery, attributeFactor, teleportDistance));
            return Mathf.Max(GlobalVar.minTeleportDistance, distance);
        }
        else
        {
            return teleportDistance / 2;
        }
    }

    //// helper function to determine the target that the spell will be cast on
    //// (e.g. cast on self if targeting a monster that isn't healable)
    Entity CorrectedTarget(Entity caster)
    {
        // targeting nothing? then try to cast on self
        if (caster.target == null)
            return canTeleportSelf ? caster : null;
        // targeting self?
        if (caster.target == caster)
            return canTeleportSelf ? caster : null;
        // targeting someone of same type? buff them or self
        if (caster.target is Player)
        {
            if (canTeleportPlayer)
                return caster.target;
            else if (canTeleportSelf)
                return caster;
            else
                return null;
        }
        else if (caster.target is Monster)
        {
            if (canTeleportMonster)
                return caster.target;
            else
                return null;
        }
        // no valid target? try to cast on self or don't cast at all
        return canTeleportSelf ? caster : null;
    }
    public override bool CheckSelf(Entity caster)
    {
        if (caster is Player)
        {
            return base.CheckSelf(caster);
        }
        else
        {
            return false;
        }
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
    //[Server]
    public override void Apply(Entity caster)
    {
        spellTarget = caster.target;
        return;
    }

    //[Client]
    public override void OnCastFinished(Entity caster)
    {
        // Player only can execute a teleport, just verify
        Player player = (Player)caster;
        if (player == Player.localPlayer)
        {
            maxDistance = CalculateDistance(caster);
            UIFollowMouse uiFollowMouse = GameObject.Find("Canvas/FollowMouse").GetComponent<UIFollowMouse>();
            uiFollowMouse.SetSpellTargetParameter(maxDistance, 0);
            // hand over and wait for teleport target
        }
    }

    //[Server]
    public override void ExecutePositionSpell(Entity caster, Vector3 targetPosition)
    {
        // Player only can execute a teleport 
        Player player = (Player)caster;
        if (player)
        {
            //we have a teleport target
            //can't teleport if center is dead
            if (spellTarget != null && spellTarget.health > 0)
            {
                // candidates hashset to be 100% sure that we don't apply an area spell
                // to a candidate twice. this could happen if the candidate has more
                // than one collider (which it often has).
                HashSet<Entity> candidates = new HashSet<Entity>();
                if (area <= 0)
                {
                    // single person teleport
                    // spellTarget only
                    candidates.Add(spellTarget);
                }
                else
                {
                    // group teleport
                    //we have a center source

                    // find all entities of same type in area around the spellTarget
                    Collider[] colliders = Physics.OverlapSphere(spellTarget.transform.position, area);
                    foreach (Collider co in colliders)
                    {
                        Entity candidate = co.GetComponentInParent<Entity>();
                        if (candidate != null &&
                            candidate.health > 0 && // can't damage dead people
                            ((candidate is Monster && canTeleportMonster) || // the right type)
                             (candidate is Player && canTeleportPlayer) ||
                             (candidate == player && canTeleportSelf))
                            )
                        {
                            candidates.Add(candidate);
                        }
                    }
                }

                // apply to all candidates
                int numberOfCandidate=1;
                foreach (Entity candidate in candidates)
                {
                    Vector3 landingPosition = targetPosition;
                    if (numberOfCandidate>1)
                    {
                        landingPosition=Universal.FindPossiblePositionAround(targetPosition,(numberOfCandidate<5?GlobalVar.groupTeleportCircle: GlobalVar.groupTeleportCircle*2));
                    }
                    if (candidate is Player)
                    {
                        // ask teleport target
                        ((Player)candidate).AskForTeleport(player.id, landingPosition.x, landingPosition.y, landingPosition.z, CastRange(player));
                    }
                    else
                    {
                        // teleport NPC direct
                        // move NPC
                        candidate.agent.ResetMovement();
                        candidate.agent.Warp(landingPosition);

                        // show effect on target
                        SpawnEffect(caster, candidate);
                    }

                    if (numberOfCandidate==1)
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
                    numberOfCandidate ++;
                }
            }
        }
    }
}