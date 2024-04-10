using PluginLit.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace PluginLit.UGUI
{
    public abstract class BranchAssetAdaptor: MonoBehaviour
    {
        [SerializeField]
        [SerializableDict("stringValue", "")]
        protected BranchAssets assets;

        private AssetReference _currentReference;

        protected AssetReference CurrentReference
        {
            get => _currentReference;

            set
            {
                _currentReference?.ReleaseAsset();
                _currentReference = value;
            }
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (assets == null)
                assets = new BranchAssets();
            assets.DefaultBranch();
        }
#endif

        protected virtual void OnEnable()
        {
            SetupAsset();
        }
        
        protected virtual void OnDisable()
        {
        }

        protected virtual void OnDestroy()
        {
            CurrentReference = null;
        }

        protected void SetupAsset()
        {
            if (assets == null)
                return;
            
            var reference = assets.GetBranchReference(UIBranch.Branch);
            if (reference.RuntimeKey.Equals(string.Empty))
                reference = null;
            
            if (CurrentReference == reference)
                return;

            CurrentReference = reference;
            OnAssetReferenceChanged(reference);
        }

        protected abstract void OnAssetReferenceChanged(AssetReference reference);

        public void OnBranchChanged(string branch)
        {
            SetupAsset();
        }
    }

    public abstract class BranchAssetAdaptor<T> : BranchAssetAdaptor where T : Object
    {
        protected override void OnAssetReferenceChanged(AssetReference reference)
        {
            OnAssetChanged(ResourceUtils.GetAsset<T>(reference));
        }

        protected abstract void OnAssetChanged(T asset);
    }
}