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

        public List<Collider>[,] grid;
        public HashSet<Collider> alreadyVisited;

        

        public bool PolenterHack { get { return false; } 
            set 
            {
                grid = new List<Collider>[cellsX, cellsY];
                for (int i = 0; i < cellsX; i++)
                {
                    for (int j = 0; j < cellsY; j++)
                    {
                        grid[i, j] = new List<Collider>();
                    }
                }
            } 
        }

        public GridSystem() 
        {
            room = OrbIt.game.room;
            alreadyVisited = new HashSet<Collider>();
            //GenerateAllReachOffsetsPerCoord(300);
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
            grid = new List<Collider>[cellsX, cellsY];
            for (int i = 0; i < cellsX; i++)
            {
                for (int j = 0; j < cellsY; j++)
                {
                    grid[i, j] = new List<Collider>();
                }
            }
            
            //
            arrayGrid = new IndexArray<Collider>[cellsX][];
            for(int i = 0; i < cellsX; i++)
            {
                arrayGrid[i] = new IndexArray<Collider>[cellsY];
                for(int j = 0; j < cellsY; j++)
                {
                    arrayGrid[i][j] = new IndexArray<Collider>(100);
                }
            }
            bucketBags = new IndexArray<IndexArray<Collider>>[cellsX][];
            for (int i = 0; i < cellsX; i++)
            {
                bucketBags[i] = new IndexArray<IndexArray<Collider>>[cellsY];
                for (int j = 0; j < cellsY; j++)
                {
                    bucketBags[i][j] = new IndexArray<IndexArray<Collider>>(20);
                }
            }
            //
            distToOffsets = GenerateReachOffsets();
            GenerateAllReachOffsetsPerCoord(300);
            bucketLists = new List<List<Collider>>[cellsX, cellsY];

        }
        public IndexArray<Collider>[][] arrayGrid;
        //public IndexArray<Node>[][][] bucketBags;
        public IndexArray<IndexArray<Collider>>[][] bucketBags;


        public IndexArray<IndexArray<Collider>> retrieveBucketBags(Collider collider)
        {
            int x = (int)collider.pos.X / cellWidth;
            int y = (int)collider.pos.Y / cellHeight;
            if (x < 0 || x >= cellsX || y < 0 || y >= cellsY) return null;
            return bucketBags[x][y];
        }
        public void insertToBuckets(Collider collider)
        {
            int x = (int)collider.pos.X / cellWidth;
            int y = (int)collider.pos.Y / cellHeight;
            if (x < 0 || x >= cellsX || y < 0 || y >= cellsY) return;
            arrayGrid[x][y].AddItem(collider);
        }
        public void clearBuckets()
        {
            for(int x = 0; x < cellsX; x++)
            {
                for(int y = 0; y < cellsY; y++)
                {
                    arrayGrid[x][y].index = 0;
                }
            }
        }


        static int largest = 0;
        public void insert(Collider collider)
        {
            Tuple<int, int> indexs = getIndexs(collider);
            //if (node == room.game.targetNode) Console.WriteLine("target indexs: {0} {1}",indexs.Item1,indexs.Item2);
            grid[indexs.Item1, indexs.Item2].Add(collider);
            if (grid[indexs.Item1, indexs.Item2].Count > largest)
            {
                largest = grid[indexs.Item1, indexs.Item2].Count;
                //Console.WriteLine(largest);
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
                            //add bucket to bucketBag
                            bucketBags[x][y].AddItem(arrayGrid[x + tuple.Item1][y + tuple.Item2]);
                        }
                    }
                    bucketBags[x][y].index = 15; //determins global reach
                }
            }
        }
        public void retrieveFromAllOffsets(Collider collider, float reachDistance, Action<Collider, Collider> action)
        {
            int x = (int)collider.pos.X / cellWidth;
            int y = (int)collider.pos.Y / cellHeight;
            if (x < 0 || x >= cellsX || y < 0 || y >= cellsY) return;
            int findcount = FindCount(reachDistance);
            for (int i = 0; i < findcount; i++)
            {
                foreach (var tuple in distToOffsets.ElementAt(i).Value)
                {
                    int xx = tuple.Item1 + x; int yy = tuple.Item2 + y;
                    if (xx < 0 || xx >= cellsX || yy < 0 || yy >= cellsY) continue;
                    //foreach (Collider c in grid[tuple.Item1, tuple.Item2])
                    //{
                    //    action(collider, c);
                    //}
                    IndexArray<Collider> buck = arrayGrid[xx][yy];
                    int count = buck.index;
                    //if (count > 0) Console.WriteLine(count);
                    for (int j = 0; j < count; j++)
                    {
                        Collider c = arrayGrid[xx][yy].array[j];
                        if (alreadyVisited.Contains(c) || collider == c) continue;
                        action(collider, c);
                    }
                }
            }
        }
        public List<List<Collider>>[,] bucketLists;
        public int preexistingCounter = 0;
        public List<List<Collider>> retrieveBuckets(Collider collider, float reachDistance)
        {
            int x = (int)collider.pos.X / cellWidth;
            int y = (int)collider.pos.Y / cellHeight;
            
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
                bucketLists[x, y] = new List<List<Collider>>();

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

        public void retrieveFromOptimizedOffsets(Collider collider, float reachDistance, Action<Node> action)
        {
            int x = (int)collider.pos.X / cellWidth;
            int y = (int)collider.pos.Y / cellHeight;
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
                    foreach (Collider c in grid[tuple.Item1 + x, tuple.Item2 + y])
                    {
                        action(c.parent);
                    }
                    //cellsHit++;
                }
            }
            //Console.WriteLine(cellsHit);
        }

        public Dictionary<float, int> distToCount = new Dictionary<float, int>();
        public void retrieveNew(Collider collider, float reachDistance, Action<Node> action)
        {
            int x = (int)collider.pos.X / cellWidth;
            int y = (int)collider.pos.Y / cellHeight;
            if (x < 0 || x >= cellsX || y < 0 || y >= cellsY) return;

            foreach(float dist in distToOffsets.Keys)
            {
                if (dist > reachDistance) break;
                foreach(var tuple in distToOffsets[dist])
                {
                    foreach (Collider c in grid[x + tuple.Item1, y + tuple.Item2])
                    {
                        action(c.parent);
                        
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
                if (f > dist)
                {
                    distToCount[dist] = count;
                    return count;
                }

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
        public Tuple<int, int> getIndexsNew(Collider collider)
        {
            //int a = (int)node.body.pos.X / cellWidth;
            return new Tuple<int, int>((int)collider.pos.X / cellWidth, (int)collider.pos.Y / cellHeight);
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

        public List<Collider> retrieve(Collider collider, int reach = -1)
        {
            //CountArray<Node>[,] nodes;

            if (reach == -1) reach = cellReach;
            List<Collider> returnList = new List<Collider>();
            Tuple<int, int> indexs = getIndexs(collider);
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

        public Tuple<int, int> getIndexs(Collider collider)
        {
            Vector2 pos = new Vector2(collider.pos.X, collider.pos.Y);
            int x = (int)collider.pos.X;
            int y = (int)collider.pos.Y;
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
                    grid[i, j] = new List<Collider>();
                    //grid[i, j].RemoveRange(0, grid[i, j].Count);
                    bucketLists[i, j] = null;
                }
            }
            //Console.WriteLine(preexistingCounter);
            preexistingCounter = 0;
        }

        public bool ContainsCollider(Collider collider)
        {
            for (int i = 0; i < cellsX; i++)
            {
                for (int j = 0; j < cellsY; j++)
                {
                    if (grid[i, j].Contains(collider)) return true;
                }
            }
            return false;
        }

        // color the nodes that targetnode is affecting
        public void colorEffectedNodes()
        {
            OrbIt game = room.game;
            // coloring the nodes
            if (room.targetNode != null)
            {
                List<Collider> returnObjectsGridSystem = retrieve(room.targetNode.body);

                foreach (Node _node in room.masterGroup.fullSet)
                {
                    if (_node.body.color != Color.Black)
                    {
                        if (returnObjectsGridSystem.Contains(_node.body))
                            _node.body.color = Color.Purple;
                        else
                            _node.body.color = Color.White;
                    }
                }
                room.targetNode.body.color = Color.Red;
            }
        }
    }
}
