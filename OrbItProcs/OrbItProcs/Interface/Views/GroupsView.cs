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

namespace OrbItProcs.Interface.Views
{
    public class GroupsView : ListView<GroupsViewItem>
    {

        public GroupsView(Sidebar sidebar, Control parent, int Left, int Top)
            : base(sidebar, parent, Left, Top)
        {

        }

    }

    public class GroupsViewItem : ViewItem
    {
        public GroupsView groupsView;
        public Button btnRemove, btnEdit;

        public GroupsViewItem(Manager manager, GroupsView groupsView, object obj, Control parent, int Top, int Left, int Width)
            : base(manager, obj, parent, Top, Left, Width)
        {
            if (obj is Group)
            {
                this.groupsView = groupsView;
                label.Text = (obj as Group).Name;

                Group g = (Group)obj;
                btnEdit = new Button(manager);
                btnEdit.Init();
                btnEdit.Parent = textPanel;
                btnEdit.Left = parent.Width - 5;
                btnEdit.Height = buttonHeight;
                btnEdit.Width = buttonWidth;
                btnEdit.Text = "e";
                btnEdit.ToolTip.Text = "Edit";

                btnRemove = new Button(manager);
                btnRemove.Init();
                btnRemove.Parent = parent;
                btnRemove.Left = btnEdit.Left - 20;
                btnRemove.Height = buttonHeight;
                btnRemove.Width = buttonWidth;
                btnRemove.Text = "-";
                btnRemove.ToolTip.Text = "Remove";
                //todo: add handlers
            }
        }
    }
}
