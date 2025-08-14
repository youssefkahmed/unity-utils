using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Playmaykr.Utils.SerializedInterfaces.Editor
{
    /// <summary>
    /// Custom property drawer for the RequireInterface attribute.
    /// This drawer allows you to specify that a field must implement a certain interface.
    /// It supports both single object fields and arrays of objects.
    /// </summary>
    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
    public class RequireInterfaceDrawer : PropertyDrawer
    {
        private RequireInterfaceAttribute RequireInterfaceAttribute => (RequireInterfaceAttribute)attribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Type requiredInterfaceType = RequireInterfaceAttribute.InterfaceType;
            EditorGUI.BeginProperty(position, label, property);

            if (property.isArray && property.propertyType == SerializedPropertyType.Generic)
            {
                DrawArrayField(position, property, label, requiredInterfaceType);
            }
            else
            {
                DrawInterfaceObjectField(position, property, label, requiredInterfaceType);
            }
        
            EditorGUI.EndProperty();
            var args = new InterfaceArgs(GetTypeOrElementType(fieldInfo.FieldType), requiredInterfaceType);
            InterfaceReferenceUtil.OnGUI(position, property, label, args);
        }

        /// <summary>
        /// Draws a field for an array of objects that implement a specific interface.
        /// This method handles the display of the array size and each element in the array.
        /// </summary>
        /// <param name="position">The position of the property in the inspector.</param>
        /// <param name="property">The serialized property representing the array.</param>
        /// <param name="label">The label for the property.</param>
        /// <param name="interfaceType">The interface type that each element in the array must implement.</param>
        private void DrawArrayField(Rect position, SerializedProperty property, GUIContent label, Type interfaceType)
        {
            property.arraySize = EditorGUI.IntField(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                label.text + " Size", property.arraySize);
        
            float yOffset = EditorGUIUtility.singleLineHeight;
            for (var i = 0; i < property.arraySize; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                var elementRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                DrawInterfaceObjectField(elementRect, element, new GUIContent($"Element {i}"), interfaceType);
                yOffset += EditorGUIUtility.singleLineHeight;
            }
        }

        /// <summary>
        /// Draws a field for a single object that must implement a specific interface.
        /// This method allows the user to assign an object that implements the specified interface.
        /// </summary>
        /// <param name="position">The position of the property in the inspector.</param>
        /// <param name="property">The serialized property representing the object.</param>
        /// <param name="label">The label for the property.</param>
        /// <param name="interfaceType">The interface type that the object must implement.</param>
        private void DrawInterfaceObjectField(Rect position, SerializedProperty property, GUIContent label, Type interfaceType)
        {
            Object oldReference = property.objectReferenceValue;
            Type baseType = GetAssignableBaseType(fieldInfo.FieldType, interfaceType);
            Object newReference = EditorGUI.ObjectField(position, label, oldReference, baseType, true);

            if (newReference != null && newReference != oldReference)
            {
                ValidateAndAssignObject(property, newReference, interfaceType);
            }
            else if (newReference == null)
            {
                property.objectReferenceValue = null;
            }
        }

        /// <summary>
        /// Validates the new object reference against the required interface type and assigns it to the property.
        /// If the object does not implement the interface, it logs a warning and sets the property to null.
        /// </summary>
        /// <param name="property">The serialized property to assign the object to.</param>
        /// <param name="newReference">The new object reference to validate and assign.</param>
        /// <param name="interfaceType">The interface type that the object must implement.</param>
        private static void ValidateAndAssignObject(SerializedProperty property, Object newReference, Type interfaceType)
        {
            if (newReference is GameObject gameObject)
            {
                Component component = gameObject.GetComponent(interfaceType);
                if (component)
                {
                    property.objectReferenceValue = component;
                    return;
                }
            }
            else if (interfaceType.IsInstanceOfType(newReference))
            {
                property.objectReferenceValue = newReference;
                return;
            }
        
            Debug.LogWarning($"The assigned object does not implement '{interfaceType.Name}'.");
            property.objectReferenceValue = null;
        }
        
        /// <summary>
        /// Determines the base type that is assignable to the specified interface type.
        /// This method checks if the field type is an array or a generic list and returns the
        /// appropriate element type that implements the interface.
        /// </summary>
        /// <param name="fieldType">The type of the field being drawn.</param>
        /// <param name="interfaceType">The interface type that the field must implement.</param>
        /// <returns>The base type that is assignable to the specified interface type.</returns>
        private static Type GetAssignableBaseType(Type fieldType, Type interfaceType)
        {
            Type elementType = fieldType.IsArray
                ? fieldType.GetElementType()
                : fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>) 
                    ? fieldType.GetGenericArguments()[0] 
                    : fieldType;
        
            if (interfaceType.IsAssignableFrom(elementType))
            {
                return elementType;
            }

            if (typeof(ScriptableObject).IsAssignableFrom(elementType))
            {
                return typeof(ScriptableObject);
            }
            
            if (typeof(MonoBehaviour).IsAssignableFrom(elementType))
            {
                return typeof(MonoBehaviour);
            }

            return typeof(Object);
        }
        
        /// <summary>
        /// Gets the type or element type of the specified type.
        /// If the type is an array, it returns the element type. If it is a generic type,
        /// it returns the first generic argument. Otherwise, it returns the type itself.
        /// </summary>
        /// <param name="type">The type to check.</param>
        private static Type GetTypeOrElementType(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            
            if (type.IsGenericType)
            {
                return type.GetGenericArguments()[0];
            }
            
            return type;
        }
    }
}