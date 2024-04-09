using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PluginLit.UGUI.Editor
{
    public class PackageAssetsDrawer<T>: UnityEditor.Editor where T: Object
    {
        private static Rect LastPropertyRect()
        {
            var field = typeof(EditorGUILayout).GetField("s_LastRect", BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null)
            {
                return (Rect)field.GetValue(null);
            }
            
            return EditorGUILayout.GetControlRect(true);
        }
        
        private Rect includeAssetsRect;
        
        public override void OnInspectorGUI()
        {
            var obj = serializedObject;
            EditorGUI.BeginChangeCheck();
            obj.UpdateIfRequiredOrScript();
            
            SerializedProperty iterator = obj.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                    EditorGUILayout.PropertyField(iterator, true, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
                    
                if (iterator.propertyPath == "includeAssets" && Event.current.type == EventType.Repaint)
                    includeAssetsRect = LastPropertyRect();
            }
            
            obj.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();

            var dirty = false;
            if (GUILayout.Button("Refresh"))
            {
                var packageAsset = (PackageAssets<T>) target;
                packageAsset.Refresh();
                dirty = true;
            }

            dirty = CheckDragEvent() || dirty;
            
            if (dirty)
            {
                UnityEditor.EditorUtility.SetDirty(target);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
            }
        }

        private bool CheckDragEvent()
        {
            var type = Event.current.type;
            if (type != EventType.DragUpdated && type != EventType.DragPerform)
                return false;

            var position = Event.current.mousePosition;
            var dropIn = includeAssetsRect.Contains(position);
            
            DragAndDrop.visualMode = dropIn ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Generic;

            if (type != EventType.DragPerform || !dropIn)
                return false;

            if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0)
            {
                var packageAsset = (PackageAssets<T>) target;
                packageAsset.AddAssets(DragAndDrop.objectReferences.Select(o => o as T));
                return true;
            }

            return false;
        }
    }

    [CustomEditor(typeof(PackageObjects), true), CanEditMultipleObjects]
    public class PackageObjectsDrawer: PackageAssetsDrawer<Object>
    {
    }
    

    [CustomEditor(typeof(PackageSprites), true), CanEditMultipleObjects]
    public class PackageSpritesDrawer: PackageAssetsDrawer<Sprite>
    {
    }
}