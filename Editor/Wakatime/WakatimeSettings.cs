using System;
using DevStatsSystem.Core.SerializedData;
using DevStatsSystem.Wakatime.Payloads;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Networking;

namespace DevStatsSystem.Wakatime
{
    [Serializable]
    internal class WakatimeSettings : SavedData<WakatimeSettings>
    {
        // Wakatime settings
        [SerializeField]
        private string m_apiKey;
        public string APIKey
        {
            get => m_apiKey;
            set
            {
                m_apiKey = value;
                ReevaluateCanRun();
            }
        }

        [SerializeField]
        private UsersDto m_userData;

        public bool CanRun => !string.IsNullOrEmpty(m_userData.id);
        [NonSerialized]
        public Action<bool> OnCanRunChanged;

        private async void ReevaluateCanRun()
        {
            (WebRequestResult result, UsersPayload payload) returned = await WakatimeWebRequests.GetUsersRequest();
            
            bool prevValue = CanRun;
            m_userData = returned.payload.data; // Always update the data even when the request failed.
            
            if (prevValue != CanRun)
            {
                OnCanRunChanged?.Invoke(CanRun);
            }
        }
        
        public KeystrokeTimeout KeystrokeTimeout = KeystrokeTimeout.FiveMinutes;
    }
}