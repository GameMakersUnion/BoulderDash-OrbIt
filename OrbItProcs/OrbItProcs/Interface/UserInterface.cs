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

        public bool SidebarActive { get; set; }

        public Node spawnerNode;
        public Sidebar sidebar;

        public UserInterface(Game1 game)
        {
            this.game = game;
            this.room = game.room;
            SidebarActive = true;
            
            sidebar = new Sidebar(this);
            sidebar.Initialize();
            zoomfactor = 0.9f;
            GameInputDisabled = false;
            IsPaused = false;
            this.keyManager = new KeyManager(this);
            
            groupSelectSet = (game.processManager.processDict[proc.groupselect] as GroupSelect).groupSelectSet; //syncs group select set to process set
        }

        public void SetSidebarActive(bool active)
        {
            if (active)
            {
                sidebar.master.Visible = true;
                sidebar.master.Enabled = true;
            }
            else
            {
                sidebar.master.Visible = false;
                sidebar.master.Enabled = false;
            }
            SidebarActive = active;
        }

        public void ToggleSidebar()
        {
            if (SidebarActive)
            {
                sidebar.master.Visible = false;
                sidebar.master.Enabled = false;
            }
            else
            {
                sidebar.master.Visible = true;
                sidebar.master.Enabled = true;
            }
            SidebarActive = !SidebarActive;
        }

        public void Update(GameTime gameTime)
        {
            ProcessKeyboard();
            ProcessMouse();
            ProcessController();

            //game.testing.KeyManagerTest(() => Keybindset.Update());
            keyManager.Update();

            //randomizerProcess = new Randomizer();
            
        }

        public void ProcessKeyboard()
        {
            keybState = Keyboard.GetState();

            if (GameInputDisabled) return;

            if (keybState.IsKeyDown(Keys.Y))
                hovertargetting = true;
            else
                hovertargetting = false;


            if (keybState.IsKeyDown(Keys.Space) && oldKeyBState.IsKeyUp(Keys.Space))
            {
                room.Update(null);
            }

            if (keybState.IsKeyDown(Keys.LeftShift))
            {
                if (!isShiftDown)
                { 
                    MouseState ms = Mouse.GetState();
                    spawnPos = new Vector2(ms.X, ms.Y) / room.zoom;
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

        public void ProcessController()
        {
            //GamePad.SetVibration(PlayerIndex.Two, 0.1f, 0.9f);
            //System.Console.WriteLine(GamePad.GetState(PlayerIndex.Two).ThumbSticks.Left.X);
            //GraphData.AddFloat(GamePad.GetState(PlayerIndex.Two).ThumbSticks.Left.X * 10);
        }

        public Action<int> ScrollAction;

        public void SetScrollableControl(Control control, Action<int> action)
        {
            if (control == null || action == null) return;
            control.MouseOver += (s, e) => {
                ScrollAction = action;
            };
            control.MouseOut += (s, e) =>
            {
                ScrollAction = null;
            };
        }

        public void ProcessMouse()
        {
            mouseState = Mouse.GetState();

            //if (mouseState.XButton1 == ButtonState.Pressed)
            //    System.Console.WriteLine("X1");
            //
            //if (mouseState.XButton2 == ButtonState.Pressed)
            //    System.Console.WriteLine("X2");

            MousePos = new Vector2(mouseState.X, mouseState.Y);
            WorldMousePos = (MousePos / room.zoom) + room.camera.pos;
            //ignore mouse clicks outside window
            if (!Game1.isFullScreen)
            {
                if (mouseState.X >= sWidth || mouseState.X < 0 || mouseState.Y >= sHeight || mouseState.Y < 0)
                    return;
            }

            if (!keyManager.MouseInGameBox)
            {
                if (ScrollAction != null)
                {
                    if (mouseState.ScrollWheelValue < oldMouseState.ScrollWheelValue)
                    {
                        ScrollAction(1);
                    }
                    else if (mouseState.ScrollWheelValue > oldMouseState.ScrollWheelValue)
                    {
                        ScrollAction(-1);
                    }
                }

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
                room.zoom *= zoomfactor;
            }
            else if (mouseState.ScrollWheelValue > oldMouseScrollValue)
            {
                room.zoom /= zoomfactor;
            }

            oldMouseScrollValue = mouseState.ScrollWheelValue;
            oldMouseState = mouseState;
        }
    }
}