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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using OrbItProcs;
using OrbItProcs.Processes;

using Component = OrbItProcs.Components.Component;
using Console = System.Console;
using EventHandler = TomShane.Neoforce.Controls.EventHandler;
using EventArgs = TomShane.Neoforce.Controls.EventArgs;

namespace OrbItProcs.Interface
{
    public partial class Sidebar
    {
        EventHandler NotImplemented;
        public Game1 game;
        public Room room;
        public UserInterface ui;
        private Group _ActiveGroup;
        public Group ActiveGroup { 
            get 
            { 
                if (cmbListPicker != null && cmbListPicker.ItemIndex != -1 && room != null && room.masterGroup != null)
                {
                    string name = cmbListPicker.Text;
                    if (room.masterGroup.childGroups.ContainsKey(name))
                    {
                        return room.masterGroup.childGroups[name];
                    }
                    //Console.WriteLine("Group couldn't be found while getting ActiveGroup property.");
                    return room.masterGroup;
                }
                else
                {
                    //Console.WriteLine("Group couldn't be found while getting ActiveGroup property.");
                    return room.masterGroup;
                }
            }
            //set { _ActiveGroup = value; }
        }
        public Node ActiveDefaultNode
        {
            get
            {
                if (cmbListPicker != null && cmbListPicker.ItemIndex != -1 && room != null && room.masterGroup != null)
                {
                    string name = cmbListPicker.Text;
                    if (room.masterGroup.childGroups.ContainsKey(name))
                    {
                        return room.masterGroup.childGroups[name].defaultNode;
                    }
                }
                return room.masterGroup.defaultNode;
            }
        }
        public InspectorItem ActiveInspectorParent;
        
        public int Width = 200;
        #region /// Neoforce Fields///
        public Manager manager;
        public Window master;
        TabControl tbcMain;
        public Label title1;
        TextBox consoletextbox;
        public ListBox lstMain;
        private CheckBox chkTempNodes;
        public ComboBox cmbListPicker;
        Button btnRemoveNode, btnRemoveAllNodes, btnAddComponent, btnDefaultNode, btnApplyToAll, btnSaveNode;
        public TreeListBox lstComp;
        //public List<object> compLst { set { lstComp.Items.Clear(); foreach (object o in value) lstComp.Items.Add(o); } }
        public ContextMenu contextMenulstComp;
        public Label lblEditNodeName;
        public Label lblInspectorAddress;
        public GroupPanel groupPanel;
        public Dictionary<string, Control> panelControls;
        //private TreeListItem activeTreeItem;
        private InspectorItem activeInspectorItem;
        private object parentObject;
        //public Node ui.editNode, ui.spawnerNode;
        public ListBox lstPresets;
        public ComboBox cmbPresets;
        public MenuItem applyToAllNodesMenuItem, toggleComponentMenuItem, removeComponentMenuItem, toggleBoolMenuItem;
        public ContextMenu presetContextMenu;
        public MenuItem deletePresetMenuItem;

        public ContextMenu mainNodeContextMenu;
        public MenuItem ConvertIntoList, PromoteToDefault;
        
        StackPanel stackpanel;
        GroupPanel gp;
        

        public PropertyEditPanel propertyEditPanel;
        #endregion

        #region /// Layout Fields///
        private int HeightCounter = 0, HeightCounter2 = 0;
        private int lstMainScrollPosition = 0;
        private int lstCompScrollPosition = 0;
        private int LeftPadding = 5;
        private int VertPadding = 4;
        #endregion

