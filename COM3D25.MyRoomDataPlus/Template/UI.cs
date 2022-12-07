using System;
using UnityEngine;

namespace COM3D2Template
{
    internal abstract class UI
    {
        //Definition
        private string[] activeScenes = { }; //eg { "SceneTitle" }
        private string title;

        //State
        private bool activeScene;
        private bool displayUI;
        protected string info;

        //Visual
        protected static string[] monoFonts = new string[] { "Consolas", "Consolas Bold", "Courier New", "Courier New Bold", "Lucida Console" };
        private Rect uiWindow = new Rect(UnityEngine.Screen.width / 2 + 20, 20, 120, 50);
        private bool displayOptions = false;

        //Resize
        private Resize resize;
        private ResizeSlider resizeSlider;

        public UI(string title, string[] scenes)
        {
            activeScene = false;
            displayUI = false;
            info = "";
            resize = new Resize();
            resizeSlider = new ResizeSlider();

            this.title = title;
            activeScenes = scenes;
        }

        public void ValidateActiveScene(string name)
        {
            activeScene = (Array.IndexOf(activeScenes, name) != -1);
        }
        public void ToggleDisplay()
        {
            if (activeScene)
            {
                displayUI = !displayUI;
            }
        }
        public void Hide()
        {
            displayUI = false;
        }

        public static Texture2D MakeTex(Color col)
        {
            int xy = 12;
            {
                Texture2D result = new Texture2D(xy, xy);
                for (int i = 0; i < xy; ++i)
                {
                    for (int j = 0; j < xy; j++)
                    {
                        bool topLeft = (i == 0 && (j == 0 || j == 1)) || (j == 0 && (i == 0 || i == 1));
                        bool bottomLeft = (i == 0 && (j == (xy - 1) || j == (xy - 2))) || (j == (xy - 1) && (i == 0 || i == 1));
                        bool topRight = (i == (xy - 1) && (j == 0 || j == 1)) || (j == 0 && (i == (xy - 1) || i == (xy - 2)));
                        bool bottomRight = (i == (xy - 1) && (j == (xy - 1) || j == (xy - 2))) || (j == (xy - 1) && (i == (xy - 1) || i == (xy - 2)));

                        //Corner
                        if (topLeft || topRight || bottomLeft || bottomRight)
                        {
                            //result.SetPixels(i, j, 1, 1, new Color[] { new Color(0, 0, 0, 0) });
                            result.SetPixels(i, j, 1, 1, new Color[] { Color.black });
                        }
                        //Border
                        else if (i == 0 || j == 0 || i == (xy - 1) || j == (xy - 1) ||
                                (i == 1 && j == 1) || (i == (xy - 2) && j == 1) || (i == 1 && j == (xy - 2)) || (i == (xy - 2) && j == (xy - 2)))
                        {
                            result.SetPixels(i, j, 1, 1, new Color[] { Color.black });
                        }
                        //Normal
                        else
                        {
                            result.SetPixels(i, j, 1, 1, new Color[] { col });
                        }
                    }
                }

                result.Apply();
                return result;
            }
        }

