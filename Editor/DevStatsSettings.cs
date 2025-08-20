using System;
using UnityEditor;
using UnityEngine;

namespace DevStatsSystem.Editor
{
    [Serializable]
    internal class DevStatsSettings
    {
        private static string SAVE_KEY = "IRLStatsSettings";

        [SerializeField]
        private string m_apiKey;

        public string APIKey => m_apiKey;

        [SerializeField]
        private bool m_isEnabled = true;

        public bool IsEnabled => m_isEnabled;

        [SerializeField]
        private bool m_debugMode = false;

        public bool IsDebugMode => m_debugMode;

        // Singleton so it's easy to access.
        private static DevStatsSettings m_instance;

        public static DevStatsSettings Get()
        {
            if (m_instance == null)
            {
                m_instance = Load();
            }

            return m_instance;
        }

        public void SetAPIKey(string apiKey)
        {
            m_apiKey = apiKey;
            Save();
        }

        public void SetIsEnabled(bool isEnabled)
        {
            m_isEnabled = isEnabled;
            Save();
        }

        public void SetDebugMode(bool debugMode)
        {
            m_debugMode = debugMode;
            Save();
        }

        public bool IsRunning()
        {
            return IsEnabled && !string.IsNullOrEmpty(APIKey);
        }

        private static DevStatsSettings Load()
        {
            if (EditorPrefs.HasKey(SAVE_KEY))
            {
                return JsonUtility.FromJson<DevStatsSettings>(EditorPrefs.GetString(SAVE_KEY));
            }

            return new DevStatsSettings();
        }

        public void Save()
        {
            Debug.Log("Saved settings");
            EditorPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(this));
        }
    }
}