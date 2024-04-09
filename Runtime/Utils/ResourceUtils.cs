using System;
using PluginLit.Core;
using UnityEngine.AddressableAssets;
using AsyncOperationHandle = UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle;
using Object = UnityEngine.Object;

namespace PluginLit.UGUI
{
    public static class ResourceUtils
    {
        public static T GetAsset<T>(AssetReference reference) where T: Object
        {
            if (reference.IsValid() && reference.IsDone)
                return reference.Asset as T;

            var handler = reference.IsValid() ? reference.OperationHandle : reference.LoadAssetAsync<T>();
            handler.WaitForCompletion();
            return handler.Result as T;
        }

        public static void LoadAsset<T>(AssetReference reference, Action<T> callback, bool async = true) where T : Object
        {
            if (reference.IsValid() && reference.IsDone)
            {
                callback?.Invoke(reference.Asset as T);
                return;
            }
            
            var handler = reference.IsValid() ? reference.OperationHandle : reference.LoadAssetAsync<T>();
            if (async)
            {
                handler.Completed += delegate(AsyncOperationHandle handle)
                {
                    callback?.Invoke(handle.Result as T);
                };
            }
            else
            {
                handler.WaitForCompletion();
                callback?.Invoke(handler.Result as T);
            }
        }
        
        
        public static T LoadAsset<T>(string key) where T : Object
        {
            var handler = Addressables.LoadAssetAsync<T>(key);
            handler.WaitForCompletion();
            return handler.Result;
        }

        public static void ReleaseAsset<T>(T asset) where T : Object
        {
            Addressables.Release(asset);
        }
    }
}