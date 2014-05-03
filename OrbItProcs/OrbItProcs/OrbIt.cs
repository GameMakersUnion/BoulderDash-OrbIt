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

    public enum comp
    {
        friction,
        gravity,
        transfer,
        displace,
        fixedforce,
        spring,
        colorgravity,
        orbiter,
        randommove,
        relativemotion,
        circler,
        modifier,
        movement,
        colorchanger,
        lifetime,
        scheduler,
        delegator,
        swap,
        shooter,
        conveyor,
        //draw components
        rune,
        waver,
        laser,
        wideray,
        phaseorb,
        flow,
        tether,
        tree,
        collision,
        spike,
        basicdraw,
        meta,
        shader,
        light,
        obstructor,
        diode,
        queuer,
        //middle,
        //slow,
        //siphon,
        //ghost,
        //chrono,
        //weird,
        sword,
        fist,
        magictorch,
        itempayload,
        shovel,

    };
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
        public const int fucksGiven =  0;
        public ProcessManager processManager { get; set; }

        public SharpSerializer serializer = new SharpSerializer();

        public static GlobalGameMode globalGameMode { get; set; }
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
        public Room tempRoom;

        public static GameTime gametime;
        public FrameRateCounter frameRateCounter;
        public resolutions? preferredFullScreen;
        public static bool soundEnabled = false;
        public static bool isFullScreen = false;
        public static int Width { get { return game.Graphics.PreferredBackBufferWidth; } set { game.Graphics.PreferredBackBufferWidth = value; } }
        public static int Height { get { return game.Graphics.PreferredBackBufferHeight; } set { game.Graphics.PreferredBackBufferHeight = value; } }
        public bool IsOldUI { get { return ui != null && ui.sidebar != null && ui.sidebar.activeTabControl == ui.sidebar.tbcMain; } }
        public static bool Debugging = false;
        public static bool bigTonyOn = false;
        private bool GraphicsReset;
        
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
                    Width = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
                    Height = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
                    break;
                case resolutions.FHD_1920x1080:
                    Width = 1920; Height = 1080; break;
                case resolutions.HD_1366x768:
                    Width = 1366; Height = 768; break;
                case resolutions.SVGA_800x600:
                    Width = 800; Height = 600; break;
                case resolutions.SXGA_1280x1024:
                    Width = 1280; Height = 1024; break;
                case resolutions.VGA_640x480:
                    Width = 640; Height = 480; break;
                case resolutions.WSXGA_1680x1050:
                    Width = 1680; Height = 1050; break;
                case resolutions.WXGA_1280x800:
                    Width = 1280; Height = 800; break;
                case resolutions.XGA_1024x768:
                    Width = 1024; Height = 768; break;
            }
            Manager.Graphics.IsFullScreen = fullScreen;
            GraphicsReset = true;
        }
        protected override void Initialize()
        {
            //Load Stuff.
            Utils.PopulateComponentTypesDictionary();
            Assets.LoadAssets(Content);
            DelegatorMethods.InitializeDelegateMethods();
            //Sup Tom.
            base.Initialize();
            //Get Roomy
            mainRoom = new Room(this, Width, Height*8);//*8); //change to height
            tempRoom = new Room(this, 200, 200);
            tempRoom.borderColor = Color.Red;
            room = mainRoom;
            //Hi-Definition Orbs:
            setResolution(resolutions.HD_1366x768, false);//(resolutions.HD_1366x768, false);
            //A game need players, no?
            Player.CreatePlayers(mainRoom);
            //UI
            ui = UserInterface.Start();
            globalGameMode = new GlobalGameMode(this);
            ui.Initialize();
            foreach (var tabpage in ui.sidebar.tbcViews.TabPages)
            {
                string whitespace = "  ";
                tabpage.Text = whitespace + tabpage.Text + whitespace;
            }
            //The only important stat in OrbIt.
            frameRateCounter = new FrameRateCounter(this);

            //We put the Procs In OrbItProcs
            processManager = new ProcessManager(this);
            processManager.SetProcessKeybinds();

            //Keybinds
            ui.keyManager.addProcessKeyAction("exitgame", KeyCodes.Escape, OnPress: () => Exit());
            ui.keyManager.addProcessKeyAction("togglesidebar", KeyCodes.OemTilde, OnPress: ui.ToggleSidebar);
            ui.keyManager.addProcessKeyAction("switchview", KeyCodes.PageDown, OnPress: ui.SwitchView);
            ui.keyManager.addProcessKeyAction("removeall", KeyCodes.Delete, OnPress: () => ui.sidebar.btnRemoveAllNodes_Click(null, null));

            MainWindow.TransparentClientArea = true;
            ui.ToggleSidebar();
            //Testing.sawtoothTest();

            LoadLevelWindow.StaticLevel("Test.xml");
        }
        Window test;
        public resolutions preferredWindowed;
        public static string gameMode = "SpiderDiggers";

        private void RoomPanel(Room room)
        {
            test = new Window(Manager);
            test.Init();
            test.SetPosition(Width / 2, Height / 2);
            test.Width = 200;
            test.Height = 300;
        }

        protected override void Update(GameTime gameTime)
        {
            //Do not write code above this.
            base.Update(gameTime);
            //Do not move the above lines.
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
                mainRoom.roomRenderTarget = new RenderTarget2D(GraphicsDevice, Width, Height);
                GraphicsReset = false;
            }
            if (OnUpdate!= null)
                OnUpdate.Invoke();
        }

        bool Title = true;
        bool cont = false;
        
        protected override void DrawScene(GameTime gameTime)
        {
            Manager.Renderer.Begin(BlendingMode.Default);
            Microsoft.Xna.Framework.Rectangle frame = new Microsoft.Xna.Framework.Rectangle(0, 0, Width, Height);

            if (!room.gameStarted && Title)
            {
                Manager.Renderer.Draw(Assets.textureDict[textures.black], frame, Color.White);
                Manager.Renderer.Draw(Assets.textureDict[textures.Logo], frame, Color.White);
                if (GamePad.GetState(Controller.intToPlayerIndex[1]).IsConnected) Manager.Renderer.Draw(Assets.textureDict[textures.Player1_2], frame, Color.White);
                if (GamePad.GetState(Controller.intToPlayerIndex[2]).IsConnected) Manager.Renderer.Draw(Assets.textureDict[textures.Player2_2], frame, Color.White);
                if (GamePad.GetState(Controller.intToPlayerIndex[3]).IsConnected) Manager.Renderer.Draw(Assets.textureDict[textures.Player3_2], frame, Color.White);
                if (GamePad.GetState(Controller.intToPlayerIndex[4]).IsConnected) Manager.Renderer.Draw(Assets.textureDict[textures.Player4_2], frame, Color.White);
                if (GamePad.GetState(Controller.intToPlayerIndex[1]).Buttons.Start == ButtonState.Pressed) {Title = false;cont =true;}
                if (GamePad.GetState(Controller.intToPlayerIndex[2]).Buttons.Start == ButtonState.Pressed) {Title = false;cont =true;}
                if (GamePad.GetState(Controller.intToPlayerIndex[3]).Buttons.Start == ButtonState.Pressed) {Title = false;cont =true;}
                if (GamePad.GetState(Controller.intToPlayerIndex[4]).Buttons.Start == ButtonState.Pressed) { Title = false; cont = true; }
            }
            else if (!room.gameStarted)
            {
                Manager.Renderer.Draw(Assets.textureDict[textures.controller], frame, Color.White);
                if (GamePad.GetState(Controller.intToPlayerIndex[1]).Buttons.A == ButtonState.Pressed) { room.gameStarted = true; }
                if (GamePad.GetState(Controller.intToPlayerIndex[2]).Buttons.A == ButtonState.Pressed) { room.gameStarted = true; }
                if (GamePad.GetState(Controller.intToPlayerIndex[3]).Buttons.A == ButtonState.Pressed) { room.gameStarted = true; }
                if (GamePad.GetState(Controller.intToPlayerIndex[4]).Buttons.A == ButtonState.Pressed) { room.gameStarted = true; }
            
            } else{
                Manager.Renderer.Draw(room.roomRenderTarget, new Microsoft.Xna.Framework.Rectangle(0, 0, Width, Height), Color.White);
            }
            
            Manager.Renderer.End();
        }
        protected override void Draw(GameTime gameTime)
        {
            //fuck tom shane
        }
        public void SwitchToMainRoom()
        {
            room = mainRoom;
        }
        public void SwitchToTempRoom(bool reset = true)
        {
            if (tempRoom == null) return;
            room = tempRoom;
            if (reset)
            {
                room.generalGroups.EmptyGroup();
            }
        }
        public static void Start()
        {
            if (game != null) throw new SystemException("Game was already Started");
            game = new OrbIt();
            //try
            //{
            game.Run();
            //}
            //catch(Exception e)
            //{
            //    game.mainRoom.camera.AbortThread();
            //    throw new SystemException("???", e);
            //}
        }


        
    }
}
