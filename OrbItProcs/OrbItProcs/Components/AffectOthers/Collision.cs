using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs
{
    public class Collision : Component, ILinkable
    {
        private Link _link = null;
        public Link link { get { return _link; } set { _link = value; } }

        public bool _AffectOnlyColliders = true;
        public bool AffectOnlyColliders { get { return _AffectOnlyColliders; } set { _AffectOnlyColliders = value; } }

        public Collision() : this(null) { }
        public Collision(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.collision; 
            methods = mtypes.affectother; 
        }
        public override void Initialize(Node parent)
        {
            this.parent = parent;
        }

        public override void AffectOther(Node other)
        {
            if (!active) { return; }
            //assuming other has been checked for 'active' from caller
            if (exclusions.Contains(other)) return;

            if (AffectOnlyColliders && (!other.comps.ContainsKey(comp.collision) || !other.isCompActive(comp.collision)))
            {
                return;
            }

            if (Utils.checkCollision(parent, other))
            {
                Dictionary<dynamic, dynamic> dict = new Dictionary<dynamic, dynamic>() { 
                { "collidee", other },
                };
                parent.OnCollidePublic(dict);
                dict["collidee"] = parent;
                other.OnCollidePublic(dict);
                Utils.resolveCollision(parent, other);
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
