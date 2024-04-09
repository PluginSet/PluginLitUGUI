using UnityEngine;

namespace PluginLit.UGUI
{
    public abstract class SafeAreaCalculator: MonoBehaviour
    {
        public abstract Rect GetSafeArea();
    }
}