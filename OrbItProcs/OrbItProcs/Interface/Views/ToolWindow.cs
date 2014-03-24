﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TomShane.Neoforce.Controls;
using EventHandler = TomShane.Neoforce.Controls.EventHandler;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs
{

    public class ToolWindow
    {
        public Manager manager;
        public Sidebar sidebar;
        public SideBar toolBar;
        public int HeightCounter = 5;
        public int LeftPadding = 5;
        public Dictionary<string, Texture2D> buttonTextures = new Dictionary<string,Texture2D>();
  //      Texture2D[,] textures;
        public Dictionary<string, Button> buttons = new Dictionary<string, Button>();

        public ToolWindow(Sidebar sidebar)
        {
            this.sidebar = sidebar;
            this.manager = sidebar.manager;
            toolBar = new SideBar(manager);

            toolBar.MouseOver += delegate { UserInterface.tomShaneWasClicked = true; };
            toolBar.MouseOut += delegate { UserInterface.tomShaneWasClicked = false; };

            toolBar.Init();
            toolBar.Top = 0;
            toolBar.Height = sidebar.game.Height;
            toolBar.Width = 70;
            toolBar.Left = sidebar.game.Width - toolBar.Width;

            //toolBar.CloseButtonVisible = false;

            toolBar.Movable = false;
            toolBar.Text = "Tools";
            manager.Add(toolBar);
            bool second = false;

            Texture2D[,] textures = Program.getGame().Content.Load<Texture2D>("Textures/buttons").sliceSpriteSheet(2, 5);
            buttonTextures["select"] = textures[0,0];
            buttonTextures["random"] = textures[1,0];
            buttonTextures["spawn"] = textures[0,1];
            buttonTextures["level"] = textures[1,1];
            buttonTextures["forceSpawn"] = textures[0,2];
            buttonTextures["forcePush"] = textures[1,2];
            buttonTextures["control"] = textures[0,3];
            buttonTextures["static"] = textures[1,3];
            buttonTextures["remove"] = textures[0,4];



            


        }

        public void AddButton(string s, Action action, string tooltip = null)
        {
            Button button = new Button(manager);

            button.Init();
            button.Parent = toolBar;
            button.Text = "";
            button.Width = toolBar.Width - (30 / 2);
            button.Left = 5;
            button.Top = HeightCounter;
            button.Height = button.Width;

                HeightCounter += button.Height + 5;

            
            button.Click += (se, e) =>
            {
                foreach(Button bb in buttons.Values)
                {
                    bb.TextColor = Color.White;
                }
                button.TextColor = UserInterface.TomShanePuke;
                if (action != null) action();
            };
            if (!String.IsNullOrWhiteSpace(tooltip)) button.ToolTip.Text = tooltip;
            else button.ToolTip.Text = s;
            Texture2D tt = buttonTextures[s];

            button.Draw += (se, e) =>
            {
                e.Renderer.Draw(tt, e.Rectangle, Color.White);
            };

            buttons.Add(s, button);
        }
    }
}
