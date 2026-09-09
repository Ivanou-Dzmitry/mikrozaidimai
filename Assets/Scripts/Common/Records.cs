using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// One best-score entry per mini-game, keyed by gameId.
[Serializable]
public class RecordEntry
{
    public string gameId;
    public int value;
}

[Serializable]
public class RecordsData
{
    public List<RecordEntry> records = new List<RecordEntry>();
}

// Persisted best score per game, saved to json in persistentDataPath (same approach as Settings).
public static class Records
{
    const string FileName = "records.json";

    static RecordsData data = new RecordsData();

    static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoLoad()
    {
        Load();
    }

    public static void Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                data = JsonUtility.FromJson<RecordsData>(json);
            }
            else
            {
                data = new RecordsData();
            }
        }
        catch (Exception e)
        {
            Log.Error($"[Records] Failed to load records: {e.Message}");
            data = new RecordsData();
        }
    }

    public static void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception e)
        {
            Log.Error($"[Records] Failed to save records: {e.Message}");
        }
    }

    public static int GetRecord(string gameId)
    {
        RecordEntry entry = data.records.Find(r => r.gameId == gameId);
        return entry != null ? entry.value : 0;
    }

    // Returns true if value beat the previous record (and persists the new one).
    public static bool TrySetRecord(string gameId, int value)
    {
        RecordEntry entry = data.records.Find(r => r.gameId == gameId);

        if (entry == null)
        {
            data.records.Add(new RecordEntry { gameId = gameId, value = value });
            Save();
            return true;
        }

        if (value > entry.value)
        {
            entry.value = value;
            Save();
            return true;
        }

        return false;
    }
}
