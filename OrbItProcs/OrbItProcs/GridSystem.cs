using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs {
    public class GridSystem {
        public Room room;
        public int cellsX, cellsY;
        public int cellwidth, cellheight;
        public int gridwidth, gridheight;
        public List<Node>[,] grid;
        public int cellReach = 2;

        //obsolete constructor
        public GridSystem(Room room, int gridwidth, int gridheight, int cellsX, int cellsY, int cellReach)
        {
            this.room = room;
            this.gridwidth = gridwidth;
            this.gridheight = gridheight;
            this.cellsX = cellsX;
            this.cellsY = cellsY;
            this.cellReach = cellReach;
            cellwidth = gridwidth / cellsX;
            cellheight = gridheight / cellsY;
            grid = new List<Node>[cellsX, cellsY];
            for (int i = 0; i < cellsX; i++)
            {
                for (int j = 0; j < cellsY; j++)
                {
                    grid[i, j] = new List<Node>();
                }
            }
        }

        public GridSystem(Room room, int cellsX, int cellReach)
        {
            this.room = room;
            this.gridwidth = room.game.worldWidth;
            this.gridheight = room.game.worldHeight;
            this.cellsX = cellsX;
            
            this.cellReach = cellReach;
            cellwidth = gridwidth / cellsX;
            cellheight = cellwidth;
            this.cellsY = gridheight / cellheight;
            //cellheight = gridheight / cellsY;
            grid = new List<Node>[cellsX, cellsY];
            for (int i = 0; i < cellsX; i++)
            {
                for (int j = 0; j < cellsY; j++)
                {
                    grid[i, j] = new List<Node>();
                }
            }
        }

        public void insert(Node node)
        {
            Tuple<int, int> indexs = getIndexs(node);
            //if (node == room.game.targetNode) Console.WriteLine("target indexs: {0} {1}",indexs.Item1,indexs.Item2);
            grid[indexs.Item1, indexs.Item2].Add(node);

        }

        public List<Node> retrieve(Node node, int reach = -1)
        {
            if (reach == -1) reach = cellReach;
            List<Node> returnList = new List<Node>();
            Tuple<int, int> indexs = getIndexs(node);
            int x = indexs.Item1;
            int y = indexs.Item2;
            //grid[indexs.Item1, indexs.Item2].Add(node);
            int xbegin, xend, ybegin, yend;
            xbegin = x - reach;
            xend = x + reach + 1;
            if (xbegin < 0) xbegin = 0;
            if (xend > cellsX) xend = cellsX;

            ybegin = y - reach;
            yend = y + reach + 1;
            if (ybegin < 0) ybegin = 0;
            if (yend > cellsY) yend = cellsY;
            //return box of slots
            /*
            for (int i = xbegin; i < xend; i++)
            {
                for (int j = ybegin; j < yend; j++)
                {
                    //grid[i, j] = new List<Node>();
                    //returnList.AddRange(grid[i, j]);
                    foreach (Node n in grid[i, j])
                    {
                        returnList.Add(n);
                    }

                }
            }
            */
            //return circle of slots
            ///*
            for (int i = xbegin; i < xend; i++)
            {
                for (int j = ybegin; j < yend; j++)
                {
                    // Wow. Never use Math class >.<
                    //double dist = Math.Pow(Math.Abs(x - i), 2) + Math.Pow(Math.Abs(y - j), 2);
                    int xd = (x - i) * (x - i);
                    if (xd < 0) xd *= -1;
                    int yd = (y - j) * (y - j);
                    if (yd < 0) yd *= -1;
                    int dist = xd + yd;

                    if (dist <= reach * reach)
                    {
                        returnList.AddRange(grid[i, j]);
                        //foreach (Node n in grid[i, j])
                        //{
                        //    returnList.Add(n);
                        //}
                    }

                }
            }
            ///*
            //Console.WriteLine("xbegin:{0} + xend:{1} + ybegin:{2} + yend:{3}", xbegin, xend, ybegin, yend);
            return returnList;

        }

        public Tuple<int, int> getIndexs(Node node)
        {
            Vector2 pos = new Vector2(node.position.X, node.position.Y);
            int x = (int)node.position.X;
            int y = (int)node.position.Y;
            int gridx = (int)pos.X - ((int)pos.X % cellwidth);
            x = gridx / cellwidth;
            //if ((int)pos.X - gridx > gridx + cellwidth - (int)node.radius) x++;
            int gridy = (int)pos.Y - ((int)pos.Y % cellheight);
            y = gridy / cellheight;
            //if ((int)pos.Y - gridy > gridy + cellheight - (int)node.radius) y++;

            if (x > cellsX - 1) x = cellsX - 1;
            if (y > cellsY - 1) y = cellsY - 1;
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            //Console.WriteLine("{0} + {1}", x, y);
            return new Tuple<int, int>(x, y);
        }

        public void clear()
        {
            for (int i = 0; i < cellsX; i++)
            {
                for (int j = 0; j < cellsY; j++)
                {
                    grid[i, j] = new List<Node>();
                }
            }
        }

        public bool ContainsNode(Node n)
        {

            for (int i = 0; i < cellsX; i++)
            {
                for (int j = 0; j < cellsY; j++)
                {
                    if (grid[i, j].Contains(n)) return true;
                }
            }
            return false;
        }

    }
}
