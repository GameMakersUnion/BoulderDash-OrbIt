using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TomShane.Neoforce.Controls;
using System.Reflection;
using System.CodeDom;
using System.Runtime.Serialization;
using System.Drawing;
using SColor = System.Drawing.Color;
using Color = Microsoft.Xna.Framework.Color;
using Component = OrbItProcs.Component;
using Console = System.Console;
using sc = System.Console;

using System.IO;
using System.Collections.ObjectModel;
using Polenter.Serialization;
using System.Threading;

namespace OrbItProcs
{

    public enum resolutions
    {
        AutoFullScreen,

        VGA_640x480,
        SVGA_800x600,
        XGA_1024x768,
        HD_1366x768,
        WXGA_1280x800,
        SXGA_1280x1024,
        WSXGA_1680x1050,
        FHD_1920x1080,
    }

    public class OrbIt : Application
    {
        public static OrbIt game;
        public static UserInterface ui;
        public static GameTime gametime;
        public static bool soundEnabled = false;
        public static bool isFullScreen = false;
        private static bool GraphicsReset = false;
        public static int ScreenWidth { get { return game.Graphics.PreferredBackBufferWidth; } set { game.Graphics.PreferredBackBufferWidth = value; } }
        public static int ScreenHeight { get { return game.Graphics.PreferredBackBufferHeight; } set { game.Graphics.PreferredBackBufferHeight = value; } }

        public static GlobalGameMode globalGameMode { get; set; }

        public SharpSerializer serializer = new SharpSerializer();
        public Room room
        {
            get { return _activeRoom; }
            set
            {
                if (value == null) throw new SystemException("Room was null when reseting room references");
                _activeRoom = value;
                if (ui != null) ui.sidebar.UpdateGroupComboBoxes();
            }
        }
        private Room _activeRoom;
        public Room mainRoom;
        //public Room tempRoom;
        
        public FrameRateCounter frameRateCounter;
        public resolutions? preferredFullScreen;
        
        public bool IsOldUI { get { return ui != null && ui.sidebar != null && ui.sidebar.activeTabControl == ui.sidebar.tbcMain; } }
        
        
        public static Action OnUpdate;
        public static bool updateTemp = false;

        private OrbIt() : base(true)
        {
            game = this;
            Content.RootDirectory = "Content";
            Graphics.SynchronizeWithVerticalRetrace = true;
            ExitConfirmation = false;
            Manager.Input.InputMethods = InputMethods.Mouse | InputMethods.Keyboard;
            Manager.AutoCreateRenderTarget = false;
            Graphics.PreferMultiSampling = false;
            SystemBorder = false;
        }

        public void setResolution(resolutions r, bool fullScreen, bool resizeRoom = false)
        {
            switch (r)
            {
                case resolutions.AutoFullScreen:
                    ScreenWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
                    ScreenHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
                    break;
                case resolutions.FHD_1920x1080:
                    ScreenWidth = 1920; ScreenHeight = 1080; break;
                case resolutions.HD_1366x768:
                    ScreenWidth = 1366; ScreenHeight = 768; break;
                case resolutions.SVGA_800x600:
                    ScreenWidth = 800; ScreenHeight = 600; break;
                case resolutions.SXGA_1280x1024:
                    ScreenWidth = 1280; ScreenHeight = 1024; break;
                case resolutions.VGA_640x480:
                    ScreenWidth = 640; ScreenHeight = 480; break;
                case resolutions.WSXGA_1680x1050:
                    ScreenWidth = 1680; ScreenHeight = 1050; break;
                case resolutions.WXGA_1280x800:
                    ScreenWidth = 1280; ScreenHeight = 800; break;
                case resolutions.XGA_1024x768:
                    ScreenWidth = 1024; ScreenHeight = 768; break;
            }
            Manager.Graphics.IsFullScreen = fullScreen;
            GraphicsReset = true;
        }
        protected override void Initialize()
        {
            //Load Stuff.
            Assets.LoadAssets(Content);
            //Sup Tom.
            base.Initialize();
            //Get Roomy
            mainRoom = new Room(this, ScreenWidth, ScreenHeight-40);//*8);//*8); //change to height
            room = mainRoom;
            //Hi-Definition Orbs:
            setResolution(resolutions.HD_1366x768, false);//(resolutions.HD_1366x768, false);
            //A game need players, no?
            Player.CreatePlayers(mainRoom);
            //UI
            ui = UserInterface.Start();
            globalGameMode = new GlobalGameMode(this);
            ui.Initialize();

            //The only important stat in OrbIt.
            frameRateCounter = new FrameRateCounter(this);

            mainRoom.attatchToSidbar();

            //Keybinds
            ui.keyManager.addProcessKeyAction("exitgame", KeyCodes.Escape, OnPress: () => Exit());
            ui.keyManager.addProcessKeyAction("togglesidebar", KeyCodes.OemTilde, OnPress: ui.ToggleSidebar);
            ui.keyManager.addProcessKeyAction("switchview", KeyCodes.PageDown, OnPress: ui.SwitchView);
            ui.keyManager.addProcessKeyAction("removeall", KeyCodes.Delete, OnPress: () => ui.sidebar.btnRemoveAllNodes_Click(null, null));

            MainWindow.TransparentClientArea = true;
            //ui.ToggleSidebar();
            //Testing.sawtoothTest();

            //LoadLevelWindow.StaticLevel("Test.xml");
        }
        public resolutions preferredWindowed;

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            gametime = gameTime;
            frameRateCounter.Update(gameTime);
            if (IsActive) ui.Update(gameTime);

            if (!ui.IsPaused)
            {
                if (mainRoom != null) mainRoom.Update(gameTime);
            }
            else
            {
                room.camera.RenderAsync();
                room.Draw();
                room.camera.CatchUp();
            }
            //tempRoom.Update(gameTime);
            frameRateCounter.Draw(Assets.font);

            base.Draw(gameTime);
            if (GraphicsReset)
            {
                Manager.Graphics.ApplyChanges();
                mainRoom.roomRenderTarget = new RenderTarget2D(GraphicsDevice, ScreenWidth, ScreenHeight);
                GraphicsReset = false;
            }
            if (OnUpdate!= null)
                OnUpdate.Invoke();
        }

        //called by tom-shame
        protected override void DrawScene(GameTime gameTime)
        {
            Manager.Renderer.Begin(BlendingMode.Default);
            Microsoft.Xna.Framework.Rectangle frame = new Microsoft.Xna.Framework.Rectangle(0, 0, ScreenWidth, ScreenHeight);

            Manager.Renderer.Draw(room.roomRenderTarget, new Microsoft.Xna.Framework.Rectangle(0, 0, ScreenWidth, ScreenHeight), Color.White);
            
            Manager.Renderer.End();
        }
        protected override void Draw(GameTime gameTime)
        {
            //fuck tom shane
        }
        public static void Start()
        {
            if (game != null) throw new SystemException("Game was already Started");
            game = new OrbIt();
            game.Run();
        }


        
    }
}
