using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs.Components
{
    public class Transfer : Component{

        public float _radius = 25f;
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

            float distVects = Vector2.Distance(other.position, parent.position);
            if (distVects < radius)
            {
                float newX = (parent.position.X - other.position.X) * 2.05f;
                float newY = (parent.position.Y - other.position.Y) * 2.05f;
                other.position.X += newX;
                other.position.Y += newY;

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
