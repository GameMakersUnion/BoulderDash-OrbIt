using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs {
    public class Gravity : Component, ILinkable
    {
        public enum Mode
        {
            Normal,
            Strong,
            ConstantForce,
        }

        private Link _link = null;
        public Link link { get { return _link; } set { _link = value; } }
        private float _multiplier = 100f;
        public float multiplier { get { return _multiplier; } set { _multiplier = value; } }

        private float _radius = 800f;
        public float radius { get { return _radius; } set { _radius = value; } }

        private int _lowerbound = 20;
        public int lowerbound { get { return _lowerbound; } set { _lowerbound = value; } }

        private bool _AffectsOnlyGravity = false;
        public bool AffectsOnlyGravity { get { return _AffectsOnlyGravity; } set { _AffectsOnlyGravity = value; } }

        private bool _AffectBoth = false;
        public bool AffectBoth { get { return _AffectBoth; } set { _AffectBoth = value; } }

        public Mode mode { get; set; }



        private int _angledelta = 0;
        public int angledelta { get { return _angledelta; } set { _angledelta = value; } }

        public Gravity() : this(null) { }
        public Gravity(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.gravity; 
            methods = mtypes.affectother;
            mode = Mode.Normal;
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

                double gravForce = (multiplier * parent.body.mass * other.body.mass);

                switch (mode)
                {
                    case Mode.Normal:
                        gravForce /= distVects * distVects;
                        break;
                    case Mode.Strong:
                        gravForce /= distVects;
                        break;
                    case Mode.ConstantForce:
                        gravForce /= 20; //#magicnumber
                        break;
                }

                if (angledelta != 0)
                    angle = (angle + Math.PI + (Math.PI * (float)(angledelta / 180.0f)) % (Math.PI * 2)) - Math.PI;

                //float gravForce = gnode1.GravMultiplier;
                double velX = Math.Cos(angle) * gravForce;
                double velY = Math.Sin(angle) * gravForce;
                Vector2 delta = new Vector2((float)velX, (float)velY);
                
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
