using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace OrbItProcs.Components
{
    public class LaserTimers : Component {
        
        private float _maxscale = 20, _currentscale = 1;
        public float maxscale { get { return _maxscale; } set { _maxscale = value; } }
        public float currentscale { get { return _currentscale; } set { _currentscale = value; } }
        public Queue<Vector2> positions;
        public Queue<float> angles;
        public List<int> collisionTimers;
        public List<float> scales;
        public List<Vector2> scalesVects;
        
        private int _queuecount = 10, _scalingcounter = -1, _timerMax = 2;
        public int queuecount { get { return _queuecount; } set { _queuecount = value; } }
        public int scalingcounter { get { return _scalingcounter; } set { _scalingcounter = value; } }
        private int timer = 0, timer2 = 0;
        public int timerMax { get { return _timerMax; } set { _timerMax = value; } }

        private double angle = 0;

        public LaserTimers() { com = comp.lasertimers; InitializeLists(); }

        public LaserTimers(Node parent)
        {
            this.parent = parent;
            parent.Collided += onCollision;
            this.com = comp.lasertimers;

            //this.maxscale = 20;
            //this.currentscale = 1;
            
            //queuecount = 10;
            //timer = timer2 = 0;
            //timerMax = 2;
            //angle = 0;
            //scalingcounter = -1;

            InitializeLists();

        }

        public override void InitializeLists()
        {
            positions = new Queue<Vector2>();
            angles = new Queue<float>();
            collisionTimers = new List<int>();
            scales = new List<float>();
            scalesVects = new List<Vector2>();
            for (int i = 0; i < queuecount; i++)
            {
                collisionTimers.Add(0);
                scales.Add(maxscale);
                scalesVects.Add(new Vector2(1, maxscale));
            }
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
            angle = Math.Atan2(parent.velocity.Y, parent.velocity.X) + (Math.PI / 2);

            if (timer > timerMax)
            {
                timer = 0;
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

            if (scalingcounter != -1)
            {
                //Console.WriteLine("ok: {0}", scalingcounter);
                if (timer2 > timerMax)
                {
                    timer2 = 0;
                    //scales[scalingcounter] = maxscale / queuecount;
                    scales[queuecount - scalingcounter - 1] = 1f;
                    scalingcounter++;
                    //Console.WriteLine("scalingcounter: {0}", scalingcounter);
                }
                else
                {
                    timer2++;
                }

            }
            if (scalingcounter >= queuecount)
            {
                scalingcounter = -1;
                //Console.WriteLine("queueddd");
            }

        }

        public override void Draw(SpriteBatch spritebatch)
        {
            //it would be really cool to have some kind of blending effects so that every combination of components will look diff

            Room room = parent.room;
            float mapzoom = room.mapzoom;

            parent.scale = 1 / mapzoom;
            float screenx = parent.position.X / mapzoom;
            float screeny = parent.position.Y / mapzoom;
            Vector2 center = new Vector2((float)getTexture().Width / 2f, (float)getTexture().Height / 2f);
            Vector2 scaling = Vector2.One;
            scaling = new Vector2(1, maxscale);

            float scaleincrement = maxscale / (queuecount * 50);
            //scaleincrement = 0;
            for (int i = 0; i < queuecount; i++)
            {
                if (scales[i] < maxscale)
                {
                    scales[i] += scaleincrement;
                    if (scales[i] > maxscale)
                    {
                        scales[i] = maxscale;
                    }
                }
            }

            Color col = new Color(0f, 0f, 0f);

            int count = 0;
            foreach (Vector2 pos in positions)
            {
                float ang = angles.ElementAt(count);
                float red = (float)count * (255f/queuecount) / 255f;
                //Console.WriteLine("count: {0}, red: {1}", count, red);
                col = new Color(red, red, red);
                //if (scales[count] < maxscale)
                //{
                    scalesVects[count] = new Vector2(1, scales[count]);
                //}
                    //spritebatch.Draw(parent.room.game1.textureDict[textures.whitepixel], pos / mapzoom, null, Color.White, ang, center, scaling, SpriteEffects.None, 0);
                spritebatch.Draw(parent.getTexture(textures.whitepixel), pos / mapzoom, null, col, ang, center, scalesVects[count], SpriteEffects.None, 0);
                count++;
            }
            //spritebatch.Draw(parent.room.game1.textureDict[textures.whitepixel], new Vector2(screenx, screeny), null, Color.White, (float)angle, center, scaling, SpriteEffects.None, 0);
            spritebatch.Draw(parent.getTexture(textures.whitepixel), new Vector2(screenx, screeny), null, Color.White, (float)angle, center, scalesVects[queuecount - 1], SpriteEffects.None, 0);
            
            //test
            /*
            Vector2 centerTexture = new Vector2(0.5f, 1);
            Vector2 start = new Vector2(200, 200);

            Vector2 end = new Vector2(room.game1.sWidth, room.game1.sHeight);
            Vector2 diff = (end - start) / mapzoom;
            Vector2 centerpoint = (end + start) / 2;
            centerpoint /= mapzoom;
            float len = diff.Length();
            //diagonal /= 2;
            //len /= 3;
            Vector2 scalevect = new Vector2(len, 1);
            float testangle = (float)(Math.Atan2(diff.Y, diff.X));// + (Math.PI / 2));
            */
            //spritebatch.Draw(parent.texture, new Vector2(screenx, screeny), null, Color.White, 0, new Vector2(parent.texture.Width / 2, parent.texture.Height / 2), parent.scale, SpriteEffects.None, 0);
            //spritebatch.Draw(parent.props[properties.core_texture], parent.props[properties.core_position], null, Color.White, 0, new Vector2(parent.props[properties.core_texture].Width / 2, parent.props[properties.core_texture].Height / 2), 1f, SpriteEffects.None, 0);
        }

        public void onCollision(Dictionary<dynamic, dynamic> args)
        {
            currentscale = maxscale / queuecount;
            scalingcounter = 1;
            scales[queuecount-1] = 1f;
        }

    }
}
