using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace OrbItProcs
{
    /// <summary>
    /// Nodes will leave behind a trail consisting of fading images of themselves.
    /// </summary>
    [Info(UserLevel.User, "Nodes will leave behind a trail consisting of fading images of themselves.", CompType)]
    public class PhaseOrb : Component
    {
        public const mtypes CompType = mtypes.minordraw | mtypes.tracer;
        public override mtypes compType { get { return CompType; } set { } }

        public int _phaserLength = 10;
        /// <summary>
        /// Sets the length of the phaser.
        /// </summary>
        [Info(UserLevel.User, "Sets the length of the phaser. ")]
        [Polenter.Serialization.ExcludeFromSerialization]
        public int phaserLength
        {
            get
            {
                return _phaserLength;
            }
            set
            {
                if (parent != null && parent.HasComponent(comp.queuer) && parent[comp.queuer].queuecount < value)
                {
                    parent[comp.queuer].queuecount = value;
                }
                _phaserLength = value;
            }
        }

        public Toggle<int> fade { get; set; }
        private int r1;
        private int g1;
        private int b1;
        //private int timer = 0;

        public PhaseOrb() : this(null) { }
        public PhaseOrb(Node parent = null)
        {
            if (parent != null)
            {
                this.parent = parent;
            }
            com = comp.phaseorb; 
            InitializeLists(); 
            fade = new Toggle<int>(phaserLength);
            
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

        public override void OnSpawn()
        {
            //r1 = Utils.random.Next(255) / 255f;
            //g1 = Utils.random.Next(255) / 255f;
            //b1 = Utils.random.Next(255) / 255f;
        }

        public override void AffectOther(Node other)
        {
            
        }
        public override void AffectSelf()
        {
        }
        public override void Draw()
        {
            parent.room.camera.AddPermanentDraw(parent.texture, parent.body.pos, parent.body.color, parent.body.scale);
        }


        public void oldDraw()
        {
            Room room = parent.room;
            float mapzoom = room.zoom;

            Color col = new Color(0, 0, 0, 0.3f);
            
            int a, b, c;
            a = b = c = 0;
            r1 = (int)parent.body.color.R;
            g1 = (int)parent.body.color.G;
            b1 = (int)parent.body.color.B;

            //Vector2 screenPos = parent.transform.position / mapzoom;
            bool IsPolygon = parent.body.shape is Polygon;
            int count = 0;
            //foreach (Vector2 pos in positions)
            //Queue<float> scales = ((Queue<float>)(parent.comps[comp.queuer].scales));
            Queue<float> scales = parent.comps[comp.queuer].scales;
            Queue<Vector2> positions = ((Queue<Vector2>)(parent.comps[comp.queuer].positions));
            //float t = parent.comps[comp.queuer].scales.ElementAt(2);
            //Console.WriteLine(scales.Count + " :: " + positions.Count);
            int min = Math.Min(positions.Count, scales.Count);
            
            for (int i = 0; i < min; i++)
            {
                if (fade.enabled)
                {
                    //color = new Color(color.R, color.G, color.B, 255/queuecount * count);
                    a += r1 / fade;
                    b += g1 / fade;
                    c += b1 / fade;
                    col = new Color(a, b, c, 200);
                }
                //if (parent.comps.ContainsKey(comp.hueshifter) && parent.comps[comp.hueshifter].active) col = parent.body.color;

                //float scale = scales.ElementAt(count) / mapzoom;
                //float scale = parent.transform.scale;
                //if (parent.comps.ContainsKey(comp.queuer))
                //{
                //    if (parent.comps[comp.queuer].scales.Count > count)
                    //scale = ((Queue<float>)(parent.comps[comp.queuer].scales)).ElementAt(count) / mapzoom;
                
                //}
                
                if (IsPolygon)
                {
                    (parent.body.shape as Polygon).DrawPolygon(positions.ElementAt(i), col);
                }
                if (!IsPolygon || (IsPolygon && parent.body.DrawCircle))
                {
                    //spritebatch.Draw(parent.getTexture(), positions.ElementAt(i) * mapzoom, null, col, 0, parent.TextureCenter(), scales.ElementAt(i) * mapzoom, SpriteEffects.None, 0);
                    room.camera.Draw(parent.texture, positions.ElementAt(i), col, scales.ElementAt(i), 0);
                }

                count++;
            }

            //float testangle = (float)(Math.Atan2(parent.transform.velocity.Y, parent.transform.velocity.X) + (Math.PI / 2));
            
            if (!fade) col = parent.body.color;
            if (!IsPolygon || (IsPolygon && parent.body.DrawCircle))
            {
                //spritebatch.Draw(parent.getTexture(), parent.body.pos * mapzoom, null, col, 0, parent.TextureCenter(), parent.body.scale * mapzoom, SpriteEffects.None, 0);
                room.camera.Draw(parent.texture, parent.body.pos, col, parent.body.scale);
            }

        }

        public void onCollision(Dictionary<dynamic, dynamic> args)
        {
        }

    }
}
