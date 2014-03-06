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
            GenerateAllReachOffsetsPerCoord(300);
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
            distToOffsets = GenerateReachOffsets();
            GenerateAllReachOffsetsPerCoord(300);
            bucketLists = new List<List<Node>>[cellsX, cellsY];
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
            distToOffsets = GenerateReachOffsets();
            GenerateAllReachOffsetsPerCoord(300);
            bucketLists = new List<List<Node>>[cellsX, cellsY];
        }



        static int largest = 0;
        public void insert(Node node)
        {
            Tuple<int, int> indexs = getIndexs(node);
            //if (node == room.game.targetNode) Console.WriteLine("target indexs: {0} {1}",indexs.Item1,indexs.Item2);
            grid[indexs.Item1, indexs.Item2].Add(node);
            if (grid[indexs.Item1, indexs.Item2].Count > largest)
            {
                largest = grid[indexs.Item1, indexs.Item2].Count;
                Console.WriteLine(largest);
            }
            //grid[indexs.Item1, indexs.Item2].ToArray();
        }

        //todo: save indexs of nodes upon insertion (duh.)
        // save 3d lookup table of retrieveBuckets lists (per x,y,reach)
        // 
        SortedDictionary<float, List<Tuple<int, int>>> distToOffsets;
        public SortedDictionary<float, List<Tuple<int, int>>> GenerateReachOffsets()
        {
            SortedDictionary<float, List<Tuple<int, int>>> offsets = new SortedDictionary<float, List<Tuple<int, int>>>();
            for (int x = 0; x < cellsX; x++)
            {
                for (int y = 0; y < cellsY; y++)
                {
                    float dist = (float)Math.Sqrt(x * x + y * y) * cellWidth;
                    if (!offsets.ContainsKey(dist)) offsets.Add(dist, new List<Tuple<int, int>>());

                    offsets[dist].Add(new Tuple<int, int>(x, y));

                    if (x == 0 && y > 0)
                    {
                        offsets[dist].Add(new Tuple<int, int>(x, -y));
                    }
                    else if (x > 0 && y == 0)
                    {
                        offsets[dist].Add(new Tuple<int, int>(-x, y));
                    }
                    else if (x > 0 && y > 0)
                    {
                        offsets[dist].Add(new Tuple<int, int>(-x, y));
                        offsets[dist].Add(new Tuple<int, int>(x, -y));
                        offsets[dist].Add(new Tuple<int, int>(-x, -y));
                    }
                }
            }
            return offsets;
        }
        public SortedDictionary<float, List<Tuple<int, int>>>[,] offsetsArray;

        public void GenerateAllReachOffsetsPerCoord(float maxdist = float.MaxValue)
        {
            //GC.GetTotalMemory(true);
            offsetsArray = new SortedDictionary<float, List<Tuple<int, int>>>[cellsX, cellsY];
            for (int x = 0; x < cellsX; x++)
            {
                for (int y = 0; y < cellsY; y++)
                {
                    offsetsArray[x,y] = new SortedDictionary<float, List<Tuple<int, int>>>();
                    foreach(float dist in distToOffsets.Keys)
                    {
                        if (dist > maxdist) break;
                        foreach(Tuple<int,int> tuple in distToOffsets[dist])
                        {
                            if (tuple.Item1 + x < 0 || tuple.Item1 + x >= cellsX || tuple.Item2 + y < 0 || tuple.Item2 + y >= cellsY) continue;
                            if (!offsetsArray[x, y].ContainsKey(dist)) offsetsArray[x, y][dist] = new List<Tuple<int, int>>();
                            offsetsArray[x, y][dist].Add(tuple);
                        }
                    }
                }
            }
        }
        public void retrieveFromAllOffsets(Node node, float reachDistance, Action<Node> action)
        {
            int x = (int)node.body.pos.X / cellWidth;
            int y = (int)node.body.pos.Y / cellHeight;
            if (x < 0 || x >= cellsX || y < 0 || y >= cellsY) return;
            for (int i = 0; i < FindCount(reachDistance); i++)
            {
                foreach(var tuple in distToOffsets.ElementAt(i).Value)
                {
                    foreach(Node n in grid[tuple.Item1, tuple.Item2])
                    {
                        action(n);
                    }
                }
            }
        }
        public List<List<Node>>[,] bucketLists;
        public int preexistingCounter = 0;
        public List<List<Node>> retrieveBuckets(Node node, float reachDistance)
        {
            int x = (int)node.body.pos.X / cellWidth;
            int y = (int)node.body.pos.Y / cellHeight;
            
            if (x < 0 || x >= cellsX || y < 0 || y >= cellsY)
            {
                return null;
            }
            else
            {
                if (bucketLists[x, y] != null)
                {
                    preexistingCounter++;
                    return bucketLists[x, y];
                }
                bucketLists[x, y] = new List<List<Node>>();

                int count = FindCount(reachDistance);
                var dict = offsetsArray[x, y];
                if (dict.Count <= count)
                {
                    throw new SystemException("Count too big exception");
                }
                //int cellsHit = 0;
                for (int i = 0; i < count; i++)
                {
                    List<Tuple<int, int>> tuples = dict.ElementAt(i).Value;
                    foreach (var tuple in tuples)
                    {
                        bucketLists[x, y].Add(grid[tuple.Item1 + x, tuple.Item2 + y]);
                    }
                }
                //Console.WriteLine(cellsHit);
                return bucketLists[x, y];
            }
        }

        public void retrieveFromOptimizedOffsets(Node node, float reachDistance, Action<Node> action)
        {
            int x = (int)node.body.pos.X / cellWidth;
            int y = (int)node.body.pos.Y / cellHeight;
            if (x < 0 || x >= cellsX || y < 0 || y >= cellsY) return;
            int count = FindCount(reachDistance);
            var dict = offsetsArray[x, y];
            if (dict.Count <= count)
            {
                throw new SystemException("Count too big exception");
            }
            //int cellsHit = 0;
            for (int i = 0; i < count; i++)
            {
                foreach (var tuple in dict.ElementAt(i).Value)
                {
                    foreach (Node n in grid[tuple.Item1 + x, tuple.Item2 + y])
                    {
                        action(n);
                    }
                    //cellsHit++;
                }
            }
            //Console.WriteLine(cellsHit);
        }

        public Dictionary<float, int> distToCount = new Dictionary<float, int>();
        public void retrieveNew(Node node, float reachDistance, Action<Node> action)
        {
            int x = (int)node.body.pos.X / cellWidth;
            int y = (int)node.body.pos.Y / cellHeight;
            if (x < 0 || x >= cellsX || y < 0 || y >= cellsY) return;

            foreach(float dist in distToOffsets.Keys)
            {
                if (dist > reachDistance) break;
                foreach(var tuple in distToOffsets[dist])
                {
                    foreach(Node n in grid[x + tuple.Item1, y + tuple.Item2])
                    {
                        action(n);
                        
                    }
                }
            }
        }

        public int FindCount(float dist)
        {
            if (distToCount.ContainsKey(dist)) return distToCount[dist];
            int count = 0;
            foreach(float f in distToOffsets.Keys)
            {
                count++;
                if (f > dist) return count;
            }
            return count;
        }


        public void PrintOffsets(int max = int.MaxValue, bool printOffsets = true, int x = -1, int y = -1)
        {
            var offsets = distToOffsets;
            if (x >= 0 && y >= 0)
            {
                offsets = offsetsArray[x, y];
            }
            Console.WriteLine(" ::::" + cellsX + "," + cellsY + "\t");
            int c = 0;
            foreach (var f in offsets.Keys)
            {
                if (c++ > max) break;
                Console.Write("{0,10} ", f);
                if (!printOffsets)
                {
                    Console.WriteLine();
                    continue;
                }
                foreach (var t in offsets[f])
                {
                    string s = t.Item1 + "," + t.Item2;
                    Console.Write("{0,10} ", s);
                }
                Console.WriteLine();
            }
        }

        

        // gets the index of the node in the gridsystem, without correcting out of bounds nodes.
        public Tuple<int, int> getIndexsNew(Node node)
        {
            //int a = (int)node.body.pos.X / cellWidth;
            return new Tuple<int, int>((int)node.body.pos.X / cellWidth, (int)node.body.pos.Y / cellHeight);
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
            //CountArray<Node>[,] nodes;

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
            //int cellsHit = 0;
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
                        //cellsHit++;
                    }

                }
                
            }
            //Console.WriteLine("Cells hit: {0}", cellsHit);
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
                    grid[i, j] = new List<Node>();
                    //grid[i, j].RemoveRange(0, grid[i, j].Count);
                    bucketLists[i, j] = null;
                }
            }
            //Console.WriteLine(preexistingCounter);
            preexistingCounter = 0;
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
