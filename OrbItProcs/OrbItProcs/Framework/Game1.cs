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
        shader
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
    }

    public class Game1 : Application
    {
        public static GameTime GlobalGameTime;
        public ManualResetEventSlim TomShaneWaiting = new ManualResetEventSlim(true);
        public UserInterface ui;
        public Room room;
        public SpriteBatch spriteBatch;
        public SpriteFont font;
        FrameRateCounter frameRateCounter;

        public SharpSerializer serializer = new SharpSerializer();

        public Room mainRoom;
        public Room tempRoom;

        public ProcessManager processManager { get; set; }
        public static int smallWidth = 1280;
        public static int smallHeight = 650;
        public static bool soundEnabled = false;
        //public static int fullWidth = 1680;
        //public static int fullHeight = 1050;

        public static int fullWidth = 1920;
        public static int fullHeight = 1080;
        public static string filepath = "Presets//Nodes/";
        public static bool isFullScreen = false;
        public static bool TakeScreenshot = false;

        public int Width { get { return Graphics.PreferredBackBufferWidth; } set { Graphics.PreferredBackBufferWidth = value; } }
        public int Height { get { return Graphics.PreferredBackBufferHeight; } set { Graphics.PreferredBackBufferHeight = value; } }

        public bool IsOldUI { get { return ui != null && ui.sidebar != null && ui.sidebar.activeTabControl == ui.sidebar.tbcMain; } }

        public static bool Debugging = false;

        public Dictionary<textures, Texture2D> textureDict;
        public Dictionary<textures, Vector2> textureCenters;
        public Texture2D[,] btnTextures;
        public static bool bigTonyOn = false;

        public ObservableCollection<object> NodePresets = new ObservableCollection<object>();
        public float backgroundHue = 180;
        public double x = 0;

        // Shader code
        public static Effect shaderEffect;


        public static readonly object drawLock = new object();
        /////////////////////
        public Redirector redirector;
        public Testing testing;

        public Game1() : base()
        {

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Graphics.SynchronizeWithVerticalRetrace = true;
            IsFixedTimeStep = false;

            Width = smallWidth;
            Height = smallHeight;
            //Width = fullWidth;
            //Height = fullHeight;
            Utils.PopulateComponentTypesDictionary();
            ClearBackground = true;
            BackgroundColor = Color.White;
            ExitConfirmation = false;

            Manager.AutoUnfocus = false;
            Manager.Input.InputMethods = InputMethods.Mouse | InputMethods.Keyboard;
            Graphics.PreferMultiSampling = false;
        }

        public static bool deviceReset = false;
        public void ToggleFullScreen(bool on)
        {
            Game1.isFullScreen = on;
            Manager.Graphics.IsFullScreen = on;
            if (on)
            {
                Width = fullWidth;
                Height = fullHeight;
                
            }
            else
            {
                Width = smallWidth;
                Height = smallHeight;
            }
            deviceReset = true;
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
            };
            btnTextures = Program.getGame().Content.Load<Texture2D>("Textures/buttons").sliceSpriteSheet(2, 5);

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
               
            ui = new UserInterface(this);

            room = new Room(this, 1880, 1175);
            mainRoom = room;
            tempRoom = new Room(this, 1880, 1175);
            tempRoom.borderColor = Color.Red;
            Program.room = room;

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

            /*
            int count = 0;
            foreach (var c in tt.Skin.Layers[0].Text.Font.Resource.Characters)
            {
                Console.WriteLine("{0} : {1}", count++, c);
            }*/
            CreatePlayers();
        }

        public void CreatePlayers()
        {
            Shooter.MakeBullet();
            for(int i = 1; i < 5; i++)
            {
                Player p = Player.GetNew(i);
                if (p == null) break;
                Vector2 spawnPos = Vector2.Zero;

                double angle = Utils.random.NextDouble() * Math.PI * 2;
                angle -= Math.PI;
                float dist = 200;
                float x = dist * (float)Math.Cos(angle);
                float y = dist * (float)Math.Sin(angle);
                spawnPos = new Vector2(room.worldWidth / 2, room.worldHeight / 2) - new Vector2(x, y);
                Node node = room.game.spawnNode((int)spawnPos.X, (int)spawnPos.Y);
                node.name = "player" + i;
                node.addComponent(comp.shooter, true);
                node.addComponent(comp.sword, true);
                p.node = node;
                room.masterGroup.fullSet.Add(node);
                node.OnSpawn();
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            room.camera.Render();
            GlobalGameTime = gameTime;
            base.Update(gameTime);

            //frameRateCounter.UpdateElapsed(gameTime.ElapsedGameTime);
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

        public static void Screenshot(GraphicsDevice device)
        {
            byte[] screenData;

            screenData = new byte[device.PresentationParameters.BackBufferWidth * device.PresentationParameters.BackBufferHeight * 4];

            device.GetBackBufferData<byte>(screenData);

            Texture2D t2d = new Texture2D(device, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight, false, device.PresentationParameters.BackBufferFormat);

            t2d.SetData<byte>(screenData);

            int i = 0;
            string date = DateTime.Now.ToShortDateString().Replace('/', '-');
            //date = date.Replace;
            //string name = "Screenshots//SS_" + date + "_#" + i + ".png";
            string name = "..//..//..//Screenshots//SS_" + date + "_#" + i + ".png";
            while (File.Exists(name))
            {
                i += 1;
                name = "..//..//..//Screenshots//SS_" + date + "_#" + i + ".png";
            }
            Stream st = new FileStream(name, FileMode.Create);

            t2d.SaveAsPng(st, t2d.Width, t2d.Height);

            st.Close();

            t2d.Dispose();
        } 

        public void ResetRoomReferences(Room newRoom, bool main = false)
        {
            if (newRoom == null) throw new SystemException("Room was null when reseting room references");
            Program.room = newRoom;
            room = newRoom;
            if (main) mainRoom = newRoom;
            ui.room = newRoom;
            ui.sidebar.room = newRoom;
            ui.sidebar.inspectorArea.room = newRoom;
            ui.sidebar.insArea2.room = newRoom;
            ui.sidebar.UpdateGroupComboBoxes();
            foreach (Process p in processManager.processes)
            {
                p.room = newRoom;
            }
            foreach(Process p in processManager.processDict.Values)
            {
                p.room = newRoom;
            }
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

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
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
                //if (ui.spawnerNode != null)
                //{
                //    Node.cloneNode(ui.spawnerNode, newNode);
                //}
                //else
                //{
                    Node.cloneNode(ui.sidebar.ActiveDefaultNode, newNode);
                //}
            }
            newNode.group = activegroup;
            newNode.name = activegroup.Name + Node.nodeCounter;
            newNode.acceptUserProps(userProperties);

            /*CollisionDelegate toggleWhite = delegate(Node source, Node target)
            {
                if (target == null) return;
                if (source.body.color == Color.White)
                    source.body.color = Utils.randomColor();
                else
                    source.body.color = Color.White;

            };
            CollisionDelegate randomCol = delegate(Node source, Node target)
            {
                if (target == null) return;
                source.body.color = Utils.randomColor();
            };
            CollisionDelegate absorbColor = delegate(Node source, Node target)
            {
                if (target == null) return;
                int div = 25;
                int r = (int)source.body.color.R + ((int)target.body.color.R - (int)source.body.color.R) / div;
                int g = (int)source.body.color.G + ((int)target.body.color.G - (int)source.body.color.G) / div;
                int b = (int)source.body.color.B + ((int)target.body.color.B - (int)source.body.color.B) / div;
                source.body.color = new Color(r, g, b, (int)source.body.color.A);
            };
            CollisionDelegate empty = delegate(Node s, Node t) { };
            Action<Node> first = n => n.body.color = n.body.permaColor;
            Action<Node> none = n => n.body.color = Color.White;

            //newNode.OnCollisionFirst += first;
            //newNode.OnCollisionNone += none;
            //newNode.OnCollisionEnd += (mm, mmm) => { };*/

            

            //newNode.delegator.AddAffectOther("switchVel", delegation);
            //newNode.delegator.AddAffectSelfAndDS("sprint", sprint, dd);

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
            //Collider col = new Collider(new Circle(Utils.random.Next(200)));
            //col.OnCollisionStay += delegate(Node source, Node target)
            //{
            //    source.body.color = Utils.randomColor();
            //};
            //newNode.collision.AddCollider(col);

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

        public void StartStateMachine()
        {
            IEnumerable<bool> sm = StateMachine();
            IEnumerator<bool> statemachine = sm.GetEnumerator();
            bool state;
            state = statemachine.MoveNext();
            state = statemachine.MoveNext();
            state = statemachine.MoveNext();
        }

        IEnumerable<bool> StateMachine()
        {
            bool state = false;
            while(true)
            {
                Console.WriteLine("state:" + state);
                yield return state;
                state = !state;
            }
        }
    }
}
