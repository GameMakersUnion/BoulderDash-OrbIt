using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs
{
    /// <summary>
    /// Pushes all affected nodes in a globally set direction
    /// </summary>
    [Info(UserLevel.User, "Pushes all affected nodes in a globally set direction", mtypes.affectother)]
    public class FixedForce : Component, ILinkable
    {
        public const mtypes CompType = mtypes.affectother;
        public override mtypes compType { get { return CompType; } set { } }
        [Info(UserLevel.Developer)]
        public Link link { get; set; }

        private Vector2 _force = new Vector2(0, 1);
        /// <summary>
        /// The direction and magnitude of the force
        /// </summary>
        [Info(UserLevel.Advanced, "The direction and magnitude of the force")]
        public Vector2 force { get; set; }
        /// <summary>
        /// The horizontal force applied: positive is rightwards, negative is leftwards.
        /// </summary>
        [Info(UserLevel.User, "The horizontal force applied: positive is rightwards, negative is leftwards.")]
        public float x { get { return _force.X; } set { _force.X = value; } }
        /// <summary>
        /// The vertical force applied: positive is downwards, negative is upwards;
        /// </summary>
        [Info(UserLevel.User, "The vertical force applied: positive is downwards, negative is upwards;")]
        public float y { get { return _force.Y; } set { _force.Y = value; } }
        /// <summary>
        /// Multiplies the set force by the given amount
        /// </summary>
        [Info(UserLevel.User, "Multiplies the set force by the given amount")]
        public float multiplier { get; set; }
        /// <summary>
        /// Determine if we should stop pushing the other node after his velocity exceeds this value in the direction of the force we are applying.
        /// </summary>
        [Info(UserLevel.Advanced, "Determine if we should stop pushing the other node after his velocity exceeds this value in the direction of the force we are applying.")]
        public Toggle<float> terminal { get; set; }

        public FixedForce() : this(null) { }
        public FixedForce(Node parent)
        {
            com = comp.fixedforce;
            if (parent != null) this.parent = parent;
            multiplier = 1f;
            terminal = new Toggle<float>(5f, true);
        }

        public override void AffectOther(Node other)
        {
            if (!active) { return; }
            if (exclusions.Contains(other)) return;

            if (terminal.enabled == false || (terminal.value > 0 && other.body.velocity.ProjectOnto(force).Length() < terminal.value))
                other.body.ApplyForce(force * multiplier / 10f);//other.body.velocity += force * multiplier / 10f;
        }
        public override void AffectSelf()
        {
        }
        public override void Draw()
        {
        }
    }
}
