using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs.Components
{
    public class Lifetime : Component
    {


        public bool active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                if (value && parent != null) Initialize(parent);
                if (parent != null && parent.comps.ContainsKey(com))
                {
                    parent.triggerSortLists();
                }
            }
        }

        private int _maxlife = 100;
        public int maxlife { get { return _maxlife; } set { _maxlife = value; } }
        private int _lifeleft = 100;
        public int lifeleft { get { return _lifeleft; } set { _lifeleft = value; } }


        public bool _immortal = false;
        public bool immortal { get { return _immortal; } set { _immortal = value; } }

        private int _timer = 0;
        public int timer { get { return _timer; } set { _timer = value; } }
        private int _timerMax = 1;
        public int timerMax { get { return _timerMax; } set { _timerMax = value; } }

        private bool _alive = true;
        public bool alive { get { return _alive; } set { _alive = value; } }

        public Lifetime() : this(null) { }
        public Lifetime(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.lifetime; 
            methods = mtypes.affectself; 
        }


        public override void Initialize(Node parent)
        {
            this.parent = parent;
            lifeleft = maxlife;
        }

        public override void AffectOther(Node other)
        {

        }
        public override void AffectSelf()
        {
            
            if (++timer % timerMax == 0)
            {
                if (lifeleft-- <= 0 && !immortal)
                {
                    Die();
                }
                
            }
        }

        public void Die()
        {
            if (!alive) return;
            alive = false;

            //perform death here.... do it properly later
            //parent.room.nodes.Remove(parent);
            parent.props[node.active] = false;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
        }

    }
}
