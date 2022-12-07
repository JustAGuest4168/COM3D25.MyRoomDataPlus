using HarmonyLib;
using MyRoomCustom;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using wf;
using static COM3D25.MyRoomDataPlus.MyRoomDataPlusCore;

namespace COM3D25.MyRoomDataPlus
{
    internal class MyRoomDataPlusHooks : COM3D2Template.Hooks<MyRoomDataPlusHooks>
    {
        private static bool PlacementData_CreateCategoryData_Init = false;
        private static bool PlacementData_CreateData_Null = false;
        private static bool PlacementData_CreateData_Full = false;
        private static MyRoomDataPlusBGJSON placementBGJSON = null;
        private static MyRoomDataPlusItemJSON placementItemJSON = null;
        private static bool TextureData_CreateData_Init = false;
        private static MyRoomDataPlusTexJSON texturePanelJSON = null;

        ///==========================================================
        ///Call Sequence
        ///==========================================================
        ///MyRoomCustom_PlacementData_CreateData_Prefix
        ///MyRoomCustom.PlacementData.CreateData()
        ///     MyRoomCustom_PlacementData_CreateCategoryData_Prefix
        ///     MyRoomCustom.PlacementData.CreateCategoryData()
        ///     MyRoomCustom_PlacementData_CreateCategoryData_Postfix => if 1st call, add new categories by reading "item" and "bg" files
        ///     PhotoBGData.Create()
        ///     PhotoBGData_Create_Postfix => add "bg" files to photo data
        ///                                => if PhotoBGData.Create() was called by PlacementData.CreateData add the "bg" files to CustomRoom Data
        ///                                => if PhotoBGData.Create() was called by PlacementData.CreateData add the "item" files to CustomRoom Data
        ///                                => if PhotoBGData.Create() was called by PlacementData.CreateData add the "wall" files to CustomRoom Data
        ///         MyRoomCustom_PlacementData_Data_Constructor_Prefix
        ///         MyRoomCustom.PlacementData.Data() [Constructor]
        ///MyRoomCustom_PlacementData_CreateData_Postfix

        #region PlacementData

        #region Category
        [HarmonyPatch(typeof(MyRoomCustom.PlacementData), "CreateCategoryData", new Type[] { })]
        [HarmonyPrefix()]
        public static void MyRoomCustom_PlacementData_CreateCategoryData_Prefix()
        {
            PlacementData_CreateCategoryData_Init = (AccessTools.Field(typeof(PlacementData), "categoryIdManager").GetValue(null) == null);
        }

        [HarmonyPatch(typeof(MyRoomCustom.PlacementData), "CreateCategoryData", new Type[] { })]
        [HarmonyPostfix()]
        public static void MyRoomCustom_PlacementData_CreateCategoryData_Postfix()
        {
            if (PlacementData_CreateCategoryData_Init)
            {
                //Get private variables
                CsvCommonIdManager categoryIdManager = (CsvCommonIdManager)AccessTools.Field(typeof(PlacementData), "categoryIdManager").GetValue(null);
                SortedDictionary<int, KeyValuePair<string, string>> idMap = (SortedDictionary<int, KeyValuePair<string, string>>)AccessTools.Field(typeof(CsvCommonIdManager), "idMap").GetValue(categoryIdManager);
                Dictionary<string, int> nameMap = (Dictionary<string, int>)AccessTools.Field(typeof(CsvCommonIdManager), "nameMap").GetValue(categoryIdManager);
                HashSet<int> enabledIdList = (HashSet<int>)AccessTools.Field(typeof(CsvCommonIdManager), "enabledIdList").GetValue(categoryIdManager);

                //Get the path to JSON files
                string[] files = Directory.GetFiles($@"{bepinPath}\item").AddRangeToArray(Directory.GetFiles($@"{bepinPath}\bg"));

                //Loop files in folder
                for (int i = 0; i < files.Length; i++)
                {
                    //Get Object
                    MyRoomDataPlusObjJSON obj = null;
                    using (StreamReader file = File.OpenText(files[i]))
                    {
                        JsonSerializer ser = new JsonSerializer();
                        obj = (MyRoomDataPlusObjJSON)ser.Deserialize(file, typeof(MyRoomDataPlusObjJSON));
                    }

                    if (obj != null)
                    {
                        //Set Data
                        if (!idMap.ContainsKey(obj.idCategory) && !nameMap.ContainsKey(obj.nameCategory))
                        {
                            idMap[obj.idCategory] = new KeyValuePair<string, string>(obj.nameCategory, "");
                            nameMap[obj.nameCategory] = obj.idCategory;
                            enabledIdList.Add(obj.idCategory);

                            StaticHelpers.Log($@"Created MyRoom Category({obj.idCategory}): {obj.nameCategory}");
                        }
                    }
                }

                //Set private variables
                AccessTools.Field(typeof(CsvCommonIdManager), "enabledIdList").SetValue(categoryIdManager, enabledIdList);
                AccessTools.Field(typeof(CsvCommonIdManager), "nameMap").SetValue(categoryIdManager, nameMap);
                AccessTools.Field(typeof(CsvCommonIdManager), "idMap").SetValue(categoryIdManager, idMap);
                AccessTools.Field(typeof(PlacementData), "categoryIdManager").SetValue(null, categoryIdManager);

                //Reset
                PlacementData_CreateCategoryData_Init = false;
            }

        }
        #endregion

