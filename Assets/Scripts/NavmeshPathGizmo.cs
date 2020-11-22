/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Draws the agent's path as Gizmo.
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public class NavmeshPathGizmo : MonoBehaviour
{
    public NavMeshAgent agent;
    void OnDrawGizmos()
    {
        NavMeshPath path = agent.path;
        // color depends on status
        Color color = Color.white;
        switch (path.status)
        {
            case NavMeshPathStatus.PathComplete: color = Color.white; break;
            case NavMeshPathStatus.PathInvalid: color = Color.red; break;
            case NavMeshPathStatus.PathPartial: color = Color.yellow; break;
        }
        // draw the path
        for (int i = 1; i < path.corners.Length; ++i)
            Debug.DrawLine(path.corners[i-1], path.corners[i], color);
        // draw velocity
        Debug.DrawLine(transform.position, transform.position + agent.velocity, Color.blue, 0, false);
    }
}