        public Sidebar(UserInterface ui)
        {
            this.game = ui.game;
            this.room = ui.game.room;
            this.ui = ui;
            NotImplemented = delegate { 
                PopUp.Toast(ui, "Not Implemented. Take a hike.");
                //throw new NotImplementedException();
            };
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
            lstMain.ItemIndexChanged += lstMain_ItemIndexChanged;
            lstMain.Click += lstMain_Click;
            //room.nodes.CollectionChanged += nodes_Sync;

            mainNodeContextMenu = new ContextMenu(manager);
            ConvertIntoList = new MenuItem("Make Default of new Group.");
            ConvertIntoList.Click += ConvertIntoList_Click;
            PromoteToDefault = new MenuItem("Make Default of current Group");
            PromoteToDefault.Click += PromoteToDefault_Click;
            mainNodeContextMenu.Items.Add(ConvertIntoList);
            lstMain.ContextMenu = mainNodeContextMenu;
            #endregion

            #region /// CheckBox ///
            /*
            chkTempNodes = new CheckBox(manager);
            chkTempNodes.Init();
            chkTempNodes.Parent = first;

            chkTempNodes.Left = LeftPadding;
            chkTempNodes.Top = HeightCounter;
            chkTempNodes.Width = first.Width - LeftPadding * 2; HeightCounter += VertPadding + chkTempNodes.Height;
            chkTempNodes.Anchor = Anchors.Left | Anchors.Bottom | Anchors.Right;

            chkTempNodes.Checked = false; // TODO : Nessecary?
            chkTempNodes.Text = "Show TempNodes";
            chkTempNodes.ToolTip.Text = "Enables or disables \nshowing temp nodes \nin the list.";
            // TODO : IMPLEMENT: chkTempNodes.CheckedChanged += chkTempNodes_CheckedChanged);
            */
            #endregion

            #region  /// List Picker ///

            cmbListPicker = new ComboBox(manager);
            cmbListPicker.Init();
            cmbListPicker.Parent = first;
            cmbListPicker.MaxItems = 20;

            cmbListPicker.Width = first.Width - LeftPadding * 6;
            cmbListPicker.Left = LeftPadding;
            cmbListPicker.Top = HeightCounter; HeightCounter += VertPadding + cmbListPicker.Height;
            cmbListPicker.Items.Add("Other Objects");
            cmbListPicker.ItemIndex = 0;
            cmbListPicker.ItemIndexChanged += cmbListPicker_ItemIndexChanged;

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
            btnRemoveNode.Click += btnRemoveNode_Click;
            #endregion

            #region  /// Remove All Nodes Button ///
            btnRemoveAllNodes = new Button(manager);
            btnRemoveAllNodes.Init();
            btnRemoveAllNodes.Parent = first;

            btnRemoveAllNodes.Top = HeightCounter;
            //btnRemoveAllNodes.Width = first.Width / 2 - LeftPadding;
            btnRemoveAllNodes.Width = first.Width / 2 - LeftPadding;
            btnRemoveAllNodes.Height = 24; HeightCounter += VertPadding + btnRemoveAllNodes.Height;
            btnRemoveAllNodes.Left = LeftPadding + btnRemoveNode.Width;

            btnRemoveAllNodes.Text = "Remove All";
            btnRemoveAllNodes.Click += btnRemoveAllNodes_Click;
            #endregion

            #region  /// Add Componenet ///
            btnAddComponent = new Button(manager);
            btnAddComponent.Init();
            btnAddComponent.Parent = first;

            btnAddComponent.Top = HeightCounter;
            btnAddComponent.Width = first.Width / 2 - LeftPadding;
            btnAddComponent.Height = 20;
            btnAddComponent.Left = LeftPadding;

            btnAddComponent.Text = "Add Component";
            btnAddComponent.Click += btnAddComponent_Click;
            #endregion

            #region  /// Default Node ///
            btnDefaultNode = new Button(manager);
            btnDefaultNode.Init();
            btnDefaultNode.Parent = first;

            btnDefaultNode.Top = HeightCounter;
            btnDefaultNode.Width = first.Width / 2 - LeftPadding;
            btnDefaultNode.Height = 20; HeightCounter += VertPadding + btnDefaultNode.Height;
            btnDefaultNode.Left = LeftPadding + btnRemoveNode.Width;

            btnDefaultNode.Text = "Default Node";
            btnDefaultNode.Click += btnDefaultNode_Click;
            #endregion

            #region  /// Presets Dropdown ///
            cmbPresets = new ComboBox(manager);
            cmbPresets.Init();
            cmbPresets.Parent = first;
            cmbPresets.MaxItems = 20;
            cmbPresets.Width = 160;
            cmbPresets.Left = LeftPadding;
            cmbPresets.Top = HeightCounter; HeightCounter += cmbPresets.Height;
            game.NodePresets.CollectionChanged += NodePresets_Sync;
            cmbPresets.ItemIndexChanged += cmbPresets_ItemIndexChanged;
            cmbPresets.Click += cmbPresets_Click;
            #endregion
            /*
            #region  /// Edit Node Name ///
            lblEditNodeName = new Label(manager);
            lblEditNodeName.Init();
            lblEditNodeName.Parent = first;
            lblEditNodeName.Top = HeightCounter; HeightCounter += VertPadding + lblEditNodeName.Height;
            lblEditNodeName.Width = first.Width - LeftPadding * 2;
            lblEditNodeName.Left = first.Width / 5;
            lblEditNodeName.Anchor = Anchors.Left;
            lblEditNodeName.Text = ">No Node Selected<";
            #endregion
            */
            #region  /// Inspector Address Label ///
            lblInspectorAddress = new Label(manager);
            lblInspectorAddress.Init();
            lblInspectorAddress.Parent = first;
            lblInspectorAddress.Top = HeightCounter; HeightCounter += VertPadding + lblInspectorAddress.Height + 10;
            lblInspectorAddress.Width = first.Width - LeftPadding * 2;
            lblInspectorAddress.Height = lblInspectorAddress.Height * 2;
            lblInspectorAddress.Left = LeftPadding;
            lblInspectorAddress.Anchor = Anchors.Left;
            lblInspectorAddress.Text = ">No Node Selected<\u2190";
            bool changed = false;
            lblInspectorAddress.TextChanged += delegate(object s, EventArgs e){
                if (!changed)
                {
                    changed = true;
                    Label l = (Label)s;
                    l.Text = l.Text.wordWrap(25);
                    
                }
                changed = false;
            };
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
            lstComp.ItemIndexChanged += lstComp_ItemIndexChanged;
            lstComp.Click += lstComp_Click;
            lstComp.DoubleClick += lstComp_DoubleClick;
            #region  /// Context Menu ///
            contextMenulstComp = new ContextMenu(manager);
            applyToAllNodesMenuItem = new MenuItem("Apply to Group");
            applyToAllNodesMenuItem.Click += applyToAllNodesMenuItem_Click;
            //applyToAllNodesMenuItem.Click += NotImplemented;
            toggleComponentMenuItem = new MenuItem("Toggle Component");
            //toggleComponentMenuItem.Click += NotImplemented;
            toggleComponentMenuItem.Click += toggleComponentMenuItem_Click;
            removeComponentMenuItem = new MenuItem("Remove Component");
            //removeComponentMenuItem.Click += NotImplemented;
            removeComponentMenuItem.Click += removeComponentMenuItem_Click;
            toggleBoolMenuItem = new MenuItem("Toggle");
            toggleBoolMenuItem.Click += toggleBoolMenuItem_Click;


            contextMenulstComp.Items.Add(applyToAllNodesMenuItem);

            lstComp.ContextMenu = contextMenulstComp;
            
            #endregion
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

            panelControls = new Dictionary<string, Control>();
            groupPanel.Text = "Property";
            #endregion

            #region  /// PropertyEditPanel ///
            propertyEditPanel = new PropertyEditPanel(groupPanel, this);
            #endregion

            #region  /// Apply to Group ///
            btnApplyToAll = new Button(manager);
            btnApplyToAll.Init();
            btnApplyToAll.Parent = first;

            btnApplyToAll.Text = "Apply To Group";
            btnApplyToAll.Top = HeightCounter;
            btnApplyToAll.Width = first.Width / 2 - LeftPadding;
            btnApplyToAll.Height = 20; //HeightCounter += VertPadding + btnApplyToAll.Height;
            btnApplyToAll.Left = LeftPadding;
            btnApplyToAll.Click += applyToAllNodesMenuItem_Click; //TODO ??
            #endregion

            #region  /// Save as Preset ///
            btnSaveNode = new Button(manager);
            btnSaveNode.Init();
            btnSaveNode.Text = "Save Node";
            btnSaveNode.Top = HeightCounter;
            btnSaveNode.Width = first.Width / 2 - LeftPadding;
            btnSaveNode.Height = 20; HeightCounter += VertPadding + btnSaveNode.Height;
            btnSaveNode.Left = LeftPadding + btnApplyToAll.Width;
            btnSaveNode.Parent = first;
            btnSaveNode.Click += btnSaveNode_Click;
                
            #endregion
            #endregion

            #region  /// Page 2 ///
            tbcMain.AddPage();
            tbcMain.TabPages[1].Text = "Second";
            TabPage second = tbcMain.TabPages[1];
            HeightCounter = 0;

            
            
            #endregion

            #region  /// Page 3 ///
            tbcMain.AddPage();
            tbcMain.TabPages[2].Text = "Third";
            TabPage third = tbcMain.TabPages[2];
            HeightCounter = 0;

            #region  /// Title (Console) ///
            Label thirdTitle = new Label(manager);
            thirdTitle.Init();
            thirdTitle.Parent = third;

            thirdTitle.Top = VertPadding;
            thirdTitle.Left = third.Width / 2 - thirdTitle.Width;
            HeightCounter2 += VertPadding * 2 + thirdTitle.Height;
            thirdTitle.Anchor = Anchors.Left;

            thirdTitle.Text = "Console";
            #endregion

            #region  /// Console textbox ///
            consoletextbox = new TextBox(manager);
            consoletextbox.Init();
            consoletextbox.Parent = third;

            consoletextbox.Left = LeftPadding;
            consoletextbox.Top = HeightCounter2;
            HeightCounter2 += VertPadding + consoletextbox.Height;
            consoletextbox.Width = second.Width - LeftPadding * 2;
            consoletextbox.Height = consoletextbox.Height + 3;

            consoletextbox.ToolTip.Text = "Enter a command, and push enter";
            consoletextbox.KeyUp += consolePressed;
            #endregion

            #region  /// Enter Button ///
            Button btnEnter = new Button(manager);
            btnEnter.Init();
            btnEnter.Parent = third;

            btnEnter.Left = LeftPadding;
            btnEnter.Top = HeightCounter2;
            btnEnter.Width = (second.Width - LeftPadding * 2) / 2;

            btnEnter.Text = "Enter";
            btnEnter.Click += consolePressed;
            #endregion

            #region  /// Clear ///
            Button btnClear = new Button(manager);
            btnClear.Init();
            btnClear.Parent = third;

            btnClear.Left = LeftPadding + btnEnter.Width;
            btnClear.Top = HeightCounter2; HeightCounter2 += VertPadding + btnClear.Height;
            btnClear.Width = (second.Width - LeftPadding * 2) / 2;

            btnClear.Text = "Clear";
            btnClear.Click += btnClear_Click;
            #endregion

            #region  /// Label (Presets) ///
            Label lblPresets = new Label(manager);
            lblPresets.Init();
            lblPresets.Parent = third;

            lblPresets.Top = HeightCounter2;
            lblPresets.Left = third.Width / 2 - lblPresets.Width;
            HeightCounter2 += VertPadding * 2 + lblPresets.Height;
            lblPresets.Anchor = Anchors.Left;

            lblPresets.Text = "Presets";
            #endregion

            #region /// Presets ///

            lstPresets = new ListBox(manager);
            lstPresets.Init();
            lstPresets.Parent = third;
            lstPresets.Top = HeightCounter2;
            lstPresets.Left = LeftPadding;
            lstPresets.Width = third.Width - LeftPadding * 2;
            lstPresets.Height = third.Height / 4; HeightCounter += VertPadding + lstPresets.Height;
            lstPresets.Anchor = Anchors.Top | Anchors.Left | Anchors.Bottom;
            lstPresets.HideSelection = false;
            lstPresets.ItemIndexChanged += lstPresets_ItemIndexChanged;
            
            // go to cmbPresets to find the preset synching reference.
            
            #region /// Presets ContextMenu ///
            presetContextMenu = new ContextMenu(manager);
            deletePresetMenuItem = new MenuItem("Delete Preset");
            deletePresetMenuItem.Click += deletePresetMenuItem_Click;
            presetContextMenu.Items.Add(deletePresetMenuItem);
            presetContextMenu.Enabled = false;
            #endregion
            lstPresets.ContextMenu = presetContextMenu;


            #endregion
            #endregion

            ResetTreeListBox(lstComp, ActiveDefaultNode);

            //lstComp.BackColor = Color.Blue;
            //lstComp.Color = Color.Black;
            //lstComp.TextColor = Color.Green;

            InitializeSecondPage();


            #region StackPanel Testing
            /*
            Window win = new Window(manager);
            win.Init();
            win.Top = 100;
            win.Left = 100;
            win.Height = 200;
            win.Width = 300;
            manager.Add(win);

            stackpanel = new StackPanel(manager, Orientation.Vertical);
           
            stackpanel.Width = 100;
            stackpanel.Height = 100;
            stackpanel.Visible = true;
            Button b1 = new Button(manager);
            b1.Init();
            b1.Text = "b1";
            b1.Click += b2_Click;
            stackpanel.Add(b1);
            Button b2 = new Button(manager);
            b2.Init();
            b2.Text = "b2";
            b2.Click += b2_Click;
            Button b3 = new Button(manager);
            b3.Init();
            stackpanel.Add(b3);
            b3.Text = "b3";
            b3.Click += b2_Click;
            win.Add(stackpanel);
            stackpanel.Init();
            stackpanel.Resize += stackpanel_Resize;


            gp = new GroupPanel(manager);
            gp.Height = 100;
            gp.Width = 100;
            gp.Init();
            win.Add(gp);
            gp.Text = "   Panel";

            b2.Left = 100;
            b2.Height = 20;
            b2.Width = 15;
            b2.Text = "^";
            gp.AutoScroll = true;
            

            TabControl tb = new TabControl(manager);
            tb.AddPage("first");
            tb.AddPage("second");
            


            tb.AutoScroll = true;
            win.Add(tb);
            tb.Init();
            tb.Height = 300;
            tb.Width = 200;
            
            tb.Refresh();
            gp.Refresh();

            GroupBox box = new GroupBox(manager);
            box.AutoScroll = true;
            box.Init();

            box.Add(b2);
            b2.Top = 500;
            b2.Left = 500;
            box.Refresh();

            box.Width = tb.TabPages[0].Width;
            box.Height = tb.TabPages[0].Height;


            tb.TabPages[0].Add(box);
            tb.TabPages[0].Refresh();
            */
            #endregion
        }

