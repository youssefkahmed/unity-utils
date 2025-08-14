using System;
using UnityEngine.Events;

namespace Playmaykr.Utils.AnimationEventsVisualizer
{
    /// <summary>
    /// Represents an animation event with a name and a Unity event to be invoked.
    /// </summary>
    /// <remarks>
    /// This class is used to define custom animation events that can be triggered during animations.
    /// The `eventName` is a string identifier for the event, and `onAnimationEvent` is a UnityEvent that will be invoked when the event occurs.
    /// </remarks>
    [Serializable]
    public class AnimationEvent
    {
        public string eventName;
        public UnityEvent onAnimationEvent;
    }
}