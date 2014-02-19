﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs
{
    public class FieldGravity : Component, ILinkable
    {
        private Link _link = null;
        public Link link { get { return _link; } set { _link = value; } }
        private float _multiplier = 100f;
        public float multiplier { get { return _multiplier; } set { _multiplier = value; } }

        private float _radius = 800f;
        public float radius { get { return _radius; } set { _radius = value; } }

        private int _lowerbound = 20;
        public int lowerbound { get { return _lowerbound; } set { _lowerbound = value; } }

        private bool _constant = false;
        public bool constant { get { return _constant; } set { _constant = value; } }

        private bool _AffectsOnlyGravity = false;
        public bool AffectsOnlyGravity { get { return _AffectsOnlyGravity; } set { _AffectsOnlyGravity = value; } }

        private bool _AffectBoth = false;
        public bool AffectBoth { get { return _AffectBoth; } set { _AffectBoth = value; } }

        private bool _StrongGravity = false;
        public bool StrongGravity { get { return _StrongGravity; } set { _StrongGravity = value; } }

        public FieldGravity() : this(null) { }
        public FieldGravity(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.gravity;
            methods = mtypes.affectother;

        }

        public override void AffectOther(Node other)
        {
            if (!active) { return; }
            if (exclusions.Contains(other)) return;

            if (!other.comps.ContainsKey(comp.gravity) && AffectsOnlyGravity)
            {
                return;
            }

            double dd = Vector2.Distance(other.body.pos, parent.body.pos);

            float distVects = Vector2.Distance(other.body.pos, parent.body.pos);

            if (distVects < radius)
            {
                if (distVects < lowerbound) distVects = lowerbound;
                double angle = Math.Atan2((parent.body.pos.Y - other.body.pos.Y), (parent.body.pos.X - other.body.pos.X));
                //float counterforce = 100 / distVects;
                //float gravForce = multiplier / (distVects * distVects * counterforce);

                //float gravForce = (multiplier * parent.transform.mass * other.transform.mass) / (distVects * distVects * counterforce);

                float gravForce = (float)(multiplier * parent.body.mass * other.body.mass) / (distVects);

                if (!StrongGravity) gravForce /= distVects;


                //float gravForce = gnode1.GravMultiplier;
                float velX = (float)Math.Cos(angle) * gravForce;
                float velY = (float)Math.Sin(angle) * gravForce;
                Vector2 delta = new Vector2(velX, velY);

                /*
                delta /= other.transform.mass;
                other.transform.velocity += delta;
                //*/
                //*

                if (AffectBoth)
                {
                    delta /= 2;

                    if (constant)
                    {
                        other.body.velocity = delta * other.body.invmass;
                        parent.body.velocity = -delta * parent.body.invmass;
                        if (parent.body.velocity.IsFucked()) System.Diagnostics.Debugger.Break();
                        if (parent.body.velocity.IsFucked()) System.Diagnostics.Debugger.Break();
                    }
                    else
                    {
                        other.body.velocity += delta * other.body.invmass;
                        parent.body.velocity -= delta * parent.body.invmass;
                    }
                }
                else
                {
                    //delta /= 2;
                    if (constant)
                    {
                        other.body.velocity = delta * other.body.invmass;
                        if (other.body.velocity.IsFucked()) System.Diagnostics.Debugger.Break();

                    }
                    else
                    {
                        other.body.velocity += delta * other.body.invmass;
                        if (other.body.velocity.IsFucked()) System.Diagnostics.Debugger.Break();

                    }
                }


                //*/
                //other.transform.velocity.X += velX;
                //other.transform.velocity.Y += velY;
                //other.transform.velocity /=  other.transform.mass; //creates snakelike effect when put below increments
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
