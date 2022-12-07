using BepInEx;
using System;
using System.IO;

namespace COM3D25.MyRoomDataPlus
{
    [BepInPlugin("org.guest4168.plugins." + PluginNameTech, PluginName, "1.0.0.0")]
    internal class MyRoomDataPlusCore : COM3D2Template.Core<MyRoomDataPlusManager, MyRoomDataPlusHooks>
    {
        public static MyRoomDataPlusHelper StaticHelpers;
        public const string PluginName = "COM3D25.MyRoomDataPlus";
        public const string PluginNameTech = "myroomdataplus";
        private static string gearIconBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAIAAAD8GO2jAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwQAADsEBuJFr7QAAABl0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC4xNkRpr/UAAAFmSURBVEhL3ZQhSwRRFIU3GAxGg8GwwWAw+AOM/gB/hMEgImJQVBARxKRB2LDBJsgGQQWjYUEEg4igRRCTgoLIIggK1/O4J8zcN7PDzntj8PDBHe65nMPswNakYv1VQW0uPkzm8OxwmMzh2eEwmcOzw2EyR44NvXbk6kkabZlqSv+CPeuCqrggqfdP2TmXoRV7nImqoAAPiJvYldVT9x6qzpfMtlLHmaiKC5KMb8vJLa3WtQws2oMkqt4KFHwM/FZQ+6Fbh6pMARjdlMc3d3B2J33z1lVUJQtAfV2eP9zN0rG1FFX5AjC5526+f2Rkw1pAFVQAmhfubP/S7oEqtGB4zb0BGFy2FhM40h7I2/sc3bjLmUO7V0UomD5wl6gxe1WEgrEtd3n/YveqCAX4B4TwGcxeFaEAZB5zyZH2QN7ep7c3CJeJ+n8FEWEyh2eHw2QOzw6HyRyeHQ6TOTw7HCZzVqaKC0R+ATD1DKWwDHhOAAAAAElFTkSuQmCC";

        public MyRoomDataPlusCore() : base(gearIconBase64, 
            new MyRoomDataPlusUI("MyRoomData+", new string[] { "SceneCreativeRoom" }), 
            new MyRoomDataPlusHelper(PluginName, PluginNameTech))
        {
            if(StaticHelpers == null)
            {
                StaticHelpers = (MyRoomDataPlusHelper)this.helper;
            }
        }


        private static string _bepinPath;
        public static string bepinPath
        {
            get
            {
                //Lazy init
                if (_bepinPath == null)
                {
                    _bepinPath = $@"{UTY.gameProjectPath}\BepinEx\plugins\[{StaticHelpers.PluginNameTech}]";

                    //Create the folder
                    if (!Directory.Exists(_bepinPath))
                    {
                        Directory.CreateDirectory(_bepinPath);
                    }

                    //Create subfolders
                    string[] subfolders = { "panel", "item", "bg" };
                    for (int i = 0; i < subfolders.Length; i++)
                    {
                        if (!Directory.Exists($@"{_bepinPath}\{subfolders[i]}"))
                        {
                            Directory.CreateDirectory($@"{_bepinPath}\{subfolders[i]}");
                        }
                    }
                }

                return _bepinPath;
            }
        }

        public class MyRoomDataPlusTexJSON
        {
            public int id { get; set; }
            public string name { get; set; }
            public string file { get; set; }

            public MyRoomDataPlusTexJSON()
            {
                id = 0;
                name = "";
                file = "";
            }
        }

        public class MyRoomDataPlusObjJSON
        {
            public virtual int idCategory { get; set; }
            public virtual string nameCategory { get; set; }
            public int idObject { get; set; }
            public string nameObject { get; set; }
            public string thumbnailObject { get; set; }

            public MyRoomDataPlusObjJSON()
            {
                idCategory = 0;
                nameCategory = "";

                idObject = 0;
                nameObject = "";
                thumbnailObject = "";
            }
        }

        public class MyRoomDataPlusItemJSON : MyRoomDataPlusObjJSON
        {
            public string fileObject { get; set; }

            public MyRoomDataPlusItemJSON() : base()
            {
                fileObject = "";
            }
        }

        public class MyRoomDataPlusBGJSON : MyRoomDataPlusObjJSON
        {
            public int idBG { get; set; }
            public string fileDayObject { get; set; }
            public string fileNightObject { get; set; }

            public override int idCategory { get { return 1100; } }
            public override string nameCategory { get { return "背景"; } }

            public MyRoomDataPlusBGJSON() : base()
            {
                idBG = 0;
                fileDayObject = "";
                fileNightObject = "";

                idCategory = 1100;
                nameCategory = "背景";
            }
        }
    }
}