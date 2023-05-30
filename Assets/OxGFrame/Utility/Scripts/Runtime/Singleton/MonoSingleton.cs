using UnityEngine;

namespace OxGFrame.Utility.Singleton
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static readonly object _locker = new object();
        private static T _instance;

        public static T GetInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    _instance = FindObjectOfType(typeof(T)) as T;
                    if (_instance == null)
                    {
                        var go = new GameObject(typeof(T).Name);
                        _instance = go.AddComponent<T>();
                    }

                    if (Application.isPlaying) DontDestroyOnLoad(_instance);
                }
            }
            return _instance;
        }
    }
}