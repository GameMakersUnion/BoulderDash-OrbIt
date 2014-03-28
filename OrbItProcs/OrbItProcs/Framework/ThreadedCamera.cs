using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OrbItProcs
{

    public class DrawCommand
    {
        public enum DrawType
        {
            standard,
            vectScaled,
            drawString
        }

        private DrawType type;
        private textures texture;
        private Vector2 position;
        public Color color;
        private Color permColor;
        private float scale;
        private Vector2 scalevect;
        private float rotation;
        private Rectangle? sourceRect;
        private Vector2 origin;
        private SpriteEffects effects;
        private float layerDepth;
        private string text;
        public float life;
        public float maxlife;
        public ShaderPack shaderPack;

        public DrawCommand(textures texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0, int maxlife = -1, ShaderPack? shaderPack = null)
        {
            this.type = DrawType.standard;
            this.texture = texture;
            this.position = position;
            this.color = color;
            this.permColor = color;
            this.scale = scale;
            this.rotation = rotation;
            this.sourceRect = sourceRect;
            this.origin = origin;
            this.effects = effects;
            this.layerDepth = layerDepth;
            this.maxlife = maxlife;
            this.life = maxlife;
            this.shaderPack = shaderPack ?? ShaderPack.Default;

            //this.scalevect = default(Vector2);
            //this.text = null;
        }
        public DrawCommand(textures texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, Vector2 scalevect, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0, int maxlife = -1, ShaderPack? shaderPack = null)
        {
            this.type = DrawType.vectScaled;
            this.texture = texture;
            this.position = position;
            this.color = color;
            this.permColor = color;
            this.scalevect = scalevect;
            this.rotation = rotation;
            this.sourceRect = sourceRect;
            this.origin = origin;
            this.effects = effects;
            this.layerDepth = layerDepth;
            this.maxlife = maxlife;
            this.life = maxlife;
            //this.shaderPack = shaderPack ?? ShaderPack.Default;
            this.shaderPack = shaderPack ?? new ShaderPack(color);

            //this.scale = default(float);
            //this.text = null;
        }
        public DrawCommand(string text, Vector2 position, Color color, float scale = 0.5f, int maxlife = -1, ShaderPack? shaderPack = null)
        {
            this.type = DrawType.drawString;
            this.position = position;
            this.color = color;
            this.permColor = color;
            this.scale = scale;
            this.text = text;
            this.maxlife = maxlife;
            this.life = maxlife;
            this.shaderPack = shaderPack ?? ShaderPack.Default;

            //this.scalevect = default(Vector2);
            //this.rotation = default(float);
            //this.sourceRect = null;
            //this.origin = default(Vector2);
            //this.effects = default(SpriteEffects);
            //this.layerDepth =  default(int);
            //this.texture = default(textures);

        }

        public void Draw(SpriteBatch batch)
        {
            switch (type)
            {
                case DrawType.standard:
                    batch.Draw(Program.getGame().textureDict[texture], position, sourceRect, color, rotation, origin, scale, effects, layerDepth);
                    break;
                case DrawType.vectScaled:
                    batch.Draw(Program.getGame().textureDict[texture], position, sourceRect, color, rotation, origin, scalevect, effects, layerDepth);
                    break;
                case DrawType.drawString:
                    batch.DrawString(Camera.font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
                    break;
            }
            if (maxlife > 0)
            {
                //this.color = new Color(color.R / 10, color.G / 10, color.B / 10, 200);
                float ratio = (float)Math.Max(life / maxlife, 0.2);
                this.color = this.permColor * ratio;
            }
        }

    }
    public class ThreadedCamera : Camera
    {
        ManualResetEventSlim CameraWaiting = new ManualResetEventSlim(false);
        Thread _worker;
        public readonly object _locker = new object();
        Queue<string> _tasks = new Queue<string>();
        Queue<DrawCommand> thisFrame = new Queue<DrawCommand>();
        Queue<DrawCommand> nextFrame = new Queue<DrawCommand>();

        List<DrawCommand> permanents = new List<DrawCommand>();
        Queue<DrawCommand> addPerm = new Queue<DrawCommand>();
        Queue<DrawCommand> removePerm = new Queue<DrawCommand>();

        public ThreadedCamera(Room room, float zoom = 0.5f, Vector2? pos = null)
            : base(room, zoom, pos)
        {
            _worker = new Thread(Work);
            _worker.Name = "CameraThread";
            _worker.IsBackground = true;
            _worker.Start();
        }

        public void Render()
        {
            thisFrame = nextFrame;
            nextFrame = new Queue<DrawCommand>(); //todo: optimize via a/b pooling
            lock (_locker)
            {
                int count = addPerm.Count;
                for (int i = 0; i < count; i++)
                {
                    permanents.Add(addPerm.Dequeue());
                }

                count = removePerm.Count;
                for (int i = 0; i < count; i++)
                {
                    permanents.Remove(removePerm.Dequeue());
                }
            }

            CameraWaiting.Set();
            //int count = thisFrame.Count;
            //for (int i = 0; i < count; i++)
            //{
            //    thisFrame.Dequeue().draw(batch);
            //}
        }
        public void AddPermanentDraw(textures texture, Vector2 position, Color color, float scale, float rotation, int life)
        {
            addPerm.Enqueue(new DrawCommand(texture, ((position - pos) * zoom) + CameraOffsetVect, null, color, rotation, room.game.textureCenters[texture], scale * zoom, SpriteEffects.None, 0, life));
        }
        public void AddPermanentDraw(textures texture, Vector2 position, Color color, Vector2 scalevect, float rotation, int life)
        {
            addPerm.Enqueue(new DrawCommand(texture, ((position - pos) * zoom) + CameraOffsetVect, null, color, rotation, room.game.textureCenters[texture], scalevect * zoom, SpriteEffects.None, 0, life));
        }

        public void removePermanentDraw(textures texture, Vector2 position, Color color, float scale)
        {
            removePerm.Enqueue(new DrawCommand(texture, ((position - pos) * zoom) + CameraOffsetVect, null, color, 0, room.game.textureCenters[texture], scale * zoom, SpriteEffects.None, 0));
        }

        private void Work(object obj)
        {
            while (true)
            {
                CameraWaiting.Reset();
                batch.GraphicsDevice.SetRenderTarget(room.roomRenderTarget);
                lock (_locker)
                {
                    //batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, Game1.shaderEffect); //tran
                    batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                    int count = thisFrame.Count;
                    for (int i = 0; i < count; i++)
                    {
                        DrawCommand gg = thisFrame.Dequeue();

                        // ----- Shader Set Parameter Code ---------
                        float[] f;
                        f = new float[2];
                        f[0] = Program.getGame().GraphicsDevice.Viewport.Width;
                        f[1] = Program.getGame().GraphicsDevice.Viewport.Height;

                        Game1.shaderEffect.Parameters["Viewport"].SetValue(f);
                        Game1.shaderEffect.Parameters["colour"].SetValue(gg.shaderPack.colour);
                        Game1.shaderEffect.Parameters["enabled"].SetValue(gg.shaderPack.enabled);

                        // ----- End Shader Set Parameter Code ---------

                        gg.Draw(batch);
                    }
                    int permCount = permanents.Count;
                    //Console.WriteLine("1: " + permCount);
                    for (int i = 0; i < permCount; i++)//todo:proper queue iteration/remove logic
                    {
                        DrawCommand command = permanents.ElementAt(i);
                        if (command.life-- < 0)
                        {
                            permanents.Remove(command);
                            i--;
                            permCount--;
                        }
                        else
                        {
                            command.Draw(batch);
                        }
                    }
                    //Console.WriteLine("2: " + permCount);
                    batch.End();
                }
                batch.GraphicsDevice.SetRenderTarget(null);
                Program.getGame().TomShaneWaiting.Set();
                CameraWaiting.Wait();         // No more tasks - wait for a signal
            }
        }
        public void Draw(textures texture, Vector2 position, Color color, float scale, ShaderPack? shaderPack = null)
        {
            nextFrame.Enqueue(new DrawCommand(texture, ((position - pos) * zoom) + CameraOffsetVect, null, color, 0, room.game.textureCenters[texture], scale * zoom, SpriteEffects.None, 0, -1, shaderPack));
        }
        public void Draw(textures texture, Vector2 position, Color color, float scale, float rotation, ShaderPack? shaderPack = null)
        {
            nextFrame.Enqueue(new DrawCommand(texture, ((position - pos) * zoom) + CameraOffsetVect, null, color, rotation, room.game.textureCenters[texture], scale * zoom, SpriteEffects.None, 0, -1, shaderPack));
        }
        public void Draw(textures texture, Vector2 position, Color color, Vector2 scalevect, float rotation, ShaderPack? shaderPack = null)
        {
            nextFrame.Enqueue(new DrawCommand(texture, ((position - pos) * zoom) + CameraOffsetVect, null, color, rotation, room.game.textureCenters[texture], scalevect * zoom, SpriteEffects.None, 0, -1, shaderPack));
        }
        public void Draw(textures texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0, ShaderPack? shaderPack = null)
        {
            nextFrame.Enqueue(new DrawCommand(texture, ((position - pos) * zoom) + CameraOffsetVect, sourceRect, color, rotation, origin, scale * zoom, effects, layerDepth, -1, shaderPack));
        }
        public void Draw(textures texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, Vector2 scalevect, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0, ShaderPack? shaderPack = null)
        {
            nextFrame.Enqueue(new DrawCommand(texture, ((position - pos) * zoom) + CameraOffsetVect, sourceRect, color, rotation, origin, scalevect * zoom, effects, layerDepth, -1, shaderPack));
        }
        public void DrawStringWorld(string text, Vector2 position, Color color, Color? color2 = null, float scale = 0.5f, bool offset = true)
        {
            Color c2 = Color.White;
            if (color2 != null) c2 = (Color)color2;
            Vector2 pos = position * zoom;
            if (offset) pos += CameraOffsetVect;
            nextFrame.Enqueue(new DrawCommand(text, position * zoom + CameraOffsetVect, c2, scale));
            nextFrame.Enqueue(new DrawCommand(text, position * zoom + CameraOffsetVect + new Vector2(1, -1), color, scale));
        }
        public void DrawStringScreen(string text, Vector2 position, Color color, Color? color2 = null, float scale = 0.5f, bool offset = true)
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
