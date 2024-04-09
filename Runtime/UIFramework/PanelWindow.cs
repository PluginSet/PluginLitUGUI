using UnityEngine;

namespace PluginLit.UGUI
{
    public class PanelWindowBase: UIWindow
    {
        public virtual PanelBehavior PanelBehavior { get; protected set; }
        protected override bool DestroyOnHide => true;
        
        protected override void OnInit()
        {
            base.OnInit();
            if (PanelBehavior == null)
                PanelBehavior = Panel.GetComponent<PanelBehavior>();
            
            if (PanelBehavior != null)
            {
                PanelBehavior.OnInit(this);
                EnterAction = PanelBehavior.EnterAction;
                ExitAction = PanelBehavior.ExitAction;
            }
        }

        protected override void SetData(params object[] args)
        {
            if (PanelBehavior != null)
                PanelBehavior.SetData(args);
        }

        protected override void SetData()
        {
            if (PanelBehavior != null)
                PanelBehavior.SetData();
        }

        protected override void BeforeShow()
        {
            base.BeforeShow();
            if (PanelBehavior != null)
                PanelBehavior.BeforeShow();
        }

        protected override void OnShown()
        {
            base.OnShown();
            if (PanelBehavior != null)
                PanelBehavior.OnShown();
        }

        protected override void BeforeHide()
        {
            if (PanelBehavior != null)
                PanelBehavior.BeforeHide();
            base.BeforeHide();
        }

        protected override void OnHide()
        {
            if (PanelBehavior != null)
                PanelBehavior.OnHide();
            base.OnHide();
        }

        public override void HideImmediately()
        {
            if (DestroyOnHide)
                base.HideImmediately();
            else
                gameObject.SetActive(false);
        }
    }

    public abstract class PanelWindowBase<T> : PanelWindowBase where T : PanelBehavior
    {
        public T Behavior { get; private set; }

        public override PanelBehavior PanelBehavior
        {
            get => Behavior;
            
            protected set => Behavior = (T)value;
        }
    }

    public sealed class PanelWindow : PanelWindowBase
    {
        [SerializeField]
        private PanelBehavior panelBehavior;
        
        public bool destroyOnHide;
        
        public bool directShow;

        [SerializeField]
        private bool isModal;

        protected override bool DestroyOnHide => destroyOnHide;
        public override bool IsModal => isModal;

        public override PanelBehavior PanelBehavior { get => panelBehavior; protected set => panelBehavior = value; }

        private bool _isFirstAwake = true;

        protected override void Awake()
        {
            if (_isFirstAwake)
            {
                UIManager.RegisterWindow(this.name, this.GetType());
                
                if (!directShow)
                    this.gameObject.SetActive(false);
                
                _isFirstAwake = false;
            }
            
            Panel = panelBehavior.GetComponent<RectTransform>();
            base.Awake();
        }

    }
}