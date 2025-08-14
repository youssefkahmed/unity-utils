using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using static System.IO.Path;
using static UnityEditor.AssetDatabase;

namespace Playmaykr.Utils.ProjectSetupTool.Editor
{
    public static class Setup
    {
        [MenuItem("Playmaykr/Utils/Setup/Create Default Folders")]
        public static void CreateDefaultFolders()
        {
            Folders.CreateDefault("_Project", 
                "Animations",
                "Data",
                "Fonts",
                "Input",
                "Materials",
                "Models",
                "Prefabs",
                "Scenes",
                "Scripts",
                "SFX",
                "Shaders",
                "Textures");
            
            Refresh();
        }

        [MenuItem("Playmaykr/Utils/Setup/Import Favorite Assets")]
        public static void ImportMyFavoriteAssets()
        {
            Assets.ImportAsset("DOTween HOTween v2.unitypackage", "Demigiant/Editor ExtensionsAnimation");
            Assets.ImportAsset("Gridbox Prototype Materials.unitypackage", "Ciathyza/Textures Materials");
            Assets.ImportAsset("Mulligan Renamer.unitypackage", "Red Blue Games/Editor ExtensionsUtilities");
        }

        [MenuItem("Playmaykr/Utils/Setup/Install Favorite Open-Source Assets")]
        public static void InstallOpenSource()
        {
            Packages.InstallPackages(new[]
            {
                "git+https://github.com/KyleBanks/scene-ref-attribute",
                "git+https://github.com/starikcetin/Eflatun.SceneReference.git#3.1.1",
                "git+https://github.com/adammyhre/Unity-Utils"
            });
        }
        
        [MenuItem("Playmaykr/Utils/Setup/Unity Registry/Install Unity AI Navigation")]
        public static void InstallUnityAINavigation()
        {
            Packages.InstallPackages(new[]
            {
                "com.unity.ai.navigation"
            });
        }
        
        [MenuItem("Playmaykr/Utils/Setup/Unity Registry/Install New Input System")]
        public static void InstallNewInputSystem()
        {
            Packages.InstallPackages(new[]
            {
                "com.unity.inputsystem"
            });
        }
        
        [MenuItem("Playmaykr/Utils/Setup/Unity Registry/Install Cinemachine")]
        public static void InstallCinemachine()
        {
            Packages.InstallPackages(new[]
            {
                "com.unity.cinemachine"
            });
        }
        
        [MenuItem("Playmaykr/Utils/Setup/Unity Registry/Install Animation Rigging")]
        public static void InstallAnimationRigging()
        {
            Packages.InstallPackages(new[]
            {
                "com.unity.animation.rigging"
            });
        }
        
        [MenuItem("Playmaykr/Utils/Setup/Unity Registry/Install Netcode for GameObjects")]
        public static void InstallNetcodeForGameObjects()
        {
            Packages.InstallPackages(new[]
            {
                "com.unity.netcode.gameobjects"
            });
        }
        
        [MenuItem("Playmaykr/Utils/Setup/Unity Registry/Install Shader Graph")]
        public static void InstallShaderGraph()
        {
            Packages.InstallPackages(new[]
            {
                "com.unity.shadergraph"
            });
        }
        
        [MenuItem("Playmaykr/Utils/Setup/Unity Registry/Install TextMesh Pro")]
        public static void InstallTextMeshPro()
        {
            Packages.InstallPackages(new[]
            {
                "com.unity.textmeshpro"
            });
        }
        
        [MenuItem("Playmaykr/Utils/Setup/Unity Registry/Install XR Interaction Toolkit")]
        public static void InstallXRInteractionToolkit()
        {
            Packages.InstallPackages(new[]
            {
                "com.unity.xr.interaction.toolkit"
            });
        }

        private static class Folders
        {
            public static void CreateDefault(string root, params string[] folders)
            {
                string fullPath = Combine(Application.dataPath, root);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
                
                foreach (string folder in folders)
                {
                    CreateSubFolders(fullPath, folder);
                }
            }
    
            private static void CreateSubFolders(string rootPath, string folderHierarchy)
            {
                string[] folders = folderHierarchy.Split('/');
                string currentPath = rootPath;
                foreach (string folder in folders)
                {
                    currentPath = Combine(currentPath, folder);
                    if (!Directory.Exists(currentPath))
                    {
                        Directory.CreateDirectory(currentPath);
                    }
                }
            }
        }

        private static class Packages
        {
            private static AddRequest Request;
            private static readonly Queue<string> PackagesToInstall = new();

            public static void InstallPackages(IEnumerable<string> packages)
            {
                foreach (string package in packages)
                {
                    PackagesToInstall.Enqueue(package);
                }

                // Start the installation of the first package
                if (PackagesToInstall.Count > 0)
                {
                    Request = Client.Add(PackagesToInstall.Dequeue());
                    EditorApplication.update += Progress;
                }
            }

            private static async void Progress()
            {
                if (!Request.IsCompleted)
                {
                    return;
                }
                
                if (Request.Status == StatusCode.Success)
                {
                    Debug.Log($"Installed: {Request.Result.packageId}");
                }
                else if (Request.Status >= StatusCode.Failure)
                {
                    Debug.Log(Request.Error.message);
                }

                EditorApplication.update -= Progress;

                // If there are more packages to install, start the next one
                if (PackagesToInstall.Count > 0)
                {
                    // Add delay before next package install
                    await Task.Delay(1000);
                    Request = Client.Add(PackagesToInstall.Dequeue());
                    EditorApplication.update += Progress;
                }
            }
        }

        private static class Assets
        {
            public static void ImportAsset(string asset, string subfolder, string rootFolder = "C:/Users/Youssef/AppData/Roaming/Unity/Asset Store-5.x")
            {
                ImportPackage(Combine(rootFolder, subfolder, asset), false);
            }
        }
    }
}