/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// https://answers.unity.com/questions/489942/how-to-make-a-readonly-property-in-inspector.html
using UnityEditor;
using UnityEngine;
using System;

[CustomPropertyDrawer(typeof(ShowTimeFromSecondsAttribute))]
public class ShowTimeFromSecondsDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        string valueStr;

        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer:
                DateTime propTime = GameTime.calendarStart.AddSeconds(prop.intValue);
                valueStr = propTime.ToString() + " (" + prop.intValue.ToString() + ")";
                break;
            default:
                valueStr = "(not supported)";
                break;
        }

        EditorGUI.LabelField(position, label.text, valueStr);
    }
}