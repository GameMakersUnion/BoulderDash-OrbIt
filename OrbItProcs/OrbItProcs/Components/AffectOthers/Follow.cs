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
    [Info(UserLevel.User, "The follow component causes this node to follow other nodes. If it is following two nodes, it will go in the average direction of the two.", CompType)]
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
        public float LerpPercent { get; set; }
        public enum followMode
        {
            FollowAll,
            FollowNearest,
            //FollowTarget,
        }
        public followMode mode { get; set; }
        public Follow() : this(null) { }
        public Follow(Node parent)
        {
            this.parent = parent;
            radius = 600;
            flee = false;
            LerpPercent = 10f;
            mode = followMode.FollowNearest;
        }
        List<Vector2> directions = new List<Vector2>();
        float nearestDistSqrd = float.MaxValue;
        Vector2 nearestDirection = new Vector2();
        public override void AffectOther(Node other)
        {
            Vector2 dir = other.body.pos - parent.body.pos;
            float distSquared = dir.LengthSquared();
            if (distSquared > radius * radius) return;
            if (mode == followMode.FollowNearest)
            {
                if (distSquared < nearestDistSqrd)
                {
                    nearestDistSqrd = distSquared;
                    nearestDirection = dir;
                }
            }
            else if (mode == followMode.FollowAll)
            {
                directions.Add(dir.NormalizeSafe());
            }
        }
        public override void AffectSelf()
        {
            if (mode == followMode.FollowNearest)
            {
                if (nearestDistSqrd == float.MaxValue) return;
                if (flee) nearestDirection *= new Vector2(-1, -1);
                float oldAngle = VMath.VectorToAngle(parent.body.velocity);
                float newAngle = VMath.VectorToAngle(nearestDirection);
                float lerpedAngle = GMath.AngleLerp(oldAngle, newAngle, LerpPercent / 100f);
                Vector2 finalDir = VMath.AngleToVector(lerpedAngle);
                parent.body.velocity = VMath.Redirect(parent.body.velocity, finalDir);

                nearestDistSqrd = float.MaxValue;
            }
            else if (mode == followMode.FollowAll)
            {
                if (directions.Count == 0) return;
                Vector2 result = new Vector2();
                foreach (Vector2 dir in directions)
                {
                    result += dir;
                }
                if (result != Vector2.Zero)
                {
                    if (flee) result *= new Vector2(-1, -1);
                    float oldAngle = VMath.VectorToAngle(parent.body.velocity);
                    float newAngle = VMath.VectorToAngle(result);
                    float lerpedAngle = GMath.AngleLerp(oldAngle, newAngle, LerpPercent / 100f);
                    Vector2 finalDir = VMath.AngleToVector(lerpedAngle);
                    parent.body.velocity = VMath.Redirect(parent.body.velocity, finalDir);
                }
                directions = new List<Vector2>();
            }
        }

    }
}
