using System;
using System.IO;
using UnityEngine;

// Scaffold: settings data that gets saved to json. Fields below are examples, extend as needed.
[Serializable]
public class SettingsData
{
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public string language = "en";
    public bool helperEnabled = true;
}

// Static access to settings: Settings.Data.masterVolume, then Settings.Save().
public static class Settings
{
    const string FileName = "settings.json";

    public static SettingsData Data { get; private set; } = new SettingsData();

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
                Data = JsonUtility.FromJson<SettingsData>(json);
            }
            else
            {
                Data = new SettingsData();
            }
        }
        catch (Exception e)
        {
            Log.Error($"[Settings] Не удалось загрузить настройки: {e.Message}");
            Data = new SettingsData();
        }
    }

    public static void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(Data, true);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception e)
        {
            Log.Error($"[Settings] Не удалось сохранить настройки: {e.Message}");
        }
    }
}
