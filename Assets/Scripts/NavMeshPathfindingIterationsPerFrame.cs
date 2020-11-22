/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Unity only calculates 'n' navmesh pathfinding iterations per frame. The
// default value of 100 is fine for small projects, but an MMO with huge amounts
// of agents will require more iterations per frame to avoid movement delays.
//
// For now we simply increase that number in Awake once.
// In the future we will be able to set the iteration number per agent:
//   https://forum.unity3d.com/threads/pathfindingiterationsperframe-for-bigger-games-should-be-per-agent.482699/
//
// Note: we could already use Update to set iterations to players.count*multiplier,
// but it's not that much better since one player could still delay all other
// player's path calculations.
using UnityEngine;
using UnityEngine.AI;
public class NavMeshPathfindingIterationsPerFrame : MonoBehaviour
{
    public int iterations = 100; // default
    void Awake()
    {
        //Debug.Log("Setting NavMesh Pathfinding Iterations Per Frame from " + NavMesh.pathfindingIterationsPerFrame + " to " + iterations);
        NavMesh.pathfindingIterationsPerFrame = iterations;
    }
}
