using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
namespace OrbItProcs
{
    public class BigTonyData : PlayerData
    {
        public bool switchAvailable = true;
    }
    //todo:make the players use maxvel 20
    public class BigTony : Gametype
    {
        Dictionary<dynamic, dynamic> userP = new Dictionary<dynamic, dynamic>()
        {
            { nodeE.position, new Vector2(0,0)},
            { nodeE.texture, textures.blueorb},
            { comp.movement, true },
            { comp.basicdraw, true },
            { comp.collision, true },
        };
        Dictionary<dynamic, dynamic> playerProps = new Dictionary<dynamic, dynamic>()
            {
                
                { nodeE.texture, textures.blackorb },
                { comp.movement, true },
                { comp.collision, true },
                { comp.basicdraw, true },
                { comp.phaseorb, true },
            };

        public Vector2 accel;
        public float absaccel { get; set; }
        public float friction { get; set; }
        public CollisionDelegate onCollisionStay;
        public CollisionDelegate onCollisionEnter;
        public CollisionDelegate onCollisionExit;
        
        public float maxScore = 50000;
        int colorChange = 120;
        float smallmass = 2;
        float bigmass = 5;
        float tonymass = 10;
        public Node bigtony = null;
        public BigTony() : base()
        {
            room.game.ui.SetSidebarActive(false);
            onCollisionEnter = delegate(Node s, Node t)
            {
                if (t != null && !room.playerNodes.Contains(t))
                {
                    t.body.color = Add(s.body.color, new Color(colorChange, colorChange, colorChange));//changed node to s
                }
            };
            onCollisionExit = delegate(Node s, Node t)
            {
                if (t != null && !room.playerNodes.Contains(t))
                {
                    t.body.color = Color.White;
                }
            };
            accel = new Vector2(0, 0);
            absaccel = 0.2f;
            friction = 0.01f;

            
        }
        public void InitializePlayers()
        {
            for (int i = 0; i < 8; i++)
            {
                Player p = Player.GetNew(i);
                if (p == null) break;
                Vector2 spawnPos = Vector2.Zero;
                double angle = Utils.random.NextDouble() * Math.PI * 2;
                angle -= Math.PI;
                float dist = 200;
                float x = dist * (float)Math.Cos(angle);
                float y = dist * (float)Math.Sin(angle);
                spawnPos = new Vector2(room.worldWidth / 2, room.worldHeight / 2) - new Vector2(x, y);

                //add //{ nodeE.position, spawnPos },
                p.node = room.game.spawnNode(playerProps);
                p.node.name = "player" + p.playerIndex;

                p.node.Comp<Queuer>().queuecount = 100;

                Collider collider = new Collider(new Circle(25));
                p.node.collision.AddCollider("trigger", collider);
                collider.OnCollisionEnter += onCollisionEnter;
                collider.OnCollisionExit += onCollisionExit;
            }
        }
        
        public void SwitchPlayerNode(Node n1, Node n2)
        {
            if (n1.player == null && n2.player == null)
            {
                return;
            }
            else if (n1.player != null && n2.player != null)
            {
                BigTonyData data1 = n1.player.Data<BigTonyData>();
                BigTonyData data2 = n2.player.Data<BigTonyData>();
                if (data1 == null || data2 == null) return;
                if (!data1.switchAvailable || !data2.switchAvailable) return;
                n1.player.node.body.color = n2.player.node.body.color;
                Node temp = n1.player.node;
                n1.player.node.collision.SwapCollider(n2.player.node, "trigger");

                n1.player.node = n2.player.node;
                n1.player.node.body.color = n1.player.pColor;
                n2.player.node = temp;

                data1.switchAvailable = false;
                data2.switchAvailable = false;
                room.scheduler.doAfterXMilliseconds(nn => data1.switchAvailable = true, 1000);
                room.scheduler.doAfterXMilliseconds(nn => data2.switchAvailable = true, 1000);
            }
            else
            {
                Player p1;
                Node n;
                if (n1.player != null && n2.player == null)
                {
                    p1 = n1.player;
                    n = n2;
                }
                else if (n1.player == null && n2.player != null)
                {
                    p1 = n2.player;
                    n = n1;
                }
                else
                {
                    p1 = null;
                    n = null;
                }
                p1.node.body.color = Color.White;
                p1.node.body.texture = textures.whitecircle;
                if (p1.node != bigtony) p1.node.body.mass = smallmass;
                p1.node.collision.SwapCollider(n, "trigger");

                Collider col = p1.node.collision.GetCollider("trigger");
                if (col != null) col.HandlersEnabled = false;

                Collider col2 = n.collision.GetCollider("trigger");
                if (col2 != null) col2.HandlersEnabled = true;

                p1.node = n;
                if (p1.node != bigtony) p1.node.body.mass = bigmass;

                p1.node.body.texture = textures.blackorb;
                p1.node.body.color = p1.pColor;
                room.scheduler.doAfterXMilliseconds(nn => p1.Data<BigTonyData>().switchAvailable = true, 1000);
            }
            //don't switch if both are nodes
        }

        public void MakeBigTony(Room room)
        {
            //if (bigtony != null)
            //{
            //    return;
            //}
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
            room.scheduler.doEveryXMilliseconds(delegate
            {
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
            updateScores = (ooo, eee) =>
            {
                foreach (var p in room.players)
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
    }
}
