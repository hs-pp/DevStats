using System;
using System.Collections.Generic;
using DevStatsSystem.Core.Wakatime.Payloads;
using UnityEngine;

namespace DevStatsSystem.Core.SerializedData
{
    [Serializable]
    public struct TimespanDayStat
    {
        public long Day;
        public float TotalTime;
    }
    
    [Serializable]
    public struct TimespanStats
    {
        public string TimespanName;
        
        public float TotalTime; // In seconds
        public float DailyAverageTime;
        public float CodeTime;
        public float AssetTime;
        
        public List<TimespanDayStat> DayStats;
        
    }
}