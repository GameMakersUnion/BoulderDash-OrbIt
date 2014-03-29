using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TomShane.Neoforce.Controls;
using EventArgs = TomShane.Neoforce.Controls.EventArgs;
using EventHandler = TomShane.Neoforce.Controls.EventHandler;
using Poop = TomShane.Neoforce.Controls.SideBar;

namespace OrbItProcs
{
    public class AddComponentWindow
    {
        public AddComponentView addCompView;

        public Manager manager;
        public Sidebar sidebar;
        public Poop poop;
        public int HeightCounter = 5;
        public int LeftPadding = 5;
        public Label lblComp, lblProperties;
        Button btnAdd, btnCancel;
        public Node node;
        public DetailedView view;
        public AddComponentWindow(Sidebar sidebar, Node n, DetailedView view)
        {
            UserInterface.GameInputDisabled = true;
            this.view = view;
            this.manager = sidebar.manager;
            this.sidebar = sidebar;
            this.node = n;
            poop = new Poop(manager);
            poop.Init();
            poop.Left = sidebar.master.Left;
            poop.Width = 600;
            poop.Top = 50;
            poop.Height = 380;
            poop.Left = poop.Width - poop.Width / 2;
            poop.Text = "Add Component";
            manager.Add(poop);
            //sideBar.ShowModal();
            

            NewLabel("Add", 15, false);
            NewLabel("Name", 65, false);
            NewLabel("Weight", 180, false);
            NewLabel("Affects\nOthers", 280, true);
            NewLabel("Affects\n  Self", 380, true);
            NewLabel("Draw", 480, false);

            addCompView = new AddComponentView(sidebar, poop, LeftPadding, 50);
            addCompView.InitNode(n);

            btnAdd = new Button(manager);
            btnAdd.Init();
            btnAdd.Parent = poop;
            btnAdd.Left = 10;
            btnAdd.Top = poop.Height - btnAdd.Height * 3;
            btnAdd.Left = poop.Width - btnAdd.Width - 50;
            btnAdd.Text = "Add";
            btnAdd.Click += AddComponents;

            btnCancel = new Button(manager);
            btnCancel.Init();
            btnCancel.Parent = poop;
            btnCancel.Left = btnAdd.Width + 30;
            btnCancel.Top = btnAdd.Top;
            btnCancel.Left = btnAdd.Left - btnCancel.Width - 30;
            btnCancel.Text = "Cancel";
            btnCancel.Click += Close;
        }
        public void Close(object sender, EventArgs e)
        {
            UserInterface.GameInputDisabled = false;
            manager.Remove(poop);
        }
        public void AddComponents(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            foreach(DetailedItem di in addCompView.viewItems)
            {
                if (!(di.obj is Type)) continue;
                if (!(di.itemControls["checkbox"] as CheckBox).Checked) continue;
                comp c = Utils.compEnums[(Type)di.obj];
                node.addComponent(c, true);
                foreach(Node n in node.group.fullSet)
                {
                    n.addComponent(c, true);
                }
            }
            view.RefreshRoot();
            ComponentView cv = (ComponentView)view;
            cv.RefreshRoot();
            Close(null, null);
        }


        public void NewLabel(string s, int left, bool line)
        {
            Label lbl = new Label(manager);
            lbl.Init();
            lbl.Parent = poop;
            lbl.Top = 35;
            lbl.Text = s;
            lbl.Left = left;
            lbl.Width = 100;
            if (line)
            {
                lbl.Top -= lbl.Height;
                lbl.Height += lbl.Height;
            }
        }

    }
}
