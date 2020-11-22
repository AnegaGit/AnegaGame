/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEngine.AI;
using System;
using System.Linq;
using System.Collections.Generic;
using UMA;
public class Universal : MonoBehaviour
{
    private static string logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Anega";
    private static string logFileName = logFilePath + "\\" + DateTime.UtcNow.ToString("yyyymmdd") + "anega.log";

    private static GameObject _emptyElementPrefab;
    public GameObject emptyElementPrefab;
    private static GameObject _toolTipPrefab;
    public GameObject toolTipPrefab;
    private static GameObject[] _localEffects;
    public GameObject[] localEffects;
    private static GameObject _effectBloodShed;
    public GameObject effectBloodShed;
    private static PanelLoading _panelLoading;
    public PanelLoading panelLoading;
    private static UMATextRecipe _hideUmaBody;
    public UMATextRecipe hideUmaBody;
    private static GatheringSourceItem[] _gatheringSourceItems;
    public GatheringSourceItem[] gatheringSourceItems;
    private static GameObject _prefabStaticElement;
    public GameObject prefabStaticElement;
    private static GameObject _allAreas;
    public GameObject allAreas;
    [HideInInspector] public static Dictionary<string, AreaInfo> areaInfos;
    private static List<GameObject> _allPortals;
    private static List<GameObject> _allSpawns;
    public GameObject newPlayerSpawn;
    private static GameObject _newPlayerSpawn;
    public GameObject defaultSpawn;
    private static GameObject _defaultSpawn;

