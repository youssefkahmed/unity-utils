using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Playmaykr.Utils.AnimationPostProcessing.Editor
{
    public class AnimationPostProcessor : AssetPostprocessor
    {
        private static AnimationPostProcessorSettings Settings;
        private static Avatar ReferenceAvatar;
        private static GameObject ReferenceFBX;
        private static ModelImporter ReferenceImporter;
        private static bool SettingsLoaded;

        private void OnPreprocessModel()
        {
            LoadSettings();
            if (!SettingsLoaded || !Settings.enabled)
            {
                return;
            }

            // Check if asset is in the specified folder
            var importer = assetImporter as ModelImporter;
            if (importer == null || !importer.assetPath.StartsWith(Settings.targetFolder))
            {
                return;
            }
            AssetDatabase.ImportAsset(importer.assetPath);
            
            // Extract materials and textures
            if (Settings.extractTextures)
            {
                importer.ExtractTextures($"{Path.GetDirectoryName(importer.assetPath)}/Textures");
                importer.materialLocation = ModelImporterMaterialLocation.External;
            }
            
            // Extract avatar from the reference FBX if not already specified
            if (ReferenceAvatar == null)
            {
                ReferenceAvatar = ReferenceImporter.sourceAvatar;
            }
            
            // Set the avatar and rig type of the imported model
            importer.sourceAvatar = ReferenceAvatar;
            importer.animationType = Settings.animationType;
            
            // Set the animation to Generic if there's an issue with the avatar
            if (ReferenceAvatar == null || !ReferenceAvatar.isValid)
            {
                importer.animationType = ModelImporterAnimationType.Generic;
            }
            
            // Use serialization to set the avatar correctly
            var destinationObject = new SerializedObject(importer.sourceAvatar);
            using (var sourceObject = new SerializedObject(ReferenceAvatar))
            {
                CopyHumanDescriptionToDestination(sourceObject, destinationObject);
            }
            
            destinationObject.ApplyModifiedProperties();
            importer.sourceAvatar = destinationObject.targetObject as Avatar;
            destinationObject.Dispose();
            
            // Translation DoF
            if (Settings.enableTranslationDoF)
            {
                HumanDescription importerHumanDescription = importer.humanDescription;
                importerHumanDescription.hasTranslationDoF = true;
                importer.humanDescription = importerHumanDescription;
            }
            
            // Use reflection to instantiate an Editor and call the Apply method as if the Apply button was pressed
            if (Settings.forceEditorApply)
            {
                Type editorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ModelImporterEditor");
                const BindingFlags nonPublic = BindingFlags.NonPublic | BindingFlags.Instance;
                var editor = UnityEditor.Editor.CreateEditor(importer, editorType);
                
                editorType.GetMethod("Apply", nonPublic)?.Invoke(editor, null);
                UnityEngine.Object.DestroyImmediate(editor);
            }
        }

        private void OnPreprocessAnimation()
        {
            LoadSettings();
            if (!SettingsLoaded || !Settings.enabled)
            {
                return;
            }

            // Check if asset is in the specified folder
            var importer = assetImporter as ModelImporter;
            if (importer == null || !importer.assetPath.StartsWith(Settings.targetFolder))
            {
                return;
            }

            ModelImporter modelImporter = CopyModelImporterSettings(importer);
        
            AssetDatabase.ImportAsset(modelImporter.assetPath, ImportAssetOptions.ForceUpdate);
        }

        private static ModelImporter CopyModelImporterSettings(ModelImporter importer)
        {
            // model tab
            importer.globalScale = ReferenceImporter.globalScale;
            importer.useFileScale = ReferenceImporter.useFileScale;
            importer.meshCompression = ReferenceImporter.meshCompression;
            importer.isReadable = ReferenceImporter.isReadable;
            importer.optimizeMeshPolygons = ReferenceImporter.optimizeMeshPolygons;
            importer.optimizeMeshVertices = ReferenceImporter.optimizeMeshVertices;
            importer.importBlendShapes = ReferenceImporter.importBlendShapes;
            importer.keepQuads = ReferenceImporter.keepQuads;
            importer.indexFormat = ReferenceImporter.indexFormat;
            importer.weldVertices = ReferenceImporter.weldVertices;
            importer.importVisibility = ReferenceImporter.importVisibility;
            importer.importCameras = ReferenceImporter.importCameras;
            importer.importLights = ReferenceImporter.importLights;
            importer.preserveHierarchy = ReferenceImporter.preserveHierarchy;
            importer.swapUVChannels = ReferenceImporter.swapUVChannels;
            importer.generateSecondaryUV = ReferenceImporter.generateSecondaryUV;
            importer.importNormals = ReferenceImporter.importNormals;
            importer.normalCalculationMode = ReferenceImporter.normalCalculationMode;
            importer.normalSmoothingAngle = ReferenceImporter.normalSmoothingAngle;
            importer.importTangents = ReferenceImporter.importTangents;
        
            // rig tab
            importer.animationType = ReferenceImporter.animationType;
            importer.optimizeGameObjects = ReferenceImporter.optimizeGameObjects;
            
            // materials tab
            importer.materialImportMode = ReferenceImporter.materialImportMode;
            importer.materialLocation = ReferenceImporter.materialLocation;
            importer.materialName = ReferenceImporter.materialName;
        
            // naming conventions
            // get the filename of the FBX in case we want to use it for the animation name
            string fileName = Path.GetFileNameWithoutExtension(importer.assetPath);
            
            // animations tab
            // return if there are no clips to copy on the reference importer
            if (ReferenceImporter.clipAnimations.Length == 0)
            {
                return importer;
            }
            
            // Copy the first reference clip settings to all imported clips
            ModelImporterClipAnimation referenceClip = ReferenceImporter.clipAnimations[0];
            var referenceClipAnimations = ReferenceImporter.defaultClipAnimations;
            var defaultClipAnimations = importer.defaultClipAnimations;
            
            foreach (ModelImporterClipAnimation clipAnimation in defaultClipAnimations)
            {
                clipAnimation.hasAdditiveReferencePose = referenceClip.hasAdditiveReferencePose;
                if (referenceClip.hasAdditiveReferencePose)
                {
                    clipAnimation.additiveReferencePoseFrame = referenceClip.additiveReferencePoseFrame;
                }
            
                // Rename if needed
                if (Settings.renameClips)
                {
                    clipAnimation.name = referenceClipAnimations.Length == 1
                        ? fileName
                        : $"{fileName}@{clipAnimation.name}";
                }
                
                // Set loop time
                clipAnimation.loopTime = Settings.loopTime;

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
                Settings = AssetDatabase.LoadAssetAtPath<AnimationPostProcessorSettings>(path);
                if (Settings.referenceAvatar == null || Settings.referenceFBX == null)
                {
                    SettingsLoaded = false;
                    return;
                }
                
                ReferenceAvatar = Settings.referenceAvatar;
                ReferenceFBX = Settings.referenceFBX;
                ReferenceImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(ReferenceFBX)) as ModelImporter;
                SettingsLoaded = true;
            }
            else
            {
                SettingsLoaded = false;
            }
        }
    }
}
