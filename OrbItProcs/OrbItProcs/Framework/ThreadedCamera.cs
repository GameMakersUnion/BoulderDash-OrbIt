using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OrbItProcs.Framework
{

    public struct DrawCommand
    {
        public enum DrawType
        {
            standard,
            vectScaled,
            drawString
        }

        private DrawType         type        ;
        private textures         texture     ;
        private Vector2          position    ;
        private Color            color       ;
        private float            scale       ;
        private Vector2          scalevect   ;
        private float            rotation    ;
        private Rectangle?       sourceRect  ;
        private Vector2          origin      ;
        private SpriteEffects    effects     ;
        private float            layerDepth  ;
        private string           text        ;


        public DrawCommand(textures texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0)
        {
            this.type = DrawType.standard;     
            this.texture = texture;
            this.position = position;
            this.color = color; 
            this.scale = scale;
            this.rotation = rotation;
            this.sourceRect = sourceRect;
            this.origin = origin;
            this.effects =  effects;
            this.layerDepth =  layerDepth;
            this.scalevect = default(Vector2);
            this.text = null;
        }
        public DrawCommand(textures texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, Vector2 scalevect, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0)
        {
            this.type = DrawType.vectScaled;
            this.texture = texture;
            this.position = position;
            this.color = color; 
            this.scale = default(float);   
            this.scalevect = scalevect;
            this.rotation = rotation;
            this.sourceRect = sourceRect;
            this.origin = origin;
            this.effects =  effects;
            this.layerDepth =  layerDepth;
            this.text = null;
        }
        public DrawCommand(string text, Vector2 position, Color color, float scale = 0.5f)
        {
            this.type = DrawType.drawString;    
            this.texture = default(textures);
            this.position = position;
            this.color = color; 
            this.scale = scale;   
            this.scalevect = default(Vector2);
            this.rotation = default(float);
            this.sourceRect = null;
            this.origin = default(Vector2);
            this.effects = default(SpriteEffects);
            this.layerDepth =  default(int);
            this.text = text;
        }

        public void draw(SpriteBatch batch)
        {
            switch (type)
            {
                case DrawType.standard:
                    batch.Draw(Program.getGame().textureDict[texture], position, sourceRect, color, rotation, origin, scale, effects, layerDepth);
                    return;
                case DrawType.vectScaled:
                    batch.Draw(Program.getGame().textureDict[texture], position, sourceRect, color, rotation, origin, scalevect, effects, layerDepth);
                    return;
                case DrawType.drawString:
                    batch.DrawString(Camera.font, text, position, color, 0f, Vector2.Zero,scale, SpriteEffects.None,0);
                    return;
            } 
        }

    }
    public class ThreadedCamera : Camera
    {
        EventWaitHandle _wh = new AutoResetEvent(false);
        Thread _worker;
        readonly object _locker = new object();
        Queue<string> _tasks = new Queue<string>();
        Queue<DrawCommand> nextFrame = new Queue<DrawCommand>();

        public ThreadedCamera(Room room, float zoom = 0.5f, Vector2? pos = null) : base(room, zoom, pos)
        {
        }

        public void Render()
        {
            int count = nextFrame.Count;
            for (int i = 0; i < count; i++)
            {
                nextFrame.Dequeue().draw(batch);
            }

        }

        private void Work(object obj)
        {
            throw new NotImplementedException();
        }
        public override void Draw(textures texture, Vector2 position, Color color, float scale)
        {
            nextFrame.Enqueue(new DrawCommand(texture, ((position - pos) * zoom) + CameraOffsetVect, null, color, 0, room.game.textureCenters[texture], scale * zoom, SpriteEffects.None, 0));
        }
        public override void Draw(textures texture, Vector2 position, Color color, float scale, float rotation)
        {
            nextFrame.Enqueue(new DrawCommand(texture, ((position - pos) * zoom) + CameraOffsetVect, null, color, rotation, room.game.textureCenters[texture], scale * zoom, SpriteEffects.None, 0));
        }
        public override void Draw(textures texture, Vector2 position, Color color, Vector2 scalevect, float rotation)
        {
            nextFrame.Enqueue(new DrawCommand(texture, ((position - pos) * zoom) + CameraOffsetVect, null, color, rotation, room.game.textureCenters[texture], scalevect * zoom, SpriteEffects.None, 0));
        }
        public override void Draw(textures texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0)
        {
            nextFrame.Enqueue(new DrawCommand(texture, ((position - pos) * zoom) + CameraOffsetVect, sourceRect, color, rotation, origin, scale * zoom, effects, layerDepth));
        }
        public override void Draw(textures texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, Vector2 scalevect, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0)
        {
            nextFrame.Enqueue(new DrawCommand(texture, ((position - pos) * zoom) + CameraOffsetVect, sourceRect, color, rotation, origin, scalevect * zoom, effects, layerDepth));
        }
        public override void DrawStringWorld(string text, Vector2 position, Color color, Color? color2 = null, float scale = 0.5f, bool offset = true)
        {
            Color c2 = Color.White;
            if (color2 != null) c2 = (Color)color2;
            Vector2 pos = position * zoom;
            if (offset) pos += CameraOffsetVect;
            nextFrame.Enqueue(new DrawCommand(text, position * zoom + CameraOffsetVect, c2, scale));
            nextFrame.Enqueue(new DrawCommand(text, position * zoom + CameraOffsetVect + new Vector2(1, -1), color, scale));
        }
        public override void DrawStringScreen(string text, Vector2 position, Color color, Color? color2 = null, float scale = 0.5f, bool offset = true)
        {
            Color c2 = Color.White;
            if (color2 != null) c2 = (Color)color2;
            Vector2 pos = position;
            if (offset) pos += CameraOffsetVect;
            nextFrame.Enqueue(new DrawCommand(text, pos, c2, scale));
            nextFrame.Enqueue(new DrawCommand(text, pos + new Vector2(1, -1), color, scale));
        }
    }
    }
