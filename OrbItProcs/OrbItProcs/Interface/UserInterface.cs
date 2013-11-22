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

namespace OrbItProcs.Interface {
    public class UserInterface {

        #region /// Essentials ///

        public Game1 game;
        public Room room;

        #endregion

        #region /// Hardware Variables ///

        
        KeyboardState oldKeyBState;
        MouseState oldMouseState;
        

        #endregion

        #region /// Neoforce ///

        private Manager manager;
        private Window window;
        TabControl tbcMain;
        Label firstTitle;
        TextBox consoletextbox;
        private ListBox lstMain;
        private CheckBox chkTempNodes;
        Button btnRemoveNode, btnRemoveAllNodes, btnAddComponent, btnDefaultNode;
        private List<String> strangs = new List<String>();
        private TreeListBox lstComp;
        private GroupPanel groupPanel;
        private TreeListItem activeTreeItem;
        private object parentObject;
        private Node editNode;
        // change to unicode characters

        #endregion

        #region /// Layout ///

        private int lstMainScrollPosition = 0;
        private int lstCompScrollPosition = 0;
        private int LeftPadding = 5;
        private int VertPadding = 4;
        private int HeightCounter = 0, HeightCounter2 = 0;

        #endregion

        #region /// ToBeRemoved ///

        string currentSelection = "placeNode";//
        int oldMouseScrollValue = 0;//
        bool hovertargetting = false;//
        int rightClickCount = 0;//
        int rightClickMax = 1;//
        public int sWidth = 1000;////
        public int sHeight = 600;////




        #endregion
        
        


        public UserInterface(Game1 game)
        {
            this.game = game;
            this.room = game.room;
            manager = game.Manager;

            Initialize();
        }

