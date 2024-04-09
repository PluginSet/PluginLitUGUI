
using System;
using System.Collections.Generic;
using PluginLit.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Logger = PluginLit.Core.Logger;

namespace PluginLit.UGUI
{
    public static class UIManager
    {
        private static readonly Logger Logger = LoggerManager.GetLogger("UIManager");
        
        public static readonly LayerMask UILayerMask = LayerMask.NameToLayer("UI");
        
        private struct WindowInfo
        {
            public string InstanceName;
            public Type WindowType;
            public bool IsSingleton;
            public int Tag;
        }

        private static readonly Dictionary<string, List<WindowInfo>> WindowInfos = new Dictionary<string, List<WindowInfo>>();
        private static List<WindowInfo> tempWindowInfos = null;
        private static readonly WindowInfo tempWindowInfo = new WindowInfo();

        private static string _defaultToastKey;

        public static void SetDefaultToastKey(string key)
        {
            _defaultToastKey = key;
        }
        
        public static void RegisterWindow<T>(string name, bool isSingleton = true, int tag = 0) where T: UIWindow
        {
            RegisterWindow(name, typeof(T), isSingleton, tag);
        }

        private static bool GetRegisteredWindowInfo(string name, out WindowInfo info)
        {
            if (WindowInfos.TryGetValue(name, out tempWindowInfos))
            {
                info = tempWindowInfos[tempWindowInfos.Count - 1];
                return true;
            }

            info = tempWindowInfo;
            Logger.Error("Cannot find window info with name {0}", name);
            return false;
        }
        
        public static void RegisterWindow(string name, Type type, bool isSingleton = true, int tag = 0)
        {
            if (WindowInfos.TryGetValue(name, out tempWindowInfos))
            {
                foreach (var windowInfo in tempWindowInfos)
                {
                    if (windowInfo.WindowType == type)
                    {
                        Logger.Warn("Repeated window name {0} with type {1}", name, type.FullName);
                        return;
                    }
                }
                
                return;
            }
            else
            {
                tempWindowInfos = new List<WindowInfo>();
                WindowInfos.Add(name, tempWindowInfos);
            }
            
            tempWindowInfos.Add(new WindowInfo()
            {
                InstanceName = $"{name}_{type.FullName}",
                WindowType = type,
                IsSingleton = isSingleton,
                Tag = tag,
            });
        }
        
        public static void UnRegisterWindow<T>(string name) where T: UIWindow
        {
            UnRegisterWindow(name, typeof(T));
        }

        public static void UnRegisterWindow(string name, Type type)
        {
            if (WindowInfos.TryGetValue(name, out tempWindowInfos))
            {
                for (int i = tempWindowInfos.Count - 1; i >= 0; i--)
                {
                    if (tempWindowInfos[i].WindowType == type)
                    {
                        tempWindowInfos.RemoveAt(i);
                        if (tempWindowInfos.Count == 0)
                            WindowInfos.Remove(name);
                        return;
                    }
                }
            }
        }
        
        public static void UnRegisterWindow(string name)
        {
            if (WindowInfos.ContainsKey(name))
                WindowInfos.Remove(name);
        }

        public static UIWindow FindWindow(string name)
        {
            if (!GetRegisteredWindowInfo(name, out var info))
            {
                Logger.Error("Cannot find window info with name {0}", name);
                return null;
            }

            return FindWindowInternal(name, in info);
        }

        private static UIWindow FindWindowInternal(string name, in WindowInfo info)
        {
            return UIWindow.GetInstance(info.InstanceName);
        }
        
        public static bool IsWindowShown(string name, bool isStrict = true)
        {
            var window = FindWindow(name);
            if (window == null)
                return false;

            return !isStrict || window.IsShown;
        }
        
        public static bool SomeModalIsShown()
        {
            if (UIRoot.Instance == null)
                return false;
            
            foreach (var layer in UIRoot.Instance.Layers)
            {
                if (layer.SomeModalShown)
                    return true;
            }

            return false;
        }

