using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TomShane.Neoforce.Controls;

namespace OrbItProcs.Interface
{
    public class LinkGeneratorWindow
    {
        public Manager manager;
        public Window window;
        public int HeightCounter3;
        //LinkGenerator
        ComboBox cbLinkType, cbLinkPresets, cbLinkPalette, cbLinkFormation;
        Label lblGenerateLink, lblLinkType, lblLinkPresets, lblLinkPalette, lblLinkFormation;
        CheckBox chkEntangled;
        Button btnAddToPalette;

        public LinkGeneratorWindow(Manager manager)
        {
            Game1 game = Program.getGame();
            game.ui.GameInputDisabled = true;
            this.manager = manager;
            window = new Window(manager);
            window.Init();
            window.Left = game.ui.sidebar.master.Left - 50;
            window.Width = 240;
            window.Top = 200;
            window.Height = 200;
            window.Text = "Link Generator";
            window.Closed += delegate { game.ui.GameInputDisabled = false; };
            window.ShowModal();
            manager.Add(window);

            //LinkGenerator.ExpandedHeight += 30;
            HeightCounter3 = 0;
            Window parent2 = window;
            int left = 0;
            int middle = 100;

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

            lblLinkFormation = new Label(manager);
            lblLinkFormation.Init();
            lblLinkFormation.Left = left;
            lblLinkFormation.Text = "Formation";
            lblLinkFormation.Parent = parent2;
            lblLinkFormation.Top = HeightCounter3; HeightCounter3 += lblLinkFormation.Height;

            cbLinkFormation = new ComboBox(manager);
            cbLinkFormation.Init();
            cbLinkFormation.Left = left;
            cbLinkFormation.Width += 20;
            cbLinkFormation.Parent = parent2;
            cbLinkFormation.Top = HeightCounter3; HeightCounter3 += cbLinkFormation.Height;

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
        }
    }
}
