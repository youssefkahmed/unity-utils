using UnityEngine;

namespace Playmaykr.Utils.Singletons
{
    public class PersistentSingleton<T> : MonoBehaviour where T : Component
    {
        public bool AutoUnparentOnAwake { get; protected set; } = true;
        public static bool HasInstance => instance != null;

        public static T TryGetInstance() => HasInstance ? instance : null;

        public static T Instance
        {
            get {
                if (!instance)
                {
                    instance = FindAnyObjectByType<T>();
                    if (!instance)
                    {
                        var go = new GameObject($"{typeof(T).Name} [Auto-Generated]");
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
        protected virtual void Awake()
        {
            InitializeSingleton();
        }

        protected virtual void InitializeSingleton()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (AutoUnparentOnAwake)
            {
                transform.SetParent(null);
            }

            if (!instance)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (instance != this)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}