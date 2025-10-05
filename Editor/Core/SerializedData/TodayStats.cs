using System;
using System.Collections.Generic;

namespace DevStatsSystem.Core.SerializedData
{
    [Serializable]
    public struct TimeSegment
    {
        public float StartTime; // In seconds where 12am is 0.
        public float Duration; // In seconds
    }
    
    [Serializable]
    public struct TodayStats
    {
        public List<TimeSegment> DayTimeSegments;
        public float TotalTime; // In seconds
        public float CodeTime;
        public float AssetTime;
    }
}