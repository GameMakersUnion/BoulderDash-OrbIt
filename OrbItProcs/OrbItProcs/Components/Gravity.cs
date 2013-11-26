using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs.Components {
    public class Gravity : Component {

        private float _multiplier = 200f;
        public float multiplier { get { return _multiplier; } set { _multiplier = value; } }

        private float _radius = 300f;
        public float radius { get { return _radius; } set { _radius = value; } }


        public Gravity(Node parent)
        {
            this.parent = parent;
            this.com = comp.gravity;
        }


        public override void Initialize()
        {

        }

        public override bool hasMethod(string methodName)
        {
            methodName = methodName.ToLower();
            if (methodName.Equals("affectother")) return true;
            if (methodName.Equals("affectself")) return false;
            if (methodName.Equals("draw")) return true;
            else return false;
        }


        public override void AffectOther(Node other)
        {
            if (!active)
            {
                return;
            }
            //assuming other has been checked for 'active' from caller
            float distVects = Vector2.Distance(other.position, parent.position);

            // INSTEAD of checking all distances between all nodes (expensive; needs to take roots)
            // do this: find all nodes within the SQUARE (radius = width/2), then find distances to remove nodes in the corners of square
            if (distVects < radius)
            {
                //Console.WriteLine("YEP");
                if (distVects < 10) distVects = 10;
                //if (distVects < (parent.mass * 10)) distVects = (parent.mass * 10);
                double angle = Math.Atan2((parent.position.Y - other.position.Y), (parent.position.X - other.position.X));
                //float counterforce = 100 / distVects;
                float counterforce = 1;
                //float gravForce = multiplier / (distVects * distVects * counterforce);
                float gravForce = (multiplier * parent.mass * other.mass) / (distVects * distVects * counterforce);
                //float gravForce = gnode1.GravMultiplier;
                float velX = (float)Math.Cos(angle) * gravForce;
                float velY = (float)Math.Sin(angle) * gravForce;
                Vector2 delta = new Vector2(velX, velY);
                delta /= other.mass;
                other.velocity += delta;
                //other.velocity.X += velX;
                //other.velocity.Y += velY;
                //other.velocity /=  other.mass; //creates snakelike effect when put below increments
            }
        }
        public override void AffectSelf()
        { 
            //do stuff (actually nope; gravity doesn't have this method)
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            //it would be really cool to have some kind of blending effects so that every combination of components will look diff
            //spritebatch.Draw(parent.getTexture(), parent.props[properties.core_position], Color.White);
            spritebatch.Draw(parent.getTexture(), parent.position, null, Color.White, 0, parent.TextureCenter(), 1f, SpriteEffects.None, 0);

        }
    }
}
