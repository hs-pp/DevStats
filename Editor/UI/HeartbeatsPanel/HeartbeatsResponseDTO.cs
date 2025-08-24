using System;
using System.Collections.Generic;

namespace DevStatsSystem.Editor.Core.DTOs
{
    [Serializable]
    public class HeartbeatsResponseDTO
    {
        public List<HeartbeatDTO> data;
        public string start;
        public string end;
    }
}