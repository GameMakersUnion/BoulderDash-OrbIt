using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Component = OrbItProcs.Component;
using System.Collections.ObjectModel;
using OrbItProcs;

namespace OrbItProcs {

    public class Room
    {
        #region // State // --------------------------------------------
        public bool DrawLinks { get; set; }
        public float WallWidth { get; set; }
        
        public int worldWidth { get; set; }
        public int worldHeight { get; set; }
        public int colIterations { get; set; }
        #endregion

        #region // References // --------------------------------------------
        public Game1 game;
        public event EventHandler AfterIteration;
        #endregion

        #region // Lists // --------------------------------------------\
        [Polenter.Serialization.ExcludeFromSerialization]
        public HashSet<Collider> CollisionSet { get; set; }
        //[Polenter.Serialization.ExcludeFromSerialization]
        public List<Rectangle> gridSystemLines = new List<Rectangle>(); //dns
        //[Polenter.Serialization.ExcludeFromSerialization]
        private List<Manifold> contacts = new List<Manifold>(); //dns
        #endregion
        public GridSystem gridsystem { get; set; }
        public GridSystem gridsystemCollision { get; set; }
        //[Polenter.Serialization.ExcludeFromSerialization]
        public Level level { get; set; }


        [Polenter.Serialization.ExcludeFromSerialization]
        public ObservableHashSet<string> groupHashes { get; set; }
        public ObservableHashSet<string> nodeHashes { get; set; }

        public int timertimer = 0;
        public int timermax = 60;

        public static long totalElapsedMilliseconds = 0;
        public RenderTarget2D roomRenderTarget;
        
        public ThreadedCamera camera;

        private Group _masterGroup;
        public Group masterGroup
        {
            get
            {
                return _masterGroup;
            }
            set
            {
                _masterGroup = value;
            }
        }
        [Polenter.Serialization.ExcludeFromSerialization]
        public Group generalGroups
        {
            get
            {
                if (masterGroup == null) return null;
                return masterGroup.childGroups["General Groups"];
            }
        }
        [Polenter.Serialization.ExcludeFromSerialization]
        public Group playerGroup
        {
            get
            {
                if (masterGroup == null) return null;
                return masterGroup.childGroups["Player Group"];
            }
        }

        [Polenter.Serialization.ExcludeFromSerialization]
        public Node defaultNode { get; set; }
        public string defaultNodeHash
        {
            get { if (defaultNode == null) return ""; else return defaultNode.nodeHash; }
            set
            {
                defaultNode = masterGroup.FindNodeByHash(value);
            }
        }

        public Node targetNodeGraphic = null;
        public Node targetNode { get; set; }

        [Polenter.Serialization.ExcludeFromSerialization]
        //public Player player1 { get; set; }
        public HashSet<Player> players { get; set; }
        [Info(UserLevel.Never)]
        public IEnumerable<Node> playerNodes { get { return players.Select(p => p.node); } }

        [Polenter.Serialization.ExcludeFromSerialization]
        public Scheduler scheduler { get; set; }

        public float zoom { get { return camera.zoom; } set { camera.zoom = value; } }

        public Color borderColor { get; set; }

        #region // Links // ------------------------------------------------------
        public ObservableHashSet<Link> _AllActiveLinks = new ObservableHashSet<Link>();
        public ObservableHashSet<Link> AllActiveLinks { get { return _AllActiveLinks; } set { _AllActiveLinks = value; } }

        public ObservableHashSet<Link> _AllInactiveLinks = new ObservableHashSet<Link>();
        public ObservableHashSet<Link> AllInactiveLinks { get { return _AllInactiveLinks; } set { _AllInactiveLinks = value; } }
        #endregion

        public Room()
        {
            Program.room = this;
            game = Program.getGame();
            groupHashes = new ObservableHashSet<string>();
            nodeHashes = new ObservableHashSet<string>();
            CollisionSet = new HashSet<Collider>();
            colIterations = 1;
            roomRenderTarget = new RenderTarget2D(game.GraphicsDevice, game.Width, game.Height);
            camera = new ThreadedCamera(this, 0.5f);
            scheduler = new Scheduler();
            borderColor = Color.Green;
            
        }

