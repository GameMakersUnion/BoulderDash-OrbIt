using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polenter.Serialization;

using OrbItProcs.Components;
using OrbItProcs.Processes;

using Component = OrbItProcs.Components.Component;
using System.Collections.ObjectModel;

namespace OrbItProcs {

    public enum textures
    {
        blueorb,
        whiteorb,
        colororb,
        whitecircle,
        whitepixel,
    }

    public class Room {


        public Game1 game;
        public ProcessManager processManager { get; set; }
        public float mapzoom { get; set; }

        public GridSystem gridsystem { get; set; }
        public List<Rectangle> gridSystemLines;
        public int gridSystemCounter = 0;

        public Dictionary<string, bool> PropertiesDict = new Dictionary<string, bool>();

        //public Dictionary<string, Group> groups = new Dictionary<string, Group>();
        public Group masterGroup;

        public ObservableCollection<object> nodes = new ObservableCollection<object>();

        public Queue<object> nodesToAdd = new Queue<object>();

        public Node defaultNode, targetNodeGraphic;
        //public NodeList<Node> nodes = new NodeList<Node>();
        public int dtimerMax = 0, dtimerCount = 0;

        public SharpSerializer serializer = new SharpSerializer();

        //public tree treeProp = tree.gridsystem;

        public Room()
        {
            setDefaultProperties();
        }

        public Room(Game1 game)
        {
            setDefaultProperties();

            processManager = new ProcessManager(this);
            
            this.game = game;

            // grid System
            gridsystem = new GridSystem(this, 40, 4);
            //gridsystem = new GridSystem(this, 40, 70, 4);
            gridSystemLines = new List<Rectangle>();

            
            
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
            //these make it convienient to check values after pausing the game my mouseing over
            if (defaultNode == null) defaultNode = null;
            //if (game.ui.sidebar.lstComp == null) game.ui.sidebar.lstComp = null;

            if (dtimerCount > dtimerMax)
            {
                dtimerCount = 0;
                processManager.Update();

                gridsystem.clear();
                gridSystemLines = new List<Rectangle>();

                HashSet<Node> toDelete = new HashSet<Node>();
                //add all nodes from every group to the full hashset of nodes, and insert unique nodes into the gridsystem
                masterGroup.ForEachFullSet(delegate(Node o) 
                {
                    Node n = (Node)o; 
                    gridsystem.insert(n);
                });

                //fullset.ToList().ForEach(delegate(Node n) { gridsystem.insert(n); });

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
                        if (n == game.ui.editNode) game.ui.editNode = null;
                        if (n == game.ui.spawnerNode) game.ui.spawnerNode = null;
                    }
                });

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
                
                //addGridSystemLines(gridsystem);
                addBorderLines();
                //colorEffectedNodes();
            }
            else
            {
                dtimerCount++;
            }

            updateTargetNodeGraphic();

        }

        public void updateTargetNodeGraphic()
        {
            if (game.targetNode != null)
            {
                targetNodeGraphic.transform.position = game.targetNode.transform.position;
                targetNodeGraphic.transform.scale = game.targetNode.transform.scale * 1.5f;
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
                    if (_node.transform.color != Color.Black)
                    {
                        if (returnObjectsGridSystem.Contains(_node))
                            _node.transform.color = Color.Purple;
                        else
                            _node.transform.color = Color.White;
                    }
                }
                game.targetNode.transform.color = Color.Red;
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

        public void Draw(SpriteBatch spritebatch)
        {
            
            if (game.targetNode != null)
            {
                targetNodeGraphic.Draw(spritebatch);
            }
            masterGroup.ForEachFullSet(delegate(Node o)
            {
                Node n = (Node)o;
                n.Draw(spritebatch);
            });
            int linecount = 0;

            foreach (Rectangle rect in gridSystemLines)
            {
                //float scale = 1 / mapzoom;
                Rectangle maprect = new Rectangle((int)(rect.X / mapzoom), (int)(rect.Y / mapzoom), (int)(rect.Width / mapzoom), (int)(rect.Height / mapzoom));
                spritebatch.DrawLine(new Vector2(maprect.X, maprect.Y), new Vector2(maprect.Width, maprect.Height), Color.Green);
                linecount++;
            }   
            
        } 

        public void setDefaultProperties()
        {
            mapzoom = 2f;

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
