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
    public class AddComponentView : DetailedView
    {
        public Label lblDescription, lblCompName;
        public AddComponentView(Sidebar sidebar, Control parent, int Left, int Top)
            : base(sidebar, parent, Left, Top)
        {
            ItemCreator = Creator;
            ColorChangeOnSelect = false;
            Width = parent.Width - 60;
            sidebar.ui.detailedViews.Remove(this);
            lblDescription = new Label(manager);
            lblDescription.Init();
            lblDescription.Parent = parent;
            lblDescription.Width = 400;
            lblDescription.Top = backPanel.Height + Top + 10;
            lblDescription.Left = 10;
            lblDescription.Height += lblDescription.Height * 2;
            lblDescription.Text = "";

            lblCompName = new Label(manager);
            lblCompName.Init();
            lblCompName.Parent = parent;
            lblCompName.Width = 400;
            lblCompName.Top = backPanel.Height + Top;
            lblCompName.Left = 10;
            lblCompName.Text = "";
            lblCompName.TextColor = UserInterface.TomShanePuke;
        }
        public void InitNode(Node node)
        {
            int heightcounter = 0;
            foreach(comp c in Utils.compTypes.Keys)
            {
                Type ctype = Utils.compTypes[c];
                Info info = Utils.GetInfoType(ctype);
                if (info == null || (int)sidebar.userLevel < (int)info.userLevel) continue;
                if (node.HasComponent(c)) continue;
                if ((Utils.GetCompTypes(ctype) & mtypes.exclusiveLinker) == mtypes.exclusiveLinker) continue;
                DetailedItem ditem = new DetailedItem(manager, this, ctype, backPanel, heightcounter, 0, backPanel.Width - 20);
                ditem.label.Text = ditem.label.Text;
                ditem.label.Left += 50;
                CreateItem(ditem);
                heightcounter += ditem.panel.Height;
            }
        }
        public void Creator(DetailedItem item, object obj)
        {
            if (item == null || obj == null) return;
            item.panel.DoubleClick += (s, e) =>
            {
                item.panel.SendMessage(Message.Click, new MouseEventArgs());
            };
            item.panel.Click += (s, e) =>
            {
                CheckBox cb = (CheckBox)item.itemControls["checkbox"];
                cb.Checked = !cb.Checked;
            };
            CheckBox checkbox = new CheckBox(manager);
            checkbox.Init();
            checkbox.Parent = item.panel;
            checkbox.Left = 6;
            checkbox.Top = 3;
            checkbox.Checked = false;
            checkbox.Text = "";
            checkbox.Name = "checkbox";
            item.AddControl(checkbox);

            mtypes types = Utils.GetCompTypes((Type)item.obj);
            if (types == mtypes.none) return;
            bool AO = (types & mtypes.affectother) == mtypes.affectother;
            bool AS = (types & mtypes.affectself) == mtypes.affectself;
            bool D = ((types & mtypes.draw) == mtypes.draw) || (types & mtypes.minordraw) == mtypes.minordraw;
            bool Q = (types & mtypes.tracer) == mtypes.tracer;
            bool TREE = (Type)item.obj == typeof(Tree);
            int weight = 0;
            if (AO) weight += 10;
            if (AS) weight += 1;
            if (D) weight += 1;
            if (Q) weight += 3;
            if (TREE) weight = 50;
            int hc = 180;
            NewLabel(weight.ToString(), hc, item, "label1");
            hc += 100;
            if (AO) NewLabel(UserInterface.Checkmark, hc, item, "label2"); else NewLabel(UserInterface.Cross, hc, item, "label2");
            hc += 100;
            if (AS) NewLabel(UserInterface.Checkmark, hc, item, "label3"); else NewLabel(UserInterface.Cross, hc, item, "label3"); //#magic - blaze it.
            hc += 95;
            if (D)  NewLabel(UserInterface.Checkmark, hc, item, "label4"); else NewLabel(UserInterface.Cross, hc, item, "label4");

            Info info = Utils.GetInfoType((Type)item.obj);
            if (info == null) Console.WriteLine("Info was null on component type " + item.obj);
            else
            {
                string summary = info.summary.wordWrap(50);
                item.panel.MouseOver += (s, e) =>
                {
                    lblCompName.Text = item.obj.ToString().LastWord('.');
                    lblDescription.Text = summary;
                };
            }
        }

        public void NewLabel(string s, int left, DetailedItem item, string name)
        {
            Label label = new Label(manager);
            label.Init();
            label.Parent = item.panel;
            label.Top = 1;
            label.Left = left;
            label.TextColor = Color.Black;
            label.Width = 30;
            label.Text = s;
            label.Name = name;
            if (s.Equals(UserInterface.Checkmark)) label.TextColor = UserInterface.TomShanePuke;
            else if (s.Equals(UserInterface.Cross)) label.TextColor = Color.Red;
            item.AddControl(label);
        }
    }
}
