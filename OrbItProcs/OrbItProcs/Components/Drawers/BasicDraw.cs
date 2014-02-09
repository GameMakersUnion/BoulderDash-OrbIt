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

        //private bool _pushable = true;
        //public bool pushable { get { return _pushable; } set { _pushable = value; } }
        public BasicDraw() : this(null) { }
        public BasicDraw(Node parent = null) 
        {
            if (parent != null) this.parent = parent;
            com = comp.basicdraw; 
            methods = mtypes.draw;
        }

        public override void Initialize(Node parent)
        {
            this.parent = parent;
        }

        
        public override void AffectSelf()
        {
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            //it would be really cool to have some kind of blending effects so that every combination of components will look diff

            Room room = parent.room;
            float mapzoom = room.mapzoom;

            //spritebatch.Draw()
            spritebatch.Draw(parent.getTexture(), parent.body.position / mapzoom, null, parent.body.color, 0, parent.TextureCenter(), parent.body.scale / mapzoom, SpriteEffects.None, 0);
            
            
        }

    }
}
