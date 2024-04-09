using UnityEngine;

namespace PluginLit.UGUI
{
    public abstract class UIToast : MonoBehaviour, IUIEntity, IIconProtocol, ITextProtocol
    {
        public int Tag { get; set; }

        public abstract string text { set; get; }
        public abstract string icon { set; get; }
        
        private UIToastShowRule _rule;
        public bool IsPlayingShow { get; protected set; }

        public void SetRule(UIToastShowRule rule)
        {
            _rule = rule;
            rule.OnToastAdded(this);
        }

        public abstract void Show(float stayDuration);

        public abstract void OutImmediately();
        
        public abstract void Hide();

        public abstract void HideImmediately();

        protected void OnToastIn()
        {
            _rule?.OnToastIn(this);
        }
        
        protected void OnToastOut()
        {
            _rule?.OnToastOut(this);
        }

        protected virtual void OnDisable()
        {
            _rule?.OnToastRemove(this);
            _rule = null;
        }
    }
}