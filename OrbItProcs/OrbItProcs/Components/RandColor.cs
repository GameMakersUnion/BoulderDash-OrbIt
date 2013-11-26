using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Component = OrbItProcs.Components.Component;

namespace OrbItProcs.Components {
    public class RandColor : Component {

        //public Color randCol;
        public RandColor() { com = comp.randcolor; }
        public RandColor(Node parent)
        {
            this.parent = parent;
            this.com = comp.randcolor;
            //randCol = Color.Blue;
            //randCol = new Color((float)Utils.random.Next(255) / (float)255, (float)Utils.random.Next(255) / (float)255, (float)Utils.random.Next(255) / (float)255);
        }

        public override bool hasMethod(string methodName)
        {
            methodName = methodName.ToLower();
            if (methodName.Equals("affectother")) return false;
            if (methodName.Equals("affectself")) return false;
            if (methodName.Equals("draw")) return true;
            else return false;
        }

        public override void Initialize()
        {
            if (parent != null)
            {
                parent.color = new Color((float)Utils.random.Next(255) / (float)255, (float)Utils.random.Next(255) / (float)255, (float)Utils.random.Next(255) / (float)255);
            }
        }

        public override void AffectOther(Node other)
        {

        }
        public override void AffectSelf()
        {
        }

        public override void Draw(SpriteBatch spritebatch)
        {

            Room room = parent.room;
            float mapzoom = room.mapzoom;

            float tempScale = parent.scale / mapzoom;
            float screenx = parent.position.X / mapzoom;
            float screeny = parent.position.Y / mapzoom;

            spritebatch.Draw(parent.getTexture(), new Vector2(screenx, screeny), null, parent.color, 0, parent.TextureCenter(), tempScale, SpriteEffects.None, 0);
            
        }

        

    }
}
