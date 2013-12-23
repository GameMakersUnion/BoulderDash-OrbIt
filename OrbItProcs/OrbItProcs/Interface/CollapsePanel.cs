using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomShane.Neoforce.Controls;


namespace OrbItProcs.Interface
{
    public class CollapsePanel
    {
        public GroupPanel panel;
        public Button collapseButton;
        public Control parent;
        public Dictionary<string, Control> panelControls;

        public int ExpandedHeight { get; set; }

        public int Top
        {
            get { return panel.Top; }
            set { panel.Top = value; collapseButton.Top = value; }
        }
        public int Left
        {
            get { return panel.Left; }
            set { panel.Left = value; collapseButton.Left = value; }
        }
        public int Width
        {
            get { return panel.Width; }
            set { panel.Width = value; }
        }
        public int Height
        {
            get { return panel.Height; }
            set { panel.Height = value; }
        }
        public string Text
        {
            get { return panel.Text; }
            set { panel.Text = value; }
        }

        public CollapsePanel(Manager manager, Control parent, string Name, int expandedHeight = 100)
        {
            this.panel = new GroupPanel(manager);
            panel.Init();
            panel.Height = expandedHeight;
            panel.Width = 180;
            panel.Text = "  " + Name.Trim();
            this.collapseButton = new Button(manager);
            collapseButton.Init();
            collapseButton.Width = 15;
            collapseButton.Height = 18;
            collapseButton.Text = "^";
            collapseButton.Click += collapseButton_Click;
            this.ExpandedHeight = expandedHeight;

            this.panelControls = new Dictionary<string, Control>();
            this.parent = parent;
            parent.Add(panel);
            parent.Add(collapseButton);

            parent.Refresh();
        }

        void collapseButton_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            Toggle();
            parent.Refresh();
        }

        public void Add(string name, Control control)
        {
            panel.Add(control);
            if (panelControls.ContainsValue(control)) return;
            panelControls.Add(name, control);
        }

        public void Collapse()
        {
            collapseButton.Text = "v";
            panel.Height = 20;
        }
        public void Expand()
        {
            collapseButton.Text = "^";
            panel.Height = ExpandedHeight;
        }

        public void Toggle()
        {
            if (collapseButton.Text.Equals("^"))
            {
                Collapse();

            }
            else
            {
                Expand();
            }
            //StackView.Refresh();
        }

    }
}
