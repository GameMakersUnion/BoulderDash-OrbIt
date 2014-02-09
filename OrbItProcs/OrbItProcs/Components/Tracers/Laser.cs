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
                if (!value && parent != null) parent.Collided -= onCollision;
                if (parent != null && parent.comps.ContainsKey(com))
                {
                    parent.triggerSortLists();
                }
            }
        }

        private Color color;

        private int alternating = 1;

        private int _queuecount = 10;
        public int queuecount { get { return _queuecount; } set { _queuecount = value; } }
        private bool _IsColorByAngle = true;
        public bool IsColorByAngle { get { return _IsColorByAngle; } set { _IsColorByAngle = value; } }


        private int timer = 0, _timerMax = 2;
        public int timerMax { get { return _timerMax; } set { _timerMax = value; } }

        private double angle = 0;

        public float lineXScale { get; set; }
        public float lineYScale { get; set; }
        public float alphaFade { get; set; }
        public float beamRatio { get; set; }

        public Laser() : this(null) { }
        public Laser(Node parent = null)
        {
            if (parent != null)
            {
                this.parent = parent;
                this.parent.Collided += onCollision;
            }
            com = comp.laser; 
            methods = mtypes.affectself | mtypes.draw; 
            InitializeLists();
            lineXScale = -1;
            lineYScale = -1;
            alphaFade = 0.1f;
            beamRatio = 0.5f;
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
            parent.Collided += onCollision;

            //AfterCloning();
        }

        public override void AffectOther(Node other)
        {

        }
        public override void AffectSelf()
        {
        }

        

        public override void Draw(SpriteBatch spritebatch)
        {
            //it would be really cool to have some kind of blending effects so that every combination of components will look diff
            Room room = parent.room;
            float mapzoom = room.mapzoom;
            //Color col = new Color(0f, 0f, 0f);

            Queue<float> scales = parent.comps[comp.queuer].scales;
            //Queue<float> angles = parent.comps[comp.queuer].angles;
            Queue<Vector2> positions = ((Queue<Vector2>)(parent.comps[comp.queuer].positions));

            Vector2 screenPos = parent.body.position / mapzoom;
            Vector2 centerTexture = new Vector2(0.5f, 0.5f);

            Vector2 start = parent.body.position;
            Vector2 end = Vector2.Zero;
            int count = 0;

            //foreach (Vector2 pos in positions)
            int min = Math.Min(positions.Count, scales.Count);
            for (int i = 1; i <= min; i++)
            {
                //Vector2 pos = positions.ElementAt(i);
                //float red = (float)i * (255f / queuecount) / 255f;
                //col = new Color(red, red, red);
                //col = Utils.randomColor();

                start = positions.ElementAt(i-1);
                if (i == positions.Count) end = parent.body.position;
                else end = positions.ElementAt(i);
                
                Vector2 diff = (end - start) / mapzoom;
                Vector2 centerpoint = (end + start) / 2;
                centerpoint /= mapzoom;
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

                float testangle = (float)(Math.Atan2(diff.Y, diff.X));// + (Math.PI / 2));
                
                diff.Normalize();
                diff = new Vector2(-diff.Y, diff.X);
                //diff *= beamdist;


                //uncommet later when not using direction based color shit
                //color = new Color(color.R, color.G, color.B, 255/queuecount * i);
                //Console.WriteLine(testangle);
                Color coll;
                if (IsColorByAngle)
                {
                    int[] collarr = HueShifter.getColorsFromAngle((testangle + (float)Math.PI) * (float)(180 / Math.PI));
                    coll = new Color(collarr[0], collarr[1], collarr[2]);
                }
                else
                {
                    coll = parent.body.color;
                }

                //if (alphaFade > 0) coll.A = (byte)(alphaFade * i * 255);

                //coll.A = (byte)i;
                //Console.WriteLine("{0} + {1}", i, coll.A);
                //coll = new Color(0.5f, 0.5f, 0.5f, 0f);

                spritebatch.Draw(parent.getTexture(textures.whitepixeltrans), centerpoint, null, new Color(255, 255, 255, coll.A), testangle, centerTexture, scalevect, SpriteEffects.None, 0);
                scalevect.Y = outerscale * 0.9f;
                //spritebatch.Draw(parent.getTexture(textures.whitepixel), centerpoint + diff, null, /*parent.transform.color*/coll, testangle, centerTexture, scalevect, SpriteEffects.None, 0);
                //spritebatch.Draw(parent.getTexture(textures.whitepixel), centerpoint - diff, null, /*parent.transform.color*/coll, testangle, centerTexture, scalevect, SpriteEffects.None, 0);

                for (int j = 0; j < 4; j++)
                {
                    beamdist = (outerscale + j * yscale) / 2f;
                    //coll = new Color(new Vector4(coll.R, coll.G, coll.B, coll.A) - new Vector4(5, 5, 5, 0));
                    spritebatch.Draw(parent.getTexture(textures.whitepixeltrans), centerpoint + diff * beamdist, null, /*parent.transform.color*/coll, testangle, centerTexture, scalevect, SpriteEffects.None, 0);
                    spritebatch.Draw(parent.getTexture(textures.whitepixeltrans), centerpoint - diff * beamdist, null, /*parent.transform.color*/coll, testangle, centerTexture, scalevect, SpriteEffects.None, 0);

                }

                //spritebatch.Draw(parent.getTexture(textures.whitepixel), centerpoint + diff, null, /*parent.transform.color*/coll, testangle, centerTexture, scalevect, SpriteEffects.None, 0);
                //spritebatch.Draw(parent.getTexture(textures.whitepixel), centerpoint - diff, null, /*parent.transform.color*/coll, testangle, centerTexture, scalevect, SpriteEffects.None, 0);

                //alternating *= -1;
                //diff *= alternating * 3;
                //spritebatch.Draw(parent.getTexture(textures.whitepixel), centerpoint + diff, null, /*parent.transform.color*/coll, testangle, centerTexture, scalevect, SpriteEffects.None, 0);
                count++;
            }
            
        }

        public void onCollision(Dictionary<dynamic, dynamic> args)
        {
            //currentscale = maxscale / queuecount;
            //scalingcounter = 1;
            //scales[queuecount - 1] = 1f;
        }

    }
}