        #region Data
        [HarmonyPatch(typeof(MyRoomCustom.PlacementData), nameof(MyRoomCustom.PlacementData.CreateData), new Type[] { })]
        [HarmonyPrefix()]
        public static void MyRoomCustom_PlacementData_CreateData_Prefix()
        {
            PlacementData_CreateData_Null = (((HashSet<int>)AccessTools.Field(typeof(PlacementData), "enabledIDList").GetValue(null)) == null);
            PlacementData_CreateData_Full = ((bool)(UnityEngine.Object)GameObject.Find("CreativeRoomManager"));
        }

        [HarmonyPatch(typeof(MyRoomCustom.PlacementData), nameof(MyRoomCustom.PlacementData.CreateData), new Type[] { })]
        [HarmonyPostfix()]
        public static void MyRoomCustom_PlacementData_CreateData_Postfix()
        {
            if (PlacementData_CreateData_Null)
            {
                if (!PlacementData_CreateData_Full)
                {
                    //Add BGs to MyRoom data
                    List<MyRoomDataPlusBGJSON> dataBGs = _get_MyRoomDataPlusBGJSONs();
                    _addPlacementDataBGJSONs(dataBGs);
                }

                //Add Items to MyRoom data
                List<MyRoomDataPlusItemJSON> dataItems = _get_MyRoomDataPlusItemJSONs();
                _addPlacementDataItemJSONs(dataItems);

                PlacementData_CreateData_Null = false;
            }
        }

        [HarmonyPatch(typeof(MyRoomCustom.PlacementData.Data), MethodType.Constructor, new Type[] { typeof(int), typeof(CsvParser) })]
        [HarmonyPrefix()]
        public static bool MyRoomCustom_PlacementData_Data_Constructor_Prefix(int uniqueID, MyRoomCustom.PlacementData.Data __instance)
        {
            if (uniqueID != -1)
            {
                return true;
            }

            //Verify custom JSON available
            if (placementBGJSON != null)
            {
                //Set Data
                AccessTools.Field(typeof(MyRoomCustom.PlacementData.Data), "ID").SetValue(__instance, placementBGJSON.idObject);
                AccessTools.Field(typeof(MyRoomCustom.PlacementData.Data), "categoryID").SetValue(__instance, 1100);//placementBGJSON.);
                AccessTools.Field(typeof(MyRoomCustom.PlacementData.Data), "drawName").SetValue(__instance, placementBGJSON.nameObject);
                AccessTools.Field(typeof(MyRoomCustom.PlacementData.Data), "thumbnailName").SetValue(__instance, placementBGJSON.thumbnailObject);
                AccessTools.Field(typeof(MyRoomCustom.PlacementData.Data), "assetName").SetValue(__instance, "");
                AccessTools.Field(typeof(MyRoomCustom.PlacementData.Data), "resourceName").SetValue(__instance, placementBGJSON.fileDayObject);
                AccessTools.Field(typeof(MyRoomCustom.PlacementData.Data), "resourceNightName").SetValue(__instance, placementBGJSON.fileNightObject);

                StaticHelpers.Log($@"MyRoomCustom_PlacementData_Data_Constructor_Prefix placementBGJSON {placementBGJSON.idObject}");

                return false;
            }

            //Verify custom JSON available
            if (placementItemJSON != null)
            {
                //Set Data
                AccessTools.Field(typeof(MyRoomCustom.PlacementData.Data), "ID").SetValue(__instance, placementItemJSON.idObject);
                AccessTools.Field(typeof(MyRoomCustom.PlacementData.Data), "categoryID").SetValue(__instance, placementItemJSON.idCategory);
                AccessTools.Field(typeof(MyRoomCustom.PlacementData.Data), "drawName").SetValue(__instance, placementItemJSON.nameObject);
                AccessTools.Field(typeof(MyRoomCustom.PlacementData.Data), "thumbnailName").SetValue(__instance, placementItemJSON.thumbnailObject);
                AccessTools.Field(typeof(MyRoomCustom.PlacementData.Data), "assetName").SetValue(__instance, placementItemJSON.fileObject);
                AccessTools.Field(typeof(MyRoomCustom.PlacementData.Data), "resourceName").SetValue(__instance, "");
                AccessTools.Field(typeof(MyRoomCustom.PlacementData.Data), "resourceNightName").SetValue(__instance, "");

                StaticHelpers.Log($@"MyRoomCustom_PlacementData_Data_Constructor_Prefix placementItemJSON {placementItemJSON.idObject}");

                return false;
            }

            return true;
        }

