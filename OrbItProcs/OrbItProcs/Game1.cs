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
using Component = OrbItProcs.Components.Component;
using Console = System.Console;
using sc = System.Console;

using OrbItProcs.Interface;
using OrbItProcs.Components;
using OrbItProcs.Processes;
using System.IO;
using System.Collections.ObjectModel;

namespace OrbItProcs
{

    public enum comp
    {
        transform,
        queuer,
        linearpull,
        movement,
        collision,
        gravity,
        randcolor,
        randvelchange,
        randinitialvel,
        transfer,
        circler, //if this goes after maxvel instead, it should have an impact on circler.
        maxvel,
        modifier,
        
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

        //lasertimers,
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
        # region /// Comp to Type Dictionary ///
        public static Dictionary<comp, Type> compTypes = new Dictionary<comp, Type>(){
            {comp.basicdraw,        typeof(BasicDraw)           },
            {comp.circler,          typeof(Circler)             },
            {comp.collision,        typeof(Collision)           },
            {comp.flow,             typeof(Flow)                },
            {comp.gravity,          typeof(Gravity)             },
            {comp.hueshifter,       typeof(HueShifter)          },
            {comp.laser,            typeof(Laser)               },
            {comp.lifetime,         typeof(Lifetime)            },
            {comp.linearpull,       typeof(LinearPull)          },
            {comp.maxvel,           typeof(MaxVel)              },
            {comp.modifier,         typeof(Modifier)            },
            {comp.movement,         typeof(Movement)            },
            {comp.phaseorb,         typeof(PhaseOrb)            },
            {comp.queuer,           typeof(Queuer)              },
            {comp.randcolor,        typeof(RandColor)           },
            {comp.randinitialvel,   typeof(RandInitialVel)      },
            {comp.randvelchange,    typeof(RandVelChange)       },
            {comp.tether,           typeof(Tether)              },
            {comp.transfer,         typeof(Transfer)            },
            {comp.transform,        typeof(Transform)           },
            {comp.tree,             typeof(Tree)                },
            {comp.waver,            typeof(Waver)               },
            {comp.wideray,          typeof(WideRay)             },
          //{comp.lasertimers,      typeof(LaserTimers)         },
          //{comp.middle,           typeof(MaxVel)              },
          //{comp.repel,            typeof(Repel)               },
          //{comp.siphon,           typeof(Siphon)              },
          //{comp.slow,             typeof(Slow)                }, 
          //{comp.weird,            typeof(Weird)               },
        };
        #endregion

        public static Component GenerateComponent(comp c)
        {
            Component component = (Component)Activator.CreateInstance(compTypes[c]);
            return component;
        }
        
        public UserInterface ui;
        public Room room;
        SpriteBatch spriteBatch;
        public SpriteFont font;
        FrameRateCounter frameRateCounter;

        public static Dictionary<Type, comp> compEnums;

        public static int sWidth = 1000;
        public static int sHeight = 600;
        public static string filepath = "Presets//Nodes/";

        public Dictionary<textures, Texture2D> textureDict;
        //Node node;
        

        public int worldWidth { get; set; }
        public int worldHeight { get; set; }

        //string currentSelection = "placeNode";
        public Node targetNode = null;

        TimeSpan elapsedTime = new TimeSpan();
        TimeSpan targetElapsedTime = new TimeSpan(0, 0, 0, 0, 16);

        public ObservableCollection<object> NodePresets = new ObservableCollection<object>();
        //public List<FileInfo> presetFileInfos = new List<FileInfo>();

        /////////////////////
        public Redirector redirector;
        public Testing testing;

