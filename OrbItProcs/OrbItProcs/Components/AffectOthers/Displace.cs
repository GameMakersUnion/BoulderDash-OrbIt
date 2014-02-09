using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs
{
    public class Displace : Component, ILinkable
    {
        private Link _link = null;
        public Link link { get { return _link; } set { _link = value; } }
        //private float _multiplier = 100f;
        //public float multiplier { get { return _multiplier; } set { _multiplier = value; } }

        private float _radius = 800f;
        public float radius { get { return _radius; } set { _radius = value; } }

        private int _lowerbound = 20;
        public int lowerbound { get { return _lowerbound; } set { _lowerbound = value; } }

        private float _pushfactor = 10f;
        public float pushfactor { get { return _pushfactor; } set { _pushfactor = value; } }

        private int _angledelta = 0;
        public int angledelta { get { return _angledelta; } set { _angledelta = value; } }

        private bool _IsLinear = false;
        public bool IsLinear { get { return _IsLinear; } set { _IsLinear = value; } }

        //private bool _constant = false;
        //public bool constant { get { return _constant; } set { _constant = value; } }
        //
        //private bool _AffectsOnlyGravity = false;
        //public bool AffectsOnlyGravity { get { return _AffectsOnlyGravity; } set { _AffectsOnlyGravity = value; } }
        //
        //private bool _AffectBoth = false;
        //public bool AffectBoth { get { return _AffectBoth; } set { _AffectBoth = value; } }
        //
        //private bool _StrongGravity = false;
        //public bool StrongGravity { get { return _StrongGravity; } set { _StrongGravity = value; } }

        public Displace() : this(null) { }
        public Displace(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.displace;
            methods = mtypes.affectother;

        }




        public override void Initialize(Node parent)
        {
            this.parent = parent;
        }

        public override void AffectOther(Node other)
        {
            if (!active) { return; }
            if (exclusions.Contains(other)) return;


            float distVects = Vector2.Distance(other.body.position, parent.body.position);

            if (distVects < radius)
            {
                if (distVects < lowerbound) distVects = lowerbound;
                double angle = Math.Atan2((parent.body.position.Y - other.body.position.Y), (parent.body.position.X - other.body.position.X));
                //float counterforce = 100 / distVects;
                //float gravForce = multiplier / (distVects * distVects * counterforce);
                //Console.WriteLine(angle);

                

                //float gravForce = (multiplier * parent.transform.mass * other.transform.mass) / (distVects * distVects * counterforce);
                float gravForce;
                if (IsLinear) gravForce = pushfactor;// * 10;
                else gravForce = (pushfactor * parent.body.mass * other.body.mass) / (distVects);

                if (angledelta != 0)
                    angle = (angle + Math.PI + (Math.PI * (float)(angledelta / 180.0f)) % (Math.PI * 2)) - Math.PI;

                //float gravForce = gnode1.GravMultiplier;
                float velX = (float)Math.Cos(angle) * gravForce;
                float velY = (float)Math.Sin(angle) * gravForce;
                Vector2 delta = new Vector2(velX, velY);

                if (IsLinear) delta /= other.body.mass;

                other.body.position -= delta;

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
