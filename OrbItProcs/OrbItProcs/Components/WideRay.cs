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
        public Queue<Vector2> positions;
        public Queue<float> angles;
        private int _queuecount = 10;
        public int queuecount { get { return _queuecount; } set { _queuecount = value; } }

        private int timer = 0, _timerMax = 1;
        public int timerMax { get { return _timerMax; } set { _timerMax = value; } }

        public double angle = 0;
        public float rayscale = 20;
        public int width = 3;

        public WideRay() { com = comp.wideray; InitializeLists(); }
        public WideRay(Node parent)
        {
            this.parent = parent;
            this.com = comp.wideray;
            /*
            queuecount = 10;
            angle = 0;
            timer = 0;
            timerMax = 1;
            rayscale = 20;
            width = 3;
            */
            InitializeLists();

        }

        public override void InitializeLists()
        {
            positions = new Queue<Vector2>();
            angles = new Queue<float>();
        }

        public override bool hasMethod(string methodName)
        {
            methodName = methodName.ToLower();
            if (methodName.Equals("affectother")) return false;
            if (methodName.Equals("affectself")) return true;
            if (methodName.Equals("draw")) return true;
            else return false;
        }

        public override void Initialize(Node parent)
        {
            this.parent = parent;
            
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
                spritebatch.Draw(parent.getTexture(textures.whitepixel), pos/mapzoom, null, parent.color, angles.ElementAt(count), centerTexture, scalevect, SpriteEffects.None, 0);
                count++;
            }

            float testangle = (float)(Math.Atan2(parent.velocity.Y, parent.velocity.X) + (Math.PI / 2));

            spritebatch.Draw(parent.getTexture(textures.whitepixel), parent.position / mapzoom, null, parent.color, testangle, centerTexture, scalevect, SpriteEffects.None, 0);
            
        }

        public void onCollision(Dictionary<dynamic, dynamic> args)
        {
        }

    }
}
