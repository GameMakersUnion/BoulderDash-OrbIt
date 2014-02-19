using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs
{
    public class BasicDraw : Component
    {

        private bool _DrawCircle = true;
        public bool DrawCircle { get { return _DrawCircle; } set { _DrawCircle = value; } }

        public BasicDraw() : this(null) { }
        public BasicDraw(Node parent = null) 
        {
            if (parent != null) this.parent = parent;
            com = comp.basicdraw; 
            methods = mtypes.draw;
        }
        
        public override void AffectSelf()
        {
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            //it would be really cool to have some kind of blending effects so that every combination of components will look diff

            if (parent.body.shape is Polygon)
            {
                parent.body.shape.Draw();
                if (!DrawCircle) return;
            }

            Room room = parent.room;
            float mapzoom = room.mapzoom;

            if (parent.DebugFlag) System.Diagnostics.Debugger.Break();

            //spritebatch.Draw()
            Texture2D tex = parent.getTexture();

            spritebatch.Draw(tex, parent.body.pos / mapzoom, null, parent.body.color, 0, parent.TextureCenter(), parent.body.scale / mapzoom, SpriteEffects.None, 0);
            
            
        }

    }
}
