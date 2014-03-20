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

namespace OrbItProcs
{

    public enum comp
    {
        queuer,
        
        collision,
        gravity,
        fixedforce,
        displace,
        orbiter,

        randvelchange,
        relativemotion,
        transfer,
        circler,
        
        modifier,

        movement,
        
        colorchanger,
        colorgravity,
        lifetime,
        scheduler,
        delegator,

        //draw components
        
        waver,
        laser,
        wideray,
        phaseorb,
        flow,

        tether,
        spring,
        tree,
        basicdraw,

        //repel,
        //middle,
        //slow,
        //siphon,
        //ghost,
        //chrono,
        //weird,
    };

    public enum textures
    {
        blueorb,
        whiteorb,
        colororb,
        whitecircle,
        whitepixel,
        whitepixeltrans,
        blackorb,
        whitesphere,
        ring,
    }

    public class Game1 : Application
    {
        public static GameTime GlobalGameTime;

        public UserInterface ui;
        public Room room;
        public SpriteBatch spriteBatch;
        public SpriteFont font;
        FrameRateCounter frameRateCounter;

        public SharpSerializer serializer = new SharpSerializer();

        public ProcessManager processManager { get; set; }
        public static int sWidth = 1000;
        public static int sHeight = 600;
        public static bool soundEnabled = false;
        //public static int fullWidth = 1680;
        //public static int fullHeight = 1050;

        public static int fullWidth = 1920;
        public static int fullHeight = 1080;
        public static string filepath = "Presets//Nodes/";
        public static bool isFullScreen = false;
        public static bool TakeScreenshot = false;

        public static bool Debugging = false;

        public Dictionary<textures, Texture2D> textureDict;
        public Dictionary<textures, Vector2> textureCenters;
        public Node targetNode = null;

        public static bool bigTonyOn = false;


        public ObservableCollection<object> NodePresets = new ObservableCollection<object>();

        /////////////////////
        public Redirector redirector;
        public Testing testing;

        public Game1() : base(true)
        {
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Graphics.SynchronizeWithVerticalRetrace = true;
            IsFixedTimeStep = false;
            //TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 5);

            Graphics.PreferredBackBufferWidth = sWidth;
            Graphics.PreferredBackBufferHeight = sHeight;
            ////

            Utils.PopulateComponentTypesDictionary();

            ClearBackground = true;
            BackgroundColor = Color.White;
            ExitConfirmation = false;

            Manager.AutoUnfocus = false;
            //MainWindow.Visible = false;
            
            //Manager.TargetFrames = 60;

            //Collision col = new Collision();
            //col.AffectOther(null);
            //typeof(Collision).GetMethod("AffectOther").Invoke();

            //Vector2 vv = new Vector2(0, 0);
            //vv.Normalize();
            //Console.WriteLine(vv);
        }


        public void ToggleFullScreen(bool on)
        {
            Game1.isFullScreen = on;
            if (on)
            {
                //SystemBorder = false;
                //FullScreenBorder = false;
                Manager.Graphics.PreferredBackBufferWidth = fullWidth;
                Manager.Graphics.PreferredBackBufferHeight = fullHeight;
                Graphics.IsFullScreen = true;
                Graphics.ApplyChanges();
            }
            else
            {
                //SystemBorder = true;
                //FullScreenBorder = true;
                Manager.Graphics.PreferredBackBufferWidth = sWidth;
                Manager.Graphics.PreferredBackBufferHeight = sHeight;
                Graphics.IsFullScreen = false;
                Graphics.ApplyChanges();
            }
        }

