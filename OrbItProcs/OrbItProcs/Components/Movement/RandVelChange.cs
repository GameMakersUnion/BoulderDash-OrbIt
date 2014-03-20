using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs
{        
    /// <summary>
    /// Node will move in a seemingly random pattern.
    /// </summary>
    [Info(UserLevel.User, "Node will move in a seemingly random pattern.")]
    public class RandVelChange : Component {

        public const mtypes CompType = mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }
        public RandVelChange() : this(null) { }
        public RandVelChange(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.randvelchange; 
        }

        public override void AffectSelf()
        {
            float counterVelocity = 40f;
            float xCounterVel = (parent.body.velocity.X * -1) / counterVelocity;
            float yCounterVel = (parent.body.velocity.Y * -1) / counterVelocity;
            float dx = 1 - ((float)Utils.random.NextDouble() * 2);
            float dy = 1 - ((float)Utils.random.NextDouble() * 2);
            dx = dx + xCounterVel;
            dy = dy + yCounterVel;
            parent.body.velocity.X += dx;
            parent.body.velocity.Y += dy;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
        }

    }
}