        public Game1()
        {
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;

            worldWidth = 1580;
            //worldHeight = 960;
            worldHeight = 1175;

            Graphics.PreferredBackBufferWidth = sWidth;
            Graphics.PreferredBackBufferHeight = sHeight;

            ClearBackground = true;
            BackgroundColor = Color.White;
            ExitConfirmation = false;

            Manager.AutoUnfocus = false;

            compEnums = new Dictionary<Type, comp>();
            foreach (comp key in compTypes.Keys.ToList())
            {
                compEnums.Add(compTypes[key], key);
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
            {textures.whitecircle, Content.Load<Texture2D>("Textures/whitecircle"   )}};
            font = Content.Load<SpriteFont>("Courier New");

            room = new Room(this);

            #region ///Default User props///
            Dictionary<dynamic, dynamic> userPr = new Dictionary<dynamic, dynamic>() {
                    { node.position, new Vector2(0, 0) },
                    { node.texture, textures.whitecircle },
                    { node.radius, 50 },
                    { comp.basicdraw, true },
                    //{ comp.collision, false },
                    { comp.movement, true },
                    //{ comp.maxvel, true },
                    //{ comp.randvelchange, true },
                    { comp.randinitialvel, true },
                    { comp.maxvel, true },
                    //{ comp.gravity, false },
                    //{ comp.linearpull, true },
                    //{ comp.laser, true },
                    //{ comp.wideray, true },
                    //{ comp.hueshifter, true },
                    //{ comp.transfer, true },
                    //{ comp.phaseorb, false },
                    //{ comp.tree, true },
                    //{ comp.queuer, true },
                    //{ comp.flow, true },
                    //{ comp.waver, false },
                    //{ comp.tether, false },
                    
                };
            #endregion

            room.defaultNode = new Node(room, userPr);
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



            Group masterGroup = new Group(room.defaultNode, Name: room.defaultNode.name, Spawnable: false);
            room.masterGroup = masterGroup;

            Group generalGroup = new Group(room.defaultNode, parentGroup: masterGroup, Name: "General Groups", Spawnable: false);
            room.masterGroup.AddGroup(generalGroup.Name, generalGroup);

            Group linkGroup = new Group(room.defaultNode, parentGroup: masterGroup, Name: "Link Groups", Spawnable: false);
            room.masterGroup.AddGroup(linkGroup.Name, linkGroup);

            Group firstGroup = new Group(firstdefault, parentGroup: generalGroup);
            generalGroup.AddGroup(firstGroup.Name, firstGroup);

            
            Dictionary<dynamic, dynamic> userPropsTarget = new Dictionary<dynamic, dynamic>() {
                    { node.position, new Vector2(0, 0) },
                    { comp.basicdraw, true },
                    { comp.hueshifter, true },
                    { comp.phaseorb, false },
                    { node.texture, textures.whitecircle }
                };
            room.targetNodeGraphic = new Node(room, userPropsTarget);
            room.targetNodeGraphic.name = "TargetNodeGraphic";

            frameRateCounter = new FrameRateCounter(this);
            base.Initialize();

            testing = new Testing();

            ui = new UserInterface(this);
            //ui.sidebar.ActiveGroup = firstGroup;
            //room.masterGroup.UpdateComboBox();
            ui.sidebar.UpdateGroupComboBoxes();
            
            room.game.ui.sidebar.cbListPicker.ItemIndex = 2;
            InitializePresets();

            Movement movement = new Movement();
            movement.active = true;
            Console.WriteLine("::" + movement.active);

            
        }

