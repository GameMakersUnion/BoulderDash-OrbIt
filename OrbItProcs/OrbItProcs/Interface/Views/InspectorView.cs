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
        public Group activeGroup;
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
            if (e is KeyEventArgs && control is TextBox)
            {
                KeyEventArgs ke = (KeyEventArgs)e;
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
            else if (control is CheckBox)
            {
                CheckBox checkbox = (CheckBox)control;
                if (ins.obj is bool)
                {
                    ins.SetValue(checkbox.Checked);
                    if (GroupSync)
                    {
                        ins.ApplyToAllNodes(activeGroup);
                    }
                }
                else if (ins.obj is Component)
                {
                    Component component = (Component)ins.obj;
                    component.active = checkbox.Checked;
                    //ins.SetValue(checkbox.Checked);
                    if (GroupSync)
                    {
                        foreach(Node n in activeGroup.fullSet)
                        {
                            if (n.HasComponent(component.com))
                                n.comps[component.com].active = checkbox.Checked;
                        }
                    }
                }
            }
            else if (control is Button)
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

        private void ItemCreatorDelegate(DetailedItem item, object obj)
        {
            if (obj is InspectorItem)
            {
                InspectorItem inspectorItem = (InspectorItem)obj;
                object o = inspectorItem.obj;
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
                        CheckBox checkbox = new CheckBox(manager);
                        checkbox.Init();
                        checkbox.Parent = item.textPanel;
                        checkbox.Left = backPanel.Width - 45;
                        checkbox.Top = 2;
                        checkbox.Text = "";
                        checkbox.ToolTip.Text = "Toggle";
                        checkbox.Checked = comp.active;
                        checkbox.Name = "component_checkbox_active";
                        item.AddControl(checkbox);

                        //check for essential
                        if (!comp.isEssential())
                        {
                            Button btnRemove = new Button(manager);
                            btnRemove.Init();
                            btnRemove.Parent = item.textPanel;
                            btnRemove.TextColor = Color.Red;
                            btnRemove.Left = checkbox.Left - 20;
                            btnRemove.Top = 3;
                            btnRemove.Height = item.buttonHeight;
                            btnRemove.Width = item.buttonWidth;
                            btnRemove.Text = "-";
                            //btnRemove.Click += removeComponent_Click;
                            btnRemove.ToolTip.Text = "Remove";
                            btnRemove.Name = "component_button_remove";
                            item.AddControl(btnRemove);
                        }
                    }
                    else if (o is int || o is Single || o is byte)
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
                        CheckBox checkbox = new CheckBox(manager);
                        checkbox.Init();
                        checkbox.Parent = item.textPanel;
                        checkbox.Left = backPanel.Width - 45;
                        checkbox.Top = 2;
                        checkbox.Text = "";
                        checkbox.ToolTip.Text = "Toggle";
                        checkbox.Checked = (bool)o;
                        checkbox.Name = "component_checkbox_active";
                        item.AddControl(checkbox);
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
    }
}
