using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomShane.Neoforce.Controls;
using Component = OrbItProcs.Component;
using Console = System.Console;
using EventHandler = TomShane.Neoforce.Controls.EventHandler;
using EventArgs = TomShane.Neoforce.Controls.EventArgs;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;

namespace OrbItProcs
{
    // Fourth sidebar tab page
    public partial class Sidebar
    {
        public ListView componentView { get; set; }

        public TabControl tbcViews;

        private TabControl _activeTabControl;
        public TabControl activeTabControl
        {
            get { return _activeTabControl; }
            set
            {
                if (value != null)
                {
                    if (_activeTabControl != null && value != _activeTabControl)
                    {
                        _activeTabControl.Visible = false;
                    }
                }
                _activeTabControl = value;
                _activeTabControl.Visible = true;
                _activeTabControl.Refresh();
            }
        }

        public void InitializeFourthPage()
        {
            tbcMain.Visible = false;

            tbcViews = new TabControl(manager);
            tbcViews.Init();
            tbcViews.Parent = master;
            tbcViews.Left = 0;
            tbcViews.Top = 0;
            tbcViews.Width = master.Width - 5;
            tbcViews.Height = Game1.sWidth - 40;
            tbcViews.Anchor = Anchors.All;

            tbcViews.AddPage();
            TabPage editTab = tbcViews.TabPages[0];
            editTab.Text = "Edit";

            tbcViews.SelectedIndex = 0;

            activeTabControl = tbcViews;

            componentView = new ListView(this, editTab, 0, 0, true);
        }
    }


    public class ListView
    {

        public Game1 game;
        public Room room;
        public UserInterface ui;
        public Sidebar sidebar;
        public InspectorItem ActiveInspectorParent;
        public Node rootNode;
        public Group activeGroup;

        public int Left;
        public int Top;
        public int Width;
        //public int Height { get { return (propertyEditPanel.grouppanel.Top + propertyEditPanel.grouppanel.Height); } }
        public int HeightCounter;
        public int LeftPadding = 5;
        public int VertPadding = 7;
        public int ScrollPosition = 0;

        public Manager manager;
        public Control parent;


        
        public Panel compsBackPanel;
        public Label lblComponents, lblGroup;
        public ComboBox cbActiveGroup;
        public Button btnAddComponent;
        //public Label lblDummy;

        public Color backColor, textColor, selectedBackColor, selectedTextColor;

        private bool isVisible = false;

        public void SetVisible(bool visible)
        {
            //if (isVisible == visible) return;
            isVisible = visible;
            

            if (visible && selectedItem != null)
            {
                SetComponent(null);
                SetComponent(selectedItem.obj);
            }
            else
            {
                SetComponent(null);
            }

            insArea.InsBox.Visible = visible;
            insArea.propertyEditPanel.grouppanel.Visible = visible;
        }

        public InspectorArea insArea;

        public ListView(Sidebar sidebar, Control parent, int Left, int Top, bool InspectorArea)
        {
            this.game = sidebar.game;
            this.room = sidebar.room;
            this.ui = sidebar.ui;
            this.sidebar = sidebar;
            this.manager = sidebar.manager;
            this.parent = parent;

            this.Left = Left;
            this.Top = Top;
            
            Initialize(InspectorArea);
            
        }

        public List<ViewItem> viewItems;
        public ViewItem selectedItem { get; set; }
        

