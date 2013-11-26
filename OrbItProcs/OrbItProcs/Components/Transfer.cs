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

        public Transfer() { com = comp.transfer; }
        public Transfer(Node parent)
        {
            this.parent = parent;
            this.com = comp.transfer;

        }

        public override bool hasMethod(string methodName)
        {
            methodName = methodName.ToLower();
            if (methodName.Equals("affectother")) return true;
            if (methodName.Equals("affectself")) return false;
            if (methodName.Equals("draw")) return true;
            else return false;
        }

        public override void Initialize()
        {
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
            spritebatch.Draw(parent.getTexture(), parent.position, null, Color.White, 0, parent.TextureCenter(), 1f, SpriteEffects.None, 0);
        }
    }
}
