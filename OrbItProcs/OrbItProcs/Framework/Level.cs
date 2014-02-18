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
        public int cellsX { get; set; }
        public int cellsY { get; set; }

        public int cellWidth { get; set; }
        public int cellHeight { get; set; }

        public int gridWidth { get { return cellsX * cellWidth; } }
        public int gridHeight { get { return cellsY * cellWidth; } }

        public Level() { }

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
            

        }

    }
}
