using System;
using System.Collections.Generic;
using DevStatsSystem.Core.Wakatime.Payloads;
using UnityEngine;

namespace DevStatsSystem.Core.SerializedData
{
    [Serializable]
    internal struct TimespanDayStat
    {
        public long Day;
        public float TotalTime;
    }
    
    [Serializable]
    internal struct TimespanStats
    {
        public string TimespanName;
        
        public float TotalTime; // In seconds
        public float DailyAverageTime;
        public float CodeTime;
        public float AssetTime;
        
        public List<TimespanDayStat> DayStats;

        /// <summary>
        /// This assumes the SummariesPayload is already reconfigured to the right range.
        /// </summary>
        public TimespanStats(string name, in SummariesPayload summariesPayload)
        {
            TimespanName = name;
            TotalTime = summariesPayload.cumulative_total.seconds;
            DailyAverageTime = summariesPayload.daily_average.seconds;
            CodeTime = 0;
            AssetTime = 0;
            DayStats = new List<TimespanDayStat>();
            
            // Regular for loops to avoid copying a bunch of structs
            for (int i = 0; i < summariesPayload.data.Length; i++)
            {
                // Collect CodeTime and AssetTime
                for (int j = 0; j < summariesPayload.data[i].languages.Length; j++)
                {
                    if (summariesPayload.data[i].languages[j].name == "C#")
                    {
                        CodeTime += summariesPayload.data[i].languages[j].total_seconds;
                    }
                    else if (summariesPayload.data[i].languages[j].name == DevStats.GetLanguage())
                    {
                        AssetTime += summariesPayload.data[i].languages[j].total_seconds;
                    }
                }

                // Build the TimespanDayStat
                DayStats.Add(new TimespanDayStat()
                {
                    Day = DateTime.Parse(summariesPayload.data[i].range.date).Ticks,
                    TotalTime = summariesPayload.data[i].grand_total.total_seconds,
                });
            }
        }
    }
}