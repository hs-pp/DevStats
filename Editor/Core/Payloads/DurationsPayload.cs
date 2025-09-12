using System;
using UnityEngine;

namespace DevStatsSystem.Editor.Core
{
    [Serializable]
    public struct DurationsPayload
    {
        public DurationProjectDto[] data;
        public string start; // Start of time range as ISO 8601 UTC datetime
        public string end; // End of time range as ISO 8601 UTC datetime
        public string timezone; // Timezone used for this request in Olson Country/Region format
        
        public override string ToString()
        {
            return JsonUtility.ToJson(this, true);
        }
    }

    [Serializable]
    public struct DurationProjectDto
    {
        public string project;
        public float time; // Start of this duration as UNIX epoch; numbers after decimal point are fractions of a second
        public float duration; // Length of time of this duration in seconds
    }
}