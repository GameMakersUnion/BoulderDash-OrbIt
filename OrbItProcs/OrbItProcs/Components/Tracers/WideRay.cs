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
    /// Replaces the basic draw with a set of lines that trail behind the node, perpendicular to its direction. is said to look like a caterpillar.
    /// </summary>
    [Info(UserLevel.User, "Replaces the basic draw with a set of lines that trail behind the node, perpendicular to its direction. is said to look like a caterpillar.", CompType)]
    public class WideRay : Component
    {
        public const mtypes CompType = mtypes.draw | mtypes.tracer;
        public override mtypes compType { get { return CompType; } set { } }

        public int _rayLength = 10;
        /// <summary>
        /// Sets the length of the ray.
        /// </summary>
        [Info(UserLevel.User, "Sets the length of the ray. ")]
        [Polenter.Serialization.ExcludeFromSerialization]
        public int rayLength
        {
            get
            {
                return _rayLength;
            }
            set
            {
                if (parent != null && parent.HasComp<Queuer>() && parent.Comp<Queuer>().queuecount < value)
                {
                    parent.Comp<Queuer>().queuecount = value;
                }
                _rayLength = value;
            }
        }

        //private double angle = 0;
        private float rayscale = 20;
        private int width = 3;

        public WideRay() : this(null) { }
        public WideRay(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.wideray;
            InitializeLists();  
        }

        public override void AfterCloning()
        {
            if (!parent.HasComp<Queuer>()) parent.addComponent(comp.queuer, true);
            //if (parent.comps.ContainsKey(comp.queuer)) 
            parent.Comp<Queuer>().qs = parent.Comp<Queuer>().qs | queues.scale | queues.position | queues.angle;
            //int i = 0;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            Room room = parent.room;
            float mapzoom = room.zoom;

            Queue<float> scales = parent.Comp<Queuer>().scales;
            Queue<float> angles = parent.Comp<Queuer>().angles;
            Queue<Vector2> positions = ((Queue<Vector2>)(parent.Comp<Queuer>().positions));


            Vector2 screenPos = parent.body.pos * mapzoom;
            Vector2 centerTexture = new Vector2(0.5f, 0.5f);

            int count = 0;
            Vector2 scalevect = new Vector2(rayscale, width);
            int min = Math.Min(Math.Min(positions.Count, scales.Count), angles.Count);
            for (int i = 0; i < min; i++)
            {
                scalevect.X = scales.ElementAt(i) * 50;
                //spritebatch.Draw(parent.getTexture(textures.whitepixel), positions.ElementAt(i) * mapzoom, null, parent.body.color, angles.ElementAt(i), centerTexture, scalevect, SpriteEffects.None, 0);
                room.camera.Draw(parent.getTexture(textures.whitepixel), positions.ElementAt(i), null, parent.body.color, angles.ElementAt(i), centerTexture, scalevect, SpriteEffects.None, 0);
                count++;
            }

            float testangle = (float)(Math.Atan2(parent.body.velocity.Y, parent.body.velocity.X) + (Math.PI / 2));
            scalevect.X = parent.body.scale * 50;
            //spritebatch.Draw(parent.getTexture(textures.whitepixel), parent.body.pos * mapzoom, null, parent.body.color, testangle, centerTexture, scalevect, SpriteEffects.None, 0);
            room.camera.Draw(parent.getTexture(textures.whitepixel), parent.body.pos, null, parent.body.color, testangle, centerTexture, scalevect, SpriteEffects.None, 0);
            
        }

        public void onCollision(Dictionary<dynamic, dynamic> args)
        {
        }

    }
}
