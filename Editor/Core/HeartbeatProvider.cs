using System;
using DevStatsSystem.Core.SerializedData;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DevStatsSystem.Core
{
    /// <summary>
    /// This provider captures a number of in-editor actions:
    /// Scenes - Open, Close, Save, Change
    /// Prefabs - Save, Change
    /// ScriptableObjects - Create, Destroy, Save, Change (Including UXMLs from UIBuilder)
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
            SceneAsset sceneAsset = SceneToSceneAsset(scene);
            if (sceneAsset == null)
            {
                return;
            }
            
            SendHeartbeat(sceneAsset, false);
        }
        
        private void OnSceneClosing(Scene scene, bool removingScene)
        {
            SceneAsset sceneAsset = SceneToSceneAsset(scene);
            if (sceneAsset == null)
            {
                return;
            }
            
            SendHeartbeat(sceneAsset, false);
        }
        
        private void OnHierarchyChanged()
        {
            if (PrefabStageUtility.GetCurrentPrefabStage() is PrefabStage prefabStage) // Prefab hierarchy has changed.
            {
                // Actually don't send this heartbeat. Prefabs normally auto-save.
                // SendHeartbeat(AssetDatabase.LoadAssetAtPath<Object>(prefabStage.assetPath), false);
            }
            else if (EditorWindow.mouseOverWindow != null && EditorWindow.mouseOverWindow.GetType() == InternalBridgeHelper.GetSceneHierarchyWindowType())
            {
                // If the mouse is inside the scene hierarchy window, we know the hierarchy change was for the scene.
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
                            SendHeartbeat(createObject, true);
                        }
                        break;
                    case ObjectChangeKind.DestroyAssetObject:
                        stream.GetDestroyAssetObjectEvent(i, out DestroyAssetObjectEventArgs destroyChange);
                        Object destroyObject = EditorUtility.InstanceIDToObject(destroyChange.instanceId);
                        if (destroyObject is ScriptableObject)
                        {
                            SendHeartbeat(destroyObject, true);
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
        }

        private SceneAsset SceneToSceneAsset(Scene scene)
        {
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
        }
        
        private void SendHeartbeat(Object asset, bool isSaveAction)
        {
            if (asset == null)
            {
                // Cannot send heartbeat for null asset!
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
            {
                // Change asset doesn't have a file path. Not sending heartbeat.
                return;
            }
            
            string filePath = $"{m_projectPath}{assetPath}";
            Heartbeat heartbeat = new Heartbeat()
            {
                FilePath = filePath,
                Timestamp = (double)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000,
                IsWrite = isSaveAction,
            };
            
            // Don't add this heartbeat if it is not a write and is the same file as the last heartbeat and
            // within x seconds of the last one. This will let us skip asset changed triggers that happen right after
            // an asset saved is triggered.
            if (!heartbeat.IsWrite && m_previousHeartbeat.FilePath == heartbeat.FilePath && heartbeat.Timestamp - m_previousHeartbeat.Timestamp < (int)DevStatsSettings.Instance.SameFileCooldown)
            {
                return;
            }
            
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