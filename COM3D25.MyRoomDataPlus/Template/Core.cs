using BepInEx;
using COM3D2API;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2Template
{
    //[BepInPlugin("org.guest4168.plugins." + StaticHelpers.PluginNameTech, StaticHelpers.PluginName, "1.0.0.0")]
    internal abstract class Core<TManager, THooks> : BaseUnityPlugin 
        where TManager:Manager
        where THooks:Hooks<THooks>, new()
    {
        //Fields
        protected Helpers helper;
        protected UI ui;
        public Core(string iconBase64, UI ui, Helpers helper)
        {
            this.helper = helper;
            gearIconBase64 = iconBase64;
            this.ui = ui;
        }

        #region Unity/BepInEx
        private GameObject managerObject;

        public void Awake()
        {
            //Copied from examples
            helper.Log("Core Awake");
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)this);

            this.managerObject = new UnityEngine.GameObject(helper.PluginNameTech + "Manager");
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)this.managerObject);
            this.managerObject.AddComponent<TManager>().Initialize<THooks>(helper);

            SceneManager.sceneLoaded += SceneLoaded;
        }

        void LogHandler(string message, LogType type)
        {
            helper.Log(message);
        }
        #endregion

        #region Gear Menu
        private bool buttonAdded = false;
        private static string gearIconBase64;

        public void SceneLoaded(Scene s, LoadSceneMode lsm)
        {
            if (GameMain.Instance != null && s != null)
            {
                switch (s.name)
                {
                    case "SceneTitle":
                    {
                        //Add the gear menu button
                        if (GameMain.Instance.SysShortcut != null && !buttonAdded)
                        {
                            SystemShortcutAPI.AddButton(helper.PluginName, onMenuButtonClickCallback, "", Convert.FromBase64String(gearIconBase64));
                            buttonAdded = true;
                        }
                        break;
                    }
                }

                ui.ValidateActiveScene(s.name);
            }

            //Hide the UI when switching levels
            ui.Hide();
        }

        private void onMenuButtonClickCallback()
        {
            //Open/Close the UI
            ui.ToggleDisplay();
        }
        #endregion

        #region Plugin
        
        private void Update()
        {
            ui.Update();
        }
        private void OnGUI()
        {
            ui.OnGUI();
        }
        #endregion
    }
}