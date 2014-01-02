using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs.Components
{
    public class RandVelChange : Component {


        public RandVelChange() : this(null) { }
        public RandVelChange(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.randvelchange; 
            methods = mtypes.affectself; 
        }


        public override void Initialize(Node parent)
        {
            this.parent = parent;
        }

        public override void AffectSelf()
        {
            float counterVelocity = 40f;
            float xCounterVel = (parent.transform.velocity.X * -1) / counterVelocity;
            float yCounterVel = (parent.transform.velocity.Y * -1) / counterVelocity;
            float dx = 1 - ((float)Utils.random.NextDouble() * 2);
            float dy = 1 - ((float)Utils.random.NextDouble() * 2);
            dx = dx + xCounterVel;
            dy = dy + yCounterVel;
            parent.transform.velocity.X += dx;
            parent.transform.velocity.Y += dy;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
        }

    }
}
