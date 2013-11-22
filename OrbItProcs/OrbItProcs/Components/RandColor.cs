using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Component = OrbItProcs.Components.Component;

namespace OrbItProcs.Components {
    public class RandColor : Component {
        static Dictionary<dynamic, dynamic> defaultCompProps = new Dictionary<dynamic, dynamic>() // make new movement enum later
        {
            //{ },

        };
        //public Node parent;
        public Color randCol;

        public RandColor(Node parent)
        {
            this.parent = parent;
            this.com = comp.randcolor;
            randCol = Color.Blue;
            randCol = new Color((float)Utils.random.Next(255) / (float)255, (float)Utils.random.Next(255) / (float)255, (float)Utils.random.Next(255) / (float)255);
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
            //Utils.ensureContains(parentNode.props,defaultCompProps);
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

            Room room = parent.room;
            float mapzoom = room.mapzoom;

            float tempScale = parent.scale / mapzoom;
            float screenx = parent.position.X / mapzoom;
            float screeny = parent.position.Y / mapzoom;

            spritebatch.Draw(parent.texture, new Vector2(screenx, screeny), null, parent.color, 0, new Vector2(parent.texture.Width / 2, parent.texture.Height / 2), tempScale, SpriteEffects.None, 0);
            
        }

        

    }
}
