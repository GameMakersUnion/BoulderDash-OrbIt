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
using EventHandler = TomShane.Neoforce.Controls.EventHandler;
using EventArgs = TomShane.Neoforce.Controls.EventArgs;
namespace OrbItProcs.Interface
{
    public class Sidebar
    {
        public Game1 game;
        public Room room;
        public UserInterface ui;

        public int Width = 200;
        #region /// Neoforce Fields///
        private Manager manager;
        private Window master;
        TabControl tbcMain;
        public Label title1;
        TextBox consoletextbox;
        public ListBox lstMain;
        private CheckBox chkTempNodes;
        Button btnRemoveNode, btnRemoveAllNodes, btnAddComponent, btnDefaultNode, btnApplyToAll, btnSaveNode;
        public TreeListBox lstComp;
        public ContextMenu contextMenulstComp;
        public Label lblEditNodeName;
        public GroupPanel groupPanel;
        private TreeListItem activeTreeItem;
        private object parentObject;
        //public Node ui.editNode, ui.spawnerNode;
        public ListBox lstPresets;
        public ComboBox cmbPresets;
        public MenuItem applyToAllNodesMenuItem;
        public MenuItem toggleComponentMenuItem;
        public MenuItem removeComponentMenuItem;
        public ContextMenu presetContextMenu;
        public MenuItem deletePresetMenuItem;
        #endregion

        #region /// Layout Fields///
        private int HeightCounter = 0, HeightCounter2 = 0;
        private int lstMainScrollPosition = 0;
        private int lstCompScrollPosition = 0;
        private int LeftPadding = 5;
        private int VertPadding = 4;
        #endregion

        public Sidebar(Game1 game, UserInterface ui)
        {
            this.game = game;
            this.room = game.room;
            this.ui = ui;
            manager = game.Manager;
        }

