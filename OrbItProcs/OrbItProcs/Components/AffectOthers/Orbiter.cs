using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs
{
    public struct OrbiterData
    {
        public float angle;
        public float angledelta;
        public float speed;
        public float radius;
        
        public OrbiterData(int randSpeed, int randRadius)
        {
            angle = Utils.random.Next(360) * (float)(Math.PI / 180);
            speed = Utils.random.Next(randSpeed) *0.1f;
            radius = Utils.random.Next(randRadius);
            angledelta = speed / radius;

        }


    }

    public class Orbiter : Component, ILinkable
    {
        private Link _link = null;
        public Link link { get { return _link; } set { _link = value; } }

        public Dictionary<Node, OrbiterData> _orbiterDatas = new Dictionary<Node,OrbiterData>();
        public Dictionary<Node, OrbiterData> orbiterDatas { get { return _orbiterDatas; } set { _orbiterDatas = value; } }

        private int _randRadius = 500;
        public int randRadius { get { return _randRadius; } set { _randRadius = value; } }
        private int _randSpeed = 50;
        public int randSpeed { get { return _randSpeed; } set { _randSpeed = value; } }

        private float _speedMult = 1f;
        public float speedMult { get { return _speedMult; } set { _speedMult = value; } }

        //private int _lowerbound = 20;
        //public int lowerbound { get { return _lowerbound; } set { _lowerbound = value; } }
        //private bool _IsLinear = false;
        //public bool IsLinear { get { return _IsLinear; } set { _IsLinear = value; } }

        public Orbiter() : this(null) { }
        public Orbiter(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.orbiter;
            methods = mtypes.affectother;

        }

        public override void AffectOther(Node other)
        {
            if (!active) { return; }
            if (exclusions.Contains(other)) return;

            if (link != null && link.formation != null && link.formation.AffectionSets.ContainsKey(parent))
            {
                foreach (Node n in link.formation.AffectionSets[parent])
                {
                    if (orbiterDatas.ContainsKey(n))
                    {
                        OrbiterData od = orbiterDatas[n];
                        od.angle += od.angledelta * speedMult;
                        if (od.angle > Math.PI)
                            od.angle = od.angle - 2 * (float)Math.PI;
                        else if (od.angle < -Math.PI)
                            od.angle = od.angle + 2 * (float)Math.PI;

                        //Console.WriteLine(od.angle + " : " + od.angledelta);

                        float x = od.radius * (float)Math.Cos(od.angle);
                        float y = od.radius * (float)Math.Sin(od.angle);

                        //n.transform.position.X = (float)Math.Atan2(parent.transform.position.Y - y, parent.transform.position.X - x);
                        n.body.pos = new Vector2(parent.body.pos.X - x, parent.body.pos.Y - y);

                        orbiterDatas[n] = od;
                    }
                    else
                    {
                        orbiterDatas[n] = new OrbiterData(randSpeed, randRadius);
                    }
                }
            }

            /*
            float distVects = Vector2.Distance(other.transform.position, parent.transform.position);

            if (distVects < radius)
            {
                if (distVects < lowerbound) distVects = lowerbound;
                double angle = Math.Atan2((parent.transform.position.Y - other.transform.position.Y), (parent.transform.position.X - other.transform.position.X));
                //float counterforce = 100 / distVects;
                //float gravForce = multiplier / (distVects * distVects * counterforce);
                //Console.WriteLine(angle);



                //float gravForce = (multiplier * parent.transform.mass * other.transform.mass) / (distVects * distVects * counterforce);
                float gravForce;
                if (IsLinear) gravForce = pushfactor;// * 10;
                else gravForce = (pushfactor * parent.transform.mass * other.transform.mass) / (distVects);

                if (angledelta != 0)
                    angle = (angle + Math.PI + (Math.PI * (float)(angledelta / 180.0f)) % (Math.PI * 2)) - Math.PI;

                //float gravForce = gnode1.GravMultiplier;
                float velX = (float)Math.Cos(angle) * gravForce;
                float velY = (float)Math.Sin(angle) * gravForce;
                Vector2 delta = new Vector2(velX, velY);

                if (IsLinear) delta /= other.transform.mass;

                other.transform.position -= delta;

            }
            */
        }
        public override void AffectSelf()
        {
        }

        public override void Draw(SpriteBatch spritebatch)
        {

        }
    }
}
