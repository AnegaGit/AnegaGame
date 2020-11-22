/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CreateStaticElement))]
public class EditorCreateStaticElement : Editor
{
    Vector3 selectedPosition = new Vector3();
    Quaternion selectedRotation = new Quaternion();

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Click where you want to have the new item");

        DrawDefaultInspector();
    }

    void OnSceneGUI()
    {
        if (Event.current.type == EventType.MouseDown)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 eulerRot = Camera.current.transform.rotation.eulerAngles;
                Quaternion newRotation = Quaternion.Euler(0, eulerRot.y, 0);
                selectedPosition = hit.point;
                selectedRotation = newRotation;

                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Create element here"), false, CreateElement);
                menu.ShowAsContext();
            }
        }
    }

    void CreateElement()
    {
        Debug.Log(">>> create");
        CreateStaticElement myScript = (CreateStaticElement)target;
        myScript.CreateElement(selectedPosition, selectedRotation);
    }
}
