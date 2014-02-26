﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs
{
    public class ColorChanger : Component
    {
        private int timer = 0, _timerMax = 2;
        public int timerMax { get { return _timerMax; } set { _timerMax = value; } }

        private int[] rgb = new int[3];
        private int _inc = 5;
        public int inc { get { return _inc; } set { _inc = value; } }

        private float _value = 1f;
        public float value { get { return _value; } set { _value = value; } }

        private float _saturation = 1f;
        public float saturation { get { return _saturation; } set { _saturation = value; } }

        private int pos = 1, sign = 1;
        private int angle = 0;

        public bool smartshifting { get; set; }

        public ColorChanger() : this(null) { }
        public ColorChanger(Node parent = null)
        {
            smartshifting = true;
            if (parent != null) this.parent = parent;
            com = comp.colorchanger;
            methods = mtypes.affectself;
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

                int[] cols = getColorsFromAngle(angle, value, saturation);
                parent.body.color = new Color(cols[0], cols[1], cols[2], 1);
                angle = (angle + tempinc) % 360;

            }
        }

        public static Color getColorFromHSV(float angle, float saturation = 1f, float value = 1f)
        {
            int[] col = getColorsFromAngle(angle, saturation, value);
            return new Color(col[0], col[1], col[2]);
        }

        public static int[] getColorsFromAngle(float angle, float saturation = 1f, float value = 1f)
        {
            int[] cols = new int[3];
            int bottom = (int)(255 * value * (1 - saturation)); //0
            int top = (int)(255 * value);
            cols[0] = bottom;
            cols[1] = bottom;
            cols[2] = bottom;

            float range = top - bottom;
            double perdegree = range / 60.0;//255.0 / 60.0;

            if (angle < 60)
            {
                cols[0] = top;
                cols[1] = (int)(perdegree * angle);
            }
            else if (angle < 120)
            {
                cols[1] = top;
                cols[0] = top - ((int)(perdegree * (angle - 60)));
            }
            else if (angle < 180)
            {
                cols[1] = top;
                cols[2] = (int)(perdegree * (angle - 120));
            }
            else if (angle < 240)
            {
                cols[2] = top;
                cols[1] = top - ((int)(perdegree * (angle - 180)));
            }
            else if (angle < 300)
            {
                cols[2] = top;
                cols[0] = (int)(perdegree * (angle - 240));
            }
            else if (angle < 360)
            {
                cols[0] = top;
                cols[2] = top - ((int)(perdegree * (angle - 300)));
            }

            return cols;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            //Color col = new Color(rgb[0], rgb[1], rgb[2],1);




        }

    }
}
