using MyRoomCustom;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using UnityEngine;
using static COM3D25.MyRoomDataPlus.MyRoomDataPlusCore;
//using static COM3D25.MyRoomDataPlus.Core;

namespace COM3D25.MyRoomDataPlus
{
    internal class MyRoomDataPlusUI : COM3D2Template.UI
    {
        public static List<List<string>> categoryDataFiles = new List<List<string>>();
        public static List<List<string>> categoryDataTemp = new List<List<string>>();

        public static List<MyRoomDataPlusTexJSON> texDataTemp = new List<MyRoomDataPlusTexJSON>();
        public static List<MyRoomDataPlusItemJSON> objDataTemp = new List<MyRoomDataPlusItemJSON>();
        public static List<Tuple<int, string, string>> objCategoryDataTemp = new List<Tuple<int, string, string>>();
        public static List<MyRoomDataPlusBGJSON> bgDataTemp = new List<MyRoomDataPlusBGJSON>();

        private static Texture2D line;
        protected float lineHeight = 2;
        private static int page = 0;
        private static bool back = false;
        private static bool save = false;

        private static string tex_id = "5000";
        private static string tex_name = "";
        private static string tex_assetName = "";

        private static string obj_id = "50000";
        private static string obj_drawName = "";
        private static bool obj_category_new = false;
        private static string obj_category_new_id = "5000";
        private static string obj_category_new_name = "";
        private static int obj_category_index = 0;
        private static string obj_categoryName = "";
        private static string obj_assetName = "";
        private static string obj_thumbnailName = "";

        private static string bg_id = "50000";
        private static string bg_id_night = "50001";
        private static string bg_obj_id = "50000";
        private static string bg_obj_name = "";
        private static string bg_obj_assetNameDay = "";
        private static string bg_obj_assetNameNight = "";
        private static string bg_obj_thumbnailName = "";

        public MyRoomDataPlusUI(string title, string[] scenes) : base(title, scenes)
        { }

        protected override void UpdateBase()
        {
            //Basic
            {
                if (objCategoryDataTemp.Count == 0)
                {
                    //Add New Option
                    objCategoryDataTemp.Add(new Tuple<int, string, string>(-1, "NEW*", "NEW*"));

                    //Add Existing
                    for (int i = 0; i < PlacementData.CategoryIDList.Count; i++)
                    {
                        if (PlacementData.CategoryIDList[i] != 1100)
                        {
                            string name = PlacementData.GetCategoryName(PlacementData.CategoryIDList[i]);
                            string translation = name;

                            switch (name)
                            {
                                case "机":
                                {
                                    translation = "Tables";
                                    break;
                                }
                                case "椅子":
                                {
                                    translation = "Chairs";
                                    break;
                                }
                                case "壁":
                                {
                                    translation = "Walls";
                                    break;
                                }
                                case "扉":
                                {
                                    translation = "Doors";
                                    break;
                                }
                                case "家具":
                                {
                                    translation = "Furniture";
                                    break;
                                }
                                case "照明":
                                {
                                    translation = "Lighting";
                                    break;
                                }
                                case "小物":
                                {
                                    translation = "Accessories";
                                    break;
                                }
                                case "調理器具":
                                {
                                    translation = "Cookware";
                                    break;
                                }
                                case "敷物":
                                {
                                    translation = "Rugs";
                                    break;
                                }
                            }
                            objCategoryDataTemp.Add(new Tuple<int, string, string>(PlacementData.CategoryIDList[i], name, translation));
                        }
                    }
                }

                if (PhotoBGData.data == null)
                {
                    PhotoBGData.Create();
                }

                if(line == null)
                {
                    line = MakeTex(new Color(.8f, .8f, .8f));
                }
            }

            //UI
            {
                //Back button
                if(back)
                {
                    back = false;

                    page = 0;
                }

                //Save button
                if(save)
                {
                    save = false;

                    switch (page)
                    {
                        case 1:
                        {
                            Save_Texture();
                            break;
                        }
                        case 2:
                        {
                            Save_Object();
                            break;
                        }
                        case 3:
                        {
                            Save_BG();
                            break;
                        }
                    }
                }
            }
        }

