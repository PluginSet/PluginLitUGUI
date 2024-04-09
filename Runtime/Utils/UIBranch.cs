using UnityEngine;

namespace PluginLit.UGUI
{
    public static class UIBranch
    {
        public const string DefaultBranch = "@main";
        
        private static string _branch;
        public static string Branch
        {
            get => _branch;

            set
            {
                _branch = value;
                UIRoot.Instance.BroadcastMessage("OnBranchChanged", value, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}