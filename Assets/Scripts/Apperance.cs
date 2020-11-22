/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System;
using UnityEngine;

public class Apperance
{
    public float height { get; set; }
    public float fat { get; set; }
    public float muscles { get; set; }
    public float breastSize { get; set; }
    public float gluteusSize { get; set; }
    public float waist { get; set; }
    string _skinColor { get; set; }
    public int hair { get; set; }
    string _hairColor { get; set; }
    public int beard { get; set; }
    string _beardColor { get; set; }
    public float headWidth { get; set; }
    public float chinSize { get; set; }
    public float chinPronounced { get; set; }
    public float eyeSize { get; set; }
    public float mouthSize { get; set; }
    public float lipsSize { get; set; }
    public float earsSize { get; set; }
    public float noseSize { get; set; }
    public float noseWidth { get; set; }
    public float noseCurve { get; set; }
    public int fangs { get; set; }



    public Color skinColor
    {
        get { return GlobalFunc.HexToColor(_skinColor); }
        set { _skinColor = GlobalFunc.ColorToHex(value); }
    }
    public Color hairColor
    {
        get { return GlobalFunc.HexToColor(_hairColor); }
        set { _hairColor = GlobalFunc.ColorToHex(value); }
    }
    public Color beardColor
    {
        get { return GlobalFunc.HexToColor(_beardColor); }
        set { _beardColor = GlobalFunc.ColorToHex(value); }
    }
    public Apperance()
    {
        SetDefaultValues();
    }

    public void SetDefaultValues()
    {
        height = GlobalVar.charApperanceHeightStartValue;
        fat = GlobalVar.charApperanceStartValue;
        muscles = GlobalVar.charApperanceStartValue;
        breastSize = GlobalVar.charApperanceStartValue;
        gluteusSize = GlobalVar.charApperanceStartValue;
        waist = GlobalVar.charApperanceStartValue;
        skinColor = GlobalVar.charApperanceStartColor;
        hair = 0;
        hairColor = GlobalVar.charApperanceStartColor;
        beard = 0;
        beardColor = GlobalVar.charApperanceStartColor;
        headWidth = GlobalVar.charApperanceStartValue;
        chinSize = GlobalVar.charApperanceStartValue;
        chinPronounced = GlobalVar.charApperanceStartValue;
        eyeSize = GlobalVar.charApperanceStartValue;
        mouthSize = GlobalVar.charApperanceStartValue;
        lipsSize = GlobalVar.charApperanceStartValue;
        earsSize = GlobalVar.charApperanceStartValue;
        noseSize = GlobalVar.charApperanceStartValue;
        noseWidth = GlobalVar.charApperanceStartValue;
        noseCurve = GlobalVar.charApperanceStartValue;
        fangs = 0;
    }

    public void ConvertFromString(string apperanceString)
    {
        if (apperanceString.Length < 88)
        {
            SetDefaultValues();
        }
        else
        {
            int value;

            if (int.TryParse((string)apperanceString.Substring(0, 4), out value))
                height = (float)value / 1000f;
            else
                height = GlobalVar.charApperanceHeightStartValue;

            if (int.TryParse((string)apperanceString.Substring(4, 4), out value))
                fat = (float)value / 1000f;
            else
                fat = GlobalVar.charApperanceStartValue;

            if (int.TryParse((string)apperanceString.Substring(8, 4), out value))
                muscles = (float)value / 1000f;
            else
                muscles = GlobalVar.charApperanceStartValue;

            if (int.TryParse((string)apperanceString.Substring(12, 4), out value))
                breastSize = (float)value / 1000f;
            else
                breastSize = GlobalVar.charApperanceStartValue;

            if (int.TryParse((string)apperanceString.Substring(16, 4), out value))
                gluteusSize = (float)value / 1000f;
            else
                gluteusSize = GlobalVar.charApperanceStartValue;

            if (int.TryParse((string)apperanceString.Substring(20, 4), out value))
                waist = (float)value / 1000f;
            else
                waist = GlobalVar.charApperanceStartValue;

            _skinColor = apperanceString.Substring(24, 6);

            if (int.TryParse((string)apperanceString.Substring(30, 2), out value))
                hair = value;
            else
                hair = 0;
            _hairColor = apperanceString.Substring(32, 6);

            if (int.TryParse((string)apperanceString.Substring(38, 2), out value))
                beard = value;
            else
                beard = 0;
            _beardColor = apperanceString.Substring(40, 6);

            if (int.TryParse((string)apperanceString.Substring(46, 4), out value))
                headWidth = (float)value / 1000f;
            else
                headWidth = GlobalVar.charApperanceStartValue;

            if (int.TryParse((string)apperanceString.Substring(50, 4), out value))
                chinSize = (float)value / 1000f;
            else
                chinSize = GlobalVar.charApperanceStartValue;

            if (int.TryParse((string)apperanceString.Substring(54, 4), out value))
                chinPronounced = (float)value / 1000f;
            else
                chinPronounced = GlobalVar.charApperanceStartValue;

            if (int.TryParse((string)apperanceString.Substring(58, 4), out value))
                eyeSize = (float)value / 1000f;
            else
                eyeSize = GlobalVar.charApperanceStartValue;

            if (int.TryParse((string)apperanceString.Substring(62, 4), out value))
                mouthSize = (float)value / 1000f;
            else
                mouthSize = GlobalVar.charApperanceStartValue;

            if (int.TryParse((string)apperanceString.Substring(66, 4), out value))
                lipsSize = (float)value / 1000f;
            else
                lipsSize = GlobalVar.charApperanceStartValue;

            if (int.TryParse((string)apperanceString.Substring(70, 4), out value))
                earsSize = (float)value / 1000f;
            else
                earsSize = GlobalVar.charApperanceStartValue;

            if (int.TryParse((string)apperanceString.Substring(74, 4), out value))
                noseSize = (float)value / 1000f;
            else
                noseSize = GlobalVar.charApperanceStartValue;

            if (int.TryParse((string)apperanceString.Substring(78, 4), out value))
                noseWidth = (float)value / 1000f;
            else
                noseWidth = GlobalVar.charApperanceStartValue;

            if (int.TryParse((string)apperanceString.Substring(82, 4), out value))
                noseCurve = (float)value / 1000f;
            else
                noseCurve = GlobalVar.charApperanceStartValue;

            if (int.TryParse((string)apperanceString.Substring(86, 2), out value))
                fangs = value;
            else
                fangs = 0;
        }
        LimitApperance();
    }