        #endregion

        #endregion

        #region PhotoBGData
        [HarmonyPatch(typeof(PhotoBGData), nameof(PhotoBGData.Create), new Type[] { })]
        [HarmonyPostfix()]
        public static void PhotoBGData_Create_Postfix()
        {
            //Get private variables
            List<PhotoBGData> bg_data_ = (List<PhotoBGData>)AccessTools.Field(typeof(PhotoBGData), "bg_data_").GetValue(null);

            //Data
            List<MyRoomDataPlusBGJSON> data = _get_MyRoomDataPlusBGJSONs();
            for (int i = 0; i < data.Count; i++)
            {
                MyRoomDataPlusBGJSON obj = data[i];

                //Day object
                if (!string.IsNullOrEmpty(obj.fileDayObject))
                {
                    PhotoBGData photoBgDataDay = AccessTools.CreateInstance<PhotoBGData>();
                    photoBgDataDay.id = obj.idBG.ToString();
                    photoBgDataDay.name = obj.nameObject;
                    photoBgDataDay.create_prefab_name = obj.fileDayObject;
                    photoBgDataDay.category = "昼";
                    bg_data_.Add(photoBgDataDay);

                    StaticHelpers.Log($@"Created PhotoBG Day ({obj.idBG}): {obj.nameObject}");
                }

                //Night object
                if (!string.IsNullOrEmpty(obj.fileNightObject))
                {
                    PhotoBGData photoBgDataNight = AccessTools.CreateInstance<PhotoBGData>();
                    photoBgDataNight.id = (obj.idBG + 1).ToString();
                    photoBgDataNight.name = obj.nameObject;
                    photoBgDataNight.create_prefab_name = obj.fileNightObject;
                    photoBgDataNight.category = "夜";
                    bg_data_.Add(photoBgDataNight);

                    StaticHelpers.Log($@"Created PhotoBG Night ({obj.idBG}): {obj.nameObject}");
                }
            }

            //Set private variables
            AccessTools.Field(typeof(PhotoBGData), "bg_data_").SetValue(null, bg_data_);

            //Add to PlacementData if in Room Editor
            if (PlacementData_CreateData_Full)
            {
                _addPlacementDataBGJSONs(data);
            }
        }

