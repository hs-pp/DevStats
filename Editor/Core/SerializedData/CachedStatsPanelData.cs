using System;
using UnityEngine;

namespace DevStatsSystem.Core.SerializedData
{
    /// <summary>
    /// TODO: Auto update once a day and allow manual updates.
    /// </summary>
    [Serializable]
    internal class CachedStatsPanelData : SavedData<CachedStatsPanelData>
    {
        [SerializeField]
        private long m_lastUpdateTime;
        
        [SerializeField]
        private TodayStats m_todayStats;
        public TodayStats TodayStats => m_todayStats;
    }
}