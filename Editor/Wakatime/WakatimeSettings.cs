using System;
using System.Text.RegularExpressions;
using DevStatsSystem.Core.SerializedData;
using DevStatsSystem.Wakatime.Payloads;
using UnityEngine;

namespace DevStatsSystem.Wakatime
{
    [Serializable]
    internal class WakatimeSettings : SavedData<WakatimeSettings>
    {
        // The same pattern wakatime-cli uses to validate --key. API keys are write-only, so the server can't confirm  one without sending a heartbeat. The format is all we can check.
        private static readonly Regex API_KEY_REGEX = new("^(waka_)?[a-f0-9]{8}-[a-f0-9]{4}-4[a-f0-9]{3}-[89ab][a-f0-9]{3}-[a-f0-9]{12}$");

        [SerializeField]
        private string m_apiKey;
        public string APIKey
        {
            get => m_apiKey;
            set
            {
                bool prevCanRun = CanRun;
                m_apiKey = value;
                if (prevCanRun != CanRun)
                {
                    OnCanRunChanged?.Invoke(CanRun);
                }

                OnSettingsReevaluated?.Invoke();
            }
        }

        [SerializeField]
        private string m_oauthToken;
        public string OAuthToken
        {
            get => m_oauthToken;
            set
            {
                m_oauthToken = value;
                ReevaluateOAuthToken();
            }
        }

        public KeystrokeTimeout KeystrokeTimeout = KeystrokeTimeout.FiveMinutes;

        [SerializeField]
        private UsersDto m_userData;
        public UsersDto UserData => m_userData;

        public bool CanRun => !string.IsNullOrEmpty(m_apiKey) && API_KEY_REGEX.IsMatch(m_apiKey);
        public bool CanFetchStats => !string.IsNullOrEmpty(m_oauthToken) && !string.IsNullOrEmpty(m_userData.id);
        [NonSerialized]
        public Action<bool> OnCanRunChanged;
        [NonSerialized]
        public Action OnSettingsReevaluated;

        private async void ReevaluateOAuthToken()
        {
            (WebRequestResult result, UsersPayload payload) returned = await WakatimeWebRequests.GetUsersRequest();
            m_userData = returned.payload.data; // Always update the data even when the request failed.

            OnSettingsReevaluated?.Invoke();
        }
    }
}
