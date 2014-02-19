using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs
{
    public class Transfer : Component, ILinkable
    {
        private Link _link = null;
        public Link link { get { return _link; } set { _link = value; } }
        private float _radius = 25f;
        public float radius { get { return _radius; } set { _radius = value; } }

        public Transfer() : this(null) { }
        public Transfer(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.transfer; 
            methods = mtypes.affectother; 
        }

        public override void AffectOther(Node other)
        {
            if (!active) return;
            if (exclusions.Contains(other)) return;

            float distVects = Vector2.Distance(other.body.pos, parent.body.pos);
            if (distVects < radius)
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

        public override void Draw(SpriteBatch spritebatch)
        {
        }
    }
}
