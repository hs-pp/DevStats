using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevStatsSystem.Editor.Core
{
    public static class DevStats
    {
        private const int SEND_INTERVAL = 120; // In seconds
        
        private static WakatimeCli m_wakatimeCli;
        private static HeartbeatProvider m_heartbeatProvider;
        private static List<Heartbeat> m_queuedHeartbeats = new();
        private static float m_lastHeartbeatSendTime = 0;
        
        [InitializeOnLoadMethod]
        static void RegisterAssemblyReloadEvents()
        {
            DevStatsSettings.OnEnabledChanged += OnDevStatsEnabledChanged;
            AssemblyReloadEvents.afterAssemblyReload -= Initialize;
            AssemblyReloadEvents.beforeAssemblyReload -= Deinitialize;
            
            if (DevStatsSettings.Instance.IsEnabled)
            {
                AssemblyReloadEvents.afterAssemblyReload += Initialize;
                AssemblyReloadEvents.beforeAssemblyReload += Deinitialize;
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
                AssemblyReloadEvents.afterAssemblyReload += Initialize;
                AssemblyReloadEvents.beforeAssemblyReload += Deinitialize;
                Initialize();
            }
            else
            {
                AssemblyReloadEvents.afterAssemblyReload -= Initialize;
                AssemblyReloadEvents.beforeAssemblyReload -= Deinitialize;
                Deinitialize();
            }
            
        }
        
        private static async void Initialize()
        {
            if (DevStatsSettings.Instance.IsEnabled && string.IsNullOrEmpty(DevStatsSettings.Instance.APIKey))
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
            EditorApplication.update += OnEditorUpdate;
        }
        
        private static void Deinitialize()
        {
            // Send any unsent heartbeats first.
            SendHeartbeat();
            
            m_heartbeatProvider.Deinitialize();
            EditorApplication.update -= OnEditorUpdate;
            
            Log("Deinitialized DevStats");
        }
        
        /// <summary>
        /// Anyone can call this.
        /// </summary>
        public static void TriggerHeartbeat(Heartbeat heartbeat)
        {
            if (!DevStatsSettings.Instance.IsRunning())
            {
                return;
            }
            
            m_queuedHeartbeats.Add(heartbeat);
        }

        private static void OnEditorUpdate()
        {
            if (!DevStatsSettings.Instance.IsRunning())
            {
                return;
            }
            
            float timeSinceStartup = (float)EditorApplication.timeSinceStartup;
            if (m_queuedHeartbeats.Count > 0 && timeSinceStartup > m_lastHeartbeatSendTime + SEND_INTERVAL)
            {
                SendHeartbeat();
                m_lastHeartbeatSendTime = timeSinceStartup;
            }
        }

        private static void SendHeartbeat()
        {
            if (m_wakatimeCli == null)
            {
                return;
            }
            
            if (m_queuedHeartbeats.Count == 0)
            {
                return;
            }
            
            _ = m_wakatimeCli.SendHeartbeats(m_queuedHeartbeats);
            m_queuedHeartbeats.Clear();
        }

        public static void Log(string log)
        {
            if (!DevStatsSettings.Instance.IsDebugMode)
            {
                return;
            }
            
            Debug.Log($"{GetLogHeader()} {log}");
        }

        public static void LogWarning(string warning)
        {
            if (!DevStatsSettings.Instance.IsDebugMode)
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
        
        public static float GetTimeRemainingDebug()
        {
            return SEND_INTERVAL - ((float)EditorApplication.timeSinceStartup - m_lastHeartbeatSendTime);
        }
    }
}