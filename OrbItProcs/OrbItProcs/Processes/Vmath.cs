using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace OrbItProcs
{
    public static class VMath
    {
        public static float EPSILON = 0.0001f;
        
        #region /// Existing Classes ///
        public static void Test()
        {
            //Vector2.Add;
            //Vector2.Barycentric;
            //Vector2.CatmullRom;
            //Vector2.Clamp;
            //Vector2.Distance;
            //Vector2.DistanceSquared;
            //Vector2.Divide;
            //Vector2.Dot;
            //Vector2.Equals;
            //Vector2.Hermite;
            //Vector2.Lerp;
            //Vector2.Max;
            //Vector2.Min;
            //Vector2.Multiply;
            //Vector2.Negate;
            //Vector2.Normalize;
            //Vector2.One;
            //Vector2.Reflect;
            //Vector2.SmoothStep;
            //Vector2.Subtract;
            //Vector2.Transform;
            //Vector2.TransformNormal;
            //Vector2.UnitX;
            //Vector2.UnitY;
            //Vector2.Zero;
        }
        #endregion

        //public bool WereShit(this entity)
        //{
        //    if (entity.In(JonSkeet, JohnCarmack, JonBlow)) return false;
        //    else return entity.WereShit();
        //}
        public static void Set(ref Vector2 v, float x, float y)
        {
            v.X = x; v.Y = y;
        }
        public static void Rotate(this Vector2 v, float radians)
        {
            double c = Math.Cos(radians);
            double s = Math.Sin(radians);
            double xp = v.X * c - v.Y * s;
            double yp = v.X * s + v.Y * c;
            v.X = (float)xp;
            v.Y = (float)yp;
        }
        public static Vector2 ProjectOnto(this Vector2 source, Vector2 target)
        {
            return (Vector2.Dot(source, target) / target.LengthSquared()) * target;
        }

        public static Vector2 Cross(Vector2 v, double a)
        {
            return new Vector2((float)a * v.Y, -(float)a * v.X);
        }
        public static Vector2 Cross(double a, Vector2 v)
        {
            return new Vector2(-(float)a * v.Y, (float)a * v.X);
        }
        public static double Cross(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }
        public static Vector2 MultVectDouble(Vector2 v, double d)
        {
            return new Vector2(v.X * (float)d, v.Y * (float)d);
        }

        public static void NormalizeSafe(ref Vector2 v)
        {
            //v.Normalize();
            //return;
            if (v.X != 0 || v.Y != 0)
            {
                //v.Normalize();
                //return;
                float len = v.Length();
                if (len == 0) return;
                float invLen = 1.0f / len;
                v.X *= invLen;
                v.Y *= invLen;
                //Console.WriteLine("A: x:{0} , y:{1}", v.X, v.Y);
                //float newlen = v.Length();
                //if (newlen != 1f) Console.WriteLine("NEWLEN: {0}", newlen);
            }
        }

        public static bool Equal(double a, double b)
        {
            return Math.Abs(a - b) <= EPSILON;
        }
        //public static double RandDouble(double l, double h)
        //{
        //    
        //}
        public static bool BiasGreaterThan(double a, double b)
        {
            double k_biasRelative = 0.95;
            double k_biasAbsolute = 0.01;
            return a >= b * k_biasRelative + a * k_biasAbsolute;
        }
    }
}
