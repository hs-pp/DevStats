using System;
using DevStatsSystem.Editor.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.Editor.UI
{
    internal class SentHeartbeatsInstanceElement : VisualElement
    {
        private const string UXML_PATH = "DevStats/UXML/SentHeartbeatsInstanceElement";
        
        private const string HEARTBEATS_COUNT_LABEL_TAG = "heartbeats-count-label";
        private const string DATETIME_LABEL_TAG = "datetime-label";
        private const string FILECOUNT_LABEL_TAG = "file-count-label";
        
        private Label m_heartbeatsCountLabel;
        private Label m_dateTimeLabel;
        private Label m_fileCountLabel;

        public SentHeartbeatsInstanceElement()
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

        public void BindSentHeartbeatsInstance(SentHeartbeatsInstance sentHeartbeatsInstance)
        {
            m_heartbeatsCountLabel.text = $"{sentHeartbeatsInstance.NumHeartbeats} <color=red>\u2665</color>'s ";
            DateTime dateTime = new DateTime(sentHeartbeatsInstance.Timestamp);
            m_dateTimeLabel.text = $"{dateTime.ToString("G")}";
            m_fileCountLabel.text = $"{sentHeartbeatsInstance.NumUniqueFiles} Files";
            tooltip = sentHeartbeatsInstance.FormattedListOfAssetPaths;
        }

        public void UnbindSentHeartbeatsInstance()
        {
            m_heartbeatsCountLabel.text = "";
            m_dateTimeLabel.text = "";
            m_fileCountLabel.text = "";
            tooltip = "";
        }
    }
}