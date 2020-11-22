/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Help To find all lines not containing "abc" for removal use regex ^((?!abc).)*$ in notepad++
using System;
using UnityEngine;

public class LogFile:MonoBehaviour
{
    private static string logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Anega";
    private static string logFileName = logFilePath + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd") + "_anega.log";
    private static string gmlogFileName = logFilePath + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd") + "_gmactions.log";

    public enum LogLevel
    {
        Error,
        Warning,
        Info,
        Extended,
        Chat,
        Always,
        Debug,
        Server
    };

    public static void WriteGmLog(int idGM, int idPlayer, string logText)
    {
        gmlogFileName = logFilePath + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd") + "_gmactions.log";
        if (!System.IO.Directory.Exists(logFilePath))
        {
            System.IO.Directory.CreateDirectory(logFilePath);
        }
        System.IO.StreamWriter logFileStream;
        if (!System.IO.File.Exists(gmlogFileName))
        {
            logFileStream = System.IO.File.CreateText(gmlogFileName);
        }
        else
        {
            logFileStream = System.IO.File.AppendText(gmlogFileName);
        }

        string textLine = string.Format("{0};{1};{2};{3}", DateTime.UtcNow.ToString("HH:mm:ss"), idGM, idPlayer, logText);
        logFileStream.WriteLine(textLine);
        logFileStream.Close();
        if (!GlobalVar.isProduction)
        {
            Debug.Log(textLine);
        }
    }

    public static void WriteDebug(string logText)
    {
        if (!GlobalVar.isProduction)
            WriteLog(LogLevel.Debug, logText);
    }

    public static void WriteLog(LogLevel logType, string logText)
    {
        if (PlayerPreferences.logLevel[(int)logType] == '1' || logType == LogLevel.Always|| logType == LogLevel.Debug )
        {
            if (!System.IO.Directory.Exists(logFilePath))
            {
                System.IO.Directory.CreateDirectory(logFilePath);
            }
            System.IO.StreamWriter logFileStream;
            if (!System.IO.File.Exists(logFileName))
            {
                logFileStream = System.IO.File.CreateText(logFileName);
            }
            else
            {
                logFileStream = System.IO.File.AppendText(logFileName);
            }

            string textLine = DateTime.UtcNow.ToString("HH:mm:ss") + ": ";
            switch (logType)
            {
                case LogLevel.Error:
                    textLine += "[ERR] ";
                    break;
                case LogLevel.Warning:
                    textLine += "[WAR] ";
                    break;
                case LogLevel.Info:
                    textLine += "[INF] ";
                    break;
                case LogLevel.Extended:
                    textLine += "[EXT] ";
                    break;
                case LogLevel.Chat:
                    textLine += "[CHA] ";
                    break;
                case LogLevel.Debug:
                    textLine += "[DBG] ";
                    break;
                case LogLevel.Server:
                    textLine += "[SRV] ";
                    break;
                default:
                    textLine += "[MIS] ";
                    break;
            }
            textLine += logText;
            logFileStream.WriteLine(textLine);
            logFileStream.Close();
            if (!GlobalVar.isProduction)
            {
                Debug.Log(textLine);
            }
        }

    }

    public static void WriteException(LogLevel logType, Exception e, string logText = "")
    {
        if (logText.Length > 0)
            logText += Environment.NewLine;
        string fullText = logText + e.Message + ";" + e.StackTrace;
        WriteLog(logType, fullText);
    }

    public static void OpenLogFile()
    {
        System.Diagnostics.Process.Start(logFileName);
    }
}