        #region OnGUI
        public void OnGUI()
        {
            if (displayUI)
            {
                //Apply Skin
                GUISkin originalSkin = GUI.skin;
                GUI.skin = skinWindow;
                uiWindow = GUILayout.Window(41684, uiWindow, this.DisplayUIWindow, string.Empty);
                //Apply original skin
                GUI.skin = originalSkin;
            }
        }
        private void DisplayUIWindow(int windowId)
        {
            //Frame
            {
                GUILayout.BeginVertical();
                {
                    //Resize
                    GUI.skin = skinWindow;
                    {
                        //Option 1
                        if (this.resize.enabled)
                        {
                            this.resize.top.DisplayUIWindow();
                        }

                        //Option 2
                        if (this.resizeSlider.enabled)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                this.resizeSlider.left.DisplayUIWindow();
                                this.resizeSlider.right.DisplayUIWindow();
                            }
                            GUILayout.EndHorizontal();
                        }
                    }

                    GUILayout.BeginHorizontal();
                    {
                        //Resize
                        GUI.skin = skinWindow;
                        {
                            //Option 1
                            if (this.resize.enabled)
                            {
                                this.resize.left.DisplayUIWindow();
                            }

                            //Option 2
                            if (this.resizeSlider.enabled)
                            {
                                GUILayout.BeginVertical(GUILayout.Width(10), GUILayout.ExpandWidth(false));
                                {
                                    this.resizeSlider.top.DisplayUIWindow();
                                    this.resizeSlider.bottom.DisplayUIWindow();
                                }
                                GUILayout.EndVertical();
                            }
                        }

                        //Content
                        {
                            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                            {
                                //Header
                                GUI.skin = skinTitle;
                                GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                                {
                                    //Toolbar
                                    {
                                        GUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));
                                        {
                                            //Header
                                            GUILayout.Label(title, GUILayout.ExpandWidth(true));

                                            //Options
                                            if (GUILayout.Button(" * ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
                                            {
                                                displayOptions = !displayOptions;
                                            }
                                            if (GUILayout.Button(" X ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
                                            {
                                                this.displayUI = false;
                                            }
                                        }
                                        GUILayout.EndHorizontal();

                                        //Options
                                        if (displayOptions)
                                        {
                                            GUILayout.BeginVertical();
                                            {
                                                this.resize.enabled = GUILayout.Toggle(this.resize.enabled, " Resize Mode 1");
                                                GUI.enabled = false;
                                                this.resizeSlider.enabled = GUILayout.Toggle(this.resizeSlider.enabled, " Resize Mode 2");
                                                GUI.enabled = true;
                                            }
                                            GUILayout.EndVertical();
                                        }
                                    }
                                }
                                GUILayout.EndVertical();

                                //Content
                                GUI.skin = skinContent;
                                GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                                {
                                    //Abstract
                                    this.DisplayUIWindowBase();
                                }
                                GUILayout.EndVertical();

                                //Info
                                GUI.skin = skinInfo;
                                GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                                {
                                    GUILayout.BeginHorizontal();
                                    {
                                        GUILayout.Label("Info: " + info, GUILayout.ExpandWidth(true));
                                        if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
                                        {
                                            info = "";
                                        }
                                    }
                                    GUILayout.EndHorizontal();
                                }
                                GUILayout.EndVertical();
                            }
                            GUILayout.EndVertical();
                        }

                        //Resize
                        GUI.skin = skinWindow;
                        {
                            //Option 2
                            if (this.resizeSlider.enabled)
                            {
                                GUILayout.BeginVertical(GUILayout.Width(10), GUILayout.ExpandWidth(false));
                                {
                                    this.resizeSlider.top.DisplayUIWindow();
                                    this.resizeSlider.bottom.DisplayUIWindow();
                                }
                                GUILayout.EndVertical();
                            }

                            //Option 1
                            if (this.resize.enabled)
                            {
                                this.resize.right.DisplayUIWindow();
                            }
                        }
                    }
                    GUILayout.EndHorizontal();

                    //Resize
                    GUI.skin = skinWindow;
                    {
                        //Option 2
                        if (this.resizeSlider.enabled)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                this.resizeSlider.left.DisplayUIWindow();
                                this.resizeSlider.right.DisplayUIWindow();
                            }
                            GUILayout.EndHorizontal();
                        }

                        //Option 1
                        if (this.resize.enabled)
                        {
                            this.resize.bottom.DisplayUIWindow();
                        }
                    }
                }
                GUILayout.EndVertical();
            }

