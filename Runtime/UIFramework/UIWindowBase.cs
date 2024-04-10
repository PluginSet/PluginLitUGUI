using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PluginLit.UGUI
{
    [RequireComponent(typeof(RectTransform))]
    public class UIWindowBase: BranchAssetAdaptor, IUIEntity
    {
        public Action<Action> EnterAction { get; set; }
        public Action<Action> ExitAction { get; set; }

        public int Tag { get; set; }

        public virtual bool IsModal { get; internal set; }

        public bool IsShown { get; protected set; }
        
        private bool _asyncOpen { get; set; }

        [SerializeField]
        private RectTransform panel;

        protected RectTransform Panel
        {
            get => panel;

            set
            {
                if (panel == value)
                    return;

                _inited = false;
                if (panel != null)
                {
                    var obj = panel.gameObject;
                    if (obj != null)
                    {
                        Addressables.ReleaseInstance(obj);
                        GameObject.Destroy(obj);
                    }
                }
                panel = value;
                if (panel != null && !_inited)
                {
                    OnInit();
                    _inited = true;
                }
            }
        }
        
        private RectTransform _transform { get; set; }

        private bool _inited;

        public virtual RectTransform GetContent()
        {
            return Panel;
        }

        public void Constructor(string runtimeKey, bool asyncOpen = true)
        {
            Constructor(new AssetReference(runtimeKey), asyncOpen);
        }

        public void Constructor(AssetReference reference, bool asyncOpen = true)
        {
            assets = new BranchAssets { { UIBranch.DefaultBranch, reference } };
            Constructor(asyncOpen);
        }

        public void Constructor(bool asyncOpen = true)
        {
            _asyncOpen = asyncOpen;
            InitRoot();
        }
        

        protected void InitRoot(bool setup = true)
        {
            _transform = gameObject.GetComponent<RectTransform>();
            if (_transform == null)
                _transform = gameObject.AddComponent<RectTransform>();
            
            _transform.anchorMin = Vector2.zero;
            _transform.anchorMax = Vector2.one;
            
            if (setup)
                SetupAsset();
        }

        protected override void OnAssetReferenceChanged(AssetReference reference)
        {
            var handler = reference.InstantiateAsync(_transform, false);
            if (_asyncOpen)
            {
                handler.Completed +=
                    delegate(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> handle)
                    {
                        OnPanelCreated(handle.Result);
                    };
            }
            else
            {
                handler.WaitForCompletion();
                OnPanelCreated(handler.Result);
            }
        }

        private void OnPanelCreated(GameObject obj)
        {
            Panel = obj.GetComponent<RectTransform>();
            
            if (gameObject.activeInHierarchy && !IsShown)
                DoShowAnimation();
        }

        protected virtual void Awake()
        {
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Panel == null)
                return;
            
            if (!_inited)
            {
                OnInit();
                _inited = true;
            }
            
            if (Panel != null && !IsShown)
                DoShowAnimation();
        }

        protected override void OnDisable()
        {
            if (!_inited)
                return;
            
            if (IsShown)
            {
                IsShown = false;
                BeforeHide();
            }
            if (IsModal)
                UIManager.AdjustModalLayer();
            OnHide();
        }

        protected override void OnDestroy()
        {
            Panel = null;
        }

        protected virtual void OnInit()
        {
            
        }

        protected virtual void DoShowAnimation()
        {
            BeforeShow();

            if (EnterAction != null)
            {
                EnterAction.Invoke(Show);
            }
            else
            {
                Show();
            }
        }

        private void Show()
        {
            IsShown = true;
            OnShown();
            
            if (IsModal)
                UIManager.AdjustModalLayer();
        }

        protected virtual void DoHideAnimation()
        {
            if (IsShown)
            {
                IsShown = false;
                BeforeHide();
            }

            if (ExitAction != null)
            {
                ExitAction.Invoke(HideImmediately);
            }
            else
            {
                HideImmediately();
            }
        }

        protected virtual void BeforeShow()
        {
            
        }

        protected virtual void OnShown()
        {
            
        }

        protected virtual void BeforeHide()
        {
            
        }

        protected virtual void OnHide()
        {
        }

        public void ShowOn(UILayer layer)
        {
            _transform.SetParent(layer.RectTransform, false);
            _transform.offsetMin = Vector2.zero;
            _transform.offsetMax = Vector2.zero;
            _transform.gameObject.SetActive(true);
            BringToFront();
        }

        public void BringToFront()
        {
            if (IsModal)
            {
                _transform.SetAsLastSibling();
                return;
            }

            var maxWinSibling = -1;
            var minModalSibling = int.MaxValue;
            foreach (var win in _transform.parent.GetComponentsInChildren<UIWindowBase>())
            {
                if (win == this)
                    continue;

                if (win.IsModal)
                    minModalSibling = Math.Min(minModalSibling, win._transform.GetSiblingIndex());
                else
                    maxWinSibling = Math.Max(maxWinSibling, win._transform.GetSiblingIndex());
            }
            if (minModalSibling == int.MaxValue)
                _transform.SetAsLastSibling();
            else
                _transform.SetSiblingIndex(Math.Min(maxWinSibling + 1, minModalSibling - 1));
        }

        public void Hide()
        {
            if (gameObject.activeInHierarchy)
                DoHideAnimation();
        }

        public virtual void HideImmediately()
        {
            OnDisable();
            Panel = null;
            Destroy(gameObject);
        }
    }
}