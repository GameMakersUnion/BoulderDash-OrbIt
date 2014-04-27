using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace OrbItProcs
{
    public class Diode : Component
    {
        public const mtypes CompType = mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }
        public bool semaphore { get; set; }
        public int maxTickets { get; set; }
        private int usedTickets;
        public enum Mode
        {
            firstBlocked,
            secondBlocked,
            bothBlocked,
            neitherBlocked,
        }
        public Mode mode { get; set; }
        public Diode() : this(null) { }
        public Diode(Node parent)
        {
            this.parent = parent;
            this.com = comp.diode;
            mode = Mode.neitherBlocked;
            maxTickets = 4;
            usedTickets = 0;
            semaphore = false;
        }
        HashSet<Node> goingThrough = new HashSet<Node>();
        public override void OnSpawn()
        {
            parent.body.OnCollisionEnter += delegate(Node source, Node dest)
            {
                bool happened = IsOnCorrectSide(parent, dest, true);
                if (happened)
                {
                    if (semaphore)
                    {
                        if (usedTickets <= maxTickets)
                        {
                            goingThrough.Add(dest);
                            usedTickets++;
                            return;
                        }
                    }
                    goingThrough.Add(dest);
                }
            };
            parent.body.OnCollisionExit += delegate(Node source, Node dest)
            {
                bool happened = IsOnCorrectSide(parent, dest, true);
                if (semaphore && happened && goingThrough.Contains(dest) && usedTickets <= maxTickets)
                {
                    usedTickets--;
                }
                goingThrough.Remove(dest);
            };
            parent.body.ExclusionCheckResolution += delegate(Collider c1, Collider c2)
            {
                if (semaphore && usedTickets > maxTickets) return false; 
                
                return goingThrough.Contains(c2.parent) || IsOnCorrectSide(parent, c2.parent, true);
            };
        }

        public bool IsOnCorrectSide(Node wall, Node other, bool belowPi)
        {
            Vector2 direction = other.body.pos - wall.body.pos;
            float dirAngle = Utils.VectorToAngle(direction);
            float resAngle = (parent.body.orient - dirAngle).between0and2pi();
            if (belowPi) return resAngle < VMath.PI;
            return resAngle >= VMath.PI;
        }

    }
}
