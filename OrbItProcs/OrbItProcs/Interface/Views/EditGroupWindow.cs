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
    public class EditGroupWindow
    {
        //public Game1 game;
        public Manager manager;
        public Sidebar sidebar;
        public Poop poop;
        public ComponentView componentView;
        public int HeightCounter = 5;
        public int LeftPadding = 5;

        public EditGroupWindow(Sidebar sidebar)
        {
            this.sidebar = sidebar;
            this.manager = sidebar.manager;

            poop = new Poop(manager);
            poop.Init();
            int tomtom = 5;
            poop.ClientArea.BackColor = UserInterface.TomDark;
            poop.BackColor = Color.Black;
            poop.BevelBorder = BevelBorder.All;
            Margins tomtomtomtom = new Margins(tomtom, tomtom, tomtom, tomtom);
            poop.ClientMargins = tomtomtomtom;

            poop.Left = sidebar.master.Left;
            poop.Width = sidebar.Width;
            poop.Top = 0;
            poop.Resizable = false;
            poop.Movable = false;
            poop.Height = OrbIt.Height;
            poop.Text = "Edit";
            manager.Add(poop);

            int width = 120;
            int offset = poop.Width - width - 20;

            TitlePanel titlePanelEditGroup = new TitlePanel(sidebar, poop, "Edit Group", true);
            titlePanelEditGroup.btnBack.Click += (s, e) =>
            {
                sidebar.groupsView.UpdateGroups();
                manager.Remove(poop);
            };

            HeightCounter += titlePanelEditGroup.Height;

            Label lblName = new Label(manager);
            lblName.Init();
            lblName.Parent = poop;
            lblName.Left = LeftPadding;
            lblName.Top = HeightCounter;
            lblName.Width = width;
            lblName.Text = "Group Name:";

            TextBox txtName = new TextBox(manager);
            txtName.Init();
            txtName.Parent = poop;
            txtName.Top = HeightCounter;
            txtName.Width = width;
            txtName.Left = offset;
            HeightCounter += txtName.Height + LeftPadding;

            componentView = new ComponentView(sidebar, poop, 0, HeightCounter);
            componentView.Width = poop.Width - 20;
            componentView.insView.Height += componentView.insView.Height / 2;

            poop.Width += 100;
            poop.Width -= 100;

            
        }
    }
}
