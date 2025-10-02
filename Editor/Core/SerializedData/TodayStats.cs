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

        public TodayStats(in DurationsPayload durations, in HeartbeatsPayload heartbeatsPayload,
            in SummaryDto todaySummary)
        {
            DayTimeSegments = new List<TimeSegment>();
            foreach (DurationInstanceDto durationInstance in durations.data)
            {
                if (durationInstance.project != DevStats.GetProjectName())
                {
                    continue;
                }

                DateTime startTime = DateTimeOffset.FromUnixTimeSeconds((long)durationInstance.time).LocalDateTime;
                TimeSpan sinceMidnight = startTime - startTime.Date;
                DayTimeSegments.Add(new()
                {
                    StartTime = (float)sinceMidnight.TotalSeconds,
                    Duration = durationInstance.duration,
                });
            }

            TotalTime = (int)todaySummary.grand_total.total_seconds;

            CodeTime = 0;
            AssetTime = 0;
            foreach (SummaryLanguageDto language in todaySummary.languages)
            {
                if (language.name == "C#")
                {
                    CodeTime = language.total_seconds;
                }
                else if (language.name == DevStats.GetLanguage())
                {
                    AssetTime = language.total_seconds;
                }
            }
            
            //Debug.Log($"Today:\n {JsonUtility.ToJson(this, true)}");
        }
    }
}