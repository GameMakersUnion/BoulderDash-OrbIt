using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TomShane.Neoforce.Controls;
using System.Reflection;

using OrbItProcs;

namespace OrbItProcs.Interface {
    public class AddComponentWindow {
        public Manager manager;
        public Game1 game;
        public Dialog window;
        public ComboBox cbBox;
        public Button btnAdd, btnCancel;
        public Label lbl;

        public AddComponentWindow(Game1 game)
        {
            this.game = game;
            this.manager = game.Manager;

            Initialize();
        }

        public void Initialize()
        {
            window = new Dialog(manager);

            window.Init();
            window.ShowModal();
            //window.TopPanel.Visible = true;
            window.Text = "Chose Component";
            window.Width = 200;
            //window.Passive = true;
            window.Height = game.sHeight / 4;
            //window.Center();
            window.Visible = true;
            window.Resizable = false;
            window.Movable = false;
            //window.Parent = sidebar;
            window.Anchor = Anchors.Top | Anchors.Right | Anchors.Bottom;
            window.BorderVisible = true;
            //window.MaximumHeight = game.sHeight;
            //window.MinimumHeight = game.sHeight;
            //window.MaximumWidth = 300;
            //window.MinimumWidth = 200;
            window.Alpha = 255;
            window.SetPosition(game.sWidth - 210, game.sHeight / 4);
            
            window.FocusLost += new TomShane.Neoforce.Controls.EventHandler(window_FocusLost);
            window.Closing += new WindowClosingEventHandler(window_FocusLost);

            window.StayOnTop = true;
            //window.Activate();
            
            manager.Add(window);

            cbBox = new ComboBox(manager);
            cbBox.Init();
            cbBox.Width = 160;
            cbBox.Left = 10;
            cbBox.Top = 20;
            cbBox.Parent = window;
            
        }

        void window_FocusLost(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            Dialog window = (Dialog)sender;
            //window.Focused = true;
            //manager.FocusedControl = window;
            //manager.AutoUnfocus = false;
            //manager.GetControl("win1").Invalidate();
            System.Console.WriteLine("Dfdf");
        }
    }

}
