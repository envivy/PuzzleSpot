using UnityEngine;

namespace BX
{
    public class BXSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = Object.FindObjectOfType(typeof(T)) as T;
                    if (null != _instance) return _instance;

                    GameObject container = new GameObject
                    {
                        name = typeof(T).ToString()
                    };
                    container.hideFlags = HideFlags.HideInHierarchy;
                    _instance = container.AddComponent(typeof(T)) as T;
                    Object.DontDestroyOnLoad(container);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            OnInstanceCreate();
        }

        protected virtual void OnInstanceCreate()
        {

        }
    }
}