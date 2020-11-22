/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// grid structure: get/set values of type T at any point
using System.Collections.Generic;
using UnityEngine;
public class Grid<T>
{
    Dictionary<Vector2Int, HashSet<T>> grid = new Dictionary<Vector2Int, HashSet<T>>();
    // cache a 9 neighbor grid of vector2 offsets so we can use them more easily
    Vector2Int[] neighorOffsets =
    {
        Vector2Int.up,
        Vector2Int.up + Vector2Int.left,
        Vector2Int.up + Vector2Int.right,
        Vector2Int.left,
        Vector2Int.zero,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.down + Vector2Int.left,
        Vector2Int.down + Vector2Int.right
    };
    // helper function so we can remove an entry without worrying
    public void Remove(Vector2Int position, T value)
    {
        // is this set in the grid? then remove it
        HashSet<T> hashSet;
        if (grid.TryGetValue(position, out hashSet))
            hashSet.Remove(value);
    }
    // helper function so we can add an entry without worrying
    public void Add(Vector2Int position, T value)
    {
        // initialize set in grid if it's not in there yet
        HashSet<T> hashSet;
        if (!grid.TryGetValue(position, out hashSet))
        {
            hashSet = new HashSet<T>();
            grid[position] = hashSet;
        }
        // add to it
        hashSet.Add(value);
    }
    // helper function to get set at position without worrying
    public HashSet<T> Get(Vector2Int position)
    {
        // return the set at position
        HashSet<T> hashSet;
        if (grid.TryGetValue(position, out hashSet))
            return hashSet;
        // or empty new set otherwise (rebuild observers doesn't want null)
        return new HashSet<T>();
    }
    // helper function to get at position and it's 8 neighbors without worrying
    public HashSet<T> GetWithNeighbours(Vector2Int position)
    {
        HashSet<T> hashSet = new HashSet<T>();
        foreach (Vector2Int offset in neighorOffsets)
            hashSet.UnionWith(Get(position + offset));
        return hashSet;
    }
}