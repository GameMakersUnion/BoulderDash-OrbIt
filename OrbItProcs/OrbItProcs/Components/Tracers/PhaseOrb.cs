using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace OrbItProcs.Components
{
    public class PhaseOrb : Component
    {
        [Polenter.Serialization.ExcludeFromSerialization]
        public Node parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                //if (value != null) UpdateParent();
            }
        }

        private int _queuecount = 10;
        public int queuecount { get { return _queuecount; } set { _queuecount = value; } }

        private float r1 = Utils.random.Next(255) / 255f;
        private float g1 = Utils.random.Next(255) / 255f;
        private float b1 = Utils.random.Next(255) / 255f;
        private int timer = 0;
        private int _timerMax = 1;
        public int timerMax { get { return _timerMax; } set { _timerMax = value; } }

        //public Queue<Vector2> positions;
        //public Queue<float> scales;

        public PhaseOrb() : this(null) { }
        public PhaseOrb(Node parent = null)
        {
            if (parent != null)
            {
                this.parent = parent;
                

            }
            com = comp.phaseorb; 
            methods = mtypes.affectself | mtypes.draw; 
            InitializeLists(); 
            
        }

        public override void AfterCloning()
        {
            if (!parent.comps.ContainsKey(comp.queuer)) parent.addComponent(comp.queuer, true);
            //if (parent.comps.ContainsKey(comp.queuer)) 
                parent.comps[comp.queuer].qs = parent.comps[comp.queuer].qs | queues.scale | queues.position;
                //int i = 0;
        }

        public override void InitializeLists()
        {
            //positions = new Queue<Vector2>();
            //scales = new Queue<float>();

        }

        public override void Initialize(Node parent)
        {
            this.parent = parent;
            r1 = Utils.random.Next(255) / 255f;
            g1 = Utils.random.Next(255) / 255f;
            b1 = Utils.random.Next(255) / 255f;
        }

        public override void AffectOther(Node other)
        {
            
        }
        public override void AffectSelf()
        {
            //angle = Math.Atan2(parent.velocity.Y, parent.velocity.X) + (Math.PI / 2);
            /*
            if (++timer % timerMax == 0)
            {
                
                if (positions.Count < queuecount)
                {
                    //positions.Enqueue(parent.position);
                    //scales.Enqueue(parent.scale);
                }
                else
                {
                    //positions.Dequeue();
                    //positions.Enqueue(parent.position);
                    //scales.Dequeue();
                    //scales.Enqueue(parent.scale);
                }
                

            }
            */


        }

        public override void Draw(SpriteBatch spritebatch)
        {
            Room room = parent.room;
            float mapzoom = room.mapzoom;

            Color col = new Color(0, 0, 0, 0.3f);
            float a, b, c;
            a = b = c = 0;

            //Vector2 screenPos = parent.position / mapzoom;

            int count = 0;
            //foreach (Vector2 pos in positions)
            //Queue<float> scales = ((Queue<float>)(parent.comps[comp.queuer].scales));
            Queue<float> scales = parent.comps[comp.queuer].scales;
            Queue<Vector2> positions = ((Queue<Vector2>)(parent.comps[comp.queuer].positions));
            //float t = parent.comps[comp.queuer].scales.ElementAt(2);
            //Console.WriteLine(scales.Count + " :: " + positions.Count);

            foreach (float s in scales)
            {
                //color = new Color(color.R, color.G, color.B, 255/queuecount * count);
                a += r1 / 10;
                b += g1 / 10;
                c += b1 / 10;
                col = new Color(a, b, c, 0.8f);
                if (parent.comps.ContainsKey(comp.hueshifter) && parent.comps[comp.hueshifter].active) col = parent.color;

                //float scale = scales.ElementAt(count) / mapzoom;
                //float scale = parent.scale;
                //if (parent.comps.ContainsKey(comp.queuer))
                //{
                //    if (parent.comps[comp.queuer].scales.Count > count)
                    //scale = ((Queue<float>)(parent.comps[comp.queuer].scales)).ElementAt(count) / mapzoom;
                
                //}

                spritebatch.Draw(parent.getTexture(), positions.ElementAt(count) / mapzoom, null, col, 0, parent.TextureCenter(), s / mapzoom , SpriteEffects.None, 0);
                count++;
            }

            //float testangle = (float)(Math.Atan2(parent.velocity.Y, parent.velocity.X) + (Math.PI / 2));
            if (parent.comps.ContainsKey(comp.hueshifter)) col = parent.color;
            spritebatch.Draw(parent.getTexture(), parent.position / mapzoom, null, col, 0, parent.TextureCenter(), parent.scale / mapzoom, SpriteEffects.None, 0);

        }

        public void onCollision(Dictionary<dynamic, dynamic> args)
        {
        }

    }
}
