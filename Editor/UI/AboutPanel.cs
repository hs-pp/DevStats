using UnityEngine;
using UnityEngine.UIElements;

namespace DevStatsSystem.UI
{
    internal class AboutPanel : ADevStatsPanel
    {
        private const string UXML_PATH = "DevStats/UXML/AboutPanel";
        private const string WAKATIME_LOGO_TAG = "wakatime-logo";
        private const string GITHUB_LINK_LABEL_TAG = "github-link-label";
        
        private VisualElement m_wakatimeLogo;
        private Label m_githubLinkLabel;

        public AboutPanel()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            VisualTreeAsset uxmlAsset = Resources.Load<VisualTreeAsset>(UXML_PATH);
            uxmlAsset.CloneTree(this);
            
            m_wakatimeLogo = this.Q<VisualElement>(WAKATIME_LOGO_TAG);
            m_wakatimeLogo.AddManipulator(new Clickable(() => { Application.OpenURL("https://wakatime.com/dashboard"); }));
            m_githubLinkLabel = this.Q<Label>(GITHUB_LINK_LABEL_TAG);
            m_githubLinkLabel.AddManipulator(new Clickable(() => { Application.OpenURL("https://github.com/hs-pp/DevStats"); }));

        }
    }
}