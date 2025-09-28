using System;
using DevStatsSystem.Core.Payloads;

namespace DevStatsSystem.Core.SerializedData
{
    [Serializable]
    internal struct AllTimeStats
    {
        public float ProjectTotalTime;
        
        public float GrandTotalTime;
        public float DailyAverageTime;
        
        public AllTimeStats(in StatsPayload statsPayload)
        {
            GrandTotalTime = statsPayload.data.total_seconds;
            DailyAverageTime = statsPayload.data.daily_average;
            ProjectTotalTime = 0;
            for (int i = 0; i < statsPayload.data.projects.Length; i++)
            {
                if (statsPayload.data.projects[i].name == DevStats.GetProjectName())
                {
                    ProjectTotalTime = statsPayload.data.projects[i].total_seconds;
                }
            }
        }
    }
}