using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Application = UnityEngine.Device.Application;
using Debug = UnityEngine.Debug;

namespace DevStats.Editor
{
    internal struct CliResult
    {
        public bool Success;
        public string Output;
        public int NumFramesWaited;
        
        public override string ToString()
        {
            return $"[{Success}] {Output}\n NumFramesWaited: {NumFramesWaited} \n";
        }
    }
    
    /// <summary>
    /// Dont forget to add the Wakatime plugin to your IDE to get full coverage!
    /// </summary>
    internal class WakatimeCliInterface
    {
        private const string WINDOWS_CLI_NAME = "wakatime-cli-windows-amd64";
        private const string MAC_CLI_NAME = "wakatime-cli-darwin-arm64";
        private const int MAX_PROCESS_WAIT_TIME = 600; // in frames.
        
        private string m_cliPath;
        private string m_gitBranch;
        
        public WakatimeCliInterface(string cliPath)
        {
            m_cliPath = cliPath;
            
            // There's no chance the git branch will change without triggering a full recompile so we can just fetch it once.
            m_gitBranch = FetchGitBranchName();
        }
        
        #region Send Heartbeat
        public async void SendHeartbeats(List<Heartbeat> heartbeats)
        {
            if (heartbeats == null || heartbeats.Count == 0)
            {
                return;
            }
            
            Heartbeat heartbeat = heartbeats[0];
            
            CliArguments args = new CliArguments();
            args.AddKey()
                .AddFile(heartbeat.File)
                .AddTimestamp(heartbeat.Timestamp)
                .AddIsWrite(heartbeat.IsWrite)
                .AddCategory(heartbeat.Category)
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

            if (DevStatsSettings.Get().IsDebugMode)
            {
                Debug.Log("Sending Heartbeat to CLI.");
            }
            
            CliResult result = await CallCLI(args, stdin);
            if (DevStatsSettings.Get().IsDebugMode)
            {
                Debug.Log($"Send Heartbeat Success: {result.ToString()}");
            }
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
            string value = $"{{\"entity\":\"{heartbeat.File.Replace("\\", "\\\\").Replace("\"", "\\\"")}\", " +
                           $"\"timestamp\":{heartbeat.Timestamp.ToString(CultureInfo.InvariantCulture)}, " +
                           $"\"is_write\":{heartbeat.IsWrite.ToString().ToLower()}, " +
                           $"\"entity_type\":\"{GetEntityType()}\", " +
                           $"\"project\":\"{GetProjectName().Replace("\"", "\\\"")}\"";
            if (!string.IsNullOrEmpty(heartbeat.Category))
            {
                value += $", \"category\":\"{heartbeat.Category}\"";
            }
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
                process.WaitForExit();
                
                return process.StandardOutput.ReadLine();
            }
            catch (Exception e)
            {
                return null;
            }
        }
        #endregion

        private async Awaitable<CliResult> CallCLI(CliArguments arguments, string stdin = null)
        {
            return await RunCommand(m_cliPath, arguments.ToArgs(false), stdin);
        }
        
        private static async Awaitable<CliResult> RunCommand(string command, string args, string stdin = null)
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

            int numFramesWaited = 0;
            while (!process.HasExited)
            {
                await Awaitable.NextFrameAsync();
                numFramesWaited++;
                if (numFramesWaited > MAX_PROCESS_WAIT_TIME) // Process took too long.
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
                    NumFramesWaited = numFramesWaited,
                };
            }
            
            return new CliResult()
            {
                Success = true,
                Output = outputStr.ToString(),
                NumFramesWaited = numFramesWaited,
            };
        }

        public async Awaitable Help()
        {
            CliResult result = await CallCLI(CliArguments.Help());
            Debug.Log(result.ToString());
            Debug.Log(CliArguments.Help().ToString());
        }

        public async Awaitable Version()
        {
            CliResult result = await CallCLI(CliArguments.Version());
            Debug.Log(result.ToString());
        }
        
        #region Loading CLI
        public static async Awaitable<WakatimeCliInterface> Get()
        {
            string cliPath = await LoadCli();
            if (string.IsNullOrEmpty(cliPath))
            {
                Debug.LogError("Failed to get WakatimeCliInterface.");
                return null;
            }

            return new WakatimeCliInterface(cliPath);
        }
        
        private static async Awaitable<string> LoadCli()
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
                    Debug.LogError("Failed to determine application platform.");
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

            Debug.LogError($"Failed to find CLI {filename}.");
            return null;
        }
        
        private static async Awaitable<bool> MakeExecutable(string filePath)
        {
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.LinuxEditor)
            {
                CliResult result = await RunCommand("/bin/chmod", $"+x \"{filePath}\"");
                return result.Success;
            }
            else
            {
                //Debug.Log("Skipping chmod +x.");
                return true;
            }
        }
        #endregion
    }
}