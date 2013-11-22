using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs {
    public class FrameRateCounter {
        ContentManager content;
        //SpriteBatch spriteBatch;
        //SpriteFont spriteFont;
        Game1 game;
        int frameRate = 0;
        int frameCounter = 0;
        int updateRate = 0;
        public int updateCounter = 0;
        public TimeSpan elapsedTime = TimeSpan.Zero;


        public FrameRateCounter(Game1 game)
        {
            //content = new ContentManager(game.Services);
            this.game = game;
        }

        public void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;
            updateCounter++;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
                updateRate = updateCounter;
                updateCounter = 0;

            }
        }

        public void UpdateElapsed(TimeSpan elapsed)
        {
            elapsedTime += elapsed;
            updateCounter++;
            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
                updateRate = updateCounter;
                updateCounter = 0;

            }
        }


        public void Draw(SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            frameCounter++;

            string fps = string.Format("fps: {0}", frameRate);
            string ups = string.Format("ups: {0}", updateRate);
            //spriteBatch.Begin();

            spriteBatch.DrawString(spriteFont, fps, new Vector2(2, game.sHeight - 70), Color.White, 0f, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);
            //spriteBatch.DrawString(spriteFont, fps, new Vector2(1, 1), Color.White, 0f, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);
            spriteBatch.DrawString(spriteFont, ups, new Vector2(2, game.sHeight - 40), Color.White, 0f, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);

            //spriteBatch.End();
        }
    }
}
