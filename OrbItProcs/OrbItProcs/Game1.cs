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

//core_active,
//core_position,
//core_velocity,
//core_velMultiplier,
//core_multiplier,
//core_effectiveRadius,
//core_radius,
//core_mass,
//core_scale,
//core_collidable,
//core_massive,
//core_texture,

using OrbItProcs.Interface;

namespace OrbItProcs {
    /// <summary>
    /// This is the main type for your game
    /// </summary>

    public enum comp {
        lasertimers,
        laser,
        wideray,
        randcolor,
        movement,
        randvelchange,
        randinitialvel,
        gravity,
        repel,
        middle,
        slow,
        transfer,
        siphon,
        ghost,
        chrono,
        weird,
        maxvel,
        linearpull,
        hueshifter,

    };


    public class Game1 : Application {
        //GraphicsDeviceManager graphics;
        //Neoforce Manager
        //private Manager manager;
        public UserInterface ui;
        public Room room;
        SpriteBatch spriteBatch;
        SpriteFont font;
        FrameRateCounter frameRateCounter;

        public int sWidth = 1000;
        public int sHeight = 600;

        MouseState oldMouseState;
        KeyboardState oldKeyBState;
        int oldMouseScrollValue = 0;
        public Texture2D blueTexture;
        public Texture2D whiteTexture;
        public Texture2D colororbTexture;
        public Texture2D whitepixelTexture;
        public Texture2D white3pixelTexture;
        //Node node;
        
        int rightClickCount = 0;
        int rightClickMax = 1;

        public int worldWidth = 1600;
        public int worldHeight = 960;

        string currentSelection = "placeNode";
        bool hovertargetting = false;
        public Node targetNode = null;

        /*
        Window window;
        Button button;
        TextBox textbox;
        */

        

        TimeSpan elapsedTime = new TimeSpan();
        TimeSpan targetElapsedTime = new TimeSpan(0, 0, 0, 0, 16);

        

        public Game1()
        {
            //graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            //Components.Add(new FrameRateCounter(this));
            IsMouseVisible = true;
            IsFixedTimeStep = false;
            //TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 50);
            
            Graphics.PreferredBackBufferWidth = sWidth;
            Graphics.PreferredBackBufferHeight = sHeight;
            //Graphics.SynchronizeWithVerticalRetrace = false;

            ClearBackground = true;
            BackgroundColor = Color.White;
            ExitConfirmation = false;

            Manager.AutoUnfocus = false;

            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            blueTexture = Content.Load<Texture2D>("bluesphere");
            whiteTexture = Content.Load<Texture2D>("whitesphere");
            colororbTexture = Content.Load<Texture2D>("colororb");
            whitepixelTexture = Content.Load<Texture2D>("whitepixel");
            white3pixelTexture = Content.Load<Texture2D>("white3pixel");
            font = Content.Load<SpriteFont>("Courier New");

            // TODO: Add your initialization logic here
            room = new Room(this);
            //node = new Node(room);
            frameRateCounter = new FrameRateCounter(this);
            //manager.Initialize();
            base.Initialize();

            

            ui = new UserInterface(this);

            //testing
            Dictionary<dynamic, dynamic> userP = new Dictionary<dynamic, dynamic>() {
                    { node.position, new Vector2(56, 76) },
                    //{ node.radius, 12 },
                    { node.collidable, true },
                    { comp.randcolor, true },
                    { comp.movement, true }, //this will default as 'true'
                    //{ comp.randvelchange, true },
                    //{ comp.randinitialvel, true },
                    { comp.gravity, true },
                    
                    //{ comp.transfer, true },
                    { node.texture, whiteTexture }
                };
            Node testnode = new Node(room, userP);


            testnode.position.X = 4; // works
            //testnode.vecttest.X = 5;
            
            // how can I directly modify the members of a certain property?
            //testnode.vecttest = new Vector2(5, testnode.vecttest.Y);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
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
            //frameRateCounter.Update(gameTime);

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

            //ProcessKeyboard();
            //ProcessMouse();

            

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
                    { node.texture, whiteTexture },
                    //{ node.radius, 12 },
                    //{ node.collidable, true },
                    { comp.randcolor, true },
                    { comp.movement, true }, //this will default as 'true'
                    //{ comp.randvelchange, true },
                    { comp.randinitialvel, true },
                    //{ comp.gravity, true },
                    
                    //{ comp.transfer, true },
                    //{ comp.lasertimers, true },
                    //{ comp.laser, true },
                    { comp.wideray, true },
                    { comp.hueshifter, true },
                };

            Node newNode = new Node();
            
            Node.cloneObject(room.defaultNode, newNode);
            newNode.position = new Vector2(worldMouseX, worldMouseY);
            //newNode.acceptUserProps(userP);

            //Node newNode = new Node(room, userP);

            

            newNode.name = "node" + Node.nodeCounter;

            


            ui.UpdateNodeList(newNode);

