using UnityEngine;

namespace Playmaykr.Utils.AnimationEventsVisualizer.Demo
{
    public class AEVDemo : MonoBehaviour
    {
        [SerializeField] private Animator capsuleAnimator;
        
        private static readonly int AnimHashJump = Animator.StringToHash("Jump");

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Space))
            {
                capsuleAnimator?.SetTrigger(AnimHashJump);
            }
        }
    }
}
