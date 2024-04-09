using UnityEngine;

namespace PluginLit.UGUI
{
    [RequireComponent(typeof(RectTransform))]
    public class MakeFullScreen: MonoBehaviour
    {
        private void OnEnable()
        {
            var selfRectTransform = GetComponent<RectTransform>();
            selfRectTransform.localPosition = Vector3.zero;
            selfRectTransform.anchorMin = Vector2.zero;
            selfRectTransform.anchorMax = Vector2.zero;
            selfRectTransform.pivot = Vector2.zero;
            selfRectTransform.anchoredPosition = Vector2.zero;
            selfRectTransform.sizeDelta = Vector2.zero;
            
            var uiCamera = UIRoot.Instance.UICamera;
            var topRightPoint = new Vector2(Screen.width, Screen.height);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(selfRectTransform, Vector2.zero, uiCamera, out var leftBottom);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(selfRectTransform, topRightPoint, uiCamera, out var rightTop);
            
            leftBottom.x = Mathf.Floor(leftBottom.x);
            leftBottom.y = Mathf.Floor(leftBottom.y);
            rightTop.x = Mathf.Ceil(rightTop.x);
            rightTop.y = Mathf.Ceil(rightTop.y);
            
            selfRectTransform.anchoredPosition = leftBottom;
            selfRectTransform.sizeDelta = rightTop - leftBottom;
        }
    }
}