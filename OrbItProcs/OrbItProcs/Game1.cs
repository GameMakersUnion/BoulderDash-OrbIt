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
        //transform,
        body,
        queuer,
        linearpull,
        maxvel,
        
        collision,
        gravity,
        fixedforce,
        fieldgravity,
        displace,
        orbiter,

        randcolor,
        randvelchange,
        randinitialvel,
        relativemotion,
        transfer,
        circler, //if this goes after maxvel instead, it should have an impact on circler.
        
        modifier,

        movement,
        
        hueshifter,
        lifetime,

        //draw components
        
        waver,
        laser,
        wideray,
        phaseorb,
        flow,

        tether,
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
        //public static int fullWidth = 1680;
        //public static int fullHeight = 1050;

        public static int fullWidth = 1920;
        public static int fullHeight = 1080;
        public static string filepath = "Presets//Nodes/";
        public static bool isFullScreen = false;
        public static bool TakeScreenshot = false;

        public Dictionary<textures, Texture2D> textureDict;
        //Node node;
        

        //public int worldWidth { get; set; }
        //public int worldHeight { get; set; }

        //string currentSelection = "placeNode";
        public Node targetNode = null;

        TimeSpan elapsedTimeUpdate = new TimeSpan();
        TimeSpan targetElapsedTimeUpdate = new TimeSpan(0, 0, 0, 0, 16);

        //TimeSpan elapsedTimeDraw = new TimeSpan();
        TimeSpan targetElapsedTimeDraw = new TimeSpan(0, 0, 0, 0, 16);

        public ObservableCollection<object> NodePresets = new ObservableCollection<object>();
        //public List<FileInfo> presetFileInfos = new List<FileInfo>();

        /////////////////////
        public Redirector redirector;
        public Testing testing;

        public bool TimeToDraw;

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

            TimeToDraw = false;

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
            {textures.blueorb, Content.Load<Texture2D>("Textures/bluesphere"        )},
            {textures.whiteorb, Content.Load<Texture2D>("Textures/whitesphere"      )},
            {textures.colororb, Content.Load<Texture2D>("Textures/colororb"         )},
            {textures.whitepixel, Content.Load<Texture2D>("Textures/whitepixel"     )},
            {textures.whitepixeltrans, Content.Load<Texture2D>("Textures/whitepixeltrans")},
            {textures.whitecircle, Content.Load<Texture2D>("Textures/whitecircle"   )}};
            font = Content.Load<SpriteFont>("Courier New");

            
            room = new Room(this, 1580, 1175);

            processManager = new ProcessManager(room);
            #region ///Default User props///
            Dictionary<dynamic, dynamic> userPr = new Dictionary<dynamic, dynamic>() {
                    { node.position, new Vector2(0, 0) },
                    { node.texture, textures.whitecircle },
                    //{ node.radius, 50 },
                    { comp.basicdraw, true },
                    { comp.collision, true },
                    { comp.movement, true },
                    //{ comp.maxvel, true },
                    //{ comp.randvelchange, true },
                    { comp.randinitialvel, true },
                    { comp.maxvel, true },
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
            Node.cloneObject(room.defaultNode, firstdefault);
            firstdefault.name = "[G0]0";
            firstdefault.IsDefault = true;

            Console.WriteLine((Double.Epsilon * 2) / 2);


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
                    { comp.basicdraw, true }, { node.texture, textures.whitecircle } };

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

            room.player1 = new Player();
            room.player1.body.pos = new Vector2(100, 100);
            processManager.processDict.Add(proc.axismovement, new AxisMovement(room.player1, 4));

            processManager.SetProcessKeybinds(ui.keyManager);
            ui.keyManager.addProcessKeyAction("exitgame", KeyCodes.Escape, OnPress: () => Exit());
            ui.keyManager.addProcessKeyAction("togglesidebar", KeyCodes.OemTilde, OnPress: ui.ToggleSidebar);
            ui.keyManager.addProcessKeyAction("screenshot", KeyCodes.PrintScreen, OnPress: TakeScreenShot);

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
            string name = "Screenshots//SS_" + date + "_#" + i + ".png";
            while (File.Exists(name))
            {
                i += 1;
                name = "Screenshots//SS_" + date + "_#" + i + ".png";

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

            //Console.WriteLine("Current Folder" + filepath);
            foreach (string file in Directory.GetFiles(filepath, "*.xml"))
            {
                //Console.WriteLine("Current Files" + filepath);
                //Console.WriteLine(file);
                Node presetnode = (Node)room.game.serializer.Deserialize(file);
                /*foreach (comp c in presetnode.comps.Keys.ToList())
                {
                    ((Component)presetnode.comps[c]).parent = presetnode;
                }
                presetnode.body.parent = presetnode;
                presetnode.collision.parent = presetnode;
                presetnode.movement.parent = presetnode;*/
                NodePresets.Add(presetnode);

                //NodePresets.Add((Node)room.serializer.Deserialize(file));
            }
            foreach (Node snode in NodePresets)
            {
                //Console.WriteLine("Presetname: {0}", snode.name);
            }
        }
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            //spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch = new SpriteBatch(Graphics.GraphicsDevice);
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

            ui.Update(gameTime);

            TimeToDraw = true;
        }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            /*
            if (!IsFixedTimeStep)
            {
                elapsedTimeDraw += gameTime.ElapsedGameTime;
                if (elapsedTimeDraw >= targetElapsedTimeDraw)
                {
                    //frameRateCounter.UpdateElapsed(elapsedTimeDraw);
                    elapsedTimeDraw = TimeSpan.Zero;
                }
                else
                {
                    //Manager.EndDraw();
                    //return;
                }
            }
            */

            //if (!TimeToDraw) return;

            Manager.BeginDraw(gameTime);
            base.Draw(gameTime);


            //BlendState bs = new BlendState();
            //
            ////bs.AlphaBlendFunction = BlendFunction.ReverseSubtract;
            ////bs.AlphaSourceBlend = bs.AlphaDestinationBlend = Blend.One;
            //
            //bs.AlphaBlendFunction = BlendState.AlphaBlend.AlphaBlendFunction;
            //bs.AlphaDestinationBlend = BlendState.AlphaBlend.AlphaDestinationBlend;
            //bs.AlphaSourceBlend = BlendState.AlphaBlend.AlphaSourceBlend;
            //bs.BlendFactor = BlendState.AlphaBlend.BlendFactor;
            //bs.ColorBlendFunction = BlendState.AlphaBlend.ColorBlendFunction;
            //bs.ColorDestinationBlend = BlendState.AlphaBlend.ColorDestinationBlend;
            //bs.ColorSourceBlend = BlendState.AlphaBlend.ColorSourceBlend;
            //bs.ColorWriteChannels = BlendState.AlphaBlend.ColorWriteChannels;
            //bs.ColorWriteChannels1 = BlendState.AlphaBlend.ColorWriteChannels1;
            //bs.ColorWriteChannels2 = BlendState.AlphaBlend.ColorWriteChannels2;
            //bs.ColorWriteChannels3 = BlendState.AlphaBlend.ColorWriteChannels3;
            //
            ////bs.ColorBlendFunction = BlendFunction.Max;
            //bs.ColorDestinationBlend = Blend.One;
            //bs.ColorSourceBlend = Blend.Zero;
            

            GraphicsDevice.Clear(Color.Black);
            //spriteBatch.Begin();
            //spriteBatch.Begin(SpriteSortMode.Deferred, bs, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);
            spriteBatch.Begin();

            room.Draw(spriteBatch);
            frameRateCounter.Draw(spriteBatch, font);

            //spriteBatch.Draw(whiteTexture, new Vector2(100, 100), null, Color.Black, 0, Vector2.Zero, new Vector2(10, 1), SpriteEffects.None, 0);

            

            spriteBatch.End();

            

            Manager.EndDraw();

            if (TakeScreenshot)
            {
                Screenshot(Manager.Graphics.GraphicsDevice);
                TakeScreenshot = false;
            }

            TimeToDraw = false;

            

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
                    Node.cloneObject(ui.spawnerNode, newNode);
                }
                else
                {
                    Node.cloneObject(ui.sidebar.ActiveDefaultNode, newNode);
                }
            }
            newNode.name = activegroup.Name + Node.nodeCounter;
            newNode.acceptUserProps(userProperties);
            //newNode.Collided += delegate(Dictionary<dynamic, dynamic> dict) { newNode.body.color = Color.Red; };
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
                newNode.comps[comp.lifetime].maxmseconds = lifetime;
                newNode.comps[comp.lifetime].immortal = false;
            }
            g.IncludeEntity(newNode);
            return newNode;
        }

        public void spawnNode(int worldMouseX, int worldMouseY)
        {
            Dictionary<dynamic, dynamic> userP = new Dictionary<dynamic, dynamic>() {
                                { node.position, new Vector2(worldMouseX,worldMouseY) },
            };
            spawnNode(userP);
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
            Node.cloneObject(ui.sidebar.inspectorArea.editNode, serializenode);
            room.game.serializer.Serialize(serializenode, filename);
            ui.game.NodePresets.Add(serializenode);
            };

            if (File.Exists(filename)){ //we must be overwriting, therefore don't update the live presetList
                PopUp.Prompt(ui, "OverWrite?", "O/W?",
                    delegate(bool c, object a) { if (c) {completeSave(); PopUp.Toast(ui, "Node was overridden"); } return true; });
            }
            else { PopUp.Toast(ui, "Node Saved"); completeSave(); }

        }

        internal void deletePreset(Node p)
        {
            Console.WriteLine("Deleting file: " + p);
            File.Delete(Game1.filepath + p.name + ".xml");
            NodePresets.Remove(p);
        }
    }
}
