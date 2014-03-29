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
        public HashSet<Collider> CollisionSetCircle { get; set; }
        public HashSet<Collider> CollisionSetPolygon { get; set; }
        //[Polenter.Serialization.ExcludeFromSerialization]
        public List<Rectangle> gridSystemLines = new List<Rectangle>(); //dns
        //[Polenter.Serialization.ExcludeFromSerialization]
        private List<Manifold> contacts = new List<Manifold>(); //dns
        #endregion
        public GridSystem gridsystemAffect { get; set; }
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
        public Group itemGroup
        {
            get
            {
                if (masterGroup == null) return null;
                return masterGroup.childGroups["Item Group"];
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
            CollisionSetCircle = new HashSet<Collider>();
            CollisionSetPolygon = new HashSet<Collider>();
            colIterations = 1;
            roomRenderTarget = new RenderTarget2D(game.GraphicsDevice, Game1.Width, Game1.Height);
            camera = new ThreadedCamera(this, 0.5f);
            scheduler = new Scheduler();
            borderColor = Color.Green;
            collideAction = (c1, c2) =>
            {
                
                if (c1.parent == c2.parent) return;
                if (c1 is Body)
                {
                    Body b = (Body)c1;
                    
                    if (gridsystemCollision.alreadyVisited.Contains(c2)) return;
                    if (c2 is Body)
                    {
                        Body bb = (Body)c2;
                        if (!b.exclusionList.Contains(bb)) b.CheckCollisionBody(bb);
                    }
                    else
                    {
                        b.CheckCollisionCollider(c2);
                    }
                }
                else
                {
                    if (gridsystemCollision.alreadyVisited.Contains(c2)) return;
                    if (c2 is Body)
                    {
                        Body bb = (Body)c2;
                        if (!c1.exclusionList.Contains(bb)) c1.CheckCollisionBody(bb);
                    }
                    else
                    {
                        //c.CheckCollision(c2);
                    }
                }
            };
        }
        Action<Collider, Collider> collideAction;
        public Room(Game1 game, int worldWidth, int worldHeight, bool Groups = true) : this()
        {
            //this.mapzoom = 2f;
            this.worldWidth = worldWidth;
            this.worldHeight = worldHeight;

            // grid System
            gridsystemAffect = new GridSystem(this, 40, 5);
            level = new Level(this, 40, 40, gridsystemAffect.cellWidth, gridsystemAffect.cellHeight);

            gridsystemCollision = new GridSystem(this, gridsystemAffect.cellsX, 20);
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
                Group itemGroup = new Group(defaultNode, masterGroup, Name: "Item Group", Spawnable: false);
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
            if (collider.shape is Circle)
                CollisionSetCircle.Add(collider);
            else if (collider.shape is Polygon)
                CollisionSetPolygon.Add(collider);
        }
        public void RemoveCollider(Collider collider)
        {
            if (CollisionSetCircle.Contains(collider))
                CollisionSetCircle.Remove(collider);
            if (CollisionSetPolygon.Contains(collider))
                CollisionSetPolygon.Remove(collider);
        }

        public void Update(GameTime gametime)
        {
            long elapsed = 0;
            if (gametime != null) elapsed = (long)Math.Round(gametime.ElapsedGameTime.TotalMilliseconds);
            totalElapsedMilliseconds += elapsed;
            
            gridsystemAffect.clear();
            gridSystemLines = new List<Rectangle>();

            game.processManager.Update();

            HashSet<Node> toDelete = new HashSet<Node>();
            //add all nodes from every group to the full hashset of nodes, and insert unique nodes into the gridsystem
            foreach (var n in masterGroup.childGroups["General Groups"].fullSet)
            {
                gridsystemAffect.insert(n.body);
            }
            Testing.OldStopTimer("gridsystem insert");
            //
            UpdateCollision();
            if (contacts.Count > 0) contacts = new List<Manifold>();

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
                foreach (var c in CollisionSetCircle) //.ToList()
                {
                    gridsystemCollision.insert(c);
                }
            }
            if (algorithm >= 5)
            {
                gridsystemCollision.clearBuckets();
                foreach (var n in CollisionSetCircle) //.ToList()
                {
                    //Console.WriteLine(CollisionSet.Count);
                    gridsystemCollision.insertToBuckets(n);
                }
            }

            Testing.PrintTimer("insertion");
            gridsystemCollision.alreadyVisited = new HashSet<Collider>();

            foreach (var c in CollisionSetPolygon.ToList())
            {
                if (c.parent.active)
                {
                    gridsystemCollision.alreadyVisited.Add(c);
                    int reach = (int)c.radius * 2;
                    if (c.shape is Circle)
                    {
                        reach = (int)(c.shape as Polygon).polyReach;
                    }
                    foreach (var otherCol in CollisionSetPolygon.ToList())
                    {
                        collideAction(c, otherCol);
                    }
                    foreach (var otherCol in CollisionSetCircle.ToList())
                    {
                        collideAction(c, otherCol);
                    }
                }
            }

            foreach (var c in CollisionSetCircle.ToList()) //.ToList() 
            {
                if (c.parent.active)
                {
                    gridsystemCollision.alreadyVisited.Add(c);
                    int reach = (int)c.radius * 2;
                    if (c.shape is Polygon)
                    {
                        reach = (int)(c.shape as Polygon).polyReach;
                        foreach (var otherCol in CollisionSetCircle.ToList())
                        {
                            collideAction(c, otherCol);
                        }
                    }
                    else if (algorithm == 5)
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
                    else if (algorithm == 6)
                    {
                        gridsystemCollision.retrieveFromAllOffsets(c, reach, collideAction);
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
                    //m.a.parent.SetColor(Color.Green);
                    //m.b.parent.SetColor(Color.Yellow);
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
            return;
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
            n.body.SetOrient(0);
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
