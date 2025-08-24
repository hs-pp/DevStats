using System;
using UnityEngine;

namespace DevStatsSystem.Editor.Core
{
    [Serializable]
    public struct Heartbeat
    {
        // Internally these fields are named to match the payloads from Wakatime so we can deserialize objects directly as heartbeats.
        [SerializeField]
        private string entity;
        [SerializeField]
        private double time; // Unix epoch timestamp
        [SerializeField]
        private bool is_write; // Basically "was saved to disk"

        public string FilePath { get => entity; set => entity = value; }
        public double Timestamp { get => time; set => time = value; }
        public bool IsWrite { get => is_write; set => is_write = value; }

        public override string ToString()
        {
            return $"<color=red>[HEARTBEAT]</color> {FilePath.Replace(Application.dataPath, "Assets")}, {IsWrite}, {Timestamp}";
        }
    }
}