        private void Save_Texture()
        {
            bool valid = true;
            MyRoomDataPlusTexJSON json = new MyRoomDataPlusTexJSON();

            //Validate Obj ID
            if (valid)
            {
                int idTex = 0;

                //Number
                if (valid)
                {
                    try
                    {
                        idTex = Int32.Parse(tex_id);
                    }
                    catch (Exception ex)
                    {
                        info = "ERROR: ID (Invalid Number)";
                        StaticHelpers.Log(info);
                        valid = false;
                    }
                }

                //Id not taken
                if (valid)
                {
                    //Check Game Data
                    if (TextureData.Contains(idTex))
                    {
                        info = "ERROR: ID (Id already used)";
                        StaticHelpers.Log(info);
                        valid = false;
                    }

                    //Check Temp Data
                    for (int i = 0; i < texDataTemp.Count; i++)
                    {
                        if (texDataTemp[i].id.ToString().Equals(idTex.ToString()))
                        {
                            info = "ERROR: ID (Id already used)";
                            StaticHelpers.Log(info);
                            valid = false;
                            break;
                        }
                    }
                }

                //Set
                if (valid)
                {
                    json.id = idTex;
                }
            }

            //Validate Name
            if (valid)
            {
                //Not empty
                if (valid)
                {
                    if (String.IsNullOrEmpty(tex_name))
                    {
                        info = "ERROR: Name (Name cannot be empty)";
                        StaticHelpers.Log(info);
                        valid = false;
                    }
                }

                //Set
                if (valid)
                {
                    json.name = tex_name;
                }
            }

            //Validate Asset
            if (valid)
            {
                //Not empty
                if (valid)
                {
                    if (String.IsNullOrEmpty(tex_assetName))
                    {
                        info = "ERROR: Asset (Asset cannot be empty)";
                        StaticHelpers.Log(info);
                        valid = false;
                    }
                }

                //Set
                if (valid)
                {
                    json.file = tex_assetName;
                }
            }

            //Create file
            if (valid)
            {
                File.WriteAllText($@"{bepinPath}\panel\Panel_{json.file}.json", JsonConvert.SerializeObject(json));
                texDataTemp.Add(json);
                info = $"SUCCESS: Panel Texture Data Created (Panel_{json.file}.json)";
                StaticHelpers.Log(info);
            }
        }
        private void Save_Object()
        {
            bool valid = true;
            MyRoomDataPlusItemJSON json = new MyRoomDataPlusItemJSON();

            //Validate Obj ID
            if (valid)
            {
                int idObject = 0;

                //Number
                if (valid)
                {
                    try
                    {
                        idObject = Int32.Parse(obj_id);
                    }
                    catch (Exception ex)
                    {
                        info = "ERROR: Obj ID (Invalid Number)";
                        StaticHelpers.Log(info);
                        valid = false;
                    }
                }

                //Id not taken
                if (valid)
                {
                    //Check Game Data
                    if (PlacementData.Contains(idObject))
                    {
                        info = "ERROR: Obj ID (Id already used)";
                        StaticHelpers.Log(info);
                        valid = false;
                    }

                    //Check Temp Data
                    for (int i = 0; i < objDataTemp.Count; i++)
                    {
                        if (objDataTemp[i].idObject.ToString().Equals(idObject.ToString()))
                        {
                            info = "ERROR: Obj ID (Id already used)";
                            StaticHelpers.Log(info);
                            valid = false;
                            break;
                        }
                    }
                    for (int i = 0; i < bgDataTemp.Count; i++)
                    {
                        if (bgDataTemp[i].idObject.ToString().Equals(idObject.ToString()))
                        {
                            info = "ERROR: Obj ID (Id already used)";
                            StaticHelpers.Log(info);
                            valid = false;
                            break;
                        }
                    }
                }

                //Set
                if (valid)
                {
                    json.idObject = idObject;
                }
            }

            //Validate Name
            if (valid)
            {
                //Not empty
                if (valid)
                {
                    if (String.IsNullOrEmpty(obj_drawName))
                    {
                        info = "ERROR: Name (Name cannot be empty)";
                        StaticHelpers.Log(info);
                        valid = false;
                    }
                }

                //Set
                if (valid)
                {
                    json.nameObject = obj_drawName;
                }
            }

            //Validate Category
            if (valid)
            {
                //New
                if (obj_category_new)
                {
                    //Id
                    {
                        int idCategory = 0;

                        //Number
                        if (valid)
                        {
                            try
                            {
                                idCategory = Int32.Parse(obj_category_new_id);
                            }
                            catch (Exception ex)
                            {
                                info = "ERROR: New Category ID (Invalid Number)";
                                StaticHelpers.Log(info);
                                valid = false;
                            }
                        }

                        //Id not taken
                        if (valid)
                        {
                            //Check Game Data
                            if (PlacementData.CategoryIDList.Contains(idCategory))
                            {
                                info = "ERROR: New Category ID (Id already used)";
                                StaticHelpers.Log(info);
                                valid = false;
                            }

                            //Check Temp Data
                            for (int i = 0; i < objDataTemp.Count; i++)
                            {
                                if (objDataTemp[i].idCategory.ToString().Equals(idCategory.ToString()))
                                {
                                    info = "ERROR: New Category ID (Id already used)";
                                    StaticHelpers.Log(info);
                                    valid = false;
                                    break;
                                }
                            }
                        }

                        //Id -1
                        if (valid)
                        {
                            if (idCategory == -1)
                            {
                                info = "ERROR: New Category ID (Id cannot be -1)";
                                StaticHelpers.Log(info);
                                valid = false;
                            }
                        }

                        //Set
                        if (valid)
                        {
                            json.idCategory = idCategory;
                        }
                    }

                    //Name
                    {
                        //Not empty
                        if (valid)
                        {
                            if (String.IsNullOrEmpty(obj_category_new_name))
                            {
                                info = "ERROR: New Category Name (Name cannot be empty)";
                                StaticHelpers.Log(info);
                                valid = false;
                            }
                        }

                        //Name not taken
                        if (valid)
                        {
                            for (int i = 0; i < objCategoryDataTemp.Count; i++)
                            {
                                if (objCategoryDataTemp[i].Item2.Equals(obj_category_new_name))
                                {
                                    info = "ERROR: New Category Name (Name already used)";
                                    StaticHelpers.Log(info);
                                    valid = false;
                                    break;
                                }
                            }
                        }

                        //Set
                        if (valid)
                        {
                            json.nameCategory = obj_category_new_name;
                        }
                    }
                }
                //Existing
                else
                {
                    //Set
                    if (valid)
                    {
                        json.idCategory = objCategoryDataTemp[obj_category_index].Item1;
                        json.nameCategory = objCategoryDataTemp[obj_category_index].Item2;
                    }
                }
            }

            //Validate Asset
            if (valid)
            {
                //Not empty
                if (valid)
                {
                    if (String.IsNullOrEmpty(obj_assetName))
                    {
                        info = "ERROR: Asset (Asset cannot be empty)";
                        StaticHelpers.Log(info);
                        valid = false;
                    }
                }

                //Set
                if (valid)
                {
                    json.fileObject = obj_assetName;
                }
            }

            //Validate thumbnail
            if (valid)
            {
                //Not empty
                if (valid)
                {
                    if (String.IsNullOrEmpty(obj_thumbnailName))
                    {
                        info = "ERROR: Thumbnail (Thumbnail cannot be empty)";
                        StaticHelpers.Log(info);
                        valid = false;
                    }
                }

                //Set
                if (valid)
                {
                    json.thumbnailObject = obj_thumbnailName;
                }
            }

            //Create file
            if (valid)
            {
                File.WriteAllText($@"{bepinPath}\item\Item_{json.fileObject}.json", JsonConvert.SerializeObject(json));
                objDataTemp.Add(json);
                if (obj_category_new)
                {
                    objCategoryDataTemp.Add(new Tuple<int, string, string>(json.idCategory, json.nameCategory, json.nameCategory));
                }
                info = $"SUCCESS: MyRoom Data Created (Item_{json.fileObject}.json)";
                StaticHelpers.Log(info);
            }
        }
        private void Save_BG()
        {
            bool valid = true;
            MyRoomDataPlusBGJSON json = new MyRoomDataPlusBGJSON();

            //Validate BG ID
            if (valid)
            {
                int idBG = 0;

                //Number
                if (valid)
                {
                    try
                    {
                        idBG = Int32.Parse(bg_id);
                    }
                    catch (Exception ex)
                    {
                        info = "ERROR: BG ID (Invalid Number)";
                        StaticHelpers.Log(info);
                        valid = false;
                    }
                }

                //Id not taken
                if (valid)
                {
                    //Check Game Data
                    for (int i = 0; i < PhotoBGData.data.Count; i++)
                    {
                        PhotoBGData photoBGData = PhotoBGData.data[i];
                        if (photoBGData.id.Equals(idBG.ToString()))
                        {
                            info = "ERROR: BG ID (Id already used)";
                            StaticHelpers.Log(info);
                            valid = false;
                            break;
                        }
                    }

                    //Check Temp Data
                    for (int i = 0; i < bgDataTemp.Count; i++)
                    {
                        if (bgDataTemp[i].idBG.ToString().Equals(idBG.ToString()))
                        {
                            info = "ERROR: BG ID (Id already used)";
                            StaticHelpers.Log(info);
                            valid = false;
                            break;
                        }
                    }
                }

                //Set
                if (valid)
                {
                    json.idBG = idBG;
                }
            }

            //Validate Obj ID
            if (valid)
            {
                int idObject = 0;

                //Number
                if (valid)
                {
                    try
                    {
                        idObject = Int32.Parse(bg_obj_id);
                    }
                    catch (Exception ex)
                    {
                        info = "ERROR: Room Obj ID (Invalid Number)";
                        StaticHelpers.Log(info);
                        valid = false;
                    }
                }

                //Id not taken
                if (valid)
                {
                    //Check Game Data
                    if (PlacementData.Contains(idObject))
                    {
                        info = "ERROR: Room Obj ID (Id already used)";
                        StaticHelpers.Log(info);
                        valid = false;
                    }

                    //Check Temp Data
                    for (int i = 0; i < bgDataTemp.Count; i++)
                    {
                        if (bgDataTemp[i].idObject.ToString().Equals(idObject.ToString()))
                        {
                            info = "ERROR: Room Obj ID (Id already used BG)";
                            StaticHelpers.Log(info);
                            valid = false;
                            break;
                        }
                    }
                    for (int i = 0; i < objDataTemp.Count; i++)
                    {
                        if (objDataTemp[i].idObject.ToString().Equals(idObject.ToString()))
                        {
                            info = "ERROR: Room Obj ID (Id already used Item)";
                            StaticHelpers.Log(info);
                            valid = false;
                            break;
                        }
                    }
                }

                //Set
                if (valid)
                {
                    json.idObject = idObject;
                }
            }

            //Validate Name
            if (valid)
            {
                //Not empty
                if (valid)
                {
                    if (String.IsNullOrEmpty(bg_obj_name))
                    {
                        info = "ERROR: Name (Name cannot be empty)";
                        StaticHelpers.Log(info);
                        valid = false;
                    }
                }

                //Set
                if (valid)
                {
                    json.nameObject = bg_obj_name;
                }
            }

            //Validate file
            if (valid)
            {
                //Either day or night not empty
                if (valid)
                {
                    if (String.IsNullOrEmpty(bg_obj_assetNameDay) && String.IsNullOrEmpty(bg_obj_assetNameNight))
                    {
                        info = "ERROR: Asset File (Day & Night cannot BOTH be empty)";
                        StaticHelpers.Log(info);
                        valid = false;
                    }
                }

                //Set
                if (valid)
                {
                    json.fileDayObject = String.IsNullOrEmpty(bg_obj_assetNameDay) ? "" : bg_obj_assetNameDay;
                    json.fileNightObject = String.IsNullOrEmpty(bg_obj_assetNameNight) ? "" : bg_obj_assetNameNight;
                }
            }

            //Validate thumbnail
            if (valid)
            {
                //Not empty
                if (valid)
                {
                    if (String.IsNullOrEmpty(bg_obj_thumbnailName))
                    {
                        info = "ERROR: Thumbnail (Thumbnail cannot be empty)";
                        StaticHelpers.Log(info);
                        valid = false;
                    }
                }

                //Set
                if (valid)
                {
                    json.thumbnailObject = bg_obj_thumbnailName;
                }
            }

            //Create file
            if (valid)
            {
                string fileName = string.IsNullOrEmpty(json.fileDayObject) ? json.fileNightObject : json.fileDayObject;
                File.WriteAllText($@"{bepinPath}\bg\RoomObj_{fileName}.json", JsonConvert.SerializeObject(json));
                bgDataTemp.Add(json);
                info = $"SUCCESS: BG Data Created (RoomObj_{fileName}.json)";
                StaticHelpers.Log(info);
            }
        }