        public void Initialize(bool InspectorArea)
        {
            HeightCounter = Top;
            Width = parent.Width - LeftPadding * 2;

            lblGroup = new Label(manager);
            lblGroup.Init();
            lblGroup.Parent = parent;
            lblGroup.Text = "Group:";
            lblGroup.Width = 60;
            lblGroup.Left = LeftPadding;
            lblGroup.TextColor = Color.Black;
            lblGroup.Top = HeightCounter;

            cbActiveGroup = new ComboBox(manager);
            cbActiveGroup.Init();
            cbActiveGroup.Parent = parent;
            cbActiveGroup.Width = Width - lblGroup.Width;
            cbActiveGroup.Left = lblGroup.Left + lblGroup.Width;
            //cbActiveGroup.TextColor = Color.Black;
            cbActiveGroup.Top = HeightCounter;
            cbActiveGroup.ItemIndexChanged += cbActiveGroup_ItemIndexChanged;
            HeightCounter += cbActiveGroup.Height + VertPadding;

            

            lblComponents = new Label(manager);
            lblComponents.Init();
            lblComponents.Parent = parent;
            lblComponents.Text = "Components";
            lblComponents.Width = 150;
            lblComponents.Left = LeftPadding;
            lblComponents.TextColor = Color.Black;
            lblComponents.Top = HeightCounter;
            HeightCounter += lblComponents.Height + VertPadding;

            backColor = new Color(92, 92, 92);
            textColor = Color.Black;//new Color(32, 32, 32);
            selectedBackColor = textColor;
            selectedTextColor = backColor;

            #region /// Components List (back panel) ///
            compsBackPanel = new Panel(manager);
            compsBackPanel.Init();
            compsBackPanel.Left = Left + LeftPadding;
            compsBackPanel.Top = HeightCounter;
            compsBackPanel.Width = Width;
            compsBackPanel.Parent = parent;
            compsBackPanel.Height = 151;
            compsBackPanel.Text = "";
            compsBackPanel.AutoScroll = true;
            compsBackPanel.Color = backColor;
            //compsBackPanel.BevelColor = Color.Black;
            //compsBackPanel.BevelMargin = 0;
            compsBackPanel.BevelBorder = BevelBorder.All;
            compsBackPanel.BevelStyle = BevelStyle.Etched;
            compsBackPanel.ClientArea.Height = 200;
            parent.Add(compsBackPanel);
            HeightCounter += compsBackPanel.Height + VertPadding;
            #endregion

            //lblDummy = new Label(manager);
            //lblDummy.Init();
            //lblDummy.Parent = compsBackPanel;
            //lblDummy.Text = "";
            //lblDummy.Top = compsBackPanel.Height + 10;
            //lblDummy.Height = 1;
            //lblDummy.Width = 1;

            btnAddComponent = new Button(manager);
            btnAddComponent.Init();
            btnAddComponent.Width = 150;
            btnAddComponent.Left = Left + LeftPadding + (Width - btnAddComponent.Width) / 2;
            btnAddComponent.Top = HeightCounter;
            
            btnAddComponent.Parent = parent;
            btnAddComponent.Text = "Add Component";
            btnAddComponent.Click += btnAddComponent_Click;
            //btnAddComponent.ToolTip.Text = "This is a test\nto see if multi\nline is doable";
            HeightCounter += btnAddComponent.Height;

            if (InspectorArea)
            {
                insArea = new InspectorArea(sidebar, parent, Left + LeftPadding, HeightCounter);
                insArea.compView = this;
                insArea.SetOverrideString("Menu");
            }

            UpdateGroupComboBox();

            cbActiveGroup.ItemIndex = 0;
        }

        public void UpdateGroupComboBox()
        {
            ComboBox cb = cbActiveGroup;
            string tempName = "";
            if (cb.ItemIndex >= 0) tempName = cb.Items.ElementAt(cb.ItemIndex).ToString();
            cb.ItemIndex = 0;
            List<object> list = cb.Items;
            list.ToList().ForEach((o) => list.Remove(o));
            room.masterGroup.childGroups["General Groups"].GroupNamesToList(list, false);

            if (!tempName.Equals("")) cb.ItemIndex = cb.Items.IndexOf(tempName);
        }

        void cbActiveGroup_ItemIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            if (cmb.ItemIndex < 0) return;
            string item = cmb.Items.ElementAt(cmb.ItemIndex).ToString();
            if (!item.Equals(""))
            {
                Group find = room.masterGroup.FindGroup(item);
                if (find == null) return;

                SwitchGroup(find);
                ScrollPosition = 0;
                compsBackPanel.ScrollTo(compsBackPanel.ScrollBarValue.Horizontal, 0);
                SetVisible(false);
            }
        }
        //optimized applytoall for only node, body, and components
        public void ApplyToGroup(FPInfo fpinfo, object value)
        {
            if (activeGroup == null) return;
            if (fpinfo == null) Console.WriteLine("FPInfo was null when applying to group in ComponentView");

            if (selectedItem.obj is Component)
            {
                Component c = (Component)selectedItem.obj;
                foreach (Node n in activeGroup.fullSet)
                {
                    if (!n.HasComponent(c.com)) n.addComponent(c.com, c.active);
                    fpinfo.SetValue(value, n[c.com]);
                }
            }
            else if (selectedItem.obj is Body)
            {
                foreach (Node n in activeGroup.fullSet)
                {
                    fpinfo.SetValue(value, n.body);
                }
            }
            else if (selectedItem.obj is Node)
            {
                foreach (Node n in activeGroup.fullSet)
                {
                    fpinfo.SetValue(value, n);
                }
            }
        }

        public void SetComponent(object obj)
        {
            //group view
            if (obj is Group)
            {
                return;
            }
            //components view
            if (insArea == null) return;
            insArea.ResetInspectorBox(obj);
            insArea.propertyEditPanel.DisableControls();
        }

