/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Parties have to be structs in order to work with SyncLists.
using System;
using System.Collections.Generic;
using System.Linq;
public struct Party
{
    public string notice;
    public string[] members; // first one == master
    public bool shareMoney;
    public bool everyoneCanInvite;
    // statics
    public static int Capacity = 8;
    public static float BonusExperiencePerMember = 0.1f;
    // helper function to find a member
    public int GetMemberIndex(string memberName)
    {
        return members != null ? Array.IndexOf(members, memberName) : -1;
    }
    public bool IsFull()
    {
        return members != null && members.Length == Capacity;
    }
    public void AddMember(string name)
    {
        if (members != null)
        {
            Array.Resize(ref members, members.Length + 1);
            members[members.Length - 1] = name;
        }
        else
        {
            members = new string[]{name};
        }
    }
    public void RemoveMember(string name)
    {
        List<string> list = members.ToList();
        list.RemoveAll(member => member == name);
        members = list.ToArray();
    }
    // can 'requester' invite someone?
    public bool CanInvite(string requesterName)
    {
        int requesterIndex = GetMemberIndex(requesterName);
        if (requesterIndex != -1)
        {
            // everyone can invite as long as the party isn't full
            return members.Length < Capacity;
        }
        return false;
    }
}