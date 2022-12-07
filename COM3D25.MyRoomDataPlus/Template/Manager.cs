using UnityEngine;

namespace COM3D2Template
{
    internal abstract class Manager : MonoBehaviour
    {
        protected Helpers helper;

        #region Unity/BepInEx
        protected bool initialized { get; private set; }
        public void Initialize<T>(Helpers helper) where T: Hooks<T>, new()
        {
            //Copied from examples
            if (this.initialized)
                return;

            this.helper = helper;

            Hooks<T>.Default.Initialize(helper);
            this.initialized = true;
            this.helper.Log("Manager Initialize");
        }

        public void Awake()
        {
            //Copied from examples
            //helper.Log("Manager Awake"); can't call from here, can't include in constructor cause of AddComponent
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)this);
        }
        #endregion
    }
}