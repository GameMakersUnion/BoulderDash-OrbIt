using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs.Components
{
    public class LinearPull : Component
    {
        private float _multiplier = 0.1f;
        public float multiplier { get { return _multiplier; } set { _multiplier = value; } }

        private float _radius = 300f;
        public float radius { get { return _radius; } set { _radius = value; } }

        public Node targetPuller;

        public LinearPull() : this(null) { }
        public LinearPull(Node parent = null, Node targetPuller = null)
        {
            if (parent != null)
            {
                this.parent = parent;
            }
            if (targetPuller != null)
            {
                this.targetPuller = targetPuller;
            }
            com = comp.linearpull; 
            methods = mtypes.affectother; 
        }


        public override void Initialize(Node parent)
        {
            this.parent = parent;
        }


        public override void AffectOther(Node other)
        {

            //assuming other has been checked for 'active' from caller
            //and assuming this has been checked: if (!(gameobject == this))
            float distVects = Vector2.Distance(other.position, parent.position);

            //distVects = distVects - parent.radius - other.radius;
            // INSTEAD of checking all distances between all nodes (expensive; needs to take roots)
            // do this: find all nodes within the SQUARE (radius = width/2), then find distances to remove nodes in the corners of square
            if (distVects < radius)
            {
                //Console.WriteLine("YEP");
                //if (distVects < 10) distVects = 10;
                //if (distVects < (parent.mass * 10)) distVects = (parent.mass * 10);
                double angle = Math.Atan2((parent.position.Y - other.position.Y), (parent.position.X - other.position.X));
                //float counterforce = 100 / distVects;
                //float counterforce = 1;
                //float gravForce = multiplier / (distVects * distVects * counterforce);
                //float gravForce = (multiplier * parent.mass * other.mass) / (distVects * distVects * counterforce);
                //float gravForce = gnode1.GravMultiplier;
                float velX = (float)Math.Cos(angle) * multiplier;
                float velY = (float)Math.Sin(angle) * multiplier;
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
        }

        public override void Draw(SpriteBatch spritebatch)
        {

        }
    }
}
