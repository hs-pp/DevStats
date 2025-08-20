using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Application = UnityEngine.Device.Application;
using Debug = UnityEngine.Debug;

namespace DevStatsSystem.Editor.Core
{
    internal struct CliResult
    {
        public bool Success;
        public string Output;
        public int MillisecondsWaited;
        
        public override string ToString()
        {
            return $"[{Success}] {Output}\nFinished in {MillisecondsWaited} millisecond(s).";
        }
    }
    
    /// <summary>
    /// Dont forget to add the Wakatime plugin to your IDE to get full coverage!
    /// </summary>
    internal class WakatimeCliController
    {
        private const string WINDOWS_CLI_NAME = "wakatime-cli-windows-amd64";
        private const string MAC_CLI_NAME = "wakatime-cli-darwin-arm64";
        private const int MILLISECONDS_PER_WAIT = 16;
        private const int MAX_PROCESS_WAIT_TIME = MILLISECONDS_PER_WAIT * 1000;
        
        private string m_cliPath;
        private string m_gitBranch;
        
        private WakatimeCliController(string cliPath)
        {
            m_cliPath = cliPath;
            
            // There's no chance the git branch will change without triggering a full recompile so we can just fetch it once.
            m_gitBranch = FetchGitBranchName();
            
            string gitBranchDisplay = string.IsNullOrEmpty(m_gitBranch) ? "N/A" : m_gitBranch;
            DevStats.Log($"WakatimeCliController Created.\nCLI: {m_cliPath.Replace(Application.dataPath, "Assets")}\nGit branch: {gitBranchDisplay}");
        }
        
        #region Send Heartbeat
        public async void SendHeartbeats(List<Heartbeat> heartbeats)
        {
            if (heartbeats == null || heartbeats.Count == 0)
            {
                return;
            }
            
            Heartbeat heartbeat = heartbeats[0];
            WakatimeCliArguments args = new WakatimeCliArguments();
            args.AddKey()
                .AddFile(heartbeat.FilePath)
                .AddTimestamp(heartbeat.Timestamp)
                .AddIsWrite(heartbeat.IsWrite)
                .AddCategory(GetCategory())
                .AddEntityType(GetEntityType())
                .AddProject(GetProjectName())
                .AddPlugin();

            string stdin = null;
            heartbeats.RemoveAt(0);
            if (heartbeats.Count > 0)
            {
                stdin = GetSerializedExtraHeartbeats(heartbeats);
                args.AddExtraHeartbeats();
            }

            DevStats.Log($"Sending Heartbeats({heartbeats.Count + 1}) to Wakatime.");
            CliResult result = await CallCli(args, stdin);
            DevStats.Log($"Sent Heartbeat Success: {result.ToString()}");
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
                           $"\"project\":\"{GetProjectName().Replace("\"", "\\\"")}\"";
            if (!string.IsNullOrEmpty(m_gitBranch)) // For some reason Wakatime auto-finds the branch for the main heartbeat but not the extras???
            {
                value += $", \"branch_name\":\"{m_gitBranch}\"";
            }
            value += "}";

            return value;
        }
        
        private string GetProjectName()
        {
            return Application.productName;
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

        private async Task<CliResult> CallCli(WakatimeCliArguments arguments, string stdin = null)
        {
            return await RunCommand(m_cliPath, arguments.ToArgs(false), stdin);
        }
        
        private static async Task<CliResult> RunCommand(string command, string args, string stdin = null)
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
                return new CliResult()
                {
                    Success = false,
                    Output = "Error! Could not start process!",
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
                    return new CliResult()
                    {
                        Success = false,
                        Output = "Error! Process timed out!",
                    };
                }
            }

            if (!string.IsNullOrEmpty(errorStr.ToString())) // Is error.
            {
                return new CliResult()
                {
                    Success = false,
                    Output = errorStr.ToString(),
                    MillisecondsWaited = millisecondsWaited,
                };
            }
            
            return new CliResult()
            {
                Success = true,
                Output = outputStr.ToString(),
                MillisecondsWaited = millisecondsWaited,
            };
        }

        public async Task Help()
        {
            CliResult result = await CallCli(WakatimeCliArguments.Help());
            Debug.Log(result.ToString());
        }

        public async Task Version()
        {
            CliResult result = await CallCli(WakatimeCliArguments.Version());
            Debug.Log(result.ToString());
        }
        
        #region Loading CLI
        public static async Task<WakatimeCliController> Get()
        {
            string cliPath = await LoadCli();
            if (string.IsNullOrEmpty(cliPath))
            {
                DevStats.LogError($"Failed to get {nameof(WakatimeCliController)}.");
                return null;
            }

            return new WakatimeCliController(cliPath);
        }
        
        private static async Task<string> LoadCli()
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
                    DevStats.LogError("Failed to determine application platform.");
                    break;
            }

            bool success = await MakeExecutable(cliPath);
            if (!success)
            {
                cliPath = "";
            }
            
            return cliPath;
        }
        
        private static string FindCliPath(string filename)
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

            DevStats.LogError($"Failed to find CLI {filename}.");
            return null;
        }
        
        private static async Task<bool> MakeExecutable(string filePath)
        {
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.LinuxEditor)
            {
                CliResult result = await RunCommand("/bin/chmod", $"+x \"{filePath}\"");
                return result.Success;
            }
            else
            {
                // Skipping chmod +x
                return true;
            }
        }
        #endregion
    }
}