        public void Initialize()
        {
            manager.Initialize();

            #region /// Master ///
            master = new Window(manager);
            master.Init();
            master.Name = "Sidebar";
            master.Width = Width;
            master.Height = Game1.sHeight;
            master.Visible = true;
            master.Resizable = false; // If true, uncomment below
            //master.MaximumHeight = Game1.sHeight; 
            //master.MinimumHeight = Game1.sHeight;
            //master.MaximumWidth = 300;
            //master.MinimumWidth = 200;
            master.Movable = false;
            master.Anchor = Anchors.Top | Anchors.Right | Anchors.Bottom;
            master.BorderVisible = false;
            master.Alpha = 255; //TODO : check necesity
            master.SetPosition(Game1.sWidth - 200, 2);
            manager.Add(master);
            #endregion

            #region  /// tabcontrol ///
            tbcMain = new TabControl(manager);
            tbcMain.Init();
            tbcMain.Parent = master;

            tbcMain.Left = 0;
            tbcMain.Top = 0;
            tbcMain.Width = master.Width - 5;
            tbcMain.Height = Game1.sWidth - 40; //TODO : WTF
            tbcMain.Anchor = Anchors.All;
            #endregion

            #region  /// Page1 ///

            tbcMain.AddPage();
            tbcMain.TabPages[0].Text = "First";
            TabPage first = tbcMain.TabPages[0];

            #region  /// Title ///
            title1 = new Label(manager);
            title1.Init();
            title1.Parent = first;

            title1.Top = HeightCounter;
            title1.Text = "Node List";
            title1.Width = 130;
            title1.Left = first.Width / 2 - title1.Width / 2; //TODO : Center auto
            HeightCounter += VertPadding + title1.Height;
            title1.Anchor = Anchors.Left;
            #endregion

            #region  /// List Main ///
            lstMain = new ListBox(manager);
            lstMain.Init();
            lstMain.Parent = first;

            lstMain.Top = HeightCounter;
            lstMain.Left = LeftPadding;
            lstMain.Width = first.Width - LeftPadding * 2;
            lstMain.Height = first.Height / 5; HeightCounter += VertPadding + lstMain.Height;
            lstMain.Anchor = Anchors.Top | Anchors.Left | Anchors.Bottom;

            lstMain.HideSelection = false; // TODO WTF
            lstMain.ItemIndexChanged += new TomShane.Neoforce.Controls.EventHandler(lstMain_ItemIndexChanged);
            lstMain.Items = room.nodes;
            #endregion

            #region /// CheckBox ///
            chkTempNodes = new CheckBox(manager);
            chkTempNodes.Init();
            chkTempNodes.Parent = first;

            chkTempNodes.Left = LeftPadding;
            chkTempNodes.Top = HeightCounter;
            chkTempNodes.Width = first.Width - LeftPadding * 2; HeightCounter += VertPadding + chkTempNodes.Height;
            chkTempNodes.Anchor = Anchors.Left | Anchors.Bottom | Anchors.Right;

            chkTempNodes.Checked = false; // TODO : Nessecary?
            chkTempNodes.Text = "Show TempNodes";
            chkTempNodes.ToolTip.Text = "Enables or disables showing temp nodes in the list.";
            // TODO : IMPLEMENT: chkTempNodes.CheckedChanged += new TomShane.Neoforce.Controls.EventHandler(chkTempNodes_CheckedChanged);
            #endregion

            #region  /// Remove Node Button ///
            btnRemoveNode = new Button(manager);
            btnRemoveNode.Init();
            btnRemoveNode.Parent = first;

            btnRemoveNode.Top = HeightCounter;
            btnRemoveNode.Width = first.Width / 2 - LeftPadding;
            btnRemoveNode.Height = 24;
            btnRemoveNode.Left = LeftPadding;

            btnRemoveNode.Text = "Remove Node";
            btnRemoveNode.Click += new TomShane.Neoforce.Controls.EventHandler(btnRemoveNode_Click);
            #endregion

            #region  /// Remove All Nodes Button ///
            btnRemoveAllNodes = new Button(manager);
            btnRemoveAllNodes.Init();
            btnRemoveAllNodes.Parent = first;

            btnRemoveAllNodes.Top = HeightCounter;
            btnRemoveAllNodes.Width = first.Width / 2 - LeftPadding;
            btnRemoveAllNodes.Height = 24; HeightCounter += VertPadding + btnRemoveAllNodes.Height;
            btnRemoveAllNodes.Left = LeftPadding + btnRemoveNode.Width;

            btnRemoveAllNodes.Text = "Remove All";
            btnRemoveAllNodes.Click += new TomShane.Neoforce.Controls.EventHandler(btnRemoveAllNodes_Click);
            #endregion

            #region  /// Remove All Nodes ///
            btnAddComponent = new Button(manager);
            btnAddComponent.Init();
            btnAddComponent.Parent = first;

            btnAddComponent.Top = HeightCounter;
            btnAddComponent.Width = first.Width / 2 - LeftPadding;
            btnAddComponent.Height = 20;
            btnAddComponent.Left = LeftPadding;

            btnAddComponent.Text = "Add Component";
            btnAddComponent.Click += new TomShane.Neoforce.Controls.EventHandler(btnAddComponent_Click);
            #endregion

            #region  /// Remove All Nodes ///
            btnDefaultNode = new Button(manager);
            btnDefaultNode.Init();
            btnDefaultNode.Parent = first;

            btnDefaultNode.Top = HeightCounter;
            btnDefaultNode.Width = first.Width / 2 - LeftPadding;
            btnDefaultNode.Height = 20; HeightCounter += VertPadding + btnDefaultNode.Height;
            btnDefaultNode.Left = LeftPadding + btnRemoveNode.Width;

            btnDefaultNode.Text = "Default Node";
            btnDefaultNode.Click += new TomShane.Neoforce.Controls.EventHandler(btnDefaultNode_Click);
            #endregion

            #region  /// Presets Dropdown ///
            cmbPresets = new ComboBox(manager);
            cmbPresets.Init();
            cmbPresets.Parent = first;

            cmbPresets.Width = 160;
            cmbPresets.Left = LeftPadding;
            cmbPresets.Top = HeightCounter; HeightCounter += VertPadding + cmbPresets.Height;

            cmbPresets.Items = game.NodePresets;
            cmbPresets.ItemIndexChanged += new TomShane.Neoforce.Controls.EventHandler(cmbPresets_ItemIndexChanged);
            #endregion

            #region  /// Presets Dropdown ///
            lblEditNodeName = new Label(manager);
            lblEditNodeName.Init();
            lblEditNodeName.Parent = first;

            lblEditNodeName.Top = HeightCounter + 10; HeightCounter += VertPadding + lblEditNodeName.Height + 10;
            lblEditNodeName.Width = 150;
            lblEditNodeName.Left = first.Width / 5;
            lblEditNodeName.Anchor = Anchors.Left;

            lblEditNodeName.Text = ">No Node Selected<";
            #endregion

            #region  /// Component List ///
            lstComp = new TreeListBox(manager);
            lstComp.Init();
            manager.Add(lstComp); // TODO : WTF
            lstComp.Parent = first;

            lstComp.Top = HeightCounter;
            lstComp.Left = LeftPadding;
            lstComp.Width = first.Width - LeftPadding * 2;
            lstComp.Height = first.Height / 4; HeightCounter += VertPadding + lstComp.Height;
            lstComp.Anchor = Anchors.Top | Anchors.Left | Anchors.Bottom;

            lstComp.HideSelection = false;
            lstComp.ItemIndexChanged += new TomShane.Neoforce.Controls.EventHandler(lstComp_ItemIndexChanged);
            lstComp.Click += new TomShane.Neoforce.Controls.EventHandler(lstComp_Click);
            #region  /// Context Menu ///
            contextMenulstComp = new ContextMenu(manager);
            applyToAllNodesMenuItem = new MenuItem("Apply to all Nodes");
            applyToAllNodesMenuItem.Click += new TomShane.Neoforce.Controls.EventHandler(applyToAllNodesMenuItem_Click);
            toggleComponentMenuItem = new MenuItem("Toggle Component");
            toggleComponentMenuItem.Click += new TomShane.Neoforce.Controls.EventHandler(toggleComponentMenuItem_Click);
            removeComponentMenuItem = new MenuItem("Remove Component");
            removeComponentMenuItem.Click += new TomShane.Neoforce.Controls.EventHandler(removeComponentMenuItem_Click);
            contextMenulstComp.Items.Add(applyToAllNodesMenuItem);
            #endregion
            lstComp.ContextMenu = contextMenulstComp;
            #endregion

            #region  /// GroupPanel ///
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
            #endregion

            #region  /// Apply to All ///
            btnApplyToAll = new Button(manager);
            btnApplyToAll.Init();
            btnApplyToAll.Parent = first;

            btnApplyToAll.Text = "Apply To All";
            btnApplyToAll.Top = HeightCounter;
            btnApplyToAll.Width = first.Width / 2 - LeftPadding;
            btnApplyToAll.Height = 20; //HeightCounter += VertPadding + btnApplyToAll.Height;
            btnApplyToAll.Left = LeftPadding;
            btnApplyToAll.Click += new TomShane.Neoforce.Controls.EventHandler(applyToAllNodesMenuItem_Click); //TODO ??
            #endregion

            #region  /// Save as Template ///
            btnSaveNode = new Button(manager);
            btnSaveNode.Init();
            btnSaveNode.Text = "Save Node";
            btnSaveNode.Top = HeightCounter;
            btnSaveNode.Width = first.Width / 2 - LeftPadding;
            btnSaveNode.Height = 20; HeightCounter += VertPadding + btnSaveNode.Height;
            btnSaveNode.Left = LeftPadding + btnApplyToAll.Width;
            btnSaveNode.Parent = first;
            //btnSaveNode.Click += new TomShane.Neoforce.Controls.EventHandler(btnSaveNode_Click);
            btnSaveNode.Click += delegate(object sender, EventArgs e) {
                PopupWindow saveNodes = new PopupWindow(game, "saveNode");};
            #endregion
            #endregion

            #region  /// Page two ///
            tbcMain.AddPage();
            tbcMain.TabPages[1].Text = "Second";
            TabPage second = tbcMain.TabPages[1];
            HeightCounter = 0;

            #region  /// Title ///
            Label secondTitle = new Label(manager);
            secondTitle.Init();
            secondTitle.Parent = second;

            secondTitle.Top = VertPadding;
            secondTitle.Left = first.Width / 2 - title1.Width / 2;
            HeightCounter2 += VertPadding * 2 + secondTitle.Height;
            secondTitle.Anchor = Anchors.Left;

            secondTitle.Text = "Console";
            #endregion

            #region  /// Console textbox ///
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
            #endregion

            #region  /// Enter Button ///
            Button btnEnter = new Button(manager);
            btnEnter.Init();
            btnEnter.Parent = second;

            btnEnter.Left = LeftPadding;
            btnEnter.Top = HeightCounter2;
            btnEnter.Width = (second.Width - LeftPadding * 2) / 2;

            btnEnter.Text = "Enter";
            btnEnter.Click += new TomShane.Neoforce.Controls.EventHandler(btnEnter_Click);
            #endregion

            #region  /// Clear ///
            Button btnClear = new Button(manager);
            btnClear.Init();
            btnClear.Parent = second;

            btnClear.Left = LeftPadding + btnEnter.Width;
            btnClear.Top = HeightCounter2; HeightCounter2 += VertPadding + btnClear.Height;            
            btnClear.Width = (second.Width - LeftPadding * 2) / 2;

            btnClear.Text = "Clear";
            btnClear.Click += new TomShane.Neoforce.Controls.EventHandler(btnClear_Click);
            #endregion

            #endregion

            #region  /// Third Page ///
            tbcMain.AddPage();
            tbcMain.TabPages[2].Text = "Third";
            TabPage third = tbcMain.TabPages[2];
            HeightCounter = 0;

            #region /// Presets ///

            lstPresets = new ListBox(manager);
            lstPresets.Init();
            lstPresets.Parent = third;
            lstPresets.Top = third.Top;
            lstPresets.Left = LeftPadding;
            lstPresets.Width = third.Width - LeftPadding * 2;
            lstPresets.Height = third.Height / 4; HeightCounter += VertPadding + lstPresets.Height;
            lstPresets.Anchor = Anchors.Top | Anchors.Left | Anchors.Bottom;
            lstPresets.HideSelection = false;
            lstPresets.ItemIndexChanged += new TomShane.Neoforce.Controls.EventHandler(lstPresets_ItemIndexChanged);
            //lstMain.Click += new TomShane.Neoforce.Controls.EventHandler(lstMain_Click);
            lstPresets.Items = game.NodePresets;
            
            #region /// Presets ContextMenu ///
            presetContextMenu = new ContextMenu(manager);
            deletePresetMenuItem = new MenuItem("Delete Preset");
            deletePresetMenuItem.Click += new TomShane.Neoforce.Controls.EventHandler(deletePresetMenuItem_Click);
            presetContextMenu.Items.Add(deletePresetMenuItem);
            #endregion
            lstPresets.ContextMenu = presetContextMenu;

            #endregion
            #endregion

#region /// Menu items ///

#endregion
        }

