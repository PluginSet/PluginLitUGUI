using UnityEngine;
using UnityEngine.UI;

namespace PluginLit.UGUI
{
    [RequireComponent(typeof(Image))]
    public class BranchSpriteImage : BranchAssetAdaptor<Sprite>
    {
        private Image _image { get; set; }

        private Image Image
        {
            get
            {
                if (_image == null)
                    _image = GetComponent<Image>();

                return _image;
            }
        }

        protected override void OnAssetChanged(Sprite asset)
        {
            Image.sprite = asset;
        }
    }
}