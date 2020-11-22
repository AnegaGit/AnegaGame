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

[CustomEditor(typeof(CreateNavMesh))]
public class EditorCreateNavMesh : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CreateNavMesh myScript = (CreateNavMesh)target;

        if (GUILayout.Button("Build temporary NavMesh data"))
        {
            myScript.RecalculateNavMesh();
        }
        if (GUILayout.Button("Clean temporary NavMesh data"))
        {
            myScript.CleanWorkingFolder();
        }
    }
}
#endif
