using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;


namespace OrbItProcs
{
    public class Spring : Component, ILinkable
    {
        private Link _link = null;
        public Link link { get { return _link; } set { _link = value; } }
        new public bool active
        {
            get { return _active; }
            set
            {
                _active = value;
            }
        }

        private float _multiplier = 100f;
        public float multiplier { get { return _multiplier; } set { _multiplier = value; } }

        private bool _activated = false;
        public bool activated { get { return _activated; } set { _activated = value; } }

        private bool _hook = false;
        public bool hook { get { return _hook; } set { _hook = value; } }
        private bool _limit = false;
        public bool limit { get { return _limit; } set { _limit = value; } }

        public int _restdist = 300;
        public int restdist { get { return _restdist; } set { _restdist = value; if (_restdist < _elasticlimit) _restdist = _elasticlimit; } }
        public int _elasticlimit = 100;
        public int elasticlimit { get { return _elasticlimit; } set { _elasticlimit = value; if (_elasticlimit > _restdist) _elasticlimit = _restdist; } }

        public Spring() : this(null) { }
        public Spring(Node parent = null)
        {

            if (parent != null)
            {
                this.parent = parent;
            }
            com = comp.spring;
            methods = mtypes.affectself | mtypes.affectother;// | mtypes.draw | mtypes.minordraw;
        }

        public override void AfterCloning()
        {
        }

        public override void InitializeLists()
        {
        }

        public override void AffectOther(Node other) // called when used as a link
        {
            if (!active) { return; }
            if (activated)
            {
                float dist = Vector2.Distance(parent.body.pos, other.body.pos);
                if (!hook && dist > restdist) return;
                if (limit && dist < elasticlimit) dist = elasticlimit;

                float stretch = dist - restdist;
                float strength = -stretch * multiplier;
                Vector2 force = other.body.pos - parent.body.pos;
                VMath.NormalizeSafe(ref force);
                force *= strength;
                other.body.ApplyForce(force);


            }
        }
        public override void AffectSelf() // called when making individual links (clicking for each link)
        {
            if (activated)
            {

            }
        }

        public override void Draw(SpriteBatch spritebatch)
        {
        }

        public void onCollision(Dictionary<dynamic, dynamic> args)
        {
        }

    }
}