        public void UpdateNodesTitle()
        {
            title1.Text = "Node List : " + room.nodes.Count;
        }

        void applyToAllNodesMenuItem_Click(object sender, TomShane.Neoforce.Controls.EventArgs e) //TODO: fix the relection copying reference types
        {
            //MenuItem menuitem = (MenuItem)sender;
            TreeListItem item = (TreeListItem)lstComp.Items.ElementAt(lstComp.ItemIndex);
            if (item.itemtype == treeitem.propertyinfo)
            {
                foreach (Node node in game.room.nodes)
                {
                    item.propertyInfo.SetValue(node, item.propertyInfo.GetValue(item.node, null), null); // make this object compliant

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

        void toggleComponentMenuItem_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            TreeListItem item = (TreeListItem)lstComp.Items.ElementAt(lstComp.ItemIndex);
            if (item.itemtype != treeitem.component)
            {
                System.Console.WriteLine("Error: The list item was not a component.");
                return;
            }

            Component component = (Component)((Node)item.obj).comps[item.component];
            component.active = !component.active;
        }

        void removeComponentMenuItem_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            TreeListItem item = (TreeListItem)lstComp.Items.ElementAt(lstComp.ItemIndex);
            if (item.itemtype != treeitem.component)
            {
                System.Console.WriteLine("Error: The list item was not a component.");
                return;
            }

            Component component = (Component)((Node)item.obj).comps[item.component];
            component.active = false;
            ui.editNode.RemoveComponent(item.component);
            if (!ui.editNode.comps.ContainsKey(item.component))
            {
                lstComp.Items.RemoveAt(lstComp.ItemIndex);
            }
            //remove the children and item
            if (item.hasChildren)
            {
                if (item.extended)
                {
                    item.prefix = "+";
                    foreach (TreeListItem subitem in item.children)
                    {
                        lstComp.Items.Remove(subitem);
                    }
                }
            }
            lstComp.Items.Remove(item);
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
                    trkMain.Range = val * 2;
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
                //dynamic field = null;
                if (item.itemtype == treeitem.propertyinfo || item.itemtype == treeitem.objpropertyinfo)
                {
                    PropertyInfo property = item.propertyInfo;

                    if (property.GetValue(parentObject, null) == null) return;

                    if (property.PropertyType.ToString().Equals("System.Int32"))
                    {
                        property.SetValue(parentObject, trkbar.Value, null);
                        gp.panelControls["txtbox"].Text = "" + trkbar.Value;
                    }
                    else if (property.PropertyType.ToString().Equals("System.Single"))
                    {
                        property.SetValue(parentObject, Convert.ToSingle(trkbar.Value), null);
                        gp.panelControls["txtbox"].Text = "" + trkbar.Value;
                        //field.SetValue(10.0f, parentObject);

                    }
                }
                else if (item.itemtype == treeitem.fieldinfo || item.itemtype == treeitem.objfieldinfo)
                {
                    FieldInfo field = item.fieldInfo;

                    if (field.GetValue(parentObject) == null) return;

                    if (field.FieldType.ToString().Equals("System.Int32"))
                    {
                        field.SetValue(parentObject, trkbar.Value);
                        gp.panelControls["txtbox"].Text = "" + trkbar.Value;
                    }
                    else if (field.FieldType.Equals("System.Single"))
                    {
                        field.SetValue(parentObject, Convert.ToSingle(trkbar.Value));
                        gp.panelControls["txtbox"].Text = "" + trkbar.Value;
                        //field.SetValue(10.0f, parentObject);

                    }

                }





            }
        }

