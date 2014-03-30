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
    public class Game1 : Application
    {
        public static Game1 game;

        #region ///Assets///
        public static SpriteFont font;
        #endregion
        #region ///Components///
        public SharpSerializer serializer = new SharpSerializer();
        #endregion
        #region ///Components///
        private Room _activeRoom;
        public Room room
        {
            get { return _activeRoom; }
            set
            {
                if (value == null) throw new SystemException("Room was null when reseting room references");
                _activeRoom = value;
                if (ui != null)
                    ui.sidebar.UpdateGroupComboBoxes();
            }
        }
        public Room mainRoom;
        public Room tempRoom;
        #endregion

        public static GameTime GlobalGameTime;
        public ManualResetEventSlim TomShaneWaiting = new ManualResetEventSlim(true);
        public UserInterface ui;

        public SpriteBatch spriteBatch;
        public FrameRateCounter frameRateCounter;
        public ProcessManager processManager { get; set; }
        public static int smallWidth = 1280;
        public static int smallHeight = 650;
        public static bool soundEnabled = false;
        public static int fullWidth = 1920;
        public static int fullHeight = 1080;
        public static string filepath = "Presets//Nodes/";
        public static bool isFullScreen = false;
        public static bool TakeScreenshot = false;
        public static int Width { get { return game.Graphics.PreferredBackBufferWidth; } set { game.Graphics.PreferredBackBufferWidth = value; } }
        public static int Height { get { return game.Graphics.PreferredBackBufferHeight; } set { game.Graphics.PreferredBackBufferHeight = value; } }
        public bool IsOldUI { get { return ui != null && ui.sidebar != null && ui.sidebar.activeTabControl == ui.sidebar.tbcMain; } }
        public static bool Debugging = false;
        public Dictionary<textures, Texture2D> textureDict;
        public Dictionary<textures, Vector2> textureCenters;
        public Texture2D[,] btnTextures;
        public static bool bigTonyOn = false;
        public ObservableCollection<object> NodePresets = new ObservableCollection<object>();
        public float backgroundHue = 180;
        public double x = 0;
        public static Effect shaderEffect; // Shader code
        public static readonly object drawLock = new object();
        public Redirector redirector;
        public Testing testing;
        public static bool EnablePlayers = true;


        private Game1() : base()
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


        //public static bool deviceReset = false;
        public void ToggleFullScreen(bool on)
        {
            Game1.isFullScreen = on;
            Manager.Graphics.IsFullScreen = on;
            if (false)
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

        protected override void Initialize()
        {
            if (!Directory.Exists(filepath)) Directory.CreateDirectory(filepath);
            textureDict = new Dictionary<textures, Texture2D>(){
            { textures.blueorb, Content.Load<Texture2D>("Textures/bluesphere"               )},
            { textures.whiteorb, Content.Load<Texture2D>("Textures/whiteorb"                )},
            { textures.colororb, Content.Load<Texture2D>("Textures/colororb"                )},
            { textures.whitepixel, Content.Load<Texture2D>("Textures/whitepixel"            )},
            { textures.whitepixeltrans, Content.Load<Texture2D>("Textures/whitepixeltrans"  )},
            { textures.whitecircle, Content.Load<Texture2D>("Textures/whitecircle"          )},
            { textures.whitesphere, Content.Load<Texture2D>("Textures/whitesphere"          )},
            { textures.blackorb, Content.Load<Texture2D>("Textures/blackorb"                )},
            { textures.ring, Content.Load<Texture2D>("Textures/ring"                        )},
            { textures.orientedcircle, Content.Load<Texture2D>("Textures/orientedcircle"    )},
            { textures.sword, Content.Load<Texture2D>("Textures/sword"    )},

            };
            btnTextures = Game1.game.Content.Load<Texture2D>("Textures/buttons").sliceSpriteSheet(2, 5);

            textureCenters = new Dictionary<textures, Vector2>();
            foreach(var tex in textureDict.Keys)
            {
                Texture2D t = textureDict[tex];
                textureCenters[tex] = new Vector2(t.Width / 2f, t.Height / 2f);
            }

            
            font = Content.Load<SpriteFont>("Courier New");
            DelegatorMethods.InitializeDelegateMethods();
            spriteBatch = new SpriteBatch(Graphics.GraphicsDevice);

            // Shader Code 
            shaderEffect = Content.Load<Effect>("Effects/Shader");


            room = new Room(this, 1880, 1175);
            mainRoom = room;
            tempRoom = new Room(this, 1880, 1175);
            tempRoom.borderColor = Color.Red;
            room = room;

            ui = new UserInterface(this);



            processManager = new ProcessManager(this);

            ui.Initialize(room);

            frameRateCounter = new FrameRateCounter(this);
            base.Initialize();          

            testing = new Testing();

            

            ui.sidebar.UpdateGroupComboBoxes();
            ui.sidebar.cbListPicker.ItemIndex = 0;
            ui.sidebar.cbListPicker.ItemIndex = 2;
            ui.sidebar.cbGroupS.ItemIndex = 2;
            ui.sidebar.cbGroupT.ItemIndex = 2;
            InitializePresets();

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
            ui.keyManager.addProcessKeyAction("screenshot", KeyCodes.PrintScreen, OnPress: TakeScreenShot);
            ui.keyManager.addProcessKeyAction("removeall", KeyCodes.Delete, OnPress: () => ui.sidebar.btnRemoveAllNodes_Click(null, null));

            if (EnablePlayers)
            {
                CreatePlayers();
            }
            ui.sidebar.InitializeFifthPage();
        }
        public static void ResetPlayers()
        {
            Room r = Game1.game.room;
            r.playerGroup.EmptyGroup();
            Controller.ResetControllers();
            CreatePlayers();
            game.ui.sidebar.playerView.InitializePlayers();
        }

        public static void CreatePlayers()
        {
            Room r = Game1.game.room;
            Shooter.MakeBullet();
            Node def = r.masterGroup.defaultNode.CreateClone();
            def.addComponent(comp.shooter, true);
            r.playerGroup.defaultNode = def;
            for(int i = 1; i < 5; i++)
            {
                Player p = Player.GetNew(i);
                if (p == null) break;
                double angle = Utils.random.NextDouble() * Math.PI * 2;
                angle -= Math.PI;
                float dist = 200;
                float x = dist * (float)Math.Cos(angle);
                float y = dist * (float)Math.Sin(angle);
                Vector2 spawnPos = new Vector2(r.worldWidth / 2, r.worldHeight / 2) - new Vector2(x, y);
                Node node = def.CreateClone();
                node.body.pos = spawnPos;
                node.name = "player" + i;
                node.SetColor(p.pColor);
                node.addComponent(comp.shooter, true);
                node.addComponent(comp.sword, true);
                node.Comp<Sword>().sword.collision.DrawRing = false;
                p.node = node;
                r.playerGroup.IncludeEntity(node);
                node.OnSpawn();
            }
        }

        protected override void Update(GameTime gameTime)
        {
            room.camera.Render();
            GlobalGameTime = gameTime;
            base.Update(gameTime);

            frameRateCounter.Update(gameTime);

            if (IsActive) ui.Update(gameTime);

            if (!ui.IsPaused)
            {
                if (room != null) room.Update(gameTime);
            }
            else
            {
                //room.colorEffectedNodes();
                //room.updateTargetNodeGraphic();
                if (room != null) room.gridSystemLines = new List<Microsoft.Xna.Framework.Rectangle>();
            }
            if (!ui.IsPaused)
            {
                x += Math.PI / 360.0;
                backgroundHue = (backgroundHue + ((float)Math.Sin(x) + 1) / 10f) % 360;
                BackgroundColor = ColorChanger.getColorFromHSV(backgroundHue, value: 0.2f);
            }

            room.Draw();
            frameRateCounter.Draw(spriteBatch, font);
            TomShaneWaiting.Wait();
            TomShaneWaiting.Reset();
            base.Draw(gameTime);
            
        }


        protected override void DrawScene(GameTime gameTime)
        {
            Manager.Renderer.Begin(BlendingMode.Default);
            Manager.Renderer.Draw(room.roomRenderTarget, new Microsoft.Xna.Framework.Rectangle(0, 0, Width, Height), Color.White);
            Manager.Renderer.End();
            
            if (TakeScreenshot)
            {
                Screenshot(Manager.Graphics.GraphicsDevice);
                TakeScreenshot = false;
            }
        }
        protected override void Draw(GameTime gameTime)
        {
            //fuck tom shane
        }
        public void SwitchToMainRoom()
        {
            //room = mainRoom;
            ResetRoomReferences(mainRoom);
        }
        public void SwitchToTempRoom(bool reset = true)
        {
            if (tempRoom == null) return;
            ResetRoomReferences(tempRoom);
            if (reset)
            {
                room.generalGroups.EmptyGroup();
            }
        }

        public void TakeScreenShot()
        {
            TakeScreenshot = true;
        }

        public void Screenshot(GraphicsDevice device)
        {
            Texture2D t2d = room.roomRenderTarget;
            int i = 0; string name;
            string date = DateTime.Now.ToShortDateString().Replace('/', '-');
            do
            {
                name = "..//..//..//Screenshots//SS_" + date + "_#" + i + ".png";
                i += 1;
            } while (File.Exists(name));
            
            Stream st = new FileStream(name, FileMode.Create);
            t2d.SaveAsPng(st, t2d.Width, t2d.Height);
            st.Close();
            t2d.Dispose();
        } 

        public void ResetRoomReferences(Room newRoom, bool main = false)
        {

        }

        public void InitializePresets()
        {
            foreach (string file in Directory.GetFiles(filepath, "*.xml"))
            {
                try
                {
                    Node presetnode = (Node)room.game.serializer.Deserialize(file);
                NodePresets.Add(presetnode);
            }
                catch(Exception e)
            {
                    Console.WriteLine("Failed to deserialize node: {0}", e.Message);
                }
            }
        }     

        public Node spawnNode(Node newNode, Action<Node> afterSpawnAction = null, int lifetime = -1, Group g = null)
        {
            Group spawngroup = ui.sidebar.ActiveGroup;
            if (g == null && !spawngroup.Spawnable) return null;
            if (g != null)
            {
                spawngroup = g;
            }
            newNode.name = "bullet" + Node.nodeCounter;

            return SpawnNodeHelper(newNode, afterSpawnAction, spawngroup, lifetime);
        }
        public Node spawnNode(Dictionary<dynamic, dynamic> userProperties, Action<Node> afterSpawnAction = null, bool blank = false, int lifetime = -1)
        {
            Group activegroup = ui.sidebar.ActiveGroup;
            if (activegroup == null || !activegroup.Spawnable) return null;
            if (room == mainRoom && ui.sidebar.activeTabControl == ui.sidebar.tbcViews && ui.sidebar.tbcViews.SelectedIndex != 0) return null;

            Node newNode = new Node();
            if (!blank)
            {
                Node.cloneNode(ui.sidebar.ActiveDefaultNode, newNode);
            }
            newNode.group = activegroup;
            newNode.name = activegroup.Name + Node.nodeCounter;
            newNode.acceptUserProps(userProperties);
            AssignColor(activegroup, newNode);
            return SpawnNodeHelper(newNode, afterSpawnAction, activegroup, lifetime);
        }

        
        private Node SpawnNodeHelper(Node newNode, Action<Node> afterSpawnAction = null, Group g = null, int lifetime = -1)
        {
            newNode.OnSpawn();
            if (afterSpawnAction != null) afterSpawnAction(newNode);
            if (lifetime != -1)
            {
                newNode.addComponent(comp.lifetime, true);
                newNode.Comp<Lifetime>().timeUntilDeath.value = lifetime;
                newNode.Comp<Lifetime>().timeUntilDeath.enabled = true;
            }
            g.IncludeEntity(newNode);
            return newNode;
        }

        public Node spawnNode()
        {
            Dictionary<dynamic, dynamic> userP = new Dictionary<dynamic, dynamic>() {
                                { nodeE.position, UserInterface.WorldMousePos },
            };
            return spawnNode(userP);
        }

        public Node spawnNode(int worldMouseX, int worldMouseY)
        {
            Dictionary<dynamic, dynamic> userP = new Dictionary<dynamic, dynamic>() {
                                { nodeE.position, new Vector2(worldMouseX,worldMouseY) },
            };
            return spawnNode(userP);
        }

        public void AssignColor(Group activegroup, Node newNode)
        {
            if (Group.IntToColor.ContainsKey(activegroup.GroupId))
            {
                newNode.body.color = Group.IntToColor[activegroup.GroupId];
            }
            else
            {
                int Enumsize = Enum.GetValues(typeof(KnownColor)).Length;

                //int rand = Utils.random.Next(size - 1);
                int index = 0;
                foreach (char c in activegroup.Name.ToCharArray().ToList())
                {
                    index += (int)c;
                }
                index = index % (Enumsize - 1);

                System.Drawing.Color syscolor = System.Drawing.Color.FromKnownColor((KnownColor)index);
                Color xnacol = new Color(syscolor.R, syscolor.G, syscolor.B, syscolor.A);
                newNode.body.color = xnacol;
            }
        }

        public void saveNode(Node node, string name)
        {
            if (name.Equals("") || node == null) return;

            name = name.Trim();
            string filename = "Presets//Nodes//" + name + ".xml";
            Action completeSave = delegate{
            ui.sidebar.inspectorArea.editNode.name = name;
            Node serializenode = new Node();
            Node.cloneNode(ui.sidebar.inspectorArea.editNode, serializenode);
            room.game.serializer.Serialize(serializenode, filename);
            ui.game.NodePresets.Add(serializenode);
            };

            if (File.Exists(filename)){ //we must be overwriting, therefore don't update the live presetList
                PopUp.Prompt("OverWrite?", "O/W?", delegate(bool c, object a) { if (c) { completeSave(); PopUp.Toast("Node was overridden"); } return true; });
            }
            else { PopUp.Toast("Node Saved"); completeSave(); }

        }

        internal void deletePreset(Node p)
        {
            Console.WriteLine("Deleting file: " + p);
            File.Delete(Game1.filepath + p.name + ".xml");
            NodePresets.Remove(p);
        }

        public static void Start()
        {
            if (game != null) throw new SystemException("Game was already Started");
            game = new Game1();
            game.Run();
        }
    }
}
