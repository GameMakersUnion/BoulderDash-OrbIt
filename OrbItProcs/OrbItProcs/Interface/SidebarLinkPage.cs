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
        public int HeightCounter3;
        public int VertPadding3;

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

            HeightCounter3 = 0;
            VertPadding3 = 0;

            CollapsePanel SourceTarget = new CollapsePanel(manager, backPanel, "Source      |   Target"); stackview.AddPanel(SourceTarget);
            CollapsePanel LinkSelect = new CollapsePanel(manager, backPanel, "second"); stackview.AddPanel(LinkSelect);
            CollapsePanel LinkPalette = new CollapsePanel(manager, backPanel, "third"); stackview.AddPanel(LinkPalette);
            CollapsePanel c4 = new CollapsePanel(manager, backPanel, "fourth"); stackview.AddPanel(c4);
            CollapsePanel c5 = new CollapsePanel(manager, backPanel, "first"); stackview.AddPanel(c5);
            CollapsePanel c6 = new CollapsePanel(manager, backPanel, "second"); stackview.AddPanel(c6);
            CollapsePanel c7 = new CollapsePanel(manager, backPanel, "third"); stackview.AddPanel(c7);
            CollapsePanel c8 = new CollapsePanel(manager, backPanel, "fourth"); stackview.AddPanel(c8);

            tbcMain.SelectedPage = tbcMain.TabPages[1];


            #region /// Source | Target ///

            Label lblSource, lblTarget, lblGroupS, lblGroupT, lblNodeS, lblNodeT;
            ComboBox cbGroupS, cbGroupT, cbNodeS, cbNodeT;
            RadioButton rdGroupS, rdGroupT, rdNodeS, rdNodeT, rdSelectionS, rdSelectionT;

            int left = backPanel.Left;
            int middle = backPanel.Left + backPanel.Width / 2 - 7;
            int right = backPanel.Width;

            SourceTarget.ExpandedHeight += 60;

            #region /// Source Side ///

            lblSource = new Label(manager);
            lblSource.Init();
            lblSource.Left = left;
            lblSource.Top = HeightCounter3; HeightCounter3 += lblSource.Height + VertPadding3;
            lblSource.Text = "   Source";
            lblSource.Parent = SourceTarget.panel;

            lblGroupS = new Label(manager);
            lblGroupS.Init();
            lblGroupS.Left = left;
            lblGroupS.Top = HeightCounter3; HeightCounter3 += lblGroupS.Height + VertPadding3;
            lblGroupS.Text = "Group";
            lblGroupS.Parent = SourceTarget.panel;

            cbGroupS = new ComboBox(manager);
            cbGroupS.Init();
            cbGroupS.Left = left;
            cbGroupS.Top = HeightCounter3; HeightCounter3 += cbGroupS.Height + VertPadding3;
            cbGroupS.Width = middle;
            cbGroupS.Parent = SourceTarget.panel;

            lblNodeS = new Label(manager);
            lblNodeS.Init();
            lblNodeS.Left = left;
            lblNodeS.Top = HeightCounter3; HeightCounter3 += lblNodeS.Height + VertPadding3;
            lblNodeS.Text = "Node";
            lblNodeS.Parent = SourceTarget.panel;

            cbNodeS = new ComboBox(manager);
            cbNodeS.Init();
            cbNodeS.Left = left;
            cbNodeS.Top = HeightCounter3; HeightCounter3 += cbNodeS.Height + VertPadding3;
            cbNodeS.Width = middle;
            cbNodeS.Parent = SourceTarget.panel;

            GroupBox radioBoxSource = new GroupBox(manager);
            radioBoxSource.Init();
            radioBoxSource.Left = left;
            radioBoxSource.Top = HeightCounter3 - 7;
            radioBoxSource.Width = middle;
            radioBoxSource.Height = 75;
            radioBoxSource.Text = "";
            radioBoxSource.Parent = SourceTarget.panel;

            HeightCounter3 = 5;

            rdGroupS = new RadioButton(manager);
            rdGroupS.Init();
            rdGroupS.Left = left;
            rdGroupS.Top = HeightCounter3; HeightCounter3 += rdGroupS.Height + VertPadding3;
            rdGroupS.Width = middle;
            rdGroupS.Text = "Group";
            rdGroupS.Parent = radioBoxSource;

            rdNodeS = new RadioButton(manager);
            rdNodeS.Init();
            rdNodeS.Left = left;
            rdNodeS.Top = HeightCounter3; HeightCounter3 += rdNodeS.Height + VertPadding3;
            rdNodeS.Width = middle;
            rdNodeS.Text = "Node";
            rdNodeS.Parent = radioBoxSource;

            rdSelectionS = new RadioButton(manager);
            rdSelectionS.Init();
            rdSelectionS.Left = left;
            rdSelectionS.Top = HeightCounter3; HeightCounter3 += rdSelectionS.Height + VertPadding3;
            rdSelectionS.Width = middle;
            rdSelectionS.Text = "Selection";
            rdSelectionS.Parent = radioBoxSource;

            #endregion

            #region /// Target Side ///

            HeightCounter3 = 0;

            lblTarget = new Label(manager);
            lblTarget.Init();
            lblTarget.Left = middle;
            lblTarget.Top = HeightCounter3; HeightCounter3 += lblTarget.Height + VertPadding3;
            lblTarget.Text = "|   Target";
            lblTarget.Parent = SourceTarget.panel;

            lblGroupT = new Label(manager);
            lblGroupT.Init();
            lblGroupT.Left = middle;
            lblGroupT.Top = HeightCounter3; HeightCounter3 += lblGroupT.Height + VertPadding3;
            lblGroupT.Text = "Group";
            lblGroupT.Parent = SourceTarget.panel;

            cbGroupT = new ComboBox(manager);
            cbGroupT.Init();
            cbGroupT.Left = middle;
            cbGroupT.Top = HeightCounter3; HeightCounter3 += cbGroupT.Height + VertPadding3;
            cbGroupT.Width = middle;
            cbGroupT.Parent = SourceTarget.panel;

            lblNodeT = new Label(manager);
            lblNodeT.Init();
            lblNodeT.Left = middle;
            lblNodeT.Top = HeightCounter3; HeightCounter3 += lblNodeT.Height + VertPadding3;
            lblNodeT.Text = "Node";
            lblNodeT.Parent = SourceTarget.panel;

            cbNodeT = new ComboBox(manager);
            cbNodeT.Init();
            cbNodeT.Left = middle;
            cbNodeT.Top = HeightCounter3; HeightCounter3 += cbNodeT.Height + VertPadding3;
            cbNodeT.Width = middle;
            cbNodeT.Parent = SourceTarget.panel;

            GroupBox radioBoxTarget = new GroupBox(manager);
            radioBoxTarget.Init();
            radioBoxTarget.Left = middle;
            radioBoxTarget.Top = HeightCounter3 - 7;
            radioBoxTarget.Width = middle;
            radioBoxTarget.Height = 75;
            radioBoxTarget.Text = "";
            radioBoxTarget.Parent = SourceTarget.panel;

            HeightCounter3 = 5;

            rdGroupT = new RadioButton(manager);
            rdGroupT.Init();
            rdGroupT.Left = left;
            rdGroupT.Top = HeightCounter3; HeightCounter3 += rdGroupT.Height + VertPadding3;
            rdGroupT.Width = middle;
            rdGroupT.Text = "Group";
            rdGroupT.Parent = radioBoxTarget;

            rdNodeT = new RadioButton(manager);
            rdNodeT.Init();
            rdNodeT.Left = left;
            rdNodeT.Top = HeightCounter3; HeightCounter3 += rdNodeT.Height + VertPadding3;
            rdNodeT.Width = middle;
            rdNodeT.Text = "Node";
            rdNodeT.Parent = radioBoxTarget;

            rdSelectionT = new RadioButton(manager);
            rdSelectionT.Init();
            rdSelectionT.Left = left;
            rdSelectionT.Top = HeightCounter3; HeightCounter3 += rdSelectionT.Height + VertPadding3;
            rdSelectionT.Width = middle;
            rdSelectionT.Text = "Selection";
            rdSelectionT.Parent = radioBoxTarget;

            #endregion

            #endregion

            #region /// Link Select ///

            ComboBox cbLinkType, cbLinkPresets, cbLinkPalette, cbLinkFormat;
            Label lblGenerateLink, lblLinkType, lblLinkPresets, lblLinkPalette, lblLinkFormat;
            CheckBox chkEntangled;
            Button btnAddToPalette;

            LinkSelect.ExpandedHeight += 30;
            HeightCounter3 = 0;
            GroupPanel parent2 = LinkSelect.panel;

            lblGenerateLink = new Label(manager);
            lblGenerateLink.Init();
            lblGenerateLink.Left = left + middle / 2;
            lblGenerateLink.Top = HeightCounter3; HeightCounter3 += lblGenerateLink.Height;
            lblGenerateLink.Text = "Generate Link";
            lblGenerateLink.Width += 40;
            lblGenerateLink.Parent = parent2;

            lblLinkType = new Label(manager);
            lblLinkType.Init();
            lblLinkType.Left = left;
            lblLinkType.Text = "Link Type";
            lblLinkType.Parent = parent2;
            lblLinkType.Top = HeightCounter3; HeightCounter3 += lblLinkType.Height;

            cbLinkType = new ComboBox(manager);
            cbLinkType.Init();
            cbLinkType.Left = left;
            cbLinkType.Width += 20;
            cbLinkType.Parent = parent2;
            cbLinkType.Top = HeightCounter3; HeightCounter3 += cbLinkType.Height;

            lblLinkFormat = new Label(manager);
            lblLinkFormat.Init();
            lblLinkFormat.Left = left;
            lblLinkFormat.Text = "Format";
            lblLinkFormat.Parent = parent2;
            lblLinkFormat.Top = HeightCounter3; HeightCounter3 += lblLinkFormat.Height;

            cbLinkFormat = new ComboBox(manager);
            cbLinkFormat.Init();
            cbLinkFormat.Left = left;
            cbLinkFormat.Width += 20;
            cbLinkFormat.Parent = parent2;
            cbLinkFormat.Top = HeightCounter3; HeightCounter3 += cbLinkFormat.Height;

            chkEntangled = new CheckBox(manager);
            chkEntangled.Init();
            chkEntangled.Left = left;
            chkEntangled.Width += 20;
            chkEntangled.Text = "Entangled";
            chkEntangled.Parent = parent2;
            chkEntangled.Top = HeightCounter3; HeightCounter3 += chkEntangled.Height;

            HeightCounter3 = lblGenerateLink.Height;

            lblLinkPresets = new Label(manager);
            lblLinkPresets.Init();
            lblLinkPresets.Left = left + middle;
            lblLinkPresets.Text = "Preset";
            lblLinkPresets.Parent = parent2;
            lblLinkPresets.Top = HeightCounter3; HeightCounter3 += lblLinkPresets.Height;

            cbLinkPresets = new ComboBox(manager);
            cbLinkPresets.Init();
            cbLinkPresets.Left = left + middle;
            cbLinkPresets.Width += 20;
            cbLinkPresets.Parent = parent2;
            cbLinkPresets.Top = HeightCounter3; HeightCounter3 += cbLinkPresets.Height;

            btnAddToPalette = new Button(manager);
            btnAddToPalette.Init();
            btnAddToPalette.Left = left + middle;
            btnAddToPalette.Width = middle;
            btnAddToPalette.Text = "Add to\nPalette";
            btnAddToPalette.Height = btnAddToPalette.Height * 2 - 10;
            btnAddToPalette.Parent = parent2;
            btnAddToPalette.Top = HeightCounter3 + 10; HeightCounter3 += btnAddToPalette.Height + 10;



            #endregion

            #region /// Link Palette ///

            LinkPalette.ExpandedHeight += 60;
            HeightCounter3 = 0;
            GroupPanel parent3 = LinkPalette.panel;

            Label lblPaletteTitle;
            ComboBox cbLinkList;
            InspectorBox LinkInspectorBox;
            Button btnCreateLink;

            #endregion


            //stackview.MovePanel(0, 1);

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

            GroupBox box = new GroupBox(manager);
            box.Init();
            box.Left = 100;
            box.Parent = w;
            box.Width = 150;
            box.Height = 100;
            box.Text = "";
            


            //*/
            #endregion

        }

        void b_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            game.ui.sidebar.backPanel.Refresh();
        }
    }
}
