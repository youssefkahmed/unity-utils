#if ENABLED_UNITY_URP
using UnityEngine;
using UnityEngine.Rendering;
#endif

namespace Playmaykr.Utils.Extensions
{
    public static class ResourcesUtils
    {
#if ENABLED_UNITY_URP
        /// <summary>
        /// Loads a volume profile from a given path.
        /// </summary>
        /// <param name="path">Path from where volume profile should be loaded.</param>
        public static void LoadVolumeProfile(this Volume volume, string path)
        {
            var profile = Resources.Load<VolumeProfile>(path);
            volume.profile = profile;
        }
#endif
    }
}