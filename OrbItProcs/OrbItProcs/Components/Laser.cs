using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs.Components
{
    public class Laser : Component
    {
        public float r, g, b;
        public Queue<Vector2> positions;
        //public Queue<Color> colors;
        public Color color;
        public Queue<float> angles;
        public int queuecount;
        public int timer, timerMax;
        public double angle;
        public HueShifter hshift;

        public Laser(Node parent)
        {
            this.parent = parent;
            parent.Collided += onCollision;

            this.com = comp.laser;
            this.r = this.g = this.b = 1f;

            queuecount = 10;
            angle = 0;
            timer = 0;
            timerMax = 2;

            InitializeLists();

        }

        public override void InitializeLists() //fix this elsewhere!
        {
            positions = new Queue<Vector2>();
            angles = new Queue<float>();
            color = Utils.randomColor();
            hshift = new HueShifter(null);
            //colors = new Queue<Color>();

        }

        public override bool hasMethod(string methodName)
        {
            methodName = methodName.ToLower();
            if (methodName.Equals("affectother")) return false;
            if (methodName.Equals("affectself")) return true;
            if (methodName.Equals("draw")) return true;
            else return false;
        }

        public override void Initialize()
        {

        }

        public override void AffectOther(Node other)
        {

        }
        public override void AffectSelf()
        {
            angle = Math.Atan2(parent.velocity.Y, parent.velocity.X) + (Math.PI / 2);

            if (timer > timerMax)
            {
                timer = 0;
                if (positions.Count < queuecount)
                {
                    positions.Enqueue(parent.position);
                    //colors.Enqueue(Utils.randomColor());
                    //angles.Enqueue((float)angle);
                }
                else
                {
                    positions.Dequeue();
                    //colors.Dequeue();
                    positions.Enqueue(parent.position);
                    //colors.Enqueue(Utils.randomColor());

                    //angles.Dequeue();
                    //angles.Enqueue((float)angle);
                    /*
                    Vector2 v0 = parent.position - positions.ElementAt(0);
                    Vector2 vq = parent.position - positions.ElementAt(8);
                    Console.WriteLine("Dif (0 p): {0}, Dif (count p): {1}", v0.Length(), vq.Length());
                     */
                }
            }
            else
            {
                timer++;
            }

        }

        public override void Draw(SpriteBatch spritebatch)
        {
            //it would be really cool to have some kind of blending effects so that every combination of components will look diff
            //parent.scale = 1 / mapzoom;
            Room room = parent.room;
            float mapzoom = room.mapzoom;
            //Color col = new Color(0f, 0f, 0f);

            Vector2 screenPos = parent.position / mapzoom;
            Vector2 centerTexture = new Vector2(0.5f, 1);

            Vector2 start = parent.position;
            Vector2 end = Vector2.Zero;
            int count = 0;

            //foreach (Vector2 pos in positions)
            for (int i = 1; i <= positions.Count; i++)
            {
                //Vector2 pos = positions.ElementAt(i);
                float red = (float)i * (255f / queuecount) / 255f;
                //col = new Color(red, red, red);
                //col = Utils.randomColor();

                start = positions.ElementAt(i-1);
                if (i == positions.Count) end = parent.position;
                else end = positions.ElementAt(i);
                
                Vector2 diff = (end - start) / mapzoom;
                Vector2 centerpoint = (end + start) / 2;
                centerpoint /= mapzoom;
                float len = diff.Length();
                Vector2 scalevect = new Vector2(len, 1);
                float testangle = (float)(Math.Atan2(diff.Y, diff.X));// + (Math.PI / 2));
                
                diff.Normalize();
                diff = new Vector2(diff.Y, diff.X);

                //uncommet later when not using direction based color shit
                //color = new Color(color.R, color.G, color.B, 255/queuecount * i);
                //Console.WriteLine(testangle);
                int[] collarr = hshift.getColorsFromAngle((testangle + (float)Math.PI) * (float)(180/Math.PI));
                Color coll = new Color(collarr[0], collarr[1], collarr[2]);
                
                spritebatch.Draw(parent.room.game1.whitepixelTexture, centerpoint, null, new Color(1f, 1f, 1f, 255 / queuecount * i), testangle, centerTexture, scalevect, SpriteEffects.None, 0);
                spritebatch.Draw(parent.room.game1.whitepixelTexture, centerpoint + diff, null, /*parent.color*/coll, testangle, centerTexture, scalevect, SpriteEffects.None, 0);
                spritebatch.Draw(parent.room.game1.whitepixelTexture, centerpoint - diff, null, /*parent.color*/coll, testangle, centerTexture, scalevect, SpriteEffects.None, 0);
                count++;
            }
            //spritebatch.Draw(parent.room.game1.whitepixelTexture, new Vector2(screenx, screeny), null, Color.White, (float)angle, center, scaling, SpriteEffects.None, 0);
            //spritebatch.Draw(parent.room.game1.whitepixelTexture, screenPos, null, Color.White, (float)angle, center, scalesVects[queuecount - 1], SpriteEffects.None, 0);

            //test
            /*
            Vector2 start = new Vector2(200, 200);
            Vector2 end = new Vector2(room.game1.sWidth, room.game1.sHeight);

            //Vector2 diff = (end - start) / mapzoom;
            Vector2 centerpoint = (end + start) / 2;
            centerpoint /= mapzoom;
            float len = diff.Length();

            Vector2 scalevect = new Vector2(len, 1);
            float testangle = (float)(Math.Atan2(diff.Y, diff.X));// + (Math.PI / 2));
            spritebatch.Draw(parent.room.game1.colororbTexture, start / mapzoom, null, Color.White, 0f, new Vector2(25f, 25f), 1 / mapzoom, SpriteEffects.None, 0);
            spritebatch.Draw(parent.room.game1.colororbTexture, end / mapzoom, null, Color.White, 0f, new Vector2(25f, 25f), 1 / mapzoom, SpriteEffects.None, 0);
            spritebatch.Draw(parent.room.game1.colororbTexture, centerpoint, null, Color.White, 0f, new Vector2(25f, 25f), 1 / mapzoom, SpriteEffects.None, 0);
            spritebatch.Draw(parent.room.game1.whitepixelTexture, centerpoint, null, Color.White, testangle, centerTexture, scalevect, SpriteEffects.None, 0);
            */
            //spritebatch.Draw(parent.texture, new Vector2(screenx, screeny), null, Color.White, 0, new Vector2(parent.texture.Width / 2, parent.texture.Height / 2), parent.scale, SpriteEffects.None, 0);
            //spritebatch.Draw(parent.props[properties.core_texture], parent.props[properties.core_position], null, Color.White, 0, new Vector2(parent.props[properties.core_texture].Width / 2, parent.props[properties.core_texture].Height / 2), 1f, SpriteEffects.None, 0);
        }

        public void onCollision(Dictionary<dynamic, dynamic> args)
        {
            //currentscale = maxscale / queuecount;
            //scalingcounter = 1;
            //scales[queuecount - 1] = 1f;
        }

    }
}
