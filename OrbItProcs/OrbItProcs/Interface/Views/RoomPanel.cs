using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomShane.Neoforce.Controls;

namespace OrbItProcs
{
    public class RoomPanel
    {
        public Panel roomPanel;
        private int Padding;
        public int Height { get { return roomPanel.Height + Padding; } }
        public RoomPanel(Sidebar sidebar, Control parent, Room room, bool interactive, int Top = 0, int Padding = 5)
        {

            roomPanel = new Panel(sidebar.manager);
            roomPanel.Init();
            roomPanel.Width = room.worldWidth;
            roomPanel.Height = room.worldHeight;

            //roomPanel.Left = (parent.Width - roomPanel.Width)/2;

            int col = 30;

            roomPanel.Color = new Color(col, col, col);
            roomPanel.BevelBorder = BevelBorder.All;
            roomPanel.BevelStyle = BevelStyle.Flat;
            roomPanel.BevelColor = Color.Black;

            parent.Add(roomPanel);
            roomPanel.ClientArea.Draw += (s, e) =>
            {
                e.Renderer.Draw(room.roomRenderTarget, e.Rectangle, Color.White);
            };
            OrbIt.onUpdate += delegate { roomPanel.Refresh(); };
        }
    }
}
