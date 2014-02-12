using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polenter.Serialization;




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

    

    public class Room {


        public Link linkTest { get; set; }
        public bool DrawLinks { get; set; }



        public ObservableHashSet<Link> _AllActiveLinks = new ObservableHashSet<Link>();
        public ObservableHashSet<Link> AllActiveLinks { get { return _AllActiveLinks; } set { _AllActiveLinks = value; } }

        public event EventHandler AfterIteration;
        
        public Game1 game;
        public ProcessManager processManager { get; set; }
        public float mapzoom { get; set; }

        public GridSystem gridsystem { get; set; }
        public GridSystem gridsystemCollision { get; set; }
        public HashSet<Node> CollisionSet { get; set; }
        public List<Rectangle> gridSystemLines;
        public int gridSystemCounter = 0;

        public Dictionary<string, bool> PropertiesDict = new Dictionary<string, bool>();

        //public Dictionary<string, Group> groups = new Dictionary<string, Group>();
        public Group masterGroup;

        public ObservableCollection<object> nodes = new ObservableCollection<object>();

        //public Queue<object> nodesToAdd = new Queue<object>();

        public Node defaultNode, targetNodeGraphic;
        //public NodeList<Node> nodes = new NodeList<Node>();
        public int dtimerMax = -1, dtimerCount = 0;

        public SharpSerializer serializer = new SharpSerializer();

        public Player player1 { get; set; }

        private List<Manifold> contacts = new List<Manifold>();

        //public tree treeProp = tree.gridsystem;

        public Room()
        {
            setDefaultProperties();
        }

        public Room(Game1 game)
        {
            setDefaultProperties();

            
            
            this.game = game;
            this.mapzoom = 2f;

            // grid System
            gridsystem = new GridSystem(this, 40, 15);

            gridsystemCollision = new GridSystem(this, gridsystem.cellsX, 5);
            CollisionSet = new HashSet<Node>();
            gridSystemLines = new List<Rectangle>();
            DrawLinks = true;
            
            
        }
        /*
        public void RemoveAllNodes()
        {
            int c = nodes.Count;
            
            for (int i = 0; i < c; i++)
            {
                ((Node)nodes.ElementAt(0)).active = false;
                nodes.Remove(nodes.ElementAt(0));
            }
            //game.ui.sidebar.UpdateNodesTitle();
        }
        */
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

            processManager.Update();

            HashSet<Node> toDelete = new HashSet<Node>();

            
            //add all nodes from every group to the full hashset of nodes, and insert unique nodes into the gridsystem
            masterGroup.childGroups["General Groups"].ForEachFullSet(delegate(Node o) 
            {
                Node n = (Node)o; 
                gridsystem.insert(n);
            });

            //UpdateCollision();
            
            //game.testing.StartTimer();
            masterGroup.ForEachFullSet(delegate(Node o)
            {
                Node n = (Node)o;
                if (n.active)
                {
                    n.Update(gametime);
                }
                if (n.IsDeleted)
                {
                    toDelete.Add(n);
                    if (n == game.targetNode) game.targetNode = null;
                    if (n == game.ui.sidebar.inspectorArea.editNode) game.ui.sidebar.inspectorArea.editNode = null;
                    if (n == game.ui.spawnerNode) game.ui.spawnerNode = null;
                }
            });
            //game.testing.StopTimer("room update");


            toDelete.ToList().ForEach(delegate(Node n) 
            {
                if (masterGroup.fullSet.Contains(n)) //masterGroup.entities.Remove(n);
                {
                    masterGroup.DeleteEntity(n);
                }
                /*
                groups.Keys.ToList().ForEach(delegate(string key)
                {
                    Group g = groups[key];
                    if (g.entities.Contains(n)) g.entities.Remove(n);
                });
                */
            });

            UpdateCollision();
            
            if (AfterIteration != null) AfterIteration(this, null);

            //addGridSystemLines(gridsystem);
            addBorderLines();
            //colorEffectedNodes();

            updateTargetNodeGraphic();

            player1.Update(gametime);
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
            CollisionSet.ToList().ForEach(delegate(Node o)
            {
                Node n = (Node)o;
                if (n.active)
                {
                    int reach = 12; //update later based on cell size and radius (or polygon size.. maybe based on it's AABB)
                    List<Node> retrievedNodes = gridsystemCollision.retrieve(n, reach);
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
                    m.ApplyImpulse();
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
                targetNodeGraphic.body.position = game.targetNode.body.position;
                //if (game.targetNode.comps.ContainsKey(comp.gravity))
                //{
                //    float rad = game.targetNode.GetComponent<Gravity>().radius;
                //    targetNodeGraphic.transform.radius = rad;
                //}
                targetNodeGraphic.body.scale = game.targetNode.body.scale * 1.5f;
            }
            
        }

        // color the nodes that targetnode is affecting
        public void colorEffectedNodes()
        {
            // coloring the nodes
            if (game.targetNode != null)
            {
                List<Node> returnObjectsGridSystem = gridsystem.retrieve(game.targetNode);

                foreach (Node _node in nodes)
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

        //draw grid lines
        public void addGridSystemLines(GridSystem gs)
        {
            for (int i = 0; i <= gs.cellsX; i++)
            {
                int x = i * gs.cellwidth;
                gridSystemLines.Add(new Rectangle(x, 0, x, game.worldHeight));
            }
            for (int i = 0; i <= gs.cellsY; i++)
            {
                int y = i * gs.cellheight;
                gridSystemLines.Add(new Rectangle(0, y, game.worldWidth, y));
            }
        }

        public void addBorderLines()
        {
            gridSystemLines.Add(new Rectangle(0, 0, game.worldWidth, 0));
            gridSystemLines.Add(new Rectangle(0, 0, 0, game.worldHeight));
            gridSystemLines.Add(new Rectangle(0, game.worldHeight, game.worldWidth, game.worldHeight));
            gridSystemLines.Add(new Rectangle(game.worldWidth, 0, game.worldWidth, game.worldHeight));
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
            HashSet<Node> groupset = (processManager.processDict[proc.groupselect] as GroupSelect).groupSelectSet;
            if (groupset != null)
            {
                targetNodeGraphic.body.color = Color.LimeGreen;
                foreach (Node n in groupset.ToList())
                {
                    targetNodeGraphic.body.position = n.body.position;
                    targetNodeGraphic.body.scale = n.body.scale * 1.5f;
                    targetNodeGraphic.Draw(spritebatch);
                }
            }

            masterGroup.ForEachFullSet(delegate(Node o)
            {
                Node n = (Node)o;
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

            player1.Draw(spritebatch);
            
        } 

        public void setDefaultProperties()
        {
            //mapzoom = 2f;

            PropertiesDict.Add("wallBounce", true);
            PropertiesDict.Add("gravityFreeze", false);
            PropertiesDict.Add("tempSlow", true);
            PropertiesDict.Add("mapOn", true);
            PropertiesDict.Add("collisionOn", true);
            PropertiesDict.Add("discoOn", false);
            PropertiesDict.Add("fixCollisionOn", true);
            PropertiesDict.Add("fullLightOn", false);
            PropertiesDict.Add("smallLightsOn", false);
            PropertiesDict.Add("bulletsOn", false);
            PropertiesDict.Add("enemiesOn", false);
            PropertiesDict.Add("friction", false);
        }

        public void makelink()
        {
            Node n1 = masterGroup.fullSet.ElementAt(0);
            Node n2 = masterGroup.fullSet.ElementAt(1);
            Tether t1 = new Tether();
            t1.activated = true;

            linkTest = new Link(n1,n2,t1);
        }

        public void grouplink()
        {
            Group g0 = masterGroup.FindGroup("[G0]"); 
            Group g1 = masterGroup.FindGroup("[G1]");
            Tether t1 = new Tether();
            t1.active = true;
            t1.activated = true;
            linkTest = new Link(g0, g1, t1,formationtype.NearestN);
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
    }
}