        public void SwitchGroup(Group group)
        {
            if (group == null) return;
            activeGroup = group;
            rootNode = group.defaultNode;
            if (rootNode == null) return;


            int selected = sidebar.tbcMain.SelectedIndex;
            if (selected != 3) sidebar.tbcMain.SelectedIndex = 3;

            int heightCount = 0;
            if (viewItems != null)
            {
                foreach (ViewItem item in viewItems)
                {
                    compsBackPanel.Remove(item.textPanel);
                }
            }

            viewItems = new List<ViewItem>();
            int itemCount = rootNode.comps.Count + 2;
            int width = compsBackPanel.Width - 4; //#magic number
            if (itemCount >= 10) 
                width -= 18;

            viewItems.Add(new ViewItem(manager, this, rootNode, compsBackPanel, heightCount, LeftPadding, width));
            //compItems[0].textPanel.Color = Color.Blue;
            heightCount += viewItems[0].label.Height;
            viewItems.Add(new ViewItem(manager, this, rootNode.body, compsBackPanel, heightCount, LeftPadding, width));

            foreach(object obj in rootNode.comps.Values)
            {
                heightCount += viewItems[0].label.Height;
                viewItems.Add(new ViewItem(manager, this, obj, compsBackPanel, heightCount, LeftPadding, width));
            }
            //heightCount += compItems[0].label.Height;
            //compItems.Add(new ComponentItem(manager, this, null, compsBackPanel, heightCount, LeftPadding));
            SetVisible(false);
            compsBackPanel.Refresh();

            if (selected != 3) sidebar.tbcMain.SelectedIndex = selected;
        }

        public void ToggleGroupComponent(comp c, bool value)
        {
            if (activeGroup != null)
            {
                foreach(Node n in activeGroup.fullSet)
                {
                    if (n.HasComponent(c)) n[c].active = value;
                }
            }
        }

        public void ComponentsList_ChangeScrollPosition(int change)
        {
            compsBackPanel.ScrollTo(compsBackPanel.ScrollBarValue.Horizontal, ScrollPosition + change * 8);
            ScrollPosition = compsBackPanel.ScrollBarValue.Vertical;
        }

        public void SetScrollPosition(int value)
        {
            //int previous = compsBackPanel.ScrollBarValue.Vertical;
            compsBackPanel.ScrollTo(compsBackPanel.ScrollBarValue.Horizontal, value);
            ScrollPosition = compsBackPanel.ScrollBarValue.Vertical;
        }

        public int SelectComponent(Type t)
        {
            string name = t.ToString().LastWord('.');
            int count = 0;
            foreach(ViewItem item in viewItems)
            {
                if (item.label.Text.Equals(name))
                {
                    item.isSelected = true;
                    return count;
                }
                count++;
            }
            return -1;
        }