        protected override void Initialize()
        {
            if (!Directory.Exists(filepath)) Directory.CreateDirectory(filepath);
            textureDict = new Dictionary<textures, Texture2D>(){
            {textures.blueorb, Content.Load<Texture2D>("Textures/bluesphere"            )},
            {textures.whiteorb, Content.Load<Texture2D>("Textures/whiteorb"             )},
            {textures.colororb, Content.Load<Texture2D>("Textures/colororb"             )},
            {textures.whitepixel, Content.Load<Texture2D>("Textures/whitepixel"        )},
            {textures.whitepixeltrans, Content.Load<Texture2D>("Textures/whitepixeltrans")},
            {textures.whitecircle, Content.Load<Texture2D>("Textures/whitecircle"   )},
            {textures.whitesphere, Content.Load<Texture2D>("Textures/whitesphere"   )},
            {textures.blackorb, Content.Load<Texture2D>("Textures/blackorb"   )},
            {textures.ring, Content.Load<Texture2D>("Textures/smoothEdge"   )}};

            textureCenters = new Dictionary<textures, Vector2>();
            foreach(var tex in textureDict.Keys)
            {
                Texture2D t = textureDict[tex];
                textureCenters[tex] = new Vector2(t.Width / 2f, t.Height / 2f);
            }

            font = Content.Load<SpriteFont>("Courier New");
            DelegatorMethods.InitializeDelegateMethods();

            room = new Room(this, 1580, 1175);

            processManager = new ProcessManager(room);
            #region ///Default User props///
            Dictionary<dynamic, dynamic> userPr = new Dictionary<dynamic, dynamic>() {
                    { nodeE.position, new Vector2(0, 0) },
                    { nodeE.texture, textures.whitecircle },
                    //{ node.radius, 50 },
                    { comp.basicdraw, true },
                    { comp.collision, true },
                    { comp.movement, true },
                    //{ comp.maxvel, true },
                    //{ comp.randvelchange, true },
                    //{ comp.gravity, true },
                    //{ comp.linearpull, true },
                    //{ comp.laser, true },
                    //{ comp.wideray, true },
                    //{ comp.hueshifter, true },
                    //{ comp.transfer, true },
                    //{ comp.phaseorb, false },
                    //{ comp.tree, true },
                    //{ comp.queuer, true },
                    //{ comp.flow, true },
                    { comp.waver, false },
                    { comp.scheduler, true },

                    //{ comp.tether, false },
                    
                };
            #endregion


            room.defaultNode = new Node(userPr);
            room.defaultNode.name = "master";
            room.defaultNode.IsDefault = true;


            //much faster than foreach keyword apparently. Nice
            room.defaultNode.comps.Keys.ToList().ForEach(delegate(comp c) 
            {
                room.defaultNode.comps[c].AfterCloning();
            });

            Node firstdefault = new Node();
            Node.cloneNode(room.defaultNode, firstdefault);
            firstdefault.name = "[G0]0";
            firstdefault.IsDefault = true;

            Group masterGroup = new Group(room.defaultNode, Name: room.defaultNode.name, Spawnable: false);
            room.masterGroup = masterGroup;

            Group generalGroup = new Group(room.defaultNode, parentGroup: masterGroup, Name: "General Groups", Spawnable: false);
            room.masterGroup.AddGroup(generalGroup.Name, generalGroup);

            Group linkGroup = new Group(room.defaultNode, parentGroup: masterGroup, Name: "Link Groups", Spawnable: false);
            room.masterGroup.AddGroup(linkGroup.Name, linkGroup);

            Group wallGroup = new Group(room.defaultNode, parentGroup: masterGroup, Name: "Walls", Spawnable: false);
            room.masterGroup.AddGroup(wallGroup.Name, wallGroup);

            Group firstGroup = new Group(firstdefault, parentGroup: generalGroup);
            generalGroup.AddGroup(firstGroup.Name, firstGroup);

            
            Dictionary<dynamic, dynamic> userPropsTarget = new Dictionary<dynamic, dynamic>() {
                    { comp.basicdraw, true }, { nodeE.texture, textures.whitecircle } };

            room.targetNodeGraphic = new Node(userPropsTarget);
            room.targetNodeGraphic.name = "TargetNodeGraphic";

            frameRateCounter = new FrameRateCounter(this);
            base.Initialize();
            MainWindow.Visible = false;            

            testing = new Testing();

            ui = new UserInterface(this);

            ui.sidebar.UpdateGroupComboBoxes();
            ui.sidebar.cbListPicker.ItemIndex = 0;
            ui.sidebar.cbListPicker.ItemIndex = 2;
            ui.sidebar.cbGroupS.ItemIndex = 2;
            ui.sidebar.cbGroupT.ItemIndex = 2;
            InitializePresets();

            //room.player1 = new Player();
            //room.player1.body.pos = new Vector2(100, 100);
            //processManager.processDict.Add(proc.axismovement, new AxisMovement(room.player1, 4));

            if (bigTonyOn)
            {
            for (int i = 1; i < 5; i++)
            {
                room.players.Add(new Player(i)); //#bigtony
            }
            }

            processManager.SetProcessKeybinds(ui.keyManager);
            ui.keyManager.addProcessKeyAction("exitgame", KeyCodes.Escape, OnPress: () => Exit());
            ui.keyManager.addProcessKeyAction("togglesidebar", KeyCodes.OemTilde, OnPress: ui.ToggleSidebar);
            ui.keyManager.addProcessKeyAction("screenshot", KeyCodes.PrintScreen, OnPress: TakeScreenShot);
            ui.keyManager.addProcessKeyAction("removeall", KeyCodes.Delete, OnPress: () => ui.sidebar.btnRemoveAllNodes_Click(null, null));

            room.MakeWalls();
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

        public void ResetRoomReferences(Room newRoom)
        {
            if (newRoom == null) throw new SystemException("Room was null when reseting room references");
            Program.room = newRoom;
            room = newRoom;
            ui.room = newRoom;
            ui.sidebar.room = newRoom;
            ui.sidebar.inspectorArea.room = newRoom;
            ui.sidebar.insArea2.room = newRoom;
            ui.sidebar.UpdateGroupComboBoxes();
            processManager.room = newRoom;
            foreach (Process p in processManager.processes)
            {
                p.room = newRoom;
            }
            foreach(Process p in processManager.processDict.Values)
            {
                p.room = newRoom;
            }
            if (processManager.processDict.ContainsKey(proc.mapeditor))
            {
                (processManager.processDict[proc.mapeditor] as MapEditor).level = newRoom.level;
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
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            //spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch = new SpriteBatch(Graphics.GraphicsDevice);
            room.camera.batch = spriteBatch;
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            
            GlobalGameTime = gameTime;
            base.Update(gameTime);

            //frameRateCounter.UpdateElapsed(gameTime.ElapsedGameTime);
            frameRateCounter.Update(gameTime);

            if (!ui.IsPaused)
            {
                room.Update(gameTime);
            }
            else
            {
                //room.colorEffectedNodes();
                //room.updateTargetNodeGraphic();
                room.gridSystemLines = new List<Microsoft.Xna.Framework.Rectangle>();
            }

            if (IsActive) ui.Update(gameTime);
        }
        public float backgroundHue = 180;
        public double x = 0;
        public Color backgroundColor = Color.Black;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //if (!IsActive) return;
            Manager.BeginDraw(gameTime);
            base.Draw(gameTime);
            if (!ui.IsPaused)
            {
                x += Math.PI / 360.0;
                backgroundHue = (backgroundHue + ((float)Math.Sin(x) + 1)/10f) % 360;
                //backgroundHue = (backgroundHue + 0.1f) % 360;
                backgroundColor = ColorChanger.getColorFromHSV(backgroundHue, value: 0.2f);
                //Console.WriteLine("Hue: {0}  R: {1}  G: {2}  B: {3}", backgroundHue, backgroundColor.R, backgroundColor.G, backgroundColor.B);
            }
            GraphicsDevice.Clear(backgroundColor);
            spriteBatch.Begin();

            room.Draw(spriteBatch);
            frameRateCounter.Draw(spriteBatch, font);

            spriteBatch.End();

            Manager.EndDraw();

            if (TakeScreenshot)
            {
                Screenshot(Manager.Graphics.GraphicsDevice);
                TakeScreenshot = false;
            }
        }

        public Node spawnNode(Node newNode, Action<Node> afterSpawnAction = null, int lifetime = -1, Group g = null)
        {
            Group spawngroup = ui.sidebar.ActiveGroupFirst;
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
            Group activegroup = ui.sidebar.ActiveGroupFirst;
            if (!activegroup.Spawnable) return null;
            Node newNode = new Node();
            if (!blank)
            {
                if (ui.spawnerNode != null)
                {
                    Node.cloneNode(ui.spawnerNode, newNode);
                }
                else
                {
                    Node.cloneNode(ui.sidebar.ActiveDefaultNode, newNode);
                }
            }
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
                if (!newNode.comps.ContainsKey(comp.lifetime))
                {
                    newNode.addComponent(comp.lifetime, true);
                }
                newNode.GetComponent<Lifetime>().timeOfDeath.value = lifetime;
                newNode.comps[comp.lifetime].timeOfDeath.enabled = true;
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
