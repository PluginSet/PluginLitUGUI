using System;
using UnityEngine;
using UnityEngine.UI;

namespace PluginLit.UGUI
{
    [RequireComponent(typeof(RectTransform))]
    public class PanelBehavior: MonoBehaviour
    {
        public virtual Action<Action> EnterAction { get; }
        public virtual Action<Action> ExitAction { get; }

        protected UIWindowBase Owner { get; private set; }
        public RectTransform Panel { get; private set; }

        private Button _closeButton;
        
        public void OnInit(UIWindowBase owner)
        {
            Panel = GetComponent<RectTransform>();
            Owner = owner;
            
            InitCommonFrame();
            InitMembers();
            OnInitUI();
        }

        private void InitCommonFrame()
        {
            var frame = Panel.Find("frame");
            if (frame == null)
                return;
            
            var closeBtn = frame.Find("closeButton");
            if (closeBtn == null)
                return;

            var btn = closeBtn.GetComponent<Button>();
            if (btn == null)
                return;
            
            btn.onClick.AddListener(HideWindow);
            _closeButton = btn;
        }

        protected virtual void OnDestroy()
        {
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(HideWindow);
            Owner = null;
        }

        public void HideWindow()
        {
            if (Owner != null)
                Owner.Hide();
            else
                gameObject.SetActive(false);
        }

        protected virtual void InitMembers()
        {
            
        }

        protected virtual void OnInitUI()
        {
            
        }

        public virtual void SetData(params object[] data)
        {
            
        }

        public virtual void SetData()
        {
            
        }
        
        public virtual void BeforeShow()
        {
            
        }

        public virtual void OnShown()
        {
            
        }
        
        public virtual void BeforeHide()
        {
            
        }
        
        public virtual void OnHide()
        {
            
        }
    }
    
    public abstract class PanelBehavior<T>: PanelBehavior
    {
        public override void SetData(params object[] data)
        {
            if (data.Length == 1 && data[0] is T d)
                SetData(d);
        }

        protected abstract void SetData(T data);
    }
    
    public abstract class PanelBehavior<T1, T2>: PanelBehavior
    {
        public override void SetData(params object[] data)
        {
            if (data.Length == 2 && data[0] is T1 d1 && data[1] is T2 d2)
                SetData(d1, d2);
        }

        protected abstract void SetData(T1 arg1, T2 arg2);
    }
}