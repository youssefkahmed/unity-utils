using System.Collections.Generic;
using UnityEngine;

namespace Playmaykr.Utils.AnimationEventsVisualizer
{
    /// <summary>
    /// Receives animation events and invokes the corresponding actions.
    /// </summary>
    /// <remarks>
    /// This component should be attached to a GameObject that will receive animation events.
    /// It contains a list of AnimationEvent objects that define the events and their associated actions.
    /// The OnAnimationEventTriggered method is called by the animation system when an event occurs,
    /// and it looks for a matching event in the list to invoke the associated action.
    /// </remarks>
    public class AnimationEventReceiver : MonoBehaviour
    {
        [SerializeField] private List<AnimationEvent> animationEvents = new();

        /// <summary>
        /// Called when an animation event is triggered.
        /// </summary>
        /// <param name="eventName">The name of the animation event that was triggered.</param>
        /// <remarks>
        /// This method searches the list of animation events for an event with a matching name.
        /// If a matching event is found, it invokes the associated UnityEvent action.
        /// </remarks>
        public void OnAnimationEventTriggered(string eventName)
        {
            AnimationEvent matchingEvent = animationEvents.Find(e => e.eventName == eventName);
            matchingEvent?.onAnimationEvent?.Invoke();
        }
    }
}