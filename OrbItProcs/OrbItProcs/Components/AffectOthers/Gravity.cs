using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs {
    /// <summary>
    /// Attracts or repels nodes that it affects.
    /// </summary>
    [Info(UserLevel.User, "Attracts or repels nodes that it affects.", mtypes.affectother)]
    public class Gravity : Component, ILinkable
    {
        public const mtypes CompType = mtypes.affectother;
        public override mtypes compType { get { return CompType; } set { } }
        [Info(UserLevel.Developer)]
        public Link link { get; set; }
        /// <summary>
        /// Strength of gravity, use negative to repel.
        /// </summary>
        [Info(UserLevel.User, "Strength of gravity, use negative to repel.")]
        public float multiplier { get; set; }
        /// <summary>
        /// Distance at which other nodes are attracted/repelled from this node
        /// </summary>
        [Info(UserLevel.Advanced, "Distance at which other nodes are attracted/repelled from this node")]
        public float radius { get ; set ; }
        /// <summary>
        /// Represents minimum distance taken into account when calculating grav force strength.
        /// </summary>
        [Info(UserLevel.Advanced, "Represents minimum distance taken into account when calculating grav force strength.")]
        public int lowerbound { get; set; }
        /// <summary>
        /// If enabled, gravity strength is constant regardless of other nodes' distance;
        /// </summary>
        [Info(UserLevel.Advanced, "If enabled, gravity strength is constant regardless of other nodes' distance;")]
        public bool isConstant { get; set; }
        /// <summary>
        /// If enabled, this node only affects other nodes with a gravity component
        /// </summary>
        [Info(UserLevel.Advanced, "If enabled, this node only affects other nodes with a gravity component")]
        public bool AffectsOnlyGravity { get; set; }
        /// <summary>
        /// If enabled, this node not only pulls or pushes other nodes; It itself is pushed and pulled by the nodes it's affecting.
        /// </summary>
        [Info(UserLevel.User, "If enabled, this node not only pulls or pushes other nodes; It itself is pushed and pulled by the nodes it's affecting.")]
        public bool AffectBoth { get; set; }
        /// <summary>
        /// Turns the gravity up to 11.
        /// </summary>
        [Info(UserLevel.Advanced, "Turns the gravity up to 11.")]
        public bool StrongGravity { get; set; }
        /// <summary>
        /// Adds an angle to the gravitational pull
        /// </summary>
        [Info(UserLevel.Advanced, "Adds an angle to the gravitational pull")]
        public int angle { get; set; }

        public Gravity() : this(null) { }
        public Gravity(Node parent)
        {
            if (parent != null) this.parent = parent;
            com = comp.gravity;
            multiplier = 100f;
            radius = 800f;
            lowerbound = 20;
        }
        //public bool TestBool = false;

        public override void AffectOther(Node other)
        {
            if (!active) { return; }
            if (exclusions.Contains(other)) return;

            if (AffectsOnlyGravity && !other.comps.ContainsKey(comp.gravity)) return;

            //if (TestBool) return;


            float distVects = Vector2.DistanceSquared(other.body.pos, parent.body.pos);

            if (distVects < radius * radius)
            {
                distVects = (float)Math.Sqrt(distVects);
                if (distVects < lowerbound) distVects = lowerbound;
                double angle = Math.Atan2((parent.body.pos.Y - other.body.pos.Y), (parent.body.pos.X - other.body.pos.X));
                //float counterforce = 100 / distVects;
                //float gravForce = multiplier / (distVects * distVects * counterforce);

                //float gravForce = (multiplier * parent.transform.mass * other.transform.mass) / (distVects * distVects * counterforce);
                double gravForce = (multiplier * parent.body.mass * other.body.mass) / (distVects);

                if (!StrongGravity) gravForce /= distVects;

                if (angle != 0)
                    angle = (angle + Math.PI + (Math.PI * (float)(angle / 180.0f)) % (Math.PI * 2)) - Math.PI;

                //float gravForce = gnode1.GravMultiplier;
                double velX = Math.Cos(angle) * gravForce;
                double velY = Math.Sin(angle) * gravForce;
                Vector2 delta = new Vector2((float)velX, (float)velY);
                
                /*
                delta /= other.transform.mass;
                other.transform.velocity += delta;
                //*/
                //*

                if (AffectBoth)
                {
                    delta /= 2;

                    if (isConstant)
                    {
                        other.body.velocity = delta * other.body.invmass;
                        parent.body.velocity = -delta * parent.body.invmass;
                        if (other.body.velocity.IsFucked()) System.Diagnostics.Debugger.Break();
                        if (parent.body.velocity.IsFucked()) System.Diagnostics.Debugger.Break();

                    }
                    else
                    {
                        other.body.velocity += delta * other.body.invmass;
                        parent.body.velocity -= delta * parent.body.invmass;
                        if (other.body.velocity.IsFucked()) System.Diagnostics.Debugger.Break();
                        if (parent.body.velocity.IsFucked()) System.Diagnostics.Debugger.Break();
                    }
                }
                else
                {
                    //delta /= 2;
                    if (isConstant)
                    {
                        other.body.velocity = delta * parent.body.invmass;
                        if (parent.body.velocity.IsFucked()) System.Diagnostics.Debugger.Break();
                    }
                    else
                    {
                        //other.body.velocity += delta / other.body.mass;
                        other.body.ApplyForce(delta);
                        if (other.body.force.IsFucked()) System.Diagnostics.Debugger.Break();

                    }
                }
                
                
                //*/
                //other.transform.velocity.X += velX;
                //other.transform.velocity.Y += velY;
                //other.transform.velocity /=  other.transform.mass; //creates snakelike effect when put below increments
            }
        }
        public override void AffectSelf()
        { 
            //do stuff (actually nope; gravity doesn't have this method)
        }

        public override void Draw(SpriteBatch spritebatch)
        {

        }
    }
}
