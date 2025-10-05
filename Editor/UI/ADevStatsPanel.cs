using UnityEngine.UIElements;

namespace DevStatsSystem.UI
{
    internal abstract class ADevStatsPanel : VisualElement
    {
        public virtual void OnShow() { }
        public virtual void OnHide() { }
    }
}