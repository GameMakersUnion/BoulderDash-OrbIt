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

        //public Node parent;
        //public bool active = true;
        private float _multiplier = 0.1f;
        public float multiplier { get { return _multiplier; } set { _multiplier = value; } }

        private float _radius = 300f;
        public float radius { get { return _radius; } set { _radius = value; } }

        public Node targetPuller;

        //in base class:
        //public Dictionary<properties, dynamic> compProps = new Dictionary<properties, dynamic>();
        //public Node parent;

        public LinearPull() { com = comp.linearpull; }
        public LinearPull(Node parent)
        {
            //never be called
            this.parent = parent;
            this.com = comp.linearpull;
        }
        public LinearPull(Node parent, Node targetPuller)
        {
            this.parent = parent;
            this.com = comp.linearpull;
            this.targetPuller = targetPuller;
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

        /*
         * bool active = node.props[properties.grav_active] ?? defaultCompProps[properties.grav_active];
            float multiplier = node.props[properties.grav_multiplier] ?? defaultCompProps[properties.grav_multiplier];
            float radius = node.props[properties.grav_radius] ?? defaultCompProps[properties.grav_radius];
         */

        public override void AffectOther(Node other)
        {
            if (!active)
            {
                //Console.WriteLine("This should have been removed from the delegates list.");
                return;
            }

            //assuming other has been checked for 'active' from caller
            //and assuming this has been checked: if (!(gameobject == this))
            float distVects = Vector2.Distance(other.position, parent.position);

            //distVects = distVects - parent.Radius - other.Radius;
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
            //do stuff (actually nope; gravity doesn't have this method)
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            //it would be really cool to have some kind of blending effects so that every combination of components will look diff
            //spritebatch.Draw(parent.props[properties.core_texture], parent.props[properties.core_position], Color.White);
            spritebatch.Draw(parent.getTexture(), parent.position, null, Color.White, 0, parent.TextureCenter(), 1f, SpriteEffects.None, 0);

        }
    }
}
