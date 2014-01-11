using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using OrbItProcs.Processes;

namespace OrbItProcs.Components
{
    public class RelativeMotion : Component, ILinkable
    {

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
            methods = mtypes.affectself;// | mtypes.draw | mtypes.minordraw;
            //methods = mtypes.affectother | mtypes.draw | mtypes.minordraw;
            //InitializeLists();
        }

        public override void AfterCloning()
        {

        }

        public override void InitializeLists()
        {
        }

        public override void Initialize(Node parent)
        {
            this.parent = parent;

        }

        public override void AffectOther(Node other) // called when used as a link
        {
            //other.transform.position += parent.transform.velocity;
            other.transform.position += parent.transform.effvelocity;

            other.comps[comp.movement].movementmode = movemode.free;
        }
        public override void AffectSelf()
        {
            
        }


        public override void Draw(SpriteBatch spritebatch)
        {
            Room room = parent.room;
            float mapzoom = room.mapzoom;

            Color col = parent.transform.color;


            spritebatch.Draw(parent.getTexture(), parent.transform.position / mapzoom, null, col, 0, parent.TextureCenter(), (parent.transform.scale / mapzoom) * 1.2f, SpriteEffects.None, 0);


        }

        public void onCollision(Dictionary<dynamic, dynamic> args)
        {
        }

    }
}
