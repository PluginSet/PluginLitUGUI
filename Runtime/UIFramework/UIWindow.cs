using System.Collections.Generic;

namespace PluginLit.UGUI
{
    public class UIWindow: UIWindowBase
    {
        private static Dictionary<string, List<UIWindow>> s_instances = new Dictionary<string, List<UIWindow>>();

        protected virtual bool DestroyOnHide => true;

        public static UIWindow GetInstance(string name)
        {
            if (s_instances.TryGetValue(name, out var list))
            {
                return list.Count > 0 ? list[0] : null;
            }

            return null;
        }

        public static void AddInstance(string name, UIWindow window)
        {
            if (s_instances.TryGetValue(name, out var list))
            {
                list.Add(window);
            }
            else
            {
                s_instances.Add(name, new List<UIWindow>(){ window });
            }
        }

        public static void ClearInstances()
        {
            s_instances.Clear();
        }

        private static void RemoveInstance(string name, UIWindow window)
        {
            if (s_instances.TryGetValue(name, out var list))
            {
                list.Remove(window);
            }
        }
        
        public string InstanceName { get; internal set; }

        private object[] _args;

        protected override void Awake()
        {
            if (string.IsNullOrEmpty(InstanceName))
                InstanceName = $"{name}_{GetType().FullName}";
            AddInstance(InstanceName, this);
        }

        protected override void OnDestroy()
        {
            RemoveInstance(InstanceName, this);
            base.OnDestroy();
        }


        public virtual void Init()
        {
            // Constructor
        }

        public void PushArgs(params object[] args)
        {
            _args = args;
            if (Panel != null && gameObject.activeInHierarchy)
                SetDataInternal();
        }
        
        protected virtual void SetData(params object[] args)
        {
            
        }
        
        protected virtual void SetData()
        {
            
        }

        private void SetDataInternal()
        {
            if (_args == null || _args.Length == 0)
                SetData();
            else
                SetData(_args);
        }

        protected override void BeforeShow()
        {
            SetDataInternal();
        }

        protected override void BeforeHide()
        {
            if (DestroyOnHide)
                RemoveInstance(InstanceName, this);
            
            base.BeforeHide();
        }
    }

    public class UIWindow<T> : UIWindow
    {
        protected override void SetData(params object[] args)
        {
            if (args.Length == 1 && args[0] is T data)
                SetData(data);
        }

        protected virtual void SetData(T data)
        {
            
        }
    }
}