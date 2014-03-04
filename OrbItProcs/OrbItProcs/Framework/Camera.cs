using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public class Camera
    {
        public Room room;
        public float zoom;
        public Vector2 pos;
        public SpriteBatch batch;
        public Camera(Room room, float zoom = 0.5f, Vector2? pos = null)
        {
            this.room = room;
            this.batch = room.game.spriteBatch;
            this.zoom = zoom;
            this.pos = pos ?? Vector2.Zero;
        }
        public void Draw(textures texture, Vector2 position, Color color, float scale)
        {
            batch.Draw(room.game.textureDict[texture], (position - pos) * zoom, null, color, 0, room.game.textureCenters[texture], scale * zoom, SpriteEffects.None, 0);
        }
        public void Draw(textures texture, Vector2 position, Color color, float scale, float rotation)
        {
            batch.Draw(room.game.textureDict[texture], (position - pos) * zoom, null, color, rotation, room.game.textureCenters[texture], scale * zoom, SpriteEffects.None, 0);
        }
        public void Draw(textures texture, Vector2 position, Color color, Vector2 scalevect, float rotation)
        {
            batch.Draw(room.game.textureDict[texture], (position - pos) * zoom, null, color, rotation, room.game.textureCenters[texture], scalevect * zoom, SpriteEffects.None, 0);
        }
        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0)
        {
            batch.Draw(texture, (position - pos) * zoom, sourceRectangle, color, rotation, origin, scale * zoom, effects, layerDepth);
        }
        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scalevect, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0)
        {
            batch.Draw(texture, (position - pos) * zoom, sourceRectangle, color, rotation, origin, scalevect * zoom, effects, layerDepth);
        }
        

    }
}
