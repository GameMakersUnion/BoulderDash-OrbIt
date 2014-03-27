using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TomShane.Neoforce.Controls;
using EventHandler = TomShane.Neoforce.Controls.EventHandler;

namespace OrbItProcs
{
    public class CreateGroupWindow
    {
        //public Game1 game;
        public Manager manager;
        public Sidebar sidebar;
        public Window window;
        public ComponentView componentView;
        public int HeightCounter = 5;
        public int LeftPadding = 5;

        //public Label lblTitle;
        public Button btnOk;
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
            
            window = new Window(manager);
            window.Init();
            window.Left = sidebar.master.Left;
            window.Width = sidebar.Width;
            window.Top = 0;
            window.Height = sidebar.game.Height;
            window.Text = "Create Group";
            window.Closed += delegate { UserInterface.GameInputDisabled = false; sidebar.CreatingGroup = false; sidebar.ui.game.SwitchToMainRoom(); sidebar.groupsView.createGroupWindow = null; };
            //window.ShowModal();
            manager.Add(window);

            int width = 120;
            int offset = window.Width - width - 20;

            btnOk = new Button(manager);
            btnOk.Init();
            btnOk.Parent = window;
            btnOk.Left = LeftPadding;
            btnOk.Top = window.Height - (int)(btnOk.Height * 2.8);
            btnOk.Text = "Create Group";
            btnOk.Width = width;

            Label lblName = new Label(manager);
            lblName.Init();
            lblName.Parent = window;
            lblName.Left = LeftPadding;
            lblName.Top = HeightCounter;
            lblName.Width = width;
            lblName.Text = "Group Name:";

            TextBox txtName = new TextBox(manager);
            txtName.Init();
            txtName.Parent = window;
            txtName.Top = HeightCounter;
            txtName.Width = width;
            txtName.Left = offset;
            HeightCounter += txtName.Height + LeftPadding;


            RadioButton rdEmpty = new RadioButton(manager);
            rdEmpty.Init();
            rdEmpty.Parent = window;
            rdEmpty.Top = HeightCounter;
            rdEmpty.Left = LeftPadding;
            rdEmpty.Text = "Default";
            rdEmpty.Checked = true;
            HeightCounter += rdEmpty.Height + LeftPadding;

            RadioButton rdExisting = new RadioButton(manager);
            rdExisting.Init();
            rdExisting.Parent = window;
            rdExisting.Top = HeightCounter;
            rdExisting.Left = LeftPadding;
            rdExisting.Text = "Existing";
            rdExisting.Checked = false;
            rdExisting.Width = width;

            ComboBox cbExisting = new ComboBox(manager);
            cbExisting.Init();
            cbExisting.Parent = window;
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
            rdTemplate.Parent = window;
            rdTemplate.Top = HeightCounter;
            rdTemplate.Left = LeftPadding;
            rdTemplate.Text = "Template";
            rdTemplate.Checked = false;
            rdTemplate.Width = width;

            ComboBox cbTemplate = new ComboBox(manager);
            cbTemplate.Init();
            cbTemplate.Parent = window;
            cbTemplate.Top = HeightCounter;
            cbTemplate.Width = width;
            cbTemplate.Left = offset;
            foreach (Node n in sidebar.game.NodePresets)
            {
                cbTemplate.Items.Add(n);
            }
            if (sidebar.game.NodePresets.Count > 0) cbTemplate.ItemIndex = 0;
            cbTemplate.Enabled = false;
            HeightCounter += cbTemplate.Height + LeftPadding;

            componentView = new ComponentView(sidebar, window, 0, HeightCounter);
            componentView.Width = 200;

            window.Width += 100;
            window.Width -= 100;

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
                    newNode.room = sidebar.game.mainRoom;
                    newNode.body.color = ColorChanger.randomColorHue();
                    newNode.basicdraw.UpdateColor();
                    Group newGroup = new Group(newNode, sidebar.game.mainRoom.generalGroups, Name: txtName.Text.Trim());
                    newNode.name = txtName.Text.Trim();
                    newNode.group = newGroup;
                    sidebar.groupsView.UpdateGroups();

                    window.Close();
                }
            };
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
