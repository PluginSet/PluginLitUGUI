using System;
using PluginLit.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PluginLit.UGUI
{
    [Serializable]
    public class BranchAssets: SerializableDict<string, AssetReference>
    {
        public AssetReference GetBranchReference(string branch)
        {
            if (SafePairs.Length <= 0)
                return null;

            if (TryGetValue(branch, out var branchAsset))
                return branchAsset;

            if (TryGetValue(UIBranch.DefaultBranch, out var mainAsset))
                return mainAsset;

            Debug.LogWarning($"No asset in {this}");
            return null;
        }

        internal void DefaultBranch()
        {
            if (Pairs != null && Pairs.Length > 0)
                return;

            Pairs = new[] { new SerializableKeyValuePair<string, AssetReference>(UIBranch.DefaultBranch, null) };
        }
    }
}