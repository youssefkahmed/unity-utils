using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Playmaykr.Utils.AnimationEventsVisualizer.Editor
{
    /// <summary>
    /// Defines a custom property drawer for <c>AnimationEvent</c>, allowing users to
    /// select animation event names from a dropdown in the Inspector. It dynamically fetches available event names
    /// from the associated Animator component and displays a property field for the event callback.
    /// The drawer ensures only valid event names are selectable and adapts its height based on the displayed properties.
    /// </summary>
    [CustomPropertyDrawer(typeof(AnimationEvent))]
    public class AnimationEventDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Access the target object
            Object targetObject = property.serializedObject.targetObject as MonoBehaviour;
            
            // Fetch dynamic event names from the Animator
            string[] eventNames = GetEventNamesFromAnimator(targetObject);
            
            SerializedProperty stateNameProperty = property.FindPropertyRelative("eventName");
            SerializedProperty stateEventProperty = property.FindPropertyRelative("onAnimationEvent");

            // Draw the dropdown for 'eventName'
            Rect stateNameRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            int selectedIndex = Mathf.Max(System.Array.IndexOf(eventNames, stateNameProperty.stringValue), 0);
            selectedIndex = EditorGUI.Popup(stateNameRect, "Event Name", selectedIndex, eventNames);
            stateNameProperty.stringValue = eventNames[selectedIndex];  // Update the value based on selection
            
            Rect stateEventRect = new(position.x, 
                position.y + EditorGUIUtility.singleLineHeight + 2,
                position.width,
                EditorGUI.GetPropertyHeight(stateEventProperty));

            EditorGUI.PropertyField(stateEventRect, stateEventProperty, true);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty stateEventProperty = property.FindPropertyRelative("onAnimationEvent");
            return EditorGUIUtility.singleLineHeight + EditorGUI.GetPropertyHeight(stateEventProperty) + 4;
        }
        
        /// <summary>
        /// Retrieves a list of unique animation event names from the Animator component attached to the specified target object.
        /// </summary>
        /// <param name="target">The object with an Animator component to get animation event names from.</param>
        /// <returns>An array of event names, including a default "None" entry if no valid events are found or if the target is invalid.</returns>
        private static string[] GetEventNamesFromAnimator(Object target)
        {
            List<string> eventNames = new() { "None" }; // Default entry
            if (target == null)
            {
                return eventNames.ToArray();
            }

            bool success = ((MonoBehaviour)target).TryGetComponent(out Animator animator);
            if (!success || !animator.runtimeAnimatorController)
            {
                return eventNames.ToArray();
            }

            // Access all states in the AnimatorController
            var controller = animator.runtimeAnimatorController as AnimatorController;
            if (!controller)
            {
                return eventNames.ToArray();
            }
            
            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                foreach (ChildAnimatorState state in layer.stateMachine.states)
                {
                    // Find StateMachineBehaviours of type AnimationEventStateBehaviour
                    foreach (StateMachineBehaviour behaviour in state.state.behaviours)
                    {
                        if (behaviour is AnimationEventStateBehaviour eventBehaviour)
                        {
                            string eventName = eventBehaviour.EventName;
                            if (!string.IsNullOrEmpty(eventName) && !eventNames.Contains(eventName))
                            {
                                eventNames.Add(eventName);
                            }
                        }
                    }
                }
            }
            return eventNames.ToArray();
        }
    }
}
