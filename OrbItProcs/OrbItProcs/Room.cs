using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Component = OrbItProcs.Component;
using System.Collections.ObjectModel;

namespace OrbItProcs {

    public enum textures
    {
        blueorb,
        whiteorb,
        colororb,
        whitecircle,
        whitepixel,
        whitepixeltrans,
    }

    

    public class Room
    {
        public bool shit
        {
            get { //System.Diagnostics.Debugger.Break(); 
                return false; 
            }
            set
            {
                //System.Diagnostics.Debugger.Break();
                //throw new SystemException("Polenter.");
            }
        }
        

        #region // State // --------------------------------------------
        public bool DrawLinks { get; set; }
        public float WallWidth { get; set; }
        public float mapzoom { get; set; }
        public int worldWidth { get; set; }
        public int worldHeight { get; set; }
        #endregion

        #region // References // --------------------------------------------
        public Game1 game;
        public event EventHandler AfterIteration;
        #endregion

        #region // Lists // --------------------------------------------\
        [Polenter.Serialization.ExcludeFromSerialization]
        public HashSet<Node> CollisionSet { get; set; }
        public List<Rectangle> gridSystemLines = new List<Rectangle>(); //dns
        private List<Manifold> contacts = new List<Manifold>(); //dns
        #endregion
        public GridSystem gridsystem { get; set; }
        public GridSystem gridsystemCollision { get; set; }
        //[Polenter.Serialization.ExcludeFromSerialization]
        public Level level { get; set; }


        [Polenter.Serialization.ExcludeFromSerialization]
        public ObservableHashSet<string> groupHashes { get; set; }
        public ObservableHashSet<string> nodeHashes { get; set; }