            //Make it draggable this must be last always
            GUI.DragWindow();
        }

        protected abstract void DisplayUIWindowBase();

        #region Skins
        private GUISkin _skinWindow;
        private GUISkin _skinTitle;
        private GUISkin _skinContent;
        private GUISkin _skinInfo;

        public GUISkin skinWindow
        {
            get
            {
                if (_skinWindow == null)
                {
                    //Copy Basic
                    GUISkin skin = newCore();

                    //Changes
                    skin.name = "GuestResizeSkin";
                    skin.font = Font.CreateDynamicFontFromOSFont(monoFonts[0], 12);
                    skin.box.normal.background = MakeTex(new Color(.176f, .176f, .176f));
                    skin.button.wordWrap = false;
                    skin.button.padding.top = 0;
                    skin.button.padding.bottom = 0;
                    skin.button.padding.left = 0;
                    skin.button.padding.right = 0;
                    skin.label.wordWrap = false;
                    skin.label.padding.top += 2;
                    skin.label.padding.bottom += 2;
                    skin.textField.wordWrap = false;
                    skin.textField.padding.top += 2;
                    skin.textField.padding.bottom += 2;
                    skin.toggle.wordWrap = false;
                    skin.window = skin.box;

                    _skinWindow = skin;
                }
                return _skinWindow;
            }
        }
        public GUISkin skinTitle
        {
            get
            {
                if (_skinTitle == null)
                {
                    //Copy Basic
                    GUISkin skin = newCore();

                    //Changes
                    skin.name = "GuestHeaderSkin";
                    skin.font = Font.CreateDynamicFontFromOSFont(monoFonts[1], 18);
                    skin.box.normal.background = MakeTex(new Color(.088f, .088f, .088f));
                    skin.box.margin.top = 0;
                    skin.box.margin.bottom = 0;
                    skin.button.wordWrap = false;
                    skin.button.padding.top += 2;
                    skin.button.padding.bottom += 2;
                    skin.button.padding.left += 2;
                    skin.button.padding.right += 2;
                    skin.label.wordWrap = false;
                    skin.label.padding.top += 2;
                    skin.label.padding.bottom += 2;
                    skin.textField.wordWrap = false;
                    skin.textField.padding.top += 2;
                    skin.textField.padding.bottom += 2;
                    skin.toggle.wordWrap = false;

                    _skinTitle = skin;
                }
                return _skinTitle;
            }
        }
        public GUISkin skinContent
        {
            get
            {
                if (_skinContent == null)
                {
                    //Copy Basic
                    GUISkin skin = newCore();

                    //Changes
                    skin.name = "GuestBaseSkin";
                    skin.font = Font.CreateDynamicFontFromOSFont(monoFonts[0], 14);
                    skin.box.normal.background = MakeTex(new Color(.176f, .176f, .176f));
                    skin.box.margin.top = 0;
                    skin.box.margin.bottom = 0;
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

                    _skinContent = skin;
                }
                return _skinContent;
            }
        }
        public GUISkin skinInfo
        {
            get
            {
                if (_skinInfo == null)
                {
                    //Copy Basic
                    GUISkin skin = newCore();

                    //Changes
                    skin.name = "GuestHeaderSkin";
                    skin.font = Font.CreateDynamicFontFromOSFont(monoFonts[0], 14);
                    skin.box.normal.background = MakeTex(new Color(.088f, .088f, .088f));
                    skin.box.margin.top = 0;
                    skin.button.wordWrap = false;
                    skin.button.padding.top += 2;
                    skin.button.padding.bottom += 2;
                    skin.button.padding.left += 2;
                    skin.button.padding.right += 2;
                    skin.label.wordWrap = false;
                    skin.label.padding.top += 2;
                    skin.label.padding.bottom += 2;
                    skin.textField.wordWrap = false;
                    skin.textField.padding.top += 2;
                    skin.textField.padding.bottom += 2;
                    skin.toggle.wordWrap = false;

                    _skinInfo = skin;
                }
                return _skinInfo;
            }
        }

        protected static GUISkin newCore()
        {
            GUISkin skin = new GUISkin();
            skin.box = new GUIStyle("box");
            skin.button = new GUIStyle("button");
            skin.horizontalScrollbar = new GUIStyle("horizontalScrollbar");
            skin.horizontalScrollbarLeftButton = new GUIStyle("horizontalScrollbarLeftButton");
            skin.horizontalScrollbarRightButton = new GUIStyle("horizontalScrollbarRightButton");
            skin.horizontalScrollbarThumb = new GUIStyle("horizontalScrollbarThumb");
            skin.horizontalSlider = new GUIStyle("horizontalSlider");
            skin.horizontalSliderThumb = new GUIStyle("horizontalSliderThumb");
            skin.label = new GUIStyle("label");
            skin.scrollView = new GUIStyle("scrollView");
            skin.textArea = new GUIStyle("textArea");
            skin.textField = new GUIStyle("textField");
            skin.toggle = new GUIStyle("toggle");
            skin.verticalScrollbar = new GUIStyle("verticalScrollbar");
            skin.verticalScrollbarDownButton = new GUIStyle("verticalScrollbarDownButton");
            skin.verticalScrollbarThumb = new GUIStyle("verticalScrollbarThumb");
            skin.verticalScrollbarUpButton = new GUIStyle("verticalScrollbarUpButton");
            skin.verticalSlider = new GUIStyle("verticalSlider");
            skin.verticalSliderThumb = new GUIStyle("verticalSliderThumb");
            skin.window = new GUIStyle("window");

            return skin;
        }
        #endregion

        #endregion

        #region Update
        public void Update()
        {
            if (displayUI)
            {
                this.UpdateBase();
                this.UpdateUIWindow();
            }
        }

        protected abstract void UpdateBase();

        private void UpdateUIWindow()
        {
            //Resize
            if (resize.enabled)
            {
                resize.Update(ref uiWindow);
            }
            if (resizeSlider.enabled)
            {
                resizeSlider.Update(ref uiWindow);
            }
        }
        #endregion

        #region Classes

        #region Resize
        protected class Resize
        {
            public bool enabled { get; set; }
            public ResizeData top { get; }
            public ResizeData bottom { get; }
            public ResizeData left { get; }
            public ResizeData right { get; }

            public Resize()
            {
                enabled = true;
                top = new ResizeData(ResizeDirection.Top);
                bottom = new ResizeData(ResizeDirection.Bottom);
                left = new ResizeData(ResizeDirection.Left);
                right = new ResizeData(ResizeDirection.Right);
            }

            public void Update(ref Rect uiWindow)
            {
                if (enabled)
                {
                    top.Update(ref uiWindow);
                    bottom.Update(ref uiWindow);
                    left.Update(ref uiWindow);
                    right.Update(ref uiWindow);
                }
            }

            public class ResizeData
            {
                private ResizeDirection direction;
                private float position;
                private bool resize;

                public ResizeData(ResizeDirection dir)
                {
                    direction = dir;
                    position = -9000.0f;
                    resize = false;
                }

                public void Update(ref Rect uiWindow)
                {
                    if(this.resize)
                    {
                        //Check mouse position
                        if (this.position == -9999.0f)
                        {
                            this.position = (this.direction == ResizeDirection.Top || this.direction == ResizeDirection.Bottom)? Input.mousePosition.y : Input.mousePosition.x;
                        }
                        else
                        {
                            //Resize window
                            if (this.direction == ResizeDirection.Top || this.direction == ResizeDirection.Bottom)
                            {
                                float change = Input.mousePosition.y - this.position;
                                uiWindow.height += (((this.direction == ResizeDirection.Top)? 1 : (-1)) * change);
                                if (this.direction == ResizeDirection.Top)
                                {
                                    uiWindow.y -= change;
                                }

                                this.position = Input.mousePosition.y;
                            }
                            else
                            {
                                float change = this.position - Input.mousePosition.x;
                                uiWindow.width += (((this.direction == ResizeDirection.Left) ? 1 : (-1)) * change);
                                if (this.direction == ResizeDirection.Left)
                                {
                                    uiWindow.x -= change;
                                }

                                this.position = Input.mousePosition.x;
                            }
                        }

                        //Check mouse released
                        if(Input.GetMouseButtonUp(0))
                        {
                            resize = false;
                        }
                    }
                    else if(this.position != -9999.0f)
                    {
                        //Reset
                        this.position = -9999.0f;
                    }
                }

                public void DisplayUIWindow()
                {
                    if (direction == ResizeDirection.Top || direction == ResizeDirection.Bottom)
                    {
                        if (GUILayout.RepeatButton("⇅", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false))) //⇅
                        {
                            this.resize = true;
                        }
                    }
                    else
                    {
                        if (GUILayout.RepeatButton("⇄", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(false))) //("⇄",
                        {
                            this.resize = true;
                        }
                    }
                }
            }
        }

        protected class ResizeSlider
        {
            public bool enabled { get; set; }
            public ResizeSliderData top { get; }
            public ResizeSliderData bottom { get; }
            public ResizeSliderData left { get; }
            public ResizeSliderData right { get; }

            public ResizeSlider()
            {
                enabled = false;
                top = new ResizeSliderData(ResizeDirection.Top);
                bottom = new ResizeSliderData(ResizeDirection.Bottom);
                left = new ResizeSliderData(ResizeDirection.Left);
                right = new ResizeSliderData(ResizeDirection.Right);
            }

            public void Update(ref Rect uiWindow)
            {
                if (enabled)
                {
                    top.Update(ref uiWindow);
                    bottom.Update(ref uiWindow);
                    left.Update(ref uiWindow);
                    right.Update(ref uiWindow);
                }
            }

            public class ResizeSliderData
            {
                private static float change = 5.0f;
                private static float max = 2.0f;
                private static float min = 0.0f;
                private static float size = 1.8f;

                private Rect uiWindow;
                private ResizeDirection direction;
                private readonly float mid;
                private bool resize;
                private float value;


                public ResizeSliderData(ResizeDirection dir)
                {
                    direction = dir;
                    mid = (max - size)/ 2;
                    value = 1.0f;
                    resize = false;
                }

                public void Update(ref Rect uiWindow)
                {
                    if (resize)
                    {
                        if (this.direction == ResizeDirection.Top || this.direction == ResizeDirection.Bottom)
                        {
                            //Drag top bar up or bottom bar down = make bigger
                            uiWindow.height += ((value < mid && this.direction == ResizeDirection.Top) || (this.direction == ResizeDirection.Bottom && value > mid)) ? (1) : (-1) * change;

                            //Drag left bar = move position of window
                            if (this.direction == ResizeDirection.Top)
                            {
                                uiWindow.y -= (value < mid) ? (1) : (-1) * change;
                            }
                        }
                        else
                        {
                            //Drag left bar left or right bar right = make bigger
                            uiWindow.width += ((value < mid && this.direction == ResizeDirection.Left) || (this.direction == ResizeDirection.Right && value > mid)) ? (1) : (-1) * change;

                            //Drag left bar = move position of window
                            if (this.direction == ResizeDirection.Left)
                            {
                                uiWindow.x -= (value < mid) ? (1) : (-1) * change;
                            }
                        }

                        //Reset to middle
                        value = mid;
                    }
                }

                public void DisplayUIWindow()
                {
                    if (direction == ResizeDirection.Top || direction == ResizeDirection.Bottom)
                    {
                        this.value = GUILayout.VerticalScrollbar(this.value, size, min, max, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(false));
                        if (GUI.changed)
                        {
                            this.resize = true;
                        }
                    }
                    else
                    {
                        this.value = GUILayout.HorizontalScrollbar(this.value, size, min, max, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                        if (GUI.changed)
                        {
                            this.resize = true;
                        }
                    }
                }
            }
        }

        protected enum ResizeDirection
        {
            Top = 0,
            Bottom = 1,
            Left = 2,
            Right = 3
        }
        #endregion

        #endregion
    }
}