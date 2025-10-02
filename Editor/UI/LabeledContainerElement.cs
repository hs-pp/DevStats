using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.UI
{
    [UxmlElement]
    public partial class LabeledContainerElement : VisualElement
    {
        private const string UXML_PATH = "DevStats/UXML/LabeledContainerElement";
        private const string ROOT_TAG = "root";
        private const string TITLE_LABEL_TAG = "title-label";
        private const string CONTENT_AREA_TAG = "content-area";

        private VisualElement m_root;
        private Label m_titleLabel;
        private VisualElement m_contentArea;

        public override VisualElement contentContainer => m_contentArea;
        
        private string m_titleText = "Title";
        [UxmlAttribute]
        public string TitleText
        {
            get => m_titleText;
            set
            {
                m_titleText = value;
                m_titleLabel.text = m_titleText;
            }
        }

        private Color m_containerColor = Color.clear;
        [UxmlAttribute]
        public Color ContainerColor
        {
            get => m_containerColor;
            set
            {
                m_containerColor = value;
                m_root.style.backgroundColor = m_containerColor;
            }
        }

        public LabeledContainerElement()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            VisualTreeAsset uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);

            m_root = this.Q<VisualElement>(ROOT_TAG);
            m_titleLabel = this.Q<Label>(TITLE_LABEL_TAG);
            m_contentArea = this.Q<VisualElement>(CONTENT_AREA_TAG);

            m_titleLabel.text = m_titleText;
        }
    }
}