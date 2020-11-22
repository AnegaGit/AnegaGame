/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateNavMesh : MonoBehaviour
{
    public Transform allAreas;
    GameObject workingFolder;

    public void RecalculateNavMesh()
    {
        GlobalFunc.ClearLogConsole();
        CleanWorkingFolder();
        //here we collect all temporary objects
        workingFolder = new GameObject();
        workingFolder.name = GlobalVar.recalculateNavMeshWorkingFolder;

        int maxCount = 0;
        int count = 0;
        foreach (Transform area in allAreas)
        {
            foreach (Transform trTerrain in area.Find("Terrain"))
            {
                Terrain terrain = trTerrain.gameObject.GetComponent<Terrain>();
                if (terrain != null && trTerrain.name.Contains(area.name))
                {
                    maxCount++;
                }
            }
        }

        foreach (Transform area in allAreas)
        {
            foreach (Transform trTerrain in area.Find("Terrain"))
            {
                Terrain terrain = trTerrain.gameObject.GetComponent<Terrain>();
                if (terrain != null && trTerrain.name.Contains(area.name))
                {
                    EditorUtility.DisplayProgressBar("Please wait", string.Format("Working on terrain {0}.", terrain.name), 1f * count / maxCount);
                    count++;

                    CheckOneTerrain(terrain);
                }
            }
        }


        EditorUtility.ClearProgressBar();
        if (EditorUtility.DisplayDialog("Calculation finished", "You can now bake the NavMesh.\nPlease remove temporary data after baking.", "OK"))
        { }
    }

    private void CheckOneTerrain(Terrain terrain)
    {
        //position data of terrain
        int resolution = terrain.terrainData.heightmapResolution;
        float zeroX = terrain.transform.position.x;
        float zeroY = terrain.transform.position.y;
        float zeroZ = terrain.transform.position.z;
        float sizeX = terrain.terrainData.size.x;
        float sizeY = terrain.terrainData.size.y;
        float sizeZ = terrain.terrainData.size.z;

        //create terrain copy
        GameObject terrainObjectShallowWater = TerrainCopy(terrain, "__shallowWater", GlobalVar.navMeshAreaShallowWater);
        terrainObjectShallowWater.transform.position = terrain.transform.position;
        float[,] heightsShallowWater = new float[resolution, resolution];
        GameObject terrainObjectDeepWater = TerrainCopy(terrain, "__deepWater", GlobalVar.navMeshAreaDeepWater);
        terrainObjectDeepWater.transform.position = terrain.transform.position;
        float[,] heightsDeepWater = new float[resolution, resolution];
        GameObject terrainObjectRoad = TerrainCopy(terrain, "__road", GlobalVar.navMeshAreaRoad);
        terrainObjectRoad.transform.position = terrain.transform.position;
        float[,] heightsRoad = new float[resolution, resolution];

        // verify every mesh position
        for (int iX = 0; iX < resolution; iX++)
        {
            for (int iZ = 0; iZ < resolution; iZ++)
            {
                // where
                Vector3 currentPos = new Vector3(zeroX + sizeX * iX / (resolution - 1), 0, zeroZ + sizeZ * iZ / (resolution - 1));
                //Vector3 currentPos = new Vector3(164, 0, 182);
                currentPos.y = terrain.SampleHeight(currentPos);

                Vector3 rayStart = currentPos;
                rayStart.y += GlobalVar.navMeshRayLength;

                // set default height
                float defaultHeight = (currentPos.y- GlobalVar.recalculateNavMeshMoveDown) / sizeY;
//                float defaultHeight = (currentPos.y - zeroY) / sizeY;
                heightsShallowWater[iZ, iX] = defaultHeight;
                heightsDeepWater[iZ, iX] = defaultHeight;
                heightsRoad[iZ, iX] = defaultHeight;

                // find water over terrain
                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, GlobalVar.navMeshRayLength - GlobalVar.depthDeepWater, GlobalVar.layerMaskWater))
                {
                    //  heightsDeepWater[iZ, iX] = (currentPos.y - zeroY + GlobalVar.recalculateNavMeshMoveUp + GlobalVar.recalculateNavMeshMoveDown) / sizeY;
                    heightsDeepWater[iZ, iX] = (hit.point.y - GlobalVar.depthDeepWater) / sizeY;
                }
                else if (Physics.Raycast(rayStart, Vector3.down, GlobalVar.navMeshRayLength - GlobalVar.depthShallowWater, GlobalVar.layerMaskWater))
                {
                    heightsShallowWater[iZ, iX] = (currentPos.y + GlobalVar.recalculateNavMeshMoveUp ) / sizeY;
                }
                else if (iX < resolution - 1 && iZ < resolution - 1)
                {
                    // find most prominent texture
                    // get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
                    float[,,] splatmapData = terrain.terrainData.GetAlphamaps(iX, iZ, 1, 1);

                    // extract the 3D array data to a 1D array:
                    float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];
                    for (int n = 0; n < cellMix.Length; n++)
                    {
                        cellMix[n] = splatmapData[0, 0, n];
                    }

                    // loop through each mix value and find the maximum
                    float maxMix = 0;
                    int maxIndex = 0;
                    for (int n = 0; n < cellMix.Length; n++)
                    {
                        if (cellMix[n] > maxMix)
                        {
                            maxIndex = n;
                            maxMix = cellMix[n];
                        }
                    }
                    string layerName = terrain.terrainData.terrainLayers[maxIndex].name;

                    if (GlobalVar.navMeshRoadLayers.Contains(layerName))
                    {
                        heightsRoad[iZ, iX] = (currentPos.y + GlobalVar.recalculateNavMeshMoveUp) / sizeY;
                    }
                }
            }
        }
        terrainObjectShallowWater.GetComponent<Terrain>().terrainData.SetHeights(0, 0, heightsShallowWater);
        terrainObjectDeepWater.GetComponent<Terrain>().terrainData.SetHeights(0, 0, heightsDeepWater);
        terrainObjectRoad.GetComponent<Terrain>().terrainData.SetHeights(0, 0, heightsRoad);
    }

    public void CleanWorkingFolder()
    {
        //clean any temporary structure
        GameObject tmp = GameObject.Find(GlobalVar.recalculateNavMeshWorkingFolder);
        if (tmp)
        {
            DestroyImmediate(tmp);
        }
    }

    //create a terrain copy
    private GameObject TerrainCopy(Terrain terrain, string nameSuffix, int navMeshArea)
    {
        GameObject terrainCopy = Instantiate(terrain.gameObject, workingFolder.transform);
        terrainCopy.name = terrain.name + nameSuffix;
        GameObjectUtility.SetNavMeshArea(terrainCopy, navMeshArea);

        TerrainData terData = (TerrainData)Object.Instantiate(terrain.terrainData);
        terrainCopy.GetComponent<Terrain>().terrainData = terData;

        // transfer position
        Vector3 pos = terrainCopy.transform.position;
        terrainCopy.transform.position = pos;
        return terrainCopy;
    }

}
#endif