        public Room(Game1 game, int worldWidth, int worldHeight, bool Groups = true) : this()
        {
            //this.mapzoom = 2f;
            this.worldWidth = worldWidth;
            this.worldHeight = worldHeight;

            // grid System
            gridsystem = new GridSystem(this, 40, 5);
            level = new Level(this, 40, 40, gridsystem.cellWidth, gridsystem.cellHeight);

            gridsystemCollision = new GridSystem(this, gridsystem.cellsX, 20);
            DrawLinks = true;
            WallWidth = 10;
            camera = new ThreadedCamera(this, 0.5f);
            scheduler = new Scheduler();

            players = new HashSet<Player>();

            #region ///Default User props///
            Dictionary<dynamic, dynamic> userPr = new Dictionary<dynamic, dynamic>() {
                { nodeE.position, new Vector2(0, 0) },
                { nodeE.texture, textures.whitecircle },
                { comp.basicdraw, true },
                { comp.collision, true },
                { comp.movement, true },
            };
            #endregion


            defaultNode = new Node(userPr);
            defaultNode.name = "master";
            //defaultNode.IsDefault = true;

            defaultNode.comps.Keys.ToList().ForEach(delegate(Type c)
            {
                defaultNode.comps[c].AfterCloning();
            });

            Node firstdefault = new Node();
            Node.cloneNode(defaultNode, firstdefault);
            firstdefault.name = "[G0]0";
            //firstdefault.IsDefault = true;

            masterGroup = new Group(defaultNode, Name: defaultNode.name, Spawnable: false);

            if (Groups)
            {
                Group playerGroup = new Group(defaultNode, masterGroup, Name: "Player Group", Spawnable: false);

                Group generalGroup = new Group(defaultNode, masterGroup, Name: "General Groups", Spawnable: false);

                Group linkGroup = new Group(defaultNode, masterGroup, Name: "Link Groups", Spawnable: false);

                Group wallGroup = new Group(defaultNode, masterGroup, Name: "Walls", Spawnable: false);

                Group firstGroup = new Group(firstdefault, generalGroup, Name: "Group1");
            }

            Dictionary<dynamic, dynamic> userPropsTarget = new Dictionary<dynamic, dynamic>() {
                    { comp.basicdraw, true }, { nodeE.texture, textures.whitecircle } };

            targetNodeGraphic = new Node(userPropsTarget);
            targetNodeGraphic.name = "TargetNodeGraphic";

            MakeWalls();
        }
        
        public void AddCollider(Collider collider)
        {
            CollisionSet.Add(collider);
        }
        public void RemoveCollider(Collider collider)
        {
            CollisionSet.Remove(collider);
        }

