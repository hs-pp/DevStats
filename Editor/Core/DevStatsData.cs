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
        private List<FailedToSendInstance> m_failedToSendInstances = new();

        public Action OnSentHeartbeatsInstancesChanged;
        public Action OnFailedToSendInstancesChanged;
        
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
            fileToCount.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            StringBuilder stringBuilder = new();
            stringBuilder.Append("Files: ");
            foreach (var file in fileToCount)
            {
                stringBuilder.Append($"\n- {Path.GetFileName(file.Item1)} ({file.Item2})");
            }
            
            // Return unique file count and formatted string for tooltip.
            return (fileToCount.Count, stringBuilder.ToString());
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

            var metaData = DevStatsData.GetHeartbeatsMetaData(heartbeats);
            NumUniqueFiles = metaData.Item1;
            FormattedListOfAssetPaths = metaData.Item2;
        }
    }

    [Serializable]
    internal struct FailedToSendInstance
    {
        public long Timestamp;
        public List<Heartbeat> Heartbeats;
        public int NumUniqueFiles; // TODO: Please let's stop serializing these...
        public string FormattedListOfAssetPaths;

        public FailedToSendInstance(long timestamp, List<Heartbeat> heartbeats)
        {
            Timestamp = timestamp;
            Heartbeats = heartbeats;
            Debug.Log("Serializing failed to send with num heartbeats: " + heartbeats.Count);
            
            var metaData = DevStatsData.GetHeartbeatsMetaData(heartbeats);
            Debug.Log("FAILED TO SEND INSTNACEN UNIQUE COUNT: " + metaData.Item2);
            NumUniqueFiles = metaData.Item1;
            FormattedListOfAssetPaths = metaData.Item2;
        }
    }
}