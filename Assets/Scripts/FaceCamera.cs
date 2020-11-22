/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Useful for Text Meshes that should face the camera.
//
// In some cases there seems to be a Unity bug where the text meshes end up in
// weird positions if it's not positioned at (0,0,0). In that case simply put it
// into an empty GameObject and use that empty GameObject for positioning.
using UnityEngine;
public class FaceCamera : MonoBehaviour
{
    // LateUpdate so that all camera updates are finished.
    void LateUpdate()
    {
       transform.forward = Camera.main.transform.forward;
    }
    // copying transform.forward is relatively expensive and slows things down
    // for large amounts of entities, so we only want to do it while the mesh
    // is actually visible
    void Awake() { enabled = false; } // disabled by default until visible
    void OnBecameVisible() { enabled = true; }
    void OnBecameInvisible() { enabled = false; }
}
