using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OrbItProcs
{
    public class Player : Node
    {
        private static Dictionary<dynamic, dynamic> userP = new Dictionary<dynamic, dynamic>()
        {
            {node.position, new Vector2(0,0)},
            {node.texture, textures.blueorb},
            {comp.movement, true },
            {comp.basicdraw, true },
            {comp.maxvel, true },
        };

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

        public Player(Vector2 position) : base(Program.getRoom(), userP)
        {
            body.position = position;
            body.radius = 64;
            accel = new Vector2(0, 0);
            maxvel = 20f;
            maxaccel = 3f;
            absaccel = 0.1f;
            friction = 0.01f;
            QuickStop = false;

            var userP = new Dictionary<dynamic, dynamic>()
            {
                { node.texture, textures.whitecircle },
                { comp.movement, true },
                { comp.basicdraw, true },
                { comp.maxvel, true },
                { comp.laser, true },
                //{ comp.linearpull, true },
            };

            launchNode = new Node(room, userP);
            //launchNode.comps[comp.laser].lineXScale = 0.5f;
            launchNode.comps[comp.laser].lineYScale = 3f;
            launchNode.GetComponent<MaxVel>().maxvel = 15;
            bulletlife = 1500;
            firefreq = 1;

            room.game.ui.Keybindset.addProcessKeyAction("firebullet", KeyCodes.LeftClick, OnPress: FireNode);
            room.game.ui.Keybindset.addProcessKeyAction("firebullets", KeyCodes.RightClick, OnHold: FireNodes);

        }

        public override void Update(GameTime gametime)
        {
            base.Update(gametime);

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
                //if (accel.X > 0 && transform.velocity.X < 0) transform.velocity.X = 0;
                //else if (accel.X < 0 && transform.velocity.X > 0) transform.velocity.X = 0;
                //
                //if (accel.Y > 0 && transform.velocity.Y < 0) transform.velocity.Y = 0;
                //else if (accel.Y < 0 && transform.velocity.Y > 0) transform.velocity.Y = 0;

                //if (accel.X == 0 && accel.Y == 0 && (transform.velocity.X != 0 || transform.velocity.Y != 0))
                //{
                //    accel = transform.velocity;
                //    accel.Normalize();
                //    accel *= -friction;
                //}
                if ((body.velocity.X != 0 || body.velocity.Y != 0))
                {
                    //accel = transform.velocity;
                    //accel.Normalize();
                    //accel *= -friction;

                    accel += body.velocity * -friction;
                }

                //accel *= friction;
            }
            

            //if (transform.velocity.X == 0 || transform.velocity.Y == 0)
            //{
            //    accel *= 0;
            //}

            MaxVelocityUpdate();

            body.velocity += accel;

            if (UserInterface.keybState.IsKeyDown(Keys.Z))
            {
                body.velocity = new Vector2(0, 0);
                accel = body.velocity;
            }

            //if (UserInterface.mouseState.LeftButton == ButtonState.Pressed && UserInterface.oldMouseState.LeftButton == ButtonState.Released)
            //{
            //    FireNode();
            //}

        }

        public void FireNode()
        {
            Vector2 pos = UserInterface.WorldMousePos;
            Node newNode = new Node();
            Node.cloneObject(launchNode, newNode);
            newNode.body.velocity = pos - body.position;
            newNode.body.position = body.position + body.velocity * 5;
            room.game.spawnNode(newNode, lifetime: bulletlife);
        }

        public void FireNodes()
        {
            if (firefreq != 0)
            {
                if (firefreqCounter++ % firefreq != 0)
                    return;
            }
            //FireNode();

            Vector2 pos = UserInterface.WorldMousePos;
            Node newNode = new Node();
            Node.cloneObject(launchNode, newNode);
            newNode.body.velocity = pos - body.position;
            newNode.body.position = body.position + body.velocity * 5;

            if (UserInterface.keybState.IsKeyDown(Keys.LeftControl))
            {
                if (newNode.comps.ContainsKey(comp.maxvel))
                    newNode.comps[comp.maxvel].active = false;

            }
            room.game.spawnNode(newNode, lifetime: bulletlife);
        }

        public void MaxVelocityUpdate()
        {
            Node parent = this;
            
            if ((Math.Pow(parent.body.velocity.X, 2) + Math.Pow(parent.body.velocity.Y, 2)) > Math.Pow(maxvel, 2))
            {
                parent.body.velocity.Normalize();
                parent.body.velocity *= maxvel;

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
