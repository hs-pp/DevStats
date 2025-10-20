using System;
using UnityEngine;

namespace DevStatsSystem.Wakatime.Payloads
{
    [Serializable]
    internal struct UsersPayload
    {
        public UsersDto data;
        
        public override string ToString()
        {
            return JsonUtility.ToJson(this, true);
        }
    }

    [Serializable]
    internal struct UsersDto
    {
        public string id;
        public string username;
        public string plan;
    }
}