/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System;
using UnityEngine;

public class GameTime
{
    private DateTime utcTime;
    private int minute, hour, day, season, year;
    private double partOfDay, partOfYear;

    public static DateTime calendarStart = new DateTime(2020, 01, 01);
    public const int secondPerDay = 29700;
    public const int dayPerSeason = 84;
    public const int dayPerYear = 336;
    public const string timeSpringText = "spring";
    public const string timeSummerText = "summer";
    public const string timeAutumnText = "autumn";
    public const string timeWinterText = "winter";
    public const float timeMinDaylength = 8.0f;
    public const float timeMaxDaylength = 16.0f;
    public const int timeCurveDaylength = 13;
    public const float timeMinTwilight = 0.5f;
    public const float timeMaxTwilight = 1.5f;
    public const int timeCurveTwilight = 14;
    public const int yearOffset = 154;

    public static string[] seasonText =
    {
            GlobalFunc.FirstToUpper(timeSpringText),
            GlobalFunc.FirstToUpper(timeSummerText),
            GlobalFunc.FirstToUpper(timeAutumnText),
            GlobalFunc.FirstToUpper(timeWinterText)
        };

    public GameTime()
    {
        Now();
    }

    public GameTime(DateTime timeInUtc)
    {
        Value = timeInUtc;
    }
    public DateTime Value
    {
        get { return utcTime; }
        set { utcTime = value; CalculateFromRealTime(); }
    }
    public int Minute
    {
        get { return minute; }
        set { minute = value; CalculateFromGameTime(); }
    }
    public int Hour
    {
        get { return hour; }
        set { hour = value; CalculateFromGameTime(); }
    }
    public int Day
    {
        get { return day; }
        set { day = value; CalculateFromGameTime(); }
    }
    public int Season
    {
        get { return season; }
        set { season = value; CalculateFromGameTime(); }
    }
    public int Year
    {
        get { return year + yearOffset; }
        set { year = value - yearOffset; CalculateFromGameTime(); }
    }
    public double PartOfDay { get { return partOfDay; } }
    public double PartOfYear { get { return partOfYear; } }
    public string DateString
    {
        get { return String.Format(GlobalVar.timeDate, day, seasonText[season], Year); }
    }
    public string TimeString
    {
        get { return DateTime.FromOADate(partOfDay).ToString("HH:mm"); }
    }
    public string DateTimeString
    {
        get { return DateString + " " + TimeString; }
    }
    public double Daylight
    {
        get { return CalulateDaylight(); }
    }
    public double CurrentDayPortion
    {
        get { return CalulateCurrentDayPortion(); }
    }
    public double CurrentNightPortion
    {
        get { return CalulateCurrentNightPortion(); }
    }
    public double CurrentTwPortion
    {
        get { return CalulateCurrentTwilightPortion(); }
    }

    public void Now()
    {
#pragma warning disable 0162
        // warning CS0162: Unreachable code detected
        if (GlobalVar.isProduction)
        {
            utcTime = DateTime.UtcNow;
        }
        else
        {
            // adapted game time for debug!
            // we create our own universal time
            double debugSeconds = (DateTime.UtcNow - calendarStart).TotalSeconds * GlobalVar.testGameTimeSpeed + GlobalVar.testGameTimeOffset*3600;
            utcTime = calendarStart.AddSeconds(debugSeconds);
        }
#pragma warning disable 0162
        CalculateFromRealTime();
    }

    private void CalculateFromRealTime()
    {
        int secondsSinceStart = SecondsSinceStart(utcTime);
        year = secondsSinceStart / (secondPerDay * dayPerYear);
        int remainingSeconds = secondsSinceStart - year * secondPerDay * dayPerYear;
        partOfYear = 1d * remainingSeconds / (secondPerDay * dayPerYear);
        season = remainingSeconds / (secondPerDay * dayPerSeason);
        remainingSeconds = remainingSeconds - season * secondPerDay * dayPerSeason;
        day = 1 + remainingSeconds / secondPerDay;
        remainingSeconds = remainingSeconds % secondPerDay;
        partOfDay = 1d * remainingSeconds / secondPerDay;
        hour = (int)(partOfDay * 24);
        minute = ((int)(partOfDay * 1440)) % 60;
    }

