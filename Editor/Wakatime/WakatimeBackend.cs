using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DevStatsSystem.Core;
using DevStatsSystem.Core.SerializedData;
using DevStatsSystem.Wakatime.Payloads;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace DevStatsSystem.Wakatime
{
    internal class WakatimeBackend : IDevStatsBackend
    {
        private const string WINDOWS_CLI_NAME = "wakatime-cli-windows-amd64";
        private const string MAC_CLI_NAME = "wakatime-cli-darwin-arm64";
        
        private const int MILLISECONDS_PER_WAIT = 16;
        private const int MAX_PROCESS_WAIT_TIME = 10000; // 10 seconds
        
        private string m_cliPath;
        private string m_gitBranch;
        
        #region Loading CLI
        public async Task<CommandResult> Load()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            m_cliPath = await LoadCli();
            if (string.IsNullOrEmpty(m_cliPath))
            {
                return new CommandResult()
                {
                    Result = CommandResultType.Failure,
                    MillisecondsWaited = 0,
                    Output = "Failed to get Wakatime Cli path.",
                };
            }
            
            // There's no chance the git branch will change without triggering a full recompile so we can just fetch it once.
            m_gitBranch = FetchGitBranchName();
            string gitBranchDisplay = string.IsNullOrEmpty(m_gitBranch) ? "N/A" : m_gitBranch;
            
            stopwatch.Stop();
            return new CommandResult()
            {
                Result = CommandResultType.Success,
                MillisecondsWaited = (int)stopwatch.ElapsedMilliseconds,
                Output = $"{nameof(WakatimeBackend)} Created.\nCLI: {m_cliPath.Replace(Application.dataPath, "Assets")}\nGit branch: {gitBranchDisplay}",
            };
        }
        
        private async Task<string> LoadCli()
        {
            string cliPath = "";
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    cliPath = FindCliPath(WINDOWS_CLI_NAME);
                    break;
                case RuntimePlatform.OSXEditor:
                    cliPath = FindCliPath(MAC_CLI_NAME);
                    break;
                default:
                    Debug.LogError("DevStats - Failed to determine application platform.");
                    break;
            }

            bool success = await MakeExecutable(cliPath);
            if (!success)
            {
                cliPath = "";
            }
            
            return cliPath;
        }
        
        private string FindCliPath(string filename)
        {
            string[] guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(filename));
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileName(assetPath) == filename)
                {
                    return Path.GetFullPath(assetPath);
                }
            }
            
            return null;
        }
        
        private async Task<bool> MakeExecutable(string filePath)
        {
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                CommandResult result = await RunCommand("/bin/chmod", $"+x \"{filePath}\"");
                return result.Result == CommandResultType.Success;
            }
            else
            {
                // Skipping chmod +x
                return true;
            }
        }
        #endregion
        
        #region CLI Commands
        private Task<CommandResult> CallCli(WakatimeCliArguments arguments, string stdin = null)
        {
            return RunCommand(m_cliPath, arguments.ToArgs(false), stdin);
        }
        
        private static async Task<CommandResult> RunCommand(string command, string args, string stdin = null)
        {
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = command,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = !string.IsNullOrEmpty(stdin),
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            Process process = Process.Start(psi);
            if (process == null)
            {
                return new CommandResult()
                {
                    Result = CommandResultType.Failure,
                    Output = "Could not start process!",
                };
            }
            
            try
            {
                process.PriorityClass = ProcessPriorityClass.AboveNormal;
            }
            catch (Exception) { /* Might fail if not Admin */ }
            
            if (!string.IsNullOrEmpty(stdin))
            {
                process.StandardInput.WriteLine($"{stdin}\n");
            }
            
            StringBuilder outputStr = new StringBuilder();
            StringBuilder errorStr = new StringBuilder();
            
            process.OutputDataReceived += (s, e) =>
            {
                outputStr.Append(e.Data);
            };
            process.ErrorDataReceived += (s, e) =>
            {
                errorStr.Append(e.Data);
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            int millisecondsWaited = 0;
            while (!process.HasExited)
            {
                await Task.Delay(MILLISECONDS_PER_WAIT);
                millisecondsWaited += MILLISECONDS_PER_WAIT;
                if (millisecondsWaited > MAX_PROCESS_WAIT_TIME) // Process took too long.
                {
                    process.Kill();
                    return new CommandResult()
                    {
                        Result = CommandResultType.Timeout,
                        Output = "Process timed out!",
                        MillisecondsWaited = millisecondsWaited,
                    };
                }
            }

            if (!string.IsNullOrEmpty(errorStr.ToString())) // Is error.
            {
                return new CommandResult()
                {
                    Result = CommandResultType.Failure,
                    Output = errorStr.ToString(),
                    MillisecondsWaited = millisecondsWaited,
                };
            }
            
            return new CommandResult()
            {
                Result = CommandResultType.Success,
                Output = outputStr.ToString(),
                MillisecondsWaited = millisecondsWaited,
            };
        }
        
        public async Task Help()
        {
            CommandResult result = await CallCli(WakatimeCliArguments.Help());
            Debug.Log(result.ToString());
        }
        
        public async Task Version()
        {
            CommandResult result = await CallCli(WakatimeCliArguments.Version());
            Debug.Log(result.ToString());
        }
        #endregion
        
        #region Send Heartbeat
        public async Task<CommandResult> SendHeartbeats(List<Heartbeat> heartbeats)
        {
            if (heartbeats == null || heartbeats.Count == 0)
            {
                return new CommandResult()
                {
                    Result = CommandResultType.Failure,
                    Output = "Empty heartbeats list.",
                    MillisecondsWaited = 0,
                };
            }
            
            Heartbeat heartbeat = heartbeats[0];
            WakatimeCliArguments args = new WakatimeCliArguments();
            args.AddKey()
                .AddFile(heartbeat.FilePath)
                .AddTimestamp(heartbeat.Timestamp)
                .AddCategory(GetCategory())
                .AddEntityType(GetEntityType())
                .AddLanguage(DevStats.GetLanguage())
                .AddProject(DevStats.GetProjectName())
                .AddPlugin();
            if (heartbeat.IsWrite)
            {
                args.AddIsWrite();
            }

            string stdin = null;
            if (heartbeats.Count > 1)
            {
                stdin = GetSerializedExtraHeartbeats(heartbeats.GetRange(1, heartbeats.Count - 1));
                args.AddExtraHeartbeats();
            }

            CommandResult result = await CallCli(args, stdin);
            return result;
        }
        
        private string GetSerializedExtraHeartbeats(List<Heartbeat> heartbeats)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append("[");
            for (int i = 0; i < heartbeats.Count; i++)
            {
                stringBuilder.Append(GetSerializedExtraHeartbeat(heartbeats[i]));
                if (i != heartbeats.Count - 1)
                {
                    stringBuilder.Append(", ");
                }
            }
            stringBuilder.Append("]");
            return stringBuilder.ToString();
        }
        
        /// <summary>
        /// Extra heartbeats have differently named fields on them???
        /// So we gotta handle these differently.
        /// </summary>
        private string GetSerializedExtraHeartbeat(Heartbeat heartbeat)
        {
            string value = $"{{\"entity\":\"{heartbeat.FilePath.Replace("\\", "\\\\").Replace("\"", "\\\"")}\", " +
                           $"\"timestamp\":{heartbeat.Timestamp.ToString(CultureInfo.InvariantCulture)}, " +
                           $"\"is_write\":{heartbeat.IsWrite.ToString().ToLower()}, " +
                           $"\"category\":\"{GetCategory()}\", " +
                           $"\"entity_type\":\"{GetEntityType()}\", " +
                           $"\"language\":\"{DevStats.GetLanguage()}\", " +
                           $"\"project\":\"{DevStats.GetProjectName().Replace("\"", "\\\"")}\"";
            if (!string.IsNullOrEmpty(m_gitBranch)) // For some reason Wakatime auto-finds the branch for the main heartbeat but not the extras???
            {
                value += $", \"branch_name\":\"{m_gitBranch}\"";
            }
            value += "}";

            return value;
        }
        
        private string GetCategory()
        {
            return "coding";
        }
        
        private string GetEntityType()
        {
            return "file";
        }
        
        private string FetchGitBranchName()
        {
            string directory = Application.dataPath;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    FileName = "git",
                    Arguments = "rev-parse --abbrev-ref HEAD",
                    UseShellExecute = false,
                    WorkingDirectory = directory,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };
                
                Process process = Process.Start(psi);
                if (process == null)
                {
                    return null;
                }
                process.WaitForExit();
                
                return process.StandardOutput.ReadLine();
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion
        
        #region Get Stats
        public async Task<StatsData> GetStats()
        {
            StatsData stats = new StatsData();
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                stats.Result = new CommandResult()
                {
                    Result = CommandResultType.Failure,
                    MillisecondsWaited = 0,
                    Output = "",
                };
                return stats;
            }
            
            var durationsPayload = await WakatimeWebRequests.GetDayDurationRequest();
            if (durationsPayload.result.Result != UnityWebRequest.Result.Success)
            {
                stats.Result = new CommandResult()
                {
                    Result = CommandResultType.Failure,
                    MillisecondsWaited = 0,
                    Output = $"WakatimeBackend: Failed to get durations payload:\nResponse Code: {durationsPayload.result.ResponseCode}\nOutput: {durationsPayload.result.ErrorMessage}",
                };
                return stats;
            }
            
            await Task.Delay(TimeSpan.FromSeconds(1));

            var statsPayload = await WakatimeWebRequests.GetStatsRequest();
            if (statsPayload.result.Result != UnityWebRequest.Result.Success)
            {
                stats.Result = new CommandResult()
                {
                    Result = CommandResultType.Failure,
                    MillisecondsWaited = 0,
                    Output = $"WakatimeBackend: Failed to get stats payload:\nResponse Code: {statsPayload.result.ResponseCode}\nOutput: {statsPayload.result.ErrorMessage}",
                };
                return stats;
            }

            if (!statsPayload.payload.data.is_up_to_date) // Let's try again after a few seconds.
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                statsPayload = await WakatimeWebRequests.GetStatsRequest();
                if (statsPayload.result.Result != UnityWebRequest.Result.Success)
                {
                    stats.Result = new CommandResult()
                    {
                        Result = CommandResultType.Failure,
                        MillisecondsWaited = 0,
                        Output = $"WakatimeBackend: Failed to get stats payload:\nResponse Code: {statsPayload.result.ResponseCode}\nOutput: {statsPayload.result.ErrorMessage}",
                    };
                    return stats;
                }
                
                // Just move on even if it's still not "up_to_date". It won't technically break anything and we can let
                // the user just refresh.
            }

            await Task.Delay(TimeSpan.FromSeconds(1));

            var summariesPayload = await WakatimeWebRequests.GetSummariesRequest(7);
            if (summariesPayload.result.Result != UnityWebRequest.Result.Success)
            {
                stats.Result = new CommandResult()
                {
                    Result = CommandResultType.Failure,
                    MillisecondsWaited = 0,
                    Output = $"WakatimeBackend: Failed to get summaries payload:\nResponse Code: {summariesPayload.result.ResponseCode}\nOutput: {summariesPayload.result.ErrorMessage}",
                };
                return stats;
            }
            
            if (EditorApplication.isCompiling)
            {
                stats.Result = new CommandResult()
                {
                    Result = CommandResultType.Failure,
                    MillisecondsWaited = 0,
                    Output = "Editor is compiling. Stopping run everything",
                };
                return stats;
            }
            
            // Update data.
            stats.TodayStats = CreateTodayStats(in durationsPayload.payload, in summariesPayload.payload);
            stats.WeekStats = CreateTimespanStats("Week", in summariesPayload.payload);
            stats.AllTimeStats = CreateAllTimeStats(in statsPayload.payload);       
            
            stopwatch.Stop();
            stats.Result = new CommandResult()
            {
                Result = CommandResultType.Success,
                MillisecondsWaited = (int)stopwatch.ElapsedMilliseconds,
                Output = "",
            };
            
            return stats;
        }

        private TodayStats CreateTodayStats(in DurationsPayload durationsPayload, in SummariesPayload summariesPayload)
        {
            // Get today summary
            int todayIndex = -1;
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            for (int i = 0; i < summariesPayload.data.Length; i++)
            {
                if (summariesPayload.data[i].range.date == today)
                {
                    todayIndex = i;
                }
            }
            if (todayIndex == -1)
            {
                return new TodayStats();
            }
            SummaryDto todaySummary = summariesPayload.data[todayIndex];
            
            // Build todayStats
            TodayStats todayStats = new TodayStats();
            todayStats.DayTimeSegments = new();
            // Regular for loops to avoid copying a bunch of structs
            for (int i = 0; i < durationsPayload.data.Length; i++)
            {
                if (durationsPayload.data[i].project != DevStats.GetProjectName())
                {
                    continue;
                }
                
                DateTime startTime = DateTimeOffset.FromUnixTimeSeconds((long)durationsPayload.data[i].time).LocalDateTime;
                TimeSpan sinceMidnight = startTime - startTime.Date;
                todayStats.DayTimeSegments.Add(new()
                {
                    StartTime = (float)sinceMidnight.TotalSeconds,
                    Duration = durationsPayload.data[i].duration,
                });
            }

            todayStats.TotalTime = (int)todaySummary.grand_total.total_seconds;

            for (int i = 0; i < todaySummary.languages.Length; i++)
            {
                if (todaySummary.languages[i].name == "C#")
                {
                    todayStats.CodeTime = todaySummary.languages[i].total_seconds;
                }
                else if (todaySummary.languages[i].name == DevStats.GetLanguage())
                {
                    todayStats.AssetTime = todaySummary.languages[i].total_seconds;
                }
            }

            return todayStats;
        }

        private TimespanStats CreateTimespanStats(string name, in SummariesPayload summariesPayload)
        {
            TimespanStats timespanStats = new TimespanStats();
            timespanStats.TimespanName = name;
            
            timespanStats.TotalTime = summariesPayload.cumulative_total.seconds;
            timespanStats.DailyAverageTime = summariesPayload.daily_average.seconds;
            timespanStats.DayStats = new List<TimespanDayStat>();
            
            // Regular for loops to avoid copying a bunch of structs
            for (int i = 0; i < summariesPayload.data.Length; i++)
            {
                // Collect CodeTime and AssetTime
                for (int j = 0; j < summariesPayload.data[i].languages.Length; j++)
                {
                    if (summariesPayload.data[i].languages[j].name == "C#")
                    {
                        timespanStats.CodeTime += summariesPayload.data[i].languages[j].total_seconds;
                    }
                    else if (summariesPayload.data[i].languages[j].name == DevStats.GetLanguage())
                    {
                        timespanStats.AssetTime += summariesPayload.data[i].languages[j].total_seconds;
                    }
                }

                // Build the TimespanDayStat
                timespanStats.DayStats.Add(new TimespanDayStat()
                {
                    Day = DateTime.Parse(summariesPayload.data[i].range.date).Ticks,
                    TotalTime = summariesPayload.data[i].grand_total.total_seconds,
                });
            }

            return timespanStats;
        }

        private AllTimeStats CreateAllTimeStats(in StatsPayload statsPayload)
        {
            AllTimeStats allTimeStats = new AllTimeStats();
            
            allTimeStats.GrandTotalTime = statsPayload.data.total_seconds;
            allTimeStats.DailyAverageTime = statsPayload.data.daily_average;
            for (int i = 0; i < statsPayload.data.projects.Length; i++)
            {
                if (statsPayload.data.projects[i].name == DevStats.GetProjectName())
                {
                    allTimeStats.ProjectTotalTime = statsPayload.data.projects[i].total_seconds;
                    break;
                }
            }

            return allTimeStats;
        }
        #endregion
    }
}