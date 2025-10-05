using System;
using System.Text;
using System.Threading.Tasks;
using DevStatsSystem.Core;
using DevStatsSystem.Core.SerializedData;
using DevStatsSystem.Wakatime.Payloads;
using UnityEngine;
using UnityEngine.Networking;

namespace DevStatsSystem.Wakatime{
    [Serializable]
    internal struct WebRequestResult
    {
        public UnityWebRequest.Result Result;
        public long ResponseCode;
        public string ErrorMessage;
    }
    
    internal static class WakatimeWebRequests
    {
        private static string URL_PREFIX = "https://api.wakatime.com/api/v1/users/current/";
        
        public static async Task<(WebRequestResult result, StatsPayload payload)> GetStatsRequest()
        {
            return await CreateRequest<StatsPayload>($"stats/all_time?timeout={(int)DevStatsSettings.Instance.KeystrokeTimeout}");
        }

        public static async Task<(WebRequestResult result, SummariesPayload payload)> GetSummariesRequest(int numDays)
        {
            string startDate = DateTime.Now.AddDays(-numDays + 1).ToString("yyyy-MM-dd");
            string endDate = DateTime.Now.ToString("yyyy-MM-dd");
            return await CreateRequest<SummariesPayload>($"summaries?start={startDate}&end={endDate}&project={DevStats.GetProjectName()}&timeout={(int)DevStatsSettings.Instance.KeystrokeTimeout}");
        }

        public static async Task<(WebRequestResult result, DurationsPayload payload)> GetDayDurationRequest()
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            return await CreateRequest<DurationsPayload>($"durations?date={date}&timeout={(int)DevStatsSettings.Instance.KeystrokeTimeout}"); // Specifying project returns garbage data. Just manually filter!!
        }
        
        private static async Task<(WebRequestResult result, T payload)> CreateRequest<T>(string url) where T : struct
        {
            UnityWebRequest request = UnityWebRequest.Get(URL_PREFIX + url);
            string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(DevStatsSettings.Instance.APIKey + ":"));
            request.SetRequestHeader("Authorization", "Basic " + auth);
            await request.SendWebRequest();

            T responseDto = JsonUtility.FromJson<T>(request.downloadHandler.text);
            WebRequestResult result = new WebRequestResult()
            {
                Result = request.result,
                ResponseCode = request.responseCode,
                ErrorMessage = request.error,
            };
            return (result, responseDto);
        }
    }
}