﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs
{
    /// <summary>
    /// Basic Draw Component, ensures that you can see the node.
    /// </summary>
    [Info(UserLevel.User, "Basic Draw Component, ensures that you can see the node.")]
    public class BasicDraw : Component
    {
        public const mtypes CompType = mtypes.essential | mtypes.draw;
        public override mtypes compType { get { return CompType; } set { } }
        /// <summary>
        /// WARNING: THIS FLAG IS ONLY OBSERVED BY POLYGONS: Determines when to draw the center of mass as a circle
        /// </summary>
        [Info(UserLevel.Advanced, "WARNING: THIS FLAG IS ONLY OBSERVED BY POLYGONS: Determines when to draw the center of mass as a circle")]
        public bool DrawCircle { get; set; }

        public BasicDraw() : this(null) { }
        public BasicDraw(Node parent = null) 
        {
            if (parent != null) this.parent = parent;
            com = comp.basicdraw; 
            DrawCircle = true;
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
            float mapzoom = room.zoom;

            Texture2D tex = parent.getTexture();
            room.camera.Draw(parent.body.texture, parent.body.pos, parent.body.color, parent.body.scale);

            /*Rectangle? sourceRect = null;
            int minx = 0, miny = 0, maxx = tex.Width, maxy = tex.Height;
            bool needsModifying = false;
            if (parent.movement.active)
            {
                if (parent.movement.mode == movemode.screenwrap)
                {
                    int offset = 5;
                    float tip = parent.body.pos.X - parent.body.radius;
                    float radiusFactor = tex.Width / (parent.body.radius * 2f); //assuming texture width == height
                    if (tip < 0) //x
                    {
                        needsModifying = true;
                        if (tip < -parent.body.radius - offset) return;
                        minx = (int)(-tip * radiusFactor);
                    }
                    else
                    {
                        tip = parent.body.pos.X + parent.body.radius;
                        if (tip > room.worldWidth)
                        {
                            needsModifying = true;
                            if (tip > room.worldWidth + (parent.body.radius + offset)) return;
                            maxx = maxx - (int)((tip - room.worldWidth) * radiusFactor);
                        }
                    }
                    tip = parent.body.pos.Y - parent.body.radius;
                    if (tip < 0) //y
                    {
                        needsModifying = true;
                        if (tip < -parent.body.radius - offset) return;
                        miny = (int)(-tip * radiusFactor);
                    }
                    else
                    {
                        tip = parent.body.pos.Y + parent.body.radius;
                        if (tip > room.worldHeight)
                        {
                            needsModifying = true;
                            if (tip > room.worldHeight + (parent.body.radius + offset)) return;
                            maxy = maxy - (int)((tip - room.worldHeight) * radiusFactor);
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
                float radiusFactor = tex.Width / (parent.body.radius * 2f);
                spritebatch.Draw(tex, (parent.body.pos + new Vector2(minx, miny) / radiusFactor) * mapzoom, sourceRect, parent.body.color, 0, parent.TextureCenter(), parent.body.scale * mapzoom, SpriteEffects.None, 0);
                if (DrawMirror)
                {
                    Rectangle old = (Rectangle)sourceRect;
                    Rectangle mirror = new Rectangle(0, 0, tex.Width, tex.Height);
                    Vector2 pos = parent.body.pos;
                    if (old.Width != tex.Width)
                    {
                        if (old.X == 0) //actual node is on the right border
                        {
                            mirror.X = old.Width;
                            pos.X = parent.body.pos.X - room.worldWidth;
                        }
                        else //actual node is on the left border
                        {
                            mirror.X = 0;
                            pos.X = parent.body.pos.X + room.worldWidth;
                        }
                        mirror.Width = tex.Width - old.Width;
                    }
                    if (old.Height != tex.Height)
                    {
                        if (old.Y == 0) //actual node is on the bottom border
                        {
                            mirror.Y = old.Height;
                            pos.Y = parent.body.pos.Y - room.worldHeight;
                        }
                        else //actual node is on the top border
                        {
                            mirror.Y = 0;
                            pos.Y = parent.body.pos.Y + room.worldHeight;
                        }
                        mirror.Height = tex.Height - old.Height;
                    }
                    //mirror.X = old.X == 0 ? old.Width : 0; //actual node is at right or left border
                    //mirror.Y = old.Y == 0 ? old.Height : 0; //actual node is at bottom or top border
                    Vector2 newpos = pos + new Vector2(mirror.X, mirror.Y) / radiusFactor;
                    bool widths = old.Width != tex.Width;
                    bool heights = old.Height != tex.Height;
                    if (widths || heights)
                    {
                        spritebatch.Draw(tex, newpos * mapzoom, mirror, parent.body.color, 0, parent.TextureCenter(), parent.body.scale * mapzoom, SpriteEffects.None, 0);
                        if (widths && heights)
                        {

                        }
                    }
                    
                }
            }
            else
            {
                spritebatch.Draw(tex, parent.body.pos * mapzoom, sourceRect, parent.body.color, 0, parent.TextureCenter(), parent.body.scale * mapzoom, SpriteEffects.None, 0);
            }*/


            //spritebatch.Draw(tex, parent.body.pos * mapzoom, null, parent.body.color, 0, parent.TextureCenter(), parent.body.scale * mapzoom, SpriteEffects.None, 0);
            
        }

        //public static void CameraDraw(this SpriteBatch batch, Texture2D tex, Vector2 position, )
    }
}