        public void Initialize()
        {
            manager.Initialize();
            //renderTarget = new RenderTarget2D(GraphicsDevice, 512, 512, false, SurfaceFormat.Color, DepthFormat.Depth24);
            // Create and setup Window control.

            //groupPanelChildren = new List<Control>();
            //panelControls = new Dictionary<string, Control>();

            strangs.Add("first");
            strangs.Add("second");
            strangs.Add("third");

            window = new Window(manager);
            window.Init();
            window.Name = "win1";
            window.Text = "Getting Started";
            window.Width = 100;
            window.Height = game.sHeight;
            //window.Center();
            window.Visible = true;
            window.Resizable = false;
            window.Movable = false;
            //window.Parent = sidebar;
            window.Anchor = Anchors.Top | Anchors.Right | Anchors.Bottom;


            window.BorderVisible = false;
            window.MaximumHeight = game.sHeight;
            window.MinimumHeight = game.sHeight;
            window.MaximumWidth = 300;
            window.MinimumWidth = 200;
            window.Alpha = 255;
            window.SetPosition(game.sWidth - 200, 2);

            tbcMain = new TabControl(manager);

            tbcMain.Init();
            tbcMain.Parent = window;
            tbcMain.Left = 0;
            tbcMain.Top = 0;
            tbcMain.Width = 195;
            //tbcMain.Height = ClientArea.Height - 8 - TopPanel.Height - BottomPanel.Height;
            tbcMain.Height = game.sWidth - 40;
            tbcMain.Anchor = Anchors.All;
            //tbcMain.Resizable = true;


            tbcMain.AddPage();
            tbcMain.TabPages[0].Text = "First";
            tbcMain.AddPage();
            tbcMain.TabPages[1].Text = "Second";
            tbcMain.AddPage();
            tbcMain.TabPages[2].Text = "Third";


            TabPage first = tbcMain.TabPages[0];
            TabPage second = tbcMain.TabPages[1];

            firstTitle = new Label(manager);
            firstTitle.Init();
            firstTitle.Parent = first;
            firstTitle.Top = HeightCounter;
            firstTitle.Text = "Node List";
            firstTitle.Left = first.Width / 2 - firstTitle.Width / 2;
            HeightCounter += VertPadding + firstTitle.Height;
            firstTitle.Anchor = Anchors.Left;


            lstMain = new ListBox(manager);
            lstMain.Init();
            lstMain.Parent = first;
            lstMain.Top = HeightCounter;
            lstMain.Left = LeftPadding;
            lstMain.Width = first.Width - LeftPadding * 2;
            lstMain.Height = first.Height / 4; HeightCounter += VertPadding + lstMain.Height;
            lstMain.Anchor = Anchors.Top | Anchors.Left | Anchors.Bottom;
            lstMain.HideSelection = false;
            lstMain.ItemIndexChanged += new TomShane.Neoforce.Controls.EventHandler(lstMain_ItemIndexChanged);
            //lstMain.Click += new TomShane.Neoforce.Controls.EventHandler(lstMain_Click);
            lstMain.Items = game.room.nodes;

            manager.Add(window);

            chkTempNodes = new CheckBox(manager);
            chkTempNodes.Init();
            chkTempNodes.Parent = first;
            chkTempNodes.Left = LeftPadding;
            chkTempNodes.Top = HeightCounter;
            chkTempNodes.Width = first.Width - LeftPadding * 2; HeightCounter += VertPadding + chkTempNodes.Height;
            chkTempNodes.Anchor = Anchors.Left | Anchors.Bottom | Anchors.Right;
            chkTempNodes.Checked = false;
            chkTempNodes.Text = "Show TempNodes";
            chkTempNodes.ToolTip.Text = "Enables or disables showing temp nodes in the list.";
            chkTempNodes.CheckedChanged += new TomShane.Neoforce.Controls.EventHandler(chkTempNodes_CheckedChanged);

            btnRemoveNode = new Button(manager);
            btnRemoveNode.Init();
            btnRemoveNode.Text = "Remove Node";
            btnRemoveNode.Top = HeightCounter;
            btnRemoveNode.Width = first.Width / 2 - LeftPadding;
            btnRemoveNode.Height = 24;
            btnRemoveNode.Left = LeftPadding;
            btnRemoveNode.Parent = first;
            btnRemoveNode.Click += new TomShane.Neoforce.Controls.EventHandler(btnRemoveNode_Click);

            btnRemoveAllNodes = new Button(manager);
            btnRemoveAllNodes.Init();
            btnRemoveAllNodes.Text = "Remove All";
            btnRemoveAllNodes.Top = HeightCounter;
            btnRemoveAllNodes.Width = first.Width / 2 - LeftPadding;
            btnRemoveAllNodes.Height = 24; HeightCounter += VertPadding + btnRemoveAllNodes.Height;
            btnRemoveAllNodes.Left = LeftPadding + btnRemoveNode.Width;
            btnRemoveAllNodes.Parent = first;
            btnRemoveAllNodes.Click += new TomShane.Neoforce.Controls.EventHandler(btnRemoveAllNodes_Click);

            btnAddComponent = new Button(manager);
            btnAddComponent.Init();
            btnAddComponent.Text = "Add Component";
            btnAddComponent.Top = HeightCounter;
            btnAddComponent.Width = first.Width / 2 - LeftPadding;
            btnAddComponent.Height = 20;
            btnAddComponent.Left = LeftPadding;
            btnAddComponent.Parent = first;
            btnAddComponent.Click += new TomShane.Neoforce.Controls.EventHandler(btnAddComponent_Click);

            btnDefaultNode = new Button(manager);
            btnDefaultNode.Init();
            btnDefaultNode.Text = "Default Node";
            btnDefaultNode.Top = HeightCounter;
            btnDefaultNode.Width = first.Width / 2 - LeftPadding;
            btnDefaultNode.Height = 20; HeightCounter += VertPadding + btnDefaultNode.Height;
            btnDefaultNode.Left = LeftPadding + btnRemoveNode.Width;
            btnDefaultNode.Parent = first;
            btnDefaultNode.Click += new TomShane.Neoforce.Controls.EventHandler(btnDefaultNode_Click);

            lstComp = new TreeListBox(manager);

            lstComp.Init();
            manager.Add(lstComp);
            lstComp.Parent = first;
            lstComp.Top = HeightCounter;
            lstComp.Left = LeftPadding;
            lstComp.Width = first.Width - LeftPadding * 2;
            lstComp.Height = first.Height / 4; HeightCounter += VertPadding + lstComp.Height;
            lstComp.Anchor = Anchors.Top | Anchors.Left | Anchors.Bottom;
            lstComp.HideSelection = false;
            lstComp.ItemIndexChanged += new TomShane.Neoforce.Controls.EventHandler(lstComp_ItemIndexChanged);
            lstComp.Click += new TomShane.Neoforce.Controls.EventHandler(lstComp_Click);

            ContextMenu contextMenu = new ContextMenu(manager);
            //contextMenu.Root
            MenuItem applyToAllNodes = new MenuItem("Apply to all Nodes");

            applyToAllNodes.Click += new TomShane.Neoforce.Controls.EventHandler(applyToAllNodes_Click);
            contextMenu.Items.Add(applyToAllNodes);

            lstComp.ContextMenu = contextMenu;

            groupPanel = new GroupPanel(manager);
            groupPanel.Init();
            groupPanel.Parent = first;
            groupPanel.Anchor = Anchors.Left | Anchors.Top | Anchors.Right;
            groupPanel.Width = first.Width - LeftPadding * 2;
            groupPanel.Top = HeightCounter;
            groupPanel.Height = 115; HeightCounter += VertPadding + groupPanel.Height;
            groupPanel.Left = LeftPadding;
            groupPanel.panelControls = new Dictionary<string, Control>();
            groupPanel.Text = "Property";





            //==============================================================================================
            //             SECOND TAB PAGE
            //==============================================================================================
            //second

            Label secondTitle = new Label(manager);
            secondTitle.Init();
            secondTitle.Parent = second;
            secondTitle.Top = VertPadding;
            secondTitle.Text = "Console";
            secondTitle.Left = first.Width / 2 - firstTitle.Width / 2;
            HeightCounter2 += VertPadding * 2 + secondTitle.Height;
            secondTitle.Anchor = Anchors.Left;

            consoletextbox = new TextBox(manager);
            consoletextbox.Init();
            consoletextbox.Parent = second;
            consoletextbox.Left = LeftPadding;
            consoletextbox.Top = HeightCounter2;
            HeightCounter2 += VertPadding + consoletextbox.Height;
            consoletextbox.Width = second.Width - LeftPadding * 2;
            consoletextbox.Height = consoletextbox.Height + 3;
            consoletextbox.ToolTip.Text = "Enter a command, and push enter";
            consoletextbox.KeyUp += new KeyEventHandler(consoletextbox_KeyUp);


            Button btnEnter = new Button(manager);
            btnEnter.Init();
            btnEnter.Parent = second;
            btnEnter.Left = LeftPadding;
            btnEnter.Top = HeightCounter2;
            //HeightCounter2 += VertPadding + btnEnter.Height;
            btnEnter.Width = (second.Width - LeftPadding * 2) / 2;
            btnEnter.Text = "Enter";
            btnEnter.Click += new TomShane.Neoforce.Controls.EventHandler(btnEnter_Click);

            Button btnClear = new Button(manager);
            btnClear.Init();
            btnClear.Parent = second;
            btnClear.Left = LeftPadding + btnEnter.Width;
            btnClear.Top = HeightCounter2;
            HeightCounter2 += VertPadding + btnClear.Height;
            btnClear.Width = (second.Width - LeftPadding * 2) / 2;
            btnClear.Text = "Clear";
            btnClear.Click += new TomShane.Neoforce.Controls.EventHandler(btnClear_Click);


        }


