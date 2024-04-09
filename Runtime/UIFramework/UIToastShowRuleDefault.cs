using System.Collections.Generic;

namespace PluginLit.UGUI
{
    public class UIToastShowRuleDefault : UIToastShowRule
    {
        // Toast 在进入/退出动画间隙停留时长
        public float DefaultStaySeconds;

        // 有新Toast当前Toast立即退出(播放完整动画，不停留)
        public bool OutImmediatelyWhenNew;
        
        // 当上个Toast在播放退出动画时，就可以播放下个Toast
        public bool EnableShowWhenLastOut;
            
        protected readonly List<UIToast> ShowedToasts = new List<UIToast>();
        protected readonly List<UIToast> ShowingToasts = new List<UIToast>();
        protected readonly List<UIToast> ToastQueue = new List<UIToast>();

        private bool _isCheckShowNext;

        public UIToastShowRuleDefault(float defaultStaySeconds, bool outImmediatelyWhenNew, bool enableShowWhenLastOut)
        {
            DefaultStaySeconds = defaultStaySeconds;
            OutImmediatelyWhenNew = outImmediatelyWhenNew;
            EnableShowWhenLastOut = enableShowWhenLastOut;
        }

        protected void ShowNext()
        {
            if (_isCheckShowNext)
                return;
            _isCheckShowNext = true;
            ShowNextInternal();
            _isCheckShowNext = false;
        }
        
        protected void ShowNextInternal()
        {
            if (ToastQueue.Count <= 0)
                return;

            if (!EnableShowWhenLastOut)
            {
                for (int i = ShowedToasts.Count - 1; i >= 0; i--)
                {
                    var toast = ShowedToasts[i];
                    if (toast == null)
                    {
                        ShowedToasts.RemoveAt(i);
                        continue;
                    }

                    return;
                }
                
                for (int i = ShowingToasts.Count - 1; i >= 0; i--)
                {
                    var toast = ShowingToasts[i];
                    if (toast == null)
                    {
                        ShowingToasts.RemoveAt(i);
                        continue;
                    }

                    return;
                }
            }

            if (OutImmediatelyWhenNew)
            {
                for (int i = ShowingToasts.Count - 1; i >= 0; i--)
                {
                    var toast = ShowingToasts[i];
                    if (toast == null)
                    {
                        ShowingToasts.RemoveAt(i);
                        continue;
                    }

                    toast.OutImmediately();
                }
            }
            
            if (ShowingToasts.Count > 0)
                return;

            for (int i = ShowedToasts.Count - 1; i >= 0; i--)
            {
                var toast = ShowedToasts[i];
                if (toast == null)
                {
                    ShowedToasts.RemoveAt(i);
                    continue;
                }
                
                toast.OutImmediately();
            }
            
            while (ToastQueue.Count > 0)
            {
                var toast = ToastQueue[0];
                ToastQueue.RemoveAt(0);
                if (toast == null) continue;
            
                ShowingToasts.Add(toast);
                _isCheckShowNext = false;
                
                toast.Show(DefaultStaySeconds);
                break;
            }
        }

        public override void OnToastAdded(UIToast toast)
        {
            ToastQueue.Add(toast);
            ShowNext();
        }

        public override void OnToastIn(UIToast toast)
        {
            ShowingToasts.Remove(toast);
            if (!ShowedToasts.Contains(toast))
                ShowedToasts.Add(toast);
            ShowNext();
        }

        public override void OnToastOut(UIToast toast)
        {
            ShowingToasts.Remove(toast);
            if (!ShowedToasts.Contains(toast))
                ShowedToasts.Add(toast);
            ShowNext();
        }

        public override void OnToastRemove(UIToast toast)
        {
            ShowedToasts.Remove(toast);
            ShowingToasts.Remove(toast);
            ToastQueue.Remove(toast);
            ShowNext();
        }
    }
}