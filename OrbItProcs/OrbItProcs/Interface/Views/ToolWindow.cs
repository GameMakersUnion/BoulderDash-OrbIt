using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TomShane.Neoforce.Controls;
using EventHandler = TomShane.Neoforce.Controls.EventHandler;

namespace OrbItProcs
{

    public class ToolWindow
    {
        public Manager manager;
        public Sidebar sidebar;
        public Window window;
        public int HeightCounter = 5;
        public int LeftPadding = 5;
        public Dictionary<string, Button> buttons = new Dictionary<string, Button>();

        public ToolWindow(Sidebar sidebar)
        {
            this.sidebar = sidebar;
            this.manager = sidebar.manager;
            window = new Window(manager);
            window.Init();
            window.Top = 0;
            window.Height = sidebar.game.Height;
            window.Width = 70;
            window.Left = sidebar.game.Width - window.Width;
            window.CloseButtonVisible = false;
            window.Movable = false;
            window.Text = "Tools";
            manager.Add(window);
            bool second = false;
            for(int i = 0; i < 20; i++)
            {
                AddButton(i.ToString(), second, null);
                second = !second;
            }

        }

        public void AddButton(string s, bool secondRow, Action action)
        {
            Button button = new Button(manager);
            button.Init();
            button.Parent = window;
            button.Text = s;
            button.Width = (window.Width - 30) / 2;
            button.Left = 5;
            button.Top = HeightCounter;
            button.Height = button.Width;
            if (secondRow)
            {
                button.Left += button.Width + 5;
                HeightCounter += button.Height + 5;
            }
            
            button.Click += (se, e) =>
            {
                foreach(Button bb in buttons.Values)
                {
                    bb.TextColor = Color.White;
                }
                button.TextColor = UserInterface.TomShanePuke;
                if (action != null) action();
            };
            
        }
    }
}
