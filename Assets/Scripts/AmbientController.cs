/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AmbientController : NetworkBehaviour
{
    //>>> test only
    //public Transform testCapsule;

    private static Transform allAreas;
    private int totalActiveSize = 0;
    private struct VegetationRule
    {
        public GlobalVar.VegetationType vegetationType;
        public int totalFrequency;
        public List<GatheringSeedItem> seedItems;
    }
    private static List<VegetationRule> vegetationRules = new List<VegetationRule>();
    private Dictionary<string, int> seedCounter = new Dictionary<string, int>();
    private Dictionary<string, int> aliveCounter = new Dictionary<string, int>();

    public override void OnStartServer()
    {
        allAreas = Universal.AllAreas.transform;
        // this is available on the server only
        PrepareAreaMaps();
        InitializeSeedCounter();

        LoadSemistaticElements();
        //the start delay is necessary to make sure everything is initialized properly
        InvokeRepeating("RemoveDecayedDynamicElements", GlobalVar.cycleCheckStartDelay, GlobalVar.cycleCheckForDecay);
        InvokeRepeating("OrganizeSemistaticElements", GlobalVar.cycleCheckStartDelay, GlobalVar.cycleCheckSemistaticElements);
        InvokeRepeating("SaveSemistaticElements", GlobalVar.cycleSaveSemistaticElements, GlobalVar.cycleSaveSemistaticElements);
        //Double delay for seed, has to wait for Initialization
        Invoke("InitializeVegetationRules", GlobalVar.cycleCheckStartDelay);
        //>>>        InvokeRepeating("SeedNewVegetation", GlobalVar.cycleCheckStartDelay * 2, GlobalVar.cycleSeedVegetration);
        InvokeRepeating("ReportSeedCount", 3600, 3600);
    }

    void RemoveDecayedDynamicElements()
    {
        // find all elements in any area/DynamicElements and remove if timed out
        int currentSeconds = GameTime.SecondsSinceZero();
        foreach (Transform area in allAreas)
        {
            Transform dynamicElements = area.Find("DynamicElements");
            foreach (Transform element in dynamicElements)
            {
                ElementSlot elementSlot = element.gameObject.GetComponent<ElementSlot>();
                if (elementSlot)
                {
                    if (elementSlot.decayAfter < currentSeconds)
                    {
                        Destroy(element.gameObject.gameObject);
                    }
                }
            }
        }
    }

    void OrganizeSemistaticElements()
    {
        // find all elements in any area/SemistaticElements and act acording item type
        int currentSec = GameTime.SecondsSinceZero();
        foreach (Transform area in allAreas)
        {
            Transform dynamicElements = area.Find("SemistaticElements");
            foreach (Transform element in dynamicElements)
            {
                ElementSlot elementSlot = element.gameObject.GetComponent<ElementSlot>();
                if (elementSlot)
                {
                    // gathering source
                    if (elementSlot.item.data is GatheringSourceItem)
                    {
                        GatheringSourceItem gatheringSourceItem = (GatheringSourceItem)elementSlot.item.data;
                        // switch phase necessary
                        if (elementSlot.decayAfter < currentSec)
                        {
                            bool foundPlayer = false;
                            // only if no player aound
                            if (gatheringSourceItem.changeInvisibleOnly)
                            {
                                foundPlayer = Universal.PlayerInRange(element.transform.position, GlobalVar.ambientActionDistanceToPlayer);
                            }
                            // no player or switch permitted in sight
                            if (!foundPlayer)
                            {
                                GatheringLifePhase currentPhase = gatheringSourceItem.lifePhases[elementSlot.item.data1];
                                int nextPhase = currentPhase.nextPhaseTimeoutDefault;
                                if (!GlobalFunc.RandomLowerLimit0_1(currentPhase.nextPhaseTimeoutDefaultProbability))
                                {
                                    nextPhase = currentPhase.nextPhaseTimeoutSpecial;
                                }
                                // next phase exist
                                if (nextPhase >= 0 && nextPhase < gatheringSourceItem.lifePhases.Length)
                                {
                                    // switch to next phase
                                    currentPhase = gatheringSourceItem.lifePhases[nextPhase];
                                    GameTime gt = new GameTime();
                                    int phaseEndTime = int.MaxValue;
                                    if (currentPhase.dayInPhase > 0)
                                    {
                                        phaseEndTime = gt.SecondsSinceStart() + (int)(GlobalFunc.RandomObfuscation(GlobalVar.gatheringResourcePhaseTimeObfuscation) * currentPhase.dayInPhase * GlobalVar.vegetationCycleInSeconds);
                                    }
                                    elementSlot.SetItemData(nextPhase, phaseEndTime);
                                    elementSlot.decayAfter = phaseEndTime;
                                    // change display for phased elements on server and each client
                                    PhasedElement[] phaseElements = elementSlot.transform.GetComponentsInChildren<PhasedElement>();
                                    if (phaseElements.Length > 0)
                                    {
                                        phaseElements[0].phase = currentPhase.modelPhase;
                                    }
                                    RpcNextPhase(elementSlot.netIdentity, currentPhase.modelPhase);
                                }
                                else
                                {
                                    Destroy(element.gameObject.gameObject);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    [ClientRpc]
    public void RpcNextPhase(NetworkIdentity ni, int modelPhase)
    {
        PhasedElement[] phaseElements = ni.transform.GetComponentsInChildren<PhasedElement>();
        if (phaseElements.Length > 0)
        {
            phaseElements[0].phase = modelPhase;
        }
    }

    // Save and load current state
    void SaveSemistaticElements()
    {
        // empty docu counter
        foreach (string key in aliveCounter.Keys.ToList())
        {
            aliveCounter[key] = 0;
        }
        int countElements = 0;
        int countAreas = 0;

        foreach (Transform area in allAreas)
        {
            AmbientsElemensList elementsInArea = new AmbientsElemensList();
            Transform dynamicElements = area.Find("SemistaticElements");
            bool areaWithElements = false;
            foreach (Transform element in dynamicElements)
            {
                ElementSlot elementSlot = element.gameObject.GetComponent<ElementSlot>();
                if (elementSlot)
                {
                    AmbientElement ambientElement = new AmbientElement();
                    ambientElement.n = element.name;
                    ambientElement.sn = elementSlot.displayName;
                    ambientElement.px = element.position.x;
                    ambientElement.py = element.position.y;
                    ambientElement.pz = element.position.z;
                    ambientElement.rx = element.rotation.eulerAngles.x;
                    ambientElement.ry = element.rotation.eulerAngles.y;
                    ambientElement.rz = element.rotation.eulerAngles.z;
                    ambientElement.sx = element.localScale.x;
                    ambientElement.sy = element.localScale.y;
                    ambientElement.sz = element.localScale.z;
                    ambientElement.da = elementSlot.decayAfter;
                    ambientElement.i = elementSlot.item.itemName;
                    ambientElement.d1 = elementSlot.item.data1;
                    ambientElement.d2 = elementSlot.item.data2;
                    ambientElement.d3 = elementSlot.item.data3;
                    ambientElement.id = elementSlot.item.durability;
                    ambientElement.iq = elementSlot.item.quality;

                    // now special parameter
                    if (elementSlot.item.data is GatheringSourceItem)
                    {
                        aliveCounter[elementSlot.item.itemName] = aliveCounter[elementSlot.item.itemName] + 1;
                    }

                    elementsInArea.elements.Add(ambientElement);
                    areaWithElements = true;
                    countElements++;
                }
            }

            if (areaWithElements)
            {
                countAreas++;
            }
            string jsonText = JsonUtility.ToJson(elementsInArea);
            System.IO.File.WriteAllText(string.Format(GlobalVar.semistaticItemsFile, GlobalVar.gameDevDir, area.name), jsonText);
        }
        LogFile.WriteLog(LogFile.LogLevel.Server, string.Format("{0} Semistatic elements in {1} areas saved", countElements, countAreas));
    }

    void LoadSemistaticElements()
    {
        int countAreas = 0;
        int countItems = 0;

        foreach (Transform area in allAreas)
        {
            bool areaToCount = false;
            // remove any element from game
            Transform dynamicElements = area.Find("SemistaticElements");
            foreach (Transform element in dynamicElements)
            {
                Destroy(element.gameObject);
            }

            // load saved data
            AmbientsElemensList elementsInArea = new AmbientsElemensList();
            string jsonText = "";
            try
            {
                jsonText = System.IO.File.ReadAllText(string.Format(GlobalVar.semistaticItemsFile, GlobalVar.gameDevDir, area.name));
            }
            catch
            {
                LogFile.WriteLog(LogFile.LogLevel.Error, string.Format("File {0} missing. No environment loaded for area {1}.", string.Format(GlobalVar.semistaticItemsFile, GlobalVar.gameDevDir, area.name), area.name));
                continue;
            }
            elementsInArea = JsonUtility.FromJson<AmbientsElemensList>(jsonText);

            foreach (AmbientElement ambientElement in elementsInArea.elements)
            {
                areaToCount = true;
                countItems++;
                // crete element
                CreateAmbientElement(ambientElement);
            }
            countAreas += (areaToCount ? 1 : 0);

        }
        LogFile.WriteLog(LogFile.LogLevel.Info, string.Format("Vegetation loaded: {0} items in {1} areas", countItems, countAreas));
    }

    public static void CreateAmbientElement(AmbientElement ambientElement)
    {
        ScriptableItem itemData;
        if (ScriptableItem.dict.TryGetValue(ambientElement.i.GetStableHashCode(), out itemData))
        {
            Vector3 position = new Vector3(ambientElement.px, ambientElement.py, ambientElement.pz);
            Quaternion rotation = Quaternion.Euler(ambientElement.rx, ambientElement.ry, ambientElement.rz);
            GameObject element = Instantiate(Universal.PrefabStaticElement, position, rotation);
            Vector3 creationScale = new Vector3(ambientElement.sx, ambientElement.sy, ambientElement.sz);
            element.transform.localScale = creationScale;

            string area = Universal.GetArea(position);
            element.transform.SetParent(allAreas.Find(area + "/SemistaticElements"));
            element.name = ambientElement.n;

            Item itemToCreate = new Item(itemData);
            ElementSlot es = element.GetComponent<ElementSlot>();
            es.item = itemToCreate;
            es.isStatic = true;
            es.applyToGround = false;
            es.specialName = ambientElement.sn;
            es.item.data.displayName = ambientElement.sn;
            es.decayAfter = ambientElement.da;
            es.SetItemData(ambientElement.d1, ambientElement.d2, ambientElement.d3);
            es.SetItemDurability(ambientElement.id);
            es.SetItemQuality(ambientElement.iq);

            UsableItem usableItem = (UsableItem)itemToCreate.data;

            GameObject model;
            if (usableItem.modelPrefab)
            {
                model = Instantiate(usableItem.modelPrefab, element.transform);
                model.transform.SetParent(element.transform, true);
                model.name = "Model";

                if (itemData is GatheringSourceItem)
                {
                    GatheringSourceItem gts = (GatheringSourceItem)itemData;
                    //set initial phase for phased element
                    PhasedElement phaseElement = model.gameObject.GetComponent<PhasedElement>();
                    if (phaseElement)
                    {
                        phaseElement.phase = gts.lifePhases[ambientElement.d1].modelPhase;
                    }
                }
            }
            NetworkServer.Spawn(element);
        }
    }

    // report vegetation
    void InitializeSeedCounter()
    {
        seedCounter.Clear();
        foreach (KeyValuePair<int, ScriptableItem> kvp in ScriptableItem.dict)
        {
            if (kvp.Value is GatheringSourceItem)
            {
                seedCounter.Add(kvp.Value.name, 0);
                aliveCounter.Add(kvp.Value.name, 0);
            }
        }
    }
    void ReportSeedCount()
    {
        string text = "Report vegetation count:" + Environment.NewLine;
        string textSeed = "Seed";
        string textTotal = "Total";

        foreach (string key in seedCounter.Keys.ToList())
        {
            text += ";" + key;
            textSeed = ";" + seedCounter[key].ToString();
            textTotal = ";" + aliveCounter[key].ToString();
            seedCounter[key] = 0;
        }
        LogFile.WriteLog(LogFile.LogLevel.Info, text + Environment.NewLine + textSeed + Environment.NewLine + textTotal);
    }

    // ambient organization
    void PrepareAreaMaps()
    {
        foreach (KeyValuePair<string, AreaInfo> kvp in Universal.areaInfos)
        {
            if (kvp.Value.hasVegetationMap)
            {
                totalActiveSize += kvp.Value.widthX * kvp.Value.widthZ / GlobalVar.totalSizeCompression;
            }
        }
    }

    void InitializeVegetationRules()
    {
        vegetationRules.Clear();
        int countUsedVegetation = 0;
        int countRules = 0;
        int countItems = 0;
        int countVegetationType = 0;
        foreach (GlobalVar.VegetationType vegetationType in Enum.GetValues(typeof(GlobalVar.VegetationType)))
        {
            if (vegetationType != GlobalVar.VegetationType.NoVegetation)
            {
                try
                {
                    countVegetationType++;
                    int toatalFrequency = 0;
                    List<GatheringSeedItem> seedItems = new List<GatheringSeedItem>();
                    //>>> fix later! use all gathering source items, allVegetation items run an exception on server
                    foreach (GatheringSourceItem sourceItem in Universal.GatheringSourceItems)
                    {
                        foreach (GatheringSeedRegion seedRegion in sourceItem.seedActions)
                        {
                            if (seedRegion.VegetationType == vegetationType)
                            {
                                GatheringSeedItem seedItem = new GatheringSeedItem();
                                seedItem.itemName = sourceItem.name;
                                seedItem.sizeMin = seedRegion.sizeMin;
                                seedItem.sizeMax = seedRegion.sizeMax;
                                seedItem.daysInPhase0 = sourceItem.lifePhases[0].dayInPhase;
                                seedItem.rules = new List<GatheringSeedRule>();
                                seedItem.rules = seedRegion.rules.ToList();
                                seedItem.frequency = seedRegion.frequency;
                                seedItems.Add(seedItem);

                                countItems++;
                                countRules += seedItem.rules.Count;
                                toatalFrequency += seedRegion.frequency;
                            }
                        }
                    }
                    if (seedItems.Count > 0)
                    {
                        VegetationRule vegetationRule = new VegetationRule();
                        vegetationRule.vegetationType = vegetationType;
                        vegetationRule.totalFrequency = toatalFrequency;
                        vegetationRule.seedItems = seedItems;
                        vegetationRules.Add(vegetationRule);
                        countUsedVegetation++;
                    }
                }
                catch
                {
                    LogFile.WriteLog(LogFile.LogLevel.Error, "Not correct initialization of vegetation rules for " + vegetationType);
                }
            }
        }
        LogFile.WriteLog(LogFile.LogLevel.Info, string.Format("Vegetation rules created: {1} out of {0} vegetation types with {2} item rule sets", countVegetationType, countUsedVegetation, countItems));
    }

    void SeedNewVegetation()
    {
        // find a position first
        int currentSelected = UnityEngine.Random.Range(0, totalActiveSize);
        int currentSize = 0;
        foreach (KeyValuePair<string, AreaInfo> kvp in Universal.areaInfos)
        {
            if (kvp.Value.hasVegetationMap)
            {
                currentSize += (int)(kvp.Value.widthX * kvp.Value.widthZ / GlobalVar.totalSizeCompression);
                if (currentSelected <= currentSize)
                {
                    int seedTry = 0;
                    while (seedTry < GlobalVar.triesPerSeedCycle)
                    {
                        seedTry++;
                        // get terrain at random position
                        Vector3 pos = new Vector3(kvp.Value.zeroX + UnityEngine.Random.Range(0.0f, kvp.Value.widthX), 0, kvp.Value.zeroZ + UnityEngine.Random.Range(0.0f, kvp.Value.widthZ));
                        string tName = string.Format("{0}_{1}{2}"
                            , kvp.Key
                            , (int)(((int)pos.x - kvp.Value.zeroX) / kvp.Value.sizeOne)
                            , (int)(((int)pos.z - kvp.Value.zeroZ) / kvp.Value.sizeOne));

                        Transform trTerrain = allAreas.Find(kvp.Key + "/Terrain/" + tName);
                        if (trTerrain)
                        {
                            Terrain terrain = trTerrain.gameObject.GetComponent<Terrain>();
                            pos.y = terrain.SampleHeight(pos) + trTerrain.position.y;

                            // not under water
                            Vector3 posOverPos = pos + new Vector3(0, GlobalVar.waterDeepSeedMax + GlobalVar.waterShallow, 0);
                            if (!Physics.Raycast(posOverPos, Vector3.down, GlobalVar.waterDeepSeedMax, GlobalVar.layerMaskWater))
                            {

                                GlobalVar.VegetationType vegetationType = GetAmbientVegetation(pos);
                                if (vegetationType != GlobalVar.VegetationType.NoVegetation)
                                {
                                    // there is a rock over the terrain
                                    posOverPos = pos + new Vector3(0, GlobalVar.rockSizeMax, 0);
                                    if (Physics.Raycast(posOverPos, Vector3.down, out RaycastHit hit, GlobalVar.rockSizeMax, GlobalVar.layerMaskTerrainAdd))
                                    {
                                        pos = hit.point;
                                        vegetationType = GlobalVar.VegetationType.RockyMountain;
                                    }

                                    //                        Debug.Log(">>> try to seed in " + kvp.Key + " at " + pos.ToString() + " for " + vegetationType);
                                    VegetationRule vegetationRule = vegetationRules.Find(x => x.vegetationType == vegetationType);
                                    if (vegetationRule.totalFrequency == 0)
                                    {
                                        return;
                                    }

                                    if (Universal.PlayerInRange(pos, GlobalVar.ambientActionDistanceToPlayer))
                                    {
                                        // don't seed anything near a player
                                        return;
                                    }

                                    // now we can seed any item fom the list
                                    int currentRandom = UnityEngine.Random.Range(0, vegetationRule.totalFrequency);
                                    int currentFrequency = 0;
                                    //>>> test only
                                    //testCapsule.position = pos; //>>> remove later

                                    foreach (GatheringSeedItem seedItem in vegetationRule.seedItems)
                                    {
                                        currentFrequency += seedItem.frequency;
                                        if (currentFrequency > currentRandom)
                                        {
                                            bool allRulesValid = true;
                                            //test this special seed Item
                                            foreach (GatheringSeedRule rule in seedItem.rules)
                                            {
                                                int countInRange = 0;
                                                List<int> foundSlots = new List<int>();

                                                Collider[] hitColliders = Physics.OverlapSphere(pos, rule.maxDistance, GlobalVar.layerMaskElement);
                                                for (int i = 0; i < hitColliders.Length; i++)
                                                {
                                                    // traverse down to find ElementSlot
                                                    Transform t = hitColliders[i].transform;
                                                    // this is to end the search if a very deep game object was hit
                                                    for (int j = 0; j < 20; j++)
                                                    {
                                                        ElementSlot hitElement = t.gameObject.GetComponent<ElementSlot>();
                                                        if (hitElement)
                                                        {
                                                            // do not process elements twice
                                                            if (!foundSlots.Contains(hitElement.GetInstanceID()))
                                                            {
                                                                foundSlots.Add(hitElement.GetInstanceID());
                                                                // is it in this rule
                                                                foreach (GatheringSourceItem sourceItem in rule.relatedItems.items)
                                                                {
                                                                    if (sourceItem.name == hitElement.item.itemName)
                                                                    {
                                                                        if (GlobalFunc.IsInRange(Vector3.Distance(pos, hitElement.transform.position), rule.minDistance, rule.maxDistance))
                                                                        {
                                                                            countInRange++;
                                                                        }
                                                                        // found iten, can leave
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        if (t.parent == null)
                                                            break;
                                                        t = t.parent.transform;
                                                    }
                                                }
                                                // can we apply this rule?
                                                if (!GlobalFunc.IsInRange(countInRange, rule.minAmount, rule.maxAmount))
                                                {
                                                    allRulesValid = false;
                                                    break;
                                                }
                                            }

                                            //everything is allowed
                                            if (allRulesValid)
                                            {
                                                //Debug.Log(">>> seed " + seedItem.itemName + " in " + kvp.Key + " at " + pos.ToString() + " for " + vegetationType);
                                                seedCounter[seedItem.itemName] = seedCounter[seedItem.itemName] + 1;
                                                // build element
                                                AmbientElement ambientElement = new AmbientElement();
                                                ambientElement.n = seedItem.itemName + "-0-" + GameTime.SecondsSinceZero().ToString();
                                                ambientElement.sn = "";
                                                ambientElement.px = pos.x;
                                                ambientElement.py = pos.y;
                                                ambientElement.pz = pos.z;
                                                ambientElement.rx = 0f;
                                                ambientElement.ry = UnityEngine.Random.value * 360f;
                                                ambientElement.rz = 0F;
                                                float size = GlobalFunc.RandomInRange(seedItem.sizeMin, seedItem.sizeMax);
                                                ambientElement.sx = size;
                                                ambientElement.sy = size;
                                                ambientElement.sz = size;
                                                float days = seedItem.daysInPhase0;
                                                int phaseEndTime = int.MaxValue;
                                                if (days > 0)
                                                {
                                                    phaseEndTime = GameTime.SecondsSinceZero() + (int)(GlobalFunc.RandomObfuscation(GlobalVar.gatheringResourcePhaseTimeObfuscation) * days * GlobalVar.vegetationCycleInSeconds);
                                                }
                                                ambientElement.da = phaseEndTime;
                                                ambientElement.i = seedItem.itemName;
                                                ambientElement.d1 = 0;
                                                ambientElement.d2 = phaseEndTime;
                                                ambientElement.d3 = 0;
                                                ambientElement.id = GlobalVar.defaultDurability;
                                                ambientElement.iq = GlobalVar.defaultQuality;
                                                ambientElement.tr = "";
                                                ambientElement.ti = "";

                                                CreateAmbientElement(ambientElement);
                                            }
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    static GlobalVar.VegetationType GetAmbientVegetation(Vector3 position)
    {
        string currentArea = Universal.GetArea(position);
        if (Universal.areaInfos.TryGetValue(currentArea, out AreaInfo currentAreaInfo))
        {
            if (currentAreaInfo.hasVegetationMap)
            {
                int posX = (int)position.x - currentAreaInfo.zeroX;
                int posZ = (int)position.z - currentAreaInfo.zeroZ;
                if (GlobalFunc.IsInRange(posX, 0, currentAreaInfo.widthX) && GlobalFunc.IsInRange(posZ, 0, currentAreaInfo.widthZ))
                {
                    int i = (int)(currentAreaInfo.vegetationMap.GetPixel(posX, posZ).r * 63);
                    GlobalVar.VegetationType vt = (GlobalVar.VegetationType)i;
                    if (Enum.IsDefined(typeof(GlobalVar.VegetationType), vt))
                    {
                        return vt;
                    }
                }
            }
        }
        return GlobalVar.VegetationType.NoVegetation;
    }


    public static void ShowAmbientPositionToPlayer(Vector3 position, Player player)
    {
        player.Inform(string.Format("{0} at {3} (map x:y {1}:{2})", GetAmbientVegetation(position), (int)position.x, (int)position.z,position.ToString()));
    }
}

// class for save and load
[Serializable]
public class AmbientElement
{
    public string n; // name
    public string sn; // special name
    public float px, py, pz; // position
    public float rx, ry, rz; // euler rotation
    public float sx, sy, sz; // size
    public int da; // decay after
    public string i; // item name
    public int d1, d2, d3; // item data 
    public int id, iq; // item durability, quality
    public string tr, ti; // text can read / illiterate
}

public class AmbientsElemensList
{
    public List<AmbientElement> elements = new List<AmbientElement>();
}

