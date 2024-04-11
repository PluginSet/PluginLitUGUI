using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PluginLit.UGUI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    // [RequireComponent(typeof(SafeAreaCalculator))]
    public class UIRoot: MonoBehaviour
    {
        public static UIRoot Instance { get; private set; }
        
        public RectTransform RectTransform { get; private set; }

        private Canvas _canvas;

        public Canvas Canvas
        {
            get
            {
                if (_canvas == null)
                    _canvas = GetComponent<Canvas>();

                return _canvas;
            }
        }
        
        public Camera UICamera => Canvas.worldCamera;

        public UILayer DefaultLayer { get; private set; }

        internal List<UILayer> Layers;

        private Vector2 _screenSize;
        private Rect _safeArea;
        
        private float _areaWith;
        private float _areaHeight;
        private float _scale;
        private float _offsetX;
        private float _offsetY;
        
        public UILayer AddLayer(string objName, int sortingOrder = 0)
        {
            var layer = UILayer.CreateLayer(RectTransform, objName, sortingOrder);
            layer.gameObject.layer = UIManager.UILayerMask;
            UILayerApplySafeArea(layer);
            Layers.Add(layer);
            Layers.Sort();
            DefaultLayer = Layers[Layers.Count - 1];
            return layer;
        }
        
        public UILayer FindLayer(string objName)
        {
            return Layers.Find(layer => layer.name == objName);
        }

        private bool GetSafeArea(out Rect rect)
        {
            Rect newRect;
            var calculator = GetComponent<SafeAreaCalculator>();
            if (calculator != null)
            {
                newRect = calculator.GetSafeArea();
            }
            else
            {
                newRect = new Rect(Vector2.zero, _screenSize);
            }

            var changed = !newRect.Equals(_safeArea);
            rect = newRect;
            
            return changed;
        }
        
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            RectTransform = GetComponent<RectTransform>();
            
            Layers = new List<UILayer>();
            Layers.AddRange(GetComponentsInChildren<UILayer>(true));
            Layers.Sort();
            if (Layers.Count > 0)
                DefaultLayer = Layers[Layers.Count - 1];

            _screenSize = new Vector2(Screen.width, Screen.height);
            GetSafeArea(out var rect);
            CalculateSafeArea(rect.x, rect.y, rect.width, rect.height);
            _safeArea = rect;
        }

        private void Update()
        {
            if (Instance != this)
                return;

            if (Math.Abs(_screenSize.x - Screen.width) < 1 && Math.Abs(_screenSize.y - Screen.height) < 1)
                return;
            
            _screenSize = new Vector2(Screen.width, Screen.height);
            if (GetSafeArea(out var rect))
            {
                _safeArea = rect;
                CalculateSafeArea(rect.x, rect.y, rect.width, rect.height);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
        
        private void CalculateSafeArea(float x, float y, float w, float h)
        {
            var canvasScaler = GetComponent<CanvasScaler>();
            if (canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ConstantPixelSize)
                return;

            var factor = canvasScaler.scaleFactor;
            var dWidth = canvasScaler.referenceResolution.x;
            var dHeight = canvasScaler.referenceResolution.y;
            var matchMode = canvasScaler.screenMatchMode;

            _offsetX = x / factor;
            _offsetY = y / factor;
            
            var scaleX = w / dWidth;
            var scaleY = h / dHeight;

            switch (matchMode)
            {
                case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
                    var match = canvasScaler.matchWidthOrHeight;
                    if (match <= 0f)
                        _scale = scaleX;
                    else if (match >= 1f)
                        _scale = scaleY;
                    else
                        _scale = scaleX * (1f - match) + scaleY * match;
                    break;
                
                case CanvasScaler.ScreenMatchMode.Expand:
                    _scale = Mathf.Min(scaleX, scaleY);
                    break;
                
                case CanvasScaler.ScreenMatchMode.Shrink:
                    _scale = Mathf.Max(scaleX, scaleY);
                    break;
            }

            _areaWith = Mathf.Ceil(w / _scale);
            _areaHeight = Mathf.Ceil(h / _scale);
            
            foreach (var layer in Layers)
            {
                UILayerApplySafeArea(layer);
            }
        }

        private void UILayerApplySafeArea(UILayer layer)
        {
            var rectTransform = layer.GetComponent<RectTransform>();
            if (rectTransform == null)
                return;
            
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(_areaWith, _areaHeight);
            rectTransform.anchoredPosition = new Vector2(_offsetX, _offsetY);
            rectTransform.localScale = new Vector3(_scale, _scale, _scale);
        }
    }
}