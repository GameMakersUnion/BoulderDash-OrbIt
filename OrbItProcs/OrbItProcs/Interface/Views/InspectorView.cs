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
    public class InspectorView : DetailedView
    {
        public InspectorItem rootItem;
        private Group _activeGroup;
        public Group activeGroup
        {
            get { return _activeGroup; }
            set
            {
                _GroupSync = value != null;
                _activeGroup = value;
            }
        }
        private bool _GroupSync = false;
        public bool GroupSync { get { return _GroupSync && activeGroup != null; } set { _GroupSync = value; } }

        public InspectorView(Sidebar sidebar, Control parent, int Left, int Top, bool Init = true)
            : base(sidebar, parent, Left, Top, Init)
        {
            //backPanel.Height = 120;
            Setup(ItemCreatorDelegate, OnEvent);
        }

        public void SetRootItem(object item)
        {
            if (item == null) return;
            ClearView();
            if (item is InspectorItem)
            {
                InspectorItem insItem = (InspectorItem)item;
                insItem.GenerateChildren();
                foreach (InspectorItem i in insItem.children)
                {
                    CreateNewItem(i);
                }
            }
            else
            {
                //InspectorItem insItem = new InspectorItem(null, item, sidebar);
                //if (item is Body)
                //{
                //    //todo: fix item path here
                //}
                //insItem.GenerateChildren();
                //foreach (InspectorItem i in insItem.children)
                //{
                //    CreateNewItem(i);
                //}
            }
        }
        public void CreateNewItem(InspectorItem item)
        {
            int top = 0;
            if (viewItems.Count > 0)
            {
                top = (viewItems[0].itemHeight - 4) * viewItems.Count;
            }
            DetailedItem detailedItem = new DetailedItem(manager, this, item, backPanel, top, LeftPadding, backPanel.Width - 4);
            if (item.ToolTip.Length > 0) detailedItem.textPanel.ToolTip.Text = item.ToolTip;
            viewItems.Add(detailedItem);
            SetupScroll(detailedItem);
        }

        public void OnEvent(Control control, DetailedItem item, EventArgs e)
        {
            if (item == null || control == null || item.obj == null) return;
            if (!(item.obj is InspectorItem)) return;
            InspectorItem ins = (InspectorItem)item.obj;
            if (e is KeyEventArgs && control.GetType() == typeof(TextBox))
            {
                KeyEventArgs ke = (KeyEventArgs)e;
                if (ke.Key != Microsoft.Xna.Framework.Input.Keys.Enter) return;
                TextBox textbox = (TextBox)control;
                object san = ins.TrySanitize(textbox.Text);
                if (san != null)
                {
                    ins.SetValue(san);
                    if (GroupSync)
                    {
                        ins.ApplyToAllNodes(activeGroup);
                    }
                }
            }
            //else if (control is CheckBox)
            //{
            //    CheckBox checkbox = (CheckBox)control;
            //    if (ins.obj is bool)
            //    {
            //        ins.SetValue(checkbox.Checked);
            //        if (GroupSync)
            //        {
            //            ins.ApplyToAllNodes(activeGroup);
            //        }
            //    }
            //    else if (checkbox.Name.Equals("toggle_checkbox"))
            //    {
            //        ins.SetValue(checkbox.Checked);
            //        if (GroupSync)
            //        {
            //            ins.ApplyToAllNodes(activeGroup);
            //        }
            //    }
            //}
            else if (control is ComboBox)
            {
                ins.SetValue(control.Text);
                if (GroupSync)
                {
                    ins.ApplyToAllNodes(activeGroup);
                }
            }
            else if (control is Button)
            {
                if (control.Name.Equals("bool_button_enabled"))
                {
                    ins.SetValue(GetButtonBool((Button)control));
                    if (GroupSync)
                    {
                        ins.ApplyToAllNodes(activeGroup);
                    }
                }
                else if (control.Name.Equals("toggle_button_enabled"))
                {
                    ins.SetValue(GetButtonBool((Button)control));
                    if (GroupSync)
                    {
                        ins.ApplyToAllNodes(activeGroup);
                    }
                }
                else if (control.Name.Equals("component_button_enabled"))
                {
                    Component component = (Component)ins.obj;
                    component.active = GetButtonBool((Button)control);
                    if (this.GetType() == typeof(ComponentView))
                    {
                        (this as ComponentView).lblCurrentComp.TextColor = control.TextColor;
                    }
                    //ins.SetValue(checkbox.Checked);
                    if (GroupSync)
                    {
                        foreach (Node n in activeGroup.fullSet)
                        {
                            if (n.HasComponent(component.com))
                                n.comps[component.com].active = component.active;
                        }
                    }
                }
                else if (control.Name.Equals("component_button_remove"))
                {
                    if (ins.obj is Component)
                    {
                        Component component = (Component)ins.obj;
                        component.parent.RemoveComponent(component.com);
                        foreach (Node n in activeGroup.fullSet)
                        {
                            n.RemoveComponent(component.com);
                        }
                    }
                }
            }

            
        }

        private void ItemCreatorDelegate(DetailedItem item, object obj)
        {
            if (obj is InspectorItem)
            {
                InspectorItem inspectorItem = (InspectorItem)obj;
                object o = inspectorItem.obj;
                bool isToggle = o.GetType().IsGenericType && o.GetType().GetGenericTypeDefinition() == typeof(Toggle<>);
                if (o != null)
                {
                    if (o is Node)
                    {
                        item.label.Text = "Root";
                    }
                    else if (o is Body || o is Component)
                    {
                        item.label.Text = o.GetType().ToString().LastWord('.');
                    }
                    if (o is Component)
                    {
                        Component comp = (Component)o;
                        //CheckBox checkbox = new CheckBox(manager);
                        //checkbox.Init();
                        //checkbox.Parent = item.textPanel;
                        //checkbox.Left = backPanel.Width - 45;
                        //checkbox.Top = 2;
                        //checkbox.Text = "";
                        //checkbox.ToolTip.Text = "Toggle";
                        //checkbox.Checked = comp.active;
                        //checkbox.Name = "component_checkbox_active";
                        //item.AddControl(checkbox);

                        Button btnEnabled = new Button(manager);
                        btnEnabled.Init();
                        btnEnabled.Parent = item.textPanel;
                        btnEnabled.TextColor = Color.Red;
                        btnEnabled.Width = 40;
                        btnEnabled.Left = item.textPanel.Width - btnEnabled.Width - 20;
                        btnEnabled.Top = 3;
                        btnEnabled.Height = item.buttonHeight;
                        btnEnabled.ToolTip.Text = "Toggle Active";
                        btnEnabled.Name = "component_button_enabled";
                        SetButtonBool(btnEnabled, comp.active);
                        item.AddControl(btnEnabled);

                        //check for essential
                        if (!comp.isEssential())
                        {
                            Button btnRemove = new Button(manager);
                            btnRemove.Init();
                            btnRemove.Parent = item.textPanel;
                            btnRemove.TextColor = Color.Red;
                            btnRemove.Left = btnEnabled.Left - 20;
                            btnRemove.Top = 3;
                            btnRemove.Height = item.buttonHeight;
                            btnRemove.Width = item.buttonWidth;
                            btnRemove.Text = "-";
                            btnRemove.ToolTip.Text = "Remove";
                            btnRemove.Name = "component_button_remove";
                            item.AddControl(btnRemove);
                        }
                    }
                    else if (o is int || o is Single || o is byte || isToggle)
                    {
                        int w = 60;
                        TextBox textbox = new TextBox(manager);
                        textbox.Init();
                        textbox.Parent = item.textPanel;
                        textbox.TextColor = UserInterface.TomShanePuke;
                        textbox.Left = backPanel.Width - w - 26;
                        textbox.Width = w;
                        textbox.Height = textbox.Height - 4;
                        textbox.Text = o.ToString();
                        textbox.Name = "number_textbox";
                        if (isToggle)
                        {
                            textbox.Name = "toggle_textbox";
                            //CheckBox checkbox = new CheckBox(manager);
                            //checkbox.Init();
                            //checkbox.Width = 20;
                            //checkbox.Parent = item.textPanel;
                            //checkbox.Left = textbox.Left - 30;
                            //checkbox.Top = 2;
                            //checkbox.Text = "";
                            //checkbox.ToolTip.Text = "Toggle";
                            ////checkbox.Checked = (bool)o;
                            //checkbox.Name = "toggle_checkbox";
                            //dynamic toggle = o;
                            //checkbox.Checked = toggle.enabled;
                            //textbox.Text = toggle.value.ToString();

                            Button btnEnabled = new Button(manager);
                            btnEnabled.Init();
                            btnEnabled.Parent = item.textPanel;
                            btnEnabled.TextColor = Color.Red;
                            btnEnabled.Width = 40;
                            btnEnabled.Left = textbox.Left - btnEnabled.Width;
                            //btnEnabled.Top = 3;
                            btnEnabled.Height = item.buttonHeight;
                            btnEnabled.ToolTip.Text = "Toggle Active";
                            btnEnabled.Name = "toggle_button_enabled";
                            SetButtonBool(btnEnabled, (o as dynamic).enabled);
                            item.AddControl(btnEnabled);

                            //item.AddControl(checkbox);
                            
                        }
                        item.AddControl(textbox);

                        //todo: make tiny + and - buttons
                    }
                    else if (o is string)
                    {
                        int w = 60;
                        TextBox textbox = new TextBox(manager);
                        textbox.ClientMargins = new Margins();

                        textbox.Init();


                        textbox.Parent = item.textPanel;
                        textbox.TextColor = UserInterface.TomShanePuke;
                        textbox.Left = backPanel.Width - w - 26;
                        textbox.Width = w;
                        textbox.Height = textbox.Height - 4;
                        textbox.Text = o.ToString();
                        textbox.Name = "string_textbox";
                        item.AddControl(textbox);

                        textbox.ClientArea.Top += 2;
                        textbox.ClientArea.Left += 2;
                        textbox.KeyPress += delegate
                        {
                            if (!textbox.Text.Equals(""))
                            {
                                textbox.ClientArea.Top += 2;
                                textbox.ClientArea.Left += 2;
                            }
                        };

                    }
                    else if (o is bool)
                    {
                        //CheckBox checkbox = new CheckBox(manager);
                        //checkbox.Init();
                        //checkbox.Parent = item.textPanel;
                        //checkbox.Left = backPanel.Width - 45;
                        //checkbox.Top = 2;
                        //checkbox.Text = "";
                        //checkbox.ToolTip.Text = "Toggle";
                        //checkbox.Checked = (bool)o;
                        //checkbox.Name = "bool_checkbox";
                        //item.AddControl(checkbox);
                        Button btnEnabled = new Button(manager);
                        btnEnabled.Init();
                        btnEnabled.Parent = item.textPanel;
                        btnEnabled.TextColor = Color.Red;
                        btnEnabled.Width = 40;
                        btnEnabled.Left = item.textPanel.Width - btnEnabled.Width - 20;
                        //btnEnabled.Top = 3;
                        btnEnabled.Height = item.buttonHeight;
                        btnEnabled.ToolTip.Text = "Toggle Enabled";
                        btnEnabled.Name = "bool_button_enabled";
                        SetButtonBool(btnEnabled, (bool)o);
                        item.AddControl(btnEnabled);
                    }
                    else if (o.GetType().IsEnum)
                    {
                        int w = 95;
                        ComboBox combobox = new ComboBox(manager);
                        combobox.ClientMargins = new Margins();
                        combobox.Init();
                        combobox.TextColor = UserInterface.TomShanePuke;
                        combobox.Parent = item.textPanel;
                        combobox.Left = backPanel.Width - w - 26;
                        combobox.Height = combobox.Height - 4;
                        combobox.Width = w;
                        int i = 0;
                        foreach(string s in Enum.GetNames(o.GetType()))
                        {
                            combobox.Items.Add(s);
                            if (s.Equals(o.ToString())) combobox.ItemIndex = i;
                            i++;
                        }
                        combobox.Name = "enum_combobox";
                        item.AddControl(combobox);

                        combobox.ClientArea.Top += 2;
                        combobox.ClientArea.Left += 2;
                        combobox.ItemIndexChanged += delegate
                        {
                            if (!combobox.Text.Equals(""))
                            {
                                combobox.ClientArea.Top += 2;
                                combobox.ClientArea.Left += 2;
                            }
                        };
                    }
                    
                }
            }
        }

        public override void Refresh()
        {
            if (viewItems != null)
            {
                foreach (DetailedItem item in viewItems)
                {
                    if (item.obj == null) continue;
                    if (item.obj is InspectorItem)
                    {
                        InspectorItem insItem = (InspectorItem)item.obj;
                        if (insItem.obj != null && (insItem.obj is Component || insItem.obj is Node || insItem.obj is Body))
                        {
                            continue;
                        }
                        item.label.Text = insItem.ToString().LastWord('.');
                        if (item.itemControls == null) continue;
                        foreach(string name in item.itemControls.Keys)
                        {
                            Control control = item.itemControls[name];
                            //todo:implement refresh controls
                        }
                    }
                    
                }
            }
        }
    }
}