        private static List<MyRoomDataPlusBGJSON> _get_MyRoomDataPlusBGJSONs()
        {
            //Data
            List<MyRoomDataPlusBGJSON> data = new List<MyRoomDataPlusBGJSON>();

            //Get the path to JSON files
            string[] files = Directory.GetFiles($@"{bepinPath}\bg");

            //Loop files in folder
            for (int i = 0; i < files.Length; i++)
            {
                //Get Object
                MyRoomDataPlusBGJSON obj = null;
                using (StreamReader file = File.OpenText(files[i]))
                {
                    JsonSerializer ser = new JsonSerializer();
                    obj = (MyRoomDataPlusBGJSON)ser.Deserialize(file, typeof(MyRoomDataPlusBGJSON));
                }

                if (obj != null)
                {
                    data.Add(obj);
                }
            }

            return data;
        }
        private static void _addPlacementDataBGJSONs(List<MyRoomDataPlusBGJSON> dataBGs)
        {
            //Get Private Variables
            HashSet<int> enabledIDList = (HashSet<int>)AccessTools.Field(typeof(PlacementData), "enabledIDList").GetValue(null);
            Dictionary<int, PlacementData.Data> basicDatas = (Dictionary<int, PlacementData.Data>)AccessTools.Field(typeof(PlacementData), "basicDatas").GetValue(null);

            //Loop data
            for (int i = 0; i < dataBGs.Count; i++)
            {
                placementBGJSON = dataBGs[i];

                //Enabled List
                enabledIDList.Add(placementBGJSON.idObject);

                //Basic Data
                PlacementData.Data placementData = new PlacementData.Data(-1, new CsvParser());
                basicDatas.Add(placementBGJSON.idObject, placementData);

                StaticHelpers.Log($@"Created MyRoom BG ({placementData.ID}): {placementData.resourceName}/{placementData.resourceNightName}");
            }

            //Reset variable
            placementBGJSON = null;

            //Set Private Variables
            AccessTools.Field(typeof(PlacementData), "enabledIDList").SetValue(null, enabledIDList);
            AccessTools.Field(typeof(PlacementData), "basicDatas").SetValue(null, basicDatas);
        }

        private static List<MyRoomDataPlusItemJSON> _get_MyRoomDataPlusItemJSONs()
        {
            //Data
            List<MyRoomDataPlusItemJSON> data = new List<MyRoomDataPlusItemJSON>();

            //Get the path to JSON files
            string[] files = Directory.GetFiles($@"{bepinPath}\item");

            //Loop files in folder
            for (int i = 0; i < files.Length; i++)
            {
                //Get Object
                MyRoomDataPlusItemJSON obj = null;
                using (StreamReader file = File.OpenText(files[i]))
                {
                    JsonSerializer ser = new JsonSerializer();
                    obj = (MyRoomDataPlusItemJSON)ser.Deserialize(file, typeof(MyRoomDataPlusItemJSON));
                }

                if (obj != null)
                {
                    data.Add(obj);
                }
            }

            return data;
        }
        private static void _addPlacementDataItemJSONs(List<MyRoomDataPlusItemJSON> dataItems)
        {
            //Get Private Variables
            HashSet<int> enabledIDList = (HashSet<int>)AccessTools.Field(typeof(PlacementData), "enabledIDList").GetValue(null);
            Dictionary<int, PlacementData.Data> basicDatas = (Dictionary<int, PlacementData.Data>)AccessTools.Field(typeof(PlacementData), "basicDatas").GetValue(null);

            //Loop data
            for (int i = 0; i < dataItems.Count; i++)
            {
                placementItemJSON = dataItems[i];

                //Enabled List
                enabledIDList.Add(placementItemJSON.idObject);

                //Basic Data
                PlacementData.Data placementData = new PlacementData.Data(-1, new CsvParser());
                basicDatas.Add(placementItemJSON.idObject, placementData);

                StaticHelpers.Log($@"Created MyRoom Item ({placementData.ID}): {placementData.assetName}");
            }

            //Reset variable
            placementItemJSON = null;

            //Set Private Variables
            AccessTools.Field(typeof(PlacementData), "enabledIDList").SetValue(null, enabledIDList);
            AccessTools.Field(typeof(PlacementData), "basicDatas").SetValue(null, basicDatas);
        }
        #endregion

        #region TextureData
        [HarmonyPatch(typeof(MyRoomCustom.TextureData), nameof(MyRoomCustom.TextureData.CreateData), new Type[] { })]
        [HarmonyPrefix()]
        public static void MyRoomCustom_TextureData_CreateData_Prefix()
        {
            TextureData_CreateData_Init = (AccessTools.Field(typeof(TextureData), "enabledIDList").GetValue(null) == null); ;
        }

