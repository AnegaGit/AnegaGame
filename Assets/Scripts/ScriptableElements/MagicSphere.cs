/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MagicSphere : MonoBehaviour
{
   private Vector3 spin = new Vector3(0.2f, 0.3f, 0.1f);

    private void Update()
    {
        transform.Rotate(spin);
    }
}
