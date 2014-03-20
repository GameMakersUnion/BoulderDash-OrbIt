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
    /// Affected nodes are not only driven by their own velocity, but are also subject to this nodes' velocity.
    /// </summary>
    [Info(UserLevel.Advanced, "Affected nodes are not only driven by their own velocity, but are also subject to this nodes' velocity.", CompType)]
    public class RelativeMotion : Component, ILinkable
    {
        public const mtypes CompType = mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }
        public Link link { get; set; }
        //public int _maxdist = 300;
        //public int maxdist { get { return _maxdist; } set { _maxdist = value; if (_maxdist < _mindist) _maxdist = _mindist; } }
        //public int _mindist = 100;
        //public int mindist { get { return _mindist; } set { _mindist = value; if (_mindist > _maxdist) _mindist = _maxdist; } }

        public RelativeMotion() : this(null) { }
        public RelativeMotion(Node parent = null)
        {

            if (parent != null)
            {
                this.parent = parent;
            }
            com = comp.relativemotion;
        }

        public override void AfterCloning()
        {

        }

        public override void InitializeLists()
        {
        }

        public override void AffectOther(Node other) // called when used as a link
        {
            //other.transform.position += parent.transform.velocity;
            other.body.pos += parent.body.effvelocity;

            other.movement.mode = movemode.free;
        }
        public override void AffectSelf()
        {
            
        }


        public override void Draw(SpriteBatch spritebatch)
        {
            Room room = parent.room;
            float mapzoom = room.zoom;

            Color col = parent.body.color;


            spritebatch.Draw(parent.getTexture(), parent.body.pos * mapzoom, null, col, 0, parent.TextureCenter(), (parent.body.scale * mapzoom) * 1.2f, SpriteEffects.None, 0);


        }

        public void onCollision(Dictionary<dynamic, dynamic> args)
        {
        }

    }
}
