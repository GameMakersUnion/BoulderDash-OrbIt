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
            {comp.maxvel, true },
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
        public Node nodeCollision { get; set; }

        public Body body { get { return node.body; } }//{ get { return playerNode == null ? null : playerNode.body; } }

        public Dictionary<string, Node> savedNodes { get; set; }

        public CollisionDelegate onCollision;
        public CollisionDelegate onCollisionStart;
        public CollisionDelegate onCollisionEnd;

        public int playerIndex;

        

        public static Node bigtony = null;
        public static float maxScore = 10000;

        public float score { get; set; }

        public Color pColor;

        public static void MakeBigTony(Room room)
        {
            if (bigtony != null)
            {
                return;
            }
            Node tony = new Node(userP);
            room.scheduler.doEveryXMilliseconds(delegate{
                Scheduler.end.Play(0.3f, -0.5f, 0f);
                int rad = 100;
                for (int i = 0; i < 10; i++)
                {
                    int rx = Utils.random.Next(rad * 2) - rad;
                    int ry = Utils.random.Next(rad * 2) - rad;
                    room.game.spawnNode(room.worldWidth / 2 + rx, room.worldHeight / 2 + ry);

                }
            }, 2000);
            tony.body.pos = new Vector2(room.worldWidth / 2, room.worldHeight / 2);
            tony.body.radius = 64;
            tony.body.mass = tonymass;
            tony.name = "bigTony";
            tony.body.texture = textures.blackorb;

            room.masterGroup.fullSet.Add(tony);
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
                                pp.node.ClearCollisionHandlers();
                                pp.nodeCollision.ClearCollisionHandlers();
                            }
                            Scheduler.fanfare.Play();
                            bigtony.OnAffectOthers -= updateScores;
                        }
                    }
                }
            };
            bigtony.OnAffectOthers += updateScores;
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

        public Player(int playerIndex)
        {
            room = Program.getRoom();
            MakeBigTony(room);//todo:fix
            score = 0;


            this.playerIndex = playerIndex;
            onCollision = delegate(Node s, Node t)
            {
                if (t != null && t != node) SwitchPlayerNode(t, requireKeypress: true);
            };
            onCollisionStart = delegate(Node s, Node t)
            {
                if (t != null && !room.playerNodes.Contains(t))
                {
                    t.body.color = Add(node.body.color, new Color(colorChange, colorChange, colorChange));
                }
            };
            onCollisionEnd = delegate(Node s, Node t)
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
            node = room.game.spawnNode((int)spawnPos.X, (int)spawnPos.Y);

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
                { comp.maxvel, true },
                { comp.laser, true },
                { comp.gravity, true },
            };

            launchNode = new Node(bulletProps);
            //launchNode.comps[comp.laser].lineXScale = 0.5f;
            launchNode.comps[comp.laser].lineYScale = 3f;
            launchNode.GetComponent<MaxVel>().maxvel = 15;
            bulletlife = 1500;
            firefreq = 1;


            //room.game.ui.keyManager.addProcessKeyAction("firebullet", KeyCodes.LeftClick, OnPress: FireNode);
            //room.game.ui.keyManager.addProcessKeyAction("firebullets", KeyCodes.RightClick, OnHold: FireNodes);

            room.masterGroup.fullSet.Add(node);


            Dictionary<dynamic, dynamic> userPP = new Dictionary<dynamic, dynamic>()
            {
                {nodeE.position, new Vector2(0,0)},
                {nodeE.texture, textures.whitecircle},
                {comp.movement, false },
                {comp.basicdraw, false },
                {comp.maxvel, true },
                { comp.collision, true },
            };
            nodeCollision = new Node(userPP);
            nodeCollision.collision.ResolveCollision = false;
            nodeCollision.OnCollision += onCollision;
            nodeCollision.OnCollisionStart += onCollisionStart;
            nodeCollision.OnCollisionEnd += onCollisionEnd; //todo: make warning when needing to update nodes that are already in collision list
            room.masterGroup.fullSet.Add(nodeCollision);

        }
        bool switchAvailable = true;
        float massChange = 10f;
        
        public void SwitchPlayerNode(Node n, bool addToSavedNodes = true, bool requireKeypress = false, Player other = null)
        {
            if (!switchAvailable || n == null || n.body.mass == 0 ) return;

            if (requireKeypress && !UserInterface.keybState.IsKeyDown(Keys.LeftControl)) return;

            if (addToSavedNodes && !savedNodes.ContainsValue(n))
            {
                savedNodes[n.name] = n;
            }
            if (other != null)
            {
                //if (playerNode != null) playerNode.ClearCollisionHandlers();
                node.body.color = other.node.body.color;
                //playerNode.OnCollisionStart -= onCollisionStart;
                Node temp = node;
                node = other.node;
                node.body.color = pColor;


                other.node = temp;
                //other.playerNode.body.color = playerNode.body.color;
                //playerNode.OnCollisionStart -= onCollisionStart;

                //playerNode.OnCollisionStart += onCollision;
                other.switchAvailable = false;
                switchAvailable = false;
                room.scheduler.doAfterXMilliseconds(nn => other.switchAvailable = true, 100);
            }
            else
            {
                //if (node != null) node.ClearCollisionHandlers();
                node.body.color = Color.Red;
                node.body.texture = textures.whiteorb;
                //node.OnCollisionStart -= onCollisionStart;
                if (node != bigtony) node.body.mass = smallmass;
                node = n;
                if (node != bigtony) node.body.mass = bigmass;
                
                node.body.texture = textures.blackorb;
                node.body.color = pColor;
                //node.OnCollisionEnd -= onCollisionEnd;

                //node.OnCollisionStart += onCollision;
            }
            
            switchAvailable = false;

            //room.scheduler.AddAppointment(new Appointment((nn, d) => switchAvailable = true, 100));
            Action<Node> time = delegate(Node nnn)
            {
                //Console.WriteLine(Game1.GlobalGameTime.TotalGameTime.Milliseconds);
                this.switchAvailable = true;
            };
            Scheduler.start.Play(0.2f, 0f, 0f);
            room.scheduler.doAfterXMilliseconds(time, 1000, false); //todo:fix other todos for fucks sake
        }

        GamePadState newGamePadState;
        GamePadState oldGamePadState;

        public void Update(GameTime gametime)
        {
            if (node == null) return;

            nodeCollision.body.pos = body.pos;
            nodeCollision.body.radius = body.radius * 1.3f;

            Vector2 stick;
            PlayerIndex index;
            switch ((playerIndex-1) / 2)
            {
                case 0: index = PlayerIndex.One; break;
                case 1: index = PlayerIndex.Two; break;
                case 2: index = PlayerIndex.Three; break;
                case 3: index = PlayerIndex.Four; break;
                default: index = PlayerIndex.One; break;
                
            }
            bool clicked = false;
            newGamePadState = GamePad.GetState(index);
            if (playerIndex % 2 == 0)
            {
                stick = GamePad.GetState(index).ThumbSticks.Left;
                //clicked = (newGamePadState.Buttons.LeftStick == ButtonState.Pressed && oldGamePadState.Buttons.LeftStick == ButtonState.Released)
                //    || (newGamePadState.Buttons.LeftShoulder == ButtonState.Pressed && oldGamePadState.Buttons.LeftShoulder == ButtonState.Released);
                clicked = newGamePadState.Buttons.LeftStick == ButtonState.Pressed || newGamePadState.Buttons.LeftShoulder == ButtonState.Pressed;
            }
            else
            {
                stick = GamePad.GetState(index).ThumbSticks.Right;
                //clicked = (newGamePadState.Buttons.RightStick == ButtonState.Pressed && oldGamePadState.Buttons.RightStick == ButtonState.Released)
                //    || (newGamePadState.Buttons.RightShoulder == ButtonState.Pressed && oldGamePadState.Buttons.RightShoulder == ButtonState.Released);
                clicked = newGamePadState.Buttons.RightStick == ButtonState.Pressed || newGamePadState.Buttons.RightShoulder == ButtonState.Pressed;
            }

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

            oldGamePadState = newGamePadState;
        }

        public void SwitchPlayer(Vector2 stick)
        {
            var list = nodeCollision.collision.previousCollision;
            if (list.Count <= 1) return;
            Node n = null;
            Player pp = null;
            if (false && list.Any(a => room.players.Any(p => { n = a; pp = p; return p.node == a && p.node != node; })))
            {
                SwitchPlayerNode(n, false, false, pp);
            }
            else if (true)
            {
                float master = -1.1f;
                float current;
                foreach (Node q in list)
                {
                    if (q == node) continue;
                    Vector2 dir = q.body.pos - body.pos; VMath.NormalizeSafe(ref dir);
                    Vector2 stickN = stick; VMath.NormalizeSafe(ref stickN);

                    if ((current = Vector2.Dot(dir, stickN)) > master)
                    {
                        master = current;
                        n = q;
                    }
                }
                //Console.WriteLine(master);

                //n = list.ElementAt(Utils.random.Next(list.Count));
                SwitchPlayerNode(n, false, false);
            }
            else
            {
                //oreach (Node q in list)
                //
                //   if (q == node) continue;
                //
            }
            
            
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
                    accel.Normalize();
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
            Node.cloneObject(launchNode, newNode);
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
            Node.cloneObject(launchNode, newNode);
            newNode.body.velocity = pos - body.pos;
            newNode.body.pos = body.pos + body.velocity * 5;

            if (UserInterface.keybState.IsKeyDown(Keys.LeftControl))
            {
                if (newNode.comps.ContainsKey(comp.maxvel))
                    newNode.comps[comp.maxvel].active = false;

            }
            room.game.spawnNode(newNode, lifetime: bulletlife);
        }

        public void MaxVelocityUpdate()
        {
            if (node == null) return;
            //Node parent = playerNode;
            
            if ((Math.Pow(body.velocity.X, 2) + Math.Pow(body.velocity.Y, 2)) > Math.Pow(maxvel, 2))
            {
                body.velocity.Normalize();
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
