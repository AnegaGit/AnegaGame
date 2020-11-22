/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshHideOrganization))]
public class MeshHideOrganizationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MeshHideOrganization myScript = (MeshHideOrganization)target;
        if (GUILayout.Button("Apply MeshHide assets"))
        {
            myScript.ReorganizeMeshHide();
        }
        if (GUILayout.Button("Resort list by names"))
        {
            myScript.SortByName();
        }
        if (GUILayout.Button("Documentation"))
        {
            myScript.CreateDocumentation();
        }

        if (GUILayout.Button(new GUIContent("Verify if files _Recipe exist that are not part of this list", "Files and directories using '_' are excluded")))
        {
            myScript.FindMissingFiles();
        }
    }
}
#endif
