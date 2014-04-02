using System;
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
    //first view page of sidebar
    public partial class Sidebar
    {
        //private bool _EditSelectedNode = false;
        //public bool EditSelectedNode
        //{
        //    get { return _EditSelectedNode; }
        //    set
        //    {
        //        if (componentView != null)
        //        {
        //            if (value)
        //            {
        //                if (room.targetNode != null)
        //                    componentView.SwitchNode(room.targetNode, false);
        //            }
        //            else
        //            {
        //                componentView.SwitchGroup(ActiveGroup);
        //            }
        //        }
        //        _EditSelectedNode = value;
        //    }
        //}

        //public ComponentView componentView { get; set; }
        public DetailedView detailedView { get; set; }
        public InspectorView inspectorView { get; set; }
        public GroupsView groupsView { get; set; }

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

        public void InitializeGroupsPage()
        {
            tbcMain.Visible = false;

            tbcViews = new TabControl(manager);
            tbcViews.Init();
            tbcViews.Parent = master;
            tbcViews.Left = 0;
            tbcViews.Top = 0;
            tbcViews.Width = master.Width - 5;
            tbcViews.Height = OrbIt.Width - 40;
            tbcViews.Anchor = Anchors.All;
            tbcViews.Color = UserInterface.TomLight;

            tbcViews.AddPage();

            TabPage groupsTab = tbcViews.TabPages[0];
            //tbcViews.Color = Color.Transparent;
            groupsTab.Text = "Groups";
            tbcViews.SelectedIndex = 0;
            activeTabControl = tbcViews;

            TitlePanel titlePanelGroups = new TitlePanel(this, groupsTab, "Groups", false);

            groupsView = new GroupsView(this, groupsTab, 0, titlePanelGroups.Height, room.generalGroups);
            groupsView.UpdateGroups();
            tbcViews.SelectedIndex = 0;

            toolWindow = new ToolWindow(this);

        }
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


            btnOptions = new Button(manager);
            btnOptions.Init();
            btnOptions.Parent = playersTab;
            btnOptions.Left = LeftPadding;
            btnOptions.Top = playersTab.Height - btnOptions.Height - LeftPadding;
            btnOptions.Text = "Options";

            btnOptions.Click += (s, e) =>
            {
                new OptionsWindow(this);
            };
        }
        GroupsView itemsView;
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
