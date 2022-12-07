using HarmonyLib;

namespace COM3D2Template
{
    public abstract class Hooks<T> where T:Hooks<T>, new()
    {
        //Single instance
        public static T Default { get; } = new T();

        #region Unity/BepInEx
        protected bool initialized { get; set; }
        protected Harmony instance { get; set; }
        protected Helpers helper;

        public void Initialize(Helpers _helper)
        {
            //Copied from examples
            if (Default.initialized)
                return;

            helper = _helper;
            Default.instance = Harmony.CreateAndPatchAll(typeof(T), "org.guest4168." + helper.PluginNameTech + ".hooks.base");
            Default.initialized = true;

            helper.Log("Hooks Initialize");
        }

        #endregion
    }
}