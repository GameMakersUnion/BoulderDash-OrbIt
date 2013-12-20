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

        private int _lowerbound = 20;
        public int lowerbound { get { return _lowerbound; } set { _lowerbound = value; } }

        private bool _constant = false;
        public bool constant { get { return _constant; } set { _constant = value; } }

        public Gravity() : this(null) { }
        public Gravity(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.gravity; 
            methods = mtypes.affectother;
            
        }




        public override void Initialize(Node parent)
        {
            this.parent = parent;
        }

        public override void AffectOther(Node other)
        {

            if (!active)
            {
                return;
            }
            if (exclusions.Contains(other)) return;

            if (!other.comps.ContainsKey(comp.gravity)) return; //controversial: what the fuck do we do

            //assuming other has been checked for 'active' from caller
            float distVects = Vector2.Distance(other.transform.position, parent.transform.position);

            // INSTEAD of checking all distances between all nodes (expensive; needs to take roots)
            // do this: find all nodes within the SQUARE (radius = width/2), then find distances to remove nodes in the corners of square
            if (distVects < radius)
            {
                //Console.WriteLine("YEP");
                if (distVects < lowerbound) distVects = lowerbound;
                //if (distVects < (parent.transform.mass * 10)) distVects = (parent.transform.mass * 10);
                double angle = Math.Atan2((parent.transform.position.Y - other.transform.position.Y), (parent.transform.position.X - other.transform.position.X));
                //float counterforce = 100 / distVects;
                float counterforce = 1;
                //float gravForce = multiplier / (distVects * distVects * counterforce);
                float gravForce = (multiplier * parent.transform.mass * other.transform.mass) / (distVects * distVects * counterforce);
                //float gravForce = gnode1.GravMultiplier;
                float velX = (float)Math.Cos(angle) * gravForce;
                float velY = (float)Math.Sin(angle) * gravForce;
                Vector2 delta = new Vector2(velX, velY);
                
                /*
                delta /= other.transform.mass;
                other.transform.velocity += delta;
                //*/
                //*
                delta /= 2;

                if (constant)
                {
                    other.transform.velocity = delta / other.transform.mass;
                    parent.transform.velocity = -delta / parent.transform.mass;
                }
                else
                {
                    other.transform.velocity += delta / other.transform.mass;
                    parent.transform.velocity -= delta / parent.transform.mass;
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