        public void Update(GameTime gameTime)
        {
            ProcessKeyboard();
            ProcessMouse();

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
                currentSelection = "placeNode";
            if (keybState.IsKeyDown(Keys.T))
                currentSelection = "targeting";
            if (keybState.IsKeyDown(Keys.W) && !oldKeyBState.IsKeyDown(Keys.W))
            {
                lstMain.ScrollTo(20);
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
            if (mouseState.X >= sWidth - window.Width - 5)
            {
                if (mouseState.Y > lstMain.Top + 24 && mouseState.Y < lstMain.Top + lstMain.Height + 24)
                {
                    if (mouseState.ScrollWheelValue < oldMouseState.ScrollWheelValue)
                    {
                        //lstMain.
                        lstMain_ChangeScrollPosition(4);

                    }
                    else if (mouseState.ScrollWheelValue > oldMouseState.ScrollWheelValue)
                    {
                        lstMain_ChangeScrollPosition(-4);
                    }
                }
                if (mouseState.Y > lstComp.Top + 24 && mouseState.Y < lstComp.Top + lstComp.Height + 24)
                {
                    if (mouseState.ScrollWheelValue < oldMouseState.ScrollWheelValue)
                    {
                        //lstMain.
                        lstComp_ChangeScrollPosition(4);

                    }
                    else if (mouseState.ScrollWheelValue > oldMouseState.ScrollWheelValue)
                    {
                        lstComp_ChangeScrollPosition(-4);
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
                    game.spawnNode(worldMouseX, worldMouseY);

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
            else if (currentSelection.Equals("targeting"))
            {
                if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                {
                    bool found = false;
                    for (int i = room.nodes.Count - 1; i >= 0; i--)
                    {
                        Node n = (Node)room.nodes.ElementAt(i);
                        // find node that has been clicked, starting from the most recently placed nodes
                        if (Vector2.DistanceSquared(n.position, new Vector2(worldMouseX, worldMouseY)) < n.Radius * n.Radius)
                        {
                            //--
                            //room.nodes.Remove(n);
                            //break;
                            //--
                            game.targetNode = n;
                            if (game.targetNode == null) System.Console.WriteLine("well ya dun goofed");
                            room.processManager.Add(new TripSpawnOnCollide(game.targetNode));
                            found = true;
                            break;
                        }
                    }
                    if (!found) game.targetNode = null;
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
                room.mapzoom += 0.2f;
            }
            else if (mouseState.ScrollWheelValue > oldMouseScrollValue)
            {
                room.mapzoom -= 0.2f;
            }

            oldMouseScrollValue = mouseState.ScrollWheelValue;
            oldMouseState = mouseState;
        }

        void applyToAllNodes_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            //MenuItem menuitem = (MenuItem)sender;
            TreeListItem item = (TreeListItem)lstComp.Items.ElementAt(lstComp.ItemIndex);
            if (item.itemtype == treeitem.propertyinfo)
            {
                foreach (Node node in game.room.nodes)
                {
                    item.propertyInfo.SetValue(node, item.propertyInfo.GetValue(item.node,null),null); // make this object compliant
                    
                }
            }
            else if (item.itemtype == treeitem.fieldinfo)
            {
                foreach (Node node in game.room.nodes)
                {
                    item.fieldInfo.SetValue(node, item.fieldInfo.GetValue(item.obj));
                }
            }
            else if (item.itemtype == treeitem.component)
            {
                foreach (Node node in game.room.nodes)
                {
                    comp c = item.component;
                    /*
                    if (node.props.ContainsKey(c))
                    {
                        node.props[c] = item.node.props[c];
                    }
                     */
                    if (node.comps.ContainsKey(c))
                    {
                        node.setCompActive(c, item.node.isCompActive(c));
                    }
                }
            }
            else if (item.itemtype == treeitem.objfieldinfo)
            {
                foreach (Node node in game.room.nodes)
                {
                    if (item.obj is Component)
                    {
                        Component c = (Component)item.obj;
                        comp compname;
                        foreach (comp key in c.parent.comps.Keys)
                        {
                            if (c.parent.comps[key] == c)
                            {
                                compname = key;
                                if (node.comps.ContainsKey(compname))
                                {
                                    item.fieldInfo.SetValue(node.comps[compname], item.fieldInfo.GetValue(item.obj));
                                }
                                break;
                            }
                        }
                        
                    }
                }
            }
            else if (item.itemtype == treeitem.objpropertyinfo)
            {
                foreach (Node node in game.room.nodes)
                {
                    if (item.obj is Component)
                    {
                        Component c = (Component)item.obj;
                        comp compname;
                        foreach (comp key in c.parent.comps.Keys)
                        {
                            if (c.parent.comps[key] == c)
                            {
                                compname = key;
                                if (node.comps.ContainsKey(compname))
                                {
                                    item.propertyInfo.SetValue(node.comps[compname], item.propertyInfo.GetValue(item.obj, null), null);
                                }
                                break;
                            }
                        }

                    }
                }
            }
            
        }

        void consoletextbox_KeyUp(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            KeyEventArgs ke = (KeyEventArgs)e;
            if (ke.Key == Keys.Enter)
            {
                //TextBox textbox = (TextBox)sender;
                ProcessConsoleCommand(consoletextbox.Text);
            }
        }

        void btnEnter_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            //consoletextbox.Text = "";
            
            ProcessConsoleCommand(consoletextbox.Text);
            

        }

        public void ProcessConsoleCommand(String text)
        {
            text = text.Trim();
            
            if (text.Equals(""))
            {
                consoletextbox.Text = "";
                return;
            }
            String[] args = text.Split(' ');
            if (args[0].Equals("gridsystem") || args[0].Equals("gs"))
            {
                //System.Console.WriteLine("first!");
                if (args.Length == 1)
                    game.room.gridsystem = new GridSystem(game.room, 20, 5);
                else if (args.Length == 2)
                    //game.room.gridsystem = new GridSystem(
                    game.room.gridsystem = new GridSystem(game.room, game.room.gridsystem.cellsX, Convert.ToInt32(args[1]));
                else if (args.Length == 3)
                    game.room.gridsystem = new GridSystem(game.room, Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));
                    //game.room.gridsystem = new GridSystem(game.room, game.worldWidth, game.worldHeight, Convert.ToInt32(args[1]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));

            }
        }

        void btnClear_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            if (e is KeyEventArgs)
            {
                KeyEventArgs ke = (KeyEventArgs)e;
                if (ke.Key == Keys.Enter)
                {
                    TextBox textbox = (TextBox)sender;
                    textbox.Text = "";

                }
            }
            else
            {
                consoletextbox.Text = "";
            }
        }

        public void UpdateGroupPanel(TreeListItem treeItem, GroupPanel grouppanel)
        {
            if (activeTreeItem == treeItem) return;

            if (grouppanel.panelControls.Keys.Count > 0) DisableControls(grouppanel);

            activeTreeItem = treeItem;

            if (treeItem.itemtype == treeitem.component)
            {
                //System.Console.WriteLine("Component, run boolean code");
                CheckBox chkbox = new CheckBox(manager);
                chkbox.Init();
                chkbox.Parent = grouppanel;
                chkbox.Left = LeftPadding;
                chkbox.Top = 10;
                chkbox.Width = 120;
                chkbox.Checked = (bool)treeItem.node.isCompActive(treeItem.component);
                chkbox.Text = treeItem.component + " (" + treeItem.node.isCompActive(treeItem.component) + ")";
                chkbox.CheckedChanged += new TomShane.Neoforce.Controls.EventHandler(chkbox_CheckedChanged);

                grouppanel.panelControls.Add("chkbox", chkbox);

                return;
            }
            parentObject = null;
            if (treeItem.itemtype == treeitem.fieldinfo || treeItem.itemtype == treeitem.propertyinfo)
            {
                parentObject = treeItem.node;
            }
            else if (treeItem.itemtype == treeitem.objfieldinfo)
            {
                parentObject = treeItem.obj;
            }
            else if (treeItem.itemtype == treeitem.objpropertyinfo)
            {
                parentObject = treeItem.obj;
            }
           
            if (treeItem.fieldInfo == null && treeItem.propertyInfo == null) return;

            dynamic field = null;
            Type t = null;
            if (treeItem.itemtype == treeitem.propertyinfo || treeItem.itemtype == treeitem.objpropertyinfo)
            {
                field = treeItem.propertyInfo;
                t = field.PropertyType;
                //System.Console.WriteLine("Yeah");
            }
            else
            {
                field = treeItem.fieldInfo;
                t = field.FieldType;
            }

            //FieldInfo field = treeItem.fieldInfo;
            groupPanel.Text = field.Name;
            
            
            //Type t = field.FieldType;
            
            //System.Console.WriteLine(t.ToString());

            if (t.ToString().Equals("System.Int32") || t.ToString().Equals("System.Single") || t.ToString().Equals("System.String"))
            {
                //System.Console.WriteLine("It's an int or float.");
                TextBox txtbox = new TextBox(manager);
                txtbox.Init();
                txtbox.Parent = grouppanel;
                txtbox.Left = LeftPadding;
                txtbox.Top = 10;
                txtbox.Width = 80;
                txtbox.Height = txtbox.Height + 3;
                
                //txtbox.BackColor = Color.Green;
                
                //txtbox.DrawBorders = true;
                txtbox.Text = field.GetValue(parentObject).ToString();

                Button btnModify = new Button(manager);
                btnModify.Init();
                btnModify.Parent = grouppanel;
                btnModify.Left = LeftPadding * 2 + txtbox.Width;
                btnModify.Top = 10;
                btnModify.Width = 80;
                btnModify.Text = "Modify";
                btnModify.Click += new TomShane.Neoforce.Controls.EventHandler(btnModify_Click);
                

                grouppanel.panelControls.Add("txtbox", txtbox);
                grouppanel.panelControls.Add("btnModify", btnModify);

                if (t.ToString().Equals("System.Int32") || t.ToString().Equals("System.Single"))
                {
                    TrackBar trkMain = new TrackBar(manager);
                    trkMain.Init();
                    trkMain.Parent = grouppanel;
                    trkMain.Left = LeftPadding;
                    trkMain.Top = 20 + btnModify.Height;
                    trkMain.Width = txtbox.Width + btnModify.Width + LeftPadding;               
                    trkMain.Anchor = Anchors.Left | Anchors.Top | Anchors.Right;
                    int val = Convert.ToInt32(field.GetValue(parentObject));
                    trkMain.Value = val;
                    if (val == 0) val = 5;
                    trkMain.Range = val *2;
                    trkMain.ValueChanged += new TomShane.Neoforce.Controls.EventHandler(trkMain_ValueChanged);
                    trkMain.btnSlider.MouseUp += new TomShane.Neoforce.Controls.MouseEventHandler(trkMain_MouseUp);
                    grouppanel.panelControls.Add("trkMain", trkMain);
                }
                
            }
            else if (t.ToString().Equals("System.Boolean"))
            {
                //System.Console.WriteLine("It's a boolean.");
                CheckBox chkbox = new CheckBox(manager);
                chkbox.Init();
                chkbox.Parent = grouppanel;
                chkbox.Left = LeftPadding;
                chkbox.Top = 10;
                chkbox.Width = 120;
                chkbox.Checked = (bool)field.GetValue(parentObject);
                chkbox.Text = field.Name + " (" + field.GetValue(parentObject) + ")";
                chkbox.CheckedChanged += new TomShane.Neoforce.Controls.EventHandler(chkbox_CheckedChanged);
                grouppanel.panelControls.Add("chkbox", chkbox);

            }
            else if (t.ToString().Equals("Microsoft.Xna.Framework.Vector2"))
            {
                //System.Console.WriteLine("It's a vector2.");
            }
            else if (t.ToString().Equals("Microsoft.Xna.Framework.Color"))
            {
                //System.Console.WriteLine("It's a color.");
            }
            
            
        }

        public void DisableControls(GroupPanel grouppanel)
        {
            List<String> list = grouppanel.panelControls.Keys.ToList(); // for some reason this isn't updated if you click quickly
            //System.Console.WriteLine(list.Count);
            foreach (String key in list)
            {
                grouppanel.Remove(grouppanel.panelControls[key]);
                grouppanel.panelControls.Remove(key);
            }
        }

        void trkMain_MouseUp(object sender, TomShane.Neoforce.Controls.MouseEventArgs e)
        {
            //TrackBar trkbar = (TrackBar)sender;
            //System.Console.WriteLine("yeah");
            TrackBar trkbar = (TrackBar)groupPanel.panelControls["trkMain"];
            
            if (trkbar.Value == trkbar.Range) trkbar.Range *= 2;
        }

        void trkMain_ValueChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            TrackBar trkbar = (TrackBar)sender;
            GroupPanel gp = (GroupPanel)(trkbar.Parent.Parent);
            if (gp.Parent == tbcMain.TabPages[0])
            {
                TreeListItem item = (TreeListItem)lstComp.Items.ElementAt(lstComp.ItemIndex);
                dynamic field = null;
                if (item.itemtype == treeitem.propertyinfo || item.itemtype == treeitem.objpropertyinfo)
                {
                    field = item.propertyInfo;
                }
                else if (item.itemtype == treeitem.fieldinfo || item.itemtype == treeitem.objfieldinfo)
                {
                    field = item.fieldInfo;

                }
                if (field.GetValue(parentObject) == null) return;
                
                if (field is Int32)
                {
                    field.SetValue(parentObject, trkbar.Value);
                    gp.panelControls["txtbox"].Text = "" + trkbar.Value;
                }
                else if (field.GetValue(parentObject) is System.Single)
                {
                    field.SetValue(parentObject, Convert.ToSingle(trkbar.Value));
                    gp.panelControls["txtbox"].Text = "" + trkbar.Value;
                    //field.SetValue(10.0f, parentObject);
                    
                }
                

            }
        }

