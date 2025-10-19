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
    
    [Serializable]
    public struct AllTimeStats
    {
        public float ProjectTotalTime;
        
        public float GrandTotalTime;
        public float DailyAverageTime;
    }
    
    [Serializable]
    public class StatsData
    {
        public CommandResult Result;
        
        public TodayStats TodayStats;
        public TimespanStats WeekStats;
        public AllTimeStats AllTimeStats;
    }
}