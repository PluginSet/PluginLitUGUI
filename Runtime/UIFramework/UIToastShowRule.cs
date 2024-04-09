using System.Collections.Generic;

namespace PluginLit.UGUI
{
    public abstract class UIToastShowRule
    {
        public static readonly UIToastShowRule DefaultRule = new UIToastShowRuleDefault(2f, true, true);
        
        private static readonly Dictionary<string, UIToastShowRule> _rules = new Dictionary<string, UIToastShowRule>();
        
        public static UIToastShowRule GetRule(string name)
        {
            if (_rules.TryGetValue(name, out var result))
                return result;

            return DefaultRule;
        }

        public static void AddRule(string name, UIToastShowRule rule)
        {
            if (_rules.ContainsKey(name))
                _rules[name] = rule;
            else
                _rules.Add(name, rule);
        }

        public abstract void OnToastAdded(UIToast toast);

        public abstract void OnToastIn(UIToast toast);
        public abstract void OnToastOut(UIToast toast);

        public abstract void OnToastRemove(UIToast toast);
    }
}