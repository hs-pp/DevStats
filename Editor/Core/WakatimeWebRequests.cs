using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DevStatsSystem.Editor.Core
{
    public static class WakatimeWebRequests
    {
        private static string URL_PREFIX = "https://api.wakatime.com/api/v1/users/current/";

        public static async Task<HeartbeatsPayload> GetHeartbeatsRequest()
        {
            return await CreateRequest<HeartbeatsPayload>($"heartbeats?date={DateTime.Now:yyyy-MM-dd}");
        }

        public static async Task<StatsPayload> GetStatsRequest()
        {
            return await CreateRequest<StatsPayload>("stats/all_time");
        }

        public static async Task<SummariesPayload> GetSummariesRequest(int numDays)
        {
            string startDate = DateTime.Now.AddDays(-numDays).ToString("yyyy-MM-dd");
            string endDate = DateTime.Now.ToString("yyyy-MM-dd");
            return await CreateRequest<SummariesPayload>($"summaries?start={startDate}&end={endDate}&project={WakatimeCli.GetProjectName()}");
        }

        public static async Task<DurationsPayload> GetDayDurationRequest()
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            return await CreateRequest<DurationsPayload>($"durations?date={date}&timeout=15"); // Specifying project returns garbage data. Just manually filter!!
        }
        
        private static async Task<T> CreateRequest<T>(string url) where T : AWebRequestPayload
        {
            UnityWebRequest request = UnityWebRequest.Get(URL_PREFIX + url);
            string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(DevStatsSettings.Instance.APIKey + ":"));
            request.SetRequestHeader("Authorization", "Basic " + auth);
            await request.SendWebRequest();

            T responseDto = JsonUtility.FromJson<T>(request.downloadHandler.text);
            responseDto.WebRequestResult = new WebRequestResult()
            {
                Result = request.result,
                ResponseCode = request.responseCode,
                ErrorMessage = request.error,
            };
            return responseDto;
        }
    }
}