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
    public class ComponentView : ListView<ComponentViewItem>
    {
        public Label lblComponents, lblGroup;
        public Button btnAddComponent;

        public InspectorView insView;

        //public new List<ComponentViewItem> viewItems;
        //public new ComponentViewItem selectedItem { get; set; }

        //public ComboBox cbActiveGroup;

        //public InspectorArea insArea;

        public ComponentView(Sidebar sidebar, Control parent, int Left, int Top)
            : base(sidebar, parent, Left, Top, false)
        {
            
            lblGroup = new Label(manager);
            lblGroup.Init();
            lblGroup.Parent = parent;
            lblGroup.Text = "Group:";
            lblGroup.Width = 60;
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
            SwitchGroup(room.masterGroup.childGroups["General Groups"].childGroups.ElementAt(0).Value);

            //UpdateGroupComboBox();
            //cbActiveGroup.ItemIndex = 0;
        }
        //public void UpdateGroupComboBox()
        //{
        //    ComboBox cb = cbActiveGroup;
        //    string tempName = "";
        //    if (cb.ItemIndex >= 0) tempName = cb.Items.ElementAt(cb.ItemIndex).ToString();
        //    cb.ItemIndex = 0;
        //    List<object> list = cb.Items;
        //    list.ToList().ForEach((o) => list.Remove(o));
        //    room.masterGroup.childGroups["General Groups"].GroupNamesToList(list, false);
        //
        //    if (!tempName.Equals("")) cb.ItemIndex = cb.Items.IndexOf(tempName);
        //}

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
                //SetVisible(false);
            }
        }
        //optimized applytoall for only node, body, and components
        public void ApplyToGroup(FPInfo fpinfo, object value)
        {
            if (activeGroup == null || selectedItem == null) return;
            if (fpinfo == null) Console.WriteLine("FPInfo was null when applying to group in ComponentView");

            if (selectedItem.obj is Component)
            {
                Component c = (Component)selectedItem.obj;
                foreach (Node n in activeGroup.fullSet)
                {
                    comp cc = c.com;
                    if (!n.HasComponent(cc)) n.addComponent(cc, c.active);
                    fpinfo.SetValue(value, n[cc]);
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
            if (insView == null) return;
            if (obj == null) return;
            if (obj.GetType().IsClass)
            {
                insView.backPanel.Visible = true;
                insView.backPanel.Refresh();
                insView.SetRootItem(obj);
            }
            //if (insArea == null) return;
            //insArea.InsBox.Visible = true;
            //insArea.InsBox.Refresh();
            //insArea.ResetInspectorBox(obj);
            //insArea.propertyEditPanel.DisableControls();
        }

        public void RefreshComponents()
        {
            if (activeGroup != null)
            {
                SwitchGroup(activeGroup);
            }
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
                foreach (ComponentViewItem item in viewItems)
                {
                    backPanel.Remove(item.textPanel);
                }
            }

            viewItems = new List<ComponentViewItem>();
            int itemCount = rootNode.comps.Count + 2;
            int width = backPanel.Width - 4; //#magic number
            if (itemCount >= 10)
                width -= 18;

            CreateItem(new ComponentViewItem(manager, this, rootNode, backPanel, heightCount, LeftPadding, width));
            //compItems[0].textPanel.Color = Color.Blue;
            int height = (viewItems[0].itemHeight - 2);
            heightCount += height;
            CreateItem(new ComponentViewItem(manager, this, rootNode.body, backPanel, heightCount, LeftPadding, width));

            foreach (object obj in rootNode.comps.Values)
            {
                heightCount += height;
                CreateItem(new ComponentViewItem(manager, this, obj, backPanel, heightCount, LeftPadding, width));
            }
            //heightCount += compItems[0].label.Height;
            //compItems.Add(new ComponentItem(manager, this, null, compsBackPanel, heightCount, LeftPadding));
            ScrollPosition = 0;
            backPanel.ScrollTo(backPanel.ScrollBarValue.Horizontal, 0);
            SetVisible(false);
            backPanel.Refresh();

            if (selected != 3) sidebar.tbcMain.SelectedIndex = selected;
        }

        public void CreateItem(ComponentViewItem item)
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

        //public override void SelectItem(ComponentViewItem item)
        //{
        //    base.SelectItem(item);
        //}

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
