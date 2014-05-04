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
    public class ComponentView : InspectorView
    {
        public Label lblComponents, lblCurrentComp;
        public Button btnAddComponent;
        public TabControl bottomArea;
        public InspectorView insView;
        //public RoomPanel roomPanel;
        public override int Height { get { return base.Height; } 
            set 
            {  
                int offset = base.Height - value;
                if (bottomArea!= null) bottomArea.Top -= offset;
                //if (lblCurrentComp != null) lblCurrentComp.Top -= offset;
                if (btnAddComponent != null) btnAddComponent.Top -= offset;
                //if (bottomArea != null) bottomArea.Height -= offset;


                base.Height = value;

            } }
        public override int Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                base.Width = value;
                RefreshComponents();
                if (insView == null) return;
                insView.Width = value;
                insView.AdjustWidth();
            }
        }

       
        public ComponentView(Sidebar sidebar, Control parent, int Left, int Top)
            : base(sidebar, parent, Left, Top, false)
        {
            this.parent = parent;
            lblComponents = new Label(manager);
            lblComponents.Init();
            lblComponents.Parent = parent;
            lblComponents.Text = "Components";
            lblComponents.Width = 150;
            lblComponents.Left = LeftPadding;
            lblComponents.TextColor = Color.Black;
            lblComponents.Top = HeightCounter;
            HeightCounter += lblComponents.Height + VertPadding;
            base.Initialize();

            btnAddComponent = new Button(manager);
            btnAddComponent.Init();
            btnAddComponent.Width = 150;
            btnAddComponent.Left = Left + LeftPadding + (Width - btnAddComponent.Width) / 2;
            btnAddComponent.Top = HeightCounter;

            btnAddComponent.Parent = parent;
            btnAddComponent.Text = "Add Component";
            btnAddComponent.Click += btnAddComponent_Click;
            HeightCounter += btnAddComponent.Height + VertPadding;

            


            bottomArea = new TabControl(manager);
            bottomArea.Init();
            bottomArea.Parent = parent;
            bottomArea.Left = 0;
            bottomArea.Top = HeightCounter;
            bottomArea.Width = sidebar.Width - 5;
            //bottomArea.Height = OrbIt.game.tempRoom.worldHeight + 40;
            bottomArea.Height = 500;

            bottomArea.AddPage();
            bottomArea.TabPages[0].Text = "Edit";
            TabPage editTab = bottomArea.TabPages[0];
            editTab.Margins = new Margins(0, 0, 0, 0);

            lblCurrentComp = new Label(manager);
            lblCurrentComp.Init();
            lblCurrentComp.Parent = editTab;
            lblCurrentComp.Width = 150;
            lblCurrentComp.Top = 5;// HeightCounter + VertPadding;
            lblCurrentComp.Left = LeftPadding;
            lblCurrentComp.Text = "";
            lblCurrentComp.TextColor = Color.Black;
            //HeightCounter += lblCurrentComp.Height;

            insView = new InspectorView(sidebar, editTab, Left, lblComponents.Height + 10);
            insView.GroupSync = true;
            insView.Height = 120;
            OnItemEvent += OnEvent2;
            editTab.Margins = new Margins(0, 0, 0, 0);

            bottomArea.AddPage();
            bottomArea.TabPages[1].Text = "Preview";
            TabPage previewTab = bottomArea.TabPages[1];
            previewTab.Margins = new Margins(0, 0, 0, 0);
            //roomPanel = new RoomPanel(sidebar, previewTab, OrbIt.game.tempRoom, true, 10, 15);

            //UpdateGroupComboBox();
            //cbActiveGroup.ItemIndex = 0;
        }

        public void OnEvent2(Control control, DetailedItem item, EventArgs e)
        {
            if (control == null || item == null) return;
            if (!(item.obj is InspectorInfo)) return;
            //InspectorInfo ins = (InspectorInfo)item.obj;
            if (control.Text.Equals("component_button_remove"))
            {
                RefreshComponents();
            }
        }

        public void SetVisible(bool visible)
        {
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
        }


        public override void SelectItem(DetailedItem item)
        {
            base.SelectItem(item);
            if (item.obj == null) return;

            SetComponent(item.obj);
            lblCurrentComp.Text = item.label.Text;
            bottomArea.Refresh();
            if (item.obj is InspectorInfo)
            {
                InspectorInfo i = (InspectorInfo)item.obj;
                if (i.obj is Component)
                {
                    lblCurrentComp.TextColor = item.itemControls["component_button_enabled"].TextColor;
                }
                if (i.obj is Body)
                {
                    lblCurrentComp.TextColor = UserInterface.TomShanePuke;
                }
            }
        }

        public void SetComponent(object obj)
        {
            if (insView == null) return;
            if (obj == null || !(obj is InspectorInfo)) return;
            if (obj.GetType().IsClass)
            {
                insView.backPanel.Visible = true;
                insView.backPanel.Refresh();
                insView.SetRootInspectorItem((InspectorInfo)obj);
            }
        }

        public override void ClearView()
        {
            base.ClearView();
            insView.ClearView();
            lblCurrentComp.Text = "";
        }
        public override void RefreshRoot()
        {
            RefreshComponents();
        }
        public void RefreshComponents()
        {
            if (activeGroup != null)
            {
                SwitchGroup(activeGroup);
            }
        }
        public void SwitchGroup(Group g)
        {
            if (g == null) return;
            activeGroup = g;
            if (insView != null) insView.activeGroup = g;
            //lblGroup.Text = "Group: " + activeGroup.Name;
            SwitchNode(g.defaultNode, true);
        }
        public void SwitchNode(Node node, bool group)
        {
            ClearView();
            if (node == null) return;

            this.rootNode = node;
            if (!group)
            {
                //lblGroup.Text = "Node: " + node.name;
                //activeGroup = null;
                GroupSync = false;
                insView.GroupSync = false;
                this.activeGroup = null;
            }
            int selected = sidebar.tbcMain.SelectedIndex;
            if (selected != 3) sidebar.tbcMain.SelectedIndex = 3;

            int heightCount = 0;
            if (viewItems != null)
            {
                foreach (DetailedItem item in viewItems)
                {
                    backPanel.Remove(item.panel);
                }
            }

            viewItems = new List<DetailedItem>();
            int itemCount = node.comps.Count + 2;
            InspectorInfo rootItem = new InspectorInfo(null, node, sidebar);
            int height = 0;
            if (sidebar.userLevel == UserLevel.Debug)
            {
                CreateItem(new DetailedItem(manager, this, rootItem, backPanel, heightCount, LeftPadding));
                height = (viewItems[0].itemHeight - 2);
                heightCount += height;
            }
            InspectorInfo bodyItem = new InspectorInfo(null, rootItem, node.body, node.GetType().GetProperty("body"));
            CreateItem(new DetailedItem(manager, this, bodyItem, backPanel, heightCount, LeftPadding));
            Info inf = Utils.GetInfoClass(node.body);
            if (inf != null) viewItems[0].toolTip = inf.summary;

            if (height == 0) height = (viewItems[0].itemHeight - 2);

            //heightCount += height;

            InspectorInfo dictItem = new InspectorInfo(null, rootItem, node.comps, node.GetType().GetProperty("comps"));
            foreach (Type c in node.comps.Keys)
            {
                string tooltip = "";
                Info info = Utils.GetInfoClass(node.comps[c]);
                if (info != null)
                {
                    if ((int)info.userLevel > (int)sidebar.userLevel) continue;
                    tooltip = info.summary;
                }
                heightCount += height;
                InspectorInfo cItem = new InspectorInfo(null, dictItem, node.comps[c], c);
                DetailedItem di = new DetailedItem(manager, this, cItem, backPanel, heightCount, LeftPadding);
                di.toolTip = tooltip;
                CreateItem(di);

            }
            //heightCount += compItems[0].label.Height;
            //compItems.Add(new ComponentItem(manager, this, null, compsBackPanel, heightCount, LeftPadding));
            ScrollPosition = 0;
            backPanel.ScrollTo(backPanel.ScrollBarValue.Horizontal, 0);
            SetVisible(false);
            backPanel.Refresh();

            

            if (selected != 3) sidebar.tbcMain.SelectedIndex = selected;

        }

        public int SelectComponent(Type t)
        {
            string name = t.ToString().LastWord('.');
            int count = 0;
            foreach (ViewItem item in viewItems)
            {
                if (item.label.Text.Equals(name))
                {
                    item.OnSelect();
                    item.isSelected = true;
                    return count;
                }
                count++;
            }
            return -1;
        }

        void btnAddComponent_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            if (GroupSync)
            {
                if (activeGroup == null)
                {

                    PopUp.Toast("You haven't selected a Group.");
                    return;
                }
            }
            if (rootNode != null) new AddComponentWindow(sidebar, parent, rootNode, this);
            
        }

    }
}