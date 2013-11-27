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

        public RandInitialVel() { com = comp.randinitialvel; }

        public RandInitialVel(Node parent)
        {
            this.parent = parent;
            this.com = comp.randinitialvel;
        }

        public override void Initialize(Node parent)
        {
            this.parent = parent;
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

        public override bool hasMethod(string methodName)
        {
            methodName = methodName.ToLower();
            if (methodName.Equals("initialize")) return true;
            if (methodName.Equals("affectother")) return false;
            if (methodName.Equals("affectself")) return false;
            if (methodName.Equals("draw")) return true;
            else return false;
        }

        public override void AffectOther(Node other)
        {
            
        }
        public override void AffectSelf()
        {
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            //it would be really cool to have some kind of blending effects so that every combination of components will look diff
            //spritebatch.Draw(parent.props[properties.core_texture], parent.props[properties.core_position], Color.White);
            spritebatch.Draw(parent.getTexture(), parent.position, null, Color.White, 0, parent.TextureCenter(), 1f, SpriteEffects.None, 0);
        }

    }
}
