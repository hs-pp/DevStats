using System;
using UnityEngine;

namespace DevStatsSystem.Editor.Core
{
    [Serializable]
    public struct SummariesPayload
    {
        public SummaryDto[] data;
        public SummariesCumulativeTotalDto cumulative_total;
        public SummariesDailyAverageDto daily_average;
        public string start; // start of time range as ISO 8601 UTC datetime
        public string end; // end of time range as ISO 8601 UTC datetime
        
        public override string ToString()
        {
            return JsonUtility.ToJson(this, true);
        }
    }

    [Serializable]
    public struct SummaryDto
    {
        public SummaryGrandTotalDto grand_total;
        public SummaryProjectDto[] projects; // Should be empty since we're specifying the project we want.
        public SummaryLanguageDto[] languages;
        public SummaryEntityDto[] entities;
        public SummaryRangeDto range;
    }

    [Serializable]
    public struct SummaryGrandTotalDto
    {
        public float total_seconds;
        public int hours;
        public int minutes;
    }

    [Serializable]
    public struct SummaryProjectDto
    {
        public string name;
        public float total_seconds;
        public float percent; // percent of time spent in this project
        public int hours;
        public int minutes;
    }

    [Serializable]
    public struct SummaryLanguageDto
    {
        public string name;
        public float total_seconds;
        public float percent; // percent of time spent in this language
        public int hours;
        public int minutes;
    }

    [Serializable]
    public struct SummaryEntityDto
    {
        public string name;
        public float total_seconds;
        public float percent; // percent of time spent in this entity
        public int hours;
        public int minutes;
        public int seconds;
    }

    [Serializable]
    public struct SummaryRangeDto
    {
        public string date;
        public string start;
        public string end;
        public string text;
        public string timezone;
    }

    [Serializable]
    public struct SummariesCumulativeTotalDto
    {
        public float seconds;
    }

    [Serializable]
    public struct SummariesDailyAverageDto
    {
        public float seconds;
    }
}