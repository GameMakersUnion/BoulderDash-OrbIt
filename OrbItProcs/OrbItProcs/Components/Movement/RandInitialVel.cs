using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs
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

        private float _multiplier = 8f;
        
        public float multiplier
        {
            get { return _multiplier; }
            set {
                _multiplier = value;
                if (parent != null && parent.body.velocity != null)
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

        public override void AfterCloning()
        {
            parent.body.velocity = new Vector2(0, 0);
        }

        public void ScaleVelocity()
        {
            if (parent.body.velocity.X != 0 && parent.body.velocity.Y != 0)
            {
                parent.body.velocity.Normalize();
                parent.body.velocity *= multiplier;
            }
        }

        public override void Initialize(Node parent)
        {
            this.parent = parent;
            if (active)
            {
                if (parent.body.velocity.X != 0 && parent.body.velocity.Y != 0)
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
                    parent.body.velocity = vel;
                }
                
            }
        }
        public override void OnSpawn()
        {
            if (!active) return;
            //parent.transform.velocity = new Vector2(0, 0);
            Initialize(parent);
            return;
            /*
            float x = ((float)Utils.random.NextDouble() * 100) - 50;
            float y = ((float)Utils.random.NextDouble() * 100) - 50;
            Vector2 vel = new Vector2(x, y);
            vel.Normalize();
            vel = vel * multiplier;
            parent.transform.velocity = vel;
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
