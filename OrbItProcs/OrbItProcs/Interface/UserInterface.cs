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

using OrbItProcs;
using OrbItProcs.Processes;

using Component = OrbItProcs.Components.Component;
using System.IO;

namespace OrbItProcs.Interface {
    public class UserInterface {

        public enum selection
        {
            placeNode,
            targetSelection,
            groupSelection,
            randomNode,
        }

        #region /// Fields ///

        public Game1 game;
        public Room room;

        public KeybindSet Keybindset;
        
        KeyboardState oldKeyBState;
        MouseState oldMouseState;
        
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
        public HashSet<Node> groupSelectSet = new HashSet<Node>();

        #endregion

        public float zoomfactor { get; set; }
        public bool GameInputDisabled { get; set; }
        public bool IsPaused { get; set; }

        public Dictionary<dynamic, dynamic> UserProps;

        //public Node editNode;
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
            this.Keybindset = new KeybindSet(this);
        }

        public void Update(GameTime gameTime)
        {
            ProcessKeyboard();
            ProcessMouse();

            Keybindset.Update();

        }

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
                currentSelection = selection.placeNode;
            if (keybState.IsKeyDown(Keys.Q))
                currentSelection = selection.targetSelection;
            if (keybState.IsKeyDown(Keys.W))
                currentSelection = selection.groupSelection;
            if (keybState.IsKeyDown(Keys.E))
                currentSelection = selection.randomNode;