        [HarmonyPatch(typeof(MyRoomCustom.TextureData), nameof(MyRoomCustom.TextureData.CreateData), new Type[] { })]
        [HarmonyPostfix()]
        public static void MyRoomCustom_TextureData_CreateData_Postfix()
        {
            if (TextureData_CreateData_Init)
            {
                //Get Private Variables
                HashSet<int> enabledIDList = (HashSet<int>)AccessTools.Field(typeof(TextureData), "enabledIDList").GetValue(null);
                Dictionary<int, TextureData.Data> basicDatas = (Dictionary<int, TextureData.Data>)AccessTools.Field(typeof(TextureData), "basicDatas").GetValue(null);

                //Add Textures from files
                string[] files = Directory.GetFiles($@"{bepinPath}\panel");
                for (int i = 0; i < files.Length; i++)
                {
                    //Get Object
                    texturePanelJSON = null;
                    using (StreamReader file = File.OpenText(files[i]))
                    {
                        JsonSerializer ser = new JsonSerializer();
                        texturePanelJSON = (MyRoomDataPlusTexJSON)ser.Deserialize(file, typeof(MyRoomDataPlusTexJSON));
                    }

                    if (texturePanelJSON != null)
                    {
                        //Enabled List
                        enabledIDList.Add(texturePanelJSON.id);

                        //Basic Data
                        TextureData.Data texData = new TextureData.Data(-1, new CsvParser());
                        basicDatas.Add(texturePanelJSON.id, texData);

                        //Resources
                        //UnityEngine.Resources.("SceneCreativeRoom/Debug/Textures/" + allDatas[index].resourceName);
                    }
                }

                //Set Private Variables
                AccessTools.Field(typeof(TextureData), "enabledIDList").SetValue(null, enabledIDList);
                AccessTools.Field(typeof(TextureData), "basicDatas").SetValue(null, basicDatas);

                //Reset
                TextureData_CreateData_Init = false;
            }
        }

        [HarmonyPatch(typeof(MyRoomCustom.TextureData.Data), MethodType.Constructor, new Type[] { typeof(int), typeof(CsvParser) })]
        [HarmonyPrefix()]
        public static bool MyRoomCustom_TextureData_Data_Constructor_Prefix(int uniqueID, MyRoomCustom.PlacementData.Data __instance)
        {
            if (uniqueID != -1)
            {
                return true;
            }

            //Verify custom JSON available
            if (texturePanelJSON != null)
            {
                //Set Data
                AccessTools.Field(typeof(MyRoomCustom.TextureData.Data), "ID").SetValue(__instance, texturePanelJSON.id);
                AccessTools.Field(typeof(MyRoomCustom.TextureData.Data), "drawName").SetValue(__instance, texturePanelJSON.name);
                AccessTools.Field(typeof(MyRoomCustom.TextureData.Data), "resourceName").SetValue(__instance, texturePanelJSON.file);

                StaticHelpers.Log($@"MyRoomCustom_TextureData_Data_Constructor_Prefix {texturePanelJSON.id}");

                return false;
            }

            return true;
        }
        #endregion

        #region CreativeRoom
        [HarmonyPatch(typeof(CreativeRoom), "LoadAllTextures", new Type[] { })]
        [HarmonyPrefix()]
        public static bool CreativeRoom_LoadAllTextures_Prefix()
        {
            //Get privates
            List<Material> m_Materials = (List<Material>)AccessTools.Field(typeof(CreativeRoom), "m_Materials").GetValue(null);

            if (m_Materials != null && m_Materials.Count > 0)
            {
                return false;
            }

            m_Materials = new List<Material>();
            List<TextureData.Data> allDatas = TextureData.GetAllDatas(true);
            Material source = new Material(Shader.Find("CM3D2/RoomPanel"));
            source.SetColor("_Color", Color.white);
            source.SetColor("_ShadowColor", Color.black);
            for (int index = 0; index < allDatas.Count; ++index)
            {
                Texture texture = UnityEngine.Resources.Load<Texture>("SceneCreativeRoom/Debug/Textures/" + allDatas[index].resourceName);
                if ((UnityEngine.Object)texture == (UnityEngine.Object)null)
                {
                    texture = ImportCM.CreateTexture(allDatas[index].resourceName + ".tex");
                    if (texture == null)
                    {
                        Debug.LogWarningFormat("テクスチャ「{0}」が見つからない", (object)allDatas[index].resourceName);
                    }
                    else
                    {
                        m_Materials.Add(new Material(source)
                        {
                            mainTexture = texture
                        });
                    }
                }
                else
                {
                    m_Materials.Add(new Material(source)
                    {
                        mainTexture = texture
                    });
                }
            }
            UnityEngine.Object.Destroy((UnityEngine.Object)source);

            //Set privates
            AccessTools.Field(typeof(CreativeRoom), "m_Materials").SetValue(null, m_Materials);

            return false;
        }
        #endregion
    }
}