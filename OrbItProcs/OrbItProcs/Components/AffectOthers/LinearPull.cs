using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs
{
    public class LinearPull : Component, ILinkable
    {
        private Link _link = null;
        public Link link { get { return _link; } set { _link = value; } }
        private float _multiplier = 20f;
        public float multiplier { get { return _multiplier; } set { _multiplier = value; } }

        private float _radius = 1300f;
        public float radius { get { return _radius; } set { _radius = value; } }

        private bool _constant = false;
        public bool constant { get { return _constant; } set { _constant = value; } }

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
            if (!active) return;
            if (exclusions.Contains(other)) return;

            float distVects = Vector2.Distance(other.body.pos, parent.body.pos);

            //todo: if this component is on a link, radius is not checked
            if (distVects < radius)
            {
                double angle = Math.Atan2((parent.body.pos.Y - other.body.pos.Y), (parent.body.pos.X - other.body.pos.X));

                float velX = (float)Math.Cos(angle) * multiplier;
                float velY = (float)Math.Sin(angle) * multiplier;
                Vector2 delta = new Vector2(velX, velY);
                //delta /= other.body.mass;
                if (constant)
                {
                    other.body.velocity = delta * other.body.invmass;
                    if (other.body.velocity.IsFucked()) System.Diagnostics.Debugger.Break();

                }
                else
                {
                    //other.body.velocity += delta;
                    other.body.ApplyForce(delta);
                    if (other.body.force.IsFucked()) System.Diagnostics.Debugger.Break();

                }
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
