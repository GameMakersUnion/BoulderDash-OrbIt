using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs.Components
{
    public class Collision : Component
    {
        public Collision() { com = comp.collision; }
        public Collision(Node parent)
        {
            this.parent = parent;
            this.com = comp.collision;
        }


        public override void Initialize()
        {

        }

        public override bool hasMethod(string methodName)
        {
            methodName = methodName.ToLower();
            if (methodName.Equals("affectother")) return true;
            if (methodName.Equals("affectself")) return false;
            if (methodName.Equals("draw")) return true;
            else return false;
        }


        public override void AffectOther(Node other)
        {
            if (!active)
            {
                return;
            }
            //assuming other has been checked for 'active' from caller

            if (Utils.checkCollision(parent, other))
            {
                parent.OnCollidePublic();
                other.OnCollidePublic();
                Utils.resolveCollision(parent, other);
            }
            
        }
        public override void AffectSelf()
        {
            //do stuff (actually nope; gravity doesn't have this method)
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            //it would be really cool to have some kind of blending effects so that every combination of components will look diff
            //spritebatch.Draw(parent.getTexture(), parent.props[properties.core_position], Color.White);
            //spritebatch.Draw(parent.getTexture(), parent.position, null, Color.White, 0, new Vector2(parent.texture.Width / 2, parent.texture.Height / 2), 1f, SpriteEffects.None, 0);

        }
    }
}
