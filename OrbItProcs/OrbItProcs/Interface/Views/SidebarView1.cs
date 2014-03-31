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
        private bool _EditSelectedNode = false;
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
            tbcViews.AddPage();
            tbcViews.AddPage();

            TabPage groupsTab = tbcViews.TabPages[0];
            //tbcViews.Color = Color.Transparent;
            groupsTab.Text = "Groups";
            tbcViews.SelectedIndex = 0;
            activeTabControl = tbcViews;
            //detailedView = new DetailedView(this, testTab, 0, 0);
            //inspectorView = new InspectorView(this, groupsTab, 0, 0);
            groupsView = new GroupsView(this, groupsTab, 0, 0, room.generalGroups);
            groupsView.UpdateGroups();
            tbcViews.SelectedIndex = 0;

            toolWindow = new ToolWindow(this);

        }
        public PlayerView playerView;
        public void InitializePlayersPage()
        {
            TabPage playersTab = tbcViews.TabPages[1];
            playersTab.Text = "Players";
            tbcViews.SelectedIndex = 1;
            activeTabControl = tbcViews;

            //componentView = new ComponentView(this, editTab, 0, 0);
            //componentView.SwitchGroup(room.masterGroup.childGroups["General Groups"].childGroups.ElementAt(0).Value);
            playerView = new PlayerView(this, playersTab, LeftPadding, 80);


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
        public void InitializeItemsPage()
        {
            TabPage itemsTab = tbcViews.TabPages[2];
            itemsTab.Text = "Items";
            tbcViews.SelectedIndex = 2;
            activeTabControl = tbcViews;
            groupsView = new GroupsView(this, itemsTab, 0, 0, room.itemGroup);
            groupsView.UpdateGroups();

            tbcViews.SelectedIndex = 0;
        }

    }
}
