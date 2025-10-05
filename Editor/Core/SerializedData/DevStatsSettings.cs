using System;
using DevStatsSystem.UI;
using UnityEngine;

namespace DevStatsSystem.Core.SerializedData
{
    [Serializable]
    internal enum StatsRefreshRate
    {
        OnceADay,
        TwiceADay,
        EveryHour,
        EveryThirtyMinutes,
        EveryFifteenMinutes,
        EveryTenMinutes,
    }

    [Serializable]
    internal enum PostFrequency : int
    {
        EveryTwoMinutes = 120,
        EveryThreeMinutes = 180,
        EveryFiveMinutes = 300,
    }
    
    [Serializable]
    internal enum SameFileCooldown : int
    {
        OneSecond = 1,
        ThreeSeconds = 3,
        FiveSeconds = 5,
        TenSeconds = 10,
    }

    [Serializable]
    internal enum KeystrokeTimeout : int
    {
        TwoMinutes = 2,
        FiveMinutes = 5,
        TenMinutes = 10,
        FifteenMinutes = 15,
    }
    
    [Serializable]
    internal class DevStatsSettings : SavedData<DevStatsSettings>
    {
        [SerializeField]
        private bool m_isEnabled = true;
        [NonSerialized]
        public Action<bool> OnEnabledChanged;
        public bool IsEnabled
        {
            get => m_isEnabled;
            set
            {
                if (value == m_isEnabled)
                {
                    return;
                }
            
                m_isEnabled = value;
                OnEnabledChanged?.Invoke(m_isEnabled);
            }
        }
        public bool PrintDebugLogs = false;
        public StatsRefreshRate StatsRefreshRate = StatsRefreshRate.EveryFifteenMinutes;
        public PostFrequency PostFrequency = PostFrequency.EveryTwoMinutes;
        public SameFileCooldown SameFileCooldown = SameFileCooldown.FiveSeconds;
        
        // Wakatime settings
        public string APIKey;
        public KeystrokeTimeout KeystrokeTimeout = KeystrokeTimeout.FiveMinutes;
        
        public bool IsRunning()
        {
            return IsEnabled && !string.IsNullOrEmpty(APIKey);
        }
    }
}