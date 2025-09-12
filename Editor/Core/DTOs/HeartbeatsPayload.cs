using System;
using System.Collections.Generic;
using System.Text;

namespace DevStatsSystem.Editor.Core
{
    [Serializable]
    public class HeartbeatsPayload : AWebRequestPayload
    {
        public List<Heartbeat> data;
        public string start;
        public string end;
    }

}