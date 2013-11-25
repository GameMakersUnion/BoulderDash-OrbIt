using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs.Components
{
    public class Lifetime : Component
    {


        public int lifeleft;
        public int maxlife;

        public int timer, timerMax;
        public bool alive = true;

        public Lifetime(Node parent)
        {
            this.parent = parent;
            this.com = comp.lifetime;

            maxlife = 100;
            lifeleft = maxlife;
            timer = 0;
            timerMax = 1;

        }

        public override void Initialize()
        {
            lifeleft = maxlife;
        }

        public override bool hasMethod(string methodName)
        {
            methodName = methodName.ToLower();
            if (methodName.Equals("initialize")) return true;
            if (methodName.Equals("affectother")) return false;
            if (methodName.Equals("affectself")) return true;
            if (methodName.Equals("draw")) return false;
            else return false;
        }

        public override void AffectOther(Node other)
        {

        }
        public override void AffectSelf()
        {
            
            if (++timer % timerMax == 0)
            {
                if (lifeleft-- <= 0)
                {
                    Die();
                }
                
            }
        }

        public void Die()
        {
            if (!alive) return;
            alive = false;

            //perform death here.... do it properly later
            //parent.room.nodes.Remove(parent);
            parent.props[node.active] = false;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            //it would be really cool to have some kind of blending effects so that every combination of components will look diff
            //spritebatch.Draw(parent.props[properties.core_texture], parent.props[properties.core_position], Color.White);
            //spritebatch.Draw(parent.texture, parent.position, null, Color.White, 0, new Vector2(parent.texture.Width / 2, parent.texture.Height / 2), 1f, SpriteEffects.None, 0);
        }

    }
}
