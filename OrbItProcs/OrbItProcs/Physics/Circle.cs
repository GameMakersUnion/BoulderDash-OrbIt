using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace OrbItProcs.Physics
{
    public class Circle
    {
        public float radius;
        public Vector2 position;

        public static bool CirclevsCircle(Circle a, Circle b)
        {
            float r = a.radius + b.radius;
            r *= r;
            float x = (b.position.X + a.position.X);
            float y = (b.position.Y + a.position.Y);
            
            return (x*x + y*y) < r;
        }

    }
}
