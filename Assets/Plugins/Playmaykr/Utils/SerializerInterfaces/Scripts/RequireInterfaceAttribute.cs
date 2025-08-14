using System;
using UnityEngine;

namespace Playmaykr.Utils.SerializedInterfaces
{
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