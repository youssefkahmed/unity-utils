using System;
using UnityEngine.Events;

namespace Utils.AnimationEvents
{
    [Serializable]
    public class AnimationEvent
    {
        public string eventName;
        public UnityEvent onAnimationEvent;
    }
}