using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevStatsSystem.Core.SerializedData;
using UnityEngine.UIElements;

namespace DevStatsSystem.Core
{
    public enum CommandResultType
    {
        Success,
        Failure,
        Timeout,
    }
    
    public struct CommandResult
    {
        public CommandResultType Result;
        public string Output;
        public int MillisecondsWaited;
        
        public override string ToString()
        {
            return $"[{Result.ToString()}] {Output}\nFinished in {MillisecondsWaited} milliseconds.";
        }
    }

    public abstract class ABackendSettingsWidget : VisualElement
    {
    }
    
    public interface IDevStatsBackend
    {
        public bool CanRun { get; }
        public Action<bool> OnCanRunChanged { get; set; }
        
        Task<CommandResult> Load();
        Task<CommandResult> SendHeartbeats(List<Heartbeat> heartbeats);
        Task<StatsData> GetStats();
        Task<CommandResult> Unload();
        
        ABackendSettingsWidget CreateSettingsWidgetInstance();
    }
}