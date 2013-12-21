using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Component = OrbItProcs.Components.Component;

namespace OrbItProcs.Components {
    public class RandColor : Component {

        new public bool active
        {
            get
            {
                return _active;
            }
            set
            {
                
                _active = value;
                if (value && parent != null)
                {
                    Initialize(parent);
                }
                if (parent != null && parent.comps.ContainsKey(com))
                {
                    parent.triggerSortLists();
                }
                
            }
        }

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

        public override void Initialize(Node parent)
        {
            this.parent = parent;
            if (active)
            {
                parent.transform.color = new Color((float)Utils.random.Next(255) / (float)255, (float)Utils.random.Next(255) / (float)255, (float)Utils.random.Next(255) / (float)255);
            }
            
        }

        public override void OnSpawn()
        {
            if (IsBasedOnTime)
            {
                parent.transform.color = Utils.IntToColor(Utils.CurrentMilliseconds());
            }
        }

        public override void AffectOther(Node other)
        {

        }
        public override void AffectSelf()
        {
        }

        public override void Draw(SpriteBatch spritebatch)
        {
        }

        

    }
}
