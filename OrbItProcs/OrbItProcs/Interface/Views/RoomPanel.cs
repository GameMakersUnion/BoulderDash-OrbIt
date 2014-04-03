﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomShane.Neoforce.Controls;

namespace OrbItProcs
{
    public class RoomPanel
    {
        public Panel panel;
        private int Padding;
        public int Height { get { return panel.Height + Padding; } }
        public RoomPanel(Sidebar sidebar, Control parent, Room room, bool interactive, int Top = 0, int Padding = 5)
        {
            this.Padding = Padding;
            panel = new Panel(sidebar.manager);
            panel.Init();
            panel.Top = Top;
            panel.Left = Padding;
            panel.Width = room.worldWidth;
            panel.Height = room.worldHeight;

            //roomPanel.Left = (parent.Width - roomPanel.Width)/2;

            int col = 30;

            panel.Color = new Color(col, col, col);
            panel.BevelBorder = BevelBorder.All;
            panel.BevelStyle = BevelStyle.Flat;
            panel.BevelColor = Color.Black;

            parent.Add(panel);
            panel.ClientArea.Draw += (s, e) =>
            {
                e.Renderer.Draw(room.roomRenderTarget, e.Rectangle, Color.White);
            };
            OrbIt.onUpdate += delegate { panel.Refresh(); };
        }
    }
}