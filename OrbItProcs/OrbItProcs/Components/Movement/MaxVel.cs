using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs
{
    public class MaxVel : Component {

        private float _maxvel = 30f;
        public float maxvel { get { return _maxvel; } set { _maxvel = value; if (maxvel < _minvel) _maxvel = _minvel; } }

        private float _minvel = 0f;
        public float minvel { get { return _minvel; } set { _minvel = value; if (_minvel > _maxvel) _minvel = _maxvel; } }

        public MaxVel() : this(null) { }
        public MaxVel(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.movement; 
            methods = mtypes.affectself; 
        }

        public override void Initialize(Node parent)
        {
            this.parent = parent;
        }

        public override void AffectSelf()
        {
            if ((Math.Pow(parent.transform.velocity.X, 2) + Math.Pow(parent.transform.velocity.Y, 2)) > Math.Pow(maxvel, 2))
            {
                parent.transform.velocity.Normalize();
                parent.transform.velocity *= maxvel;
            }
            if ((Math.Pow(parent.transform.velocity.X, 2) + Math.Pow(parent.transform.velocity.Y, 2)) < Math.Pow(minvel, 2))
            {
                parent.transform.velocity.Normalize();
                parent.transform.velocity *= minvel;
            }
        }

        public override void Draw(SpriteBatch spritebatch)
        {

        }
    }
}
