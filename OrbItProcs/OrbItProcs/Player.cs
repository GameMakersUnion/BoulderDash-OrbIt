using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OrbItProcs
{
    public class Player
    {
        private static Dictionary<dynamic, dynamic> userP = new Dictionary<dynamic, dynamic>()
        {
            {nodeE.position, new Vector2(0,0)},
            {nodeE.texture, textures.blueorb},
            {comp.movement, true },
            {comp.basicdraw, true },
            { comp.collision, true },
        };

        public Room room;
        public float maxvel { get; set; }
        public float maxaccel { get; set; }
        public Vector2 accel;
        public float absaccel { get; set; }
        public float friction { get; set; }
        public bool QuickStop { get; set; }

        public Node launchNode { get; set; }
        public int bulletlife { get; set; }
        public int firefreq { get; set; }
        //public int fireamount { get; set; }
        public int firefreqCounter = 0;

        public Node node { get; private set; } //todo:rename enum
        //public Node nodeCollision { get; set; }

        public Body body { get { return node.body; } }//{ get { return playerNode == null ? null : playerNode.body; } }

        public Dictionary<string, Node> savedNodes { get; set; }

        public CollisionDelegate onCollisionStay;
        public CollisionDelegate onCollisionEnter;
        public CollisionDelegate onCollisionExit;

        public int playerIndex;

        

        public static Node bigtony = null;
        public static float maxScore = 50000;

        public float score { get; set; }
                               
        public Color pColor;   
                               
        public static void MakeBigTony(Room room)
        {                      
            if (bigtony != null)
            {                  
                return;        
            }
            Dictionary<dynamic, dynamic> tonyProps = new Dictionary<dynamic, dynamic>()
            {
                { nodeE.position, new Vector2(room.worldWidth/2,room.worldHeight/2) },
                { nodeE.texture, textures.blackorb },
                { comp.movement, true },
                { comp.collision, true },
                { comp.basicdraw, true },
                { comp.phaseorb, true },
                //{ comp.laser, true },
                //{ comp.gravity, true },
            };
            Node tony = new Node(tonyProps);
            room.scheduler.doEveryXMilliseconds(delegate{
                if (Game1.soundEnabled) Scheduler.end.Play(0.3f, -0.5f, 0f);
                int rad = 100; 
                for (int i = 0; i < 10; i++)
                {              
                    int rx = Utils.random.Next(rad * 2) - rad;
                    int ry = Utils.random.Next(rad * 2) - rad;
                    //room.game.spawnNode(room.worldWidth / 2 + rx, room.worldHeight / 2 + ry);
                               
                }              
            }, 2000);          
            //tony.body.pos = new Vector2(room.worldWidth / 2, room.worldHeight / 2);
            tony.body.radius = 64;
            tony.body.mass = tonymass;
            tony.body.velocity *= 100;
            tony.name = "bigTony";
            tony.body.texture = textures.blackorb;
            tony[comp.queuer].queuecount = 100;
            
            bigtony = tony;    
                               
            EventHandler updateScores = null;
            updateScores = (ooo,eee)=>
            {                  
                foreach(var p in room.players)
                {              
                    if (p.node == bigtony)
                    {          
                        p.score += Game1.GlobalGameTime.ElapsedGameTime.Milliseconds;
                        if (p.score >= maxScore)
                        {      
                            p.node.body.radius += 500;
                            p.node.body.mass += 100;
                            foreach (var pp in room.players)
                            {  
                                //pp.node.body.ClearHandlers();
                                //pp.nodeCollision.body.ClearHandlers();
                                pp.node.collision.AllHandlersEnabled = false;
                            }  
                            if (Game1.soundEnabled) Scheduler.fanfare.Play();
                            bigtony.OnAffectOthers -= updateScores;
                        }      
                    }          
                }              
            };                 
            bigtony.OnAffectOthers += updateScores;

            if (Game1.bigTonyOn)
            {
                room.masterGroup.fullSet.Add(bigtony); //#bigtony
            }
        }                      
                               
                               
                               
        public static Color Add(Color c, Color b)
        {                      
            c = new Color(c.R + b.R, c.G + b.G, c.B + b.B, c.A);
            return c;          
        }                      
        public static Color Subtract(Color c, Color b)
        {
            c = new Color(c.R - b.R, c.G - b.G, c.B - b.B, c.A);
            return c;
        }
        int colorChange = 120;
        float smallmass = 2;
        float bigmass = 5;
        static float tonymass = 10;

        public HalfController halfController;

        public Player(int playerIndex)
        {
            room = Program.getRoom();
            room.game.ui.SetSidebarActive(false);
            MakeBigTony(room);//todo:fix
            score = 0;

            halfController = new HalfController(playerIndex, FullPadMode.mirrorMode);


            this.playerIndex = playerIndex;
            onCollisionStay = delegate(Node s, Node t)
            {
                //if (t != null && t != node) 
                  //  SwitchPlayerNode(t);
            };
            onCollisionEnter = delegate(Node s, Node t)
            {
                if (t != null && !room.playerNodes.Contains(t))
                {
                    t.body.color = Add(node.body.color, new Color(colorChange, colorChange, colorChange));
                }
            };
            onCollisionExit = delegate(Node s, Node t)
            {
                if (t != null && !room.playerNodes.Contains(t))
                {
                    t.body.color = Color.White;//Subtract(node.body.color, new Color(colorChange, colorChange, colorChange));
                }
            };
            
            savedNodes = new Dictionary<string, Node>();
            //playerNode = new Node(userP);
            //
            //playerNode.body.pos = Vector2.Zero;
            //playerNode.body.radius = 64;
            //playerNode.body.mass = 10;
            //playerNode.name = "bigTony";

            switch (playerIndex)
            {
                case 1: pColor = Color.Blue; break;
                case 2: pColor = Color.Green; break;
                case 3: pColor = Color.Red; break;
                case 4: pColor = Color.Yellow; break;
            }

            Vector2 spawnPos = Vector2.Zero;
            double angle = Utils.random.NextDouble() * Math.PI * 2;
            angle -= Math.PI;
            float dist = 200;
            float x = dist * (float)Math.Cos(angle);
            float y = dist * (float)Math.Sin(angle);
            spawnPos = new Vector2(room.worldWidth / 2, room.worldHeight / 2) - new Vector2(x, y);


            Dictionary<dynamic, dynamic> playerProps = new Dictionary<dynamic, dynamic>()
            {
                { nodeE.position, spawnPos },
                { nodeE.texture, textures.blackorb },
                { comp.movement, true },
                { comp.collision, true },
                { comp.basicdraw, true },
                { comp.phaseorb, true },
                //{ comp.laser, true },
                //{ comp.gravity, true },
            };

            //node = room.game.spawnNode((int)spawnPos.X, (int)spawnPos.Y);
            node = room.game.spawnNode(playerProps);
            node.name = "player" + playerIndex;

            node[comp.queuer].queuecount = 100;

            Collider collider = new Collider(new Circle(25));
            node.collision.AddCollider("trigger", collider);
            collider.OnCollisionStay += onCollisionStay;
            collider.OnCollisionEnter += onCollisionEnter;
            collider.OnCollisionExit += onCollisionExit;

            SwitchPlayerNode(node);

            accel = new Vector2(0, 0);
            maxvel = 20f;
            maxaccel = 3f;
            absaccel = 0.2f;
            friction = 0.01f;
            QuickStop = false;

            var bulletProps = new Dictionary<dynamic, dynamic>()
            {
                { nodeE.texture, textures.whitecircle },
                { comp.movement, true },
                //{ comp.collision, true },
                { comp.basicdraw, true },
                { comp.laser, true },
                { comp.gravity, true },
            };

            launchNode = new Node(bulletProps);
            //launchNode.comps[comp.laser].brightness = 0.5f;
            launchNode.comps[comp.laser].thickness.value = 3f;
            launchNode.GetComponent<Movement>().maxVel.value = 15;
            bulletlife = 1500;
            firefreq = 1;


            //room.game.ui.keyManager.addProcessKeyAction("firebullet", KeyCodes.LeftClick, OnPress: FireNode);
            //room.game.ui.keyManager.addProcessKeyAction("firebullets", KeyCodes.RightClick, OnHold: FireNodes);

            

            room.masterGroup.fullSet.Add(node);

            //Dictionary<dynamic, dynamic> userPP = new Dictionary<dynamic, dynamic>()
            //{
            //    { nodeE.position, new Vector2(0,0) },
            //    { nodeE.texture, textures.whitecircle },
            //    { comp.movement, false },
            //    { comp.basicdraw, false },
            //    { comp.maxvel, true },
            //    { comp.collision, true },
            //};
            //nodeCollision = new Node(userPP);
            //nodeCollision.collision.isSolid = false;
            //nodeCollision.body.OnCollisionStay += onCollisionStay;
            //nodeCollision.body.OnCollisionEnter += onCollisionEnter;
            //nodeCollision.body.OnCollisionExit += onCollisionExit;
            //room.masterGroup.fullSet.Add(nodeCollision);
            //nodeCollision.name = "phantom" + playerIndex;

        }
        bool switchAvailable = true;
        //float massChange = 10f;
        
        public void SwitchPlayerNode(Node n, bool addToSavedNodes = true, bool requireKeypress = false, Player other = null)
        {
            if (!switchAvailable || n == null || n.body.mass == 0 ) return;

            if (requireKeypress && !UserInterface.keybState.IsKeyDown(Keys.LeftControl)) return;

            if (addToSavedNodes && !savedNodes.ContainsValue(n))
            {
                savedNodes[n.name] = n;
            }
            if (other != null) //swapping with player
            {
                node.body.color = other.node.body.color;
                Node temp = node;

                //Collider trigger = temp.collision.GetCollider("trigger", true);
                //temp.collision.AddCollider("trigger", other.node.collision.colliders["trigger"]);
                //other.node.collision.colliders.Remove("trigger");
                //other.node.collision.AddCollider("trigger", trigger);
                node.collision.SwapCollider(other.node, "trigger");

                node = other.node;
                node.body.color = pColor;
                other.node = temp;

                other.switchAvailable = false;
                switchAvailable = false;
                room.scheduler.doAfterXMilliseconds(nn => other.switchAvailable = true, 1000);
            }
            else //swapping with non-player node
            {
                node.body.color = Color.White;
                node.body.texture = textures.whitecircle;
                if (node != bigtony) node.body.mass = smallmass;
                //Collider trigger = node.collision.colliders["trigger"];
                //node.collision.colliders.Remove("trigger");
                node.collision.SwapCollider(n, "trigger");

                Collider col = node.collision.GetCollider("trigger");
                if (col != null) col.HandlersEnabled = false;

                Collider col2 = n.collision.GetCollider("trigger");
                if (col2 != null) col2.HandlersEnabled = true;

                node = n;
                if (node != bigtony) node.body.mass = bigmass;
                //node.collision.AddCollider("trigger", trigger);

                
                node.body.texture = textures.blackorb;
                node.body.color = pColor;
            }
            
            switchAvailable = false;

            //room.scheduler.AddAppointment(new Appointment((nn, d) => switchAvailable = true, 100));
            Action<Node> time = delegate(Node nnn)
            {
                //Console.WriteLine(Game1.GlobalGameTime.TotalGameTime.Milliseconds);
                this.switchAvailable = true;
            };
            if (Game1.soundEnabled) Scheduler.start.Play(0.2f, 0f, 0f);
            room.scheduler.doAfterXMilliseconds(time, 1000, false); //todo:fix other todos for fucks sake
        }


        HalfPadState newHalfPadState;
        HalfPadState oldHalfPadState;

        public void Update(GameTime gametime)
        {
            if (node == null) return;
            newHalfPadState = halfController.getState();
            //nodeCollision.body.pos = body.pos;
            if (node != bigtony) node.collision.colliders["trigger"].radius = body.radius * 1.5f;//1.05f;
            else node.collision.colliders["trigger"].radius = body.radius * 1.2f;

            Vector2 stick;

            bool clicked = false;
            stick = newHalfPadState.stick1.v2;
            clicked = newHalfPadState.Btn3 == ButtonState.Pressed || newHalfPadState.Btn1 == ButtonState.Pressed;

            //Console.WriteLine(stick);
            stick.Y *= -1;
            stick *= 0.4f;
            stick *= absaccel;
            //stick = new Vector2(0, 0);
            if ((body.velocity.X != 0 || body.velocity.Y != 0))
            {
                stick += body.velocity * -friction;
            }
            stick *= body.mass;
            MaxVelocityUpdate();
            body.ApplyForce(stick);
            //Console.WriteLine(accel);

            if (clicked)
            {
                //Console.WriteLine(playerIndex);
                //GraphData.AddFloat(playerIndex);

                SwitchPlayer(stick);

            }

            //oldGamePadState = newGamePadState;
            oldHalfPadState = newHalfPadState;
        }

        public void SwitchPlayer(Vector2 stick)
        {
            var list = node.collision.colliders["trigger"].previousCollision;
            if (list.Count == 0) return;
            //Node n = null;
            //Player pp = null;
            //if (list.Any(a => room.players.Any(p => { n = a; pp = p; return p.node == a && p.node != node; })))
            //{
            //    SwitchPlayerNode(n, false, false, pp);
            //}
            foreach(var p in room.players)
            {
                if (p == this) continue;
                if (list.Contains(p.node.body))
                {
                    SwitchPlayerNode(p.node, false, false, p);
                    return;
                }
            }
            Node n = null;
            float master = -10000000f;
            float current;
            var playernodes = room.playerNodes;
            foreach (Collider q in list)
            {
                if (q.parent == node || playernodes.Contains(q.parent)) continue; //fixed?
                Vector2 dir = q.pos - body.pos; VMath.NormalizeSafe(ref dir);
                Vector2 stickN = stick; VMath.NormalizeSafe(ref stickN);

                if ((current = Vector2.Dot(dir, stickN)) > master)
                {
                    master = current;
                    n = q.parent;
                }
            }
            Console.WriteLine(master);

            //n = list.ElementAt(Utils.random.Next(list.Count));
            SwitchPlayerNode(n, false, false);

            
            
        }


        public void UpdateOld(GameTime gametime)
        {
            if (node == null) return;

            //room.camera.pos = body.pos - new Vector2(room.worldWidth/2,room.worldHeight/2);

            float x = 0, y = 0;
            
            if (UserInterface.keybState.IsKeyDown(Keys.W))
                y -= absaccel;
            if (UserInterface.keybState.IsKeyDown(Keys.S))
                y += absaccel;
            if (UserInterface.keybState.IsKeyDown(Keys.A))
                x -= absaccel;
            if (UserInterface.keybState.IsKeyDown(Keys.D))
                x += absaccel;


            accel.X = x;
            accel.Y = y;

            if (accel.X != 0 && accel.Y != 0)
            {
                accel *= 0.707f;
            }

            if (QuickStop)
            {
                if (accel.X > 0 && body.velocity.X < 0) body.velocity.X = 0;
                else if (accel.X < 0 && body.velocity.X > 0) body.velocity.X = 0;

                if (accel.Y > 0 && body.velocity.Y < 0) body.velocity.Y = 0;
                else if (accel.Y < 0 && body.velocity.Y > 0) body.velocity.Y = 0;
                
                if (accel.X == 0 && accel.Y == 0 && (body.velocity.X != 0 || body.velocity.Y != 0))
                {
                    accel = body.velocity;
                    VMath.NormalizeSafe(ref accel);
                    accel *= -friction;
                }
            }
            else
            {
                if ((body.velocity.X != 0 || body.velocity.Y != 0))
                {
                    accel += body.velocity * -friction;
                }
            }
            accel *= body.mass;

            MaxVelocityUpdate();

            body.ApplyForce(accel);

            if (UserInterface.keybState.IsKeyDown(Keys.Z))
            {
                body.velocity = new Vector2(0, 0);
                accel = body.velocity;
            }

        }

        public void FireNode()
        {
            if (node == null) return;
            Vector2 pos = UserInterface.WorldMousePos;
            Node newNode = new Node();
            Node.cloneNode(launchNode, newNode);
            newNode.body.velocity = pos - body.pos;
            newNode.body.pos = body.pos + body.velocity * 5;
            room.game.spawnNode(newNode, lifetime: bulletlife);
        }

        public void FireNodes()
        {
            if (node == null) return;
            if (firefreq != 0)
            {
                if (firefreqCounter++ % firefreq != 0)
                    return;
            }
            //FireNode();

            Vector2 pos = UserInterface.WorldMousePos;
            Node newNode = new Node();
            Node.cloneNode(launchNode, newNode);
            newNode.body.velocity = pos - body.pos;
            newNode.body.pos = body.pos + body.velocity * 5;

            room.game.spawnNode(newNode, lifetime: bulletlife);
        }

        public void MaxVelocityUpdate()
        {
            if (node == null) return;
            //Node parent = playerNode;
            
            if ((Math.Pow(body.velocity.X, 2) + Math.Pow(body.velocity.Y, 2)) > Math.Pow(maxvel, 2))
            {
                VMath.NormalizeSafe(ref body.velocity);
                body.velocity *= maxvel;

            }
            /*
            if ((Math.Pow(accel.X, 2) + Math.Pow(accel.Y, 2)) > Math.Pow(maxvel, 2))
            {
                accel.Normalize();
                accel *= maxaccel;

                accel = new Vector2(0, 0);
                return;
            }
            */
            /*
            float minvel = 0;
            if ((Math.Pow(parent.transform.velocity.X, 2) + Math.Pow(parent.transform.velocity.Y, 2)) < Math.Pow(minvel, 2))
            {
                parent.transform.velocity.Normalize();
                parent.transform.velocity *= minvel;
            }
            */
        }
    }
}
