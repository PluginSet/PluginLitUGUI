using PluginSet.Core;
using PluginSet.Core.Editor;

namespace PluginLit.UGUI.Editor
{
    [BuildTools]
    public static class BuildUnityGUITools
    {
        [OnSyncEditorSetting]
        public static void OnSyncEditorSetting(BuildProcessorContext context)
        {
            var buildParams = context.BuildChannels.Get<BuildUnityGUIParams>();
            
            var pluginConfig = context.Get<PluginSetConfig>("pluginsConfig");
            var config = pluginConfig.AddConfig<PluginSetUnityGUIConfig>("UGUI");
            buildParams.CloneTo(config);
        }
    }
}