        protected override void DisplayUIWindowBase()
        {
            GUILayout.BeginVertical();
            {
                switch (page)
                {
                    case 0:
                    {
                        //Menu

                        //Header
                        {
                            GUI.skin = skinHeader;
                            GUILayout.Label("MENU", GUILayout.MinWidth(400.0f));
                            GUI.skin = skinLine;
                            GUILayout.Box(line, GUILayout.MaxHeight(lineHeight), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                            GUI.skin = skinContent;
                        }

                        //Form
                        {
                            if (GUILayout.Button("Add Texture")) { page = 1; }
                            if (GUILayout.Button("Add Room Object")) { page = 2; }
                            if (GUILayout.Button("Add Room BG")) { page = 3; }
                        }

                        //Space
                        GUILayout.FlexibleSpace();
                        break;
                    }
                    case 1:
                    {
                        //Textures

                        //Header
                        {
                            GUI.skin = skinHeader;
                            GUILayout.Label("PANEL TEXTURE", GUILayout.MinWidth(400.0f));
                            GUI.skin = skinLine;
                            GUILayout.Box(line, GUILayout.MaxHeight(lineHeight), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                            GUI.skin = skinContent;
                        }

                        //Form
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(" ID:      ", GUILayout.ExpandWidth(false));
                                tex_id = GUILayout.TextField(tex_id, GUILayout.ExpandWidth(true));
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(" Name:    ", GUILayout.ExpandWidth(false));
                                tex_name = GUILayout.TextField(tex_name, GUILayout.ExpandWidth(true));
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(" Asset:   ", GUILayout.ExpandWidth(false));
                                tex_assetName = GUILayout.TextField(tex_assetName, GUILayout.ExpandWidth(true));
                                GUILayout.Label(".tex", GUILayout.ExpandWidth(false));
                                if (GUILayout.Button(" Pick ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
                                {
                                    string dialogResult = PickFileName("Tex files (*.tex)|*.tex");
                                    if (!string.IsNullOrEmpty(dialogResult))
                                    {
                                        tex_assetName = dialogResult;
                                    }
                                }
                            }
                            GUILayout.EndHorizontal();
                        }

                        goto default;
                    }
                    case 2:
                    {
                        //Objects

                        //Header
                        {
                            GUI.skin = skinHeader;
                            GUILayout.Label("ROOM OBJECT", GUILayout.MinWidth(400.0f));
                            GUI.skin = skinLine;
                            GUILayout.Box(line, GUILayout.MaxHeight(lineHeight), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                            GUI.skin = skinContent;
                        }

                        //Form
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(" Obj ID:      ", GUILayout.ExpandWidth(false));
                                obj_id = GUILayout.TextField(obj_id, GUILayout.ExpandWidth(true));
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(" Name:        ", GUILayout.ExpandWidth(false));
                                obj_drawName = GUILayout.TextField(obj_drawName, GUILayout.ExpandWidth(true));
                            }
                            GUILayout.EndHorizontal();

                            //OLD
                            {
                                //GUILayout.BeginVertical("box");
                                //{
                                //    GUILayout.Label("Category");
                                //    obj_categoryName = (objCategoryDataTemp.Count == 0) ? "" : objCategoryDataTemp[obj_category_index].Item3;

                                //    //Option
                                //    obj_category_new = GUILayout.Toggle(obj_category_new, " New Category?", GUILayout.ExpandWidth(true));

                                //    //New
                                //    {
                                //        GUI.enabled = obj_category_new;
                                //        {
                                //            GUILayout.BeginHorizontal();
                                //            {
                                //                GUILayout.Label("New ID:     ", GUILayout.ExpandWidth(false));
                                //                obj_category_new_id = GUILayout.TextField(obj_category_new_id, GUILayout.ExpandWidth(true));
                                //            }
                                //            GUILayout.EndHorizontal();
                                //            GUILayout.BeginHorizontal();
                                //            {
                                //                GUILayout.Label("New Name:   ", GUILayout.ExpandWidth(false));
                                //                obj_category_new_name = GUILayout.TextField(obj_category_new_name, GUILayout.ExpandWidth(true));
                                //            }
                                //            GUILayout.EndHorizontal();
                                //        }
                                //    }
                                //    //Existing
                                //    {
                                //        GUI.enabled = !obj_category_new;
                                //        {
                                //            GUILayout.BeginHorizontal();
                                //            if (GUILayout.Button(" < ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
                                //            {
                                //                obj_category_index -= 1;
                                //                if (obj_category_index < 0)
                                //                {
                                //                    obj_category_index = objCategoryDataTemp.Count - 1;
                                //                }
                                //            }
                                //            GUILayout.Label(obj_categoryName, GUILayout.ExpandWidth(true));
                                //            if (GUILayout.Button(" > ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
                                //            {
                                //                obj_category_index += 1;
                                //                if (obj_category_index >= objCategoryDataTemp.Count)
                                //                {
                                //                    obj_category_index = 0;
                                //                }
                                //            }
                                //            GUILayout.EndHorizontal();
                                //        }
                                //    }

                                //    GUI.enabled = true;
                                //}
                                //GUILayout.EndVertical();
                            }
                            //NEW
                            {
                                obj_categoryName = (objCategoryDataTemp.Count == 0) ? "NEW*" : objCategoryDataTemp[obj_category_index].Item3;

                                //Selection
                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Label(" Category:    ", GUILayout.ExpandWidth(false));
                                    if (GUILayout.Button(" < ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
                                    {
                                        obj_category_index -= 1;
                                        if (obj_category_index < 0)
                                        {
                                            obj_category_index = objCategoryDataTemp.Count - 1;
                                        }
                                    }
                                    GUILayout.Label(obj_categoryName, GUILayout.ExpandWidth(true));
                                    if (GUILayout.Button(" > ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
                                    {
                                        obj_category_index += 1;
                                        if (obj_category_index >= objCategoryDataTemp.Count)
                                        {
                                            obj_category_index = 0;
                                        }
                                    }
                                }
                                GUILayout.EndHorizontal();

                                //New Category
                                {
                                    obj_category_new = obj_categoryName.Equals("NEW*") || obj_category_index == 0;
                                    if(obj_category_new)
                                    {
                                        GUILayout.BeginHorizontal();
                                        {
                                            GUILayout.Label("    New ID:   ", GUILayout.ExpandWidth(false));
                                            obj_category_new_id = GUILayout.TextField(obj_category_new_id, GUILayout.ExpandWidth(true));
                                        }
                                        GUILayout.EndHorizontal();
                                        GUILayout.BeginHorizontal();
                                        {
                                            GUILayout.Label("    New Name: ", GUILayout.ExpandWidth(false));
                                            obj_category_new_name = GUILayout.TextField(obj_category_new_name, GUILayout.ExpandWidth(true));
                                        }
                                        GUILayout.EndHorizontal();
                                    }
                                    else
                                    {
                                        obj_category_new_id = "";
                                        obj_category_new_name = "";
                                    }
                                }
                            }
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(" Asset:       ", GUILayout.ExpandWidth(false));
                                obj_assetName = GUILayout.TextField(obj_assetName, GUILayout.ExpandWidth(true));
                                GUILayout.Label(".asset_bg", GUILayout.ExpandWidth(false));
                                if (GUILayout.Button(" Pick ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
                                {
                                    string dialogResult = PickFileName("Asset BG files (*.asset_bg)|*.asset_bg");
                                    if (!string.IsNullOrEmpty(dialogResult))
                                    {
                                        obj_assetName = dialogResult;
                                    }
                                }
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(" Thumbnail:   ", GUILayout.ExpandWidth(false));
                                obj_thumbnailName = GUILayout.TextField(obj_thumbnailName, GUILayout.ExpandWidth(true));
                                GUILayout.Label(".tex     ", GUILayout.ExpandWidth(false));
                                if (GUILayout.Button(" Pick ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
                                {
                                    string dialogResult = PickFileName("Tex files (*.tex)|*.tex");
                                    if (!string.IsNullOrEmpty(dialogResult))
                                    {
                                        obj_thumbnailName = dialogResult;
                                    }
                                }
                            }
                            GUILayout.EndHorizontal();
                        }

                        goto default;
                    }
                    case 3:
                    {
                        //BGs

                        //Header
                        {
                            GUI.skin = skinHeader;
                            GUILayout.Label("ROOM BG", GUILayout.MinWidth(400.0f));
                            GUI.skin = skinLine;
                            GUILayout.Box(line, GUILayout.MaxHeight(lineHeight), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                            GUI.skin = skinContent;
                        }

                        //Form
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(" BG ID:           ", GUILayout.ExpandWidth(false));
                                bg_id = GUILayout.TextField(bg_id, GUILayout.ExpandWidth(true));
                                try
                                {
                                    bg_id_night = (Int32.Parse(bg_id) + 1).ToString();
                                }
                                catch (Exception ex)
                                {
                                    bg_id = "50000";
                                    bg_id_night = "50001";
                                    StaticHelpers.Log("Conversion error BG ID");
                                }
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(" BG ID (Night):   ", GUILayout.ExpandWidth(false));
                                GUI.enabled = false;
                                GUILayout.TextField(bg_id_night, GUILayout.ExpandWidth(true));
                                GUI.enabled = true;
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(" Room Obj ID:     ", GUILayout.ExpandWidth(false));
                                bg_obj_id = GUILayout.TextField(bg_obj_id, GUILayout.ExpandWidth(true));
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(" Name:            ", GUILayout.ExpandWidth(false));
                                bg_obj_name = GUILayout.TextField(bg_obj_name, GUILayout.ExpandWidth(true));
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(" BG Day:      ", GUILayout.ExpandWidth(false));
                                bg_obj_assetNameDay = GUILayout.TextField(bg_obj_assetNameDay, GUILayout.ExpandWidth(true));
                                GUILayout.Label(".asset_bg", GUILayout.ExpandWidth(false));
                                if (GUILayout.Button(" Pick ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
                                {
                                    string dialogResult = PickFileName("Asset BG files (*.asset_bg)|*.asset_bg");
                                    if (!string.IsNullOrEmpty(dialogResult))
                                    {
                                        bg_obj_assetNameDay = dialogResult;
                                    }
                                }
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(" BG Night:    ", GUILayout.ExpandWidth(false));
                                bg_obj_assetNameNight = GUILayout.TextField(bg_obj_assetNameNight, GUILayout.ExpandWidth(true));
                                GUILayout.Label(".asset_bg", GUILayout.ExpandWidth(false));
                                if (GUILayout.Button(" Pick ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
                                {
                                    string dialogResult = PickFileName("Asset BG files (*.asset_bg)|*.asset_bg");
                                    if (!string.IsNullOrEmpty(dialogResult))
                                    {
                                        bg_obj_assetNameNight = dialogResult;
                                    }
                                }
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(" Thumbnail:   ", GUILayout.ExpandWidth(false));
                                bg_obj_thumbnailName = GUILayout.TextField(bg_obj_thumbnailName, GUILayout.ExpandWidth(true));
                                GUILayout.Label(".tex     ", GUILayout.ExpandWidth(false));
                                if (GUILayout.Button(" Pick ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
                                {
                                    string dialogResult = PickFileName("Tex files (*.tex)|*.tex");
                                    if (!string.IsNullOrEmpty(dialogResult))
                                    {
                                        bg_obj_thumbnailName = dialogResult;
                                    }
                                }
                            }
                            GUILayout.EndHorizontal();
                        }

                        goto default;
                    }
                    default:
                    {
                        //Space
                        GUILayout.FlexibleSpace();

                        //Buttons
                        {
                            GUILayout.Space(10);
                            GUILayout.BeginHorizontal();
                            {
                                //Back
                                if (GUILayout.Button("Back"))
                                {
                                    back = true;
                                }

                                //Save
                                if (GUILayout.Button("Save"))
                                {
                                    save = true;
                                }
                            }
                            GUILayout.EndHorizontal();
                        }

                        break;
                    }
                }
            }
            GUILayout.EndVertical();
        }

        private static string PickFileName(string extension)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = $@"{UTY.gameProjectPath}\Mod";
                openFileDialog.Filter = extension; //"txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    return System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                }
            }

            return null;
        }

        #region Skins
        private GUISkin _skinHeader;
        private GUISkin _skinLine;
        public GUISkin skinHeader
        {
            get
            {
                if (_skinHeader == null)
                {
                    //Copy Basic
                    GUISkin skin = newCore();

                    //Changes
                    skin.name = "GuestBaseSkin";
                    skin.font = Font.CreateDynamicFontFromOSFont(monoFonts[1], 16);
                    skin.box.normal.background = MakeTex(new Color(.176f, .176f, .176f));
                    skin.box.margin.top = 0;
                    skin.box.margin.bottom = 0;
                    skin.button.wordWrap = false;
                    skin.button.padding.top += 2;
                    skin.button.padding.bottom += 2;
                    skin.label.wordWrap = false;
                    skin.label.padding.top += 2;
                    skin.label.padding.bottom = 8;
                    skin.label.padding.left = 4;
                    skin.label.padding.right = 4;
                    skin.label.margin.bottom = 0;
                    skin.textField.wordWrap = false;
                    skin.textField.padding.top += 2;
                    skin.textField.padding.bottom += 2;
                    skin.toggle.wordWrap = false;

                    _skinHeader = skin;
                }
                return _skinHeader;
            }
        }
        protected GUISkin skinLine
        {
            get
            {
                if (_skinLine == null)
                {
                    //Copy Basic
                    GUISkin skin = newCore();

                    //Changes
                    skin.name = "GuestBaseSkin";
                    skin.font = Font.CreateDynamicFontFromOSFont(monoFonts[0], 14);
                    skin.box.normal.background = Texture2D.whiteTexture;
                    skin.box.margin.top = 0;
                    skin.box.margin.bottom = 0;
                    skin.box.border.left = 1;
                    skin.box.border.right = 1;
                    skin.box.border.top = 1;
                    skin.box.border.bottom = 1;
                    skin.button.wordWrap = false;
                    skin.button.padding.top += 2;
                    skin.button.padding.bottom += 2;
                    skin.label.wordWrap = false;
                    skin.label.padding.top += 2;
                    skin.label.padding.bottom += 2;
                    skin.textField.wordWrap = false;
                    skin.textField.padding.top += 2;
                    skin.textField.padding.bottom += 2;
                    skin.toggle.wordWrap = false;

                    _skinLine = skin;
                }
                return _skinLine;
            }
        }
        #endregion
    }
}