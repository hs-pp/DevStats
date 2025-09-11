using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace DevStatsSystem.Editor.Core
{
    public static class WakatimeWebRequests
    {
        private static string URL_PREFIX = "https://api.wakatime.com/api/v1/users/current/";

        public static void GetHeartbeatsRequest(Action<HeartbeatsDto> callback, Action<string> errorCallback = null)
        {
            CreateRequest($"heartbeats?date={DateTime.Now:yyyy-MM-dd}", callback, errorCallback);
        }

        public static void GetStatsRequest(Action<StatsDto> callback, Action<string> errorCallback = null)
        {
            CreateRequest("stats/all_time", callback, errorCallback);
        }

        public static void GetSummariesRequest(int numDays, Action<SummariesDto> callback, Action<string> errorCallback = null)
        {
            string startDate = DateTime.Now.AddDays(-numDays).ToString("yyyy-MM-dd");
            string endDate = DateTime.Now.ToString("yyyy-MM-dd");
            CreateRequest($"summaries?start={startDate}&end={endDate}&project={WakatimeCli.GetProjectName()}", callback, errorCallback);
        }

        public static void GetDayDurationRequest(Action<DurationsDto> callback, Action<string> errorCallback = null)
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            CreateRequest($"durations?date={date}&timeout=15", callback, errorCallback); // Specifying project returns garbage data. Just manually filter!!
        }
        
        private static void CreateRequest<T>(string url, Action<T> onComplete = null, Action<string> onError = null)
        {
            UnityWebRequest request = UnityWebRequest.Get(URL_PREFIX + url);
            string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(DevStatsSettings.Instance.APIKey + ":"));
            request.SetRequestHeader("Authorization", "Basic " + auth);
            request.SendWebRequest().completed += operation =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    T responseDto = JsonUtility.FromJson<T>(request.downloadHandler.text);
                    Debug.Log(request.downloadHandler.text);
                    onComplete?.Invoke(responseDto);
                }
                else
                {
                    Debug.LogError($"Wakatime WebRequest error: {request.error}");
                    onError?.Invoke(request.error);
                }
            };
        }
    }
}