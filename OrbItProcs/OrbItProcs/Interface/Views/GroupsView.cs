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
using System.Collections.ObjectModel;

namespace OrbItProcs
{
    public class GroupsView : DetailedView
    {
        Label groupLabel;
        Button btnCreateGroup, btnSelectedNode;
        public CreateGroupWindow createGroupWindow;
        public EditGroupWindow editGroupWindow;
        public GroupsView(Sidebar sidebar, Control parent, int Left, int Top)
            : base(sidebar, parent, Left, Top, false)
        {
            HeightCounter = Top + 23;
            groupLabel = new Label(manager);
            groupLabel.Init();
            groupLabel.Parent = parent;
            groupLabel.Left = LeftPadding;
            groupLabel.Top = HeightCounter;
            groupLabel.Width = sidebar.Width/2;
            groupLabel.Text = "Groups";
            groupLabel.TextColor = Color.Black;
            HeightCounter += groupLabel.Height + LeftPadding;

            ItemCreator += ItemCreatorDelegate;

            Initialize();

            btnCreateGroup = new Button(manager);
            btnCreateGroup.Init();
            btnCreateGroup.Parent = parent;
            btnCreateGroup.Top = HeightCounter;
            btnCreateGroup.Left = LeftPadding;
            btnCreateGroup.Text = "Create Group";
            btnCreateGroup.Width = 120;
            btnCreateGroup.Click += (s, e) =>
            {
                createGroupWindow = new CreateGroupWindow(sidebar);

            };
            HeightCounter += btnCreateGroup.Height + LeftPadding;

            btnSelectedNode = new Button(manager);
            btnSelectedNode.Init();
            btnSelectedNode.Parent = parent;
            btnSelectedNode.Top = HeightCounter;
            btnSelectedNode.Left = LeftPadding;
            btnSelectedNode.Text = "Selected Node to Group";
            btnSelectedNode.Width = 160;
            btnSelectedNode.Click += (s, e) =>
            {
                if (room.targetNode == null) return;
                Group g = new Group(room.targetNode.CreateClone(), room.generalGroups);
                UpdateGroups();
            };
        }

        public override void SelectItem(DetailedItem item)
        {
            base.SelectItem(item);
            if (item.obj == null) return;
            if (item.obj is Group)
            {
                //editGroupWindow.componentView.SwitchGroup((Group)item.obj);
            }
        }
        public void UpdateGroups()
        {
            ClearView();
            showRemoveButton = sidebar.game.mainRoom.generalGroups.childGroups.Count > 1;
            foreach (Group g in sidebar.game.mainRoom.generalGroups.childGroups.Values)
            {
                CreateNewItem(g);
            }
        }
        private bool showRemoveButton = false;
        public void CreateNewItem(Group g)
        {
            int top = 0;
            if (viewItems.Count > 0)
            {
                top = (viewItems[0].itemHeight - 4) * viewItems.Count;
            }
            DetailedItem detailedItem = new DetailedItem(manager, this, g, backPanel, top, LeftPadding, backPanel.Width - 4);
            base.CreateItem(detailedItem);
        }

        private void ItemCreatorDelegate(DetailedItem item, object obj)
        {
            if (item.obj is Group)
            {
                Group g = (Group)item.obj;
                item.label.Text = g.Name;
                Button btnEdit = new Button(manager);
                btnEdit.Init();
                btnEdit.Parent = item.panel;
                btnEdit.Width = 30;
                btnEdit.Left = item.panel.Width - btnEdit.Width - 10;
                btnEdit.Top = 2;
                btnEdit.Height = item.buttonHeight;
                
                btnEdit.Text = "Edit";
                btnEdit.ToolTip.Text = "Edit";
                btnEdit.TextColor = UserInterface.TomShanePuke;
                btnEdit.Click += (s, e) =>
                {
                    item.isSelected = true;
                    editGroupWindow = new EditGroupWindow(sidebar);
                    editGroupWindow.componentView.SwitchGroup(g);

                };

                Button btnEnabled = new Button(manager);
                btnEnabled.Init();
                btnEnabled.Parent = item.panel;
                btnEnabled.Width = 30;
                btnEnabled.Left = btnEdit.Left - btnEnabled.Width - 5;
                btnEnabled.Top = 2;
                btnEnabled.Height = item.buttonHeight;
                //btnEnabled.Draw += btnEnabled_Draw;

                //btnEnabled.Text = "On";
                SetButtonBool(btnEnabled, !g.Disabled);
                btnEnabled.ToolTip.Text = "Group Enabled";
                btnEnabled.TextColor = UserInterface.TomShanePuke;
                btnEnabled.Click += (s, e) =>
                {
                    g.Disabled = !GetButtonBool(btnEnabled);
                    SetButtonBool(btnEnabled, !g.Disabled);
                };

                if (!showRemoveButton) return;

                Button btnRemove = new Button(manager);
                btnRemove.Init();
                btnRemove.Parent = item.panel;
                btnRemove.Width = item.buttonWidth;
                btnRemove.Top = 2;
                btnRemove.Left = btnEnabled.Left - btnRemove.Width - 5;
                btnRemove.Height = item.buttonHeight;
                
                btnRemove.TextColor = Color.Red;
                btnRemove.Text = "-";
                btnRemove.ToolTip.Text = "Remove";
                btnRemove.Click += (s, e) =>
                {

                    if (sidebar.groupsView.editGroupWindow.componentView.activeGroup == g) 
                    {
                        sidebar.groupsView.editGroupWindow.componentView.ClearView();
                        sidebar.groupsView.editGroupWindow.componentView.activeGroup = null;
                    }
                    g.EmptyGroup();
                    g.DeleteGroup();
                    UpdateGroups();
                };
            }
        }

        void btnEnabled_Draw(object sender, DrawEventArgs e)
        {
            //Button b = (Button)sender;
            //new Rectangle(500, 500, 600, 600)
            e.Renderer.Draw(sidebar.game.textureDict[textures.blackorb], e.Rectangle, new Rectangle(0,0,25,25), Color.White);
        }
    }
}
