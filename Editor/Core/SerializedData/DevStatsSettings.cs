using System;
using UnityEngine;

namespace DevStatsSystem.Core.SerializedData
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
        private bool m_printDebugLogs = false;
        public bool PrintDebugLogs
        {
            get => m_printDebugLogs;
            set => m_printDebugLogs = value;
        }

        [SerializeField]
        private int m_heartbeatSendInterval = 120; // in seconds
        public int HeartbeatSendInterval
        {
            get => m_heartbeatSendInterval;
            set
            {
                m_heartbeatSendInterval = value;
                if (m_heartbeatSendInterval < 120)
                {
                    m_heartbeatSendInterval = 120; // This is min
                }
                OnHeartbeatSendIntervalChanged?.Invoke();
            }
        }
        public Action OnHeartbeatSendIntervalChanged;

        [SerializeField]
        private int m_sameFileInterval = 5; // in seconds
        public int SameFileInterval
        {
            get => m_sameFileInterval;
            set => m_sameFileInterval = value;
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