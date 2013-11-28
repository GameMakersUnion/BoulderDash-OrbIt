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
                if (parent != null && parent.velocity != null)
                {
                    if (_multiplier == 0 && value > 0)
                    {
                        _multiplier = value;
                        Initialize(parent);
                    }
                    else
                    {
                        //float m = value / _multiplier;
                        //parent.velocity *= m;
                        parent.velocity.Normalize();
                        parent.velocity *= value;
                    }
                }
                _multiplier = value;
            }
        }

        public RandInitialVel() : this(null) { }
        public RandInitialVel(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.randinitialvel; 
            methods = mtypes.initialize; 
        }

        public override void Initialize(Node parent)
        {
            this.parent = parent;
            if (active)
            {
                if (parent.velocity.X != 0 && parent.velocity.Y != 0)
                {
                    //Console.WriteLine("yeah");
                    parent.velocity.Normalize();
                    parent.velocity *= multiplier;
                    //Console.WriteLine(parent.velocity);
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
