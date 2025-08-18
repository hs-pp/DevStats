using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public struct DevStatsSettings
{
    private static string SAVE_KEY = "IRLStatsSettings";
    public string APIKey;

    public static DevStatsSettings Load()
    {
        if (EditorPrefs.HasKey(SAVE_KEY))
        {
            return JsonUtility.FromJson<DevStatsSettings>(EditorPrefs.GetString(SAVE_KEY));
        }

        return new DevStatsSettings();
    }

    public void Save()
    {
        EditorPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(this));
    }
}