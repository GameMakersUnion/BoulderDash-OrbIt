using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs.Components
{
    public class Movement : Component {
        static Dictionary<dynamic, dynamic> defaultCompProps = new Dictionary<dynamic, dynamic>() // make new movement enum later
            {
            //{ },

        };
        //public Node parent;
        public bool pushable = true;

        public Movement(Node parent)
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
            //Utils.ensureContains(parentNode.props,defaultCompProps);
        }

        public override void AffectOther(Node other)
        {
            
        }
        public override void AffectSelf()
        {
            //do stuff (actually node; gravity doesn't have this method
            parent.position.X += parent.velocity.X;
            parent.position.Y += parent.velocity.Y;
            wallBounce();
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            //it would be really cool to have some kind of blending effects so that every combination of components will look diff

            Room room = parent.room;
            float mapzoom = room.mapzoom;

            parent.scale = 1 / mapzoom;
            float screenx = parent.position.X / mapzoom;
            float screeny = parent.position.Y / mapzoom;

            spritebatch.Draw(parent.texture, new Vector2(screenx, screeny), null, parent.color, 0, new Vector2(parent.texture.Width / 2, parent.texture.Height / 2), parent.scale, SpriteEffects.None, 0);
            //spritebatch.Draw(parent.props[properties.core_texture], parent.props[properties.core_position], null, Color.White, 0, new Vector2(parent.props[properties.core_texture].Width / 2, parent.props[properties.core_texture].Height / 2), 1f, SpriteEffects.None, 0);
        }

        public void wallBounce()
        {
            //if (room.PropertiesDict["wallBounce"])
            //float levelwidth = room.game1...;
            int levelwidth = parent.room.game1.worldWidth;
            int levelheight = parent.room.game1.worldHeight;



            if (parent.position.X >= (levelwidth - parent.Radius))
            {
                parent.position.X = levelwidth - parent.Radius;
                parent.velocity.X *= -1;
                parent.OnCollidePublic();

            }
            else if (parent.position.X < parent.Radius)
            {
                parent.position.X = parent.Radius;
                parent.velocity.X *= -1;
                parent.OnCollidePublic();
            }
            if (parent.position.Y >= (levelheight - parent.Radius))
            {
                parent.position.Y = levelheight - parent.Radius;
                parent.velocity.Y *= -1;
                parent.OnCollidePublic();
            }
            else if (parent.position.Y < parent.Radius)
            {
                parent.position.Y = parent.Radius;
                parent.velocity.Y *= -1;
                parent.OnCollidePublic();
            }


        }

    }
}
