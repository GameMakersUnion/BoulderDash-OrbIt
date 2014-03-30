﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TomShane.Neoforce.Controls;
using EventArgs = TomShane.Neoforce.Controls.EventArgs;
using EventHandler = TomShane.Neoforce.Controls.EventHandler;
using Poop = TomShane.Neoforce.Controls.SideBar;

namespace OrbItProcs
{
    public class CreateGroupWindow
    {
        //public Game1 game;
        public Manager manager;
        public Sidebar sidebar;
        public SideBar poop;
        public ComponentView componentView;
        public int HeightCounter = 5;
        public int LeftPadding = 5;

        //public Label lblTitle;
        public Button btnOk;
        public Button btnBack;
        public Room temproom;
        public Group tempgroup;

        public CreateGroupWindow(Sidebar sidebar)
        {
            this.sidebar = sidebar;
            this.manager = sidebar.manager;
            sidebar.CreatingGroup = true;
            sidebar.ui.game.SwitchToTempRoom();
            temproom = sidebar.ui.game.tempRoom;
            tempgroup = sidebar.ActiveGroup;//temproom.generalGroups.childGroups.ElementAt(0).Value;

            poop = new Poop(manager);
            poop.Init();
            int tomtom = 5;
            poop.ClientArea.BackColor = UserInterface.TomDark;
            poop.BackColor = Color.Black;
            poop.BevelBorder = BevelBorder.All;
            Margins tomtomtomtom = new Margins(tomtom, tomtom, tomtom, tomtom);
            poop.ClientMargins = tomtomtomtom;

            poop.Left = sidebar.master.Left;
            poop.Width = sidebar.Width;
            poop.Top = 0;
            poop.Height = Game1.Height;
            //poop.Text = "Create Group";
            //poop.Closed += delegate { UserInterface.GameInputDisabled = false; sidebar.CreatingGroup = false; sidebar.ui.game.SwitchToMainRoom(); sidebar.groupsView.createGroupWindow = null; };
            //window.ShowModal();
            manager.Add(poop);

            int width = 120;
            int offset = poop.Width - width - 20;

            Panel topPanel = new Panel(manager);
            topPanel.Init();
            topPanel.Parent = poop;
            topPanel.Left = LeftPadding;
            topPanel.Top = LeftPadding;
            topPanel.Width = poop.Width - LeftPadding * 4;
            int col = 30;
            topPanel.Color = new Color(col, col, col);
            topPanel.BevelBorder = BevelBorder.All;
            topPanel.BevelStyle = BevelStyle.Flat;
            topPanel.BevelColor = Color.Black;

            btnBack = new Button(manager);
            btnBack.Init();
            btnBack.Parent = topPanel;
            btnBack.Top = HeightCounter;
            btnBack.Text = "Back";
            btnBack.Width = 40;
            btnBack.Left = LeftPadding;
            btnBack.Click += (s, e) => Close();

            topPanel.Height = btnBack.Height + LeftPadding * 3;

            Label lblTitle = new Label(manager);
            lblTitle.Init();
            lblTitle.Parent = topPanel;
            lblTitle.Top = HeightCounter + LeftPadding;
            lblTitle.Width = 120;
            lblTitle.Left = poop.Width / 2 - lblTitle.Width / 4;
            lblTitle.Text = "Create Group";

            HeightCounter += lblTitle.Height + LeftPadding * 6;

            btnOk = new Button(manager);
            btnOk.Init();
            btnOk.Parent = poop;
            btnOk.Top = poop.Height - (int)(btnOk.Height * 2.8);
            btnOk.Text = "Create Group";
            btnOk.Width = width;
            btnOk.Left = poop.Width / 2 - btnOk.Width / 2;

            Label lblName = new Label(manager);
            lblName.Init();
            lblName.Parent = poop;
            lblName.Left = LeftPadding;
            lblName.Top = HeightCounter;
            lblName.Width = width;
            lblName.Text = "Group Name:";

            TextBox txtName = new TextBox(manager);
            txtName.Init();
            txtName.Parent = poop;
            txtName.Top = HeightCounter;
            txtName.Width = width;
            txtName.Left = offset;
            HeightCounter += txtName.Height + LeftPadding;


            RadioButton rdEmpty = new RadioButton(manager);
            rdEmpty.Init();
            rdEmpty.Parent = poop;
            rdEmpty.Top = HeightCounter;
            rdEmpty.Left = LeftPadding;
            rdEmpty.Text = "Default";
            rdEmpty.Checked = true;
            HeightCounter += rdEmpty.Height + LeftPadding;

            RadioButton rdExisting = new RadioButton(manager);
            rdExisting.Init();
            rdExisting.Parent = poop;
            rdExisting.Top = HeightCounter;
            rdExisting.Left = LeftPadding;
            rdExisting.Text = "Existing";
            rdExisting.Checked = false;
            rdExisting.Width = width;

            ComboBox cbExisting = new ComboBox(manager);
            cbExisting.Init();
            cbExisting.Parent = poop;
            cbExisting.Top = HeightCounter;
            cbExisting.Width = width;
            cbExisting.Left = offset;
            foreach(Group g in sidebar.game.mainRoom.generalGroups.childGroups.Values)
            {
                cbExisting.Items.Add(g);
            }
            cbExisting.ItemIndex = 0;
            cbExisting.Enabled = false;
            HeightCounter += cbExisting.Height + LeftPadding;

            RadioButton rdTemplate = new RadioButton(manager);
            rdTemplate.Init();
            rdTemplate.Parent = poop;
            rdTemplate.Top = HeightCounter;
            rdTemplate.Left = LeftPadding;
            rdTemplate.Text = "Template";
            rdTemplate.Checked = false;
            rdTemplate.Width = width;

            ComboBox cbTemplate = new ComboBox(manager);
            cbTemplate.Init();
            cbTemplate.Parent = poop;
            cbTemplate.Top = HeightCounter;
            cbTemplate.Width = width;
            cbTemplate.Left = offset;
            foreach (Node n in Assets.NodePresets)
            {
                cbTemplate.Items.Add(n);
            }
            if (Assets.NodePresets.Count > 0) cbTemplate.ItemIndex = 0;
            cbTemplate.Enabled = false;
            HeightCounter += cbTemplate.Height + LeftPadding;

            componentView = new ComponentView(sidebar, poop, 0, HeightCounter);
            componentView.Width = poop.Width - LeftPadding * 4;

            poop.Width += 100;
            poop.Width -= 100;

            SetGroup(temproom.defaultNode);


            rdEmpty.Click += (s, e) =>
            {
                cbExisting.Enabled = false;
                cbTemplate.Enabled = false;
                SetGroup(sidebar.room.defaultNode);
            };
            rdExisting.Click += (s, e) =>
            {
                cbExisting.Enabled = true;
                cbTemplate.Enabled = false;
                ComboUpdate(cbExisting);
            };
            cbExisting.ItemIndexChanged += (s, e) =>
            {
                ComboUpdate(cbExisting);
            };
            rdTemplate.Click += (s, e) =>
            {
                cbExisting.Enabled = false;
                cbTemplate.Enabled = true;
                ComboUpdate(cbTemplate);
            };
            cbTemplate.ItemIndexChanged += (s, e) =>
            {
                ComboUpdate(cbTemplate);
            };
            
            btnOk.Click += (s, e) =>
            {
                if (String.IsNullOrWhiteSpace(txtName.Text))
                    PopUp.Toast("Please enter a group name.");
                else if(sidebar.game.mainRoom.generalGroups.childGroups.Keys.Contains(txtName.Text))
                    PopUp.Toast("Group already exists.");
                else{                   
                    Node newNode = tempgroup.defaultNode.CreateClone();
                    Game1.game.room = sidebar.game.mainRoom;
                    newNode.body.color = ColorChanger.randomColorHue();
                    newNode.basicdraw.UpdateColor();
                    Group newGroup = new Group(newNode, sidebar.game.mainRoom.generalGroups, Name: txtName.Text.Trim());
                    newNode.name = txtName.Text.Trim();
                    newNode.group = newGroup;
                    sidebar.groupsView.UpdateGroups();

                    Close();
                }
            };
        }

        public void Close()
        {
            UserInterface.GameInputDisabled = false;
            sidebar.CreatingGroup = false;
            sidebar.ui.game.SwitchToMainRoom();
            sidebar.groupsView.createGroupWindow = null;
            manager.Remove(poop);
        }

        public void ComboUpdate(ComboBox cb)
        {
            if (cb.ItemIndex >= 0 && !cb.Text.Equals(""))
            {
                object o = cb.Items.ElementAt(cb.ItemIndex);
                if (o is Group)
                {
                    SetGroup((o as Group).defaultNode);
                }
                else if (o is Node)
                {
                    SetGroup((Node)o);
                }
            }
        }

        public void SetGroup(Node n)
        {
            Node clone = n.CreateClone();
            Group g = tempgroup;
            //if (g == null)
            //{
            //    g = new Group(clone, parentGroup: sidebar.room.generalGroups);
            //}
            //else
            //{
                g.defaultNode = clone;
                g.EmptyGroup();
            //}
            componentView.SwitchGroup(g);
        }
    }
}