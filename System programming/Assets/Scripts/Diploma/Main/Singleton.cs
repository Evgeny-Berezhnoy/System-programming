using UnityEngine;

namespace Main
{
    public class Singleton<T> : MonoBehaviour where T : Object
    {
        #region Fileds

        private static T _staticInstance;

        #endregion

        #region Properties

        public static T Instance
        {
            get
            {
                if (_staticInstance != null) return _staticInstance;
                
                _staticInstance = FindObjectOfType(typeof(T)) as T;

                if (_staticInstance is null)
                {
                    Debug.LogError("An instance of " + typeof(T) + " is needed in the scene, but there is none.");
                };

                return _staticInstance;
            }
        }

        #endregion
    }
}