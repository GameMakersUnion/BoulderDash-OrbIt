using System;
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
    public class AddComponentWindow
    {
        public bool addToGroup;
        public AddComponentView addCompView;
        public Manager manager;
        public Sidebar sidebar;
        public Poop poop;
        public int HeightCounter = 5;
        public int LeftPadding = 5;
        public Label lblComp, lblProperties;
        public Button btnAdd;//, btnCancel;
        public Node node;
        public DetailedView view;
        public Control under;
        public AddComponentWindow(Sidebar sidebar, Control under, Node n, DetailedView view, bool addToGroup = true)
        {
            this.under = under;
            under.Visible = false;
            sidebar.master.Visible = false;
            this.addToGroup = addToGroup;

            Control par = sidebar.tbcViews.TabPages[0];
            UserInterface.GameInputDisabled = true;
            this.view = view;
            this.manager = sidebar.manager;
            this.sidebar = sidebar;
            this.node = n;
            poop = new Poop(manager);
            poop.Init();
            poop.Left = sidebar.master.Left;
            poop.Width = par.Width;
            poop.Top = 5;
            poop.Height = par.Height + 15;
            poop.BevelBorder = BevelBorder.All;
            poop.BevelColor = Color.Black;
            poop.Left = LeftPadding;// poop.Width - poop.Width / 2;
            poop.Text = "Add Component";
            poop.BackColor = new Color(30, 60, 30);
            manager.Add(poop);
            //sideBar.ShowModal();

            TitlePanel titlePanelAddComponent = new TitlePanel(sidebar, poop, "Add Component", true);
            titlePanelAddComponent.btnBack.Click += Close;
            HeightCounter += titlePanelAddComponent.Height;
            //
            NewLabel("Add", 15, false);
            NewLabel("Name", 50, false);
            //NewLabel("Weight", 180, false);
            //NewLabel("Affects\nOthers", 280, true);
            //NewLabel("Affects\n  Self", 380, true);
            //NewLabel("Draw", 480, false);
            int left = 145;
            NewLabel("AO", left, false);
            NewLabel("AS", left + 20, false);
            NewLabel("DR", left + 40, false);

            addCompView = new AddComponentView(sidebar, poop, LeftPadding, 80, par.Height - 260);
            
            //addCompView.Height += 210;
            addCompView.InitNode(n);

            btnAdd = new Button(manager);
            btnAdd.Init();
            btnAdd.Parent = poop;
            btnAdd.Width = 150;
            btnAdd.Top = poop.Height - btnAdd.Height * 2;
            btnAdd.Left = poop.Width / 2 - btnAdd.Width / 2;
            btnAdd.Text = "Add Components";
            btnAdd.Click += AddComponents;

            //btnCancel = new Button(manager);
            //btnCancel.Init();
            //btnCancel.Parent = poop;
            //btnCancel.Left = btnAdd.Width + 30;
            //btnCancel.Top = btnAdd.Top;
            //btnCancel.Left = btnAdd.Left - btnCancel.Width - 30;
            //btnCancel.Text = "Cancel";
            //btnCancel.Click += Close;
        }
        public void Close(object sender, EventArgs e)
        {
            UserInterface.GameInputDisabled = false;
            manager.Remove(poop);
            under.Visible = true;
            sidebar.master.Visible = true;
        }
        public void AddComponents(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            foreach(DetailedItem di in addCompView.viewItems)
            {
                if (!(di.obj is Type)) continue;
                if (!(di.itemControls["checkbox"] as CheckBox).Checked) continue;
                Type c =(Type)di.obj;
                node.addComponent(c, true);
                if (node.group != null && addToGroup) //todo: more checks about whether to add to everyone in group
                {
                    foreach (Node n in node.group.fullSet)
                    {
                        n.addComponent(c, true);
                    }
                }
            }
            if (view != null)
            {
                view.RefreshRoot();
                ComponentView cv = (ComponentView)view;
                cv.RefreshRoot();
            }
            Close(null, null);
        }
        public void NewLabel(string s, int left, bool line)
        {
            Label lbl = new Label(manager);
            lbl.Init();
            lbl.Parent = poop;
            lbl.Top = 60;
            lbl.Text = s;
            lbl.Left = left;
            lbl.Width = 100;
            if (line)
            {
                lbl.Top -= lbl.Height;
                lbl.Height += lbl.Height;
            }
        }

    }
}
