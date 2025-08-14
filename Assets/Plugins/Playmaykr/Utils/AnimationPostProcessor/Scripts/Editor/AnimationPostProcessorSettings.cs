using UnityEditor;
using UnityEngine;

namespace Playmaykr.Utils.AnimationPostProcessing.Editor
{
    /// <summary>
    /// Allows configuration of Animation Post Processor settings.
    /// </summary>
    /// <remarks>
    /// To use this, create an instance of this ScriptableObject in your project and configure
    /// the settings as needed. The post processor will use these settings when processing animations.
    /// </remarks>
    [CreateAssetMenu(fileName = "AnimationPostProcessorSettings", menuName = "Playmaykr/Utils/Animations/AnimationPostProcessor/Settings", order = 1)]
    public class AnimationPostProcessorSettings : ScriptableObject
    {
        public bool enabled = true;
        public string targetFolder = "Assets/_Project/Animations";
        public Avatar referenceAvatar;
        public GameObject referenceFBX;
    
        public bool enableTranslationDoF = true;
        public ModelImporterAnimationType animationType = ModelImporterAnimationType.Human;
        public bool loopTime = true;
        public bool renameClips = true;
        public bool forceEditorApply = true;
        public bool extractTextures = true;
    }
}