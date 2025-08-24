using System;

namespace DevStatsSystem.Editor.Core.DTOs
{
    [Serializable]
    public class HeartbeatDTO
    {
        public string entity;
        public float time;
        public bool is_write;
    }
}