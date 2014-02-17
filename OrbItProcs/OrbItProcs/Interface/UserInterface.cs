using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TomShane.Neoforce.Controls;
using System.Reflection;


using Component = OrbItProcs.Component;
using System.IO;

namespace OrbItProcs {
    public class UserInterface {

        public enum selection
        {
            placeNode,
            targetSelection,
            groupSelection,
            randomNode,
        }

        public static Vector2 MousePos;
        public static Vector2 WorldMousePos;

        #region /// Fields ///

        public Game1 game;
        public Room room;

        public KeyManager keyManager;

        
        public static KeyboardState keybState, oldKeyBState;
        public static MouseState mouseState, oldMouseState;
        
        //public string currentSelection = "placeNode";//
        public selection currentSelection = selection.placeNode;
        int oldMouseScrollValue = 0;//
        bool hovertargetting = false;//
        int rightClickCount = 0;//
        int rightClickMax = 1;//
        public int sWidth = 1000;////
        public int sHeight = 600;////
        bool isShiftDown = false;
        bool isTargeting = false;
        public Vector2 spawnPos;
        Vector2 groupSelectionBoxOrigin = new Vector2(0, 0);
        public HashSet<Node> groupSelectSet;

        #endregion

        public float zoomfactor { get; set; }
        public bool GameInputDisabled { get; set; }
        public bool IsPaused { get; set; }

        public Dictionary<dynamic, dynamic> UserProps;

        public Node spawnerNode;
        public Sidebar sidebar;

        public UserInterface(Game1 game)
        {
            this.game = game;
            this.room = game.room;
            
            sidebar = new Sidebar(this);
            sidebar.Initialize();
            zoomfactor = 0.9f;
            GameInputDisabled = false;
            IsPaused = false;
            this.keyManager = new KeyManager(this);
            


            groupSelectSet = (game.processManager.processDict[proc.groupselect] as GroupSelect).groupSelectSet; //syncs group select set to process set
        }

        public void Update(GameTime gameTime)
        {
            ProcessKeyboard();
            ProcessMouse();

            //game.testing.KeyManagerTest(() => Keybindset.Update());
            keyManager.Update();

            //randomizerProcess = new Randomizer();
            
        }

        public void ProcessKeyboard()
        {
            keybState = Keyboard.GetState();

            if (GameInputDisabled) return;
            //game.processManager.PollKeyboard();

            if (keybState.IsKeyDown(Keys.Y))
            {
                hovertargetting = true;
            }
            else
                hovertargetting = false;


            if (keybState.IsKeyDown(Keys.Space) && oldKeyBState.IsKeyUp(Keys.Space))
            {
                room.Update(null);
                System.Console.WriteLine(room.nodeHashes.Count);
            }

            if (keybState.IsKeyDown(Keys.LeftShift))
            {
                if (!isShiftDown)
                { 
                    MouseState ms = Mouse.GetState();
                    spawnPos = new Vector2(ms.X * room.mapzoom, ms.Y * room.mapzoom);
                }
                isShiftDown = true;
            }
            else
            {
                isShiftDown = false;
            }

            if (keybState.IsKeyDown(Keys.F) && !oldKeyBState.IsKeyDown(Keys.F))
                IsPaused = !IsPaused;

            oldKeyBState = Keyboard.GetState();
        }

        public Node SelectNode(Vector2 pos)
        {
            Node found = null;
            float shortedDistance = Int32.MaxValue;
            for (int i = room.masterGroup.fullSet.Count - 1; i >= 0; i--)
            {
                Node n = (Node)room.masterGroup.fullSet.ElementAt(i);
                // find node that has been clicked, starting from the most recently placed nodes
                float distsquared = Vector2.DistanceSquared(n.body.pos, pos);
                if (distsquared < n.body.radius * n.body.radius)
                {
                    if (distsquared < shortedDistance)
                    {
                        found = n;
                        shortedDistance = distsquared;
                    }

                }
            }
            return found;
        }

        public void ProcessMouse()
        {
            mouseState = Mouse.GetState();

            if (mouseState.XButton1 == ButtonState.Pressed)
                System.Console.WriteLine("X1");

            if (mouseState.XButton2 == ButtonState.Pressed)
                System.Console.WriteLine("X2");

            MousePos = new Vector2(mouseState.X, mouseState.Y);
            WorldMousePos = MousePos * room.mapzoom;
            //ignore mouse clicks outside window
            if (mouseState.X >= sWidth || mouseState.X < 0 || mouseState.Y >= sHeight || mouseState.Y < 0)
                return;

            //make sure clicks inside the ui are ignored by game logic
            if (mouseState.X >= sWidth - sidebar.Width - 5)
            {
                if (mouseState.Y > sidebar.lstMain.Top + 24 && mouseState.Y < sidebar.lstMain.Top + sidebar.lstMain.Height + 24)
                {
                    if (mouseState.ScrollWheelValue < oldMouseState.ScrollWheelValue)
                    {
                        sidebar.lstMain_ChangeScrollPosition(1);
                        
                    }
                    else if (mouseState.ScrollWheelValue > oldMouseState.ScrollWheelValue)
                    {
                        sidebar.lstMain_ChangeScrollPosition(-1);
                        
                    }
                }

                sidebar.inspectorArea.ScrollInsBox(mouseState, oldMouseState);
                sidebar.insArea2.ScrollInsBox(mouseState, oldMouseState);

                oldMouseState = mouseState;
                return;
            }

            if (GameInputDisabled) return;
            //game.processManager.PollMouse(mouseState, oldMouseState);


            int worldMouseX = (int)WorldMousePos.X;
            int worldMouseY = (int)WorldMousePos.Y;

            
            if (hovertargetting)
            {
                if (true || mouseState.LeftButton == ButtonState.Pressed)
                {
                    bool found = false;
                    for (int i = room.masterGroup.fullSet.Count - 1; i >= 0; i--)
                    {
                        Node n = (Node)room.masterGroup.fullSet.ElementAt(i);
                        // find node that has been clicked, starting from the most recently placed nodes
                        if (Vector2.DistanceSquared(n.body.pos, new Vector2(worldMouseX, worldMouseY)) < n.body.radius * n.body.radius)
                        {
                            game.targetNode = n;
                            found = true;
                            break;
                        }
                    }
                    if (!found) game.targetNode = null;
                }
            }

            if (mouseState.RightButton == ButtonState.Released && oldMouseState.RightButton == ButtonState.Pressed)
            {
                rightClickCount = 0;
            }

            if (mouseState.ScrollWheelValue < oldMouseScrollValue)
            {
                room.mapzoom /= zoomfactor;
            }
            else if (mouseState.ScrollWheelValue > oldMouseScrollValue)
            {
                room.mapzoom *= zoomfactor;
            }

            oldMouseScrollValue = mouseState.ScrollWheelValue;
            oldMouseState = mouseState;
        }
    }
}