        public void InitializePresets()
        {

            //Console.WriteLine("Current Folder" + filepath);
            foreach (string file in Directory.GetFiles(filepath, "*.xml"))
            {
                //Console.WriteLine("Current Files" + filepath);
                //Console.WriteLine(file);
                Node presetnode = (Node)room.serializer.Deserialize(file);
                foreach (comp c in presetnode.comps.Keys.ToList())
                {
                    ((Component)presetnode.comps[c]).parent = presetnode;
                }
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
            spriteBatch = new SpriteBatch(GraphicsDevice);
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
            base.Update(gameTime);
            if (!IsFixedTimeStep)
            {
                elapsedTime += gameTime.ElapsedGameTime;
                if (elapsedTime >= targetElapsedTime)
                {
                    frameRateCounter.UpdateElapsed(elapsedTime);
                    elapsedTime = TimeSpan.Zero;
                }
                else
                {
                    return;
                }
            }



            

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
        }
        public void spawnNode(Dictionary<dynamic, dynamic> userProperties, Action<Node> afterSpawnAction = null)
        {
            //
            //testing.TestOnClick();
            //testing.TestHashSet();
            //testing.WhereTest();
            //testing.ForLoops();
            //testing.ColorsTest();
            //

            Group activegroup = ui.sidebar.ActiveGroupFirst;
            //if (activegroup.Name.Equals("master")) return;
            if (!activegroup.Spawnable) return;
            Node newNode = new Node();
            if (ui.spawnerNode != null)
            {
                Node.cloneObject(ui.spawnerNode, newNode);
            }
            else
            {
                Node.cloneObject(ui.sidebar.ActiveDefaultNode, newNode);
            }
            newNode.name = activegroup.Name + Node.nodeCounter;

            newNode.acceptUserProps(userProperties);
            newNode.OnSpawn();
            if (afterSpawnAction != null) afterSpawnAction(newNode);
            

            //activegroup.entities.Add(newNode);
            activegroup.IncludeEntity(newNode);
            int Enumsize = Enum.GetValues(typeof(KnownColor)).Length;

            //int rand = Utils.random.Next(size - 1);
            int index = 0;
            foreach(char c in activegroup.Name.ToCharArray().ToList())
            {
                index += (int)c;
            }
            index = index % (Enumsize - 1);

            
            if (Group.IntToColor.ContainsKey(activegroup.GroupId))
            {
                newNode.transform.color = Group.IntToColor[activegroup.GroupId];
            }
            else
            {
                System.Drawing.Color syscolor = System.Drawing.Color.FromKnownColor((KnownColor)index);
                Color xnacol = new Color(syscolor.R, syscolor.G, syscolor.B, syscolor.A);
                newNode.transform.color = xnacol;
            }
        }
        public void spawnNode(int worldMouseX, int worldMouseY)
        {
            Dictionary<dynamic, dynamic> userP = new Dictionary<dynamic, dynamic>() {
                                { node.position, new Vector2(worldMouseX,worldMouseY) },
            };
            spawnNode(userP);
        }

        public void saveNode(Node node, string name)
        {
            name = name.Trim();
            string filename = "Presets//Nodes//" + name + ".xml";
            Action completeSave = delegate{
            ui.sidebar.inspectorArea.editNode.name = name;
            Node serializenode = new Node();
            Node.cloneObject(ui.sidebar.inspectorArea.editNode, serializenode);
                room.serializer.Serialize(serializenode, filename);
                ui.game.NodePresets.Add(serializenode);
            };

            if (File.Exists(filename)){ //we must be overwriting, therefore don't update the live presetList
                PopUp.Prompt(ui, "OverWrite?", "O/W?",
                    delegate(bool c, object a) { if (c) {completeSave(); PopUp.Toast(ui, "Node was overridden"); } return true; });
            }
            else { PopUp.Toast(ui, "Node Saved"); completeSave(); }

        }

        

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            
            Manager.BeginDraw(gameTime);
            base.Draw(gameTime);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            
            room.Draw(spriteBatch);
            frameRateCounter.Draw(spriteBatch, font);

            //spriteBatch.Draw(whiteTexture, new Vector2(100, 100), null, Color.Black, 0, Vector2.Zero, new Vector2(10, 1), SpriteEffects.None, 0);

            spriteBatch.End();

            Manager.EndDraw();


        }

        internal void deletePreset(Node p)
        {
            Console.WriteLine("Deleting file: " + p);
            File.Delete(Game1.filepath + p.name + ".xml");
            NodePresets.Remove(p);
        }
    }
}
