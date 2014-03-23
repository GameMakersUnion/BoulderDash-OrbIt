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
                        componentView.SwitchGroup(ActiveGroupFirst);
                    }
                }
                _EditSelectedNode = value;
            }
        }

        public ComponentView componentView { get; set; }
        public DetailedView detailedView { get; set; }
        public InspectorView inspectorView { get; set; }

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

        public void InitializeFourthPage()
        {
            tbcMain.Visible = false;

            tbcViews = new TabControl(manager);
            tbcViews.Init();
            tbcViews.Parent = master;
            tbcViews.Left = 0;
            tbcViews.Top = 0;
            tbcViews.Width = master.Width - 5;
            tbcViews.Height = Program.getGame().Width - 40;
            tbcViews.Anchor = Anchors.All;

            tbcViews.AddPage();
            TabPage editTab = tbcViews.TabPages[0];
            editTab.Text = "Edit";

            tbcViews.SelectedIndex = 0;

            activeTabControl = tbcViews;

            componentView = new ComponentView(this, editTab, 0, 0);

            btnOptions = new Button(manager);
            btnOptions.Init();
            btnOptions.Parent = editTab;
            btnOptions.Left = LeftPadding;
            btnOptions.Top = editTab.Height - btnOptions.Height - LeftPadding;
            btnOptions.Text = "Options";
            btnOptions.Click += (s, e) =>
            {
                OptionsWindow oWindow = new OptionsWindow(manager, this);
            };
        }

        public void InitializeFifthPage()
        {
            tbcMain.Visible = false;

            tbcViews.AddPage();
            TabPage testTab = tbcViews.TabPages[1];
            //tbcViews.Color = Color.Transparent;
            testTab.Text = "Test";

            tbcViews.SelectedIndex = 1;

            activeTabControl = tbcViews;

            //detailedView = new DetailedView(this, testTab, 0, 0);
            inspectorView = new InspectorView(this, testTab, 0, 0);
            tbcViews.SelectedIndex = 0;
        }
    }
}
