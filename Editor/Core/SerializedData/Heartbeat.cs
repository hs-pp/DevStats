using System;
using UnityEngine;

namespace DevStatsSystem.Core.SerializedData
{
    [Serializable]
    public struct Heartbeat
    {
        public string FilePath;
        public double Timestamp;
        public bool IsWrite;
        
        public override string ToString()
        {
            return $"<color=red>[HEARTBEAT]</color> {FilePath.Replace(Application.dataPath, "Assets")}, {IsWrite}, {Timestamp}";
        }
    }
}