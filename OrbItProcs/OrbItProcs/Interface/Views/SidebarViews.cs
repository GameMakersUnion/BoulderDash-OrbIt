﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomShane.Neoforce.Controls;
using Component = OrbItProcs.Component;
using Console = System.Console;
using EventHandler = TomShane.Neoforce.Controls.EventHandler;
using EventArgs = TomShane.Neoforce.Controls.EventArgs;
using Microsoft.Xna.Framework;

namespace OrbItProcs
{
    //views pages
    public partial class Sidebar
    {
        public DetailedView detailedView { get; set; }
        public InspectorView inspectorView { get; set; }
        public GroupsView groupsView { get; set; }
        public GroupsView presetsView { get; set; }

        public TabControl tbcViews;

        private TabControl _activeTabControl;

        public Button btnOptions;

        public TabControl activeTabControl
        {
            get { return _activeTabControl; }
            set
            {
                if (value != null)
                {
                    if (_activeTabControl != null && value != _activeTabControl)
                    {
                        _activeTabControl.Visible = false;
                    }
                }
                _activeTabControl = value;
                _activeTabControl.Visible = true;
                _activeTabControl.Refresh();
            }
        }

        public ToolWindow toolWindow;
        public TabControl tbcGroups;
        public void InitializeGroupsPage()
        {
            tbcMain.Visible = false;

            tbcViews = new TabControl(manager);
            tbcViews.Init();
            master.Add(tbcViews);
            tbcViews.Left = 0;
            tbcViews.Top = 0;
            tbcViews.Width = master.Width - 5;
            tbcViews.Height = master.Height - 40;
            tbcViews.Anchor = Anchors.All;
            tbcViews.Color = UserInterface.TomLight;

            tbcViews.AddPage();

            TabPage groupsTab = tbcViews.TabPages[0];
            //tbcViews.Color = Color.Transparent;
            groupsTab.Text = "Groups";
            tbcViews.SelectedIndex = 0;
            activeTabControl = tbcViews;

            TitlePanel titlePanelGroups = new TitlePanel(this, groupsTab, "Groups", false);

            tbcGroups = new TabControl(manager);
            tbcGroups.Init();
            tbcGroups.Parent = groupsTab;
            tbcGroups.Top = titlePanelGroups.Height * 2;
            tbcGroups.Height = 400;
            tbcGroups.Width = groupsTab.Width;

            tbcGroups.AddPage("Custom");
            TabPage customPage = tbcGroups.TabPages[0];
            groupsView = new GroupsView(this, customPage, 0, -20, room.generalGroups);
            groupsView.btnCreateGroup.Text = "Create Custom Group";
            groupsView.lblGroupLabel.Text = "Custom Groups";
            groupsView.UpdateGroups();

            
            tbcGroups.AddPage("Presets");
            tbcGroups.SelectedIndex = 1;
            TabPage presetsPage = tbcGroups.TabPages[1];
            presetsView = new GroupsView(this, presetsPage, 0, -20, room.presetGroups);
            presetsView.btnCreateGroup.Text = "Create Preset Group";
            presetsView.lblGroupLabel.Text = "Preset Groups";
            presetsView.UpdateGroups();
            tbcGroups.SelectedIndex = 0;

            tbcViews.SelectedIndex = 0;

            toolWindow = new ToolWindow(this);
            gamemodeWindow = new GamemodeWindow(this);
            gamemodeWindow.window.Visible = false;

            Button btnGameMode = new Button(manager);
            btnGameMode.Init();
            btnGameMode.Top = tbcViews.Top + tbcViews.Height;
            btnGameMode.Left = 15;
            btnGameMode.Text = "Mode";
            btnGameMode.Width = (int)manager.Skin.Fonts[0].Resource.MeasureString(btnGameMode.Text).X+10;
            btnGameMode.ClientMargins = new Margins(0, btnGameMode.ClientMargins.Top, 0, btnGameMode.ClientMargins.Bottom);
            btnGameMode.Anchor = Anchors.Bottom;
            master.Add(btnGameMode);
            btnGameMode.Click += (s, e) =>
            {
                gamemodeWindow.window.Visible = !gamemodeWindow.window.Visible;
            };

            btnOptions = new Button(manager);
            btnOptions.Init();
            master.Add(btnOptions);
            btnOptions.Left = btnGameMode.Left+btnGameMode.Width;
            btnOptions.Top = tbcViews.Top + tbcViews.Height;
            btnOptions.Text = "Options";
            btnOptions.Width = (int)manager.Skin.Fonts[0].Resource.MeasureString(btnOptions.Text).X + 10;
            btnOptions.ClientMargins = new Margins(0, btnOptions.ClientMargins.Top, 0, btnOptions.ClientMargins.Bottom);
            btnOptions.Anchor = Anchors.Bottom;

            btnOptions.Click += (s, e) =>
            {
                new OptionsWindow(this);
            };

            btnFullScreen = new Button(manager);
            btnFullScreen.Init();
            master.Add(btnFullScreen);
            btnFullScreen.Left = btnOptions.Left + btnOptions.Width;
            btnFullScreen.Top = tbcViews.Top + tbcViews.Height;
            btnFullScreen.Text = "FullScreen";
            btnFullScreen.Width = (int)manager.Skin.Fonts[0].Resource.MeasureString(btnFullScreen.Text).X + 10;
            btnFullScreen.ClientMargins = new Margins(0, btnFullScreen.ClientMargins.Top, 0, btnFullScreen.ClientMargins.Bottom);
            btnFullScreen.Anchor = Anchors.Bottom;

            btnFullScreen.Click += (s, e) =>
            {
                if (btnFullScreen.Text == "FullScreen")
                {
                    btnFullScreen.Text = "Windowed";
                    game.setResolution(game.preferredFullScreen ?? resolutions.AutoFullScreen, true);
                }
                else
                {
                    game.setResolution(game.preferredFullScreen ?? resolutions.WSXGA_1680x1050, false);
                    btnFullScreen.Text = "FullScreen";
                }
            };

            btnPause = new Button(manager);
            btnPause.Init();
            master.Add(btnPause);
            btnPause.Left = btnFullScreen.Left + btnFullScreen.Width;
            btnPause.Top = tbcViews.Top + tbcViews.Height;
            btnPause.Text = "Pause";
            btnPause.Width = (int)manager.Skin.Fonts[0].Resource.MeasureString("Resume").X + 10;
            btnPause.ClientMargins = new Margins(0, btnPause.ClientMargins.Top, 0, btnPause.ClientMargins.Bottom);
            btnPause.Anchor = Anchors.Bottom;

            btnPause.Click += (s, e) =>
            {

                ui.IsPaused = !ui.IsPaused;
                btnPause.Text = ui.IsPaused ? "Resume" : "Pause";

            };


        }
        GamemodeWindow gamemodeWindow;
        public PlayerView playerView;
        public void InitializePlayersPage()
        {
            tbcViews.AddPage();
            TabPage playersTab = tbcViews.TabPages[1];
            playersTab.Text = "Players";
            tbcViews.SelectedIndex = 1;
            activeTabControl = tbcViews;

            TitlePanel titlePanelPlayers = new TitlePanel(this, playersTab, "Players", false);

            playerView = new PlayerView(this, playersTab, LeftPadding, titlePanelPlayers.Height);



        }
        GroupsView itemsView;
        private Button btnFullScreen;
        private Button btnPause;
        public void InitializeItemsPage()
        {
            tbcViews.AddPage();
            TabPage itemsTab = tbcViews.TabPages[2];
            itemsTab.Text = "Items";
            tbcViews.SelectedIndex = 2;
            activeTabControl = tbcViews;

            TitlePanel titlePanelItems = new TitlePanel(this, itemsTab, "Items", false);

            itemsView = new GroupsView(this, itemsTab, 0, titlePanelItems.Height, room.itemGroup);
            
            itemsView.UpdateGroups();

            tbcViews.SelectedIndex = 0;
        }

        public void InitializeBulletsPage()
        {
            tbcViews.AddPage();
            TabPage bulletsTab = tbcViews.TabPages[3];
            bulletsTab.Text = "Bullets";
            tbcViews.SelectedIndex = 3;
            activeTabControl = tbcViews;

            TitlePanel titlePanelBullets = new TitlePanel(this, bulletsTab, "Bullets", false);

            //itemsView = new GroupsView(this, testingTab, 0, 0, room.itemGroup);
            //itemsView.lblGroupLabel.Text = "Testing";
            //itemsView.UpdateGroups();


            tbcViews.SelectedIndex = 0;
        }

    }
}
