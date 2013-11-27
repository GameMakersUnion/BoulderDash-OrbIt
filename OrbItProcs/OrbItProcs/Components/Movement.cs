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

        private bool _pushable = true;
        public bool pushable { get { return _pushable; } set { _pushable = value; } }

        public Movement() { com = comp.movement; }
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

        public override void Initialize(Node parent)
        {
            this.parent = parent;
        }

        public override void AffectOther(Node other)
        {
            
        }
        public override void AffectSelf()
        {
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

            spritebatch.Draw(parent.getTexture(), new Vector2(screenx, screeny), null, parent.color, 0, parent.TextureCenter(), parent.scale, SpriteEffects.None, 0);
            
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
