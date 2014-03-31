﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TomShane.Neoforce.Controls;
using EventHandler = TomShane.Neoforce.Controls.EventHandler;

namespace OrbItProcs
{
    public class OptionsWindow
    {
        public Manager manager;
        public Sidebar sidebar;
        public Window window;
        public int HeightCounter = 5;
        public int LeftPadding = 5;

        public ComboBox cbUserLevel;
        public Label lblUserLevel;
        public Button btnOk;

        public OptionsWindow(Sidebar sidebar)
        {
            UserInterface.GameInputDisabled = true;
            this.manager = sidebar.manager;
            this.sidebar = sidebar;
            
            window = new Window(manager);
            window.Init();
            window.Left = sidebar.master.Left;
            window.Width = sidebar.master.Width;
            window.Top = 200;
            window.Height = 200;
            window.Text = "Options";

            window.Closed += delegate { UserInterface.GameInputDisabled = false; };
            window.ShowModal();
            manager.Add(window);

            btnOk = new Button(manager);
            btnOk.Init();
            btnOk.Parent = window;
            btnOk.Left = LeftPadding;
            btnOk.Top = window.Height - (btnOk.Height * 3);
            btnOk.Text = "Ok";// +"\u2713";
            btnOk.Click += (s, e) => window.Close();

            lblUserLevel = new Label(manager);
            lblUserLevel.Init();
            lblUserLevel.Parent = window;
            lblUserLevel.Left = LeftPadding;
            lblUserLevel.Top = HeightCounter;
            lblUserLevel.Text = "User Level";
            lblUserLevel.Width += 10;

            cbUserLevel = new ComboBox(manager);
            cbUserLevel.Init();
            cbUserLevel.Parent = window;
            cbUserLevel.Top = HeightCounter;
            cbUserLevel.Left = lblUserLevel.Width;
            cbUserLevel.Width = 150;
            HeightCounter += cbUserLevel.Height;
            cbUserLevel.TextColor = Color.Black;
            foreach(string ul in Enum.GetNames(typeof(UserLevel)))
            {
                cbUserLevel.Items.Add(ul);
            }
            cbUserLevel.ItemIndexChanged += (s, e) =>
            {
                sidebar.userLevel = (UserLevel)cbUserLevel.ItemIndex;
                
            };
            int count = 0;
            foreach(object s in cbUserLevel.Items)
            {
                if (s.ToString().Equals(sidebar.userLevel.ToString()))
                {
                    cbUserLevel.ItemIndex = count;
                }
                count++;
            }

            CreateCheckBox("FullScreen", OrbIt.isFullScreen, (o, e) =>
            {
                if ((o as CheckBox).Checked) OrbIt.game.setResolution(resolutions.AutoFullScreen, true);
                else OrbIt.game.setResolution(resolutions.WXGA_1280x800, false);
            });
            CreateCheckBox("Hide Links", sidebar.ui.game.room.DrawLinks, (o, e) => sidebar.ui.game.room.DrawLinks = !(o as CheckBox).Checked);

            CreateCheckBox("Edit Selected Node", false, (o, e) => Utils.notImplementedException() );
            
            //CreateCheckBox("Edit Selected Node", sidebar.EditSelectedNode, (o, e) => sidebar.EditSelectedNode = (o as CheckBox).Checked);
        }

        public void CreateCheckBox(string key, bool isChecked, EventHandler ev)
        {
            CheckBox cb = new CheckBox(manager);
            cb.Init();
            cb.Parent = window;
            cb.Text = key;
            cb.Top = HeightCounter;
            cb.Left = LeftPadding;
            cb.Width = 180;
            cb.Click += ev;
            cb.Checked = isChecked;
            HeightCounter += cb.Height;
        }
    }
}