        void chkbox_CheckedChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {

            CheckBox checkbox = (CheckBox)sender;
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
                else if (item.itemtype == treeitem.propertyinfo || item.itemtype == treeitem.objpropertyinfo)
                {
                    item.propertyInfo.SetValue(parentObject, checkbox.Checked, null);
                    checkbox.Text = item.propertyInfo.Name + " (" + item.propertyInfo.GetValue(parentObject, null) + ")";
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
            ui.editNode = game.targetNode;
            lblEditNodeName.Text = ui.editNode.name;

            lstComp.Items = TreeListItem.GenerateList((Node)listbox.Items.ElementAt(listbox.ItemIndex), "");

        }

        void lstPresets_ItemIndexChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            ListBox listbox = (ListBox)sender;
            //remove panelControl elements (from groupPanel at the bottom)
            if (groupPanel.panelControls.Keys.Count > 0) DisableControls(groupPanel);
            //System.Console.WriteLine("" + treebox.ItemIndex);
            //game.room.defaultNode = (Node)listbox.Items.ElementAt(listbox.ItemIndex);
            ui.editNode = (Node)listbox.Items.ElementAt(listbox.ItemIndex);
            lblEditNodeName.Text = ui.editNode.name;
            ui.spawnerNode = ui.editNode;
            //ui.editNode = game.targetNode;

            lstComp.Items = TreeListItem.GenerateList(ui.editNode, "");
            if (cmbPresets.ItemIndex != lstPresets.ItemIndex)
            {
                cmbPresets.ItemIndex = lstPresets.ItemIndex;
            }

        }

