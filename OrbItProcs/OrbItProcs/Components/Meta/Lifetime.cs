using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs
{
    public class Lifetime : Component
    {
        //private int _maxlife = 100;
        //public int maxlife { get { return _maxlife; } set { _maxlife = value; } }
        //private int _lifeleft = 100;
        //public int lifeleft { get { return _lifeleft; } set { _lifeleft = value; } }

        private int _maxmseconds = 5000;
        public int maxmseconds { get { return _maxmseconds; } set { _maxmseconds = value; } }
        private int _mseconds = 0;
        public int mseconds { get { return _mseconds; } set { _mseconds = value; } }

        private bool _immortal = true;
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
            mseconds = 0;
            alive = true;
        }

        public override void AffectSelf()
        {
            int mill = Game1.GlobalGameTime.ElapsedGameTime.Milliseconds;
            mseconds += mill;
            if (mseconds > maxmseconds && !immortal)
            {
                Die();
            }

            /*
            if (++timer % timerMax == 0)
            {
                if (lifeleft-- <= 0 && !immortal)
                {
                    Die();
                }
            }
            */
        }

        public void Die()
        {
            //if (!alive) return;
            //alive = false;

            //perform death here.... do it properly later
            //parent.room.nodes.Remove(parent);
            parent.active = false;
            parent.IsDeleted = true;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
        }

    }
}
