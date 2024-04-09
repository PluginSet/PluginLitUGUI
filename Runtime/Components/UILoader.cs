using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace PluginLit.UGUI
{
    [RequireComponent(typeof(RawImage))]
    public class UILoader: MonoBehaviour, IIconProtocol
    {
        private static bool IsWebUrl(string url)
        {
            url = url.ToLower();
            return url.StartsWith("http://") || url.StartsWith("https://") || url.StartsWith("file://");
        }
        
        private string _icon;
        
        [SerializeField]
        private RawImage image;
        
        private Texture2D _loadedTexture;
        private UnityWebRequest _requestTexture;
        
        public string icon
        {
            get => _icon;

            set
            {
                if (string.Equals(_icon, value))
                    return;
                
                SetIcon(value);
            }
        }

        private void Awake()
        {
            image = GetComponent<RawImage>();
        }

        public void SetIcon(string value)
        {
            _icon = value;
            var oldTexture = _loadedTexture;
            _loadedTexture = null;

            if (_requestTexture != null)
            {
                _requestTexture.Abort();
                _requestTexture = null;
            }
            
            LoadTexture(value);
            
            if (oldTexture != null)
                ResourceUtils.ReleaseAsset(oldTexture);
        }

        public void SetTexture(Texture2D texture2D)
        {
            image.texture = texture2D;
        }

        private void LoadTexture(string url)
        {
            if (IsWebUrl(url))
            {
                StartCoroutine(LoadRemoteTexture(url));
            }
            else
            {
                _loadedTexture = ResourceUtils.LoadAsset<Texture2D>(url);
                SetTexture(_loadedTexture);
            }
        }
        
        private IEnumerator LoadRemoteTexture(string url)
        {
            _requestTexture = UnityWebRequestTexture.GetTexture(url);
            yield return _requestTexture.SendWebRequest();
            if (_requestTexture.isDone)
            {
                var texture = DownloadHandlerTexture.GetContent(_requestTexture);
                SetTexture(texture);
            }
            _requestTexture = null;
        }

        private void OnDestroy()
        {
            if (_loadedTexture != null)
            {
                ResourceUtils.ReleaseAsset(_loadedTexture);
                _loadedTexture = null;
            }
            
            if (_requestTexture != null)
            {
                _requestTexture.Abort();
                _requestTexture = null;
            }
        }
        
#if UNITY_EDITOR
        private void InitImage()
        {
            image = GetComponent<RawImage>();
        }
        
        private void OnEnable()
        {
            InitImage();
        }

        private void OnValidate()
        {
            InitImage();
        }
#endif
    }
}