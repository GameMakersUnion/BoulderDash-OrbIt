using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace OrbItProcs.Components
{
    public class ColorGravity : Component, ILinkable
    {
        private Link _link = null;
        public Link link { get { return _link; } set { _link = value; } }

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
        public bool hueOnly { get; set; }
        public float hue { get; set; }
        public float huevelocity { get; set; }
        public float friction { get; set; }
        public float divisor { get; set; }
        public float maxhuevel { get; set; }
        public ColorGravity() : this(null){ }
        public ColorGravity(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.colorgravity;
            methods = mtypes.affectself | mtypes.affectother;
            colvelocity = new Vector3(0f, 0f, 0f);
            multiplier = 1f;
            distancemod = DistanceMod.spatial;
            hueOnly = true;
            huevelocity = 0f;
            hue = 0f;
            divisor = 1000f;
            maxhuevel = 10f;
            friction = 0.9f;
        }

        public override void OnSpawn()
        {
            r = parent.body.color.R / 255f;
            g = parent.body.color.R / 255f;
            b = parent.body.color.R / 255f;
            hue = HueFromColor(parent.body.color);
        }

        public override void AffectOther(Node other)
        {
            if (hueOnly)
            {
                if (!other.HasActiveComponent(comp.colorgravity)) return;

                float dist = 1f;
                if (distancemod == DistanceMod.color)
                {
                    dist = hue - other.comps[comp.colorgravity].hue;
                    if (dist == 0) return;
                    float force = multiplier * other.body.mass * parent.body.mass / (dist * dist);
                    if (dist < 0) force *= -1;
                    float diff = hue - other.comps[comp.colorgravity].hue;
                    if (Math.Abs(diff) > 180) force *= -1;
                    if (force > maxhuevel) force = maxhuevel;
                    else if (force < -maxhuevel) force = -maxhuevel;
                    other[comp.colorgravity].huevelocity += force;
                    huevelocity += force;
                }
                else if (distancemod == DistanceMod.spatial)
                {
                    dist = Vector2.Distance(parent.body.pos, other.body.pos) / divisor;
                    if (dist == 0) return;
                    float force = multiplier * other.body.mass * parent.body.mass / (dist * dist);
                    float diff = hue - other.comps[comp.colorgravity].hue;
                    //int wrap = Math.Abs(diff) > 180 ? -1 : 1;
                    if (Math.Abs(diff) > 180) force *= -1;
                    if (diff < 0) force *= -1;
                    if (force > maxhuevel) force = maxhuevel;
                    else if (force < -maxhuevel) force = -maxhuevel;

                    other[comp.colorgravity].huevelocity += force;
                    huevelocity -= force;
                    //todo: make graphing function to represent dataover time for variables

                    
                }
            }
            else
            {
                Vector3 parentCol = parent.body.color.ToVector3();
                Vector3 otherCol = other.body.color.ToVector3();
                float dist = 1f;
                if (distancemod == DistanceMod.color)
                {
                    dist = Vector3.Distance(parentCol, otherCol) / 100f;
                }
                else if (distancemod == DistanceMod.spatial)
                {
                    dist = Vector2.Distance(parent.body.pos, other.body.pos) / divisor / divisor;
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
        }

        public override void AffectSelf()
        {
            if (hueOnly)
            {
                if (huevelocity > maxhuevel) huevelocity = maxhuevel;
                else if (huevelocity < -maxhuevel) huevelocity = -maxhuevel;
                huevelocity *= friction;
                hue += huevelocity;
                //Console.WriteLine("1) {0} : HUE: {1}   HUEVEL: {2}", parent.name, hue, huevelocity);
                //if (hue < 0) { hue = 0; huevelocity *= -1; }
                //if (hue > 360) { hue = 360; huevelocity *= -1; }
                hue = ColorChanger.SawtoothFloat(hue, 360f);
                parent.body.color = ColorChanger.getColorFromHSV(hue);
                //Console.WriteLine("2) {0} : HUE: {1}   HUEVEL: {2}", parent.name, hue, huevelocity);
            }
            else
            {
                r += colvelocity.X / friction;
                g += colvelocity.Y / friction;
                b += colvelocity.Z / friction;
                if (r > 1f || r < 0f)
                {
                    r = DelegateManager.Triangle(r, 1f);
                    colvelocity.X *= -1;
                }
                if (g > 1f || g < 0f)
                {
                    g = DelegateManager.Triangle(g, 1f);
                    colvelocity.Y *= -1;
                }
                if (b > 1f || b < 0f)
                {
                    b = DelegateManager.Triangle(b, 1f);
                    colvelocity.Z *= -1;
                }
                parent.body.color = new Color(r, g, b);
            }
        }

        public float HueFromColor(Color c)
        {
            //180/pi*atan2( sqrt(3)*(G-B) , 2*R-G-B )
            return (float)(180 / Math.PI * Math.Atan2(Math.Sqrt(3) * (c.G - c.B), 2 * c.R - c.G - c.B));
        }
        public float HueFromColor(int r, int g, int b)
        {
            return (float)(180 / Math.PI * Math.Atan2(Math.Sqrt(3) * (g - b), 2 * r - g - b));
        }

        public override void Draw(SpriteBatch spritebatch)
        {
        }

    }
}
