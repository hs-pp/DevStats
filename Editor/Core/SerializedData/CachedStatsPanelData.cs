using System;

namespace DevStatsSystem.Core.SerializedData
{
    /// <summary>
    /// Cache the last received StatsData so we can reload it between recompiles until the next time the stats are fetched. 
    /// </summary>
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