namespace COM3D2Template
{
    public abstract class Helpers
    {
        public string PluginName { get; }
        public string PluginNameTech { get; }

        public Helpers(string name, string nameTech)
        {
            PluginName = name;
            PluginNameTech = nameTech;
        }

        public void Log(string message)
        {
            UnityEngine.Debug.Log(this.PluginName + ":" + message);
        }
    }
}