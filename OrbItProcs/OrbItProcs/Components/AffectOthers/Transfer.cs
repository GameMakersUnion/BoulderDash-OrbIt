using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs.Components
{
    public class Transfer : Component, ILinkable
    {

        private float _radius = 25f;
        public float radius { get { return _radius; } set { _radius = value; } }

        public Transfer() : this(null) { }
        public Transfer(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.transfer; 
            methods = mtypes.affectother; 
        }

        public override void Initialize(Node parent)
        {
            this.parent = parent;
        }

        public override void AffectOther(Node other)
        {
            //if (!active) return;
            if (exclusions.Contains(other)) return;

            float distVects = Vector2.Distance(other.transform.position, parent.transform.position);
            if (distVects < radius)
            {
                float newX = (parent.transform.position.X - other.transform.position.X) * 2.05f;
                float newY = (parent.transform.position.Y - other.transform.position.Y) * 2.05f;
                other.transform.position.X += newX;
                other.transform.position.Y += newY;
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
