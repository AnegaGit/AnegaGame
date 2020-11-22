/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// This component can be attached to moveable windows, so that they are only
// moveable within the Screen boundaries.
using UnityEngine;
public class UIKeepInScreen : MonoBehaviour
{
    void Update()
    {
        // get current rectangle
        Rect rect = GetComponent<RectTransform>().rect;
        // to world space
        Vector2 minworld = transform.TransformPoint(rect.min);
        Vector2 maxworld = transform.TransformPoint(rect.max);
        Vector2 sizeworld = maxworld - minworld;
        // keep the min position in screen bounds - size
        maxworld = new Vector2(Screen.width, Screen.height) - sizeworld;
        // keep position between (0,0) and maxworld
        float x = Mathf.Clamp(minworld.x, 0, maxworld.x);
        float y = Mathf.Clamp(minworld.y, 0, maxworld.y);
        // set new position to xy(=local) + offset(=world)
        Vector2 offset = (Vector2)transform.position - minworld;
        transform.position = new Vector2(x, y) + offset;
    }
}
