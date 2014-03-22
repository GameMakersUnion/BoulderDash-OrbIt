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
        public Label lblComponents, lblGroup;
        public Button btnAddComponent;

        public InspectorView insView;

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
                insView.Refresh();
            }
        }

        //public new List<ComponentViewItem> viewItems;
        //public new ComponentViewItem selectedItem { get; set; }

        //public ComboBox cbActiveGroup;

        //public InspectorArea insArea;

        public ComponentView(Sidebar sidebar, Control parent, int Left, int Top)
            : base(sidebar, parent, Left, Top, false)
        {
            GroupSync = true;
            lblGroup = new Label(manager);
            lblGroup.Init();
            lblGroup.Parent = parent;
            lblGroup.Text = "Group:";
            lblGroup.Width = 180;
            lblGroup.Left = LeftPadding;
            lblGroup.TextColor = Color.Black;
            lblGroup.Top = HeightCounter;
            HeightCounter += lblGroup.Height + VertPadding;

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

            insView = new InspectorView(sidebar, parent, Left, HeightCounter);
            insView.GroupSync = true;
            insView.Height = 120;
            OnItemEvent += OnEvent2;

            //UpdateGroupComboBox();
            //cbActiveGroup.ItemIndex = 0;
        }

        public void OnEvent2(Control control, DetailedItem item, EventArgs e)
        {
            if (control == null || item == null) return;
            if (!(item.obj is InspectorItem)) return;
            InspectorItem ins = (InspectorItem)item.obj;
            if (control is Button && ins.obj is Component)
            {
                RefreshComponents();
            }
        }

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

            //insArea.InsBox.Visible = visible;
            //insArea.propertyEditPanel.grouppanel.Visible = visible;
            //
            //insArea.InsBox.Refresh();
            //insArea.propertyEditPanel.grouppanel.Refresh();
        }


        public override void SelectItem(DetailedItem item)
        {
            base.SelectItem(item);
            if (item.obj == null) return;

            SetComponent(item.obj);
        }

        public void SetComponent(object obj)
        {
            if (insView == null) return;
            if (obj == null) return;
            if (obj.GetType().IsClass)
            {
                insView.backPanel.Visible = true;
                insView.backPanel.Refresh();
                insView.SetRootItem(obj);
            }
        }

        public override void ClearView()
        {
            base.ClearView();
            insView.ClearView();
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
            lblGroup.Text = "Group: " + activeGroup.Name;
            SwitchNode(g.defaultNode, true);
        }
        public void SwitchNode(Node node, bool group)
        {
            ClearView();
            if (node == null) return;

            this.rootNode = node;
            if (!group)
            {
                lblGroup.Text = "Node: " + node.name;
                //activeGroup = null;
                GroupSync = false;
                insView.GroupSync = false;
            }
            int selected = sidebar.tbcMain.SelectedIndex;
            if (selected != 3) sidebar.tbcMain.SelectedIndex = 3;

            int heightCount = 0;
            if (viewItems != null)
            {
                foreach (DetailedItem item in viewItems)
                {
                    backPanel.Remove(item.textPanel);
                }
            }

            viewItems = new List<DetailedItem>();
            int itemCount = node.comps.Count + 2;
            int width = backPanel.Width - 4; //#magic number
            if (itemCount >= 10)
                width -= 18;
            InspectorItem rootItem = new InspectorItem(null, node, sidebar);
            CreateItem(new DetailedItem(manager, this, rootItem, backPanel, heightCount, LeftPadding, width));
            //compItems[0].textPanel.Color = Color.Blue;
            int height = (viewItems[0].itemHeight - 2);
            heightCount += height;
            InspectorItem bodyItem = new InspectorItem(null, rootItem, node.body, node.GetType().GetProperty("body"));
            CreateItem(new DetailedItem(manager, this, bodyItem, backPanel, heightCount, LeftPadding, width));
            InspectorItem dictItem = new InspectorItem(null, rootItem, node.comps, node.GetType().GetProperty("comps"));
            foreach (comp c in node.comps.Keys)
            {
                Component co = (Component)node.comps[c];
                var infos = co.GetType().GetCustomAttributes(typeof(Info), false);
                string tooltip = "";
                if (infos != null && infos.Length > 0)
                {
                    Info info = (Info)infos.ElementAt(0);
                    if ((int)info.userLevel > (int)sidebar.userLevel) continue;
                    tooltip = info.summary;
                }
                heightCount += height;
                InspectorItem cItem = new InspectorItem(null, dictItem, node.comps[c], c);
                DetailedItem di = new DetailedItem(manager, this, cItem, backPanel, heightCount, LeftPadding, width);
                di.textPanel.ToolTip.Text = tooltip;
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

        public void CreateItem(DetailedItem item)
        {
            viewItems.Add(item);
            SetupScroll(item);
            item.OnSelect = delegate
            {
                if (this != null)
                {
                    SelectItem(item);
                }
                else
                {
                    throw new SystemException("You've clicked the unclickable. Also, Dante owes Zack a coke.");
                }
            };
        }

        public void ToggleGroupComponent(comp c, bool value)
        {
            if (activeGroup != null)
            {
                foreach (Node n in activeGroup.fullSet)
                {
                    if (n.HasComponent(c)) n[c].active = value;
                }
            }
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
            if (o[1] == null) return false;
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
            RefreshComponents();
            int index = SelectComponent(Utils.compTypes[c]);

            //scroll down
            if (index != -1 && viewItems.Count > 0)
            {
                int itemheight = viewItems.ElementAt(0).itemHeight;
                int visualItemCapacity = backPanel.Height / itemheight;
                if (index > visualItemCapacity)
                {
                    SetScrollPosition((index - visualItemCapacity) * itemheight);
                }
            }

            //scroll up
            else
            {
                SetScrollPosition(0);
            }
            return true;
        }
    }


    public class ComponentViewItem : ViewItem
    {
        public ComponentView componentView;
        public CheckBox checkbox;
        public Button btnRemove;

        public override bool isSelected
        {
            get
            {
                return base.isSelected;
            }
            set
            {
                base.isSelected = value;
                if (value)
                {
                    if (obj != null)
                    {
                        //compView.selectedItem = this;
                        componentView.SetComponent(obj);
                        //componentView.insArea.SetOverrideString(label.Text);
                    }
                }
                //else
                //{
                //    if (componentView.insArea != null)
                //    {
                //        //componentView.insArea.ClearInspectorBox();
                //        //componentView.insArea.SetOverrideString("");
                //    }
                //}
            }
        }

        public ComponentViewItem(Manager manager, ComponentView componentView, object obj, Control parent, int Top, int Left, int Width)
            : base(manager, obj, parent, Top, Left, Width)
        {
            this.componentView = componentView;
            this.textPanel.Width = componentView.backPanel.Width - 4;
            this.textColor = componentView.textColor;
            this.backColor = componentView.backColor;
            RefreshColor();

            if (obj is Node)
            {
                label.Text = "Root";
            }
            else if (obj is Body || obj is Component)
            {
                label.Text = obj.GetType().ToString().LastWord('.');
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
                    this.componentView.ToggleGroupComponent(comp.com, comp.active);
                };

                if (!(comp is Movement) && !(comp is Collision) && !(comp is BasicDraw))
                {
                    btnRemove = new Button(manager);
                    btnRemove.Init();
                    btnRemove.Parent = textPanel;
                    btnRemove.TextColor = Color.Red;
                    btnRemove.Left = checkbox.Left - 20;
                    btnRemove.Height = buttonHeight;
                    btnRemove.Width = buttonWidth;
                    btnRemove.Text = "-";
                    btnRemove.Click += removeComponent_Click;
                    btnRemove.ToolTip.Text = "Remove";
                }
            }

        }

        void removeComponent_Click(object sender, EventArgs e)
        {
            if (!(obj is Component) || componentView == null || componentView.activeGroup == null) return;
            Component component = (Component)obj;

            componentView.activeGroup.defaultNode.RemoveComponent(component.com);
            foreach (Node n in componentView.activeGroup.fullSet)
            {
                if (n.HasComponent(component.com))
                {
                    n.RemoveComponent(component.com);
                }
            }
            componentView.RefreshComponents();
        }

    }
}
