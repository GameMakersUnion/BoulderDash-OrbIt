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
        }
        public override void OnSpawn()
        {
            
        }

    }
}
