using System;
using DevStatsSystem.Core.Wakatime.Payloads;

namespace DevStatsSystem.Core.SerializedData
{
    [Serializable]
    public struct AllTimeStats
    {
        public float ProjectTotalTime;
        
        public float GrandTotalTime;
        public float DailyAverageTime;
    }
}