        public void Update(GameTime gametime)
        {
            //Testing.StandardizedTesting(700);
            //Testing.StandardizedTesting2(200);
            
            //Console.WriteLine(gametime.ElapsedGameTime.Milliseconds + " :: " + gametime.ElapsedGameTime.TotalMilliseconds);
            long elapsed = 0;
            if (gametime != null) elapsed = (long)Math.Round(gametime.ElapsedGameTime.TotalMilliseconds);
            totalElapsedMilliseconds += elapsed;
            //these make it convienient to check values after pausing the game my mouseing over
            //if (defaultNode == null) defaultNode = null;
            //if (game.ui.sidebar.lstComp == null) game.ui.sidebar.lstComp = null;
            
            gridsystem.clear();
            gridSystemLines = new List<Rectangle>();

            game.processManager.Update();

            HashSet<Node> toDelete = new HashSet<Node>();

            //if (++timertimer % timermax == 0)
            //    Console.WriteLine("=======================UPDATE START==========");
            //if (timertimer % timermax == 0)
            //    Testing.StartTimer();
            //add all nodes from every group to the full hashset of nodes, and insert unique nodes into the gridsystem
            foreach (var n in masterGroup.childGroups["General Groups"].fullSet)
            {
                gridsystem.insert(n.body);
            }
            Testing.OldStopTimer("gridsystem insert");
            //
            UpdateCollision();
            if (contacts.Count > 0) contacts = new List<Manifold>();
            

            //if (timertimer % timermax == 0)
            //    Testing.StartTimer();
            foreach(Node n in masterGroup.fullSet.ToList())
            {
                if (n.active)
                {
                    n.Update(gametime);
                }
            }
            Testing.OldStopTimer("node update");

            
            
            if (AfterIteration != null) AfterIteration(this, null);


            //addGridSystemLines(gridsystem);
            //addLevelLines(level);
            addBorderLines();
            //colorEffectedNodes();

            updateTargetNodeGraphic();

            //player1.Update(gametime);
            //if (Game1.bigTonyOn)
            //{
            //    foreach (var player in players)
            //    {
            //        player.Update(gametime); //#bigtony
            //    }
            //}

            scheduler.AffectSelf();
        }
        static int algorithm = 5;
        public void UpdateCollision()
        {
            Testing.modInc();
            //Testing.w("insertion").Start();
            if (algorithm <= 4)
            {
                gridsystemCollision.clear();
                foreach (var c in CollisionSet) //.ToList()
                {
                    gridsystemCollision.insert(c);
                }
            }
            if (algorithm == 5)
            {
                gridsystemCollision.clearBuckets();
                foreach (var n in CollisionSet) //.ToList()
                {
                    //Console.WriteLine(CollisionSet.Count);
                    gridsystemCollision.insertToBuckets(n);
                }
            }

            Testing.PrintTimer("insertion");
            gridsystemCollision.alreadyVisited = new HashSet<Collider>();

            //todo: remove tolists if possible
            foreach (var c in CollisionSet.ToList()) //.ToList() 
            {
                if (c.parent.active)
                {
                    int reach; //update later based on cell size and radius (or polygon size.. maybe based on it's AABB)
                    if (c.shape is Polygon)
                    {
                        reach = 20;
                    }
                    else
                    {
                        reach = (int)(c.radius * 5) / gridsystemCollision.cellWidth;
                        //reach = 2;
                    }
                    gridsystemCollision.alreadyVisited.Add(c);
                    //if (algorithm == 1)
                    //{
                    //    ///*
                    //    //Testing.w("retrieve").Start();
                    //    List<Collider> retrievedCollider = gridsystemCollision.retrieve(c, reach);
                    //    //Testing.w("retrieve").Stop();
                    //    //Testing.w("manifolds").Start();
                    //    foreach (var r in retrievedCollider) //todo: this may be iterating over a deleted node (or removed)
                    //    {
                    //        if (gridsystemCollision.alreadyVisited.Contains(r))
                    //            continue;
                    //        //n.collision.AffectOther(r);
                    //        c.CheckCollisionCollider(r);
                    //    }
                    //    //Testing.w("manifolds").Stop();
                    //}
                    //else if (algorithm == 4)
                    //{
                    //    ///*
                    //    var buckets = gridsystemCollision.retrieveBuckets(c, 115);
                    //    if (buckets != null)
                    //    {
                    //        foreach (var bucket in buckets)
                    //        {
                    //            foreach (var nn in bucket)
                    //            {
                    //                if (gridsystemCollision.alreadyVisited.Contains(nn))
                    //                    continue;
                    //                c.CheckCollisionCollider(nn);
                    //            }
                    //        }
                    //    }
                    //    //*/
                    //}
                    if (algorithm == 5)
                    {
                        var bucketBag = gridsystemCollision.retrieveBucketBags(c);
                        if (bucketBag != null)
                        {
                            if (c is Body)
                            {
                                Body b = (Body)c;
                                for (int i = 0; i < bucketBag.index; i++)
                                {
                                    for (int j = 0; j < bucketBag.array[i].index; j++)
                                    {
                                        Collider cc = bucketBag.array[i].array[j];
                                        if (cc.parent == b.parent) continue;
                                        if (gridsystemCollision.alreadyVisited.Contains(cc))
                                        continue;
                                        if (cc is Body)
                                        {
                                            Body bb = (Body)cc;
                                            if (!b.exclusionList.Contains(bb)) b.CheckCollisionBody(bb);
                                            
                                        }
                                        else
                                        {
                                            b.CheckCollisionCollider(cc);
                                        }
                                    }
                                }
                            }
                            else
                            {
                            for (int i = 0; i < bucketBag.index; i++)
                            {
                                for (int j = 0; j < bucketBag.array[i].index; j++)
                                {
                                        Collider cc = bucketBag.array[i].array[j];
                                        if (cc.parent == c.parent) continue;
                                        if (gridsystemCollision.alreadyVisited.Contains(cc))
                                        continue;
                                        if (cc is Body)
                                        {
                                            Body bb = (Body)cc;
                                            if (!c.exclusionList.Contains(bb)) c.CheckCollisionBody(bb);
                                        }
                                        else
                                        {
                                            //c.CheckCollision(cc);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //Testing.PrintTimer("insertion");
            //Testing.PrintTimer("retrieve");
            //Testing.PrintTimer("manifolds");
            //COLLISION
            foreach (Manifold m in contacts)
            {
                m.Initialize();
            }
            // \COLLISION
            //update velocity
            for (int ii = 0; ii < colIterations; ii++)
            {
                foreach (Manifold m in contacts)
                {
                    m.ApplyImpulse();
                }
            }
            foreach (Node n in masterGroup.fullSet.ToList())
            {
                n.movement.IntegrateVelocity();

                VMath.Set(ref n.body.force, 0, 0);
                n.body.torque = 0;
            }
            foreach (Manifold m in contacts)
                m.PositionalCorrection();
        }

        public void Draw()
        {
            //spritebatch.Draw(game.textureDict[textures.whitepixel], new Vector2(300, 300), null, Color.Black, 0f, Vector2.Zero, 100f, SpriteEffects.None, 0);
            
            if (targetNode != null)
            {
                updateTargetNodeGraphic();
                targetNodeGraphic.Draw();
            }
            HashSet<Node> groupset = (game.processManager.processDict[proc.groupselect] as GroupSelect).groupSelectSet;
            if (groupset != null)
            {
                targetNodeGraphic.body.color = Color.LimeGreen;
                foreach (Node n in groupset.ToList())
                {
                    targetNodeGraphic.body.pos = n.body.pos;
                    targetNodeGraphic.body.scale = n.body.scale * 1.5f;
                    targetNodeGraphic.Draw();
                }
            }
            foreach(var n in masterGroup.fullSet)
            {
                //Node n = (Node)o;
                n.Draw();
            }
            int linecount = 0;

            if (DrawLinks)
            {
                foreach (Link link in AllActiveLinks)
                {
                    link.GenericDraw();
                }
            }
            //if (linkTest != null) linkTest.GenericDraw(spritebatch);

            foreach (Rectangle rect in gridSystemLines)
            {
                //float scale = 1 / mapzoom;
                Rectangle maprect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
                //spritebatch.DrawLine((new Vector2(maprect.X, maprect.Y) - camera.pos) * zoom, (new Vector2(maprect.Width, maprect.Height) - camera.pos) * zoom, Color.Green, 2);
                Utils.DrawLine(this, new Vector2(maprect.X, maprect.Y), new Vector2(maprect.Width, maprect.Height), 2, borderColor);
                linecount++;
            }

            //player1.Draw(spritebatch);
            //level.Draw(spritebatch);

            game.processManager.Draw();

            GraphData.DrawGraph();
            //Testing.TestHues();
        }
        public void AddManifold(Manifold m)
        {
            contacts.Add(m);
        }

        public void MakeWalls()
        {
            Dictionary<dynamic, dynamic> props = new Dictionary<dynamic, dynamic>() {
                    { nodeE.position, new Vector2(0, 0) },
                    { comp.basicdraw, true },
                    { comp.collision, true },
                    //{ comp.movement, true },
            };
            Node left = ConstructWallPoly(props, (int)WallWidth / 2, worldHeight / 2, new Vector2(WallWidth / 2, worldHeight / 2)); left.name = "left wall";
            Node right = ConstructWallPoly(props, (int)WallWidth / 2, worldHeight / 2, new Vector2(worldWidth - WallWidth / 2, worldHeight / 2)); right.name = "right wall";
            Node top = ConstructWallPoly(props, (worldWidth + (int)WallWidth * 2) / 2, (int)WallWidth / 2, new Vector2(worldWidth / 2, (int)WallWidth / 2)); top.name = "top wall";
            Node bottom = ConstructWallPoly(props, (worldWidth + (int)WallWidth * 2) / 2, (int)WallWidth / 2, new Vector2(worldWidth / 2, worldHeight - WallWidth / 2)); bottom.name = "bottom wall";
        }

        public Node ConstructWallPoly(Dictionary<dynamic, dynamic> props, int hw, int hh, Vector2 pos)
        {
            Node n = new Node(props);
            n[comp.basicdraw].active = false;
            Polygon poly = new Polygon();
            poly.body = n.body;
            poly.body.pos = pos;
            poly.SetBox(hw, hh);
            //poly.SetOrient(0f);

            n.body.shape = poly;
            n.body.SetStatic();
            n.body.orient =(0);
            //n.body.restitution = 1f;

            //n.movement.pushable = false;

            masterGroup.childGroups["Walls"].entities.Add(n);
            return n;
        }

        public void updateTargetNodeGraphic()
        {
            if (targetNode != null)
            {
                targetNodeGraphic.body.color = Color.White;
                targetNodeGraphic.body.pos = targetNode.body.pos;
                //if (game.targetNode.comps.ContainsKey(comp.gravity))
                //{
                //    float rad = game.targetNode.GetComponent<Gravity>().radius;
                //    targetNodeGraphic.transform.radius = rad;
                //}
                targetNodeGraphic.body.scale = targetNode.body.scale * 1.5f;
            }
            
        }
        //draw grid lines
        public void addGridSystemLines(GridSystem gs)
        {
            for (int i = 0; i <= gs.cellsX; i++)
            {
                int x = i * gs.cellWidth;
                gridSystemLines.Add(new Rectangle(x, 0, x, worldHeight));
            }
            for (int i = 0; i <= gs.cellsY; i++)
            {
                int y = i * gs.cellHeight;
                gridSystemLines.Add(new Rectangle(0, y, worldWidth, y));
            }
        }

        public void addLevelLines(Level lev)
        {
            for (int i = 0; i <= lev.cellsX; i++)
            {
                int x = i * lev.cellWidth;
                gridSystemLines.Add(new Rectangle(x, 0, x, worldHeight));
            }
            for (int i = 0; i <= lev.cellsY; i++)
            {
                int y = i * lev.cellHeight;
                gridSystemLines.Add(new Rectangle(0, y, worldWidth, y));
            }
        }

        public void addBorderLines()
        {
            gridSystemLines.Add(new Rectangle(0, 0, worldWidth, 0));
            gridSystemLines.Add(new Rectangle(0, 0, 0, worldHeight));
            gridSystemLines.Add(new Rectangle(0, worldHeight, worldWidth, worldHeight));
            gridSystemLines.Add(new Rectangle(worldWidth, 0, worldWidth, worldHeight));
        }

        public void addRectangleLines(int x, int y, int width, int height)
        {
            gridSystemLines.Add(new Rectangle(x, y, width, y));
            gridSystemLines.Add(new Rectangle(x, y, x, height));
            gridSystemLines.Add(new Rectangle(x, height, width, height));
            gridSystemLines.Add(new Rectangle(width, y, width, height));
        }
        public void addRectangleLines(float x, float y, float width, float height)
        {
            addRectangleLines((int)x, (int)y, (int)width, (int)height);
        }

        
        
        public void tether()
        {
            Group g1 = masterGroup.FindGroup(game.ui.sidebar.cbListPicker.SelectedItem());
            g1.defaultNode.Comp<Tether>().compType = mtypes.affectother | mtypes.draw;
        }

        public void hide()
        {
            //game.ui.sidebar.lstComp.Visible = false;
            
        }
        public void show()
        {
            //game.ui.sidebar.lstComp.Visible = true;
        }
        public Group findGroupByHash(string value)
        {
            if (masterGroup == null) return null;
            return findGroupByHashRecurse(masterGroup, value);
        }

        private Group findGroupByHashRecurse(Group g, string value)
        {
            if (g.groupHash.Equals(value)) return g;
            foreach(Group child in g.childGroups.Values)
            {
                Group ret = findGroupByHashRecurse(child, value);
                if (ret != null) return ret;
            }
            return null;
        }
    }
}
