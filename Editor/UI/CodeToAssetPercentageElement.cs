using DevStatsSystem.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.UI
{
    [UxmlElement]
    public partial class CodeToAssetPercentageElement : VisualElement
    {
        private const string UXML_PATH = "DevStats/UXML/CodeToAssetPercentageElement";
        private const string CODE_PERCENTAGE_ELEMENT_TAG = "code-percentage-element";
        private const string ASSET_PERCENTAGE_ELEMENT_TAG = "asset-percentage-element";

        private static Color CODE_NORMAL_COLOR = Color.royalBlue;
        private static Color CODE_HOVER_COLOR = Color.dodgerBlue;
        
        private static Color ASSET_NORMAL_COLOR = Color.darkOrange;
        private static Color ASSET_HOVER_COLOR = Color.sandyBrown;
        
        private VisualElement m_codePercentageElement;
        private VisualElement m_assetPercentageElement;

        public CodeToAssetPercentageElement()
        {
            CreateLayout();
            SetData(0, 0);
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);

            m_codePercentageElement = this.Q<VisualElement>(CODE_PERCENTAGE_ELEMENT_TAG);
            m_assetPercentageElement = this.Q<VisualElement>(ASSET_PERCENTAGE_ELEMENT_TAG);

            m_codePercentageElement.style.backgroundColor = CODE_NORMAL_COLOR;
            m_codePercentageElement.RegisterCallback<MouseEnterEvent>(_ => { m_codePercentageElement.style.backgroundColor = CODE_HOVER_COLOR; });
            m_codePercentageElement.RegisterCallback<MouseLeaveEvent>(_ => { m_codePercentageElement.style.backgroundColor = CODE_NORMAL_COLOR; });
            
            m_assetPercentageElement.style.backgroundColor = ASSET_NORMAL_COLOR;
            m_assetPercentageElement.RegisterCallback<MouseEnterEvent>(_ => { m_assetPercentageElement.style.backgroundColor = ASSET_HOVER_COLOR; });
            m_assetPercentageElement.RegisterCallback<MouseLeaveEvent>(_ => { m_assetPercentageElement.style.backgroundColor = ASSET_NORMAL_COLOR; });
        }

        public void SetData(float codeTime, float assetTime)
        {
            if (codeTime == 0 && assetTime == 0)
            {
                m_codePercentageElement.style.display = DisplayStyle.Flex;
                m_codePercentageElement.tooltip = "-";

                m_assetPercentageElement.style.display = DisplayStyle.Flex;
                m_assetPercentageElement.tooltip = "-";
            }
            else
            {
                float totalTime = codeTime + assetTime;

                m_codePercentageElement.style.flexGrow = codeTime / totalTime;
                m_codePercentageElement.style.display = codeTime > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                m_codePercentageElement.tooltip =
                    $"{DevStats.SecondsToFormattedTimePassed(codeTime)}\n{(codeTime / totalTime * 100):F1}%";

                m_assetPercentageElement.style.flexGrow = assetTime / totalTime;
                m_assetPercentageElement.style.display = assetTime > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                m_assetPercentageElement.tooltip =
                    $"{DevStats.SecondsToFormattedTimePassed(assetTime)}\n{(assetTime / totalTime * 100):F1}%";
            }
        }
    }
}