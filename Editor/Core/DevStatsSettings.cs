using System;
using UnityEngine;

namespace DevStatsSystem.Editor.Core
{
    [Serializable]
    internal class DevStatsSettings : SavedData<DevStatsSettings>
    {
        [SerializeField]
        private string m_apiKey;
        public string APIKey
        {
            get => m_apiKey;
            set => m_apiKey = value;
        }

        [SerializeField]
        private bool m_isEnabled = true;
        public bool IsEnabled => m_isEnabled;

        [SerializeField]
        private bool m_isDebugMode = false;
        public bool IsDebugMode
        {
            get => m_isDebugMode;
            set => m_isDebugMode = value;
        }
        
        [NonSerialized]
        public Action<bool, bool> OnEnabledChanged;

        public void SetIsEnabled(bool isEnabled)
        {
            if (isEnabled == m_isEnabled)
            {
                return;
            }
            
            bool previousValue = m_isEnabled;
            m_isEnabled = isEnabled;
            
            OnEnabledChanged?.Invoke(isEnabled, previousValue);
        }

        public bool IsRunning()
        {
            return IsEnabled && !string.IsNullOrEmpty(APIKey);
        }
    }
}