using UnityEngine;

namespace Playmaykr.Utils.AnimationEventsVisualizer
{
    /// <summary>
    /// A state machine behaviour that triggers an animation event at a specified normalized time.
    /// </summary>
    /// <remarks>
    /// This behaviour can be attached to an animation state in an Animator Controller.
    /// It will trigger the specified event when the animation reaches the defined normalized time.
    /// The event is can be received by an <c>AnimationEventReceiver</c> component on the same GameObject.
    /// </remarks>
    public class AnimationEventStateBehaviour : StateMachineBehaviour
    {
        public float TriggerTime => triggerTime;
        public string EventName => eventName;
        
        [SerializeField] private string eventName;
        [SerializeField, Range(0f, 1f)] private float triggerTime;

        private bool _hasTriggered;
        private AnimationEventReceiver _receiver;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _hasTriggered = false;
            _receiver = animator.GetComponent<AnimationEventReceiver>();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Normalized time can exceed 1 when the animation loops
            float currentTime = stateInfo.normalizedTime % 1f;
            
            if (!_hasTriggered && currentTime >= triggerTime)
            {
                NotifyReceiver();
                _hasTriggered = true;
            }
        }

        private void NotifyReceiver()
        {
            if (_receiver != null)
            {
                _receiver.OnAnimationEventTriggered(eventName);
            }
        }
    }
}
