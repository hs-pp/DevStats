using System;
using System.Collections.Generic;
using UnityEngine;

namespace DevStatsSystem.Core.Payloads
{
    [Serializable]
    internal struct HeartbeatsPayload
    {
        public List<HeartbeatDto> data;
        public string start;
        public string end;
        
        public override string ToString()
        {
            return JsonUtility.ToJson(this, true);
        }
    }

    [Serializable]
    internal struct HeartbeatDto
    {
        public string entity;
        public double time; // Unix epoch timestamp\
        public bool is_write; // Basically "was saved to disk"
    }
}