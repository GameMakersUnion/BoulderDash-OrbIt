﻿using System;
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
        public bool DrawLinks { get; set; }
        public float WallWidth { get; set; }
        
        public string name;

        public int worldWidth { get; set; }
        public int worldHeight { get; set; }
        public int colIterations { get; set; }

        #region // References // --------------------------------------------
        public OrbIt game { get { return OrbIt.game; } }
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
        public Queue<String> levelList = new Queue<string>();

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
        public Group presetGroups
        {
            get
            {
                if (masterGroup == null) return null;
                return masterGroup.childGroups["Preset Groups"];
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
        public Group bulletGroup
        {
            get
            {
                if (masterGroup == null) return null;
                return masterGroup.childGroups["Bullet Group"];
            }
        }
        [Polenter.Serialization.ExcludeFromSerialization]
        public Group wallGroup
        {
            get
            {
                if (masterGroup == null) return null;
                return masterGroup.childGroups["Wall Group"];
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

        private bool resizeRoomSignal = false;

        public Room()
        {
            
        }
        Action<Collider, Collider> collideAction;

        
        public Room(OrbIt game, int worldWidth, int worldHeight, bool Groups = true) : this()
        {
            levelList.Enqueue("Level1");
            levelList.Enqueue("Level2");
            levelList.Enqueue("Level3");
            levelList.Enqueue("Level4");

            this.worldWidth = worldWidth;
            this.worldHeight = worldHeight;
            groupHashes = new ObservableHashSet<string>();
            nodeHashes = new ObservableHashSet<string>();
            CollisionSetCircle = new HashSet<Collider>();
            CollisionSetPolygon = new HashSet<Collider>();
            colIterations = 1;
            
            
            scheduler = new Scheduler();
            borderColor = Color.DarkGray;
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
                        //if (!b.exclusionList.Contains(bb)) 
                            b.CheckCollisionBody(bb);
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
                        //if (!c1.exclusionList.Contains(bb)) 
                            c1.CheckCollisionBody(bb);
                    }
                    //else
                    //{
                    //    //c.CheckCollision(c2);
                    //}
                }
            };
            //this.mapzoom = 2f;


            // grid System
            gridsystemAffect = new GridSystem(this, 40, new Vector2(0, worldHeight - OrbIt.Height), worldWidth, OrbIt.Height);
            gridsystemCollision = new GridSystem(this, gridsystemAffect.cellsX, new Vector2(0, worldHeight - OrbIt.Height), worldWidth, OrbIt.Height);
            //gridsystemAffect = new GridSystem(this, 40, new Vector2(0, worldHeight - OrbIt.Height), worldWidth, OrbIt.Height);
            level = new Level(this, 40, 40, gridsystemAffect.cellWidth, gridsystemAffect.cellHeight);
            roomRenderTarget = new RenderTarget2D(game.GraphicsDevice, OrbIt.Width, OrbIt.Height);
            //gridsystemCollision = new GridSystem(this, gridsystemAffect.cellsX, new Vector2(0, worldHeight - OrbIt.Height), worldWidth, OrbIt.Height);
            camera = new ThreadedCamera(this, 1f);
            DrawLinks = true;
            WallWidth = 10;
            scheduler = new Scheduler();

            players = new HashSet<Player>();

            #region ///Default User props///
            Dictionary<dynamic, dynamic> userPr = new Dictionary<dynamic, dynamic>() {
                { nodeE.position, new Vector2(0, 0) },
                { nodeE.texture, textures.boulder1 },
                { comp.basicdraw, true },
                { comp.collision, true },
                { comp.movement, true },
            };
            #endregion


            defaultNode = new Node(this, userPr);
            defaultNode.name = "master";
            //defaultNode.IsDefault = true;

            defaultNode.comps.Keys.ToList().ForEach(delegate(Type c)
            {
                defaultNode.comps[c].AfterCloning();
            });

            Node firstdefault = new Node(this, ShapeType.eCircle);
            //firstdefault.addComponent(comp.itempayload, true);
            Node.cloneNode(defaultNode, firstdefault);
            firstdefault.name = "[G0]0";
            //firstdefault.IsDefault = true;

            masterGroup = new Group(this, defaultNode, null, defaultNode.name, false);
            if (Groups)
            {
                Group generalGroup = new Group(this, defaultNode, masterGroup, "General Groups", false);
                Group presetsGroup = new Group(this, defaultNode, masterGroup, "Preset Groups", false);
                Group playerGroup = new Group(this, defaultNode.CreateClone(this), masterGroup, "Player Group", false);
                Group itemGroup = new Group(this, defaultNode, masterGroup, "Item Group", false);
                Group linkGroup = new Group(this, defaultNode, masterGroup, "Link Groups", false);
                Group bulletGroup = new Group(this, defaultNode.CreateClone(this), masterGroup, "Bullet Group", true);
                Group wallGroup = new Group(this, defaultNode, masterGroup, "Wall Group", true);
                Group firstGroup = new Group(this, firstdefault, generalGroup, "Group1");
            }

            Dictionary<dynamic, dynamic> userPropsTarget = new Dictionary<dynamic, dynamic>() {
                    { comp.basicdraw, true }, 
                    { comp.colorchanger, true }, 
                    { nodeE.texture, textures.ring } 
            };

            targetNodeGraphic = new Node(this,userPropsTarget);
            
            targetNodeGraphic.name = "TargetNodeGraphic";

            //MakeWalls();

            MakePresetGroups();
            MakeItemGroups();
        }

        public void MakePresetGroups()
        {
            var infos = Utils.compInfos;
            int runenum = 0;
            foreach(Type t in infos.Keys)
            {
                Info info = infos[t];
                if ((info.compType & mtypes.essential) == mtypes.essential) continue;
                if ((info.compType & mtypes.exclusiveLinker) == mtypes.exclusiveLinker) continue;
                if ((info.compType & mtypes.item) == mtypes.item) continue;
                if (info.userLevel == UserLevel.Developer || info.userLevel == UserLevel.Advanced) continue;
                if (t == typeof(Lifetime)) continue;
                if (t == typeof(Rune)) continue;
                Node nodeDef = defaultNode.CreateClone(this);
                nodeDef.SetColor(Utils.randomColor());
                nodeDef.addComponent(t, true);
                nodeDef.addComponent(typeof(Rune), true);
                nodeDef.Comp<Rune>().runeTexture = (textures)runenum++;
                Group presetgroup = new Group(this, nodeDef, presetGroups, t.ToString().LastWord('.') + " Group");
            }
        }

        public void MakeItemGroups()
        {
            Node itemDef = defaultNode.CreateClone(this);
            itemDef.addComponent(comp.itempayload, true);
            itemDef.movement.active = false;

            var infos = Utils.compInfos;
            foreach (Type t in infos.Keys)
            {
                Info info = infos[t];
                if ((info.compType & mtypes.item) != mtypes.item) continue;
                if (t == typeof(ItemPayload)) continue;
                //if (info.userLevel == UserLevel.Developer || info.userLevel == UserLevel.Advanced) continue;
                Node nodeDef = itemDef.CreateClone(this); ///
                //nodeDef.addComponent(t, true);
                Component c = Node.MakeComponent(t, true, nodeDef);
                nodeDef.Comp<ItemPayload>().AddComponentItem(c);
                Group itemgroup = new Group(this, nodeDef, itemGroup, t.ToString().LastWord('.') + " Item");
            }

            //Node gravDef = itemDef.CreateClone(this);
            //Gravity grav = new Gravity(gravDef);
            //gravDef.Comp<ItemPayload>().AddComponentItem(grav);
            //Group gravItems = new Group(this, gravDef, itemGroup, "gravItem");
            //
            //Node shooterDef = itemDef.CreateClone(this);
            //Shooter shoot = new Shooter(shooterDef);
            ////grav.mode = Gravity.Mode.Strong;
            //shooterDef.Comp<ItemPayload>().AddComponentItem(shoot);
            //Group shooterItems = new Group(this, shooterDef, itemGroup, "shooterItem");
            //
            //Node swordDef = itemDef.CreateClone(this);
            //Sword sword = new Sword(swordDef);
            ////grav.mode = Gravity.Mode.Strong;
            //swordDef.Comp<ItemPayload>().AddComponentItem(sword);
            //Group swordItems = new Group(this, swordDef, itemGroup, "swordItem");
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
        public int affectAlgorithm = 2;
        public bool ColorNodesInReach = false;

        public float scrollRate = 0.5f;//0.5f;
        public bool skipOutsideGrid = true;
        public bool scroll = true; //#tojam
        public int waitTime = 5000;
        public int waitTimeCounter = 0;
        public bool drawCage = true;
        public void Update(GameTime gametime)
        {
            int heightCounter = OrbIt.Height / 2;
            while (drawCage && heightCounter < worldHeight - OrbIt.Height)
            {
                //int rand = Utils.random.Next(50);
                //if (rand < 3)
                //{
                //    camera.Draw(textures.cage, new Vector2(worldWidth / 2, heightCounter), Color.Pink, 1f, Layers.Under4);
                //}
                //else
                //{
                    camera.Draw(textures.goat, new Vector2(worldWidth / 2, heightCounter), Color.Yellow, 0.5f, Layers.Under4);
                //}
                heightCounter += OrbIt.Height;
            }

            FloodFill.boulderFountain();
            if (scroll && gameStarted)
            {
                if (waitTimeCounter < waitTime)
                {
                    waitTimeCounter += gametime.ElapsedGameTime.Milliseconds;
                }
                else
                {
                    if (gridsystemAffect.position.Y > 0) { gridsystemAffect.position.Y -= scrollRate; gridsystemCollision.position.Y -= scrollRate; }
                    camera.pos = gridsystemCollision.position + new Vector2(gridsystemCollision.gridWidth / 2, gridsystemCollision.gridHeight / 2);
//#tojam
                }
            }
            camera.RenderAsync();
            long elapsed = 0;
            if (gametime != null) elapsed = (long)Math.Round(gametime.ElapsedGameTime.TotalMilliseconds);
            totalElapsedMilliseconds += elapsed;

            gridSystemLines = new List<Rectangle>();

            //game.processManager.Update();
            int counter = 0;
            camera.Draw(textures.endLight, new Vector2(0, 0), Color.White, .5f, Layers.Over4, center: false);

            Assets.Spider.UpdateSpider(this);

            do
            {
                camera.Draw(textures.ridgesL, new Vector2(0, counter), Color.DarkGray, .5f, Layers.Under4, center: false);
                camera.Draw(textures.ridgesR, new Vector2((this.worldWidth - Assets.textureDict[textures.ridgesL].Width * .5f), counter), Color.DarkGray, .5f, Layers.Under4, center: false);
                counter += Assets.textureDict[textures.ridgesL].Height / 2;
            } while (counter < worldHeight);
            HashSet<Node> toDelete = new HashSet<Node>();
            //if (affectAlgorithm == 1)//OLD for testing
            //{
            //    gridsystemAffect.clear();
            //    foreach (var n in masterGroup.fullSet)
            //    {
            //        if (ColorNodesInReach) n.body.color = Color.White;
            //        if (masterGroup.childGroups["Wall Group"].fullSet.Contains(n)) continue;
            //        gridsystemAffect.insert(n.body);
            //    }
            //}
            if (affectAlgorithm == 2)
            {
                gridsystemAffect.clearBuckets();
                foreach (var n in masterGroup.fullSet)
                {
                    if (skipOutsideGrid && (n.body.pos.Y < gridsystemAffect.position.Y || n.body.pos.Y > gridsystemAffect.position.Y + gridsystemAffect.gridHeight)) continue;

                    if (ColorNodesInReach) n.body.color = Color.White;
                    if (masterGroup.childGroups["Wall Group"].fullSet.Contains(n)) continue;
                    gridsystemAffect.insertToBuckets(n.body);
                }
            }

            UpdateCollision();
            if (contacts.Count > 0) contacts = new List<Manifold>();

            foreach(Node n in masterGroup.fullSet.ToList())
            {
                //if (skipOutsideGrid && (n.body.pos.Y < gridsystemAffect.position.Y || n.body.pos.Y > gridsystemAffect.position.Y + gridsystemAffect.gridHeight)) continue;

                if (!n.body.pos.isWithin(gridsystemAffect.position, gridsystemAffect.position + new Vector2(gridsystemAffect.gridWidth, gridsystemAffect.gridHeight))) continue;
                
                if (n.active)
                {
                    n.Update(gametime);
                }
            }
            if (AfterIteration != null) AfterIteration(this, null);

            //addGridSystemLines(gridsystemCollision);
            //addLevelLines(level);
            addBorderLines();
            //colorEffectedNodes();

            updateTargetNodeGraphic();

            scheduler.AffectSelf();

            CheckForRoomResize();

            Draw();
            camera.CatchUp();
        }

        private void CheckForRoomResize()
        {
            if (resizeRoomSignal)
            {
                triggerResizeRoom();
                resizeRoomSignal = false;
            }
        }

        

        static int algorithm = 7;
        private void UpdateCollision()
        {
            Testing.modInc();
            //Testing.w("insertion").Start();
            //if (algorithm <= 4)
            //{
            //    gridsystemCollision.clear();
            //    foreach (var c in CollisionSetCircle) //.ToList()
            //    {
            //        gridsystemCollision.insert(c);
            //    }
            //}
            if (algorithm >= 5)
            {
                gridsystemCollision.clearBuckets();
                foreach (var c in CollisionSetCircle) //.ToList()
                {
                    if (skipOutsideGrid && (c.pos.Y < gridsystemCollision.position.Y || c.pos.Y > gridsystemCollision.position.Y + gridsystemCollision.gridHeight)) continue;

                    //if (ColorNodesInReach) c.parent.body.color = Color.White;
                    if (!c.parent.active) continue;
                    gridsystemCollision.insertToBuckets(c);
                }
            }

            Testing.PrintTimer("insertion");//oh, mama.

            gridsystemCollision.alreadyVisited = new HashSet<Collider>();

            foreach (var c in CollisionSetPolygon.ToList())
            {
                if (c.parent.active)
                {
                    gridsystemCollision.alreadyVisited.Add(c);
                    int reach = (int)c.radius * 2;
                    if (c.shape is Circle)//todo fix the fact that circles are in the polygons list
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
                if (skipOutsideGrid && (c.pos.Y < gridsystemCollision.position.Y || c.pos.Y > gridsystemCollision.position.Y + gridsystemCollision.gridHeight)) continue;
                if (c.parent.active)
                {
                    gridsystemCollision.alreadyVisited.Add(c);
                    int reach = (int)c.radius * 2;
                    if (c.shape is Polygon)
                    {
                        reach = (int)(c.shape as Polygon).polyReach; //shouldnt have a polygon in the circle list
                        foreach (var otherCol in CollisionSetCircle.ToList())
                        {
                            collideAction(c, otherCol);
                        }
                    }
                    //else if (algorithm == 5)
                    //{
                    //    var bucketBag = gridsystemCollision.retrieveBucketBags(c);
                    //    if (bucketBag != null)
                    //    {
                    //        if (c is Body)
                    //        {
                    //            Body b = (Body)c;
                    //            for (int i = 0; i < bucketBag.index; i++)
                    //            {
                    //                for (int j = 0; j < bucketBag.array[i].index; j++)
                    //                {
                    //                    Collider cc = bucketBag.array[i].array[j];
                    //                    if (cc.parent == b.parent) continue;
                    //                    if (c.parent == targetNode) cc.parent.body.color = Color.Purple;
                    //                    if (gridsystemCollision.alreadyVisited.Contains(cc))
                    //                        continue;
                    //                    if (cc is Body)
                    //                    {
                    //                        Body bb = (Body)cc;
                    //                        //if (!b.exclusionList.Contains(bb)) 
                    //                            b.CheckCollisionBody(bb);
                    //                    }
                    //                    else
                    //                    {
                    //                        b.CheckCollisionCollider(cc);
                    //                    }
                    //                }
                    //            }
                    //        }
                    //        else
                    //        {
                    //            for (int i = 0; i < bucketBag.index; i++)
                    //            {
                    //                for (int j = 0; j < bucketBag.array[i].index; j++)
                    //                {
                    //                    Collider cc = bucketBag.array[i].array[j];
                    //                    if (cc.parent == c.parent) continue;
                    //                    if (cc.parent == targetNode) cc.parent.body.color = Color.Purple;
                    //                    if (gridsystemCollision.alreadyVisited.Contains(cc))
                    //                    continue;
                    //                    if (cc is Body)
                    //                    {
                    //                        Body bb = (Body)cc;
                    //                        //if (!c.exclusionList.Contains(bb)) 
                    //                            c.CheckCollisionBody(bb);
                    //                    }
                    //                    else
                    //                    {
                    //                        //c.CheckCollision(cc);
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    else if (algorithm == 7)
                    {
                        gridsystemCollision.retrieveOffsetArraysCollision(c, collideAction, c.radius * 2);
                    }
                    //else if (algorithm == 6)
                    //{
                    //    gridsystemCollision.retrieveFromAllOffsets(c, reach, collideAction);
                    //}
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
                if (n.body.pos.isWithin(gridsystemAffect.position, gridsystemAffect.position + new Vector2(gridsystemAffect.gridWidth, gridsystemAffect.gridHeight)))
                {
                n.movement.IntegrateVelocity();

                VMath.Set(ref n.body.force, 0, 0);
                n.body.torque = 0;
            }
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
            foreach(var n in masterGroup.fullSet.ToList()) //todo:wtfuck threading?
            {
                //tojam
                if (skipOutsideGrid && (n.body.pos.Y < (gridsystemAffect.position.Y - gridsystemAffect.gridHeight /2) || n.body.pos.Y > gridsystemAffect.position.Y + gridsystemAffect.gridHeight)) continue;
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
            OrbIt.globalGameMode.Draw();
            //if (linkTest != null) linkTest.GenericDraw(spritebatch);

            foreach (Rectangle rect in gridSystemLines)
            {
                //float scale = 1 / mapzoom;
                Rectangle maprect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
                //spritebatch.DrawLine((new Vector2(maprect.X, maprect.Y) - camera.pos) * zoom, (new Vector2(maprect.Width, maprect.Height) - camera.pos) * zoom, Color.Green, 2);
                camera.DrawLine(new Vector2(maprect.X, maprect.Y), new Vector2(maprect.Width, maprect.Height), 2, borderColor, Layers.Under5);
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
            Node n = new Node(this, props);
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

            masterGroup.childGroups["Wall Group"].entities.Add(n);
            return n;
        }

        

        public void updateTargetNodeGraphic()
        {
            if (targetNode != null)
            {
                targetNodeGraphic.Comp<ColorChanger>().AffectSelf();
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
                int x = i * gs.cellWidth + (int)gs.position.X;
                gridSystemLines.Add(new Rectangle(x, (int)gs.position.Y, x, gs.gridHeight + (int)gs.position.Y));
            }
            for (int i = 0; i <= gs.cellsY; i++)
            {
                int y = i * gs.cellHeight + (int)gs.position.Y;
                gridSystemLines.Add(new Rectangle((int)gs.position.X, y, gs.gridWidth + (int)gs.position.X, y));
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
            Group g1 = masterGroup.FindGroup(OrbIt.ui.sidebar.cbListPicker.SelectedItem());
            g1.defaultNode.Comp<Tether>().compType = mtypes.affectother | mtypes.draw;
        }

        public void hide()
        {
            //Game1.ui.sidebar.lstComp.Visible = false;
        }
        public void show()
        {
            //Game1.ui.sidebar.lstComp.Visible = true;
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

        public Node spawnNode(Node newNode, Action<Node> afterSpawnAction = null, int lifetime = -1, Group g = null)
        {
            Group spawngroup = g ?? OrbIt.ui.sidebar.ActiveGroup;
            if (spawngroup == null || !spawngroup.Spawnable) return null;
            if (g != null)
            {
                spawngroup = g;
            }
            newNode.name = "bullet" + Node.nodeCounter;

            return SpawnNodeHelper(newNode, afterSpawnAction, spawngroup, lifetime);
        }
        public Node spawnNode(Dictionary<dynamic, dynamic> userProperties, Action<Node> afterSpawnAction = null, bool blank = false, int lifetime = -1)
        {
            Group activegroup = OrbIt.ui.sidebar.ActiveGroup;
            if (activegroup == null || !activegroup.Spawnable) return null;

            Node newNode = new Node(this, ShapeType.eCircle);
            if (!blank)
            {
                Node.cloneNode(OrbIt.ui.sidebar.ActiveDefaultNode, newNode);
            }
            newNode.group = activegroup;
            newNode.name = activegroup.Name + Node.nodeCounter;
            newNode.acceptUserProps(userProperties);

            return SpawnNodeHelper(newNode, afterSpawnAction, activegroup, lifetime);
        }

        private Node SpawnNodeHelper(Node newNode, Action<Node> afterSpawnAction = null, Group g = null, int lifetime = -1)
        {
            //newNode.addComponent(comp.itempayload, true);
            newNode.OnSpawn();
            if (afterSpawnAction != null) afterSpawnAction(newNode);
            if (lifetime != -1)
            {
                newNode.addComponent(comp.lifetime, true);
                newNode.Comp<Lifetime>().timeUntilDeath.value = lifetime;
                newNode.Comp<Lifetime>().timeUntilDeath.enabled = true;
            }
            
            g.IncludeEntity(newNode);
            return newNode;
        }

        public Node spawnNode()
        {
            Dictionary<dynamic, dynamic> userP = new Dictionary<dynamic, dynamic>() {
                                { nodeE.position, UserInterface.WorldMousePos },
            };
            return spawnNode(userP);
        }

        public Node spawnNode(int worldMouseX, int worldMouseY)
        {
            Dictionary<dynamic, dynamic> userP = new Dictionary<dynamic, dynamic>() {
                                { nodeE.position, new Vector2(worldMouseX,worldMouseY) },
            };
            return spawnNode(userP);
        }

        bool fillWithGrid = false;
        internal void resize(Vector2 vector2, bool fillWithGrid = false)
        {
            resizeVect = vector2;
            resizeRoomSignal = true;
            this.fillWithGrid = fillWithGrid;
        }
        private void triggerResizeRoom()
        {
            worldWidth = (int)resizeVect.X;
            worldHeight = (int)resizeVect.Y;
            int newCellsX = worldWidth / gridsystemCollision.cellWidth;
            int gridHeight = fillWithGrid ? worldHeight : OrbIt.Height;
            gridsystemAffect = new GridSystem(this, newCellsX, new Vector2(0, worldHeight - gridHeight), worldWidth, gridHeight);
            level = new Level(this, newCellsX, newCellsX, gridsystemAffect.cellWidth, gridsystemAffect.cellHeight);
            //roomRenderTarget = new RenderTarget2D(game.GraphicsDevice, worldWidth, worldHeight);
            gridsystemCollision = new GridSystem(this, newCellsX, new Vector2(0, worldHeight - gridHeight), worldWidth, gridHeight);
            fillWithGrid = false;
        }

        private Vector2 resizeVect; //in the land down under
        public bool loading;
        public bool gameStarted = false;
        internal void boulderize(Action afterFilling)
        {
            int heightCounter = OrbIt.Height/2;

            FloodFill.afterFilling += afterFilling;
            while (heightCounter < worldHeight - OrbIt.Height)
            {
                FloodFill.startFill(new Vector2(worldWidth / 2, heightCounter));
                heightCounter += OrbIt.Height;
            }
        }
    }
}
