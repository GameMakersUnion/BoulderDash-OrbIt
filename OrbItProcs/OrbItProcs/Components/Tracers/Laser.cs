using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs
{
    public class Laser : Component
    {
        public bool active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                if (parent != null && parent.comps.ContainsKey(com))
                {
                    parent.triggerSortLists();
                }
            }
        }

        private Color color;

        private int alternating = 1;
        [Polenter.Serialization.ExcludeFromSerialization]
        public int queuecount { get { if (parent != null && parent.HasComponent(comp.queuer)) return parent[comp.queuer].queuecount; else return 10; } set { if (parent != null && parent.HasComponent(comp.queuer)) parent[comp.queuer].queuecount = value; } }
        private bool _IsColorByAngle = true;
        public bool IsColorByAngle { get { return _IsColorByAngle; } set { _IsColorByAngle = value; } }


        private int timer = 0, _timerMax = 2;
        public int timerMax { get { return _timerMax; } set { _timerMax = value; } }

        private double angle = 0;

        public float lineXScale { get; set; }
        public float lineYScale { get; set; }
        public float alphaFade { get; set; }
        public float beamRatio { get; set; }

        public int beamCount { get; set; }

        public Laser() : this(null) { }
        public Laser(Node parent = null)
        {
            if (parent != null)
            {
                this.parent = parent;
            }
            com = comp.laser; 
            methods = mtypes.affectself | mtypes.draw; 
            InitializeLists();
            lineXScale = -1;
            lineYScale = -1;
            alphaFade = 0.1f;
            beamRatio = 0.7f;
            beamCount = 1;
        }

        public override void AfterCloning()
        {
            if (!parent.comps.ContainsKey(comp.queuer)) parent.addComponent(comp.queuer, true);
            //if (parent.comps.ContainsKey(comp.queuer)) 
            parent.comps[comp.queuer].qs = parent.comps[comp.queuer].qs | queues.scale | queues.position;// | queues.angle;
            //int i = 0;
        }
        public override void InitializeLists()
        {
            //positions = new Queue<Vector2>();
            //angles = new Queue<float>();
            //scales = new Queue<float>();
            color = Utils.randomColor();
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
        }
        public override void Draw(SpriteBatch spritebatch)
        {
            Room room = parent.room;
            //float mapzoom = room.zoom;

            Queue<float> scales = parent.comps[comp.queuer].scales;
            Queue<Vector2> positions = ((Queue<Vector2>)(parent.comps[comp.queuer].positions));

            //Vector2 pos = parent.body.pos;
            //Vector2 centerTexture = new Vector2(0.5f, 0.5f);

            Vector2 start = parent.body.pos;
            Vector2 end = Vector2.Zero;
            int count = 0;

            int min = Math.Min(positions.Count, scales.Count);
            for (int i = 1; i <= min; i++)
            {
                start = positions.ElementAt(i - 1);
                if (i == positions.Count) end = parent.body.pos;
                else end = positions.ElementAt(i);

                //don't draw lines from screen edge to edge if screenwrapping
                if (parent.movement.mode == movemode.screenwrap)
                {
                    float diffx = end.X - start.X;
                    if (diffx > room.worldWidth / 2)
                    {
                        start.X += room.worldWidth;
                    }
                    else if (diffx < -room.worldWidth / 2)
                    {
                        start.X -= room.worldWidth;
                    }
                    float diffy = end.Y - start.Y;
                    if (diffy > room.worldHeight / 2)
                    {
                        start.Y += room.worldHeight;
                    }
                    else if (diffy < -room.worldHeight / 2)
                    {
                        start.Y -= room.worldHeight;
                    }
                }

                Vector2 diff = (end - start);
                Vector2 centerpoint = (end + start) / 2;
                float len = diff.Length();
                Vector2 scalevect;
                float xscale = len;
                float yscale = scales.ElementAt(i - 1) * 3;
                float outerscale = yscale;
                float beamdist = 1f;
                if (lineXScale >= 0)
                {
                    xscale = lineXScale;
                }
                if (lineYScale >= 0)
                {
                    yscale = lineYScale;
                    outerscale = yscale * beamRatio;
                }

                scalevect = new Vector2(xscale, yscale);

                float testangle = (float)(Math.Atan2(diff.Y, diff.X));

                diff.Normalize();
                diff = new Vector2(-diff.Y, diff.X);

                //uncommet later when not using direction based color shit
                Color coll;
                int alpha = 255;//i * (255 / min);
                //Console.WriteLine(alpha);
                if (IsColorByAngle)
                {
                    int[] collarr = HueShifter.getColorsFromAngle((testangle + (float)Math.PI) * (float)(180 / Math.PI));
                    coll = new Color(collarr[0], collarr[1], collarr[2], alpha);
                }
                else
                {
                    coll = new Color(parent.body.color.R, parent.body.color.G, parent.body.color.B, alpha);
                }

                room.camera.Draw(textures.whitepixeltrans, centerpoint, new Color(255, 255, 255, coll.A), scalevect, testangle);
                
                scalevect.Y = outerscale * 0.9f;
                //int beamnum = 0;
                int sign = 1;
                for (int j = 0; j < beamCount; j++)
                {
                    if (j % 2 == 0) beamdist = (outerscale * j + yscale) / 2f;
                    room.camera.Draw(textures.whitepixeltrans, centerpoint + diff * beamdist * sign, coll, scalevect, testangle);
                    sign *= -1;
                    //room.camera.Draw(textures.whitepixeltrans, centerpoint - diff * beamdist, coll, scalevect, testangle);
                }
                count++;
            }

        }

    }
}
