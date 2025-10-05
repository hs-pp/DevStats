using System.Collections.Generic;
using System.Threading.Tasks;
using DevStatsSystem.Core.SerializedData;

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

    public class StatsData
    {
        public CommandResult Result;
        
        public TodayStats TodayStats;
        public TimespanStats WeekStats;
        public AllTimeStats AllTimeStats;
    }
    
    public interface IDevStatsBackend
    {
        Task<CommandResult> Load();
        Task<CommandResult> SendHeartbeats(List<Heartbeat> heartbeats);
        Task<StatsData> GetStats();
    }
}