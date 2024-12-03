using System.Collections.Generic;
using UnityEngine;

namespace Utils.AnimationEvents
{
    public class AnimationEventReceiver : MonoBehaviour
    {
        [SerializeField] private List<AnimationEvent> animationEvents = new();

        public void OnAnimationEventTriggered(string eventName)
        {
            AnimationEvent matchingEvent = animationEvents.Find(e => e.eventName == eventName);
            matchingEvent?.onAnimationEvent?.Invoke();
        }
    }
}