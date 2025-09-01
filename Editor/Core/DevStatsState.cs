using System;
using System.Collections.Generic;
using UnityEngine;

namespace DevStatsSystem.Editor.Core
{
    [Serializable]
    internal class DevStatsState : SavedData<DevStatsState>
    {
        [SerializeField]
        private List<Heartbeat> m_queuedHeartbeats = new();
        [SerializeField]
        private long m_lastHeartbeatSendTime = 0;
        public long LastHeartbeatSendTime
        {
            get => m_lastHeartbeatSendTime;
            set
            {
                m_lastHeartbeatSendTime = value;
                OnLastHeartbeatSentTimeChanged?.Invoke();
            }
        }
        
        [NonSerialized]
        public Action OnQueuedHeartbeatsChanged;
        [NonSerialized]
        public Action OnLastHeartbeatSentTimeChanged;
        [NonSerialized]
        public Action OnFailedToSendHeartbeatsChanged;
        
        public int GetQueuedHeartbeatCount()
        {
            return m_queuedHeartbeats.Count;
        }

        public List<Heartbeat> GetQueuedHeartbeats()
        {
            return m_queuedHeartbeats;
        }

        public void AddHeartbeatToQueue(Heartbeat heartbeat)
        {
            m_queuedHeartbeats.Add(heartbeat);
            OnQueuedHeartbeatsChanged?.Invoke();
        }

        public void ClearQueuedHeartbeats()
        {
            m_queuedHeartbeats.Clear();
            OnQueuedHeartbeatsChanged?.Invoke();
        }
    }
}