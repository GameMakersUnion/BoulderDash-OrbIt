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
        public bool EditSelectedNode
        {
            get { return _EditSelectedNode; }
            set
            {
                if (componentView != null)
                {
                    if (value)
                    {
                        if (room.targetNode != null)
                            componentView.SwitchNode(room.targetNode, false);
                    }
                    else
                    {
                        componentView.SwitchGroup(ActiveGroup);
                    }
                }
                _EditSelectedNode = value;
            }
        }

        public ComponentView componentView { get; set; }
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

        public void InitializeFourthPage()
        {
            tbcMain.Visible = false;

            tbcViews = new TabControl(manager);
            tbcViews.Init();
            tbcViews.Parent = master;
            tbcViews.Left = 0;
            tbcViews.Top = 0;
            tbcViews.Width = master.Width - 5;
            tbcViews.Height = Game1.smallWidth - 40;
            tbcViews.Anchor = Anchors.All;

            tbcViews.AddPage();
            TabPage editTab = tbcViews.TabPages[0];
            editTab.Text = "Edit";

            tbcViews.SelectedIndex = 0;

            activeTabControl = tbcViews;

            componentView = new ComponentView(this, editTab, 0, 0);
            componentView.SwitchGroup(room.masterGroup.childGroups["General Groups"].childGroups.ElementAt(0).Value);

            btnOptions = new Button(manager);
            btnOptions.Init();
            btnOptions.Parent = editTab;
            btnOptions.Left = LeftPadding;
            btnOptions.Top = editTab.Height - btnOptions.Height - LeftPadding;
            btnOptions.Text = "Options";
            btnOptions.Click += (s, e) =>
            {
                OptionsWindow oWindow = new OptionsWindow(this);
            };

            toolWindow = new ToolWindow(this);
        }

        public void InitializeFifthPage()
        {
            tbcViews.AddPage();
            TabPage groupsTab = tbcViews.TabPages[1];
            //tbcViews.Color = Color.Transparent;
            groupsTab.Text = "Groups";

            tbcViews.SelectedIndex = 1;
            activeTabControl = tbcViews;
            //detailedView = new DetailedView(this, testTab, 0, 0);
            //inspectorView = new InspectorView(this, groupsTab, 0, 0);
            groupsView = new GroupsView(this, groupsTab, 0, 0);
            groupsView.UpdateGroups();
            tbcViews.SelectedIndex = 0;
        }


    }
}
