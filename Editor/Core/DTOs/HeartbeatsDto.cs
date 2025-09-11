using System;
using System.Collections.Generic;
using System.Text;

namespace DevStatsSystem.Editor.Core
{
    [Serializable]
    public class HeartbeatsDto
    {
        public List<Heartbeat> data;
        public string start;
        public string end;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("HeartbeatsDto:\n");
            sb.Append($"{start} - {end}\n");
            for (int i = 0; i < data.Count; i++)
            {
                sb.Append($"{data[i]}\n");
            }

            return sb.ToString();
        }
    }

}