        private Group _masterGroup;
        public Group masterGroup
        {
            get
            {
                return _masterGroup;
            }
            set
            {
                //System.Diagnostics.Debugger.Break();
                _masterGroup = value;
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

        public Node targetNodeGraphic { get; set; }

        [Polenter.Serialization.ExcludeFromSerialization]
        public Player player1 { get; set; }

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
            CollisionSet = new HashSet<Node>();
        }

        public Room(Game1 game, int worldWidth, int worldHeight) : this()
        {
            this.mapzoom = 2f;
            this.worldWidth = worldWidth;
            this.worldHeight = worldHeight;

            // grid System
            gridsystem = new GridSystem(this, 40, 5);
            level = new Level(this, 40, 40, gridsystem.cellWidth, gridsystem.cellHeight);

            gridsystemCollision = new GridSystem(this, gridsystem.cellsX, 20);
            DrawLinks = true;
            WallWidth = 500;


        }
        
        public void MakeWalls()
        {
            Dictionary<dynamic, dynamic> props = new Dictionary<dynamic, dynamic>() {
                    { node.position, new Vector2(0, 0) },
                    { comp.basicdraw, true },
                    { comp.collision, true },
                    //{ comp.movement, true },
            };
            Node left = ConstructWallPoly(props, (int)WallWidth / 2, worldHeight / 2, new Vector2(-WallWidth / 2, worldHeight / 2)); left.name = "left wall";
            Node right = ConstructWallPoly(props, (int)WallWidth / 2, worldHeight / 2, new Vector2(worldWidth + WallWidth / 2, worldHeight / 2)); right.name = "right wall";
            Node top = ConstructWallPoly(props, (worldWidth + (int)WallWidth * 2) / 2, (int)WallWidth / 2, new Vector2(worldWidth / 2, (int)-WallWidth / 2)); top.name = "top wall";
            Node bottom = ConstructWallPoly(props, (worldWidth + (int)WallWidth * 2) / 2, (int)WallWidth / 2, new Vector2(worldWidth / 2, worldHeight + (int)WallWidth / 2)); bottom.name = "bottom wall";
        }

        public Node ConstructWallPoly(Dictionary<dynamic, dynamic> props, int hw, int hh, Vector2 pos)
        {
            Node n = new Node(props);
            Polygon poly = new Polygon();
            poly.body = n.body;
            poly.body.pos = pos;
            poly.SetBox(hw,hh);
            //poly.SetOrient(0f);
            
            n.body.shape = poly;
            n.body.SetStatic();
            n.body.SetOrient(0);
            //n.body.restitution = 1f;

            //n.movement.pushable = false;

            masterGroup.childGroups["Walls"].entities.Add(n);
            return n;
        }

        public void Update(GameTime gametime)
        {
            if (contacts.Count > 0) contacts = new List<Manifold>();
            //these make it convienient to check values after pausing the game my mouseing over
            if (defaultNode == null) defaultNode = null;
            //if (game.ui.sidebar.lstComp == null) game.ui.sidebar.lstComp = null;

            ObservableHashSet<Node> hs = new ObservableHashSet<Node>();
            ICollection<Node> i = hs;

            gridsystem.clear();
            gridSystemLines = new List<Rectangle>();

            game.processManager.Update();

            HashSet<Node> toDelete = new HashSet<Node>();

            
            //add all nodes from every group to the full hashset of nodes, and insert unique nodes into the gridsystem
            masterGroup.childGroups["General Groups"].ForEachFullSet(delegate(Node n) 
            {
                gridsystem.insert(n);
            });

            //
            UpdateCollision();
            
            //game.testing.StartTimer();
            foreach(Node n in masterGroup.fullSet.ToList())
            {
                if (n.active)
                {
                    n.Update(gametime);
                }
            }
            //game.testing.StopTimer("room update");

            //masterGroup.ForEachThreading(gametime);
            
            
            if (AfterIteration != null) AfterIteration(this, null);

            //addGridSystemLines(gridsystem);
            //addLevelLines(level);
            addBorderLines();
            //colorEffectedNodes();

            updateTargetNodeGraphic();

            //player1.Update(gametime);
        }

        public void AddManifold(Manifold m)
        {
            contacts.Add(m);
        }

        public void UpdateCollision()
        {
            gridsystemCollision.clear();
            CollisionSet.ToList().ForEach(delegate(Node n) { gridsystemCollision.insert(n); });
            gridsystemCollision.alreadyVisited = new HashSet<Node>();
            CollisionSet.ToList().ForEach(delegate(Node n)
            {
                if (n.active)
                {
                    //int reach = 5; //update later based on cell size and radius (or polygon size.. maybe based on it's AABB)
                    List<Node> retrievedNodes = gridsystemCollision.retrieve(n);
                    gridsystemCollision.alreadyVisited.Add(n);
                    retrievedNodes.ForEach(delegate(Node r)
                    {
                        if (gridsystemCollision.alreadyVisited.Contains(r))
                            return;
                        n.collision.AffectOther(r);
                    });
                }
            });

            //COLLISION
            foreach (Manifold m in contacts)
                m.Initialize();
            int iterations = 10;
            for (int ii = 0; ii < iterations; ii++)
            {
                foreach (Manifold m in contacts)
                {
                    
                    m.ApplyImpulse();
                }
            }
            foreach (Node n in masterGroup.fullSet)
            {
                if (n.movement.active)
                {
                    n.movement.IntegrateVelocity();
                }
                VMath.Set(ref n.body.force, 0, 0);
                n.body.torque = 0;
            }
            foreach (Manifold m in contacts)
                m.PositionalCorrection();
            // \COLLISION
        }

        public void updateTargetNodeGraphic()
        {
            if (game.targetNode != null)
            {
                targetNodeGraphic.body.color = Color.White;
                targetNodeGraphic.body.pos = game.targetNode.body.pos;
                //if (game.targetNode.comps.ContainsKey(comp.gravity))
                //{
                //    float rad = game.targetNode.GetComponent<Gravity>().radius;
                //    targetNodeGraphic.transform.radius = rad;
                //}
                targetNodeGraphic.body.scale = game.targetNode.body.scale * 1.5f;
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

        public void Draw(SpriteBatch spritebatch)
        {
            //spritebatch.Draw(game.textureDict[textures.whitepixel], new Vector2(300, 300), null, Color.Black, 0f, Vector2.Zero, 100f, SpriteEffects.None, 0);

            if (game.targetNode != null)
            {
                updateTargetNodeGraphic();
                targetNodeGraphic.Draw(spritebatch);
            }
            HashSet<Node> groupset = (game.processManager.processDict[proc.groupselect] as GroupSelect).groupSelectSet;
            if (groupset != null)
            {
                targetNodeGraphic.body.color = Color.LimeGreen;
                foreach (Node n in groupset.ToList())
                {
                    targetNodeGraphic.body.pos = n.body.pos;
                    targetNodeGraphic.body.scale = n.body.scale * 1.5f;
                    targetNodeGraphic.Draw(spritebatch);
                }
            }

            masterGroup.ForEachFullSet(delegate(Node n)
            {
                //Node n = (Node)o;
                n.Draw(spritebatch);
            });
            int linecount = 0;

            if (DrawLinks)
            {
                foreach (Link link in AllActiveLinks)
                {
                    link.GenericDraw(spritebatch);
                }
            }
            //if (linkTest != null) linkTest.GenericDraw(spritebatch);

            foreach (Rectangle rect in gridSystemLines)
            {
                //float scale = 1 / mapzoom;
                Rectangle maprect = new Rectangle((int)(rect.X / mapzoom), (int)(rect.Y / mapzoom), (int)(rect.Width / mapzoom), (int)(rect.Height / mapzoom));
                spritebatch.DrawLine(new Vector2(maprect.X, maprect.Y), new Vector2(maprect.Width, maprect.Height), Color.Green, 2);
                
                linecount++;
            }

            //player1.Draw(spritebatch);
            //level.Draw(spritebatch);

            game.processManager.Draw(spritebatch);
            
        } 
        
        public void tether()
        {
            Group g1 = masterGroup.FindGroup(game.ui.sidebar.cbListPicker.SelectedItem());
            g1.defaultNode.comps[comp.tether].methods = mtypes.affectother | mtypes.draw;
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
