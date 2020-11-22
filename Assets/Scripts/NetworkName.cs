/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Synchronizing an entity's name is crucial for components that need the proper
// name in the Start function (e.g. to load the spellbar by name).
//
// Simply using OnSerialize and OnDeserialize is the easiest way to do it. Using
// a SyncVar would require Start, Hooks etc.
using Mirror;
public class NetworkName : NetworkBehaviour
{
    // server-side serialization
    public override bool OnSerialize(NetworkWriter writer, bool initialState)
    {
        writer.Write(name);
        return true;
    }
    // client-side deserialization
    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        name = reader.ReadString();
    }
}