    private void CalculateFromGameTime()
    {
        int secondsSinceStart = year * secondPerDay * dayPerYear;
        secondsSinceStart += season * secondPerDay * dayPerSeason;
        secondsSinceStart += (day - 1) * secondPerDay;
        secondsSinceStart += hour * secondPerDay / 24;
        secondsSinceStart += minute * secondPerDay / 1440;
        utcTime = calendarStart.AddSeconds(secondsSinceStart);
        CalculateFromRealTime();
    }

    public int SecondsSinceStart()
    {
        return SecondsSinceStart(utcTime);
    }

    public int SecondsSinceStart(DateTime utcDateTime)
    {
        return (int)(utcDateTime - calendarStart).TotalSeconds;
    }

    public double DiffHours(DateTime secondTime)
    {
        return (secondTime - utcTime).TotalSeconds / secondPerDay * 24;
    }

    public double DiffDays(DateTime secondTime)
    {
        return (secondTime - utcTime).TotalSeconds / secondPerDay;
    }

    private double CalulateDaylight()
    {
        double currentDaylight = GlobalFunc.ValueFromProportion(
            NonLinearCurves.GetInterimDouble0_1(timeCurveDaylength, partOfYear),
            timeMinDaylength,
            timeMaxDaylength)
            / 24 / 2;
        double currentTwilight = GlobalFunc.ValueFromProportion(
            NonLinearCurves.GetInterimDouble0_1(timeCurveTwilight, partOfYear),
            timeMinTwilight,
            timeMaxTwilight)
            / 24 / 2;

        double dawnStart = 0.5 - currentDaylight - currentTwilight;
        double dawnEnd = 0.5 - currentDaylight + currentTwilight;
        double duskStart = 0.5 + currentDaylight - currentTwilight;
        double duskEnd = 0.5 + currentDaylight + currentTwilight;
        double daylight = 0;
        if (partOfDay < dawnStart)
        {
            daylight = 0;
        }
        else if (partOfDay < dawnEnd)
        {
            daylight = (partOfDay - dawnStart) / (dawnEnd - dawnStart);
        }
        else if (partOfDay < duskStart)
        {
            daylight = 1;
        }
        else if (partOfDay < duskEnd)
        {
            daylight = (duskEnd - partOfDay) / (duskEnd - duskStart);
        }
        else
        {
            daylight = 0;
        }
        return daylight;
    }

    private double CalulateCurrentDayPortion()
    {
        double currentDaylight = GlobalFunc.ValueFromProportion(
            NonLinearCurves.GetInterimDouble0_1(timeCurveDaylength, partOfYear),
            timeMinDaylength,
            timeMaxDaylength)
           / 24;
        return currentDaylight;
    }

    private double CalulateCurrentTwilightPortion()
    {
        double currentTwilight = GlobalFunc.ValueFromProportion(
            NonLinearCurves.GetInterimDouble0_1(timeCurveTwilight, partOfYear),
            timeMinTwilight,
            timeMaxTwilight)
            / 24;
        return currentTwilight;
    }

    private double CalulateCurrentNightPortion()
    {
        return 1 - CalulateCurrentDayPortion() - 2 * CalulateCurrentTwilightPortion();
    }

    // static part
    /// <summary>
    /// Coverts a DataTime into seconds since 2020-01-01 00:00:00
    /// </summary>
    public static int UtcToSeconds(DateTime time)
    {
        return (int)(time - calendarStart).TotalSeconds;
    }

    /// <summary>
    /// Coverts seconds since 2020-01-01 00:00:00 into a DataTime
    /// </summary>
    public static DateTime SecondsToUtc(int seconds)
    {
        return calendarStart.AddSeconds(seconds);
    }

    /// <summary>
    /// return seconds since 2020-01-01 00:00:00 for now
    /// </summary>
    public static int SecondsSinceZero()
    {
        return (int)(DateTime.UtcNow - calendarStart).TotalSeconds;
    }

    /// <summary>
    /// return seconds since 2020-01-01 00:00:00
    /// </summary>
    public static int SecondsSinceZero(DateTime utcTime)
    {
        return (int)(utcTime - calendarStart).TotalSeconds;
    }
}

