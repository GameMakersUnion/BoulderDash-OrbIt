﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace OrbItProcs
{
    public enum movemode
    {
        free,
        wallbounce,
        screenwrap,
        falloff,
        halt,
    };
    /// <summary>
    /// Basic Movement Component
    /// </summary>
    [Info(UserLevel.User, "Basic Movement Component", CompType)]
    public class Movement : Component {

        public const mtypes CompType = mtypes.essential | mtypes.playercontrol;// | mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }
        public bool pushable { get; set; }

        private Toggle<float> _maxVelocity = new Toggle<float>(30f, true);
        private Toggle<float> _minVelocity = new Toggle<float>(0f, false);
        /// <summary>
        /// If enabled, this limits the node's speed to stay below the specified velocity.
        /// </summary>
        [Info(UserLevel.User, "If enabled, this limits the node's velocity to stay below the specified velocity.")]
        public Toggle<float> maxVelocity { get { return _maxVelocity; } set { _maxVelocity = value; if (maxVelocity < _minVelocity) _maxVelocity = _minVelocity; } }
        /// <summary>
        /// If enabled, this limits the node's velocity to stay above the specified velocity.
        /// </summary>
        [Info(UserLevel.User, "If enabled, this limits the node's velocity to stay above the specified velocity.")]
        public Toggle<float> minVelocity { get { return _minVelocity; } set { _minVelocity = value; if (_minVelocity > _maxVelocity) _minVelocity = _maxVelocity; } }
        /// <summary>
        /// Gives the node this velocity in a random direction when spawned.
        /// </summary>
        [Info(UserLevel.User, "If enabled, gives the node this velocity in a random direction when spawned.")]
        public Toggle<float> randInitialVel { get; set; }



        private movemode _mode = movemode.wallbounce;
        /// <summary>
        /// How the screen's Limits affect this wall:
        /// Free: the restraints won't affect the wall.
        /// Wallbounce: the node's velocity is inverted when they hit the wall,
        /// Screenwrap: the node appears at the opposite wall,
        /// Falloff: the node is deleted when it exits the screen,
        /// Halt the node stops upon exiting the screen,
        /// </summary>
        [Info(UserLevel.User, "How the screen's Limits affect this wall: \n Free: the restraints won't affect the wall. \n Wallbounce: the node's velocity is inverted when they hit the wall, \n Screenwrap: the node appears at the opposite wall, \n Falloff: the node is deleted when it exits the screen, \n Halt the node stops upon exiting the screen,")]
        public movemode mode { get { return _mode; } set { _mode = value; } }

        public Movement() : this(null) { }
        public Movement(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.movement;
            randInitialVel = new Toggle<float>(8f);
            pushable = true;
        }
        public override void OnSpawn()
        {
            if (randInitialVel) RandomizeVelocity();
            moderateVelocity();
        }
        private void IntegrateForces()
        {
            if (!active) return;
            if (parent.body.invmass == 0)
                return;

            Body b = parent.body;
            b.velocity += VMath.MultVectDouble(b.force, b.invmass); //* dt / 2.0;
            b.angularVelocity += b.torque * b.invinertia; // * dt / 2.0;

        }
        public void IntegrateVelocity()
        {
            if (!active) return;
            if (parent.body.invmass == 0)
            {
                parent.body.velocity = Vector2.Zero;
                return;
            }

            moderateVelocity();
            Body b = parent.body;
            b.pos += b.velocity;
            b.orient += b.angularVelocity;
            b.orient =(b.orient);
            IntegrateForces(); //calls the integrateforces method

            AffectSelf();
        }

        [Clickable]
        public void moderateVelocity()
        {
            double velSquared = parent.body.velocity.X * parent.body.velocity.X + parent.body.velocity.Y * parent.body.velocity.Y;

            if (minVelocity.enabled && velSquared < minVelocity * minVelocity)
            {
                VMath.NormalizeSafe(ref parent.body.velocity);
                parent.body.velocity *= minVelocity;
            }
            if (maxVelocity.enabled && velSquared > maxVelocity * maxVelocity)
            {
                VMath.NormalizeSafe(ref parent.body.velocity);
                parent.body.velocity *= maxVelocity;
            }
        }
        public void RandomizeVelocity()
        {
            float x = ((float)Utils.random.NextDouble() * 100) - 50;
            float y = ((float)Utils.random.NextDouble() * 100) - 50;
            Vector2 vel = new Vector2(x, y);
            VMath.NormalizeSafe(ref vel);
            //vel.Normalize();
            vel = vel * randInitialVel;
            parent.body.velocity = vel;
        }
        [Clickable]
        public void scaleVelocity()
        {
            if (parent.body.velocity.X != 0 && parent.body.velocity.Y != 0)
            {
                VMath.NormalizeSafe(ref parent.body.velocity);
                parent.body.velocity *= randInitialVel;
            }
        }

        public override void AffectSelf()
        {
            


            //parent.body.position.X += parent.body.velocity.X * VelocityModifier;
            //parent.body.position.Y += parent.body.velocity.Y * VelocityModifier;


            //return;
            if (mode == movemode.screenwrap) screenWrap();
            if (mode == movemode.wallbounce) wallBounce();
            if (mode == movemode.falloff)    fallOff();
            if (mode == movemode.halt) halt();

            GraphData.AddFloat(parent.body.pos.X);

            //Trippy();
        }

        public void Trippy()
        {
            //test (holy SHIT that looks cool)
            PropertyInfo pi = parent.body.GetType().GetProperty("scale");
            pi.SetValue(parent.body, parent.body.pos.X % 4.0f, null);
        }
        public float absaccel = 0.2f;
        public float friction = 0.01f;
        private float v = 0.0f;

        public override void PlayerControl(Controller controller)
        {

            Vector2 stick = controller.GetLeftStick();
            Vector2 stick2 = controller.GetRightStick();
            
                //if (node != bigtony) node.collision.colliders["trigger"].radius = body.radius * 1.5f;
                //else node.collision.colliders["trigger"].radius = body.radius * 1.2f;
                //bool clicked = false;
                //clicked = hc.newHalfPadState.Btn3 == ButtonState.Pressed || hc.newHalfPadState.Btn1 == ButtonState.Pressed;
                //
                //if (clicked)
                //{
                //    SwitchPlayer(stick);
                //}

            
            if (stick2.LengthSquared() > 0.6f * 0.6f)
            {
                v = Utils.VectorToAngle(stick2).between0and2pi();
                if (v == 0f) v = 0.00001f;
            }
            else if (stick.LengthSquared() > 0.6f * 0.6f)
            {
                v = Utils.VectorToAngle(stick).between0and2pi();
                if (v == 0f) v = 0.00001f;
            }
            float result = Utils.AngleLerp(parent.body.orient, v, 0.1f);

            parent.body.orient =(result);

            stick *= 0.4f;
            stick *= absaccel;
            if ((parent.body.velocity.X != 0 || parent.body.velocity.Y != 0))
            {
                stick += parent.body.velocity * -friction;
            }
            stick *= parent.body.mass;
            //todo: update maxvel?
            parent.body.ApplyForce(stick);


        }
        //reminder: make a vocal recognition extension for visual studio to take you where you want ("Class: Movement. Method: fallOff.")
        public void fallOff()
        {
            int levelwidth = parent.room.worldWidth;
            int levelheight = parent.room.worldHeight;

            Vector2 pos = parent.body.pos;

            if (parent.HasComp<Queuer>() && (parent.Comp<Queuer>().qs & queues.position) == queues.position)
            {
                Queuer q = parent.Comp<Queuer>();
                Queue<Vector2> positions = ((Queue<Vector2>)(parent.Comp<Queuer>().positions));
                pos = positions.ElementAt(0);
            }

            if (pos.X >= (levelwidth + parent.body.radius))
            {
                parent.IsDeleted = true;
            }
            else if (pos.X < parent.body.radius * -1)
            {
                parent.IsDeleted = true;
            }

            if (pos.Y >= (levelheight + parent.body.radius))
            {
                parent.IsDeleted = true;
            }
            else if (pos.Y < parent.body.radius * -1)
            {
                parent.IsDeleted = true;
            }
        }

        public void checkForQueue()
        {

        }

        public void wallBounce()
        {
            if (OrbIt.gameMode == "SpiderDiggers")
            {
                 SpiderBounce();
                 return;
            }
            //if (room.PropertiesDict["wallBounce"])
            //float levelwidth = room.game...;
            int levelwidth = parent.room.worldWidth;
            int levelheight = parent.room.worldHeight;

            if (parent.body.pos.X >= (levelwidth - parent.body.radius))
            {
                //float off = parent.body.pos.X - (levelwidth - parent.body.radius);
                //parent.body.pos.X = (levelwidth - parent.body.radius - off) % parent.room.worldWidth;
                parent.body.pos.X = DelegateManager.Triangle(parent.body.pos.X, parent.room.worldWidth - (int)parent.body.radius);
                parent.body.velocity.X *= -1;
                parent.body.InvokeOnCollisionStay(null); //todo: find out why we needed null, fix this

            }
            if (parent.body.pos.X < parent.body.radius)
            {
                //float off = parent.body.radius - parent.body.pos.X;
                //parent.body.pos.X = (parent.body.radius + off) % parent.room.worldWidth;
                parent.body.pos.X = DelegateManager.Triangle(parent.body.pos.X - parent.body.radius, parent.room.worldWidth) + parent.body.radius;
                parent.body.velocity.X *= -1;
                parent.body.InvokeOnCollisionStay(null);
            }
            if (parent.body.pos.Y >= (levelheight - parent.body.radius))
            {
                //float off = parent.body.pos.Y - (levelheight - parent.body.radius);
                //parent.body.pos.Y = (levelheight - parent.body.radius - off) % parent.room.worldHeight;
                parent.body.pos.Y = DelegateManager.Triangle(parent.body.pos.Y, parent.room.worldHeight - (int)parent.body.radius);
                parent.body.velocity.Y *= -1;
                parent.body.InvokeOnCollisionStay(null);
            }
            if (parent.body.pos.Y < parent.body.radius)
            {
                //float off = parent.body.radius - parent.body.pos.Y;
                //parent.body.pos.Y = (parent.body.radius + off) % parent.room.worldHeight;
                parent.body.pos.Y = DelegateManager.Triangle(parent.body.pos.Y - parent.body.radius, parent.room.worldHeight) + parent.body.radius;
                parent.body.velocity.Y *= -1;
                parent.body.InvokeOnCollisionStay(null);
            }

            
        }
        public static bool WinSpiderGame = false;
        private void SpiderBounce()
        {
            if (parent.body.pos.Y < parent.body.radius)
            {
                parent.body.pos.Y = DelegateManager.Triangle(parent.body.pos.Y - parent.body.radius, parent.room.worldHeight) + parent.body.radius;
                parent.body.velocity.Y *= -1;
                parent.body.InvokeOnCollisionStay(null);
                if (parent.IsPlayer)
                {
                    try
                    {
                        LoadLevelWindow.StaticLevel(parent.room.levelList.Dequeue());
                    }
                    catch(Exception e)
                    {
                        WinSpiderGame = true;
                    }
                }
                else
                {
                    //if (parent.texture == textures.boulder1)
                    //{
                    //    parent.texture = textures.boulderShine;
                    //    parent.collision.active = false;
                    //}
                }
            }

            int levelLeft = (int)parent.room.gridsystemAffect.position.X, levelTop = (int)parent.room.gridsystemAffect.position.Y;
            int levelwidth = parent.room.gridsystemAffect.gridWidth;
            int levelheight = parent.room.gridsystemAffect.gridHeight;

            if (parent.body.pos.X >= (levelLeft +levelwidth - parent.body.radius))
            {
                //float off = parent.body.pos.X - (levelwidth - parent.body.radius);
                //parent.body.pos.X = (levelwidth - parent.body.radius - off) % parent.room.worldWidth;
                parent.body.pos.X = DelegateManager.Triangle(parent.body.pos.X, levelwidth - (int)parent.body.radius)+ levelLeft;
                parent.body.velocity.X *= -1;
                parent.body.InvokeOnCollisionStay(null); //todo: find out why we needed null, fix this

            }
            else if (parent.body.pos.X < levelLeft + parent.body.radius)
            {
                //float off = parent.body.radius - parent.body.pos.X;
                //parent.body.pos.X = (parent.body.radius + off) % parent.room.worldWidth;
                parent.body.pos.X = DelegateManager.Triangle(parent.body.pos.X - parent.body.radius, levelwidth) + parent.body.radius + levelLeft;
                parent.body.velocity.X *= -1;
                parent.body.InvokeOnCollisionStay(null);
            }
            else if (parent.body.pos.Y < levelTop + parent.body.radius)
            {
                if (parent.body.pos.Y > levelTop + parent.body.radius - 100)
                {
                    parent.body.velocity.Y *= -1;
                }
                else return;
                //parent.body.pos.Y = DelegateManager.Triangle(parent.body.pos.Y - levelTop - parent.body.radius, levelheight) + parent.body.radius + levelTop;
                //parent.body.velocity.Y *= -1;
                //parent.body.InvokeOnCollisionStay(null);
                //parent.body.pos.Y += 5;
            }
            else if (!parent.IsPlayer && parent.body.texture == textures.boulder1)
            {
                float y = Assets.Spider.finalpos.Y;
                float distFromCenter = parent.body.pos.X - parent.room.gridsystemAffect.gridWidth / 2;
                distFromCenter = (float)Math.Abs(distFromCenter);
                float maxDistFromCenter = 120;

                float distFromSpiderhead = Vector2.Distance(parent.body.pos, Assets.Spider.spiderHead);
                if (distFromSpiderhead < 120 && !parent.room.loading)
                {
                    Assets.Spider.spiderPos -= 10;
                    parent.texture = textures.boulderShine;
                    parent.collision.active = false;
                }

                if (!parent.room.loading && parent.body.pos.Y >= y + 200 && distFromCenter > maxDistFromCenter)
                {
                    parent.texture = textures.boulderShine;
                    parent.collision.active = false;
                }

                if (parent.body.pos.Y >= (levelTop + levelheight - parent.body.radius))
                {
                    parent.OnDeath(null);

                }
            }

            if (parent.IsPlayer)
            {
                float y = Assets.Spider.finalpos.Y;
                float distFromCenter = parent.body.pos.X - parent.room.gridsystemAffect.gridWidth / 2;
                int sign = -1;
                if (distFromCenter < 0) sign = 1;
                distFromCenter = (float)Math.Abs(distFromCenter);
                float maxDistFromCenter = 120;
                if (!parent.room.loading && parent.body.pos.Y >= y + 200 && distFromCenter > maxDistFromCenter)
                {
                    parent.movement.maxVelocity.value = 30f;
                    parent.body.velocity = new Vector2(20 * sign, 0);
                    CollisionDelegate callback = null;
                    callback = delegate(Node n1, Node n2)
                    {
                        parent.movement.maxVelocity.value = 2;
                        parent.body.OnCollisionEnter -= callback;
                    };
                    parent.body.OnCollisionEnter += callback;
                }
            }
            
        }

        public void halt()
        {
            //if (room.PropertiesDict["wallBounce"])
            //float levelwidth = room.game...;
            int levelwidth = parent.room.worldWidth;
            int levelheight = parent.room.worldHeight;

            if (parent.body.pos.X >= (levelwidth - parent.body.radius))
            {
                parent.body.pos.X = levelwidth - parent.body.radius;
                parent.body.velocity.X *= 0;
                parent.body.InvokeOnCollisionStay(null);

            }
            if (parent.body.pos.X < parent.body.radius)
            {
                parent.body.pos.X = parent.body.radius;
                parent.body.velocity.X *= 0;
                parent.body.InvokeOnCollisionStay(null);
            }
            if (parent.body.pos.Y >= (levelheight - parent.body.radius))
            {
                parent.body.pos.Y = levelheight - parent.body.radius;
                parent.body.velocity.Y *= 0;
                parent.body.InvokeOnCollisionStay(null);
            }
            if (parent.body.pos.Y < parent.body.radius)
            {
                parent.body.pos.Y = parent.body.radius;
                parent.body.velocity.Y *= 0;
                parent.body.InvokeOnCollisionStay(null);
            }


        }

        
        public void screenWrap()
        {
            //if (room.PropertiesDict["wallBounce"])
            //float levelwidth = room.game...;
            int levelwidth = parent.room.worldWidth;
            int levelheight = parent.room.worldHeight;

            //todo: modulus screen width
            //hitting top/bottom of screen
            //teleport node
            if (parent.body.pos.X >= levelwidth)
            {
                parent.body.pos.X = parent.body.pos.X - levelwidth;//1;
            }
            else if (parent.body.pos.X < 0)
            {
                parent.body.pos.X = levelwidth - parent.body.pos.X;//1;
            }
            //show half texture on other side
            if (parent.body.pos.X >= (levelwidth - parent.body.radius))
            {
                //
            }
            else if (parent.body.pos.X < parent.body.radius)
            {
                //
            }

            //hitting sides
            //teleport node
            if (parent.body.pos.Y >= levelheight)
            {
                parent.body.pos.Y = parent.body.pos.Y - levelheight;//1;
            }
            else if (parent.body.pos.Y < 0)
            {
                parent.body.pos.Y = levelheight - parent.body.pos.Y;//1;
            }
            //show half texture on other side
            if (parent.body.pos.Y >= (levelheight - parent.body.radius))
            {
                //
            }
            else if (parent.body.pos.Y < parent.body.radius)
            {
                //
            }



        }

    }
}
