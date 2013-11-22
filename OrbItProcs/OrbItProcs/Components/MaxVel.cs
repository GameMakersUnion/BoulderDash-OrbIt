using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs.Components
{
    public class MaxVel : Component {

        public float maxvel = 20f;

        public MaxVel(Node parent)
        {
            this.parent = parent;
            this.com = comp.movement;
        }

        public override bool hasMethod(string methodName)
        {
            methodName = methodName.ToLower();
            if (methodName.Equals("affectother")) return false;
            if (methodName.Equals("affectself")) return true;
            if (methodName.Equals("draw")) return true;
            else return false;
        }

        public override void Initialize()
        {
        }

        public override void AffectOther(Node other)
        {
        }
        public override void AffectSelf()
        {
            if ((Math.Pow(parent.velocity.X, 2) + Math.Pow(parent.velocity.Y, 2)) > Math.Pow(maxvel, 2))
            {
                parent.velocity.Normalize();
                parent.velocity *= maxvel;
            }
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            //it would be really cool to have some kind of blending effects so that every combination of components will look diff

            Room room = parent.room;
            float mapzoom = room.mapzoom;

            parent.scale = 1 / mapzoom;
            float screenx = parent.position.X / mapzoom;
            float screeny = parent.position.Y / mapzoom;

            spritebatch.Draw(parent.texture, new Vector2(screenx, screeny), null, Color.White, 0, new Vector2(parent.texture.Width / 2, parent.texture.Height / 2), parent.scale, SpriteEffects.None, 0);
            //spritebatch.Draw(parent.props[properties.core_texture], parent.props[properties.core_position], null, Color.White, 0, new Vector2(parent.props[properties.core_texture].Width / 2, parent.props[properties.core_texture].Height / 2), 1f, SpriteEffects.None, 0);
        }
    }
}
