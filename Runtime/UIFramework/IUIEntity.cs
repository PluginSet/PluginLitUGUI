namespace PluginLit.UGUI
{
    public interface IUIEntity
    {
        string name { get; set; }
        
        int Tag { get; set; }

        void Hide();
        
        void HideImmediately();
    }
}