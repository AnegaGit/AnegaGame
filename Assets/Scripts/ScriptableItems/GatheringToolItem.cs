/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Text;
using UnityEngine;
[CreateAssetMenu(menuName = "Anega/Item/Gathering Tool", order = 200)]
public class GatheringToolItem : EquipmentItem
{
    [Header("Gathering")]
    public Skills.Skill skillTool;
    [Range(0, 100)] public int levelTool;
    [Range(0, 1)] public float luckPortionTool;
    public float luckMaxTool;
    public float workTimeMin;
    public float workTimeMax;
    public float workDistance;

    // use it in inventory start usage
    public override void Use(Player player, int containerId, int slotIndex)
    {
        // always call base function too
        base.Use(player, containerId, slotIndex);
        // Use if item in hand
        if (GlobalFunc.IsInHand(containerId, slotIndex))
        {
            // search for resource to work with and start working
            if (FindResource(player, out GatheringSourceItem sourceItem))
            {
                // skilled enough for the resource?
                if (player.selectedElement.CanUse(player))
                {
                    float distance = Vector3.Distance(player.transform.position, player.selectedElement.transform.position);
                    if (distance <= workDistance)
                    {
                        // calculate working time
                        float attributeFactor = NonLinearCurves.GetFloat0_1(GlobalVar.fightAttributeNonlinear, player.attributes.CombinedAction(skillTool) * 5);
                        int attributeSkill = (int)(player.skills.LevelOfSkill(skillTool) * attributeFactor);
                        float workTime = NonLinearCurves.FloatFromCurvePosition(GlobalVar.toolWorkingTimeNonlinear, attributeSkill, 0, 100, workTimeMax, workTimeMin);
                        LogFile.WriteDebug(string.Format("Gathering: Start working with {4} for {0}s  attributeFactor:{1}; attributeSkill:{2} on {3}"
                            , workTime, attributeFactor, attributeSkill, player.selectedElement.name, this.displayName));
                        player.StartWorking(slotIndex);
                        player.UseOverTime(containerId, slotIndex, workTime);
                    }
                    else
                    {
                        player.Inform(string.Format("You cannot reach {1} with your {0} properly. Maybe you walk a little bit closer.", displayName, player.selectedElement.displayName));
                        player.StopWorking();
                    }
                }
                else
                {
                    player.Inform(string.Format("That's too difficult for you. You miss the skill how to use the tool {0} on {1}.", displayName, player.selectedElement.displayName));
                    player.StopWorking();
                }
            }
            else
            {
                player.StopWorking();
            }
        }
        else
        {
            // move item to right hand
            player.SwapInventoryEquip(containerId, slotIndex, GlobalVar.equipmentRightHand);
        }
    }

