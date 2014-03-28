using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TomShane.Neoforce.Controls;
using EventHandler = TomShane.Neoforce.Controls.EventHandler;

namespace OrbItProcs
{
    public class EditGroupWindow
    {
        //public Game1 game;
        public Manager manager;
        public Sidebar sidebar;
        public Window window;
        public ComponentView componentView;
        public int HeightCounter = 5;
        public int LeftPadding = 5;
        public Button btnOk;

        public EditGroupWindow(Sidebar sidebar)
        {
            this.sidebar = sidebar;
            this.manager = sidebar.manager;
            
            window = new Window(manager);
            window.Init();
            window.Left = sidebar.master.Left;
            window.Width = sidebar.Width;
            window.Top = 0;
            window.Resizable = false;
            window.Movable = false;
            window.Height = sidebar.game.Height;
            window.Text = "Edit";
            manager.Add(window);

            int width = 120;
            int offset = window.Width - width - 20;

            btnOk = new Button(manager);
            btnOk.Init();
            btnOk.Parent = window;
            btnOk.Left = LeftPadding;
            btnOk.Top = window.Height - (int)(btnOk.Height * 2.8);
            btnOk.Text = "Go Back";
            btnOk.Width = width;

            Label lblName = new Label(manager);
            lblName.Init();
            lblName.Parent = window;
            lblName.Left = LeftPadding;
            lblName.Top = HeightCounter;
            lblName.Width = width;
            lblName.Text = "Group Name:";

            TextBox txtName = new TextBox(manager);
            txtName.Init();
            txtName.Parent = window;
            txtName.Top = HeightCounter;
            txtName.Width = width;
            txtName.Left = offset;
            HeightCounter += txtName.Height + LeftPadding;

            componentView = new ComponentView(sidebar, window, 0, HeightCounter);
            componentView.Width = 200;
            
            window.Width += 100;
            window.Width -= 100;

            btnOk.Click += (s, e) =>
            {
                    sidebar.groupsView.UpdateGroups();
                    window.Close();
            };
        }


        public void SetGroup(Node n)
        {
            //Node clone = n.CreateClone();
            //Group g = tempgroup;
            //if (g == null)
            //{
            //    g = new Group(clone, parentGroup: sidebar.room.generalGroups);
            //}
            //else
            //{
            //g.defaultNode = clone;
            //g.EmptyGroup();
            //}
            //componentView.SwitchGroup(g);
        }
    }
}
