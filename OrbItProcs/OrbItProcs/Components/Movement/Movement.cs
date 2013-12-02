using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using System.Reflection;

using OrbItProcs.Processes;

namespace OrbItProcs.Components
{
    public enum movemode
    {
        wallbounce,
        screenwrap,
        falloff,
    };

    public class Movement : Component {

        private bool _pushable = true;
        public bool pushable { get { return _pushable; } set { _pushable = value; } }

        private movemode _movementmode = movemode.wallbounce;
        public movemode movementmode { get { return _movementmode; } set { _movementmode = value; } }

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
            movementmode = movemode.screenwrap;
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

            if (movementmode == movemode.screenwrap) screenWrap();
            if (movementmode == movemode.wallbounce) wallBounce();
            if (movementmode == movemode.falloff)    fallOff();
            
        }

        public override void Draw(SpriteBatch spritebatch)
        {
        }

        public void fallOff()
        {
            int levelwidth = parent.room.game.worldWidth;
            int levelheight = parent.room.game.worldHeight;

            Vector2 pos = parent.position;

            if (parent.comps.ContainsKey(comp.queuer) && (parent.comps[comp.queuer].qs & queues.position) == queues.position)
            {
                Queue<Vector2> positions = ((Queue<Vector2>)(parent.comps[comp.queuer].positions));
                pos = positions.ElementAt(0);
            }


            if (pos.X >= (levelwidth + parent.radius))
            {
                parent.props[node.active] = false;
            }
            else if (pos.X < parent.radius * -1)
            {
                parent.props[node.active] = false;
            }


            if (pos.Y >= (levelheight + parent.radius))
            {
                parent.props[node.active] = false;
            }
            else if (pos.Y < parent.radius * -1)
            {
                parent.props[node.active] = false;
            }
        }

        public void checkForQueue()
        {
            
        }

        public void wallBounce()
        {
            //if (room.PropertiesDict["wallBounce"])
            //float levelwidth = room.game...;
            int levelwidth = parent.room.game.worldWidth;
            int levelheight = parent.room.game.worldHeight;



            if (parent.position.X >= (levelwidth - parent.radius))
            {
                parent.position.X = levelwidth - parent.radius;
                parent.velocity.X *= -1;
                parent.OnCollidePublic();

            }
            if (parent.position.X < parent.radius)
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
            if (parent.position.Y < parent.radius)
            {
                parent.position.Y = parent.radius;
                parent.velocity.Y *= -1;
                parent.OnCollidePublic();
            }


        }

        public void screenWrap()
        {
            //if (room.PropertiesDict["wallBounce"])
            //float levelwidth = room.game...;
            int levelwidth = parent.room.game.worldWidth;
            int levelheight = parent.room.game.worldHeight;

            //hitting top/bottom of screen
            //teleport node
            if (parent.position.X >= levelwidth)
            {
                parent.position.X = 1;
            }
            else if (parent.position.X < 0)
            {
                parent.position.X = levelwidth - 1;
            }
            //show half texture on other side
            if (parent.position.X >= (levelwidth - parent.radius))
            {
                //
            }
            else if (parent.position.X < parent.radius)
            {
                //
            }

            //hitting sides
            //teleport node
            if (parent.position.Y >= levelheight)
            {
                parent.position.Y = 1;
            }
            else if (parent.position.Y < 0)
            {
                parent.position.Y = levelheight - 1;
            }
            //show half texture on other side
            if (parent.position.Y >= (levelheight - parent.radius))
            {
                //
            }
            else if (parent.position.Y < parent.radius)
            {
                //
            }



        }

    }
}