            //newNode.comps[comp.gravity].multiplier = 1000000f;
            Console.WriteLine(newNode.velocity);
            room.nodes.Add(newNode);
            //Console.WriteLine(newNode.comps[comp.randinitialvel].multiplier);
            Console.WriteLine("Nodes: {0}", room.nodes.Count);
        }
        /*
        public void ProcessKeyboard()
        {
            KeyboardState keybState = Keyboard.GetState();

            if (keybState.IsKeyDown(Keys.Y))
            {
                hovertargetting = true;
            }
            else
                hovertargetting = false;

            if (keybState.IsKeyDown(Keys.D1))
                currentSelection = "placeNode";
            if (keybState.IsKeyDown(Keys.T))
                currentSelection = "targeting";
            if (keybState.IsKeyDown(Keys.W) && !oldKeyBState.IsKeyDown(Keys.W))
            {
                ui.lstMain.ScrollTo(20);
            }
            
            


            if (keybState.IsKeyDown(Keys.F) && currentSelection.Equals("pause") && !oldKeyBState.IsKeyDown(Keys.F))
                currentSelection = "placeNode";
            else if (keybState.IsKeyDown(Keys.F) && !oldKeyBState.IsKeyDown(Keys.F))
                currentSelection = "pause";
            
            oldKeyBState = Keyboard.GetState();
        }

        

        public void ProcessMouse()
        {
            MouseState mouseState = Mouse.GetState();
            //ignore mouse clicks outside window
            if (mouseState.X >= sWidth || mouseState.X < 0 || mouseState.Y >= sHeight || mouseState.Y < 0)
                return;

            //make sure clicks inside the ui are ignored by game logic
            if (mouseState.X >= sWidth - ui.window.Width - 5)
            {
                if (mouseState.Y > ui.lstMain.Top + 24 && mouseState.Y < ui.lstMain.Top + ui.lstMain.Height + 24)
                {
                    if (mouseState.ScrollWheelValue < oldMouseState.ScrollWheelValue)
                    {
                        //ui.lstMain.
                        ui.lstMain_ChangeScrollPosition(4);

                    }
                    else if (mouseState.ScrollWheelValue > oldMouseState.ScrollWheelValue)
                    {
                        ui.lstMain_ChangeScrollPosition(-4);
                    }
                }
                if (mouseState.Y > ui.lstComp.Top + 24 && mouseState.Y < ui.lstComp.Top + ui.lstComp.Height + 24)
                {
                    if (mouseState.ScrollWheelValue < oldMouseState.ScrollWheelValue)
                    {
                        //ui.lstMain.
                        ui.lstComp_ChangeScrollPosition(4);

                    }
                    else if (mouseState.ScrollWheelValue > oldMouseState.ScrollWheelValue)
                    {
                        ui.lstComp_ChangeScrollPosition(-4);
                    }


                }

                oldMouseState = mouseState;
                return;
            }

            int worldMouseX = (int)(mouseState.X * room.mapzoom);
            int worldMouseY = (int)(mouseState.Y * room.mapzoom);

            if (currentSelection.Equals("placeNode"))
            {
                if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                {
                    //new node
                    spawnNode(worldMouseX, worldMouseY);
                    
                }
                // rapid placement of nodes
                if (mouseState.RightButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
                {

                    if (rightClickCount > rightClickMax)
                    {
                        //new node(s)
                        int rad = 100;
                        for (int i = 0; i < 10; i++)
                        {
                            int rx = Utils.random.Next(rad * 2) - rad;
                            int ry = Utils.random.Next(rad * 2) - rad;
                            spawnNode(worldMouseX + rx, worldMouseY + ry);
                        }

                        rightClickCount = 0;
                    }
                    else
                    {
                        rightClickCount++;
                    }

                }
            }
            else if (currentSelection.Equals("targeting"))
            {
                if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                {
                    bool found = false;
                    for (int i = room.nodes.Count-1; i >= 0; i--)
                    {
                        Node n = (Node)room.nodes.ElementAt(i);
                        // find node that has been clicked, starting from the most recently placed nodes
                        if (Vector2.DistanceSquared(n.position, new Vector2(worldMouseX, worldMouseY)) < n.Radius * n.Radius)
                        {
                            //--
                            room.nodes.Remove(n);
                            break;
                            //--
                            targetNode = n;
                            found = true;
                            break;
                        }
                    }
                    if (!found) targetNode = null;
                }
            }


            if (hovertargetting)
            {
                if (true || mouseState.LeftButton == ButtonState.Pressed)
                {
                    bool found = false;
                    for (int i = room.nodes.Count - 1; i >= 0; i--)
                    {
                        Node n = (Node)room.nodes.ElementAt(i);
                        // find node that has been clicked, starting from the most recently placed nodes
                        if (Vector2.DistanceSquared(n.position, new Vector2(worldMouseX, worldMouseY)) < n.Radius * n.Radius)
                        {
                            targetNode = n;
                            found = true;
                            break;
                        }
                    }
                    if (!found) targetNode = null;
                }
            }

            if (mouseState.RightButton == ButtonState.Released && oldMouseState.RightButton == ButtonState.Pressed)
            {
                rightClickCount = 0;
            }

            if (mouseState.ScrollWheelValue < oldMouseScrollValue)
            {
                room.mapzoom += 0.2f;
            }
            else if (mouseState.ScrollWheelValue > oldMouseScrollValue)
            {
                room.mapzoom -= 0.2f;
            }

            oldMouseScrollValue = mouseState.ScrollWheelValue;
            oldMouseState = mouseState;
        }
        */

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            
            Manager.BeginDraw(gameTime);

            //Manager.Draw(gameTime);

            base.Draw(gameTime);

            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            room.Draw(spriteBatch);
            frameRateCounter.Draw(spriteBatch, font);

            //spriteBatch.Draw(whiteTexture, new Vector2(100, 100), null, Color.Black, 0, Vector2.Zero, new Vector2(10, 1), SpriteEffects.None, 0);

            spriteBatch.End();

            Manager.EndDraw();


        }
    }
}
