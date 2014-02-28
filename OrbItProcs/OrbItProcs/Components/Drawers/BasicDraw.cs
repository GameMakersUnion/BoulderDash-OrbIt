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
                if (!parent.body.DrawCircle) return;
            }

            Room room = parent.room;
            float mapzoom = room.mapzoom;

            if (parent.DebugFlag) System.Diagnostics.Debugger.Break();

            //spritebatch.Draw()
            Texture2D tex = parent.getTexture();

            Rectangle? sourceRect = null;
            bool wrapDouble = false;
            int minx = 0, miny = 0, maxx = tex.Width, maxy = tex.Height;
            bool needsModifying = false;
            if (parent.movement.active)
            {
                if (parent.movement.mode == movemode.screenwrap)
                {
                    float tip = parent.body.pos.X - parent.body.radius;
                    if (tip < 0) //x
                    {
                        needsModifying = true;
                        if (tip < -parent.body.radius) return;
                        minx = (int)(-tip * parent.body.radius * 2 / tex.Width);
                    }
                    else
                    {
                        tip = parent.body.pos.X + parent.body.radius;
                        if (tip > room.worldWidth)
                        {
                            needsModifying = true;
                            if (tip > room.worldWidth + parent.body.radius) return;
                            maxx = maxx - (int)((tip - room.worldWidth) * parent.body.radius * 2 / tex.Width);
                        }
                    }
                    tip = parent.body.pos.Y - parent.body.radius;
                    if (tip < 0) //y
                    {
                        needsModifying = true;
                        if (tip < -parent.body.radius) return;
                        miny = (int)(-tip * parent.body.radius * 2 / tex.Height);
                    }
                    else
                    {
                        tip = parent.body.pos.Y + parent.body.radius;
                        if (tip > room.worldHeight)
                        {
                            needsModifying = true;
                            if (tip > room.worldHeight + parent.body.radius) return;
                            maxy = maxy - (int)((tip - room.worldHeight) * parent.body.radius * 2 / tex.Height);
                        }
                    }
                    if (needsModifying)
                    {
                        sourceRect = new Rectangle(minx, miny, maxx - minx, maxy - miny);
                    }
                }
                //else if (parent.movement.mode == movemode.screenwrap)
                //{
                //
                //}
                
            }
            if (sourceRect != null)
            {
                spritebatch.Draw(tex, (parent.body.pos + new Vector2(minx, miny)) / mapzoom, sourceRect, parent.body.color, 0, parent.TextureCenter(), parent.body.scale / mapzoom, SpriteEffects.None, 0);
            }
            else
            {
                spritebatch.Draw(tex, parent.body.pos / mapzoom, sourceRect, parent.body.color, 0, parent.TextureCenter(), parent.body.scale / mapzoom, SpriteEffects.None, 0);
            }
            
            
        }

    }
}
