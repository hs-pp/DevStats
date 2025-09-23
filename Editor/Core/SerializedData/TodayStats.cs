using System;
using System.Collections.Generic;
using DevStatsSystem.Core.Payloads;

namespace DevStatsSystem.Core.SerializedData
{
    [Serializable]
    internal struct WorkSegment
    {
        public long StartTime; // In ticks
        public float Duration; // In seconds
    }
    
    [Serializable]
    internal struct TodayStats
    {
        public List<WorkSegment> DayWorkSegments;
        public float TotalTime; // In seconds
        public int NumHeartbeats;
        public float CodePercentage;
        public float UnityAssetPercentage;

        public TodayStats(in DurationsPayload durations, in HeartbeatsPayload heartbeatsPayload,
            in SummaryDto todaySummary)
        {
            DayWorkSegments = new List<WorkSegment>();
            foreach (DurationInstanceDto durationInstance in durations.data)
            {
                if (durationInstance.project != DevStats.GetProjectName())
                {
                    continue;
                }

                DayWorkSegments.Add(new()
                {
                    StartTime = DateTimeOffset
                        .FromUnixTimeSeconds((long)durationInstance.time) // Integer part
                        .AddSeconds(durationInstance.time % 1) // Fractional part
                        .LocalDateTime.Ticks,
                    Duration = durationInstance.duration,
                });
            }

            NumHeartbeats = heartbeatsPayload.data.Count;
            TotalTime = todaySummary.grand_total.total_seconds;

            CodePercentage = 0;
            UnityAssetPercentage = 0;
            foreach (SummaryLanguageDto language in todaySummary.languages)
            {
                if (language.name == "C#")
                {
                    CodePercentage = language.percent;
                }
                else if (language.name == DevStats.GetLanguage())
                {
                    UnityAssetPercentage = language.percent;
                }
            }
            
            //Debug.Log($"Today:\n {JsonUtility.ToJson(this, true)}");
        }
    }
}