    public string CreateString()
    {
        string apperanceString = "";
        LimitApperance();
        apperanceString += string.Format("{0,4}", (int)(height * 1000));
        apperanceString += string.Format("{0,4}", (int)(fat * 1000));
        apperanceString += string.Format("{0,4}", (int)(muscles * 1000));
        apperanceString += string.Format("{0,4}", (int)(breastSize * 1000));
        apperanceString += string.Format("{0,4}", (int)(gluteusSize * 1000));
        apperanceString += string.Format("{0,4}", (int)(waist * 1000));
        apperanceString += string.Format("{0,6}", _skinColor);
        apperanceString += string.Format("{0,2}", hair);
        apperanceString += string.Format("{0,6}", _hairColor);
        apperanceString += string.Format("{0,2}", beard);
        apperanceString += string.Format("{0,6}", _beardColor);
        apperanceString += string.Format("{0,4}", (int)(headWidth * 1000));
        apperanceString += string.Format("{0,4}", (int)(chinSize * 1000));
        apperanceString += string.Format("{0,4}", (int)(chinPronounced * 1000));
        apperanceString += string.Format("{0,4}", (int)(eyeSize * 1000));
        apperanceString += string.Format("{0,4}", (int)(mouthSize * 1000));
        apperanceString += string.Format("{0,4}", (int)(lipsSize * 1000));
        apperanceString += string.Format("{0,4}", (int)(earsSize * 1000));
        apperanceString += string.Format("{0,4}", (int)(noseSize * 1000));
        apperanceString += string.Format("{0,4}", (int)(noseWidth * 1000));
        apperanceString += string.Format("{0,4}", (int)(noseCurve * 1000));
        apperanceString += string.Format("{0,2}", fangs);

        return apperanceString;
    }

    private void LimitApperance()
    {
        height = GlobalFunc.KeepInRange(height, 0f, 1f);
        fat = GlobalFunc.KeepInRange(fat, 0f, 1f);
        muscles = GlobalFunc.KeepInRange(muscles, 0f, 1f);
        breastSize = GlobalFunc.KeepInRange(breastSize, 0f, 1f);
        gluteusSize = GlobalFunc.KeepInRange(gluteusSize, 0f, 1f);
        waist = GlobalFunc.KeepInRange(waist, 0f, 1f);
        headWidth = GlobalFunc.KeepInRange(headWidth, 0f, 1f);
        chinSize = GlobalFunc.KeepInRange(chinSize, 0f, 1f);
        chinPronounced = GlobalFunc.KeepInRange(chinPronounced, 0f, 1f);
        eyeSize = GlobalFunc.KeepInRange(eyeSize, 0f, 1f);
        mouthSize = GlobalFunc.KeepInRange(mouthSize, 0f, 1f);
        lipsSize = GlobalFunc.KeepInRange(lipsSize, 0f, 1f);
        earsSize = GlobalFunc.KeepInRange(earsSize, 0f, 1f);
        noseSize = GlobalFunc.KeepInRange(noseSize, 0f, 1f);
        noseWidth = GlobalFunc.KeepInRange(noseWidth, 0f, 1f);
        noseCurve = GlobalFunc.KeepInRange(noseCurve, 0f, 1f);
    }
}
