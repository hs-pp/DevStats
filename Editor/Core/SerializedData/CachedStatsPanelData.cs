using System;

namespace DevStatsSystem.Core.SerializedData
{
    [Serializable]
    [IsProjectSpecific]
    internal class CachedStatsPanelData : SavedData<CachedStatsPanelData>
    {
        public StatsData StatsData;
        public long LastUpdateTime;

        public void UpdateData(StatsData statsData)
        {
            StatsData = statsData;
            LastUpdateTime = DateTime.UtcNow.Ticks;

            Save();
        }
    }
}