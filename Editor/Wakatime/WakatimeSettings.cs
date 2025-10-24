using System;
using DevStatsSystem.Core.SerializedData;
using DevStatsSystem.Wakatime.Payloads;
using UnityEngine;

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
        
        public KeystrokeTimeout KeystrokeTimeout = KeystrokeTimeout.FiveMinutes;

        [SerializeField]
        private UsersDto m_userData;
        public UsersDto UserData => m_userData;
        
        public bool CanRun => !string.IsNullOrEmpty(m_userData.id);
        [NonSerialized]
        public Action<bool> OnCanRunChanged;
        [NonSerialized]
        public Action OnCanRunReevaluated;

        private async void ReevaluateCanRun()
        {
            (WebRequestResult result, UsersPayload payload) returned = await WakatimeWebRequests.GetUsersRequest();

            bool prevValue = CanRun;
            m_userData = returned.payload.data; // Always update the data even when the request failed.
            
            if (prevValue != CanRun)
            {
                OnCanRunChanged?.Invoke(CanRun);
            }

            OnCanRunReevaluated?.Invoke();
        }
    }
}