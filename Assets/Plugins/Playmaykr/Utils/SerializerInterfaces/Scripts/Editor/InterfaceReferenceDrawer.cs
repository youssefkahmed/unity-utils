using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Playmaykr.Utils.SerializedInterfaces.Editor
{
    /// <summary>
    /// Custom property drawer for <see cref="InterfaceReference{T}"/> and <see cref="InterfaceReference{T, TInterface}"/>.
    /// This drawer allows you to assign an object that implements a specific interface,
    /// and it will validate that the assigned object is compatible with the interface.
    /// If the assigned object does not implement the required interface, it will log a warning and
    /// set the property to null.
    /// </summary>
    [CustomPropertyDrawer(typeof(InterfaceReference<>))]
    [CustomPropertyDrawer(typeof(InterfaceReference<,>))]
    public class InterfaceReferenceDrawer : PropertyDrawer
    {
        private const string UnderlyingValueFieldName = "underlyingValue";
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty underlyingProperty = property.FindPropertyRelative(UnderlyingValueFieldName);
            InterfaceArgs args = GetArguments(fieldInfo);

            EditorGUI.BeginProperty(position, label, property);

            Object assignedObject = EditorGUI.ObjectField(position, label, underlyingProperty.objectReferenceValue, args.objectType, true);
            if (assignedObject != null)
            {
                Object component = null;
                if (assignedObject is GameObject gameObject)
                {
                    component = gameObject.GetComponent(args.interfaceType);
                }
                else if (args.interfaceType.IsInstanceOfType(assignedObject))
                {
                    component = assignedObject;
                }

                if (component != null)
                {
                    ValidateAndAssignObject(underlyingProperty, component, component.name, args.interfaceType.Name);
                }
                else
                {
                    Debug.LogWarning($"Assigned object does not implement required interface '{args.interfaceType.Name}'.");
                    underlyingProperty.objectReferenceValue = null;
                }
            }
            else
            {
                underlyingProperty.objectReferenceValue = null;
            }

            EditorGUI.EndProperty();
            InterfaceReferenceUtil.OnGUI(position, underlyingProperty, label, args);
        }

        /// <summary>
        /// Retrieves the types from the field info, determining the object type and interface type.
        /// It checks if the field type is a generic interface reference or a list, and extracts
        /// the appropriate types accordingly.
        /// </summary>
        /// <param name="fieldInfo">The field info to extract types from.</param>
        /// <returns>An instance of <see cref="InterfaceArgs"/> containing the object type and interface type.</returns>
        /// <exception cref="ArgumentException">Thrown if the field type is not a valid interface reference or list.</exception>
        private static InterfaceArgs GetArguments(FieldInfo fieldInfo)
        {
            Type fieldType = fieldInfo.FieldType;
            if (!TryGetTypesFromInterfaceReference(fieldType, out Type objectType, out Type interfaceType))
            {
                GetTypesFromList(fieldType, out objectType, out interfaceType);
            }
        
            return new InterfaceArgs(objectType, interfaceType);

            void GetTypesFromList(Type type, out Type objType, out Type intfType)
            {
                objType = intfType = null;
            
                Type listInterface = type
                    .GetInterfaces()
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>));

                if (listInterface != null)
                {
                    Type elementType = listInterface.GetGenericArguments()[0];
                    TryGetTypesFromInterfaceReference(elementType, out objType, out intfType);
                }
            }

            bool TryGetTypesFromInterfaceReference(Type type, out Type objType, out Type intfType)
            {
                objType = intfType = null;
                if (type?.IsGenericType != true)
                {
                    return false;
                }
            
                Type genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(InterfaceReference<>)) type = type.BaseType;

                if (type?.GetGenericTypeDefinition() == typeof(InterfaceReference<,>))
                {
                    var types = type.GetGenericArguments();
                    intfType = types[0];
                    objType = types[1];
                    return true;
                }
            
                return false;
            }
        }

        /// <summary>
        /// Validates the assigned object against the specified interface and assigns it to the property.
        /// If the object does not implement the interface, it logs a warning and sets the property to null.
        /// </summary>
        /// <param name="property">The serialized property to assign the object to.</param>
        /// <param name="targetObject">The object to validate and assign.</param>
        /// <param name="componentNameOrType">The name or type of the component for logging purposes.</param>
        /// <param name="interfaceName">The name of the interface to validate against.</param>
        private static void ValidateAndAssignObject(SerializedProperty property, Object targetObject, string componentNameOrType, string interfaceName = null)
        {
            if (targetObject != null)
            {
                property.objectReferenceValue = targetObject;
            }
            else
            {
                string message = interfaceName != null
                    ? $"GameObject '{componentNameOrType}'"
                    : "assigned object";

                Debug.LogWarning($"The {message} does not have a component that implements '{interfaceName}'.");
                property.objectReferenceValue = null;
            }
        }
    }

    /// <summary>
    /// Struct to hold the types of the object and interface for the property drawer.
    /// </summary>
    public struct InterfaceArgs
    {
        public readonly Type objectType;
        public readonly Type interfaceType;
        
        public InterfaceArgs(Type objectType, Type interfaceType)
        {
            Debug.Assert(typeof(Object).IsAssignableFrom(objectType), $"{nameof(objectType)} needs to be of Type {typeof(Object)}.");
            Debug.Assert(interfaceType.IsInterface, $"{nameof(interfaceType)} needs to be an interface.");
        
            this.objectType = objectType;
            this.interfaceType = interfaceType;
        }
    }
}