using System;
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
        public enum Initial
        {
            Random,
            Managed,
        }
        public const mtypes CompType = mtypes.essential | mtypes.draw;
        public override mtypes compType { get { return CompType; } set { } }
        /// <summary>
        /// (Polygons only) Determines when to draw the center of mass as a circle
        /// </summary>
        [Info(UserLevel.Advanced, "WARNING: THIS FLAG IS ONLY OBSERVED BY POLYGONS: Determines when to draw the center of mass as a circle")]
        public bool DrawCircle { get; set; }

        public Initial _InitialColor = Initial.Managed;
        /// <summary>
        /// Determines whether the color will be random or set by the Red, Green and Blue properties initially.
        /// </summary>
        [Info(UserLevel.User, "Determines whether the color will be random or set by the Red, Green and Blue properties initially.")]
        public Initial InitialColor { get { return _InitialColor; } set { _InitialColor = value; Colorize(); } }
        /// <summary>
        /// Red color component
        /// </summary>
        [Info(UserLevel.User, "Red color component")]
        public int Red { get; set; }
        /// <summary>
        /// Green color component
        /// </summary>
        [Info(UserLevel.User, "Green color component")]
        public int Green { get; set; }
        /// <summary>
        /// Blue color component
        /// </summary>
        [Info(UserLevel.User, "Blue color component")]
        public int Blue { get; set; }
        /// <summary>
        /// Alpha color component
        /// </summary>
        [Info(UserLevel.User, "Alpha color component")]
        public int Alpha { get; set; }
        public BasicDraw() : this(null) { }
        public BasicDraw(Node parent = null) 
        {
            if (parent != null) this.parent = parent;
            com = comp.basicdraw; 
            DrawCircle = true;
            UpdateColor();
        }

        public void UpdateColor()
        {
            if (parent == null) return;
            Color c = parent.body.color;
            Red = c.R;
            Green = c.G;
            Blue = c.B;
            Alpha = c.A;
        }

        public override void OnSpawn()
        {
            Colorize();
            int runenum = Utils.random.Next(16);
            textures r = (textures)runenum;
            parent.body.texture = r;
        }

        public void Colorize()
        {
            if (InitialColor == Initial.Random)
            {
                RandomizeColor();
            }
            else if (InitialColor == Initial.Managed)
            {
                SetColor();
            }
        }

        public void SetColor()
        {
            if (parent != null)
            {
                parent.body.color = new Color(Red, Green, Blue, Alpha);
                parent.body.permaColor = parent.body.color;
            }
        }
        
        public void RandomizeColor()
        {
            if (parent != null)
            {
                parent.body.color = Utils.randomColor();
                parent.body.permaColor = parent.body.color;
            }
        }

        public override void Draw()
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

            if (parent.HasComp(comp.shader))
                room.camera.Draw(parent.body.texture, parent.body.pos, parent.body.color, parent.body.scale, parent.body.orient, parent.Comp<Shader>().shaderPack);
            else
                room.camera.Draw(parent.body.texture, parent.body.pos, parent.body.color, parent.body.scale, parent.body.orient);

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
