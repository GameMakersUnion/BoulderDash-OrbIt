using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs.Components
{
    public class HueShifter : Component
    {
        private int timer = 0, _timerMax = 2;
        public int timerMax { get { return _timerMax; } set { _timerMax = value; } }

        private int[] rgb = new int[3];
        private int _inc = 5;
        public int inc { get { return _inc; } set { _inc = value; } }

        private int pos = 1, sign = 1;
        private int angle = 0;

        public bool smartshifting { get; set; }

        public HueShifter() : this(null) { }
        public HueShifter(Node parent = null)
        {
            smartshifting = true;
            if (parent != null) this.parent = parent;
            com = comp.hueshifter; 
            methods = mtypes.affectself; 
        }

        public override void Initialize(Node parent)
        {
            this.parent = parent;
        }

        public override void AffectSelf()
        {
            timer++;
            if ((timer) % timerMax == 0)
            {
                int range = 20;
                int tempinc = inc;
                if (angle % 120 > 60 - range && angle % 120 < 60 + range)
                {
                    tempinc = inc * 2;
                    //Console.WriteLine(angle);
                }

                int[] cols = getColorsFromAngle(angle);
                parent.transform.color = new Color(cols[0], cols[1], cols[2], 1);
                angle = (angle + tempinc) % 360;
                
            }

        }

        public static int[] getColorsFromAngle(float angle)
        { 
            int[] cols = new int[3];
            cols[0] = 0;
            cols[1] = 0;
            cols[2] = 0;

            double perdegree = 255.0 / 60.0;

            if (angle < 60)
            {
                cols[0] = 255;
                cols[1] = (int)(perdegree * angle);
            }
            else if (angle < 120)
            {
                cols[1] = 255;
                cols[0] = 255 - ((int)(perdegree * (angle - 60)));
            }
            else if (angle < 180)
            {
                cols[1] = 255;
                cols[2] = (int)(perdegree * (angle - 120));
            }
            else if (angle < 240)
            {
                cols[2] = 255;
                cols[1] = 255 - ((int)(perdegree * (angle - 180)));
            }
            else if (angle < 300)
            {
                cols[2] = 255;
                cols[0] = (int)(perdegree * (angle - 240));
            }
            else if (angle < 360)
            {
                cols[0] = 255;
                cols[2] = 255 - ((int)(perdegree * (angle - 300)));
            }

            return cols;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            //Color col = new Color(rgb[0], rgb[1], rgb[2],1);
            
           


        }

    }
}
