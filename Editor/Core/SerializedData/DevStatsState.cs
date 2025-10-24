using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace DevStatsSystem.Core.SerializedData
{
    /// <summary>
    /// Stores queued heartbeats, heartbeat history, and failed to send heartbeats to retry later.
    /// All data can be viewed in the Heartbeats panel. 
    /// </summary>
    [Serializable]
    [IsProjectSpecific]
    internal class DevStatsState : SavedData<DevStatsState>
    {
        #region Queued Heartbeats
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
        #endregion
        
        #region History
        private const int MAX_HISTORY = 20;
        
        [SerializeField]
        private List<SentHeartbeatsInstance> m_sentHeartbeatsInstances = new();
        public Action OnSentHeartbeatsInstancesChanged;

        public void AddSentHeartbeatInstance(List<Heartbeat> heartbeats)
        {
            m_sentHeartbeatsInstances.Add(new SentHeartbeatsInstance(DateTime.Now.Ticks, heartbeats));
            if (m_sentHeartbeatsInstances.Count == MAX_HISTORY)
            {
                m_sentHeartbeatsInstances.RemoveAt(0);
            }
            
            OnSentHeartbeatsInstancesChanged?.Invoke();
        }

        public List<SentHeartbeatsInstance> GetSentHeartbeatsInstances()
        {
            return m_sentHeartbeatsInstances;
        }
        #endregion
        
        #region Failed To Send
        [SerializeField]
        private List<FailedToSendInstance> m_failedToSendInstances = new();
        public Action OnFailedToSendInstancesChanged;
        
        public void AddFailedToSendInstance(List<Heartbeat> heartbeats)
        {
            m_failedToSendInstances.Add(new FailedToSendInstance(DateTime.Now.Ticks, heartbeats));
            OnFailedToSendInstancesChanged?.Invoke();
        }

        public List<FailedToSendInstance> GetFailedToSendInstances()
        {
            return m_failedToSendInstances;
        }

        public void ClearFailedToSendInstances()
        {
            m_failedToSendInstances.Clear();
            OnFailedToSendInstancesChanged?.Invoke();
        }
        #endregion
        
        #region Misc
        public static (int, string) GetHeartbeatsMetaData(List<Heartbeat> heartbeats)
        {
            Dictionary<string, int> fileCountsLookup = new();
            foreach (Heartbeat heartbeat in heartbeats)
            {
                string filePath = heartbeat.FilePath;
                if (!fileCountsLookup.ContainsKey(filePath))
                {
                    fileCountsLookup.Add(filePath, 0);
                }
                fileCountsLookup[filePath]++;
            }
            
            List<(string, int)> fileToCount = new();
            foreach (string filePath in fileCountsLookup.Keys)
            {
                fileToCount.Add((filePath, fileCountsLookup[filePath]));
            }
            fileToCount.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            StringBuilder stringBuilder = new();
            stringBuilder.Append("Files: ");
            foreach (var file in fileToCount)
            {
                stringBuilder.Append($"\n- {Path.GetFileName(file.Item1)} ({file.Item2})");
            }
            
            // Return unique file count and formatted string for tooltip.
            return (fileToCount.Count, stringBuilder.ToString());
        }
        #endregion
    }
    
    [Serializable]
    internal struct SentHeartbeatsInstance
    {
        public long Timestamp;
        public int NumHeartbeats;
        public int NumUniqueFiles;
        public string FormattedListOfAssetPaths;

        public SentHeartbeatsInstance(long timestamp, List<Heartbeat> heartbeats)
        {
            Timestamp = timestamp;
            NumHeartbeats = heartbeats.Count;

            var metaData = DevStatsState.GetHeartbeatsMetaData(heartbeats);
            NumUniqueFiles = metaData.Item1;
            FormattedListOfAssetPaths = metaData.Item2;
        }
    }

    [Serializable]
    internal struct FailedToSendInstance
    {
        public long Timestamp;
        public List<Heartbeat> Heartbeats;

        public FailedToSendInstance(long timestamp, List<Heartbeat> heartbeats)
        {
            Timestamp = timestamp;
            Heartbeats = heartbeats;
        }
    }
}