            if (keybState.IsKeyDown(Keys.Space) && oldKeyBState.IsKeyUp(Keys.Space))
            {
                room.Update(null);
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

        public void ProcessMouse()
        {
            MouseState mouseState = Mouse.GetState();
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

            int worldMouseX = (int)(mouseState.X * room.mapzoom);
            int worldMouseY = (int)(mouseState.Y * room.mapzoom);


            if (currentSelection == selection.placeNode)
            {
                if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                {
                    //new node
                    
                        game.spawnNode(worldMouseX, worldMouseY);
                    

                }
                // rapid placement of nodes
                if (mouseState.RightButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
                {
                    if (isShiftDown)
                    {
                        rightClickCount++;
                        if (rightClickCount % rightClickMax == 0)
                        {
                            //Vector2 positionToSpawn = new Vector2(Game1.sWidth, Game1.sHeight);
                            Vector2 positionToSpawn = spawnPos;
                            //positionToSpawn /= (game.room.mapzoom * 2);
                            //positionToSpawn /= (2);
                            Vector2 diff = new Vector2(mouseState.X, mouseState.Y);
                            diff *= room.mapzoom;
                            diff = diff - positionToSpawn;
                            //diff.Normalize();

                            //new node(s)
                            Dictionary<dynamic, dynamic> userP = new Dictionary<dynamic, dynamic>() {
                                
                                //{ node.texture, textures.whitecircle },
                                //{ node.radius, 12 },
                                { comp.randcolor, true },
                                { comp.movement, true },
                                //{ comp.randvelchange, true },
                                { comp.randinitialvel, true },
                                //{ comp.gravity, false },
                                { comp.lifetime, true },
                                //{ comp.transfer, true },
                                //{ comp.lasertimers, true },
                                //{ comp.laser, true },
                                //{ comp.wideray, true },
                                //{ comp.hueshifter, true },
                                //{ comp.phaseorb, true },
                                //{ comp.collision, false },
                                { node.position, positionToSpawn },
                                { node.velocity, diff },
                            };

                            if (oldKeyBState.IsKeyDown(Keys.LeftControl))
                            {
                                Action<Node> after = delegate(Node n) { n.transform.velocity = diff; }; game.spawnNode(userP, after);
                            }
                            else
                            {
                                game.spawnNode(userP);
                            }
                            
                            

                            
                            rightClickCount = 0;
                        }
                    }
                    else
                    {
                        if (rightClickCount > rightClickMax)
                        {
                            //new node(s)
                            int rad = 100;
                            for (int i = 0; i < 10; i++)
                            {
                                int rx = Utils.random.Next(rad * 2) - rad;
                                int ry = Utils.random.Next(rad * 2) - rad;
                                game.spawnNode(worldMouseX + rx, worldMouseY + ry);
                            }

                            rightClickCount = 0;
                        }
                        else
                        {
                            rightClickCount++;
                        }
                    }

                }
            }
            else if (currentSelection == selection.randomNode)
            {
                if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                {
                    //random node
                    Vector2 pos = new Vector2(mouseState.X, mouseState.Y);
                    pos *= room.mapzoom;

                    //new node(s)
                    Dictionary<dynamic, dynamic> userP = new Dictionary<dynamic, dynamic>() {
                                { node.position, pos },
                    };
                    HashSet<comp> comps = new HashSet<comp>() { comp.basicdraw, comp.randcolor, comp.movement, comp.lifetime };
                    HashSet<comp> allComps = new HashSet<comp>();
                    foreach (comp c in Enum.GetValues(typeof(comp)))
                    {
                        allComps.Add(c);
                    }

                    int enumsize = allComps.Count;
                    int total = enumsize - comps.Count;

                    
                    Random random = Utils.random;
                    int compsToAdd = random.Next(total);

                    int counter = 0;
                    while (compsToAdd > 0)
                    {
                        if (counter++ > enumsize) break;
                        int compNum = random.Next(enumsize - 1);
                        comp newcomp = (comp)compNum;
                        if (comps.Contains(newcomp))
                        {
                            continue;
                        }
                        comps.Add(newcomp);
                        compsToAdd--;
                    }

                    foreach (comp c in comps)
                    {
                        userP.Add(c, true);
                    }

                    UserProps = userP;

                    game.spawnNode(userP, blank: true);
                }
                if (mouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released)
                {
                    //random node
                    Vector2 pos = new Vector2(mouseState.X, mouseState.Y);
                    pos *= room.mapzoom;

                    if (UserProps != null)
                    {
                        UserProps[node.position] = pos;
                        game.spawnNode(UserProps);
                    }
                }
            }
            else if (currentSelection == selection.targetSelection)
            {
                if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                {
                    //bool found = false;
                    Node found = null;
                    float shortedDistance = Int32.MaxValue;
                    for (int i = room.masterGroup.fullSet.Count - 1; i >= 0; i--)
                    {
                        Node n = (Node)room.masterGroup.fullSet.ElementAt(i);
                        // find node that has been clicked, starting from the most recently placed nodes
                        float distsquared = Vector2.DistanceSquared(n.transform.position, new Vector2(worldMouseX, worldMouseY));
                        if ( distsquared < n.transform.radius * n.transform.radius)
                        {
                            if (distsquared < shortedDistance)
                            {
                                found = n;
                                shortedDistance = distsquared;
                            }
                            
                        }
                    }
                    if (found != null)
                    {
                        sidebar.SetTargetNode(found);

                        TripSpawnOnCollide ts = new TripSpawnOnCollide(game.targetNode);
                        ProcessMethod pm = (d) => {
                            System.Console.WriteLine(ts.triggerNode.name + ts.colCount);
                            
                        };
                        ts.Collision += pm;


                        room.processManager.Add(ts);
                    }
                    else
                    {
                        //targetnode is deselected if you click on nothing
                        game.targetNode = null;
                    }
                }
                else if (mouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released)
                {
                    Node found = null;
                    float shortedDistance = Int32.MaxValue;
                    for (int i = room.masterGroup.fullSet.Count - 1; i >= 0; i--)
                    {
                        Node n = (Node)room.masterGroup.fullSet.ElementAt(i);
                        // find node that has been clicked, starting from the most recently placed nodes
                        float distsquared = Vector2.DistanceSquared(n.transform.position, new Vector2(worldMouseX, worldMouseY));
                        if (distsquared < n.transform.radius * n.transform.radius)
                        {
                            if (distsquared < shortedDistance)
                            {
                                found = n;
                                shortedDistance = distsquared;
                            }
                        }
                    }
                    if (found != null)
                    {
                        if (game.targetNode != null && game.targetNode.comps.ContainsKey(comp.flow))
                        {
                            game.targetNode.comps[comp.flow].AddToOutgoing(found);
                        }
                        if (game.targetNode != null && game.targetNode.comps.ContainsKey(comp.tether))
                        {
                            game.targetNode.comps[comp.tether].AddToOutgoing(found);
                        }
                    }
                    else
                    {
                    }
                }
                else if (mouseState.MiddleButton == ButtonState.Pressed && oldMouseState.MiddleButton == ButtonState.Released)
                {
                    Node found = null;
                    float shortedDistance = Int32.MaxValue;
                    for (int i = room.masterGroup.fullSet.Count - 1; i >= 0; i--)
                    {
                        Node n = (Node)room.masterGroup.fullSet.ElementAt(i);
                        // find node that has been clicked, starting from the most recently placed nodes
                        float distsquared = Vector2.DistanceSquared(n.transform.position, new Vector2(worldMouseX, worldMouseY));
                        if (distsquared < n.transform.radius * n.transform.radius)
                        {
                            if (distsquared < shortedDistance)
                            {
                                found = n;
                                shortedDistance = distsquared;
                            }
                        }
                    }
                    if (found != null)
                    {
                        if (found.comps.ContainsKey(comp.flow))
                        {
                            found.comps[comp.flow].activated = !found.comps[comp.flow].activated;
                        }
                        if (found.comps.ContainsKey(comp.tether))
                        {
                            found.comps[comp.tether].activated = !found.comps[comp.tether].activated;
                        }
                    }
                    else
                    {
                    }
                }
                //oldMouseScrollValue = mouseState.ScrollWheelValue;
                //oldMouseState = mouseState;
                //return;
            }
            else if (currentSelection == selection.groupSelection)
            {
                

                if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                {
                    groupSelectionBoxOrigin = new Vector2(mouseState.X, mouseState.Y);
                    groupSelectionBoxOrigin *= room.mapzoom;
                }
                else if (mouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
                {
                    bool ctrlDown = oldKeyBState.IsKeyDown(Keys.LeftControl);
                    bool altDown = oldKeyBState.IsKeyDown(Keys.LeftAlt);
                    if (altDown) ctrlDown = false;

                    Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);
                    mousePos *= room.mapzoom;
                    //groupSelectionBoxOrigin *= room.mapzoom;

                    float lowerx = Math.Min(mousePos.X, groupSelectionBoxOrigin.X);
                    float upperx = Math.Max(mousePos.X, groupSelectionBoxOrigin.X);
                    float lowery = Math.Min(mousePos.Y, groupSelectionBoxOrigin.Y);
                    float uppery = Math.Max(mousePos.Y, groupSelectionBoxOrigin.Y);

                    if (!ctrlDown && !altDown) groupSelectSet = new HashSet<Node>();

                    foreach(Node n in room.masterGroup.fullSet.ToList())
                    {
                        float xx = n.transform.position.X;
                        float yy = n.transform.position.Y;

                        if (xx >= lowerx && xx <= upperx
                         && yy >= lowery && yy <= uppery)
                        {
                            if (altDown)
                            {
                                if (groupSelectSet.Contains(n)) groupSelectSet.Remove(n);
                                else groupSelectSet.Add(n);
                            }
                            else
                            {
                                groupSelectSet.Add(n);
                            }
                        }
                    }
                    //System.Console.WriteLine(groupSelectSet.Count);

                    room.addRectangleLines(lowerx, lowery, upperx, uppery);
                }

                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);
                    mousePos *= room.mapzoom;

                    float lowerx = Math.Min(mousePos.X, groupSelectionBoxOrigin.X);
                    float upperx = Math.Max(mousePos.X, groupSelectionBoxOrigin.X);
                    float lowery = Math.Min(mousePos.Y, groupSelectionBoxOrigin.Y);
                    float uppery = Math.Max(mousePos.Y, groupSelectionBoxOrigin.Y);

                    room.addRectangleLines(lowerx, lowery, upperx, uppery);
                }
            }
            
            if (hovertargetting)
            {
                if (true || mouseState.LeftButton == ButtonState.Pressed)
                {
                    bool found = false;
                    for (int i = room.masterGroup.fullSet.Count - 1; i >= 0; i--)
                    {
                        Node n = (Node)room.masterGroup.fullSet.ElementAt(i);
                        // find node that has been clicked, starting from the most recently placed nodes
                        if (Vector2.DistanceSquared(n.transform.position, new Vector2(worldMouseX, worldMouseY)) < n.transform.radius * n.transform.radius)
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