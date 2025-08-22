using System.Reflection;
using UnityEditor;
using UnityEngine;

#if UNITY_2023_2_OR_NEWER
using System;
using Object = UnityEngine.Object;
#endif

namespace Playmaykr.Utils.Hotkeys.Editor
{
    public static class LockInspector
    {
        private static readonly MethodInfo FlipLocked;
        private static readonly PropertyInfo ConstrainProportions;
        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        static LockInspector()
        {
            // Cache static MethodInfo and PropertyInfo for performance
#if UNITY_2023_2_OR_NEWER
            Type editorLockTrackerType = typeof(EditorGUIUtility).Assembly.GetType("UnityEditor.EditorGUIUtility+EditorLockTracker");
            FlipLocked = editorLockTrackerType.GetMethod("FlipLocked", BindingFlags);
#endif
            ConstrainProportions = typeof(Transform).GetProperty("constrainProportionsScale", BindingFlags);
        }

        [MenuItem("Edit/Toggle Inspector Lock %l")]
        public static void Lock()
        {
#if UNITY_2023_2_OR_NEWER
        // New approach for Unity 2023.2 and above, including Unity 6
        Type inspectorWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");

        foreach (Object inspectorWindow in Resources.FindObjectsOfTypeAll(inspectorWindowType))
        {
            object lockTracker = inspectorWindowType.GetField("m_LockTracker", BindingFlags)?.GetValue(inspectorWindow);
            FlipLocked?.Invoke(lockTracker, new object[] { });
        }
#else
            // Old approach for Unity versions before 2023.2
            ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
#endif

            // Constrain Proportions lock for all versions including Unity 6
            foreach (UnityEditor.Editor activeEditor in ActiveEditorTracker.sharedTracker.activeEditors)
            {
                if (activeEditor.target is not Transform target)
                {
                    continue;
                }

                var currentValue = (bool)ConstrainProportions.GetValue(target, null);
                ConstrainProportions.SetValue(target, !currentValue, null);
            }

            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }

        [MenuItem("Edit/Toggle Inspector Lock %l", true)]
        public static bool Valid()
        {
            return ActiveEditorTracker.sharedTracker.activeEditors.Length != 0;
        }
    }
}
