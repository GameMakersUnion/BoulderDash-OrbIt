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
        queuer,
        gravity,
        transfer,
        displace,
        fixedforce,
        spring,
        colorgravity,
        orbiter,
        randvelchange,
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
        //draw components
        waver,
        laser,
        wideray,
        phaseorb,
        flow,
        tether,
        tree,
        collision,
        basicdraw,
        meta,
        shader,
        //middle,
        //slow,
        //siphon,
        //ghost,
        //chrono,
        //weird,
        sword,
    };
    public enum textures
    {
        whitecircle,
        orientedcircle,
        blackorb,
        whitesphere,
        ring,
        whiteorb,
        blueorb,
        colororb,
        whitepixel,
        whitepixeltrans,
        sword
    }
    public enum resolutions
    {
        FHD_1920x1080,
        HD_1366x768,
        SXGA_1280x1024,
        WSXGA_1680x1050,
        XGA_1024x768,
        WXGA_1280x800,
        SVGA_800x600,
        VGA_640x480,
    }

    public class OrbIt : Application
    {
        public static OrbIt game;
        public static UserInterface ui;
        public ProcessManager processManager { get; set; }

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
        public Room tempRoom;

        public static GameTime gametime;
        public FrameRateCounter frameRateCounter;


        public static int smallWidth = 1280;
        public static int smallHeight = 650;
        public static bool soundEnabled = false;
        public static int fullWidth = 1920;
        public static int fullHeight = 1080;
        public static bool isFullScreen = false;
        public static int Width { get { return game.Graphics.PreferredBackBufferWidth; } set { game.Graphics.PreferredBackBufferWidth = value; } }
        public static int Height { get { return game.Graphics.PreferredBackBufferHeight; } set { game.Graphics.PreferredBackBufferHeight = value; } }
        public bool IsOldUI { get { return ui != null && ui.sidebar != null && ui.sidebar.activeTabControl == ui.sidebar.tbcMain; } }
        public static bool Debugging = false;
        public static bool bigTonyOn = false;
        public static bool EnablePlayers = true;


        private OrbIt() : base()
        {
            game = this;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Graphics.SynchronizeWithVerticalRetrace = true;
            IsFixedTimeStep = false;
            Width = smallWidth;
            Height = smallHeight;

            Utils.PopulateComponentTypesDictionary();
            ClearBackground = true;
            BackgroundColor = Color.White;
            ExitConfirmation = false;

            Manager.AutoUnfocus = false;
            Manager.Input.InputMethods = InputMethods.Mouse | InputMethods.Keyboard;
            Graphics.PreferMultiSampling = false;
        }

        public void ToggleFullScreen(bool on)
        {
            OrbIt.isFullScreen = on;
            Manager.Graphics.IsFullScreen = on;
            if (on)
            {
                Width = fullWidth;
                Height = fullHeight;
            }
            else if (false)
            {
                Width = smallWidth;
                Height = smallHeight;
            }
            Manager.Graphics.ApplyChanges();
            Manager.CreateRenderTarget(Width, Height);
        }

        public void setResolution(resolutions r)
        {

        }


        protected override void Initialize()
        {
            Assets.LoadAssets(Content);
            DelegatorMethods.InitializeDelegateMethods();
            room = new Room(this, 1880, 1175);
            room.name = "main";
            mainRoom = room;
            tempRoom = new Room(this, 1880, 1175);
            room.name = "temp";
            tempRoom.borderColor = Color.Red;
            room = mainRoom;
            ui = UserInterface.Start();
            processManager = new ProcessManager(this);
            ui.Initialize(room);
            frameRateCounter = new FrameRateCounter(this);
            base.Initialize();          
            ui.sidebar.UpdateGroupComboBoxes();
            ui.sidebar.cbListPicker.ItemIndex = 0;
            ui.sidebar.cbListPicker.ItemIndex = 2;
            ui.sidebar.cbGroupS.ItemIndex = 2;
            ui.sidebar.cbGroupT.ItemIndex = 2;
            //room.player1 = new Player();
            //room.player1.body.pos = new Vector2(100, 100);
            //processManager.processDict.Add(proc.axismovement, new AxisMovement(room.player1, 4));
            //if (bigTonyOn)
            //{
            //    for (int i = 1; i < 5; i++)
            //    {
            //        Player p = Player.GetNew(i);
            //        if (p != null)
            //            room.players.Add(p); //#bigtony
            //    }
            //}
            processManager.SetProcessKeybinds();
            ui.keyManager.addProcessKeyAction("exitgame", KeyCodes.Escape, OnPress: () => Exit());
            ui.keyManager.addProcessKeyAction("togglesidebar", KeyCodes.OemTilde, OnPress: ui.ToggleSidebar);
            ui.keyManager.addProcessKeyAction("switchview", KeyCodes.PageDown, OnPress: ui.SwitchView);
            ui.keyManager.addProcessKeyAction("removeall", KeyCodes.Delete, OnPress: () => ui.sidebar.btnRemoveAllNodes_Click(null, null));

            if (EnablePlayers)
            {
                Player.CreatePlayers();
            }
            ui.sidebar.InitializeFifthPage();
        }
       
        protected override void Update(GameTime gameTime)
        {
            //Do not write code above this.
            room.camera.RenderAsync();
            base.Update(gameTime);
            //Do not move the above lines.
            gametime = gameTime;
            frameRateCounter.Update(gameTime);
            if (IsActive) ui.Update(gameTime);
            if (!ui.IsPaused)
            {
                if (room != null) room.Update(gameTime);
            }
            else
            {
                if (room != null) room.gridSystemLines = new List<Microsoft.Xna.Framework.Rectangle>();
            }

            room.Draw();
            frameRateCounter.Draw(Assets.font);
            room.camera.CatchUp();
            base.Draw(gameTime);
            
        }


        protected override void DrawScene(GameTime gameTime)
        {
            Manager.Renderer.Begin(BlendingMode.Default);
            Manager.Renderer.Draw(room.roomRenderTarget, new Microsoft.Xna.Framework.Rectangle(0, 0, Width, Height), Color.White);
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
            game.Run();
        }
    }
}
