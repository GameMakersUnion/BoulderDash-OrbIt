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
        //public int r, g, b;
        //public Color color;
        public int timer, timerMax;
        public int[] rgb;
        public int inc, pos, sign;
        public int angle;


        public HueShifter(Node parent)
        {
            this.parent = parent;
            this.com = comp.hueshifter;
            //this.r = this.g = this.b = 255;

            timer = 0;
            timerMax = 2;
            InitializeLists();
            rgb = new int[3];
            rgb[0] = 255;
            rgb[1] = 0;
            rgb[2] = 0;
            pos = 1;
            inc = 30;
            sign = 1;


            angle = 0;

            //Console.WriteLine("construct");
        }

        public override void InitializeLists()
        {
            //color = new Color();

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
            //Console.WriteLine("yeah");

            timer++;
            if ((timer) % timerMax == 0)
            {
                int[] cols = getColorsFromAngle(angle);
                parent.color = new Color(cols[0], cols[1], cols[2], 1);
                angle = (angle + 10) % 360;

                // alternative algorithm....... (not using angles)
                /*
                if (inc > 0)
                {
                    //rgb[pos] += inc;
                    rgb[pos] += Utils.random.Next(60);

                    if (rgb[pos] >= 255)
                    {
                        rgb[pos] = 255;
                        pos = (pos + 1) % 3;
                        inc *= -1;
                        
                    }
                }
                else if (inc < 0)
                {
                    //rgb[pos] += inc;
                    rgb[pos] -= Utils.random.Next(60);

                    if (rgb[pos] <= 0)
                    {
                        rgb[pos] = 0;
                        pos = (pos + 1) % 3;
                        inc *= -1;
                    }
                }
                parent.color = new Color(rgb[0], rgb[1], rgb[2], 1);
                 */

                
            }

            

        }

        public int[] getColorsFromAngle(float angle)
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
                //cols[0] = 255;
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
                //cols[2] = 255;
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
                //cols[0] = 255;
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
