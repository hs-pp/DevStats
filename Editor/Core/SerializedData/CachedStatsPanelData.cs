using System;
using System.Collections.Generic;
using DevStatsSystem.Core.Payloads;
using UnityEngine;

namespace DevStatsSystem.Core.SerializedData
{
    [Serializable]
    internal class CachedStatsPanelData : SavedData<CachedStatsPanelData>
    {
        [SerializeField]
        private long m_lastUpdateTime;
        public long LastUpdateTime => m_lastUpdateTime;
        
        [SerializeField]
        private TodayStats m_todayStats;
        public ref TodayStats TodayStats => ref m_todayStats;

        public void UpdateData(in DurationsPayload durations, in HeartbeatsPayload heartbeats, in StatsPayload statsPayload, in SummariesPayload summaries)
        {
            m_lastUpdateTime = DateTime.UtcNow.Ticks;

            // Today Stats
            int todaySummaryIndex = GetIndexOfTodaySummary(summaries);
            if (todaySummaryIndex != -1)
            {
                m_todayStats = new TodayStats(in durations, in heartbeats, in summaries.data[todaySummaryIndex]);
            }
            else
            {
                m_todayStats = new TodayStats();
            }
            Save();
        }

        private int GetIndexOfTodaySummary(SummariesPayload summaries)
        {
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            for (int i = 0; i < summaries.data.Length; i++)
            {
                if (summaries.data[i].range.date == today)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}