        void stackpanel_Resize(object sender, ResizeEventArgs e)
        {
            Console.WriteLine("resized");
        }

        void b2_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            if (b.Text.Equals("^")) b.Text = "v";
            else b.Text = "^";


            if (gp.Height == 100)
                gp.Height = 20;
            else gp.Height = 100;

            stackpanel.Refresh();
        }

        void PromoteToDefault_Click(object sender, EventArgs e)
        {
            Node n = (Node)lstMain.Items.ElementAt(lstMain.ItemIndex);
            Node newdefault = new Node();
            Node.cloneObject(n, newdefault);
            Group g = ActiveGroup;
            g.defaultNode = newdefault;
            g.fullSet.Remove(n);
            SetDefaultNodeAsEdit();
        }

        void ConvertIntoList_Click(object sender, EventArgs e)
        {
            Node n = (Node)lstMain.Items.ElementAt(lstMain.ItemIndex);
            Node newdefault = new Node();
            Node.cloneObject(n, newdefault);
            newdefault.transform.velocity = new Vector2(0, 0);
            Group g = new Group(newdefault, parentGroup: room.masterGroup, Name: newdefault.name);
            room.masterGroup.AddGroup(g.Name, g);
            Group active = ActiveGroup;
            active.fullSet.Remove(n);
            
            g.fullSet.Add(n);

            int index = cmbListPicker.Items.IndexOf(newdefault.name);
            cmbListPicker.ItemIndex = index;
        }

