/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestCode))]
public class EditorTestCode : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TestCode myScript = (TestCode)target;

        if (GUILayout.Button("Clear Log"))
        {
            myScript.ClearLog();
        }
        if (GUILayout.Button("Run Test"))
        {
            myScript.RunTest();
        }

    }
}
#endif