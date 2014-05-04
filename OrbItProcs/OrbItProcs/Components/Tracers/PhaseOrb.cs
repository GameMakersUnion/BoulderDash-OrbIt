using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace OrbItProcs
{
    /// <summary>
    /// Nodes will leave behind a trail consisting of fading images of themselves.
    /// </summary>
    [Info(UserLevel.User, "Nodes will leave behind a trail consisting of fading images of themselves.", CompType)]
    public class PhaseOrb : Component
    {
        public const mtypes CompType = mtypes.minordraw | mtypes.tracer;
        public override mtypes compType { get { return CompType; } set { } }

        public int _phaserLength = 10;
        /// <summary>
        /// Sets the length of the phaser.
        /// </summary>
        [Info(UserLevel.User, "Sets the length of the phaser. ")]
        [Polenter.Serialization.ExcludeFromSerialization]
        public int phaserLength
        {
            get
            {
                return _phaserLength;
            }
            set
            {
                //if (parent != null && parent.HasComp<Queuer>() && parent.Comp<Queuer>().queuecount < value)
                //{
                //    parent.Comp<Queuer>().queuecount = value;
                //}
                _phaserLength = value;
            }
        }

        public Toggle<int> fade { get; set; }
        private int r1;
        private int g1;
        private int b1;
        //private int timer = 0;

        public PhaseOrb() : this(null) { }
        public PhaseOrb(Node parent = null)
        {
            if (parent != null)
            {
                this.parent = parent;
            }
            InitializeLists(); 
            fade = new Toggle<int>(phaserLength);
            
        }

        public override void AfterCloning()
        {
            //if (!parent.HasComp<Queuer>()) parent.addComponent(comp.queuer, true);
            //parent.Comp<Queuer>().qs = parent.Comp<Queuer>().qs | queues.scale | queues.position;
        }
        public override void Draw()
        {
            parent.room.camera.AddPermanentDraw(parent.texture, parent.body.pos, parent.body.color, parent.body.scale, 0, phaserLength);
        }
    }
}
