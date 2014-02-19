using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Component = OrbItProcs.Component;

namespace OrbItProcs {
    public class RandColor : Component {

        public bool _IsBasedOnTime = true;
        public bool IsBasedOnTime { get { return _IsBasedOnTime; } set { _IsBasedOnTime = value; } }

        //public Color randCol;

        public RandColor() : this(null) { }
        public RandColor(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.randcolor; 
            methods = mtypes.initialize; 
        }

        public override void OnSpawn()
        {
            if (!active) return;
            if (IsBasedOnTime)
            {
                //parent.body.color = Utils.IntToColor(Utils.CurrentMilliseconds());
                float num = (float)Utils.random.Next(100000) / (float)100000;
                int n = (int)(num * 16000000);
                parent.body.color = Utils.IntToColor(n);
            }
        }

        public override void AffectSelf()
        {
        }

        public override void Draw(SpriteBatch spritebatch)
        {
        }

        

    }
}
