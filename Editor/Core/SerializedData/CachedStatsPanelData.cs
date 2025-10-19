using System;

namespace DevStatsSystem.Core.SerializedData
{
    [Serializable]
    [IsProjectSpecific]
    internal class CachedStatsPanelData : SavedData<CachedStatsPanelData>
    {
        public TodayStats TodayStats;
        public TimespanStats WeekStats;
        public AllTimeStats AllTimeStats;
        public long LastUpdateTime;

        public void UpdateData(StatsData statsData)
        {
            TodayStats = statsData.TodayStats;
            WeekStats = statsData.WeekStats;
            AllTimeStats = statsData.AllTimeStats;
            LastUpdateTime = DateTime.UtcNow.Ticks;

            Save();
        }
    }
}