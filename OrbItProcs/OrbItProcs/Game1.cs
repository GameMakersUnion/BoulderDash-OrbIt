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

using Component = OrbItProcs.Components.Component;
using Console = System.Console;
using sc = System.Console; // use this

using OrbItProcs.Interface;
using OrbItProcs.Components;
using OrbItProcs.Processes;
using System.IO;
using System.Collections.ObjectModel;

namespace OrbItProcs
{

    public enum comp
    {
        queuer,
        movement,
        collision,
        gravity,
        randcolor,
        randvelchange,
        randinitialvel,
        transfer,
        maxvel,
        modifier,
        linearpull,
        hueshifter,
        lifetime,

        //draw components
        flow,
        waver,
        laser,
        wideray,
        phaseorb,
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
            {comp.transfer,         typeof(Transfer)            },
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
        
        private UserInterface ui;
        public Room room;
        SpriteBatch spriteBatch;
        public SpriteFont font;
        FrameRateCounter frameRateCounter;

        public static int sWidth = 1000;
        public static int sHeight = 600;
        public static string filepath = "Presets//Nodes/";

        public Dictionary<textures, Texture2D> textureDict;
        //Node node;

        public int worldWidth = 1600;
        public int worldHeight = 960;

        string currentSelection = "placeNode";
        public Node targetNode = null;

        TimeSpan elapsedTime = new TimeSpan();
        TimeSpan targetElapsedTime = new TimeSpan(0, 0, 0, 0, 16);

        public ObservableCollection<object> NodePresets = new ObservableCollection<object>();
        //public List<FileInfo> presetFileInfos = new List<FileInfo>();


        /////////////////////


        public Game1()
        {
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
            
            Graphics.PreferredBackBufferWidth = sWidth;
            Graphics.PreferredBackBufferHeight = sHeight;

            ClearBackground = true;
            BackgroundColor = Color.White;
            ExitConfirmation = false;

            Manager.AutoUnfocus = false;

            
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
                    //{ comp.randinitialvel, true },
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
                    //{ comp.waver, true },
                    
                };
            #endregion

            room.defaultNode = new Node(room, userPr);
            room.defaultNode.name = "DEFAULTNODE";

            /*MODIFIER ADDITION

            room.defaultNode.addComponent(comp.modifier, true); //room.defaultNode.comps[comp.modifier].active = false;
            ModifierInfo modinfo = new ModifierInfo();
            modinfo.AddFPInfoFromString("o1", "scale", room.defaultNode);
            modinfo.AddFPInfoFromString("m1", "position", room.defaultNode);
            modinfo.AddFPInfoFromString("v1", "position", room.defaultNode);

            modinfo.args.Add("mod", 4.0f);
            modinfo.args.Add("times", 3.0f);
            modinfo.args.Add("test", 3.0f);
            
            //modinfo.delegateName = "Mod";
            //modinfo.delegateName = "Triangle";
            //modinfo.delegateName = "VelocityToOutput";
            //modinfo.delegateName = "VectorSine";
            modinfo.delegateName = "VectorSineComposite";

            room.defaultNode.comps[comp.modifier].modifierInfos["sinecomposite"] = modinfo;
            */

            
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


            ui = new UserInterface(this);
            InitializePresets();
        }

        public void InitializePresets()
        {

            System.Console.WriteLine("Current Folder" + filepath);
            foreach (string file in Directory.GetFiles(filepath,"*.xml"))
            {
                System.Console.WriteLine("Current Files" + filepath);
                System.Console.WriteLine(file);
                NodePresets.Add((Node)room.serializer.Deserialize(file));
            }
            foreach (Node snode in NodePresets)
            {
                System.Console.WriteLine("Presetname: {0}", snode.name);
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

            ui.Update(gameTime);

            if (!currentSelection.Equals("pause"))
                room.Update(gameTime);
            else
            {
                room.colorEffectedNodes();
            }
        }

        public void spawnNode(int worldMouseX, int worldMouseY)
        {
            Dictionary<dynamic, dynamic> userP = new Dictionary<dynamic, dynamic>() {
                    { node.position, new Vector2(worldMouseX, worldMouseY) },
                    { node.texture, textures.whiteorb },
                    //{ node.radius, 12 },
                    //{ comp.randcolor, true },
                    { comp.basicdraw, true },
                    { comp.movement, true }, //this will default as 'true'
                    //{ comp.randvelchange, true },
                    //{ comp.randinitialvel, true },
                    //{ comp.gravity, true },
                    
                    //{ comp.transfer, true },
                    //{ comp.lasertimers, true },
                    //{ comp.laser, true },
                    //{ comp.wideray, true },
                    //{ comp.hueshifter, true },
                };

            Node newNode = new Node();
            if (ui.spawnerNode != null)
            {
                Node.cloneObject(ui.spawnerNode, newNode);

            }
            else
            {
                Node.cloneObject(room.defaultNode, newNode);
            }
            
            newNode.position = new Vector2(worldMouseX, worldMouseY);
            //newNode.acceptUserProps(userP);

            //Node newNode = new Node(room, userP);

            newNode.name = "node" + Node.nodeCounter;
            //if (newNode.comps.ContainsKey(comp.modifier)) newNode.comps[comp.modifier].UpdateReferences();

            //newNode.comps[comp.gravity].multiplier = 1000000f;
            //Console.WriteLine(newNode.velocity);
            room.nodes.Add(newNode);
            //Console.WriteLine(newNode.comps[comp.randinitialvel].multiplier);
            //Console.WriteLine("Nodes: {0}", room.nodes.Count);
            //ui.sidebar.UpdateNodesTitle();
        }
        public void saveNode(Node node, string name)
        {
            bool updatePresetList = true;
            name = name.Trim();
            ui.editNode.name = name;
            List<string> filesWithName = Directory.GetFiles(filepath, name + ".xml").ToList();
            if (filesWithName.Count > 0) //we must be overwriting, therefore don't update the live presetList
                updatePresetList = false;

            string filename = "Presets//Nodes//" + name + ".xml";
            Node serializenode = new Node();
            Node.cloneObject(ui.editNode, serializenode);

            room.serializer.Serialize(serializenode, filename);
            if (updatePresetList)
                foreach (string file in Directory.GetFiles(filepath, name + ".xml"))
            {
                    ui.game.NodePresets.Add((Node)ui.room.serializer.Deserialize(file));
                    break;
            }
            foreach (Node preset in ui.game.NodePresets)
                if (preset.name == name)
                    {
                        throw new NotImplementedException();
                     //PopupWindow.Prompt(ui,"A preset already has that name\nOverwrite anyways?");
            
                
                    }
                    }

        public void spawnNode(Dictionary<dynamic, dynamic> userProperties)
            {
            Node newNode = new Node();
            if (ui.spawnerNode != null)
                {
                Node.cloneObject(ui.spawnerNode, newNode);
                    }
                    else
                    {
                Node.cloneObject(room.defaultNode, newNode);
                    }
            newNode.acceptUserProps(userProperties);
            newNode.name = "node" + Node.nodeCounter;
            room.nodes.Add(newNode);
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
            System.Console.WriteLine("Deleting file: " + p);
            File.Delete(Game1.filepath + p.name + ".xml");
            NodePresets.Remove(p);
        }
    }
}
