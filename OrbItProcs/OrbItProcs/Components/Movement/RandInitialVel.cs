using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs.Components
{
    public class RandInitialVel : Component {

        public bool active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                if (value && parent != null) Initialize(parent);
                if (parent != null && parent.comps.ContainsKey(com))
                {
                    parent.triggerSortLists();
                }
            }
        }

        private float _multiplier = 16f;
        
        public float multiplier
        {
            get { return _multiplier; }
            set {
                _multiplier = value;
                if (parent != null && parent.velocity != null)
                {
                    ScaleVelocity();
                }
            }
        }

        public RandInitialVel() : this(null) { }
        public RandInitialVel(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.randinitialvel; 
            methods = mtypes.initialize; 
        }

        public void ScaleVelocity()
        {
            if (parent.velocity.X != 0 && parent.velocity.Y != 0)
            {
                parent.velocity.Normalize();
                parent.velocity *= multiplier;
            }
        }

        public override void Initialize(Node parent)
        {
            this.parent = parent;
            if (active)
            {
                if (parent.velocity.X != 0 && parent.velocity.Y != 0)
                {
                    ScaleVelocity();
                }
                else
                {
                    float x = ((float)Utils.random.NextDouble() * 100) - 50;
                    float y = ((float)Utils.random.NextDouble() * 100) - 50;
                    Vector2 vel = new Vector2(x, y);
                    vel.Normalize();
                    vel = vel * multiplier;
                    parent.velocity = vel;
                }
                
            }
        }

        public override void AffectOther(Node other)
        {
            
        }
        public override void AffectSelf()
        {
        }

        public override void Draw(SpriteBatch spritebatch)
        {
        }

    }
}
