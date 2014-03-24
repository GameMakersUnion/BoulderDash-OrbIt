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
        public const mtypes CompType = mtypes.affectother | mtypes.minordraw;
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
        [Info(UserLevel.Advanced, "If enabled, this node not only pulls or pushes other nodes; It itself is pushed and pulled by the nodes it's affecting.")]
        public bool AffectBoth { get; set; }
        /// <summary>
        /// Turns the gravity up to 11.
        /// </summary>
        [Info(UserLevel.Advanced, "Turns the gravity up to 11.")]
        public bool StrongGravity { get; set; }
        /// <summary>
        /// Adds an angle to the gravitational pull
        /// </summary>
        [Info(UserLevel.User, "Adds an angle to the gravitational pull")]
        public int angle { get; set; }
        /// <summary>
        /// The mode that gravity will operate under.
        /// Normal: The normal gravity strength.
        /// Strong: The gravity strength is much stronger; it is squared.
        /// ConstantForce: Applies the same gravity force, regardless of the target's distance from this node. 
        /// </summary>
        [Info(UserLevel.User, "The mode that gravity will operate under.")]
        public Mode mode { get; set; }
        /// <summary>
        /// Causes the node to repulse other nodes, pushing them away.
        /// </summary>
        [Info(UserLevel.User, "Causes the node to repulse other nodes, pushing them away.")]
        public bool Repulsive { get; set; }

        public enum Mode
        {
            Normal,
            Strong,
            ConstantForce,
        }
        private float drawscale;
        public Gravity() : this(null) { }
        public Gravity(Node parent)
        {
            if (parent != null) this.parent = parent;
            com = comp.gravity; 
            multiplier = 100f;
            radius = 800f;
            lowerbound = 20;
            mode = Mode.Normal;
            Repulsive = false;
        }

        //public bool EveryOther = false;
        //public int counter = 0;

        public override void AffectOther(Node other)
        {
            if (!active) { return; }
            if (exclusions.Contains(other)) return;

            //if (EveryOther && counter++ % 2 == 0) return;

            if (AffectsOnlyGravity && !other.comps.ContainsKey(comp.gravity)) return;

            float distVects = Vector2.DistanceSquared(other.body.pos, parent.body.pos);

            if (distVects < radius * radius)
            {
                distVects = (float)Math.Sqrt(distVects);
                if (distVects < lowerbound) distVects = lowerbound;
                double angle = Math.Atan2((parent.body.pos.Y - other.body.pos.Y), (parent.body.pos.X - other.body.pos.X));

                float gravForce = (multiplier * parent.body.mass * other.body.mass);

                switch (mode)
                {
                    case Mode.Normal:
                        gravForce /= distVects * distVects;
                        break;
                    case Mode.Strong:
                        gravForce /= distVects;
                        break;
                    case Mode.ConstantForce:
                        gravForce /= 100; //#magicnumber
                        break;
                }
                if (Repulsive) gravForce *= -1;

                if (angle != 0)
                    angle = (angle + Math.PI + (Math.PI * (float)(angle / 180.0f)) % (Math.PI * 2)) - Math.PI;

                //float gravForce = gnode1.GravMultiplier;
                float velX = (float)Math.Cos(angle) * gravForce;
                float velY = (float)Math.Sin(angle) * gravForce;
                Vector2 delta = new Vector2(velX, velY);
                
                /*
                delta /= other.transform.mass;
                other.transform.velocity += delta;
                //*/
                //*

                if (AffectBoth)
                {
                    delta /= 2;
                        other.body.velocity += delta * other.body.invmass;
                        parent.body.velocity -= delta * parent.body.invmass;
                    }
                    else
                    {
                        other.body.ApplyForce(delta);
                }
                //other.body.velocity += delta;
                //other.body.velocity /= other.body.mass; //creates snakelike effect when put below increments
            }
        }
        public override void AffectSelf()
        { 
            //do stuff (actually nope; gravity doesn't have this method)
        }
        
        public override void Draw(SpriteBatch spritebatch)
        {
            return;
            if (!Repulsive)
            {
                parent.room.camera.Draw(textures.ring, parent.body.pos, parent.body.color, drawscale / 50f);
                drawscale -= 10f;
                if (drawscale < 0) drawscale = radius;
            }
        }
    }
}
