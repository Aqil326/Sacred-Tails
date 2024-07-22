using UnityEngine;
namespace Utils
{
    // public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    // {
    //     private static T instance;
    //     public static T Instance
    //     {
    //         get
    //         {
    //             if (instance == null)
    //             {
    //                 instance = FindObjectOfType<T>();
    //             }
    //             DontDestroyOnLoad(instance);
    //             return instance;
    //         }
    //     }
    // }

    public class Singleton<T> : Singleton where T : MonoBehaviour
    {
        #region  Fields
        private static T _instance;
        private static readonly object Lock = new object();

        [SerializeField]
        private bool _persistent = true;
        #endregion

        #region  Properties
        public static T Instance
        {
            get
            {
                if (Quitting)
                {
                    Debug.LogWarning($"[{nameof(Singleton)}<{typeof(T)}>] Instance will not be returned because the application is quitting.");
                    // ReSharper disable once AssignNullToNotNullAttribute
                    return null;
                }
                lock (Lock)
                {
                    if (_instance != null)
                        return _instance;
                    var instances = FindObjectsOfType<T>();
                    var count = instances.Length;
                    if (count > 0)
                    {
                        if (count == 1)
                            return _instance = instances[0];
                        Debug.LogWarning($"[{nameof(Singleton)}<{typeof(T)}>] There should never be more than one {nameof(Singleton)} of type {typeof(T)} in the scene, but {count} were found. The first instance found will be used, and all others will be destroyed.");
                        for (var i = 1; i < instances.Length; i++)
                            Destroy(instances[i]);
                        return _instance = instances[0];
                    }

                    SacredTailsLog.LogMessage($"[{nameof(Singleton)}<{typeof(T)}>] An instance is needed in the scene and no existing instances were found, so a new instance will be created.");
                    return _instance = new GameObject($"({nameof(Singleton)}){typeof(T)}")
                               .AddComponent<T>();
                }
            }
        }
        #endregion

        #region  Methods
        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(this.gameObject);
            }
            if (_persistent)
                DontDestroyOnLoad(gameObject);
            OnAwake();
        }

        protected virtual void OnAwake() { }
        #endregion
    }

    public abstract class Singleton : MonoBehaviour
    {
        #region  Properties
        public static bool Quitting { get; private set; }
        #endregion

        #region  Methods
        private void OnEnable()
        {
            Quitting = false;
        }
        private void OnApplicationQuit()
        {
            Quitting = true;
        }
        #endregion
    }
    //Free candy!
}