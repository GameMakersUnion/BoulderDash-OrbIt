using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using System.Reflection;
using System.Diagnostics;

namespace OrbItProcs
{
    public enum movemode
    {
        free,
        wallbounce,
        screenwrap,
        falloff,
    };

    public class Movement : Component {

        private bool _pushable = true;
        public bool pushable { get { return _pushable; } set { _pushable = value; } }

        private float _VelocityModifier = 1f;
        public float VelocityModifier { get { return _VelocityModifier; } set { _VelocityModifier = value; } }

        public Vector2 tempPosition = new Vector2(0, 0);
        
        private movemode _movementmode = movemode.wallbounce;
        public movemode movementmode { get { return _movementmode; } set { _movementmode = value; } }

        public Movement() : this(null) { }
        public Movement(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.movement; 
            methods = mtypes.affectself;
            active = false;
        }

        public override void Initialize(Node parent)
        {
            this.parent = parent;
        }

        private void IntegrateForces()
        {
            if (parent.body.velocity.IsFucked()) System.Diagnostics.Debugger.Break();
            if (parent.body.force.IsFucked()) System.Diagnostics.Debugger.Break();

            if (parent.body.invmass == 0)
                return;

            Body b = parent.body;
            b.velocity += VMath.MultVectDouble(b.force, b.invmass); //* dt / 2.0;
            
            b.angularVelocity += b.torque * b.invinertia; // * dt / 2.0;

        }
        public void IntegrateVelocity()
        {
            if (parent.body.velocity.IsFucked()) System.Diagnostics.Debugger.Break();
            if (parent.body.force.IsFucked()) System.Diagnostics.Debugger.Break();

            if (!active) return;
            if (parent.body.invmass == 0)
                return;
            Body b = parent.body;
            b.pos += b.velocity;
            b.orient += b.angularVelocity;
            b.SetOrient(b.orient);
            IntegrateForces(); //calls the private integrate forces method
        }

        public override void AffectSelf()
        {
            parent.body.effvelocity = parent.body.pos - tempPosition;
            if (!pushable && tempPosition != new Vector2(0,0)) parent.body.pos = tempPosition;
            tempPosition = parent.body.pos;

            //parent.body.position.X += parent.body.velocity.X * VelocityModifier;
            //parent.body.position.Y += parent.body.velocity.Y * VelocityModifier;



            if (movementmode == movemode.screenwrap) screenWrap();
            if (movementmode == movemode.wallbounce) wallBounce();
            if (movementmode == movemode.falloff)    fallOff();

            //Trippy();
        }

        public void Trippy()
        {
            //test (holy SHIT that looks cool)
            PropertyInfo pi = parent.body.GetType().GetProperty("scale");
            pi.SetValue(parent.body, parent.body.pos.X % 4.0f, null);
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            //C3.XNA.Primitives2D.DrawCircle()
        }

        public void fallOff()
        {
            int levelwidth = parent.room.game.worldWidth;
            int levelheight = parent.room.game.worldHeight;

            Vector2 pos = parent.body.pos;

            if (parent.comps.ContainsKey(comp.queuer) && (parent.comps[comp.queuer].qs & queues.position) == queues.position)
            {
                Queue<Vector2> positions = ((Queue<Vector2>)(parent.comps[comp.queuer].positions));
                pos = positions.ElementAt(0);
            }

            if (pos.X >= (levelwidth + parent.body.radius))
            {
                parent.IsDeleted = true;
            }
            else if (pos.X < parent.body.radius * -1)
            {
                parent.IsDeleted = true;
            }

            if (pos.Y >= (levelheight + parent.body.radius))
            {
                parent.IsDeleted = true;
            }
            else if (pos.Y < parent.body.radius * -1)
            {
                parent.IsDeleted = true;
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

            if (parent.body.pos.X >= (levelwidth - parent.body.radius))
            {
                parent.body.pos.X = levelwidth - parent.body.radius;
                parent.body.velocity.X *= -1;
                parent.OnCollidePublic(null);

            }
            if (parent.body.pos.X < parent.body.radius)
            {
                parent.body.pos.X = parent.body.radius;
                parent.body.velocity.X *= -1;
                parent.OnCollidePublic(null);
            }
            if (parent.body.pos.Y >= (levelheight - parent.body.radius))
            {
                parent.body.pos.Y = levelheight - parent.body.radius;
                parent.body.velocity.Y *= -1;
                parent.OnCollidePublic(null);
            }
            if (parent.body.pos.Y < parent.body.radius)
            {
                parent.body.pos.Y = parent.body.radius;
                parent.body.velocity.Y *= -1;
                parent.OnCollidePublic(null);
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
            if (parent.body.pos.X >= levelwidth)
            {
                parent.body.pos.X = 1;
            }
            else if (parent.body.pos.X < 0)
            {
                parent.body.pos.X = levelwidth - 1;
            }
            //show half texture on other side
            if (parent.body.pos.X >= (levelwidth - parent.body.radius))
            {
                //
            }
            else if (parent.body.pos.X < parent.body.radius)
            {
                //
            }

            //hitting sides
            //teleport node
            if (parent.body.pos.Y >= levelheight)
            {
                parent.body.pos.Y = 1;
            }
            else if (parent.body.pos.Y < 0)
            {
                parent.body.pos.Y = levelheight - 1;
            }
            //show half texture on other side
            if (parent.body.pos.Y >= (levelheight - parent.body.radius))
            {
                //
            }
            else if (parent.body.pos.Y < parent.body.radius)
            {
                //
            }



        }

    }
}
