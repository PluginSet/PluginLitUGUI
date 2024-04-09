using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PluginLit.UGUI
{
    public sealed class UILayer: MonoBehaviour, IComparable<UILayer>
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/PluginLit UGUI/Add Layer")]
        public static void AddLayerOnRoot(UnityEditor.MenuCommand menuCommand)
        {
            var gameObject = menuCommand.context as GameObject;
            if (gameObject == null)
                return;
            
            var rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform == null)
                return;
            
            var layer = CreateLayer(rectTransform, "Layer");
            var layerRect = layer.GetComponent<RectTransform>();
            layerRect.anchorMin = Vector2.zero;
            layerRect.anchorMax = Vector2.one;
            layerRect.offsetMin = Vector2.zero;
            layerRect.offsetMax = Vector2.zero;
            
            UnityEditor.Undo.RegisterCreatedObjectUndo(layer.gameObject, "Create Layer");
        }
#endif
        
        
        // ReSharper disable Unity.PerformanceAnalysis
        internal static UILayer CreateLayer(RectTransform parent, string name, int sortingOrder = 0)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var layer = obj.AddComponent<UILayer>();
            layer.canvas = obj.AddComponent<Canvas>();
            obj.AddComponent<GraphicRaycaster>();
            layer.InitCanvas(sortingOrder);
            return layer;
        }

        [SerializeField]
        private Canvas canvas;
        public RectTransform RectTransform { get; private set; }
        
        public bool SomeModalShown => _modalLayer != null && _modalLayer.gameObject.activeSelf;

        private RectTransform _modalLayer;

        private void Awake()
        {
            if (canvas == null)
                canvas = GetComponent<Canvas>();
            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();
        }

        private void OnDestroy()
        {
            if (_modalLayer != null && UIConfig.ModalLayerAsset != null)
            {
                UIConfig.ModalLayerAsset.ReleaseInstance(_modalLayer.gameObject);
                _modalLayer = null;
            }
        }

        private void InitCanvas(int sortingOrder)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
            RectTransform = GetComponent<RectTransform>();
        }
        
        public int CompareTo(UILayer other)
        {
            return other.canvas.sortingOrder.CompareTo(canvas.sortingOrder);
        }

        internal bool AdjustModalLayer()
        {
            var maxSiblingIndex = int.MinValue;
            UIWindowBase topWindow = null;
            foreach (var win in RectTransform.GetComponentsInChildren<UIWindowBase>())
            {
                if (!win.IsShown)
                    continue;
                
                if (!win.IsModal)
                    continue;

                var siblingIndex = win.transform.GetSiblingIndex();
                if (siblingIndex > maxSiblingIndex)
                {
                    topWindow = win;
                    maxSiblingIndex = siblingIndex;
                }
            }

            if (topWindow == null)
            {
                HideModalLayer();
                return false;
            }
            
            ShowModalLayer(topWindow);
            return true;
        }
        
        private void ShowModalLayer(UIWindowBase topWindow)
        {
            if (_modalLayer == null)
            {
                _modalLayer = CreateModalLayer();
                _modalLayer.gameObject.AddComponent<MakeFullScreen>();
            }
            
            _modalLayer.gameObject.SetActive(true);
            _modalLayer.SetAsLastSibling();
            topWindow.transform.SetAsLastSibling();
        }

        internal void HideModalLayer()
        {
            if (_modalLayer == null)
                return;
            
            _modalLayer.gameObject.SetActive(false);
        }

        private RectTransform CreateModalLayer()
        {
            var asset = UIConfig.ModalLayerAsset;
            if (asset != null)
            {
                var handler = UIConfig.ModalLayerAsset.InstantiateAsync(RectTransform, false);
                handler.WaitForCompletion();
                return handler.Result.GetComponent<RectTransform>();
            }
            
            var obj = new GameObject("ModalLayer");
            obj.AddComponent<CanvasRenderer>();
            var image = obj.AddComponent<Image>();
            image.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
            image.color = UIConfig.ModalLayerColor;
            var rectTransform = obj.GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = obj.AddComponent<RectTransform>();
            rectTransform.SetParent(RectTransform, false);

            return rectTransform;
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            canvas = GetComponent<Canvas>();
        }
#endif
    }
}