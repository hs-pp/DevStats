using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevStatsSystem.Editor.Core
{
    public static class DevStats
    {
        public const int SEND_INTERVAL = 120; // In seconds
        private const int SEND_INTERVAL_NANOSECONDS = SEND_INTERVAL * 10000000;
        
        private static WakatimeCli m_wakatimeCli;
        private static HeartbeatProvider m_heartbeatProvider;
        private static DevStatsState m_state;
        private static DevStatsData m_data;
        private static DevStatsSettings m_settings;
        
        [InitializeOnLoadMethod]
        static void RegisterAssemblyReloadEvents()
        {
            m_settings = DevStatsSettings.Instance;
            m_settings.OnEnabledChanged += OnDevStatsEnabledChanged;

            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }
        
        private static void OnAfterAssemblyReload()
        {
            if (m_settings.IsEnabled)
            {
                Initialize();
            }
        }

        private static void OnBeforeAssemblyReload()
        {
            m_settings.Save();
            
            if (m_settings.IsEnabled)
            {
                Deinitialize();
            }
        }

        private static void OnDevStatsEnabledChanged(bool newValue, bool prevValue)
        {
            if (newValue == prevValue)
            {
                return;
            }
            
            if (newValue)
            {
                Initialize();
            }
            else
            {
                Deinitialize();
            }
        }
        
        private static async void Initialize()
        {
            if (m_settings.IsEnabled && string.IsNullOrEmpty(m_settings.APIKey))
            {
                LogError("DevStats is enabled but API key is missing. Open the DevStats window from \"Window/DevStats\" and set the API key!");
            }
            else
            {
                Log("Initialized DevStats.");
            }

            m_wakatimeCli = await WakatimeCli.Get();
            if (m_wakatimeCli == null)
            {
                return;
            }
            m_heartbeatProvider = new(TriggerHeartbeat);
            m_heartbeatProvider.Initialize();

            m_state = DevStatsState.Instance;
            m_data = DevStatsData.Instance;
            EditorApplication.update += OnEditorUpdate;
        }
        
        private static void Deinitialize()
        {
            m_state.Save();
            m_data.Save();
            
            m_heartbeatProvider.Deinitialize();
            EditorApplication.update -= OnEditorUpdate;
            
            Log("Deinitialized DevStats");
        }
        
        /// <summary>
        /// Anyone can call this.
        /// </summary>
        public static void TriggerHeartbeat(Heartbeat heartbeat)
        {
            if (!m_settings.IsRunning())
            {
                return;
            }
            
            m_state.AddHeartbeatToQueue(heartbeat);
        }

        private static void OnEditorUpdate()
        {
            if (!m_settings.IsRunning())
            {
                return;
            }

            long timeSinceStartup = DateTime.Now.Ticks;
            if (m_state.GetQueuedHeartbeatCount() > 0 && timeSinceStartup > m_state.LastHeartbeatSendTime + SEND_INTERVAL_NANOSECONDS)
            {
                SendHeartbeat(m_state.GetQueuedHeartbeats());
                m_state.ClearQueuedHeartbeats();
                m_state.LastHeartbeatSendTime = timeSinceStartup;
            }
        }

        private static async void SendHeartbeat(List<Heartbeat> heartbeats)
        {
            if (m_wakatimeCli == null)
            {
                return;
            }

            if (heartbeats == null || heartbeats.Count == 0)
            {
                return;
            }
            
            List<Heartbeat> localHeartbeats = new List<Heartbeat>(heartbeats);
            CliResult result = await m_wakatimeCli.SendHeartbeats(localHeartbeats);
            if (result.Result == CliResultType.Success)
            {
                m_data.AddSentHeartbeatInstance(localHeartbeats);
            }
            else
            {
                m_data.AddFailedToSendInstance(localHeartbeats);
            }
        }

        private static void SendToFailed(List<Heartbeat> heartbeats)
        {
            if (heartbeats == null || heartbeats.Count == 0)
            {
                return;
            }
            
            m_data.AddFailedToSendInstance(new(heartbeats));
        }

        public static void RetryFailedHeartbeats()
        {
            if (m_data.GetFailedToSendInstances().Count == 0)
            {
                return;
            }

            List<Heartbeat> allFailedHeartbeats = new();
            foreach (FailedToSendInstance instance in m_data.GetFailedToSendInstances())
            {
                allFailedHeartbeats.AddRange(instance.Heartbeats);
            }
            SendHeartbeat(allFailedHeartbeats);
            m_data.ClearFailedToSendInstances();
        }

        public static void Log(string log)
        {
            if (!m_settings.IsDebugMode)
            {
                return;
            }
            
            Debug.Log($"{GetLogHeader()} {log}");
        }

        public static void LogWarning(string warning)
        {
            if (!m_settings.IsDebugMode)
            {
                return;
            }
            
            Debug.LogWarning($"{GetLogHeader()} {warning}");
        }

        public static void LogError(string error)
        {
            Debug.LogError($"{GetLogHeader()} {error}");
        }

        private static string GetLogHeader()
        {
            return "<b><color=#F37828>[DevStats]</color></b>";
        }
    }
}