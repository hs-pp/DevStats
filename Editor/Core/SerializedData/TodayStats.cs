using System;
using System.Collections.Generic;
using DevStatsSystem.Core.Wakatime.Payloads;

namespace DevStatsSystem.Core.SerializedData
{
    [Serializable]
    internal struct TimeSegment
    {
        public float StartTime; // In seconds where 12am is 0.
        public float Duration; // In seconds
    }
    
    [Serializable]
    internal struct TodayStats
    {
        public List<TimeSegment> DayTimeSegments;
        public float TotalTime; // In seconds
        public float CodeTime;
        public float AssetTime;

        public TodayStats(in DurationsPayload durations, in SummaryDto todaySummary)
        {
            DayTimeSegments = new List<TimeSegment>();
            
            // Regular for loops to avoid copying a bunch of structs
            for (int i = 0; i < durations.data.Length; i++)
            {
                if (durations.data[i].project != DevStats.GetProjectName())
                {
                    continue;
                }
                
                DateTime startTime = DateTimeOffset.FromUnixTimeSeconds((long)durations.data[i].time).LocalDateTime;
                TimeSpan sinceMidnight = startTime - startTime.Date;
                DayTimeSegments.Add(new()
                {
                    StartTime = (float)sinceMidnight.TotalSeconds,
                    Duration = durations.data[i].duration,
                });
            }

            TotalTime = (int)todaySummary.grand_total.total_seconds;

            CodeTime = 0;
            AssetTime = 0;
            for (int i = 0; i < todaySummary.languages.Length; i++)
            {
                if (todaySummary.languages[i].name == "C#")
                {
                    CodeTime = todaySummary.languages[i].total_seconds;
                }
                else if (todaySummary.languages[i].name == DevStats.GetLanguage())
                {
                    AssetTime = todaySummary.languages[i].total_seconds;
                }
            }
        }
    }
}