using System;
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
        public bool IsEnabled
        {
            get => m_isEnabled;
            set
            {
                if (value == m_isEnabled)
                {
                    return;
                }
            
                bool prevRunning = IsRunning();
                m_isEnabled = value;
                if (prevRunning != IsRunning())
                {
                    OnIsRunningChanged?.Invoke(IsRunning());
                }
            }
        }
        public StatsRefreshRate StatsRefreshRate = StatsRefreshRate.EveryFifteenMinutes;
        public PostFrequency PostFrequency = PostFrequency.EveryTwoMinutes;
        public SameFileCooldown SameFileCooldown = SameFileCooldown.FiveSeconds;
        
        // Wakatime settings
        [SerializeField]
        private string m_apiKey;
        public string APIKey
        {
            get => m_apiKey;
            set
            {
                bool prevRunning = IsRunning();
                m_apiKey = value;
                if (prevRunning != IsRunning())
                {
                    OnIsRunningChanged?.Invoke(!prevRunning);
                }
            }
        }
        public KeystrokeTimeout KeystrokeTimeout = KeystrokeTimeout.FiveMinutes;
        
        public bool IsRunning()
        {
            return IsEnabled && !string.IsNullOrEmpty(APIKey);
        }
        [NonSerialized]
        public Action<bool> OnIsRunningChanged;
    }
}