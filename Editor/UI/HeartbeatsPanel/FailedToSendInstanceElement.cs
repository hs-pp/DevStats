using System;
using DevStatsSystem.Editor.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.Editor.UI
{
    internal class FailedToSendInstanceElement : VisualElement
    {
        private const string UXML_PATH = "DevStats/UXML/FailedToSendInstanceElement";
        
        private const string HEARTBEATS_COUNT_LABEL_TAG = "heartbeats-count-label";
        private const string DATETIME_LABEL_TAG = "datetime-label";
        private const string FILECOUNT_LABEL_TAG = "file-count-label";
        
        private Label m_heartbeatsCountLabel;
        private Label m_dateTimeLabel;
        private Label m_fileCountLabel;

        public FailedToSendInstanceElement()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_heartbeatsCountLabel = this.Q<Label>(HEARTBEATS_COUNT_LABEL_TAG);
            m_dateTimeLabel = this.Q<Label>(DATETIME_LABEL_TAG);
            m_fileCountLabel = this.Q<Label>(FILECOUNT_LABEL_TAG);
        }

        public void BindFailedToSendInstance(FailedToSendInstance failedToSendInstance)
        {
            m_heartbeatsCountLabel.text = $"{failedToSendInstance.Heartbeats.Count} <color=red>\u2665</color>";
            DateTime dateTime = new DateTime(failedToSendInstance.Timestamp);
            m_dateTimeLabel.text = $"{dateTime.ToString("H:mm:ss:tt MM/dd/yy")}";
            
            var metaData = DevStatsState.GetHeartbeatsMetaData(failedToSendInstance.Heartbeats);

            m_fileCountLabel.text = $"{metaData.Item1} Files";
            tooltip = metaData.Item2;
        }

        public void UnbindFailedToSendInstance()
        {
            m_heartbeatsCountLabel.text = "";
            m_dateTimeLabel.text = "";
            m_fileCountLabel.text = "";
            tooltip = "";
        }
    }
}