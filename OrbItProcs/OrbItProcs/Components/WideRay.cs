using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace OrbItProcs.Components
{
    public class WideRay : Component
    {
        public float r, g, b;
        public Queue<Vector2> positions;
        //public Queue<Color> colors;
        public Color color;
        public Queue<float> angles;
        public int queuecount;
        public int timer, timerMax;
        public double angle;
        public float rayscale;
        public int width;


        public WideRay(Node parent)
        {
            this.parent = parent;

            this.com = comp.wideray;
            this.r = this.g = this.b = 1f;

            queuecount = 10;
            angle = 0;
            timer = 0;
            timerMax = 1;
            rayscale = 20;
            width = 3;
            InitializeLists();

        }

        public override void InitializeLists()
        {
            positions = new Queue<Vector2>();
            angles = new Queue<float>();
            color = Utils.randomColor();
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
            object o = new object();
            Type t = o.GetType();
            //List<FieldInfo> fields = t.GetFields();
            
        }

        public override void AffectOther(Node other)
        {

        }
        public override void AffectSelf()
        {
            angle = Math.Atan2(parent.velocity.Y, parent.velocity.X) +(Math.PI / 2);

            timer++;
            if (timer % timerMax == 0)
            {
                if (positions.Count < queuecount)
                {
                    positions.Enqueue(parent.position);
                    angles.Enqueue((float)angle);
                }
                else
                {
                    positions.Dequeue();
                    positions.Enqueue(parent.position);
                    angles.Dequeue();
                    angles.Enqueue((float)angle);
                }
            }


        }

        public override void Draw(SpriteBatch spritebatch)
        {
            Room room = parent.room;
            float mapzoom = room.mapzoom;

            Vector2 screenPos = parent.position / mapzoom;
            Vector2 centerTexture = new Vector2(0.5f, 1);

            int count = 0;
            Vector2 scalevect = new Vector2(rayscale, width);
            foreach (Vector2 pos in positions)
            {
                //color = new Color(color.R, color.G, color.B, 255/queuecount * count);

                spritebatch.Draw(parent.room.game1.whitepixelTexture, pos/mapzoom, null, parent.color, angles.ElementAt(count), centerTexture, scalevect, SpriteEffects.None, 0);
                count++;
            }

            float testangle = (float)(Math.Atan2(parent.velocity.Y, parent.velocity.X) + (Math.PI / 2));

            spritebatch.Draw(parent.room.game1.whitepixelTexture, parent.position / mapzoom, null, parent.color, testangle, centerTexture, scalevect, SpriteEffects.None, 0);
            
        }

        public void onCollision(Dictionary<dynamic, dynamic> args)
        {
        }

    }
}
