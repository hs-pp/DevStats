using System;
using DevStatsSystem.Editor.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace DevStatsSystem.Editor.UI
{
    public class HeartbeatElement : VisualElement
    {
        private const string UXML_PATH = "DevStats/UXML/HeartbeatElement";
        private const string FILE_OBJECTFIELD_TAG = "file-objectfield";
        private const string SAVE_ICON_TAG = "save-icon";
        private const string TIME_LABEL_TAG = "time-label";
        
        private ObjectField m_fileObjectField;
        private VisualElement m_saveIcon;
        private Label m_timeLabel;

        public HeartbeatElement()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_fileObjectField = this.Q<ObjectField>(FILE_OBJECTFIELD_TAG);
            m_saveIcon = this.Q<VisualElement>(SAVE_ICON_TAG);
            m_timeLabel = this.Q<Label>(TIME_LABEL_TAG);
        }

        public void BindHeartbeat(Heartbeat heartbeat)
        {
            m_fileObjectField.value = AssetDatabase.LoadAssetAtPath<Object>(heartbeat.FilePath.Replace(Application.dataPath, "Assets"));
            m_saveIcon.style.display = heartbeat.IsWrite ? DisplayStyle.Flex : DisplayStyle.None;
            DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds((long)heartbeat.Timestamp).ToLocalTime().DateTime;
            m_timeLabel.text = dateTime.ToString("hh:mm:sstt");
        }

        public void UnbindHeartbeat()
        {
            m_fileObjectField.value = null;
            m_saveIcon.style.display = DisplayStyle.None;
            m_timeLabel.text = string.Empty;
        }
    }
}