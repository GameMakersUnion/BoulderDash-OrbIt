using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomShane.Neoforce.Controls;

namespace OrbItProcs.Interface
{
    public partial class Sidebar
    {
        public StackView stackview;
        public Panel backPanel;

        public void InitializeSecondPage()
        {
            stackview = new StackView();
            TabPage second = tbcMain.TabPages[1];
            second.Text = "Links";

            backPanel = new Panel(manager);
            backPanel.Height = second.Height;
            backPanel.Width = second.Width;
            //backPanel.Width = second.Width + 20;
            backPanel.AutoScroll = true;
            backPanel.Init();
            second.Add(backPanel);

            

            CollapsePanel c1 = new CollapsePanel(manager, backPanel, "first"); stackview.AddPanel(c1);
            CollapsePanel c2 = new CollapsePanel(manager, backPanel, "second"); stackview.AddPanel(c2);
            CollapsePanel c3 = new CollapsePanel(manager, backPanel, "third"); stackview.AddPanel(c3);
            CollapsePanel c4 = new CollapsePanel(manager, backPanel, "fourth"); stackview.AddPanel(c4);
            CollapsePanel c5 = new CollapsePanel(manager, backPanel, "first"); stackview.AddPanel(c5);
            CollapsePanel c6 = new CollapsePanel(manager, backPanel, "second"); stackview.AddPanel(c6);
            CollapsePanel c7 = new CollapsePanel(manager, backPanel, "third"); stackview.AddPanel(c7);
            CollapsePanel c8 = new CollapsePanel(manager, backPanel, "fourth"); stackview.AddPanel(c8);

            //groupbox.Refresh();
            //groupbox.Width = groupbox.Width + 1;
            tbcMain.SelectedPage = tbcMain.TabPages[1];
            backPanel.Refresh();
            tbcMain.SelectedPage = tbcMain.TabPages[0];


            #region tests
            /*
            Window w = new Window(manager);
            manager.Add(w);
            w.Top = w.Left = 100;
            w.Height = w.Width = 200;
            w.Init();

            Panel c = new Panel(manager);
            c.AutoScroll = true;
            c.Init();
            c.Width = c.Height = 50;
            
            w.Add(c);

            Button b = new Button(manager);
            b.Init();
            b.Width = 20;
            b.Height = 20;
            b.Top = 150;
            b.Click += b_Click;
            c.Add(b);
            //*/
            #endregion

        }

        void b_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            game.ui.sidebar.backPanel.Refresh();
        }
    }
}
