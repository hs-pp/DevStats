using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DevStatsSystem.Editor.Core
{
    public struct Heartbeat
    {
        public string FilePath;
        public decimal Timestamp; // Unix epoch timestamp
        public bool IsWrite; // Basically "was saved to disk"

        public override string ToString()
        {
            return $"<color=red>[HEARTBEAT]</color> {FilePath.Replace(Application.dataPath, "Assets")}, {IsWrite}, {Timestamp}";
        }
    }
    
    /// <summary>
    /// This provider captures a number of in-editor actions:
    /// Scenes - Open, Close, Save, Change
    /// Prefabs - Save, Change
    /// ScriptableObjects - Create, Destroy, Save, Change (Including UXMLs and UIBuilder)
    /// </summary>
    internal class HeartbeatProvider
    {
        private Action<Heartbeat> TriggerHeartbeat;
        private string m_projectPath = Application.dataPath.Replace("Assets", "");
        private Heartbeat m_previousHeartbeat;

        public HeartbeatProvider(Action<Heartbeat> triggerHeartbeatCallback)
        {
            TriggerHeartbeat = triggerHeartbeatCallback;
        }
        
        public void Initialize()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            ObjectChangeEvents.changesPublished += OnChangesPublished;
            AssetSaveDetector.OnAssetSaved += OnAssetSaved;
        }

        public void Deinitialize()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosing -= OnSceneClosing;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            ObjectChangeEvents.changesPublished -= OnChangesPublished;
            AssetSaveDetector.OnAssetSaved -= OnAssetSaved;
        }
        
        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (!scene.IsValid())
            {
                return;
            }
            
            SendHeartbeat(SceneToSceneAsset(scene), false);
        }
        
        private void OnSceneClosing(Scene scene, bool removingScene)
        {
            if (!scene.IsValid())
            {
                return;
            }
            
            SendHeartbeat(SceneToSceneAsset(scene), false);
        }
        
        private void OnHierarchyChanged()
        {
            if (PrefabStageUtility.GetCurrentPrefabStage() is PrefabStage prefabStage) // Prefab hierarchy has changed.
            {
                SendHeartbeat(AssetDatabase.LoadAssetAtPath<Object>(prefabStage.assetPath), false);
            }
            else if (EditorWindow.focusedWindow.GetType() == InternalBridgeHelper.GetSceneHierarchyWindowType())
            {
                SendHeartbeat(SceneToSceneAsset(EditorSceneManager.GetActiveScene()), false);
            }
            // We don't care for any other scenario... for now.
        }
        
        private void OnChangesPublished(ref ObjectChangeEventStream stream)
        {
            for (int i = 0; i < stream.length; i++)
            {
                ObjectChangeKind changeKind = stream.GetEventType(i);
                switch (changeKind)
                {
                    case ObjectChangeKind.ChangeAssetObjectProperties:
                        stream.GetChangeAssetObjectPropertiesEvent(i, out ChangeAssetObjectPropertiesEventArgs propChange);
                        Object changeObject = EditorUtility.InstanceIDToObject(propChange.instanceId);
                        if (changeObject is ScriptableObject)
                        {
                            SendHeartbeat(changeObject, false);
                        }
                        break;
                    case ObjectChangeKind.CreateAssetObject:
                        stream.GetCreateAssetObjectEvent(i, out CreateAssetObjectEventArgs createChange);
                        Object createObject = EditorUtility.InstanceIDToObject(createChange.instanceId);
                        if (createObject is ScriptableObject)
                        {
                            SendHeartbeat(createObject, false);
                        }
                        break;
                    case ObjectChangeKind.DestroyAssetObject:
                        stream.GetDestroyAssetObjectEvent(i, out DestroyAssetObjectEventArgs destroyChange);
                        Object destroyObject = EditorUtility.InstanceIDToObject(destroyChange.instanceId);
                        if (destroyObject is ScriptableObject)
                        {
                            SendHeartbeat(destroyObject, false);
                        }
                        break;
                }
            }
        }

        private void OnAssetSaved(Object asset)
        {
            if (asset is ScriptableObject or SceneAsset or GameObject)
            {
                SendHeartbeat(asset, true);
            }
            else
            {
                DevStats.LogWarning($"Unknown Object saved ({asset.name}). Should DevStats track it?");
            }
        }

        private SceneAsset SceneToSceneAsset(Scene scene)
        {
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
        }
        
        private void SendHeartbeat(Object asset, bool isSaveAction)
        {
            if (asset == null)
            {
                DevStats.LogError("Cannot send heartbeat for null asset!");
                return;
            }

            string filePath = $"{m_projectPath}{AssetDatabase.GetAssetPath(asset)}";
            Heartbeat heartbeat = new Heartbeat()
            {
                FilePath = filePath,
                Timestamp = (decimal)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000,
                IsWrite = isSaveAction,
            };
            
            // Don't add this heartbeat if this one is not a write and is the same file as the last heartbeat and
            // within 1 second of the last one. This will let us skip asset changed triggers that happen right after
            // an asset saved is triggered.
            if (!heartbeat.IsWrite && m_previousHeartbeat.FilePath == heartbeat.FilePath && heartbeat.Timestamp - m_previousHeartbeat.Timestamp < 1)
            {
                return;
            }

            DevStats.Log(heartbeat.ToString());
            
            TriggerHeartbeat?.Invoke(heartbeat);
            m_previousHeartbeat = heartbeat;
        }
    }
    
    /// <summary>
    /// This needs to be a separate class to tap into OnWillSaveAssets callback.
    /// </summary>
    public class AssetSaveDetector : UnityEditor.AssetModificationProcessor
    {
        public static Action<Object> OnAssetSaved;
        public static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (var path in paths)
            {
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset != null)
                {
                    OnAssetSaved?.Invoke(asset);
                }
            }

            return paths;
        }
    }
}