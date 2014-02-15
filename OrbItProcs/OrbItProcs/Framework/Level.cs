using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs
{
    public class Level
    {
        public Room room;
        public int cellsX, cellsY;
        public int cellWidth, cellHeight;
        public int gridWidth { get { return cellsX * cellWidth; } }
        public int gridHeight { get { return cellsY * cellWidth; } }
        
        public Level(Room room, int cellsX, int cellsY, int cellWidth, int? cellHeight = null)
        {
            this.room = room;
            this.cellsX = cellsX;
            this.cellsY = cellsY;
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight ?? cellWidth;
            

        }

        public void Update()
        {

        }

        public void Draw(SpriteBatch batch)
        {
            Vector2 MousePos = UserInterface.WorldMousePos;
            double x = MousePos.X / (double)cellWidth;
            int vertX = (int)Math.Floor(x + 0.5) * cellWidth;
            double y = MousePos.Y / (double)cellHeight;
            int vertY = (int)Math.Floor(y + 0.5) * cellHeight;
            Vector2 vert = new Vector2(vertX, vertY) / room.mapzoom;

            Texture2D tx = room.game.textureDict[textures.whitecircle];
            Vector2 cen = new Vector2(tx.Width / 2f, tx.Height / 2f);//store this in another textureDict to avoid recalculating

            batch.Draw(room.game.textureDict[textures.whitecircle], vert, null, Color.White, 0f, cen, 0.3f, SpriteEffects.None, 0);

        }

    }
}
