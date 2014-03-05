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
        public int cellsX { get; set; }
        public int cellsY { get; set; }

        public int cellWidth { get; set; }
        public int cellHeight { get; set; }
        public int gridWidth { get; set; }
        public int gridHeight { get; set; }

        private int _cellReach;
        public int cellReach { get { return _cellReach; } set { if (value < 1) return; _cellReach = value; } }

        public List<Node>[,] grid;
        public HashSet<Node> alreadyVisited;

        

        public bool PolenterHack { get { return false; } 
            set 
            {
                grid = new List<Node>[cellsX, cellsY];
                for (int i = 0; i < cellsX; i++)
                {
                    for (int j = 0; j < cellsY; j++)
                    {
                        grid[i, j] = new List<Node>();
                    }
                }
            } 
        }

        public GridSystem() 
        {
            room = Program.getRoom();
            alreadyVisited = new HashSet<Node>();
        }

        public GridSystem(Room room, int cellsX, int cellReach = 4): this()
        {
            this.room = room;
            
            this.gridWidth = room.worldWidth;
            this.gridHeight = room.worldHeight;
            this.cellsX = cellsX;
            
            this.cellReach = cellReach;
            cellWidth = gridWidth / cellsX;
            cellHeight = cellWidth;
            this.cellsY = gridHeight / cellHeight;
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

        public GridSystem(Room room, int cellsX, int cellsY, int cellReach = 4)
        {
            this.room = room;
            this.gridWidth = room.worldWidth;
            this.gridHeight = room.worldHeight;
            this.cellsX = cellsX;
            this.cellsY = cellsY;

            this.cellReach = cellReach;
            cellWidth = gridWidth / cellsX;
            cellHeight = gridHeight / cellsY;
            alreadyVisited = new HashSet<Node>();
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

        //todo: save indexs of nodes upon insertion (duh.)
        // save 3d lookup table of retrieveBuckets lists (per x,y,reach)
        // 

        public void GenerateReachOffsets()
        {
            Console.WriteLine(" ::::" + cellsX + "," + cellsY + "\t");
            SortedDictionary<float, List<Tuple<int, int>>> distToCoords = new SortedDictionary<float, List<Tuple<int, int>>>();
            for (int x = 0; x < cellsX; x++)
            {
                for (int y = 0; y < cellsY; y++)
                {
                    int offsetX = cellsX - x - 1;
                    int offsetY = cellsY - y - 1;
                    float dist = (float)Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
                    if (!distToCoords.ContainsKey(dist)) distToCoords.Add(dist, new List<Tuple<int, int>>());
                    distToCoords[dist].Add(new Tuple<int, int>(x, y));
                }
            }
            foreach(var f in distToCoords.Keys)
            {
                Console.Write(f + "\t:\t");
                foreach(var t in distToCoords[f])
                {
                    Console.Write(t.Item1 + "," + t.Item2 + "\t");
                }
                Console.WriteLine();
            }
        }


        public void testRetrieve(int x, int y, int reach)
        {
            Console.WriteLine("Retrieve at: {0},{1}\tReach: {2}", x, y, reach);
            for (int i = 0; i < cellsX; i++)
            {
                for (int j = 0; j < cellsY; j++)
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
                        Console.Write(i + "," + j + "\t");
                    }

                }
            }
            Console.WriteLine();
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
            Vector2 pos = new Vector2(node.body.pos.X, node.body.pos.Y);
            int x = (int)node.body.pos.X;
            int y = (int)node.body.pos.Y;
            int gridx = (int)pos.X - ((int)pos.X % cellWidth);
            x = gridx / cellWidth;
            //if ((int)pos.X - gridx > gridx + cellwidth - (int)node.transform.radius) x++;
            int gridy = (int)pos.Y - ((int)pos.Y % cellHeight);
            y = gridy / cellHeight;
            //if ((int)pos.Y - gridy > gridy + cellheight - (int)node.transform.radius) y++;

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
                    //grid[i, j] = new List<Node>();
                    grid[i, j].RemoveRange(0, grid[i, j].Count);
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

        // color the nodes that targetnode is affecting
        public void colorEffectedNodes()
        {
            Game1 game = room.game;
            // coloring the nodes
            if (game.targetNode != null)
            {
                List<Node> returnObjectsGridSystem = retrieve(game.targetNode);

                foreach (Node _node in room.masterGroup.fullSet)
                {
                    if (_node.body.color != Color.Black)
                    {
                        if (returnObjectsGridSystem.Contains(_node))
                            _node.body.color = Color.Purple;
                        else
                            _node.body.color = Color.White;
                    }
                }
                game.targetNode.body.color = Color.Red;
            }
        }
    }
}
