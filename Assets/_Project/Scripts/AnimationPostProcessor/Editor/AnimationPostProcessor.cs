using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Utils.AnimationPostProcessing
{
    public class AnimationPostProcessor : AssetPostprocessor
    {
        private static AnimationPostProcessorSettings _settings;
        private static Avatar _referenceAvatar;
        private static GameObject _referenceFBX;
        private static ModelImporter _referenceImporter;
        private static bool _settingsLoaded;

        private void OnPreprocessModel()
        {
            LoadSettings();
            if (!_settingsLoaded || !_settings.enabled)
            {
                return;
            }

            // Check if asset is in the specified folder
            var importer = assetImporter as ModelImporter;
            if (importer == null || !importer.assetPath.StartsWith(_settings.targetFolder))
            {
                return;
            }
            AssetDatabase.ImportAsset(importer.assetPath);
            
            // Extract materials and textures
            if (_settings.extractTextures)
            {
                importer.ExtractTextures($"{Path.GetDirectoryName(importer.assetPath)}/Textures");
                importer.materialLocation = ModelImporterMaterialLocation.External;
            }
            
            // Extract avatar from the reference FBX if not already specified
            if (_referenceAvatar == null)
            {
                _referenceAvatar = _referenceImporter.sourceAvatar;
            }
            
            // Set the avatar and rig type of the imported model
            importer.sourceAvatar = _referenceAvatar;
            importer.animationType = _settings.animationType;
            
            // Set the animation to Generic if there's an issue with the avatar
            if (_referenceAvatar == null || !_referenceAvatar.isValid)
            {
                importer.animationType = ModelImporterAnimationType.Generic;
            }
            
            // Use serialization to set the avatar correctly
            var destinationObject = new SerializedObject(importer.sourceAvatar);
            using (var sourceObject = new SerializedObject(_referenceAvatar))
            {
                CopyHumanDescriptionToDestination(sourceObject, destinationObject);
            }
            
            destinationObject.ApplyModifiedProperties();
            importer.sourceAvatar = destinationObject.targetObject as Avatar;
            destinationObject.Dispose();
            
            // Translation DoF
            if (_settings.enableTranslationDoF)
            {
                HumanDescription importerHumanDescription = importer.humanDescription;
                importerHumanDescription.hasTranslationDoF = true;
                importer.humanDescription = importerHumanDescription;
            }
            
            // Use reflection to instantiate an Editor and call the Apply method as if the Apply button was pressed
            if (_settings.forceEditorApply)
            {
                Type editorType = typeof(Editor).Assembly.GetType("UnityEditor.ModelImporterEditor");
                const BindingFlags nonPublic = BindingFlags.NonPublic | BindingFlags.Instance;
                var editor = Editor.CreateEditor(importer, editorType);
                
                editorType.GetMethod("Apply", nonPublic)?.Invoke(editor, null);
                UnityEngine.Object.DestroyImmediate(editor);
            }
        }

        private void OnPreprocessAnimation()
        {
            LoadSettings();
            if (!_settingsLoaded || !_settings.enabled)
            {
                return;
            }

            // Check if asset is in the specified folder
            var importer = assetImporter as ModelImporter;
            if (importer == null || !importer.assetPath.StartsWith(_settings.targetFolder))
            {
                return;
            }

            ModelImporter modelImporter = CopyModelImporterSettings(importer);
        
            AssetDatabase.ImportAsset(modelImporter.assetPath, ImportAssetOptions.ForceUpdate);
        }

        private static ModelImporter CopyModelImporterSettings(ModelImporter importer)
        {
            // model tab
            importer.globalScale = _referenceImporter.globalScale;
            importer.useFileScale = _referenceImporter.useFileScale;
            importer.meshCompression = _referenceImporter.meshCompression;
            importer.isReadable = _referenceImporter.isReadable;
            importer.optimizeMeshPolygons = _referenceImporter.optimizeMeshPolygons;
            importer.optimizeMeshVertices = _referenceImporter.optimizeMeshVertices;
            importer.importBlendShapes = _referenceImporter.importBlendShapes;
            importer.keepQuads = _referenceImporter.keepQuads;
            importer.indexFormat = _referenceImporter.indexFormat;
            importer.weldVertices = _referenceImporter.weldVertices;
            importer.importVisibility = _referenceImporter.importVisibility;
            importer.importCameras = _referenceImporter.importCameras;
            importer.importLights = _referenceImporter.importLights;
            importer.preserveHierarchy = _referenceImporter.preserveHierarchy;
            importer.swapUVChannels = _referenceImporter.swapUVChannels;
            importer.generateSecondaryUV = _referenceImporter.generateSecondaryUV;
            importer.importNormals = _referenceImporter.importNormals;
            importer.normalCalculationMode = _referenceImporter.normalCalculationMode;
            importer.normalSmoothingAngle = _referenceImporter.normalSmoothingAngle;
            importer.importTangents = _referenceImporter.importTangents;
        
            // rig tab
            importer.animationType = _referenceImporter.animationType;
            importer.optimizeGameObjects = _referenceImporter.optimizeGameObjects;
            
            // materials tab
            importer.materialImportMode = _referenceImporter.materialImportMode;
            importer.materialLocation = _referenceImporter.materialLocation;
            importer.materialName = _referenceImporter.materialName;
        
            // naming conventions
            // get the filename of the FBX in case we want to use it for the animation name
            string fileName = Path.GetFileNameWithoutExtension(importer.assetPath);
            
            // animations tab
            // return if there are no clips to copy on the reference importer
            if (_referenceImporter.clipAnimations.Length == 0)
            {
                return importer;
            }
            
            // Copy the first reference clip settings to all imported clips
            ModelImporterClipAnimation referenceClip = _referenceImporter.clipAnimations[0];
            var referenceClipAnimations = _referenceImporter.defaultClipAnimations;
            var defaultClipAnimations = importer.defaultClipAnimations;
            
            foreach (ModelImporterClipAnimation clipAnimation in defaultClipAnimations)
            {
                clipAnimation.hasAdditiveReferencePose = referenceClip.hasAdditiveReferencePose;
                if (referenceClip.hasAdditiveReferencePose)
                {
                    clipAnimation.additiveReferencePoseFrame = referenceClip.additiveReferencePoseFrame;
                }
            
                // Rename if needed
                if (_settings.renameClips)
                {
                    clipAnimation.name = referenceClipAnimations.Length == 1
                        ? fileName
                        : $"{fileName}@{clipAnimation.name}";
                }
                
                // Set loop time
                clipAnimation.loopTime = _settings.loopTime;

                clipAnimation.maskType = referenceClip.maskType;
                clipAnimation.maskSource = referenceClip.maskSource;

                clipAnimation.keepOriginalOrientation = referenceClip.keepOriginalOrientation;
                clipAnimation.keepOriginalPositionXZ = referenceClip.keepOriginalPositionXZ;
                clipAnimation.keepOriginalPositionY = referenceClip.keepOriginalPositionY;

                clipAnimation.lockRootRotation = referenceClip.lockRootRotation;
                clipAnimation.lockRootPositionXZ = referenceClip.lockRootPositionXZ;
                clipAnimation.lockRootHeightY = referenceClip.lockRootHeightY;

                clipAnimation.mirror = referenceClip.mirror;
                clipAnimation.wrapMode = referenceClip.wrapMode;
            }

            importer.clipAnimations = defaultClipAnimations;
            
            return importer;
        }
        
        private static void CopyHumanDescriptionToDestination(SerializedObject sourceObject, SerializedObject destinationObject)
        {
            destinationObject.CopyFromSerializedProperty(sourceObject.FindProperty("m_HumanDescription"));
        }
        
        private static void LoadSettings()
        {
            string[] guids = AssetDatabase.FindAssets("t:AnimationPostProcessorSettings");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _settings = AssetDatabase.LoadAssetAtPath<AnimationPostProcessorSettings>(path);
                if (_settings.referenceAvatar == null || _settings.referenceFBX == null)
                {
                    _settingsLoaded = false;
                    return;
                }
                
                _referenceAvatar = _settings.referenceAvatar;
                _referenceFBX = _settings.referenceFBX;
                _referenceImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_referenceFBX)) as ModelImporter;
                _settingsLoaded = true;
            }
            else
            {
                _settingsLoaded = false;
            }
        }
    }
}
