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

        string currentSelection = "placeNode";
        public Node targetNode = null;

        TimeSpan elapsedTime = new TimeSpan();
        TimeSpan targetElapsedTime = new TimeSpan(0, 0, 0, 0, 16);

        public ObservableCollection<object> NodePresets = new ObservableCollection<object>();
        //public List<FileInfo> presetFileInfos = new List<FileInfo>();

        /////////////////////
        public Redirector redirector;

        public Game1()
        {
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;

            worldWidth = 1600;
            worldHeight = 960;
            
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
                    { comp.waver, false },
                    { comp.tether, true },
                    
                };
            #endregion

            room.defaultNode = new Node(room, userPr);
            room.defaultNode.name = "master";

            //much faster than foreach keyword apparently. Nice
            room.defaultNode.comps.Keys.ToList().ForEach(delegate(comp c) 
            {
                room.defaultNode.comps[c].AfterCloning();
            });

            Node firstdefault = new Node();
            Node.cloneObject(room.defaultNode, firstdefault);
            firstdefault.name = "first";

            Group masterGroup = new Group(room.defaultNode, Name: room.defaultNode.name);
            room.masterGroup = masterGroup;
            //room.groups.Add(masterGroup.Name, masterGroup);
            Group firstGroup = new Group(firstdefault, parentGroup: masterGroup , Name: firstdefault.name);
            room.masterGroup.AddGroup(firstGroup.Name, firstGroup, false);
            //room.groups.Add(firstGroup.Name, firstGroup);

            
            //////////////////////////////////////////////////////////////////////////////////////
            List<int> ints = new List<int> { 1, 2, 3 };
            ints.ForEach(delegate(int i) { if (i==2) ints.Remove(i); }); //COOL: NO ENUMERATION WAS MODIFIED ERROR
            ints.ForEach(delegate(int i) { Console.WriteLine(i); });

            MethodInfo testmethod = room.GetType().GetMethod("test");
            Action<Room, int, float, string> del = (Action<Room, int, float, string>)Delegate.CreateDelegate(typeof(Action<Room, int, float, string>), testmethod);
            del(room, 1, 0.3f, "Action worked.");

            Action<int, float, string> del2 = (Action<int, float, string>)Delegate.CreateDelegate(typeof(Action< int, float, string>), room, testmethod);
            //target is bound to 'room' in this example due to the overload of CreateDelegate used.
            del2(2, 3.3f, "Action worked again.");

            PropertyInfo pinfo = typeof(Component).GetProperty("active");
            MethodInfo minfo = pinfo.GetGetMethod();
            Console.WriteLine("{0}", minfo.ReturnType);

            Movement tester = new Movement();
            tester.active = true;

            bool ret = (bool)minfo.Invoke(tester, new object[] { }); //VERY expensive (slow)
            Console.WriteLine("{0}", ret);

            

            Func<Component, bool> delGet = (Func<Component, bool>)Delegate.CreateDelegate(typeof(Func<Component, bool>), minfo);
            Console.WriteLine("{0}", delGet(tester)); //very fast, and no cast or creation of empty args array required

            minfo = pinfo.GetSetMethod();
            //Console.WriteLine("{0} {1}", minfo.ReturnType, minfo.GetParameters()[0].ParameterType);

            Action<Component, bool> delSet = (Action<Component, bool>)Delegate.CreateDelegate(typeof(Action<Component, bool>), minfo);
            delSet(tester, false);
            Console.WriteLine("Here we go: {0}", delGet(tester));
            delSet(tester, true);
            /////////////////////////////////////////////////////////////////////////////////////////
            /*
            //gets all types that are a subclass of Component
            List<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(assembly => assembly.GetTypes())
                       .Where(type => type.IsSubclassOf(typeof(Component))).ToList();
            foreach (Type t in types) Console.WriteLine(t);
            */

            //room.defaultNode.Update(new GameTime()); //for testing

            //MODIFIER ADDITION
            /*
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
            //ui.sidebar.ActiveGroup = firstGroup;
            room.masterGroup.UpdateComboBox();
            room.game.ui.sidebar.cmbListPicker.ItemIndex = 1;
            InitializePresets();

            Movement movement = new Movement();
            movement.active = true;
            Console.WriteLine("::" + movement.active);

            Redirector.PopulateDelegatesAll();
            redirector = new Redirector();
            //redirector.PopulateDelegatesAll();
            //Console.WriteLine(redirectior.setters[typeof(Movement)]["pushable"].GetType());
            //redirectior.setters[typeof(Movement)]["pushable"](movement, false);
            

        }

        public void InitializePresets()
        {

            //Console.WriteLine("Current Folder" + filepath);
            foreach (string file in Directory.GetFiles(filepath,"*.xml"))
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

            ui.Update(gameTime);

            if (!currentSelection.Equals("pause"))
                room.Update(gameTime);
            else
            {
                room.colorEffectedNodes();
            }
        }
        public void spawnNode(Dictionary<dynamic, dynamic> userProperties)
        {
            //testing to see how long it takes to generate all the getter/setter delegates
            
            object transformobj = room.defaultNode.transform;
            dynamic nodedynamic = room.defaultNode;
            List<Func<Node, float>> delList = new List<Func<Node, float>>();
            float total = 0;
            MethodInfo minfo = typeof(Transform).GetProperty("mass").GetGetMethod();
            Func<Transform, float> getDel = (Func<Transform, float>)Delegate.CreateDelegate(typeof(Func<Transform, float>), minfo);
            
            DateTime dt = DateTime.Now;
            Movement movement = new Movement();

            //redirector.TargetObject = movement;
            //redirector.PropertyToObject["active"] = movement;
            redirector.AssignObjectToPropertiesAll(movement);
            PropertyInfo pinfo = movement.GetType().GetProperty("active");
            //Action<object, object> movementsetter = redirector.setters[typeof(Movement)]["active"];
            //Console.WriteLine(":::" + movement.active);
            //bool a = redirector.active;
            bool a = false;
            for(int i = 0; i < 100000; i++)
            {
                //if (i > 0) if (i > 1) if (i > 2) if (i > 3) if (i > 4) total++;
                
                //delList.Add(getDel);
                //float slow = (float)minfo.Invoke((Transform)transformobj, new object[] { });
                //float mass = getDel(room.defaultNode);
                //float mass2 = getDel((Transform)transformobj); //doesn't work because it's of type Object at compile time
                //float mass2 = getDel(nodedynamic);
                //total += mass;
                //gotten = room.defaultNode.GetComponent<Movement>(); //generic method to grab components
                //gotten = room.defaultNode.comps[comp.movement];
                //bool act = gotten.active;
                //gotten.active = true;
                //redirector.active = false; //21m(impossible)... 24m(new) ... 19m (newer) ... 16m(newest)
                //a = redirector.active;
                //pinfo.SetValue(movement, false, null); //34m
                //movementsetter(movement, false); //4m(old)......... 6m(new)
                //movement.active = false;

            }
            //Movement move = room.defaultNode.comps[comp.movement];

            int mill = DateTime.Now.Millisecond - dt.Millisecond;
            if (mill < 0) mill += 1000;
            Console.WriteLine("{0} - {1} = {2}",DateTime.Now.Millisecond,dt.Millisecond, mill);
            //Console.WriteLine(total);
            /* //this code won't run right now, but it represents the ability to make a specific generic method based on type variables from another generic method, and then invoke it... (this is slow)
            MethodInfo method = GetType().GetMethod("DoesEntityExist")
                             .MakeGenericMethod(new Type[] { typeof(Type) });
            method.Invoke(this, new object[] { dt, mill });
            */

            //gotten.fallOff();

            /////////////////////////////////////////////////////////////////////////////
            Group activegroup = ui.sidebar.ActiveGroup;
            if (activegroup.Name.Equals("master")) return;
            Node newNode = new Node();
            if (ui.spawnerNode != null)
            {
                Node.cloneObject(ui.spawnerNode, newNode);
            }
            else
            {
                Node.cloneObject(ui.sidebar.ActiveDefaultNode, newNode);
            }
            newNode.acceptUserProps(userProperties);
            newNode.OnSpawn();

            newNode.name = activegroup.Name + Node.nodeCounter;

            //activegroup.entities.Add(newNode);
            activegroup.IncludeEntity(newNode);
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

                    PopUp.Toast(ui, "Node was overridden");


                }
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