        void btnAddComponent_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            if (activeGroup == null)
                PopUp.Toast("You haven't selected a Group.");
            else
            {
                ObservableCollection<dynamic> nodecomplist = new ObservableCollection<dynamic>((Enum.GetValues(typeof(comp)).Cast<dynamic>().Where(c => !activeGroup.defaultNode.HasComponent(c))));
                List<dynamic> missingcomps = new List<dynamic>(Enum.GetValues(typeof(comp)).Cast<dynamic>().Where(c => activeGroup.defaultNode.HasComponent(c)));

                PopUp.opt[] options = new PopUp.opt[]{
                    new PopUp.opt(PopUp.OptType.info, "Add component to: " + activeGroup.Name),
                    new PopUp.opt(PopUp.OptType.dropDown, nodecomplist),
                    };

                PopUp.makePopup(ui, options, "Add Component", delegate(bool a, object[] o)
                {
                    if (a) return addComponent(o);
                    else return false;
                });
            }
        }

        private bool addComponent(object[] o)
        {
            comp c = (comp)o[1];
            foreach (Node n in activeGroup.fullSet)
            {
                if (!n.HasComponent(c))
                    n.addComponent(c, true);
            }
            Node def = activeGroup.defaultNode;
            if (!def.HasComponent(c))
                def.addComponent(c, true);

            //refresh view
            SwitchGroup(activeGroup);
            int index = SelectComponent(Utils.compTypes[c]);
            
            //scroll down
            if (index > 8)
            {
                SetScrollPosition((index - 8) * 18); //#magic number
            }
            //scroll up
            else
            {
                SetScrollPosition(0);
            }


            return true;
        }
    }

    public class ViewItem
    {
        public ListView listView;
        public object obj;
        public Label label;
        public CheckBox checkbox;
        public Button btnRemove, btnEdit;
        public Panel textPanel;
        //public ContextMenu contextMenu;
        //public MenuItem removeComponentMenuItem;

        private bool _isSelected;
        public bool isSelected
        {
            get { return _isSelected; }
            set
            {
                if (value)
                {
                    if (textPanel != null) textPanel.Color = listView.selectedBackColor;
                    if (label != null) label.TextColor = listView.selectedTextColor;

                    foreach (var item in listView.viewItems)
                    {
                        if (item == this) continue;
                        if (item.isSelected) item.isSelected = false;
                    }
                    if (obj != null)
                    {
                        //compView.selectedItem = this;
                        listView.SetComponent(obj);
                        listView.insArea.SetOverrideString(label.Text);
                    }
                }
                else
                {
                    if (textPanel != null) textPanel.Color = listView.backColor;
                    if (label != null) label.TextColor = listView.textColor;
                    if (listView.insArea != null)
                    {
                        listView.insArea.ClearInspectorBox();
                        listView.insArea.SetOverrideString("");
                    }
                    //compView.selectedItem = null;
                }
                _isSelected = value;
                
            }
        }

        public void textPanel_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButton.Left)
            {
                if (!isSelected) 
                { 
                    listView.SetVisible(true); 
                    listView.selectedItem = this; 
                }
                isSelected = !isSelected;
            }
            if (!isSelected) 
            {
                listView.SetVisible(isSelected); 
                listView.selectedItem = null;
            }
        }

        void removeComponent_Click(object sender, EventArgs e)
        {
            if (!(obj is Component) || listView == null || listView.activeGroup == null) return;
            Component component = (Component)obj;

            listView.activeGroup.defaultNode.RemoveComponent(component.com);
            foreach(Node n in listView.activeGroup.fullSet)
            {
                if (n.HasComponent(component.com))
                {
                    n.RemoveComponent(component.com);
                }
            }
            listView.SwitchGroup(listView.activeGroup);
        }

        public ViewItem(Manager manager, ListView compView, object obj, Control parent, int Top, int Left, int Width)
        {
            this.listView = compView;
            this.obj = obj;

            textPanel = new Panel(manager);
            textPanel.Init();
            textPanel.Parent = parent;
            textPanel.Top = Top;
            textPanel.Height = 19;
            //textPanel.MaximumHeight = 64;
            textPanel.Click += textPanel_Click;
            textPanel.BevelBorder = BevelBorder.All;
            textPanel.BevelStyle = BevelStyle.Raised;

            compView.sidebar.ui.SetScrollableControl(textPanel, compView.ComponentsList_ChangeScrollPosition);

            label = new Label(manager);
            label.Init();
            label.Parent = textPanel;
            label.Left = Left;

            textPanel.Width = Width;
            label.Width = 100;

            if (obj != null)
            {
                if (obj is Node)
                {
                    label.Text = "Root";
                }
                else if (obj is Body || obj is Component)
                {
                    label.Text = obj.GetType().ToString().LastWord('.');
                }
                else if (obj is Group)
                {
                    label.Text = (obj as Group).Name;
                }

                //no checkboxes for root or body
                if (obj is Component)
                {
                    Component comp = (Component)obj;

                    checkbox = new CheckBox(manager);
                    checkbox.Init();
                    checkbox.Parent = textPanel;
                    checkbox.Left = parent.Width - 45;
                    checkbox.Text = "";
                    checkbox.ToolTip.Text = "Toggle";
                    checkbox.Checked = comp.active;
                    checkbox.CheckedChanged += (s, e) =>
                    {
                        comp.active = checkbox.Checked;
                        compView.ToggleGroupComponent(comp.com, comp.active);
                    };

                    if (!(comp is Movement) && !(comp is Collision) && !(comp is BasicDraw))
                    {
                        btnRemove = new Button(manager);
                        btnRemove.Init();
                        btnRemove.Parent = textPanel;
                        btnRemove.Left = checkbox.Left - 20;
                        btnRemove.Height = checkbox.Height - 3;
                        btnRemove.Width = 15;
                        btnRemove.Text = "-";
                        btnRemove.Click += removeComponent_Click;
                        btnRemove.ToolTip.Text = "Remove";
                    }
                }
                else if (obj is Group)
                {
                    Group g = (Group)obj;
                    btnEdit = new Button(manager);
                    btnEdit.Init();
                    btnEdit.Parent = textPanel;
                    btnEdit.Left = parent.Width - 5;
                    btnEdit.Width = 15;
                    btnEdit.Text = "e";
                    btnEdit.ToolTip.Text = "Edit";

                    btnRemove = new Button(manager);
                    btnRemove.Init();
                    btnRemove.Parent = parent;
                    btnRemove.Left = btnEdit.Left - 20;
                    btnRemove.Width = 15;
                    btnRemove.Text = "-";
                    btnRemove.ToolTip.Text = "Remove";
                    //todo: add handlers

                }

            }
            else
            {
                label.Text = "";
            }
            isSelected = false;
        }
    }

    
}
