using System;
using UnityEngine;

namespace Playmaykr.Utils.SerializedInterfaces
{
    /// <summary>
    /// Attribute to mark a field as requiring a specific interface.
    /// This is used to ensure that the field can only be assigned objects that implement the specified interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RequireInterfaceAttribute : PropertyAttribute
    {
        public Type InterfaceType { get; private set; }

        public RequireInterfaceAttribute(Type interfaceType)
        {
            Debug.Assert(interfaceType.IsInterface, $"{nameof(interfaceType)} needs to be an interface.");
            InterfaceType = interfaceType;
        }
    }
}