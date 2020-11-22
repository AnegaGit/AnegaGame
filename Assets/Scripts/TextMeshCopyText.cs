/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Copy text from one text mesh to another (for shadows etc.)
using UnityEngine;
[RequireComponent(typeof(TextMesh))]
public class TextMeshCopyText : MonoBehaviour
{
    public TextMesh source;
    public TextMesh destination;
	void Update()
    {
	    destination.text = source.text;
	}
}
