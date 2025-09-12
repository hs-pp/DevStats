using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DevStatsSystem.Editor.Core
{
    [Serializable]
    public struct HeartbeatsPayload
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
    public struct HeartbeatDto
    {
        public string entity;
        public double time; // Unix epoch timestamp\
        public bool is_write; // Basically "was saved to disk"
    }
}