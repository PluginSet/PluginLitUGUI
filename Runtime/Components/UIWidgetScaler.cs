using UnityEngine;

namespace PluginLit.UGUI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class UIWidgetScaler: MonoBehaviour
    {
        public enum WidgetScalerMode
        {
            MatchWidth,
            MatchHeight,
            ShowAll,
            NoBorder,
        }
        
        public RectTransform targetTransform;
        public WidgetScalerMode mode = WidgetScalerMode.NoBorder;
        
        private RectTransform _selfRectTransform;

        private Vector2 _selfSizeDelta;
        private Vector2 _lastSizeDelta;

        private void OnEnable()
        {
            InitTarget();
            UpdateWidget();
        }
        
        private void UpdateWidget()
        {
            if (targetTransform == null)
                return;
            
            _lastSizeDelta = targetTransform.rect.size;
            _selfSizeDelta = _selfRectTransform.rect.size;
            
            switch (mode)
            {
                case WidgetScalerMode.MatchWidth:
                    _selfRectTransform.localScale = Vector3.one * (_lastSizeDelta.x / _selfSizeDelta.x);
                    break;
                case WidgetScalerMode.MatchHeight:
                    _selfRectTransform.localScale = Vector3.one * (_lastSizeDelta.y / _selfSizeDelta.y);
                    break;
                case WidgetScalerMode.ShowAll:
                    var scale = Mathf.Min(_lastSizeDelta.x / _selfSizeDelta.x, _lastSizeDelta.y / _selfSizeDelta.y);
                    _selfRectTransform.localScale = Vector3.one * scale;
                    break;
                case WidgetScalerMode.NoBorder:
                    var scale2 = Mathf.Max(_lastSizeDelta.x / _selfSizeDelta.x, _lastSizeDelta.y / _selfSizeDelta.y);
                    _selfRectTransform.localScale = Vector3.one * scale2;
                    break;
            }
        }
        
        private void InitTarget()
        {
            _lastSizeDelta = Vector2.left;
            if (_selfRectTransform == null)
                _selfRectTransform = GetComponent<RectTransform>();
            
            if (targetTransform == null && transform.parent != null)
                targetTransform = transform.parent.GetComponent<RectTransform>();
        }
        
        private void Update()
        {
            if (targetTransform == null)
                return;
            
            if (targetTransform.rect.size != _lastSizeDelta || _selfRectTransform.rect.size != _selfSizeDelta)
                UpdateWidget();
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            InitTarget();
            UpdateWidget();
        }
#endif

    }
}