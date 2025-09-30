using System;
using DevStatsSystem.Core.Payloads;

namespace DevStatsSystem.Core.SerializedData
{
    [Serializable]
    internal struct TimespanStats
    {
        public string TimespanName;
        
        public float TotalTime; // In seconds
        public float DailyAverageTime;
        public float CodeTime;
        public float AssetTime;

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
            for (int i = 0; i < summariesPayload.data.Length; i++)
            {
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
            }
        }
    }
}