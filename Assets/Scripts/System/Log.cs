using System;
using System.IO;
using UnityEngine;

// Simple file-based log system scaffold: writes events to a txt file in persistentDataPath
// and automatically captures engine errors/exceptions (including crashes) via Application.logMessageReceived.
public static class Log
{
    const string FileName = "log.txt";

    static string FilePath => Path.Combine(Application.persistentDataPath, FileName);
    static bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoInit()
    {
        Init();
    }

    public static void Init()
    {
        if (initialized) return;
        initialized = true;

        try
        {
            File.WriteAllText(FilePath, $"=== Лог сессии начат {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Log] Не удалось создать файл лога: {e.Message}");
        }

        Application.logMessageReceived += HandleUnityLog;
    }

    public static void Shutdown()
    {
        if (!initialized) return;
        Application.logMessageReceived -= HandleUnityLog;
        initialized = false;
    }

    public static void Info(string message) => Write("INFO", message);
    public static void Warning(string message) => Write("WARN", message);
    public static void Error(string message) => Write("ERROR", message);

    static void HandleUnityLog(string condition, string stackTrace, LogType type)
    {
        switch (type)
        {
            case LogType.Exception:
                Write("CRASH", $"{condition}\n{stackTrace}");
                break;
            case LogType.Error:
            case LogType.Assert:
                Write("ERROR", condition);
                break;
        }
    }

    static void Write(string tag, string message)
    {
        string line = $"[{DateTime.Now:HH:mm:ss}] [{tag}] {message}";

        try
        {
            File.AppendAllText(FilePath, line + "\n");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Log] Не удалось записать в лог: {e.Message}");
        }
    }
}
