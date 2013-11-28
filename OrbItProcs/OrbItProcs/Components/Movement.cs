using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using System.Reflection;

namespace OrbItProcs.Components
{
    public class Movement : Component {

        private bool _pushable = true;
        public bool pushable { get { return _pushable; } set { _pushable = value; } }

        public Movement() : this(null) { }
        public Movement(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.movement; 
            methods = mtypes.affectself; 
        }

        public override void Initialize(Node parent)
        {
            this.parent = parent;
        }

        public override void AffectOther(Node other)
        {
            
        }
        public override void AffectSelf()
        {
            parent.position.X += parent.velocity.X;
            parent.position.Y += parent.velocity.Y;

            //test (holy SHIT that looks cool)
            //PropertyInfo pi = parent.GetType().GetProperty("scale");
            //pi.SetValue(parent, parent.position.X % 4.0f, null);

            wallBounce();
        }

        public override void Draw(SpriteBatch spritebatch)
        {
        }

        public void wallBounce()
        {
            //if (room.PropertiesDict["wallBounce"])
            //float levelwidth = room.game1...;
            int levelwidth = parent.room.game1.worldWidth;
            int levelheight = parent.room.game1.worldHeight;



            if (parent.position.X >= (levelwidth - parent.radius))
            {
                parent.position.X = levelwidth - parent.radius;
                parent.velocity.X *= -1;
                parent.OnCollidePublic();

            }
            else if (parent.position.X < parent.radius)
            {
                parent.position.X = parent.radius;
                parent.velocity.X *= -1;
                parent.OnCollidePublic();
            }
            if (parent.position.Y >= (levelheight - parent.radius))
            {
                parent.position.Y = levelheight - parent.radius;
                parent.velocity.Y *= -1;
                parent.OnCollidePublic();
            }
            else if (parent.position.Y < parent.radius)
            {
                parent.position.Y = parent.radius;
                parent.velocity.Y *= -1;
                parent.OnCollidePublic();
            }


        }

    }
}
