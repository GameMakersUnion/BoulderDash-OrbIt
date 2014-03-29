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
        public Button btnBack;

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
            poop.Height = Game1.Height;
            poop.Text = "Edit";
            manager.Add(poop);

            int width = 120;
            int offset = poop.Width - width - 20;

            Panel topPanel = new Panel(manager);
            topPanel.Init();
            topPanel.Parent = poop;
            topPanel.Left = LeftPadding;
            topPanel.Top = LeftPadding;
            topPanel.Width = poop.Width - LeftPadding * 4;
            int col = 30;
            topPanel.Color = new Color(col, col, col);
            topPanel.BevelBorder = BevelBorder.All;
            topPanel.BevelStyle = BevelStyle.Flat;
            topPanel.BevelColor = Color.Black;

            btnBack = new Button(manager);
            btnBack.Init();
            btnBack.Parent = topPanel;
            btnBack.Top = HeightCounter;
            btnBack.Text = "Back";
            btnBack.Width = 40;
            btnBack.Left = LeftPadding;

            topPanel.Height = btnBack.Height + LeftPadding * 3;


            Label lblTitle = new Label(manager);
            lblTitle.Init();
            lblTitle.Parent = topPanel;
            lblTitle.Top = HeightCounter + LeftPadding;
            lblTitle.Width = 120;
            lblTitle.Left = poop.Width / 2 - lblTitle.Width / 4;
            lblTitle.Text = "Edit Group";

            HeightCounter += lblTitle.Height + LeftPadding * 6;

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

            btnBack.Click += (s, e) =>
            {
                    sidebar.groupsView.UpdateGroups();
                    manager.Remove(poop);
            };
        }
    }
}
