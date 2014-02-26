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

        private float _multiplier = 8f;
        
        public float multiplier
        {
            get { return _multiplier; }
            set {
                _multiplier = value;
                if (parent != null && parent.body.velocity != null)
                {
                    //ScaleVelocity();
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
        }

        public override void OnSpawn()
        {
            if (!active) return;
            if (parent.body.velocity.X != 0 && parent.body.velocity.Y != 0)
            {
                ScaleVelocity();
            }
            else
            {
                RandomizeVelocity();
            }
        }

        public void ScaleVelocity()
        {
            if (parent.body.velocity.X != 0 && parent.body.velocity.Y != 0)
            {
                parent.body.velocity.Normalize();
                parent.body.velocity *= multiplier;
            }
        }

        public void RandomizeVelocity()
        {
            float x = ((float)Utils.random.NextDouble() * 100) - 50;
            float y = ((float)Utils.random.NextDouble() * 100) - 50;
            Vector2 vel = new Vector2(x, y);
            vel.Normalize();
            vel = vel * multiplier;
            parent.body.velocity = vel;
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
