using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;


namespace OrbItProcs
{
    /// <summary>
    /// Pushes away nodes that are within the radius, can also pull in nodes that beyond the radius. 
    /// </summary>
    [Info(UserLevel.User, "Pushes away nodes that are within the radius, can also pull in nodes that beyond the radius. ", CompType)]
    public class Spring : Component, ILinkable
    {
        public const mtypes CompType = mtypes.affectself | mtypes.affectother;
        public override mtypes compType { get { return CompType; } set { } }
        [Info(UserLevel.Developer)]
        public Link link { get; set; }
        /// <summary>
        /// The strength of the spring's force
        /// </summary>
        [Info(UserLevel.User, "The strength of the spring's force")]
        public float multiplier { get; set; }
        /// <summary>
        /// If enabled, the spring will not only repel nodes, but also attract those outside the boundry.
        /// </summary>
        [Info(UserLevel.User, "If enabled, the spring will not only repel nodes, but also attract those outside the boundry.")]
        public bool hook { get; set; }

        public int _restdist = 300;
        /// <summary>
        /// The distance at which no force is applied.
        /// </summary>
        [Info(UserLevel.User, "The distance at which no force is applied.")]
        public int restdist { get { return _restdist; } set { _restdist = value; if (_restdist < _lowerBound.value) _restdist = _lowerBound.value; } }

        private Toggle<int> _lowerBound = new Toggle<int>(100, true);
        /// <summary>
        /// Represents minimum distance taken into account when calculating push away.
        /// </summary>
        [Info(UserLevel.Advanced, "Represents minimum distance taken into account when calculating push away.")]
        public Toggle<int> lowerBound { get { return _lowerBound; } set { _lowerBound = value; if (_lowerBound.value > _restdist) _lowerBound.value = _restdist; } }

        public Spring() : this(null) { }
        public Spring(Node parent = null)
        {
            multiplier = 100f;

            if (parent != null)
            {
                this.parent = parent;
            }
            com = comp.spring;
        }


        public override void AffectOther(Node other) // called when used as a link
        {
            if (!active) { return; }
                float dist = Vector2.Distance(parent.body.pos, other.body.pos);
                if (!hook && dist > restdist) return;
                if (lowerBound.enabled && dist < lowerBound.value) dist = lowerBound.value;

                float stretch = dist - restdist;
                float strength = -stretch * multiplier;
                Vector2 force = other.body.pos - parent.body.pos;
                VMath.NormalizeSafe(ref force);
                force *= strength;
                other.body.ApplyForce(force);
        }
        public override void AffectSelf() // called when making individual links (clicking for each link)
        {

        }

        public override void Draw(SpriteBatch spritebatch)
        {
        }

    }
}
