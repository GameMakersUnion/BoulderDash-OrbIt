using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs
{
    /// <summary>
    /// When another node enters the radius of this node, it gets teleported to the opposite side of this node, relative to this node's origin. 
    /// </summary>
    [Info(UserLevel.User, "When another node enters the radius of this node, it gets teleported to the opposite side of this node, relative to this node's origin. ", CompType)]
    public class Transfer : Component, ILinkable
    {
        public const mtypes CompType = mtypes.affectother;
        public override mtypes compType { get { return CompType; } set { } }
        public Link link { get; set; }
        /// <summary>
        /// Distance at which the node is teleported, based on a scale of the node's radius.
        /// </summary>
        [Info(UserLevel.User, "Distance at which the node is teleported, based on a scale of the node's radius.")]
        public float radiusScale { get; set; }

        public Transfer() : this(null) { }
        public Transfer(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.transfer;
            radiusScale = 2f;
        }

        public override void AffectOther(Node other)
        {
            if (!active) return;
            if (exclusions.Contains(other)) return;

            float distVects = Vector2.Distance(other.body.pos, parent.body.pos);
            float r = parent.body.radius * radiusScale;
            if (distVects < r)
            {
                float newX = (parent.body.pos.X - other.body.pos.X) * 2.05f;
                float newY = (parent.body.pos.Y - other.body.pos.Y) * 2.05f;
                other.body.pos.X += newX;
                other.body.pos.Y += newY;
            }
        }
        public override void AffectSelf()
        {
        }

        public override void Draw()
        {
        }
    }
}
