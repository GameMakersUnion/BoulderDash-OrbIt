using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs.Components
{
    public class Collision : Component, ILinkable
    {
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
            //if (!active) { return; }
            //assuming other has been checked for 'active' from caller
            if (exclusions.Contains(other)) return;

            if (Utils.checkCollision(parent, other))
            {
                parent.OnCollidePublic();
                other.OnCollidePublic();
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