        void lstMain_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button != MouseButton.Right) return;
            ListBox listbox = (ListBox)sender;
            listbox.ContextMenu.Items.ToList().ForEach(o => listbox.ContextMenu.Items.Remove(o));
            //foreach (MenuItem m in listbox.ContextMenu.Items.ToList())
            //{
            //    listbox.ContextMenu.Items.Remove(m);
            //}
            Color c = listbox.ContextMenu.Color;
            listbox.ContextMenu.Color = new Color(0f, 0f, 0f, 0f);
            if(listbox.ItemIndex >= 0 && listbox.Items.ElementAt(listbox.ItemIndex) is Node)
            {
                listbox.ContextMenu.Color = new Color(1f, 1f, 1f, 1.0f);
                listbox.ContextMenu.Items.Add(ConvertIntoList);
                listbox.ContextMenu.Items.Add(PromoteToDefault);
            }
        }

        void cmbListPicker_ItemIndexChanged(object sender, EventArgs e)
        {
            
            ComboBox cmb = (ComboBox)sender;
            string item = cmb.Items.ElementAt(cmb.ItemIndex).ToString();
            if (item.Equals("Other Objects"))
            {
                foreach (object o in lstMain.Items.ToList())
                {
                    lstMain.Items.Remove(o);
                }
                lstMain.Items.Add(room.game);
                lstMain.Items.Add(room);
                lstMain.Items.Add(room.masterGroup);
            }
            else if (!item.Equals(""))
            {
                foreach (object o in lstMain.Items.ToList())
                {
                    lstMain.Items.Remove(o);
                }

                Group find = room.masterGroup.FindGroup(item);
                if (find == null) return;
                lstMain.Items.AddRange(find.entities);
                SyncTitleNumber(find);

                SetDefaultNodeAsEdit();

            }
            lstMain.ScrollTo(0);
        }

        public void ResetTreeListBox(TreeListBox treelistbox, object rootobj)
        {
            treelistbox.ItemIndex = 0;
            InspectorItem rootitem = new InspectorItem(treelistbox.Items, rootobj);
            treelistbox.rootitem = rootitem;
            rootitem.GenerateChildren();
            ActiveInspectorParent = rootitem;

            foreach (object o in treelistbox.Items.ToList())
            {
                treelistbox.Items.Remove(o);
            }
            foreach (object o in rootitem.children.ToList())
            {
                treelistbox.Items.Add(o);
            }
            if (rootobj is Node)
            {
                ui.editNode = (Node)rootobj;
                //lblEditNodeName.Text = ui.editNode.name;
                lblInspectorAddress.Text = "/" + ui.editNode.ToString();
            }
            else
            {
                //lblEditNodeName.Text = rootobj.GetType().ToString();
                lblInspectorAddress.Text = rootobj.GetType().ToString();
            }

        }

        void NodePresets_Sync(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (((ObservableCollection<Object>)sender).Count() < 1) presetContextMenu.Enabled = false;
            else presetContextMenu.Enabled = true;
            cmbPresets.Items.syncToOCDelegate(e);
            lstPresets.Items.syncToOCDelegate(e);
        }

        void nodes_Sync(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            int count = ActiveGroup == null ? 0 : ActiveGroup.entities.Count;
            title1.Text = "Node List : " + count;
            if (cmbListPicker.Text.Equals("Nodes"))
                lstMain.Items.syncToOCDelegate(e);
        }

        public void SyncTitleNumber(Group caller)
        {
            Group g = ActiveGroup;
            if (g != caller) return;
            int count = g == null ? 0 : g.entities.Count;
            title1.Text = g.Name + " : " + count;
        }

        void btnSaveNode_Click(object sender, EventArgs e)
        {
            if (ui.editNode == null)
                PopUp.Toast(ui, "You haven't selected a Node.");
            else
                PopUp.Text(ui, "Pick a preset name", "Name preset",
                                delegate(bool c, object input) {
                                    if (c) ui.game.saveNode(ui.editNode, (string)input);
                                        return true; });
        }

        void applyToAllNodesMenuItem_Click(object sender, TomShane.Neoforce.Controls.EventArgs e) //TODO: fix the relection copying reference types
        {
            List<InspectorItem> itemspath = new List<InspectorItem>();
            InspectorItem item = (InspectorItem)lstComp.Items.ElementAt(lstComp.ItemIndex);
            object value = item.GetValue();

            BuildItemsPath(item, itemspath);

            Group activeGroup = ActiveGroup;
            activeGroup.ForEachAllSets(delegate(Node o)
            {
                Node n = (Node)o;
                if (n == itemspath.ElementAt(0).obj) return;
                InspectorItem temp = new InspectorItem(null, n);
                int count = 0;
                foreach (InspectorItem pathitem in itemspath)
                {
                    if (temp.obj.GetType() != pathitem.obj.GetType())
                    {
                        Console.WriteLine("The paths did not match while applying to all. {0} != {1}", temp.obj.GetType(), pathitem.obj.GetType());
                        break;
                    }
                    if (count == itemspath.Count - 1) //last item
                    {
                        if (pathitem.membertype == member_type.dictentry)
                        {
                            dynamic dict = temp.parentItem.obj;
                            dynamic key = pathitem.key;
                            if (!dict.ContainsKey(key)) break;
                            if (dict[key] is Component)
                            {
                                dict[key].active = ((Component)value).active;
                            }
                            else if (temp.IsPanelType())
                            {
                                dict[key] = value;
                            }
                        }
                        else
                        {
                            if (value is Component)
                            {
                                ((Component)temp.obj).active = ((Component)value).active;
                            }
                            else if (temp.IsPanelType())
                            {
                                temp.fpinfo.SetValue(value, temp.parentItem.obj);
                            }
                        }
                    }
                    else
                    {
                        InspectorItem next = itemspath.ElementAt(count + 1);
                        if (next.membertype == member_type.dictentry)
                        {
                            dynamic dict = temp.obj;
                            dynamic key = next.key;
                            if (!dict.ContainsKey(key)) break;
                            temp = new InspectorItem(null, temp, dict[key], key);
                        }
                        else
                        {
                            temp = new InspectorItem(null, temp, next.fpinfo.GetValue(temp.obj), next.fpinfo.propertyInfo);
                        }
                    }
                    count++;
                }
            });
        }

        public void BuildItemsPath(InspectorItem item, List<InspectorItem> itemspath)
        {
            InspectorItem temp = item;
            itemspath.Insert(0, temp);
            while (temp.parentItem != null)
            {
                temp = temp.parentItem;
                itemspath.Insert(0, temp);
            }
        }

        
        void toggleComponentMenuItem_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            InspectorItem item = (InspectorItem)lstComp.Items.ElementAt(lstComp.ItemIndex);
            if (!(item.obj is Component))
            {
                Console.WriteLine("Error: The list item was not a component.");
                return;
            }

            Component component = (Component)item.obj;
            component.active = !component.active;
        }

        void toggleBoolMenuItem_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            InspectorItem item = (InspectorItem)lstComp.Items.ElementAt(lstComp.ItemIndex);
            if (!(item.obj is bool))
            {
                Console.WriteLine("Error: The list item was not a bool.");
                return;
            }

            item.SetValue(!(bool)item.GetValue());
        }


        void removeComponentMenuItem_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            InspectorItem item = (InspectorItem)lstComp.Items.ElementAt(lstComp.ItemIndex);
            if (!(item.obj is Component))
            {
                Console.WriteLine("Error: The list item was not a component.");
                return;
            }

            Component component = (Component)item.obj;
            component.active = false;
            ui.editNode.RemoveComponent(item.component);
            item.RemoveChildren();
            lstComp.Items.Remove(item);
        }


        void consolePressed(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            if (sender is Button || (sender is TextBox && (((KeyEventArgs)e).Key == Keys.Enter))) 
                ProcessConsoleCommand(consoletextbox.Text);
        }

        public void ProcessConsoleCommand(String text)
        {
            text = text.Trim();

            if (text.Equals(""))
            {
                PopUp.Toast(ui, "No Command Provided");
                consoletextbox.Text = "";
                return;
            }
            object currentObj = game.room;



            List<String> args = text.Split(' ').ToList();
            String methodname;
            if (args.Count > 0)
            {
                methodname = args.ElementAt(0);
                args.RemoveAt(0);
            }
            else
            {
                PopUp.Toast(ui, "No Command Provided");
                return;
            }

            MethodInfo methinfo = currentObj.GetType().GetMethod(methodname);

            if (methinfo == null || methinfo.IsPrivate)
            {
                PopUp.Toast(ui, "Invalid method specification.");
                return;
            }

            ParameterInfo[] paraminfos = methinfo.GetParameters();

            int paramNum = paraminfos.Length;
            object[] finalargs = new object[paramNum];

            for(int i = 0; i < paramNum; i++)
            {

                Type ptype = paraminfos[i].ParameterType;
                if (i >= args.Count)
                {
                    if (paraminfos[i].IsOptional)
                    {
                        finalargs[i] = Type.Missing;
                        continue;
                    }
                    PopUp.Toast(ui, "Parameter Inconsistenc[ies].");
                    return;
                }
                try
                {
                  finalargs[i] = TypeDescriptor.GetConverter(ptype).ConvertFromInvariantString(args[i]);
                }
                catch (Exception e)
                {
                    PopUp.Toast(ui, "Casting exception: " + e.Message);
                    return;
                }

            }
            if (methinfo.IsStatic) currentObj = null;
            try
            {
                methinfo.Invoke(currentObj, finalargs);
            }
            catch (Exception e)
            {
                PopUp.Toast(ui, "Invoking exception: " + e.Message);
                return;
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
        
        public void lstMain_ChangeScrollPosition(int change)
        {
            
            if (lstMainScrollPosition + change < 0) lstMainScrollPosition = 0;
            else if (lstMainScrollPosition + change > lstMain.Items.Count-7) lstMainScrollPosition = lstMain.Items.Count-7;
            else lstMainScrollPosition += change;
            lstMain.ScrollTo(lstMain.Items.Count - 1);
            lstMain.ScrollTo(lstMainScrollPosition);
        }

        public void lstComp_ChangeScrollPosition(int change)
        {
            if (lstCompScrollPosition + change < 0) lstCompScrollPosition = 0;
            else if (lstCompScrollPosition + change > lstComp.Items.Count-9) lstCompScrollPosition = lstComp.Items.Count-9;
            else lstCompScrollPosition += change;
            lstComp.ScrollTo(lstComp.Items.Count - 1);
            lstComp.ScrollTo(lstCompScrollPosition);
        }

        void lstMain_ItemIndexChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            ListBox listbox = (ListBox)sender;
            //remove panelControl elements (from groupPanel at the bottom)
            if (panelControls.Keys.Count > 0)
            {
                propertyEditPanel.DisableControls();
            }
            

            if (listbox.ItemIndex >= 0 && listbox.Items.ElementAt(listbox.ItemIndex) is Node)
            {
                SetTargetNode((Node)listbox.Items.ElementAt(listbox.ItemIndex));
            }
            else if (listbox.ItemIndex >= 0)
            {
                ResetTreeListBox(lstComp, listbox.Items.ElementAt(listbox.ItemIndex));
            }

        }

        public void SetTargetNode(Node target)
        {
            //if (game.targetNode == target) return;
            game.targetNode = target;
            
            if (ui.editNode != target)
                ResetTreeListBox(lstComp, game.targetNode);
            ui.editNode = target;
            //lblEditNodeName.Text = ui.editNode.name;
            //lblInspectorAddress.Text = "/" + ui.editNode.ToString();
            
        }

        void lstPresets_ItemIndexChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            ListBox listbox = (ListBox)sender;
            //remove panelControl elements (from groupPanel at the bottom)
            if (propertyEditPanel.panelControls.Keys.Count > 0) //DisableControls(groupPanel);
            {
                propertyEditPanel.DisableControls();
            }
            if (listbox.ItemIndex < 0) return;
            ui.editNode = (Node)listbox.Items.ElementAt(listbox.ItemIndex);
            //lblEditNodeName.Text = ui.editNode.name;
            lblInspectorAddress.Text = "/" + ui.editNode.ToString();
            ui.spawnerNode = ui.editNode;
            
            ResetTreeListBox(lstComp, ui.editNode);

            if (cmbPresets.ItemIndex != lstPresets.ItemIndex)
            {
                cmbPresets.ItemIndex = lstPresets.ItemIndex;
            }

        }

        void deletePresetMenuItem_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {

            String presetName = ((Node)lstPresets.selected()).name + ".xml";
            string message = "Are you sure you want to delete this preset file? : " + presetName;
            PopUp.Prompt(ui, message , action:
            delegate(bool del, object ans)
            {
                if (del)
                {
                    game.deletePreset((Node)lstPresets.selected());
                }
                return true;
            });
        }

        void cmbPresets_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            ComboBox combobox = (ComboBox)sender;
            if (combobox.ItemIndex >= 0)
            {
                lstPresets_ItemIndexChanged(lstPresets, e); //HACKs
            }
        }

        void cmbPresets_ItemIndexChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {

            ComboBox combobox = (ComboBox)sender;
            if (combobox.ItemIndex != lstPresets.ItemIndex)
            {
                lstPresets.ItemIndex = combobox.ItemIndex;
            }
        }

        void lstComp_ItemIndexChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            TreeListBox listComp = (TreeListBox)sender;

            if (listComp.ItemIndex < 0 || lstComp.Items.Count == 0) return;
            InspectorItem item = (InspectorItem)listComp.Items.ElementAt(listComp.ItemIndex);

            //UpdateGroupPanel(item, groupPanel);
        }

        void lstComp_DoubleClick(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            TreeListBox listComp = (TreeListBox)sender;
            MouseEventArgs mouseArgs = (MouseEventArgs)e;
            if (mouseArgs.Button == MouseButton.Left)
            {
                if (listComp.ItemIndex < 0 || lstComp.Items.Count == 0) return;
                InspectorItem item = (InspectorItem)listComp.Items.ElementAt(listComp.ItemIndex);
                item.DoubleClickItem(this);
                InspectorItem temp = ActiveInspectorParent;
                string lbltext = "/" + temp.Name();
                while (temp.parentItem != null)
                {
                    temp = temp.parentItem;
                    lbltext = lbltext.Insert(0, "/" + temp.Name());
                }
                lblInspectorAddress.Text = lbltext;

                //Type t = item.obj.GetType();

                //if (activeInspectorItem != item)
                //    propertyEditPanel.UpdatePanel(item);

            }
        }

        void lstComp_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            TreeListBox listComp = (TreeListBox)sender;
            MouseEventArgs mouseArgs = (MouseEventArgs)e;

            

            if (mouseArgs.Button == MouseButton.Right)
            {
                contextMenulstComp.Items.RemoveRange(0, contextMenulstComp.Items.Count);
                if (listComp.ItemIndex < 0 || lstComp.Items.Count == 0) return;


                InspectorItem litem = (InspectorItem)listComp.Items.ElementAt(listComp.ItemIndex);

                if (litem.obj is Component)
                {
                    contextMenulstComp.Items.Add(toggleComponentMenuItem);
                    contextMenulstComp.Items.Add(removeComponentMenuItem);
                    contextMenulstComp.Items.Add(applyToAllNodesMenuItem);
                }
                else
                {
                    if (litem.obj is bool)
                    {
                        contextMenulstComp.Items.Add(toggleBoolMenuItem);
                    }

                    contextMenulstComp.Items.Add(applyToAllNodesMenuItem);
                }

            }
            else if (mouseArgs.Button == MouseButton.Left)
            {
                if (listComp.ItemIndex < 0 || listComp.Items.Count == 0) return;
                InspectorItem item = (InspectorItem)listComp.Items.ElementAt(listComp.ItemIndex);
                //item.ClickItem(listComp.ItemIndex);
                Type t = item.obj.GetType();

                if (activeInspectorItem != item)
                    propertyEditPanel.UpdatePanel(item);
                
            }
        }

        void chkTempNodes_CheckedChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            //TODO: Ask harley what to do
        }

        void btnAddComponent_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            if (ui.editNode == null)
                PopUp.Toast(ui, "You haven't selected a Node.");
            else
            {
                ObservableCollection<dynamic> nodecomplist = new ObservableCollection<dynamic>((Enum.GetValues(typeof(comp)).Cast<dynamic>().Where(c => !ui.editNode.comps.ContainsKey(c))));
                List<dynamic> missingcomps = new List<dynamic>(Enum.GetValues(typeof(comp)).Cast<dynamic>().Where(c => ui.editNode.comps.ContainsKey(c)));

                PopUp.opt[] options = new PopUp.opt[]{
                    new PopUp.opt(PopUp.OptType.info, "Add component to: " + ui.editNode.name),
                    new PopUp.opt(PopUp.OptType.dropDown, nodecomplist),
                    new PopUp.opt(PopUp.OptType.checkBox, "Add to all", 
                        delegate(object s, TomShane.Neoforce.Controls.EventArgs a){
                            if ((s as CheckBox).Checked) nodecomplist.AddRange(missingcomps);
                            else nodecomplist.RemoveRange(missingcomps);})};
                
                PopUp.makePopup(ui, options, "Add Component", delegate(bool a, object[] o)
                {
                    if (a) return addComponent(o);
                    else return false;
                    });
            }
        }

        private bool addComponent(object[] o)
        {

            bool writeable = false;
            if ((bool)o[2])
            {
                foreach (Object n in ActiveGroup.fullSet)
                    if (!((Node)n).comps.ContainsKey((comp)o[1]))
                        ((Node)n).addComponent((comp)o[1], true);
                Node def = ActiveGroup.defaultNode;
                if (!(def).comps.ContainsKey((comp)o[1]))
                    (def).addComponent((comp)o[1], true);
                return true;
            }

            else
            {
                if (!ui.editNode.comps.ContainsKey((comp)o[1]))
                    ui.editNode.addComponent((comp)o[1], true);
                else PopUp.Prompt(ui,
                            "The node already contains this component. Overwrite to default component?",
                            action: delegate(bool k, object ans) { writeable = k; return true; });

                if (writeable)
                {
                    ui.editNode.addComponent((comp)o[1], true);
                    lstComp.rootitem.RefrestMasterList();
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        void btnDefaultNode_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            SetDefaultNodeAsEdit();
        }

        public void SetDefaultNodeAsEdit()
        {
            if (ui.editNode == ActiveDefaultNode) return;
            ui.editNode = ActiveDefaultNode;
            ui.spawnerNode = ui.editNode;

            ResetTreeListBox(lstComp, ActiveDefaultNode);

            //lblEditNodeName.Text = ui.editNode.name + "(DEFAULT)";
            lblInspectorAddress.Text = "/" + ui.editNode.ToString();
        }

        void btnRemoveAllNodes_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            Group g = ActiveGroup;
            if (g.fullSet.Contains(game.targetNode)) game.targetNode = null;
            if (g.fullSet.Contains(ui.editNode) && ui.editNode != g.defaultNode)
            {
                lstComp.Items.Clear();
                lstComp.rootitem = null;
                ui.editNode = null;
            }
            g.fullSet.ToList().ForEach(delegate(Node o) 
            {
                g.DeleteEntity(o);
            });

            lstMain.ItemIndex = -1;

        }

        void btnRemoveNode_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            Group g = ActiveGroup;
            if (g != null && g.fullSet.Contains(game.targetNode))
                g.DeleteEntity(game.targetNode);
            if (game.targetNode != null)
            {
                //game.targetNode.active = false;
                game.targetNode.IsDeleted = true;
                game.targetNode = null;
            }
            if (ui.editNode != ActiveDefaultNode && !lstPresets.Items.Contains(ui.editNode))
            {
                lstComp.Items.Clear();
                lstComp.rootitem = null;
                ui.editNode = null;
            }
            propertyEditPanel.DisableControls();
        }
        void addComponent(object ans, Node n)
        {
                if (ans == null)
                {
                    PopUp.Toast(ui, "You didn't select a component.");
                    return; //I added this, because if not, the above toast does not show. -zck
                }
                bool writeable = true;
                
                
                
        }
    }
}