        void chkbox_CheckedChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            
            CheckBox checkbox = (CheckBox) sender;
            if (checkbox.Parent.Parent.Parent == tbcMain.TabPages[0]) // checkbox is in the 'first' tab page
            {
                TreeListItem item = (TreeListItem)lstComp.Items.ElementAt(lstComp.ItemIndex);

                if (item.itemtype == treeitem.component)
                {
                    item.node.setCompActive(item.component, checkbox.Checked);
                    checkbox.Text = item.component + " (" + item.node.isCompActive(item.component) + ")";
                }
                else if (item.itemtype == treeitem.fieldinfo || item.itemtype == treeitem.objfieldinfo)
                {
                    item.fieldInfo.SetValue(parentObject, checkbox.Checked);
                    checkbox.Text = item.fieldInfo.Name + " (" + item.fieldInfo.GetValue(parentObject) + ")";
                }
            }
            else if (checkbox.Parent.Parent == tbcMain.TabPages[1]) // checkbox is in the 'second' tab page
            {

            }


            
            
        }

        void btnModify_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            TreeListItem item = (TreeListItem)lstComp.Items.ElementAt(lstComp.ItemIndex);

            dynamic field = null;
            Type t = null;
            if (item.itemtype == treeitem.fieldinfo || item.itemtype == treeitem.objfieldinfo)
            {
                field = item.fieldInfo;
                t = field.FieldType;
            }
            else if (item.itemtype == treeitem.propertyinfo || item.itemtype == treeitem.objpropertyinfo)
            {
                field = item.propertyInfo;
                t = field.PropertyType;
            }
            else
            {
                field = item.fieldInfo;
                t = field.FieldType;
            }
            //GroupPanel grouppanel = (GroupPanel)((Button) sender).Parent;
            GroupPanel grouppanel = groupPanel;

