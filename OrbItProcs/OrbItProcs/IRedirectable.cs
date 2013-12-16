using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public class Redirectable
    {
        public virtual Redirectable parent { get; set; }
        public virtual int x { get { if (radix.ContainsKey("get_x")) return radix["get_x"].x; else { return parent.x; } } set { parent.x = value; } }
        public virtual string y { get { try { return parent.y; } catch { return null; } } set { try { parent.y = value; } catch { } } }

        public Dictionary<string, Redirectable> radix = new Dictionary<string, Redirectable>();

        

    }
}
