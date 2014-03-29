using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs {
    public class FrameRateCounter {
        //ContentManager content;
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
            int y1 = 70;

            string fps = string.Format("fps: {0}", frameRate);
            string ups = string.Format("ups: {0}", updateRate);
            string process = "";
            //string fpsups = string.Format("fps:{0} ups:{1}", frameRate, updateRate);
            Room room = Program.getRoom();
            bool hasProcess = room != null && room.game.ui.keyManager.TemporaryProcess != null;
            if (hasProcess)
            {
                y1 += 30;
                process = room.game.ui.keyManager.TemporaryProcess.GetType().ToString().LastWord('.');
            }

            room.camera.DrawStringScreen(fps, new Vector2(0, Game1.Height - y1), Color.Black);
            y1 -= 30;
            room.camera.DrawStringScreen(ups, new Vector2(0, Game1.Height - y1), Color.Black);
            y1 -= 30;
            if (hasProcess) room.camera.DrawStringScreen(process, new Vector2(0, Game1.Height - y1), Color.Black);

            if (room.masterGroup != null)
            {
                string count = room.generalGroups.fullSet.Count.ToString();
                int x = Game1.Width - (count.Length * 7) - 20;
                room.camera.DrawStringScreen(count, new Vector2(x, Game1.Height - y1), Color.Black, offset: false);
            }

            //draw player scores
            Vector2 pos = new Vector2(1, 2);
            //foreach(var p in room.players)
            //{
            //    string score = (p.score / 100f).ToString();
            //    room.camera.DrawStringScreen(score, pos, p.pColor, scale: 1f);
            //    //spriteBatch.DrawString(spriteFont, score, pos, p.pColor, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
            //    //spriteBatch.DrawString(spriteFont, score, new Vector2(pos.X - 1, pos.Y - 1), p.pColor, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
            //    pos.X += 100;
            //
            //}
        }
    }
}
