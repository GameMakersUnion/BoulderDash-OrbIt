using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs.Components
{
    public class Transfer : Component{
        static Dictionary<dynamic, dynamic> defaultCompProps = new Dictionary<dynamic, dynamic>()
            {
            //{ properties.trns_active,                true },
            //{ properties.trns_radius,                25f },

        };
        //public Node parent;
        //public bool active = true;
        public float radius = 25f;

        public Transfer(Node parent)
        {
            //never be called
            this.parent = parent;
            this.com = comp.transfer;

        }

        public override bool hasMethod(string methodName)
        {
            methodName = methodName.ToLower();
            if (methodName.Equals("affectother")) return true;
            if (methodName.Equals("affectself")) return false;
            if (methodName.Equals("draw")) return true;
            else return false;
        }

        public override void Initialize()
        {
            //Utils.ensureContains(parentNode.props, defaultCompProps);
        }

        public override void AffectOther(Node other)
        {
            //if (!parent.props[comp.transfer]) return; //this comparison is done within the Node

            float distVects = Vector2.Distance(other.position, parent.position);
            if (distVects < radius)
            {
                float newX = (parent.position.X - other.position.X) * 2.05f;
                float newY = (parent.position.Y - other.position.Y) * 2.05f;
                other.position.X += newX;
                other.position.Y += newY;

            }
        }
        public override void AffectSelf()
        {
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            //it would be really cool to have some kind of blending effects so that every combination of components will look diff
            //spritebatch.Draw(parent.props[properties.core_texture], parent.props[properties.core_position], Color.White);
            spritebatch.Draw(parent.texture, parent.position, null, Color.White, 0, new Vector2(parent.texture.Width / 2, parent.texture.Height / 2), 1f, SpriteEffects.None, 0);
        }
    }
}
