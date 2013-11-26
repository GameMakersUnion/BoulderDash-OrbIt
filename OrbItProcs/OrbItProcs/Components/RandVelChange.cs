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


        public RandVelChange() { com = comp.randvelchange; }
        public RandVelChange(Node parent)
        {
            this.parent = parent;
            this.com = comp.randvelchange;
        }

        public override void Initialize()
        {
        }

        public override bool hasMethod(string methodName)
        {
            methodName = methodName.ToLower();
            if (methodName.Equals("affectother")) return false;
            if (methodName.Equals("affectself")) return true;
            if (methodName.Equals("draw")) return true;
            else return false;
        }

        public override void AffectOther(Node other)
        {

        }
        public override void AffectSelf()
        {
            float counterVelocity = 40f;
            float xCounterVel = (parent.velocity.X * -1) / counterVelocity;
            float yCounterVel = (parent.velocity.Y * -1) / counterVelocity;
            float dx = 1 - ((float)Utils.random.NextDouble() * 2);
            float dy = 1 - ((float)Utils.random.NextDouble() * 2);
            dx = dx + xCounterVel;
            dy = dy + yCounterVel;
            parent.velocity.X += dx;
            parent.velocity.Y += dy;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            //it would be really cool to have some kind of blending effects so that every combination of components will look diff
            //spritebatch.Draw(parent.props[properties.core_texture], parent.props[properties.core_position], Color.White);
            spritebatch.Draw(parent.getTexture(), parent.position, null, Color.White, 0, parent.TextureCenter(), 1f, SpriteEffects.None, 0);
        }

    }
}
