/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Sets the Rigidbody's velocity in Start().
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class DefaultVelocity : MonoBehaviour
{
    public Rigidbody rigidBody;
    public Vector3 velocity;
    void Start()
    {
        rigidBody.velocity = velocity;
    }
}
