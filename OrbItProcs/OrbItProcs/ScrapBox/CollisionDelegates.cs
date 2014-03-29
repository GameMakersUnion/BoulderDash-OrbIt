using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public static class CollisionDelegates
    {
        public static CollisionDelegate toggleWhite = delegate(Node source, Node target)
            {
                if (target == null) return;
                if (source.body.color == Color.White)
                    source.body.color = Utils.randomColor();
                else
                    source.body.color = Color.White;

            };
        public static CollisionDelegate randomCol = delegate(Node source, Node target)
            {
                if (target == null) return;
                source.body.color = Utils.randomColor();
            };
        public static CollisionDelegate absorbColor = delegate(Node source, Node target)
            {
                if (target == null) return;
                int div = 25;
                int r = (int)source.body.color.R + ((int)target.body.color.R - (int)source.body.color.R) / div;
                int g = (int)source.body.color.G + ((int)target.body.color.G - (int)source.body.color.G) / div;
                int b = (int)source.body.color.B + ((int)target.body.color.B - (int)source.body.color.B) / div;
                source.body.color = new Color(r, g, b, (int)source.body.color.A);
            };
        public static CollisionDelegate empty = delegate(Node s, Node t) { };
    }
}
