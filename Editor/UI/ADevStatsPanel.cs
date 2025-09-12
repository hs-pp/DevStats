using UnityEngine.UIElements;

namespace DevStatsSystem.UI
{
    internal abstract class ADevStatsPanel : VisualElement
    {
        public abstract void OnShow();
        public abstract void OnHide();
    }
}