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
        /// <summary>
        /// Creates default project folders.
        /// </summary>
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

        /// <summary>
        /// Imports a group of (hard-coded) essential assets.
        /// </summary>
        [MenuItem("Playmaykr/Utils/Setup/Import Favorite Assets")]
        public static void ImportMyFavoriteAssets()
        {
            Assets.ImportAsset("DOTween HOTween v2.unitypackage", "Demigiant/Editor ExtensionsAnimation");
            Assets.ImportAsset("Gridbox Prototype Materials.unitypackage", "Ciathyza/Textures Materials");
            Assets.ImportAsset("Mulligan Renamer.unitypackage", "Red Blue Games/Editor ExtensionsUtilities");
        }

        /// <summary>
        /// Installs a group of (hard-coded) favorite, open-source assets from GitHub.
        /// </summary>
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
        
        /// <summary>
        /// Installs Unity's AI Navigation package.
        /// </summary>
        [MenuItem("Playmaykr/Utils/Setup/Unity Registry/Install Unity AI Navigation")]
        public static void InstallUnityAINavigation()
        {
            Packages.InstallPackages(new[]
            {
                "com.unity.ai.navigation"
            });
        }
        
        /// <summary>
        /// Installs Unity's New Input System package.
        /// </summary>
        [MenuItem("Playmaykr/Utils/Setup/Unity Registry/Install New Input System")]
        public static void InstallNewInputSystem()
        {
            Packages.InstallPackages(new[]
            {
                "com.unity.inputsystem"
            });
        }
        
        /// <summary>
        /// Installs Unity's Cinemachine package.
        /// </summary>
        [MenuItem("Playmaykr/Utils/Setup/Unity Registry/Install Cinemachine")]
        public static void InstallCinemachine()
        {
            Packages.InstallPackages(new[]
            {
                "com.unity.cinemachine"
            });
        }
        
        /// <summary>
        /// Installs Unity's Animation Rigging package.
        /// </summary>
        [MenuItem("Playmaykr/Utils/Setup/Unity Registry/Install Animation Rigging")]
        public static void InstallAnimationRigging()
        {
            Packages.InstallPackages(new[]
            {
                "com.unity.animation.rigging"
            });
        }
        
        /// <summary>
        /// Installs Unity's Netcode for GameObjects package.
        /// </summary>
        [MenuItem("Playmaykr/Utils/Setup/Unity Registry/Install Netcode for GameObjects")]
        public static void InstallNetcodeForGameObjects()
        {
            Packages.InstallPackages(new[]
            {
                "com.unity.netcode.gameobjects"
            });
        }
        
        /// <summary>
        /// Installs Unity's Shader Graph package.
        /// </summary>
        [MenuItem("Playmaykr/Utils/Setup/Unity Registry/Install Shader Graph")]
        public static void InstallShaderGraph()
        {
            Packages.InstallPackages(new[]
            {
                "com.unity.shadergraph"
            });
        }
        
        /// <summary>
        /// Installs Unity's TextMeshPro package.
        /// </summary>
        [MenuItem("Playmaykr/Utils/Setup/Unity Registry/Install TextMesh Pro")]
        public static void InstallTextMeshPro()
        {
            Packages.InstallPackages(new[]
            {
                "com.unity.textmeshpro"
            });
        }
        
        /// <summary>
        /// Installs Unity's XR Interaction Toolkit package.
        /// </summary>
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
            /// <summary>
            /// Creates default project folders in the specified root directory.
            /// </summary>
            /// <param name="root">The root directory where the folders will be created.</param>
            /// <param name="folders">An array of folder names to create under the root directory.</param>
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
    
            /// <summary>
            /// Creates subfolders in the specified root path based on the provided folder hierarchy.
            /// </summary>
            /// <param name="rootPath">The root path where the folders will be created.</param>
            /// <param name="folderHierarchy">The folder hierarchy to create, separated by slashes (e.g., "Folder1/Folder2/Folder3").</param>
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

            /// <summary>
            /// Installs a list of packages from the Unity Package Manager.
            /// </summary>
            /// <param name="packages">An enumerable collection of package names or URLs to install.</param>
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

            /// <summary>
            /// Monitors the installation progress of the package and logs the result.
            /// </summary>
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
            /// <summary>
            /// Imports a Unity package from the specified path.
            /// </summary>
            public static void ImportAsset(string asset, string subfolder, string rootFolder = "C:/Users/Youssef/AppData/Roaming/Unity/Asset Store-5.x")
            {
                ImportPackage(Combine(rootFolder, subfolder, asset), false);
            }
        }
    }
}