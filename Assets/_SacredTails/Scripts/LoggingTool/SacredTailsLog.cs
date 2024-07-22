using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class SacredTailsLog
{
    public static bool isBot = false;
    public static List<string> messagesLogged = new List<string>();
    public static void Init()
    {
#if UNITY_EDITOR
        isBot = true;
#else
        List<string> arguments = Environment.GetCommandLineArgs().ToList();
        isBot = arguments.Contains("IsBot");
#endif
    }

    public static void LogMessageForBot(string message)
    {
        LogMessage(message, true);
    }
    public static void LogErrorMessageForBot(string message)
    {
        LogErrorMessage(message, true);
    }

    public static void LogMessage(string message, bool botDebug = false)
    {
        if (isBot && !botDebug)
            return;
        Debug.Log(message);

        AddLogToFileLog(message);
    }

    public static void LogErrorMessage(string message, bool botDebug = false)
    {
        if ((isBot && !botDebug))
            return;
        Debug.LogError(message);

        AddLogToFileLog(message);
    }

    private static void AddLogToFileLog(string message)
    {
        messagesLogged.Add("---------------------------------------------------------------------------------");
        messagesLogged.Add(message);
        messagesLogged.Add("---------------------------------------------------------------------------------");
    }


    public static void OnEnd(int numberOfBot)
    {
        string filePath = Environment.CurrentDirectory + $"\\LOGS\\log_bot{numberOfBot}_{DateTime.UtcNow.ToString("yyyy-MM-dd HH")}.txt";
        File.WriteAllLines(filePath, messagesLogged.ToArray());
    }

}