            //FieldInfo field = item.fieldInfo;
            //Type t = field.FieldType;
            //System.Console.WriteLine(t.ToString());

            String str;

            if (t.ToString().Equals("System.Int32"))
            {
                str = grouppanel.panelControls["txtbox"].Text.Trim();
                int integer;
                if (str.Length < 1) return;
                if (Int32.TryParse(str, out integer))
                    field.SetValue(parentObject, integer);
                else
                    return;
            }
            if (t.ToString().Equals("System.Single"))
            {
                str = grouppanel.panelControls["txtbox"].Text.Trim();
                float f;
                if (str.Length < 1) return;
                if (float.TryParse(str, out f))
                    field.SetValue(parentObject, f);
                else
                    return;
            }
            if (t.ToString().Equals("System.String"))
            {
                str = grouppanel.panelControls["txtbox"].Text;
                if (str.Length < 1) return;
                field.SetValue(parentObject, str);
                return;
            }

        }

        public void lstMain_ChangeScrollPosition(int change)
        {
            if (lstMainScrollPosition + change < 0) lstMainScrollPosition = 0;
            else if (lstMainScrollPosition + change > lstMain.Items.Count) lstMainScrollPosition = lstMain.Items.Count;
            else lstMainScrollPosition += change;
            lstMain.ScrollTo(lstMainScrollPosition);
        }

        public void lstComp_ChangeScrollPosition(int change)
        {
            if (lstCompScrollPosition + change < 0) lstCompScrollPosition = 0;
            else if (lstCompScrollPosition + change > lstComp.Items.Count) lstCompScrollPosition = lstComp.Items.Count;
            else lstCompScrollPosition += change;
            lstComp.ScrollTo(lstCompScrollPosition);
        }

        void lstMain_ItemIndexChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            ListBox listbox = (ListBox)sender;
            //remove panelControl elements (from groupPanel at the bottom)
            if (groupPanel.panelControls.Keys.Count > 0) DisableControls(groupPanel);
            //System.Console.WriteLine("" + treebox.ItemIndex);
            game.targetNode = (Node)listbox.Items.ElementAt(listbox.ItemIndex);
            editNode = game.targetNode;

            lstComp.Items = TreeListItem.GenerateList((Node)listbox.Items.ElementAt(listbox.ItemIndex),"");

        }

        void lstComp_ItemIndexChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            TreeListBox listComp = (TreeListBox)sender;

            if (listComp.ItemIndex < 0) return;
            TreeListItem item = (TreeListItem)listComp.Items.ElementAt(listComp.ItemIndex);


            UpdateGroupPanel(item, groupPanel);
        }

        void lstComp_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            TreeListBox listComp = (TreeListBox)sender;

            if (listComp.ItemIndex < 0) return;
            TreeListItem item = (TreeListItem)listComp.Items.ElementAt(listComp.ItemIndex);
            if (item.hasChildren)
            {
                if (item.extended)
                {
                    item.prefix = "+";
                    foreach (TreeListItem subitem in item.children)
                    {
                        listComp.Items.Remove(subitem);
                    }
                }
                else
                {
                    item.prefix = "-";
                    int i = 1;
                    foreach (TreeListItem subitem in item.children)
                    {
                        listComp.Items.Insert(listComp.ItemIndex + i++,subitem);
                    }
                }
                item.extended = !item.extended;
            }

            UpdateGroupPanel(item, groupPanel);
        }

        void chkTempNodes_CheckedChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            //TODO: Ask harley what to do
            
        }

        void btnAddComponent_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            AddComponentWindow addComponentWindow = new AddComponentWindow(game);
            // if it's open don't open again... (TODO)
        }

        void btnDefaultNode_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            lstComp.Items = TreeListItem.GenerateList(game.room.defaultNode, "");
            editNode = game.room.defaultNode;
        }

        void btnRemoveAllNodes_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            game.room.RemoveAllNodes();

            if (editNode == null)
            {
                System.Console.WriteLine("cleared");
            }

            if (editNode != game.room.defaultNode)
            {
                System.Console.WriteLine("cleared");
                lstComp.Items.Clear();
            }
        }

        void btnRemoveNode_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            //game.room.RemoveAllNodes();
            if (game.targetNode != null)
            {
                game.room.nodes.Remove(game.targetNode);
                game.targetNode = null;
            }
            lstComp.Items.Clear();
            DisableControls(groupPanel);
        }

        public void UpdateNodeList(Node node)
        { 
            //lstMain.Items.AddRange(
            //lstMain.Items.Add(node);
            
        }

    }
}
/*
    List<Y> listOfY = listOfX.Cast<Y>().ToList()
 * List<Y> listOfY = listOfX.Cast<Y>().ToList()
 * List<Y> listOfY = listOfX.Cast<Y>().ToList()
 * List<Y> listOfY = listOfX.Cast<Y>().ToList()
 * List<Y> listOfY = listOfX.Cast<Y>().ToList()
 * List<Y> listOfY = listOfX.Cast<Y>().ToList()
 * List<Y> listOfY = listOfX.Cast<Y>().ToList()
 * 
*/

/*
textbox = new TextBox(manager);
            textbox.Init();
            textbox.Text = "textbox";
            textbox.Width = 100;
            textbox.Height = 20; HeightCounter += VertPadding + VertPadding + textbox.Height;
            textbox.Top = 0;
            textbox.Left = LeftPadding;
            textbox.BackColor = Color.White;
            textbox.Parent = first;
*/