        public static T ShowOn<T>(UILayer layer, string name, params object[] args) where T: UIWindow
        {
            if (!GetRegisteredWindowInfo(name, out var info))
            {
                Logger.Error("Cannot find window info with name {0}", name);
                return null;
            }

            T window = null;
            if (info.IsSingleton)
            {
                window = FindWindowInternal(name, in info) as T;
            }

            if (window == null)
            {
                var gameObject = new GameObject(name)
                {
                    layer = UILayerMask
                };

                gameObject.SetActive(false);
                window = gameObject.AddComponent(info.WindowType) as T;
                if (window == null)
                    throw new Exception($"Window component type {info.WindowType} is not base on UIWindow");
                
                window.Init();
                window.name = name;
                window.InstanceName = info.InstanceName;
                window.Tag = info.Tag;
            }
            
            window.PushArgs(args);
            window.ShowOn(layer);

            return window;
        }

        public static void CreateToastRule(string name, float defaultStaySeconds, bool outImmediatelyWhenNew, bool enableShowWhenLastOut)
        {
            UIToastShowRule.AddRule(name, new UIToastShowRuleDefault(defaultStaySeconds, outImmediatelyWhenNew, enableShowWhenLastOut));
        }

        public static void ShowToastOn(UILayer layer, string text, string icon = null, string key = null, int tag = 0)
        {
            if (string.IsNullOrEmpty(key))
                key = _defaultToastKey;
            
            ShowToastOn(layer, text, icon, key, tag, key);
        }
        
        public static void ShowToastOn(UILayer layer, string text, string icon, string key, int tag, string rule)
        {
            if (string.IsNullOrEmpty(key))
                key = _defaultToastKey;

            var handler = Addressables.InstantiateAsync(key);
            handler.WaitForCompletion();
            var toast = handler.Result;
            if (toast == null)
                return;
            
            var toastComponent = toast.GetComponent<UIToast>();
            if (toastComponent == null)
            {
                Logger.Error("Toast component is null");
                return;
            }

            toastComponent.Tag = tag;
            toastComponent.text = text; 
            toastComponent.icon = icon;
            
            toast.GetComponent<RectTransform>().SetParent(layer.RectTransform, false);
            toastComponent.SetRule(UIToastShowRule.GetRule(rule));
        }

        public static void HideAll(UILayer layer)
        {
            HideAllWinOn(layer, window => true);
        }
        
        public static void CloseAll()
        {
            CloseAllWin(val => true);
        }

        public static void CloseAll(UILayer layer)
        {
            CloseAllWinOn(layer, window => true);
        }

        public static void HideAllOn(UILayer layer, int tag)
        {
            HideAllWinOn(layer, win => tag.Equals(win.Tag));
        }
        
        public static void CloseAllOn(UILayer layer, int tag)
        {
            CloseAllWinOn(layer, win => tag.Equals(win.Tag));
        }

        public static void HideAllWindow(string name)
        {
            HideAllWin(win => win.name.Equals(name));
        }

        public static void HideAllWindow(int tag)
        {
            HideAllWin(win => tag.Equals(win.Tag));
        }

        public static void CloseAllWindow(string name)
        {
            CloseAllWin(win => win.name.Equals(name));
        }

        public static void CloseAllWindow(int tag)
        {
            CloseAllWin(win => tag.Equals(win.Tag));
        }

        private static void HideAllWinOn(UILayer layer, Func<IUIEntity, bool> match)
        {
            foreach (var win in layer.GetComponentsInChildren<IUIEntity>(true))
            {
                if (match(win))
                    win.Hide();
            }
        }

        private static void CloseAllWinOn(UILayer layer, Func<IUIEntity, bool> match)
        {
            foreach (var win in layer.GetComponentsInChildren<IUIEntity>(true))
            {
                if (match(win))
                    win.HideImmediately();
            }
        }

        private static void HideAllWin(Func<IUIEntity, bool> match)
        {
            foreach (var layer in UIRoot.Instance.Layers)
            {
                HideAllWinOn(layer, match);
            }
        }
        
        private static void CloseAllWin(Func<IUIEntity, bool> match)
        {
            foreach (var layer in UIRoot.Instance.Layers)
            {
                CloseAllWinOn(layer, match);
            }
        }
        

        internal static void AdjustModalLayer()
        {
            var isTop = true;
            foreach (var layer in UIRoot.Instance.Layers)
            {
                if (isTop && layer.AdjustModalLayer())
                {
                    isTop = false;
                    continue;
                }
                
                layer.HideModalLayer();
            }
        }
    }
}