    // initializes general parameter etc.
    // run first in runtime
    void Awake()
    {
        // Initialize non linear curves
        GameTime gt = new GameTime();
        LogFile.WriteLog(LogFile.LogLevel.Always, string.Format("Anega started. Local time:{1} Game time:{0}", gt.DateTimeString, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        NonLinearCurves.LoadParameter();
        PlayerPreferences.LoadParameter();
        _emptyElementPrefab = emptyElementPrefab;
        _toolTipPrefab = toolTipPrefab;
        _localEffects = localEffects;
        _effectBloodShed = effectBloodShed;
        _panelLoading = panelLoading;
        _hideUmaBody = hideUmaBody;
        _gatheringSourceItems = gatheringSourceItems;
        _prefabStaticElement = prefabStaticElement;
        _allAreas = allAreas;
        _newPlayerSpawn = newPlayerSpawn;
        _defaultSpawn = defaultSpawn;

        //find static elements
        _allPortals = new List<GameObject>();
        _allSpawns = new List<GameObject>();
        foreach (Transform area in allAreas.transform)
        {
            Transform staticElements = area.Find("StaticElements");
            foreach (Transform element in staticElements)
            {
                ElementSlot elementSlot = element.gameObject.GetComponent<ElementSlot>();
                if (elementSlot)
                {
                    if (elementSlot.item.data is PortalItem)
                    {
                        _allPortals.Add(element.gameObject);
                    }
                }
                SpawnElement spawnElement = element.gameObject.GetComponent<SpawnElement>();
                if (spawnElement)
                {
                    if (!element.gameObject.Equals(_newPlayerSpawn))
                    {
                        _allSpawns.Add(element.gameObject);
                    }
                }
            }
        }

        areaInfos = new Dictionary<string, AreaInfo>();
        LoadAreaInfos();
    }

    public static GameObject EmptyElementPrefab { get { return _emptyElementPrefab; } }
    public static GameObject ToolTipPrefab { get { return _toolTipPrefab; } }
    public static GameObject LocalEffect(int effectNumber)
    {
        if (effectNumber >= _localEffects.Length)
        {
            effectNumber = 0;
        }
        return _localEffects[effectNumber];
    }
    public static GameObject EffectBloodShed { get { return _effectBloodShed; } }
    public static PanelLoading LoadingPanel { get { return _panelLoading; } }
    public static UMATextRecipe RecipeHideBody { get { return _hideUmaBody; } }
    public static GatheringSourceItem[] GatheringSourceItems { get { return _gatheringSourceItems; } }
    public static GameObject PrefabStaticElement { get { return _prefabStaticElement; } }
    public static GameObject AllAreas { get { return _allAreas; } }
    public static List<GameObject> AllPortals { get { return _allPortals; } }
    public static List<GameObject> AllSpawns { get { return _allSpawns; } }
    public static Transform NewPlayerSpawn { get { return _newPlayerSpawn.GetComponent<SpawnElement>().RandomPosition(); } }
    public static Transform DefaultSpawn { get { return _defaultSpawn.GetComponent<SpawnElement>().RandomPosition(); } }


    // collection of MonoBehaviour functions /////////////////////////////////////////////////////////////////////
    // cannot be in GlobalFunc since it has no UnityEngine

    /// <summary>
    /// Privater function to prepare area infos
    /// </summary>
    private void LoadAreaInfos()
    {
        foreach (Transform area in _allAreas.transform)
        {
            string areaName = area.name;
            string displayName = areaName;
            bool skyIsVisible = true;
            AreaProperties areaProperties = area.GetComponent<AreaProperties>();
            if (areaProperties)
            {
                if (areaProperties.displayName.Length > 0)
                {
                    displayName = areaProperties.displayName;
                    skyIsVisible = areaProperties.skyIsVisible;
                }
            }
            // try loading resources
            bool hasVegetationMap = false;
            Texture2D vegetationMap = Resources.Load<Texture2D>("AmbientStructure/Vegetation_" + areaName);
            if (vegetationMap)
            {
                hasVegetationMap = true;
            }
            bool hasMagicMap = false;
            Texture2D magicMap = Resources.Load<Texture2D>("AmbientStructure/Magic_" + areaName);
            if (magicMap)
            {
                hasMagicMap = true;
            }

            // get area dimensions from all terrains
            int posxMin = int.MaxValue;
            int posyMin = int.MaxValue;
            int poszMin = int.MaxValue;
            int posxMax = int.MinValue;
            int posyMax = int.MinValue;
            int poszMax = int.MinValue;
            int tSize = 0;

            foreach (Transform trTerrain in area.Find("Terrain"))
            {
                Terrain terrain = trTerrain.gameObject.GetComponent<Terrain>();
                if (terrain != null && trTerrain.name.Contains(areaName))
                {
                    int tPosxMin = (int)terrain.transform.position.x;
                    int tPosyMin = (int)terrain.transform.position.y;
                    int tPoszMin = (int)terrain.transform.position.z;
                    int tPosxMax = tPosxMin + (int)terrain.terrainData.size.x;
                    int tPosyMax = tPosxMin + (int)terrain.terrainData.size.y;
                    int tPoszMax = tPoszMin + (int)terrain.terrainData.size.z;
                    posxMin = Mathf.Min(posxMin, tPosxMin);
                    posyMin = Mathf.Min(posyMin, tPosyMin);
                    poszMin = Mathf.Min(poszMin, tPoszMin);
                    posxMax = Mathf.Max(posxMax, tPosxMax);
                    posyMax = Mathf.Max(posyMax, tPosyMax);
                    poszMax = Mathf.Max(poszMax, tPoszMax);
                    tSize = (int)terrain.terrainData.size.x;
                }
            }
            areaInfos.Add(areaName, new AreaInfo(displayName, skyIsVisible, posxMin, posyMin, poszMin, posxMax - posxMin, posyMax - posyMin, poszMax - poszMin, tSize, hasVegetationMap, vegetationMap, hasMagicMap, magicMap));

            if (posxMax > int.MinValue)
            {

                //>>> nust still be done in ambient!!!   totalActiveSize += (posxMax - posxMin) * (poszMax - poszMin) / GlobalVar.totalSizeCompression;

                // check for correct names
                foreach (Transform trTerrain in area.Find("Terrain"))
                {
                    Terrain terrain = trTerrain.gameObject.GetComponent<Terrain>();
                    if (terrain != null && trTerrain.name.Contains(areaName))
                    {
                        int xZero = (int)(((int)terrain.transform.position.x - posxMin) / tSize);
                        int zZero = (int)(((int)terrain.transform.position.z - poszMin) / tSize);
                        if (trTerrain.name != string.Format("{0}_{1}{2}", areaName, xZero, zZero))
                        {
                            LogFile.WriteLog(LogFile.LogLevel.Error, string.Format("Terrain {0} has wrong name. It should be '{1}'", trTerrain.name, string.Format("{0}_{1}{2}", areaName, xZero, zZero)));
                        }
                    }
                }
            }
            else
            {
                if (areaName != "Example")
                {
                    LogFile.WriteLog(LogFile.LogLevel.Error, string.Format("Area {0} does not have a correct named terrain. It must contain a terrain with the name '{0}'", areaName));
                }
            }
        }
    }

    /// <summary>
    /// Find a position on NavMesh close to the target
    /// </summary>
    public static Vector3 FindPossiblePosition(Vector3 startPosition, float distance)
    {
        // get closest position on navmesh
        Vector3 targetPos;
        for (int i = 0; i < 30; i++)
        {
            //Random position on a circle
            Vector3 direction = UnityEngine.Random.rotation.eulerAngles;
            direction.y = 0;
            direction.Normalize();
            targetPos = startPosition + direction * distance * i / 30;

            //get NavMesh position
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPos, out hit, 2.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        // nothing there take center
        return startPosition;
    }

    /// <summary>
    /// Find a position on NavMesh in a circle around
    /// </summary>
    public static Vector3 FindPossiblePositionAround(Vector3 startPosition, float distance)
    {
        Vector3 targetPos;
        for (int i = 0; i < 30; i++)
        {
            //Random position on a circle
            Vector3 direction = UnityEngine.Random.rotation.eulerAngles;
            direction.y = 0;
            direction.Normalize();
            targetPos = startPosition + direction * distance;

            //get NavMesh position
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPos, out hit, 2.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        // nothing there take center
        return startPosition;
    }
    /// <summary>
    /// Find the position in NavMesh direct under or over the position
    /// </summary>
    public static Vector3 FindClosestNavMeshPosition(Vector3 startPosition, float distance)
    {
        // get closest position on navmesh
        NavMeshHit hit;

        if (NavMesh.SamplePosition(startPosition, out hit, distance, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // nothing there take center
        return startPosition;
    }

    /// <summary>
    /// Find area acording to position
    /// </summary>
    public static string GetArea(Vector3 searchPos)
    {
        foreach (KeyValuePair<string, AreaInfo> kvp in areaInfos)
        {
            if (
                    (searchPos.x >= kvp.Value.zeroX)
                    && (searchPos.x < kvp.Value.zeroX + kvp.Value.widthX)
                    && (searchPos.y >= kvp.Value.zeroY)
                    && (searchPos.y < kvp.Value.zeroY + kvp.Value.widthY + 10)
                    && (searchPos.z >= kvp.Value.zeroZ)
                    && (searchPos.z < kvp.Value.zeroZ + kvp.Value.widthZ)
                )
            {
                return kvp.Key;
            }
        }
        return ("OutOfGame");
    }

    /// <summary>
    /// Get the related area property
    /// </summary>
    public static bool GetAreaSky(string area)
    {
        Transform trArea = _allAreas.transform.Find(area);
        if (trArea)
        {
            AreaProperties properties = trArea.gameObject.GetComponent<AreaProperties>();
            return properties.skyIsVisible;
        }
        return true;
    }

    /// <summary>
    /// Change layer of the game object and all children
    /// </summary>
    public static void ChangeLayers(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
        {
            ChangeLayers(child.gameObject, layer);
        }
    }

    /// <summary>
    /// returns true if there is any player around position in range
    /// </summary>
    public static bool PlayerInRange(Vector3 position, float range)
    {
        List<Player> players = Player.onlinePlayers.Values.ToList();
        foreach (Player player in players)
        {
            if (Vector3.Distance(position, player.transform.position) < range)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns strength of ambient mana in range 0-1
    /// </summary>
    public static float GetAmbientMana(Vector3 position, string currentArea = "")
    {
        if (currentArea.Length == 0)
        {
            currentArea = Universal.GetArea(position);
        }
        if (Universal.areaInfos.TryGetValue(currentArea, out AreaInfo currentAreaInfo))
        {
            if (currentAreaInfo.hasMagicMap)
            {
                int posX = (int)position.x - currentAreaInfo.zeroX;
                int posZ = (int)position.z - currentAreaInfo.zeroZ;
                if (GlobalFunc.IsInRange(posX, 0, currentAreaInfo.widthX) && GlobalFunc.IsInRange(posZ, 0, currentAreaInfo.widthZ))
                {
                    return currentAreaInfo.magicMap.GetPixel(posX, posZ).b;
                }
            }
        }
        return 0.5f;
    }

    public static Transform NewPlayerSpawnPoint()
    {
        return NewPlayerSpawn.GetComponent<SpawnElement>().RandomPosition();
    }
}

/// <summary>
/// Often needed list structs
/// </summary>
[Serializable]
public struct StringFloat
{
    public string text;
    public float value;
}
[Serializable]
public struct StringFloatRange
{
    public string text;
    public float min;
    public float max;
}
[Serializable]
public struct StringColor
{
    public string text;
    public Color value;
}
