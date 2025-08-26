using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DevStatsSystem.Editor.Core
{
    internal class DevStatsData : SavedData<DevStatsData>// better name pls
    {
        private const int MAX_HISTORY = 20;
        
        [SerializeField]
        private List<SentHeartbeatsInstance> m_sentHeartbeatsInstances = new();
        [SerializeField]
        private List<FailedToSendHeartbeats> m_failedToSendHeartbeats = new();

        public Action OnSentHeartbeatsInstancesChanged;
        
        public void AddSentHeartbeatInstance(List<Heartbeat> heartbeats)
        {
            m_sentHeartbeatsInstances.Add(new SentHeartbeatsInstance(DateTime.Now.Ticks, heartbeats));
            if (m_sentHeartbeatsInstances.Count == MAX_HISTORY)
            {
                m_sentHeartbeatsInstances.RemoveAt(0);
            }
            
            Save();
            OnSentHeartbeatsInstancesChanged?.Invoke();
        }

        public List<SentHeartbeatsInstance> GetSentHeartbeatsInstances()
        {
            return m_sentHeartbeatsInstances;
        }

        public void AddFailedToSendHeartbeats(List<Heartbeat> heartbeats)
        {
            m_failedToSendHeartbeats.Add(new FailedToSendHeartbeats()
            {
                Timestamp = DateTime.Now.Ticks,
                Heartbeats = heartbeats,
            });
        }
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
            
            List<string> filesByFrequency = heartbeats
                .Select(x => x.FilePath)
                .GroupBy(s => s) // Group by each string
                .OrderByDescending(g => g.Count()) // Order by frequency, descending
                .Select(g => g.Key) // Select only the unique string
                .ToList();

            StringBuilder formattedFilesBuilder = new();
            formattedFilesBuilder.AppendLine("Files: ");
            foreach (var file in filesByFrequency)
            {
                formattedFilesBuilder.AppendLine($"- {Path.GetFileName(file)}");
            }

            NumUniqueFiles = filesByFrequency.Count;
            FormattedListOfAssetPaths = formattedFilesBuilder.ToString();
        }
    }

    [Serializable]
    internal struct FailedToSendHeartbeats
    {
        public long Timestamp;
        public List<Heartbeat> Heartbeats;
    }
}