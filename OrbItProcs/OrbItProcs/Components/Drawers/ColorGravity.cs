using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace OrbItProcs.Components
{
    public class ColorGravity : Component
    {
        public enum DistanceMod
        {
            color,
            spatial,
        }

        public override bool active
        {
            get
            {
                return base.active;
            }
            set
            {
                if (value)
                {
                    r = parent.body.color.R / 255f;
                    g = parent.body.color.R / 255f;
                    b = parent.body.color.R / 255f;
                }
                base.active = value;
            }
        }

        public Vector3 colvelocity;
        public float multiplier { get; set; }
        public float r { get; set; }
        public float g { get; set; }
        public float b { get; set; }
        public DistanceMod distancemod { get; set; }

        public ColorGravity() : this(null){ }
        public ColorGravity(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.colorgravity;
            methods = mtypes.affectself | mtypes.affectother;
            colvelocity = new Vector3(0f, 0f, 0f);
            multiplier = 1f;
            distancemod = DistanceMod.spatial;
        }

        public override void OnSpawn()
        {
            r = parent.body.color.R / 255f;
            g = parent.body.color.R / 255f;
            b = parent.body.color.R / 255f;
        }

        public override void AffectSelf()
        {
            r += colvelocity.X;
            g += colvelocity.Y;
            b += colvelocity.Z;
            if (r > 1f || r < 0f)
            {
                r = DelegateManager.TriangleFunction(r, 1f);
                colvelocity.X *= -1;
            }
            if (g > 1f || g < 0f)
            {
                g = DelegateManager.TriangleFunction(g, 1f);
                colvelocity.Y *= -1;
            }
            if (b > 1f || b < 0f)
            {
                b = DelegateManager.TriangleFunction(b, 1f);
                colvelocity.Z *= -1;
            }
            parent.body.color = new Color(r, g, b);
        }

        public override void AffectOther(Node other)
        {
            Vector3 parentCol = parent.body.color.ToVector3();
            Vector3 otherCol = other.body.color.ToVector3();
            float dist = 1f;
            if (distancemod == DistanceMod.color)
            {
                dist = Vector3.Distance(parentCol, otherCol);
            }
            else if (distancemod == DistanceMod.spatial)
            {
                dist = Vector2.Distance(parent.body.pos, other.body.pos) / 10000f;
            }
            Vector3 direction = otherCol - parentCol;
            if (dist < 1) dist = 1;
            if (direction != Vector3.Zero) direction.Normalize();
            float mag = multiplier * parent.body.mass * other.body.mass / (dist * dist);
            Vector3 impulse = mag * direction;
            impulse /= 10000f;
            if (other.HasActiveComponent(comp.colorgravity))
                other.comps[comp.colorgravity].colvelocity += impulse;
            colvelocity -= impulse;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
        }

    }
}
