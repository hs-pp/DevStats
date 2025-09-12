using System;
using UnityEngine;
using UnityEngine.Networking;

namespace DevStatsSystem.Editor.Core
{
    [Serializable]
    public abstract class AWebRequestPayload
    {
        public WebRequestResult WebRequestResult;

        public override string ToString()
        {
            return JsonUtility.ToJson(this, true);
        }
    }

    [Serializable]
    public struct WebRequestResult
    {
        public UnityWebRequest.Result Result;
        public long ResponseCode;
        public string ErrorMessage;
    }
}