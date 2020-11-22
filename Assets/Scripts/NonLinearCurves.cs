/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using System;
using Newtonsoft.Json;

static class NonLinearCurves
{
    public static float[,] curves = new float[GlobalVar.maxNonLinearCurves, 101];
    private static NonlinearCurvesParameter loadedParameter = new NonlinearCurvesParameter();

    public static void LoadParameter()
    {
        try
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("Misc\\nonlinearcurves");
            loadedParameter = JsonConvert.DeserializeObject<NonlinearCurvesParameter>(jsonFile.text);
            curves = loadedParameter.curves;
            LogFile.WriteLog(LogFile.LogLevel.Info, "Nonlinear curves loaded.");
        }
        catch (Exception ex)
        {
            LogFile.WriteException(LogFile.LogLevel.Error, ex, "Critical Error: File nonlinearcurves not loaded!");
        }
    }

    /// <summary>
    /// Curve value at position (0..100) result 0..100
    /// </summary>
    /// <param name="pos0_100">0..100</param>
    /// <returns>0..100</returns>
    public static float GetValue(int curveId, int pos0_100)
    {
        pos0_100 = GlobalFunc.KeepInRange(pos0_100, 0, 100);
        curveId = GlobalFunc.KeepInRange(curveId, 0, GlobalVar.maxNonLinearCurves - 1);
        float result;
        try
        {
            result = curves[curveId, pos0_100];
        }
        catch
        {
            result = pos0_100;
        }
        return result;
    }
    /// <summary>
    /// Curve value at relative position (0..1) result 0..100
    /// </summary>  
    /// <param name="pos0_1">0..1</param>
    /// <returns>0..100</returns>
    public static float GetValue(int curveId, double pos0_1)
    {
        int pos0_100 = (int)(pos0_1 * 100);
        return GetValue(curveId, pos0_100);
    }
    /// <summary>
    /// relatve curve value at position (0..100) result 0..1
    /// </summary>
    /// <param name="pos0_100">0..100</param>
    /// <returns>0..1</returns>
    public static float GetFloat0_1(int curveId, int pos0_100)
    {
        return GetValue(curveId, pos0_100) / 100;
    }
    /// <summary>
    /// relatve curve value at relative position (0..1) result 0..1
    /// </summary>
    /// <param name="pos0_1">0..1</param>
    /// <returns>0..1</returns>
    public static float GetFloat0_1(int curveId, double pos0_1)
    {
        int pos0_100 = (int)(pos0_1 * 100);
        return GetValue(curveId, pos0_100) / 100;
    }
    /// <summary>
    /// relatve curve value at position (0..100) result 0..1
    /// </summary>
    /// <param name="pos0_100">0..100</param>
    /// <returns>0..1</returns>
    public static double GetDouble0_1(int curveId, int pos0_100)
    {
        return GetValue(curveId, pos0_100) / 100.0;
    }

    /// <summary>
    /// lookup with interpolation at relative position (0..1), result 0..1
    /// </summary>
    /// <param name="pos0_1">0..1</param>
    /// <returns>0..1</returns>
    public static double GetInterimDouble0_1(int curveId, double pos0_1)
    {
        double pos = pos0_1 * 100;
        int pos0_100 = (int)pos;
        int pos0_101 = pos0_100 + 1;
        pos0_100 = GlobalFunc.KeepInRange(pos0_100, 0, 100);
        pos0_101 = GlobalFunc.KeepInRange(pos0_101, 0, 100);
        curveId = GlobalFunc.KeepInRange(curveId, 0, GlobalVar.maxNonLinearCurves - 1);
        double result;
        try
        {
            float v1 = curves[curveId, pos0_100];
            float v2 = curves[curveId, pos0_101];
            result = (v1 + (pos - pos0_100) * (v2 - v1)) / 100d;
        }
        catch
        {
            result = pos0_100;
        }
        return result;
    }

    /// <summary>
    /// lookup with interpolation at relative position (0..1), result 0..1
    /// </summary>
    /// <param name="pos0_1">0..1</param>
    /// <returns>0..1</returns> 
    public static float GetInterimFloat0_1(int curveId, double pos0_1)
    {
        return (float)GetInterimDouble0_1(curveId, pos0_1);
    }

    /// <summary>
    /// Get position in out range according to position of inValue in inRange coverted via curve
    /// </summary>
    public static double DoubleFromCurvePosition(int curveId, double inValue, double inMin, double inMax, double outMin, double outMax)
    {
        double curvePos = GlobalFunc.ProportionFromValue(inValue, inMin, inMax);
        double curveValue = GetInterimDouble0_1(curveId, curvePos);
        return GlobalFunc.ValueFromProportion(curveValue, outMin, outMax);
    }
    public static float FloatFromCurvePosition(int curveId, double inValue, double inMin, double inMax, double outMin, double outMax)
    {
        return (float)DoubleFromCurvePosition(curveId, inValue, inMin, inMax, outMin, outMax);
    }
    public static int IntFromCurvePosition(int curveId, double inValue, double inMin, double inMax, double outMin, double outMax)
    {
        return (int)DoubleFromCurvePosition(curveId, inValue, inMin, inMax, outMin, outMax);
    }

    /// <summary>
    /// summary of all results
    /// </summary>
    public static float GetIntegral(int curveId, int min = 0, int max = 100)
    {
        float integral = 0;
        min = Mathf.Clamp(min, 0, max);
        max = Mathf.Clamp(max, min, 100);
        for (int i = min; i <= max; i++)
        {
            integral += GetValue(curveId, i);
        }
        return integral;
    }
}

class NonlinearCurvesParameter
{
    public float[,] curves = new float[GlobalVar.maxNonLinearCurves, 101];
}
