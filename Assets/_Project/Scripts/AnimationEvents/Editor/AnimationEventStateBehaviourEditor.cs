using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Utils.AnimationEvents
{
    /// <summary>
    /// Custom editor for the AnimationEventStateBehaviour class, providing a GUI for previewing animation states
    /// and handling animation events within the Unity editor. Enables previewing animations and managing
    /// animation events directly in the editor.
    /// </summary>
    [CustomEditor(typeof(AnimationEventStateBehaviour))]
    public class AnimationEventStateBehaviourEditor : Editor
    {
        private Motion _previewClip;
        private float _previewTime;
        private bool _isPreviewing;

        private PlayableGraph _playableGraph;
        private AnimationMixerPlayable _mixer;
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var stateBehaviour = (AnimationEventStateBehaviour) target;
            if (!Validate(stateBehaviour, out string errorMessage))
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Info);
                return;
            }
            
            GUILayout.Space(10);

            if (_isPreviewing)
            {
                if (GUILayout.Button("Stop Preview"))
                {
                    EnforceTPose();
                    _isPreviewing = false;
                    AnimationMode.StopAnimationMode();
                    if (_playableGraph.IsValid())
                    {
                        _playableGraph.Destroy();
                    }
                }
                else
                {
                    PreviewAnimationClip(stateBehaviour);
                }
            }
            else if (GUILayout.Button("Preview"))
            {
                _isPreviewing = true;
                AnimationMode.StartAnimationMode();
            }

            GUILayout.Label($"Previewing at {_previewTime:F2}s", EditorStyles.helpBox);
        }

        #region Validation

        private bool Validate(AnimationEventStateBehaviour stateBehaviour, out string errorMessage)
        {
            AnimatorController animatorController = GetValidAnimatorController(out errorMessage);
            if (animatorController == null)
            {
                return false;
            }

            ChildAnimatorState matchingState = animatorController.layers
                .Select(layer => FindMatchingState(layer.stateMachine, stateBehaviour))
                .FirstOrDefault(state => state.state != null);

            _previewClip = GetAnimationClipFromMotion(matchingState.state?.motion);
            if (_previewClip == null)
            {
                errorMessage = "No valid AnimationClip found for the current state.";
                return false;
            }

            return true;
        }
        
        private AnimatorController GetValidAnimatorController(out string errorMessage)
        {
            errorMessage = string.Empty;

            GameObject targetGameObject = Selection.activeGameObject;
            if (targetGameObject == null)
            {
                errorMessage = "Please select a GameObject with an Animator to preview.";
                return null;
            }

            var animator = targetGameObject.GetComponent<Animator>();
            if (animator == null)
            {
                errorMessage = "The selected GameObject does not have an Animator component.";
                return null;
            }

            var animatorController = animator.runtimeAnimatorController as AnimatorController;
            if (animatorController == null)
            {
                errorMessage = "The selected Animator does not have a valid AnimatorController.";
                return null;
            }

            return animatorController;
        }

        private static AnimationClip GetAnimationClipFromMotion(Motion motion)
        {
            if (motion is AnimationClip clip)
            {
                return clip;
            }

            if (motion is BlendTree blendTree)
            {
                return blendTree.children
                    .Select(child => GetAnimationClipFromMotion(child.motion))
                    .FirstOrDefault(childClip => childClip);
            }

            return null;
        }
        
        #endregion

        #region Helpers

        private static ChildAnimatorState FindMatchingState(AnimatorStateMachine stateMachine, AnimationEventStateBehaviour stateBehaviour)
        {
            foreach (ChildAnimatorState state in stateMachine.states)
            {
                if (state.state.behaviours.Contains(stateBehaviour))
                {
                    return state;
                }
            }

            foreach (ChildAnimatorStateMachine subStateMachine in stateMachine.stateMachines)
            {
                ChildAnimatorState matchingState = FindMatchingState(subStateMachine.stateMachine, stateBehaviour);
                if (matchingState.state)
                {
                    return matchingState;
                }
            }

            return default;
        }

        [MenuItem("GameObject/Enforce T-Pose", false, 0)]
        private static void EnforceTPose()
        {
            GameObject selected = Selection.activeGameObject;
            if (!selected || !selected.TryGetComponent(out Animator animator) || !animator.avatar)
            {
                return;
            }

            var skeletonBones = animator.avatar.humanDescription.skeleton;
            foreach (HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones)))
            {
                // Last bone isn't actually a human bone
                if (bone == HumanBodyBones.LastBone) continue;

                Transform boneTransform = animator.GetBoneTransform(bone);
                if (!boneTransform) continue;

                SkeletonBone skeletonBone = skeletonBones.FirstOrDefault(sb => sb.name == boneTransform.name);
                if (skeletonBone.name == null) continue;

                if (bone == HumanBodyBones.Hips)
                {
                    boneTransform.localPosition = skeletonBone.position;
                }
                boneTransform.localRotation = skeletonBone.rotation;
            }
        }
        
        #endregion

        #region Previews & Sampling

        private void PreviewAnimationClip(AnimationEventStateBehaviour stateBehaviour)
        {
            AnimatorController animatorController = GetValidAnimatorController(out string _);
            if (!animatorController)
            {
                return;
            }

            ChildAnimatorState matchingState = animatorController.layers
                .Select(layer => FindMatchingState(layer.stateMachine, stateBehaviour))
                .FirstOrDefault(state => state.state != null);

            if (!matchingState.state)
            {
                return;
            }

            Motion motion = matchingState.state.motion;
            // Handle BlendTree logic
            if (motion is BlendTree)
            {
                SampleBlendTreeAnimation(stateBehaviour, stateBehaviour.TriggerTime);
                return;
            }

            // If it's a simple AnimationClip, sample it directly
            if (motion is AnimationClip clip)
            {
                _previewTime = stateBehaviour.TriggerTime * clip.length;
                AnimationMode.SampleAnimationClip(Selection.activeGameObject, clip, _previewTime);
            }
        }

        private void SampleBlendTreeAnimation(AnimationEventStateBehaviour stateBehaviour, float normalizedTime)
        {
            Selection.activeGameObject.TryGetComponent(out Animator animator);
            
            if (_playableGraph.IsValid())
            {
                _playableGraph.Destroy();
            }
            _playableGraph = PlayableGraph.Create("BlendTreePreviewGraph");
            _mixer = AnimationMixerPlayable.Create(_playableGraph, 1);

            var output = AnimationPlayableOutput.Create(_playableGraph, "Animation", animator);
            output.SetSourcePlayable(_mixer);

            AnimatorController animatorController = GetValidAnimatorController(out string _);
            if (!animatorController)
            {
                return;
            }

            ChildAnimatorState matchingState = animatorController.layers
                .Select(layer => FindMatchingState(layer.stateMachine, stateBehaviour))
                .FirstOrDefault(state => state.state);

            // If the matching state is not a BlendTree, bail out
            if (matchingState.state.motion is not BlendTree blendTree)
            {
                return;
            }
            
            // Determine the maximum threshold value in the blend tree
            float maxThreshold = blendTree.children.Max(child => child.threshold);

            var clipPlayables = new AnimationClipPlayable[blendTree.children.Length];
            var weights = new float[blendTree.children.Length];
            var totalWeight = 0f;

            // Scale target weight according to max threshold
            float targetWeight = Mathf.Clamp(normalizedTime * maxThreshold, blendTree.minThreshold, maxThreshold);

            for (var i = 0; i < blendTree.children.Length; i++)
            {
                ChildMotion child = blendTree.children[i];
                float weight = CalculateWeightForChild(blendTree, child, targetWeight);
                weights[i] = weight;
                totalWeight += weight;

                AnimationClip clip = GetAnimationClipFromMotion(child.motion);
                clipPlayables[i] = AnimationClipPlayable.Create(_playableGraph, clip);
            }

            // Normalize weights so they sum to 1
            for (var i = 0; i < weights.Length; i++)
            {
                weights[i] /= totalWeight;
            }

            _mixer.SetInputCount(clipPlayables.Length);
            for (var i = 0; i < clipPlayables.Length; i++)
            {
                _mixer.ConnectInput(i, clipPlayables[i], 0);
                _mixer.SetInputWeight(i, weights[i]);
            }

            AnimationMode.SamplePlayableGraph(_playableGraph, 0, normalizedTime);
        }

        private static float CalculateWeightForChild(BlendTree blendTree, ChildMotion child, float targetWeight)
        {
            var weight = 0f;

            if (blendTree.blendType == BlendTreeType.Simple1D)
            {
                // Find the neighbors around the target weight
                ChildMotion? lowerNeighbor = null;
                ChildMotion? upperNeighbor = null;

                foreach (ChildMotion motion in blendTree.children)
                {
                    if (motion.threshold <= targetWeight && (lowerNeighbor == null || motion.threshold > lowerNeighbor.Value.threshold))
                    {
                        lowerNeighbor = motion;
                    }

                    if (motion.threshold >= targetWeight && (upperNeighbor == null || motion.threshold < upperNeighbor.Value.threshold))
                    {
                        upperNeighbor = motion;
                    }
                }

                if (lowerNeighbor.HasValue && upperNeighbor.HasValue)
                {
                    if (Mathf.Approximately(child.threshold, lowerNeighbor.Value.threshold))
                    {
                        weight = 1.0f - Mathf.InverseLerp(lowerNeighbor.Value.threshold, upperNeighbor.Value.threshold, targetWeight);
                    }
                    else if (Mathf.Approximately(child.threshold, upperNeighbor.Value.threshold))
                    {
                        weight = Mathf.InverseLerp(lowerNeighbor.Value.threshold, upperNeighbor.Value.threshold, targetWeight);
                    }
                }
                else
                {
                    // Handle edge cases where there is no valid interpolation range
                    weight = Mathf.Approximately(targetWeight, child.threshold) ? 1f : 0f;
                }
            }
            else if (blendTree.blendType is BlendTreeType.FreeformCartesian2D or BlendTreeType.FreeformDirectional2D)
            {
                Vector2 targetPos = new(
                    GetBlendParameterValue(blendTree, blendTree.blendParameter),
                    GetBlendParameterValue(blendTree, blendTree.blendParameterY)
                );
                
                float distance = Vector2.Distance(targetPos, child.position);
                weight = Mathf.Clamp01(1.0f / (distance + 0.001f));
            }

            return weight;
        }

        private static float GetBlendParameterValue(BlendTree blendTree, string parameterName)
        {
            MethodInfo methodInfo = typeof(BlendTree).GetMethod("GetInputBlendValue", BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo == null)
            {
                Debug.LogError("Failed to find GetInputBlendValue method via reflection.");
                return 0f;
            }

            return (float) methodInfo.Invoke(blendTree, new object[] { parameterName });
        }
        
        #endregion
    }
}