﻿using System;
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
            gridsystem = new GridSystem(this, 40, 40);
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

                HashSet<Node> toRemove = new HashSet<Node>();
                //add all nodes from every group to the full hashset of nodes, and insert unique nodes into the gridsystem
                masterGroup.entities.ToList().ForEach(delegate(object o) 
                {
                    Node n = (Node)o; 
                    gridsystem.insert(n);
                });

                //fullset.ToList().ForEach(delegate(Node n) { gridsystem.insert(n); });

                masterGroup.entities.ToList().ForEach(delegate(object o)
                {
                    Node n = (Node)o;
                    n.Update(gametime);
                    if (!n.active)
                    {
                        toRemove.Add(n);
                    }
                });

                toRemove.ToList().ForEach(delegate(Node n) 
                {
                    if (masterGroup.entities.Contains(n)) masterGroup.entities.Remove(n);
                    /*
                    groups.Keys.ToList().ForEach(delegate(string key)
                    {
                        Group g = groups[key];
                        if (g.entities.Contains(n)) g.entities.Remove(n);
                    });
                    */
                });
                

                /*
                List<Node> toRemove = new List<Node>();
                //Console.WriteLine("AMOUNT OF NODES: {0}",nodes.Count);
                foreach (Node _node in nodes)
                {
                    gridsystem.insert(_node);

                    //_node.color = Color.White;
                }
                foreach (Node _node in nodes)
                {

                    _node.Update(gametime);

                    if (!_node.active)
                    {
                        toRemove.Add(_node);
                    }
                }
                
                int toAddCounter = nodesToAdd.Count;
                for (int i = 0; i < toAddCounter; i++)
                {
                    nodes.Add(nodesToAdd.Dequeue());
                }
                //Console.WriteLine("Nodes: {0}\t Quadcount: {1}\t Gridcount: {2}\t Normal: {3}", nodes.Count, quadcounter,gridcounter,nodes.Count * nodes.Count);

                foreach (Node node in toRemove)
                {
                    //Console.WriteLine("node removed. ------------------------------------");
                    nodes.Remove(node);
                    //game.ui.sidebar.UpdateNodesTitle();
                }
                */
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
                targetNodeGraphic.position = game.targetNode.position;
                targetNodeGraphic.scale = game.targetNode.scale * 1.5f;
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
                    if (_node.color != Color.Black)
                    {
                        if (returnObjectsGridSystem.Contains(_node))
                            _node.color = Color.Purple;
                        else
                            _node.color = Color.White;
                    }
                }
                game.targetNode.color = Color.Red;
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
            masterGroup.entities.ToList().ForEach(delegate(object o)
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

        public void test(int a, float f, string s = " Ok")
        {
            Console.WriteLine(a*a + " " + f + " " + s);
        }
    }
}
