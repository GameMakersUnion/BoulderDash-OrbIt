using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TomShane.Neoforce.Controls;
using EventHandler = TomShane.Neoforce.Controls.EventHandler;

namespace OrbItProcs
{
    public class AddComponentWindow
    {
        public AddComponentView addCompView;

        public Manager manager;
        public Sidebar sidebar;
        public Window window;
        public int HeightCounter = 5;
        public int LeftPadding = 5;
        public Label lblComp, lblProperties;
        Button btnAdd, btnCancel;
        public Node node;
        public AddComponentWindow(Sidebar sidebar, Node n)
        {
            UserInterface.GameInputDisabled = true;
            this.manager = sidebar.manager;
            this.sidebar = sidebar;
            this.node = n;
            window = new Window(manager);
            window.Init();
            window.Left = sidebar.master.Left;
            window.Width = 600;
            window.Top = 50;
            window.Height = 380;
            window.Left = window.Width - window.Width / 2;
            window.Text = "Add Component";
            manager.Add(window);
            window.ShowModal();
            

            NewLabel("Add", 15, false);
            NewLabel("Name", 65, false);
            NewLabel("Weight", 180, false);
            NewLabel("Affects\nOthers", 280, true);
            NewLabel("Affects\n  Self", 380, true);
            NewLabel("Draw", 480, false);

            addCompView = new AddComponentView(sidebar, window, LeftPadding, 50);
            addCompView.InitNode(n);

            btnAdd = new Button(manager);
            btnAdd.Init();
            btnAdd.Parent = window;
            btnAdd.Left = 10;
            btnAdd.Top = window.Height - btnAdd.Height * 3;
            btnAdd.Left = window.Width - btnAdd.Width - 50;
            btnAdd.Text = "Add";
            btnAdd.Click += AddComponents;

            btnCancel = new Button(manager);
            btnCancel.Init();
            btnCancel.Parent = window;
            btnCancel.Left = btnAdd.Width + 30;
            btnCancel.Top = btnAdd.Top;
            btnCancel.Left = btnAdd.Left - btnCancel.Width - 30;
            btnCancel.Text = "Cancel";
            btnCancel.Click += (s, e) => window.Close();

            window.Closed += (s, e) =>
            {
                UserInterface.GameInputDisabled = false;
            };
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
            if (sidebar.CreatingGroup && sidebar.groupsView.createGroupWindow != null)
            {
                sidebar.groupsView.createGroupWindow.componentView.RefreshComponents();
            }
            window.Close();
        }


        public void NewLabel(string s, int left, bool line)
        {
            Label lbl = new Label(manager);
            lbl.Init();
            lbl.Parent = window;
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
