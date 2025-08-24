using System;
using UnityEngine;

namespace DevStatsSystem.Editor.Core
{
    [Serializable]
    internal class DevStatsSettings : SavedData<DevStatsSettings>
    {
        [SerializeField]
        private string m_apiKey;
        public string APIKey => m_apiKey;

        [SerializeField]
        private bool m_isEnabled = true;
        public bool IsEnabled => m_isEnabled;

        [SerializeField]
        private bool m_isDebugMode = false;
        public bool IsDebugMode => m_isDebugMode;
        
        [NonSerialized]
        public Action<bool, bool> OnEnabledChanged;

        public void SetAPIKey(string apiKey)
        {
            m_apiKey = apiKey;
            Save();
        }

        public void SetIsEnabled(bool isEnabled)
        {
            if (isEnabled == m_isEnabled)
            {
                return;
            }
            
            bool previousValue = m_isEnabled;
            m_isEnabled = isEnabled;
            Save();
            
            OnEnabledChanged?.Invoke(isEnabled, previousValue);
        }

        public void SetIsDebugMode(bool debugMode)
        {
            m_isDebugMode = debugMode;
            Save();
        }

        public bool IsRunning()
        {
            return IsEnabled && !string.IsNullOrEmpty(APIKey);
        }
    }
}