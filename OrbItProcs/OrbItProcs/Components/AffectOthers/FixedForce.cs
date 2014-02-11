﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs
{
    public class FixedForce : Component, ILinkable
    {
        private Link _link = null;
        public Link link { get { return _link; } set { _link = value; } }

        //public int gridx = 0, gridy = 0;

        //public override bool active
        //{
        //    get { return _active; }
        //    set
        //    {
        //        if (parent != null)
        //        {
        //            if (!_active && value && parent != parent.room.game.ui.sidebar.ActiveDefaultNode)
        //            {
        //            }
        //            else if (_active && !value)
        //            {
        //            }
        //            _active = value;
        //            //Console.WriteLine(parent.name);
        //        }
        //    }
        //}

        public Vector2 _force = new Vector2(0, 1);
        public Vector2 force { get { return _force; } set { _force = value; } }

        public float x { get { return _force.X; } set { _force.X = value; } }
        public float y { get { return _force.Y; } set { _force.Y = value; } }

        private float _multiplier = 1f;
        public float multiplier { get { return _multiplier; } set { _multiplier = value; } }
        private float _terminal = 5f;
        public float terminal { get { return _terminal; } set { _terminal = value; } }

        public FixedForce() : this(null) { }
        public FixedForce(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.fixedforce;
            methods = mtypes.affectother;
        }
        public override void Initialize(Node parent)
        {
            this.parent = parent;
        }

        public override void AffectOther(Node other)
        {
            if (!active) { return; }

            //if ((!other.comps.ContainsKey(comp.collision) || !other.isCompActive(comp.collision))) { return; }

            //assuming other has been checked for 'active' from caller
            if (exclusions.Contains(other)) return;
            
            if (terminal > 0 && other.body.velocity.ProjectOnto(force).Length() < terminal)
                other.body.velocity += force * multiplier / 10f;

            

        }
        public override void AffectSelf()
        {
        }

        public override void Draw(SpriteBatch spritebatch)
        {
        }

        
    }
}