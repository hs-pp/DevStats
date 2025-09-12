using System;
using UnityEngine;

namespace DevStatsSystem.Editor.Core
{
    [Serializable]
    public struct StatsPayload
    {
        public StatsDto data;
        
        public override string ToString()
        {
            return JsonUtility.ToJson(this, true);
        }
    }
    
    [Serializable]
    public struct StatsDto
    {
        public float total_seconds;
        public float daily_average;
        public StatsProjectDto[] projects;
        public StatsLanguageDto[] languages;
        public StatsBestDayDto best_day;
        public string range;
        public int holidays; // Number of days in this range with no coding time logged
        public int days_minus_holidays; // Number of days in this range excluding days with no coding time logged

        public bool is_stuck;
        public bool is_up_to_date;

        public string start; // Start of this time range as ISO 8601 UTC datetime
        public string end; // End of this time range as ISO 8601 UTC datetime
        public int timeout; // value of the user's keystroke timeout setting in minutes.
    }

    [Serializable]
    public struct StatsProjectDto
    {
        public string name;
        public float total_seconds;
        public float percent;
        public int hours;
        public int minutes;
    }

    [Serializable]
    public struct StatsLanguageDto
    {
        public string name;
        public float total_seconds;
        public float percent;
        public int hours;
        public int minutes;
        public int seconds;
    }

    [Serializable]
    public struct StatsBestDayDto
    {
        public string date; // Day with most coding time logged as Date string in YEAR-MONTH-DAY format
        public float total_seconds;
    }
}