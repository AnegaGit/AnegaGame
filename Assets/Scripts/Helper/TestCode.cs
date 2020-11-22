/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class TestCode : MonoBehaviour
{
    public int testValue1;
    public int testValue2;

    public void ClearLog()
    {
        GlobalFunc.ClearLogConsole();
    }
    public void RunTest()
    {
        Debug.Log(">>> has " + GlobalFunc.HasBitSet(testValue1, testValue2).ToString());
        Debug.Log(">>> set " + GlobalFunc.SetBit(testValue1, testValue2).ToString());
        Debug.Log(">>> remove " + GlobalFunc.RemoveBit(testValue1, testValue2).ToString());
        Debug.Log(">>> remove " + GlobalFunc.SetBit(testValue1, testValue2,false).ToString());

        Debug.Log(">>> mask: "+ GlobalFunc.SetBit(0, testValue2).ToString());
        Debug.Log   (">>> mask "+GlobalFunc.MaskForBit(testValue2).ToString());

    }
}
#endif