    public override void InUse(Player player, int containerId, int slotIndex)
    {
        // security check, resource still usable
        if (!player.selectedElement)
        {
            // element deselected, stop working
            player.StopWorking();
            return;
        }
        else if (!(player.selectedElement.item.data is GatheringSourceItem))
        {
            // most probably we did not start with that target, stop working
            player.StopWorking();
            return;
        }
        GatheringSourceItem sourceItem = (GatheringSourceItem)player.selectedElement.item.data;
        GatheringLifePhase sourcePhase = sourceItem.lifePhases[player.selectedElement.item.data1];
        if (sourcePhase.itemsInResource == 0) //hasnoResource
        {
            // anybody emptied the resource meanwhile, remove from target and try again
            player.selectedElement = null;
            Use(player, containerId, slotIndex);
        }
        float distance = Vector3.Distance(player.transform.position, player.selectedElement.transform.position);
        if (distance > workDistance)
        {
            // walked out of range, stop working
            player.StopWorking();
            return;
        }
        if (!player.inventory.GetEquipment(slotIndex, out ItemSlot itemSlot))
        {
            // nothing in hand anymore, stop working
            player.StopWorking();
            return;
        }
        else
        {
            if (!(itemSlot.item.data is GatheringToolItem))
            {
                //other item in hand, stop working
                player.StopWorking();
                return;
            }
            else
            {
                // work long enough, resource has anything, correct tool, get the reward
                int playerSkillLevel = player.skills.LevelOfSkill(skillTool);
                float workTime = (float)NonLinearCurves.DoubleFromCurvePosition(GlobalVar.toolWorkingTimeNonlinear, player.skills.LevelOfSkill(skillTool), 0, 100, workTimeMax, workTimeMin);
                float skillFactorResource = NonLinearCurves.GetFloat0_1(GlobalVar.gatheringItemsInResourceCurve, playerSkillLevel + GlobalVar.gatheringItemsInResourceFitBestAt - sourcePhase.bestSkillLevel);
                //luck factor limited to avoid div/0
                float luckFactor = Mathf.Clamp(GlobalFunc.LuckFactor(player, luckPortionTool, luckMaxTool), GlobalVar.gatheringToolLuckMin, GlobalVar.gatheringToolLuckMax);
                float skillFactorTool = NonLinearCurves.GetFloat0_1(GlobalVar.toolMasteryNonlinear, playerSkillLevel + GlobalVar.toolMasteryFitBestAt - levelTool);
                float qualityFactor = NonLinearCurves.GetFloat0_1(GlobalVar.toolItemQualityNonlinear, itemSlot.item.quality);
                float attributeFactor = NonLinearCurves.GetFloat0_1(GlobalVar.fightAttributeNonlinear, player.attributes.CombinedAction(skillTool) * 5);
                float yieldProbability = 1f / sourcePhase.itemsInResource / skillFactorResource / luckFactor;
                //Debug.Log(string.Format(">>> Gathering: Get result after workTime: {0}s; skillFactorResource:{1}; luckFactor:{2}; skillFactorTool:{3}; qualityFactor:{4}; attributeFactor:{5} yieldProbability:{6}",
                //    workTime, skillFactorResource, luckFactor, skillFactorTool, qualityFactor, attributeFactor, yieldProbability));

                // nothing to get from resource anymore
                if (GlobalFunc.RandomLowerLimit0_1(yieldProbability))
                {
                    //Debug.Log(">>> yield");
                    //get next phase
                    int nextPhase;
                    if (GlobalFunc.RandomLowerLimit0_1(sourcePhase.nextPhaseUsedDefaultProbability))
                    {
                        nextPhase = sourcePhase.nextPhaseUsedDefault;
                    }
                    else
                    {
                        nextPhase = sourcePhase.nextPhaseUsedSpecial;
                    }
                    // destroy if no next phase exists
                    if (nextPhase < 0 || nextPhase >= sourceItem.lifePhases.Length)
                    {
                        Destroy(player.selectedElement.gameObject.gameObject);
                    }
                    else
                    {
                        // switch
                        sourcePhase = sourceItem.lifePhases[nextPhase];
                        GameTime gt = new GameTime();
                        int phaseEndTime = int.MaxValue;
                        if (sourcePhase.dayInPhase > 0)
                        {
                            phaseEndTime = gt.SecondsSinceStart() + (int)(GlobalFunc.RandomObfuscation(GlobalVar.gatheringResourcePhaseTimeObfuscation) * sourcePhase.dayInPhase * GlobalVar.vegetationCycleInSeconds);
                        }
                        player.selectedElement.SetItemData(nextPhase, phaseEndTime);
                        player.selectedElement.decayAfter = phaseEndTime;
                        player.selectedElementChangePhase(sourcePhase.modelPhase);
                    }

                    // deselect element and try other
                    player.selectedElement = null;
                    Use(player, containerId, slotIndex);
                }
                else
                {
                    // valid try
                    float voidFactor = (skillFactorTool + qualityFactor + attributeFactor) / 3;
                    //                    Debug.Log(">>> void tries >" + (1 - GlobalFunc.PositionFromProportion(voidFactor, sourcePhase.voidTriesMin, sourcePhase.voidTriesMax)).ToString());
                    if (GlobalFunc.RandomLowerLimit0_1(1 - GlobalFunc.ValueFromProportion(voidFactor, sourcePhase.voidTriesMin, sourcePhase.voidTriesMax)))
                    {
                        //                        Debug.Log(">>> find something");
                        if (GlobalFunc.RandomLowerLimit0_1(luckFactor * sourcePhase.probabilityExtraStuff))
                        {
                            // special gift
                            Debug.Log(">>> find special item");
                        }
                        else
                        {
                            // normal output
                            float totalProbability = 0;
                            foreach (GatheringContent contentItem in sourcePhase.content)
                            {
                                totalProbability += contentItem.probability;
                            }

                            float rnd = Random.Range(0, totalProbability);
                            float sumProb = 0;
                            foreach (GatheringContent contentItem in sourcePhase.content)
                            {
                                sumProb += contentItem.probability;
                                if (rnd <= sumProb)
                                {
                                    //this item
                                    if (contentItem.infoText.Length > 0)
                                    {
                                        player.Inform(contentItem.infoText);
                                    }
                                    // create item
                                    LogFile.WriteDebug(string.Format("Gathering: get {0} x {1} from {2}", contentItem.amount, contentItem.item.name, player.selectedElement.name));
                                    player.LearnSkill(skillTool, (levelTool + sourcePhase.bestSkillLevel) / 2, workTime);
                                    GlobalFunc.DegradeItem(player, GlobalVar.containerEquipment, itemSlot.slot, workTime);
                                    player.CmdAddItemToAvailableInventory(contentItem.item.name, contentItem.amount, contentItem.durability, contentItem.quality, "");
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        //                        Debug.Log(">>> void try");
                        // learn from failure and demolish your tool
                        workTime = workTime * GlobalVar.gatheringLearnFromFailure;
                        player.LearnSkill(skillTool, (levelTool + sourcePhase.bestSkillLevel) / 2, workTime);
                        GlobalFunc.DegradeItem(player, GlobalVar.containerEquipment, itemSlot.slot, workTime);
                    }
                    // show effect
                    if (sourcePhase.onUseEffect)
                    {
                        player.RpcUseAction(containerId, slotIndex, 1);
                    }

                    // next try
                    Use(player, containerId, slotIndex);
                }
            }
        }
    }

    public override void OnUseAction(Player player, int container, int slot, int action)
    {
        // show some debris
        GatheringSourceItem sourceItem = (GatheringSourceItem)player.selectedElement.item.data;
        GatheringLifePhase sourcePhase = sourceItem.lifePhases[player.selectedElement.item.data1];
        GameObject effect = sourcePhase.onUseEffect;
        Instantiate(effect, player.selectedElement.transform.position, effect.transform.rotation);
    }

    // can it be used as element
    public override bool CanUse(Player player, ElementSlot element)
    {
        return false;
    }
    // can it be used as inventory item
    public override bool CanUse(Player player, ItemSlot itemSlot)
    {
        // can't use if already working
        if (player.state == GlobalVar.stateWorking)
        {
            return false;
        }
        else
        {
            int playerSkillLevel = player.skills.LevelOfSkill(skillTool);
            return (playerSkillLevel + GlobalVar.toolMasteryFitBestAt > levelTool);
        }
    }

    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{TOOLSKILL}", Skills.Name(skillTool));
        tip.Replace("{TOOLLEVEL}", GlobalFunc.ExamineLimitText(levelTool, GlobalVar.skillLevelText));
        tip.Replace("{TOOLLUCKPORTION}", GlobalFunc.ExamineLimitText(luckPortionTool, GlobalVar.luckPortionText));
        tip.Replace("{TOOLLUCKMAX}", luckMaxTool.ToString());
        tip.Replace("{TOOLFREQUENCY}", GlobalFunc.ExamineLimitText(60f * 2 / (workTimeMin + workTimeMax), GlobalVar.hitsPerMinuteText));
        return tip.ToString();
    }

    // check all available resources
    private bool FindResource(Player player, out GatheringSourceItem sourceItem)
    {
        sourceItem = null;
        // is selected element resource
        if (player.selectedElement)
        {
            if (player.selectedElement.item.data is GatheringSourceItem)
            {
                sourceItem = (GatheringSourceItem)player.selectedElement.item.data;
            }
        }
        // search for other candidate
        // nothing proper selected
        if (sourceItem == null)
        {
            sourceItem = FindNearItem(player, workDistance, skillTool);
        }
        // or empty resource, or wrong skill for selected resource
        else
        {
            GatheringLifePhase currentPhase = sourceItem.lifePhases[player.selectedElement.item.data1];
            if (skillTool != currentPhase.gatheringSkill || currentPhase.itemsInResource == 0)
            {
                sourceItem = FindNearItem(player, workDistance, skillTool);
            }
        }
        if (sourceItem == null)
        {
            //nothing found
            player.Inform(string.Format("There is nothing around where you can get anything with your {0}.", displayName));
            return false;
        }
        return true;
    }

    // search around player for any usable resource
    private GatheringSourceItem FindNearItem(Player player, float distance, Skills.Skill usedSkill)
    {
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, distance, GlobalVar.layerMaskElement);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            // traverse down to find ElementSlot
            Transform t = hitColliders[i].transform;
            // this is a hack since I found no proper while exit and want to avoid a while(true)
            for (int j = 0; j < 20; j++)
            {
                ElementSlot hitElement = t.gameObject.GetComponent<ElementSlot>();
                if (hitElement)
                {
                    if (hitElement.item.data is GatheringSourceItem)
                    {
                        GatheringSourceItem sourceItem = (GatheringSourceItem)hitElement.item.data;
                        GatheringLifePhase currentPhase = sourceItem.lifePhases[hitElement.item.data1];

                        //has any resource and can be gathered by this skill
                        if (currentPhase.itemsInResource > 0 && currentPhase.gatheringSkill == usedSkill)
                        {
                            // switch and mark target
                            player.target = null;
                            player.selectedElement = hitElement;
                            player.SetIndicatorViaParent(hitElement.transform);
                            return sourceItem;
                        }
                    }
                }
                if (t.parent == null)
                    break;
                t = t.parent.transform;
            }
        }
        return null;
    }
}
