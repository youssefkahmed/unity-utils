using UnityEngine;

namespace Playmaykr.Utils.Singletons
{
    /// <summary>
    /// Persistent Regulator singleton, will destroy any other older components of the same type it finds on Awake
    /// </summary>
    public class RegulatorSingleton<T> : MonoBehaviour where T : Component
    {
        public static bool HasInstance => instance != null;

        public float InitializationTime { get; private set; }

        public static T Instance
        {
            get
            {
                if (!instance)
                {
                    instance = FindAnyObjectByType<T>();
                    if (!instance)
                    {
                        var go = new GameObject($"{typeof(T).Name} [Auto-Generated]")
                        {
                            hideFlags = HideFlags.HideAndDontSave
                        };
                        instance = go.AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        protected static T instance;

        /// <summary>
        /// Make sure to call base.Awake() in override if you need Awake.
        /// </summary>
        protected virtual void Awake() {
            InitializeSingleton();
        }

        protected virtual void InitializeSingleton()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            
            InitializationTime = Time.time;
            DontDestroyOnLoad(gameObject);

            T[] oldInstances = FindObjectsByType<T>(FindObjectsSortMode.None);
            foreach (T old in oldInstances)
            {
                if (old.GetComponent<RegulatorSingleton<T>>().InitializationTime < InitializationTime)
                {
                    Destroy(old.gameObject);
                }
            }

            if (instance == null)
            {
                instance = this as T;
            }
        }
    }
}