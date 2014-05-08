using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace OrbItProcs
{
    /// <summary>
    /// The follow component causes this node to follow other nodes. If it is following two nodes, it will go in the average direction of the two.
    /// </summary>
    [Info(UserLevel.User, "The follow component causes this node to follow other nodes. If it is following two nodes, it will go in the average direction of the two.")]
    public class Follow : Component
    {
        public const mtypes CompType = mtypes.affectother | mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }
        /// <summary>
        /// The radius is the reach of the follow component, deciding how far a node can be to be followable.
        /// </summary>
        [Info(UserLevel.User, "The radius is the reach of the follow component, deciding how far a node can be to be followable.")]
        public float radius { get; set; }
        /// <summary>
        /// If enabled, the node will flee from others rather than follow.
        /// </summary>
        [Info(UserLevel.User, "If enabled, the node will flee from others rather than follow.")]
        public bool flee { get; set; }
        public Follow() : this(null) { }
        public Follow(Node parent)
        {
            this.parent = parent;
            radius = 600;
        }
        List<Vector2> directions = new List<Vector2>();
        public override void AffectOther(Node other)
        {
            Vector2 dir = other.body.pos - parent.body.pos;
            if (dir.LengthSquared() > radius * radius) return;
            directions.Add(dir.NormalizeSafe());
        }
        public override void AffectSelf()
        {
            if (directions.Count == 0) return;
            Vector2 result = new Vector2();
            foreach(Vector2 dir in directions)
            {
                result += dir;
            }
            if (result != Vector2.Zero)
            {
                parent.body.velocity = VMath.Redirect(parent.body.velocity, result);
                if (flee)
                    parent.body.velocity *= new Vector2(-1, -1);
            }
            directions = new List<Vector2>();
        }

    }
}
