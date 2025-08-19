using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DevStats.Editor
{
    public static class DevStatsController
    {
        private const int SEND_INTERVAL = 120; // Should be every 2 minutes.
        
        private static WakatimeCliInterface m_wakatimeCli;
        private static HeartbeatProvider m_heartbeatProvider;
        private static List<Heartbeat> m_queuedHeartbeats = new();
        private static float m_lastHeartbeatSendTime = 0;
        
        [InitializeOnLoadMethod]
        static void RegisterAssemblyReloadEvents()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;

            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        public static float GetTimeRemainingDebug()
        {
            return SEND_INTERVAL - ((float)EditorApplication.timeSinceStartup - m_lastHeartbeatSendTime);
        }
        
        private static void OnBeforeAssemblyReload()
        {
            // Send any unsent heartbeats first.
            SendHeartbeat();
            
            m_heartbeatProvider.Deinitialize();
            EditorApplication.update -= OnEditorUpdate;
        }
        
        private static async void OnAfterAssemblyReload()
        {
            m_wakatimeCli = await WakatimeCliInterface.Get();
            m_heartbeatProvider = new();
            m_heartbeatProvider.Initialize(OnHeartbeatTriggered);
            EditorApplication.update += OnEditorUpdate;
        }
        
        private static void OnHeartbeatTriggered(Heartbeat heartbeat)
        {
            if (!DevStatsSettings.Get().IsRunning())
            {
                return;
            }
            
            m_queuedHeartbeats.Add(heartbeat);
        }

        private static void OnEditorUpdate()
        {
            if (!DevStatsSettings.Get().IsRunning())
            {
                return;
            }
            
            float timeSinceStartup = (float)EditorApplication.timeSinceStartup;
            if (m_wakatimeCli != null && m_queuedHeartbeats.Count > 0 && timeSinceStartup > m_lastHeartbeatSendTime + SEND_INTERVAL)
            {
                SendHeartbeat();
                m_lastHeartbeatSendTime = timeSinceStartup;
            }
        }

        private static void SendHeartbeat()
        {
            if (m_queuedHeartbeats.Count == 0)
            {
                return;
            }
            
            m_wakatimeCli.SendHeartbeats(m_queuedHeartbeats);
            m_queuedHeartbeats.Clear();
        }
    }
}