        void deletePresetMenuItem_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            //string filepath = "Presets//Nodes";
            //DirectoryInfo d = new DirectoryInfo(filepath);

            FileInfo fileinfo = game.presetFileInfos.ElementAt(lstPresets.ItemIndex);
            System.Console.WriteLine("Deleting file: " + fileinfo.Name);

            string message = "Are you sure you want to delete the file: " + fileinfo.Name;
            PopupWindow confirmDelete = new PopupWindow(game, "showMessage", message);
            confirmDelegate deleteDel = delegate(bool del)
            {
                if (del)
                {
                    fileinfo.Delete();
                    game.presetFileInfos.Remove(fileinfo);
                    lstPresets.Items.RemoveAt(lstPresets.ItemIndex);
                }
            };
            confirmDelete.addDelegate(deleteDel);

            //System.Console.WriteLine("name ::: " + d.FullName);
            /*
            foreach (FileInfo file in d.GetFiles("*.xml"))
            {
                string filename = file.Name;
                //System.Console.WriteLine(filename);
                //string path = file.FullName;
                filename = "Presets//Nodes//" + filename;
                NodePresets.Add((Node)room.serializer.Deserialize(filename));
                presetFileInfos.Add(file);

            }
            //*/
        }

        void cmbPresets_ItemIndexChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {

            ComboBox combobox = (ComboBox)sender;
            /*
            if (groupPanel.panelControls.Keys.Count > 0) DisableControls(groupPanel);
            ui.editNode = (Node)combobox.Items.ElementAt(combobox.ItemIndex);
            lblEditNodeName.Text = ui.editNode.name;
            */
            System.Console.WriteLine("num : {0}", cmbPresets.ItemIndex);
            if (combobox.ItemIndex != lstPresets.ItemIndex)
            {
                lstPresets.ItemIndex = combobox.ItemIndex;
            }
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
            MouseEventArgs mouseArgs = (MouseEventArgs)e;
            if (mouseArgs.Button == MouseButton.Right)
            {
                contextMenulstComp.Items.RemoveRange(0, contextMenulstComp.Items.Count);
                TreeListItem litem = (TreeListItem)listComp.Items.ElementAt(listComp.ItemIndex);

                if (litem.itemtype == treeitem.component)
                {
                    contextMenulstComp.Items.Add(toggleComponentMenuItem);
                    contextMenulstComp.Items.Add(removeComponentMenuItem);
                    contextMenulstComp.Items.Add(applyToAllNodesMenuItem);
                }
                else
                {
                    contextMenulstComp.Items.Add(applyToAllNodesMenuItem);
                }

            }
            else if (mouseArgs.Button == MouseButton.Left)
            {

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
                            listComp.Items.Insert(listComp.ItemIndex + i++, subitem);
                        }
                    }
                    item.extended = !item.extended;
                }

                UpdateGroupPanel(item, groupPanel);
            }
        }

        void chkTempNodes_CheckedChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            //TODO: Ask harley what to do

        }

        void btnAddComponent_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            PopupWindow addComponentWindow = new PopupWindow(game, "addComponent");
            // if it's open don't open again... (TODO)
        }

        void btnDefaultNode_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            lstComp.Items = TreeListItem.GenerateList(game.room.defaultNode, "");
            ui.editNode = game.room.defaultNode;
            ui.spawnerNode = ui.editNode;
            lblEditNodeName.Text = ui.editNode.name;
        }

        void btnRemoveAllNodes_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            game.room.RemoveAllNodes();
            game.targetNode = null;

            if (ui.editNode != game.room.defaultNode && !lstPresets.Items.Contains(ui.editNode))
            {
                lstComp.Items.Clear();
                ui.editNode = null;
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
            if (ui.editNode != game.room.defaultNode && !lstPresets.Items.Contains(ui.editNode))
            {
                lstComp.Items.Clear();
                ui.editNode = null;
            }
            DisableControls(groupPanel);
        }

        public void UpdateNodeList(Node node)
        {
            //lstMain.Items.AddRange(
            //lstMain.Items.Add(node);

        }
    }
}
