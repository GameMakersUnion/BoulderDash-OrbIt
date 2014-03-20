using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbItProcs
{
    /// <summary>
    /// Replaces the Basic Circle drawing with a thin and long laser that trails behind the node's current position.
    /// </summary>
    [Info(UserLevel.User, "Replaces the Basic Circle drawing with a thin and long laser that trails behind the node's current position.", CompType)]
    public class Laser : Component
    {
        public const mtypes CompType = mtypes.affectself | mtypes.draw;
        public override mtypes compType { get { return CompType; } set { } }

        public int _laserLength = 10;
        /// <summary>
        /// Sets the length of the laser.
        /// </summary>
        [Info(UserLevel.User, "Sets the length of the laser. ")]
        [Polenter.Serialization.ExcludeFromSerialization]
        public int laserLength 
        { 
            get 
            {
                return _laserLength; 
            } 
            set 
            {
                if (parent != null && parent.HasComponent(comp.queuer) && parent[comp.queuer].queuecount < value)
                {
                    parent[comp.queuer].queuecount = value;
                }
                _laserLength = value;
            } 
        }
        /// <summary>
        /// Cool effect where the laser changes color depending on which way it's travelling. Consider using ColorChanger component and setting this to false.
        /// </summary>
        [Info(UserLevel.User, "Cool effect where the laser changes color depending on which way it's travelling. Consider using ColorChanger component and setting this to false.")]
        public bool IsColorByAngle { get; set; }
        /// <summary>
        /// If enabled, changes the brightness in a trippy way. Try setting it to decimal values for extra trippyness.
        /// </summary>
        [Info(UserLevel.Advanced, "If enabled, changes the brightness in a trippy way. Try setting it to decimal values for extra trippyness.")]
        public Toggle<float> brightness { get; set; }
        /// <summary>
        /// Sets the width of the line.
        /// </summary>
        [Info(UserLevel.User, "Sets the width of the line.")]
        public Toggle<float> thickness { get; set; }
        /// <summary>
        /// The ratio of the white core to the coloured outer parts of the laser.
        /// </summary>
        [Info(UserLevel.User, "The ratio of the white core to the coloured outer parts of the laser.")]
        public float beamRatio { get; set; }

        public Laser() : this(null) { }
        public Laser(Node parent = null)
        {
            if (parent != null)
            {
                this.parent = parent;
            }
            com = comp.laser; 
            InitializeLists();
            brightness = new Toggle<float>(1f, false);
            thickness = new Toggle<float>(1f, false);
            beamRatio = 0.7f;
            IsColorByAngle = true;
        }

        public override void AfterCloning()
        {
            if (!parent.comps.ContainsKey(comp.queuer)) parent.addComponent(comp.queuer, true);
            //if (parent.comps.ContainsKey(comp.queuer)) 
            parent.comps[comp.queuer].qs = parent.comps[comp.queuer].qs | queues.scale | queues.position;// | queues.angle;
            //int i = 0;
        }
        public override void Initialize(Node parent)
        {
            this.parent = parent;
        }
        public override void Draw(SpriteBatch spritebatch)
        {
            Room room = parent.room;
            //float mapzoom = room.zoom;

            Queue<float> scales = parent.comps[comp.queuer].scales;
            Queue<Vector2> positions = ((Queue<Vector2>)(parent.comps[comp.queuer].positions));

            //Vector2 pos = parent.body.pos;
            //Vector2 centerTexture = new Vector2(0.5f, 0.5f);

            Vector2 start = parent.body.pos;
            Vector2 end = Vector2.Zero;
            int count = 0;

            int min = Math.Min(positions.Count, scales.Count);
            min = Math.Min(min, laserLength);
            for (int i = 1; i <= min; i++)
            {

                start = positions.ElementAt(i - 1);
                if (i == positions.Count) end = parent.body.pos;
                else
                {
                    

                    end = positions.ElementAt(i);
                }
                if (end.IsFucked() || start.IsFucked()) break;
                //don't draw lines from screen edge to edge if screenwrapping
                if (parent.movement.mode == movemode.screenwrap)
                {
                    float diffx = end.X - start.X;
                    if (diffx > room.worldWidth / 2)
                    {
                        start.X += room.worldWidth;
                    }
                    else if (diffx < -room.worldWidth / 2)
                    {
                        start.X -= room.worldWidth;
                    }
                    float diffy = end.Y - start.Y;
                    if (diffy > room.worldHeight / 2)
                    {
                        start.Y += room.worldHeight;
                    }
                    else if (diffy < -room.worldHeight / 2)
                    {
                        start.Y -= room.worldHeight;
                    }
                }

                Vector2 diff = (end - start);
                Vector2 centerpoint = (end + start) / 2;
                float len = diff.Length();
                Vector2 scalevect;
                float xscale = len;
                float yscale = scales.ElementAt(i - 1) * 3;
                float outerscale = yscale;
                float beamdist = 1f;
                if (brightness)
                {
                    xscale = brightness;
                }
                if (thickness)
                {
                    yscale = thickness;
                    outerscale = yscale * beamRatio;


                }

                scalevect = new Vector2(xscale, yscale);

                float testangle = (float)(Math.Atan2(diff.Y, diff.X));

                diff.Normalize();
                diff = new Vector2(-diff.Y, diff.X);

                //uncommet later when not using direction based color shit
                Color coll;
                if (IsColorByAngle)
                {
                    coll = ColorChanger.getColorFromHSV((testangle + (float)Math.PI) * (float)(180 / Math.PI));
                }
                else
                {
                    coll = parent.body.color;
                }

                room.camera.Draw(textures.whitepixeltrans, centerpoint, new Color(255, 255, 255, coll.A), scalevect, testangle);

                scalevect.Y = outerscale * 0.9f;
                int beamnum = 1;
                for (int j = 0; j < beamnum; j++)
                {
                    beamdist = (outerscale * j + yscale) / 2f;
                    room.camera.Draw(textures.whitepixeltrans, centerpoint + diff * beamdist, coll, scalevect, testangle);
                    room.camera.Draw(textures.whitepixeltrans, centerpoint - diff * beamdist, coll, scalevect, testangle);
                }
                count++;
            }

        }

        public void DrawOld(SpriteBatch spritebatch)
        {
            //it would be really cool to have some kind of blending effects so that every combination of components will look diff
            Room room = parent.room;
            float mapzoom = room.zoom;
            //Color col = new Color(0f, 0f, 0f);

            Queue<float> scales = parent.comps[comp.queuer].scales;
            //Queue<float> angles = parent.comps[comp.queuer].angles;
            Queue<Vector2> positions = ((Queue<Vector2>)(parent.comps[comp.queuer].positions));

            Vector2 screenPos = parent.body.pos * mapzoom;
            Vector2 centerTexture = new Vector2(0.5f, 0.5f);

            Vector2 start = parent.body.pos;
            Vector2 end = Vector2.Zero;
            int count = 0;

            //foreach (Vector2 pos in positions)
            int min = Math.Min(positions.Count, scales.Count);
            for (int i = 1; i <= min; i++)
            {
                //Vector2 pos = positions.ElementAt(i);
                //float red = (float)i * (255f / queuecount) / 255f;
                //col = new Color(red, red, red);
                //col = Utils.randomColor();

                start = positions.ElementAt(i-1);
                if (i == positions.Count) end = parent.body.pos;
                else end = positions.ElementAt(i);

                
                //don't draw lines from screen edge to edge if screenwrapping
                //if (parent.movement.movementmode == movemode.screenwrap && diff.LengthSquared() > room.worldHeight * room.worldHeight / 4 && parent.body.velocity.LengthSquared() < room.worldHeight * room.worldHeight / 4) continue;
                if (parent.movement.mode == movemode.screenwrap)
                {
                    float diffx = end.X - start.X;
                    if (diffx > room.worldWidth / 2)
                    {
                        start.X += room.worldWidth;
                    }
                    else if (diffx < -room.worldWidth / 2)
                    {
                        start.X -= room.worldWidth;
                    }
                    float diffy = end.Y - start.Y;
                    if (diffy > room.worldHeight / 2)
                    {
                        start.Y += room.worldHeight;
                    }
                    else if (diffy < -room.worldHeight / 2)
                    {
                        start.Y -= room.worldHeight;
                    }

                }

                Vector2 diff = (end - start);
                diff *= mapzoom;
                Vector2 centerpoint = (end + start) / 2;
                centerpoint *= mapzoom;
                float len = diff.Length();
                Vector2 scalevect;
                float xscale = len;
                float yscale = scales.ElementAt(i - 1) * 3;
                float outerscale = yscale;
                float beamdist = 1f;
                if (brightness)
                {
                    xscale = brightness;
                }
                if (thickness)
                {
                    yscale = thickness;
                    outerscale = yscale * beamRatio;
                    

                }
                yscale *= 2 * mapzoom;
                outerscale *= 2 * mapzoom;

                scalevect = new Vector2(xscale, yscale);

                float testangle = (float)(Math.Atan2(diff.Y, diff.X));// + (Math.PI / 2));
                
                diff.Normalize();
                diff = new Vector2(-diff.Y, diff.X);
                //diff *= beamdist;


                //uncommet later when not using direction based color shit
                //color = new Color(color.R, color.G, color.B, 255/queuecount * i);
                //Console.WriteLine(testangle);
                Color coll;
                if (IsColorByAngle)
                {
                    coll = ColorChanger.getColorFromHSV((testangle + (float)Math.PI) * (float)(180 / Math.PI));
                }
                else
                {
                    coll = parent.body.color;
                }

                //if (alphaFade > 0) coll.A = (byte)(alphaFade * i * 255);

                //coll.A = (byte)i;
                //Console.WriteLine("{0} + {1}", i, coll.A);
                //coll = new Color(0.5f, 0.5f, 0.5f, 0f);

                spritebatch.Draw(parent.getTexture(textures.whitepixeltrans), centerpoint, null, new Color(255, 255, 255, coll.A), testangle, centerTexture, scalevect, SpriteEffects.None, 0);
                scalevect.Y = outerscale * 0.9f;
                //spritebatch.Draw(parent.getTexture(textures.whitepixel), centerpoint + diff, null, /*parent.transform.color*/coll, testangle, centerTexture, scalevect, SpriteEffects.None, 0);
                //spritebatch.Draw(parent.getTexture(textures.whitepixel), centerpoint - diff, null, /*parent.transform.color*/coll, testangle, centerTexture, scalevect, SpriteEffects.None, 0);
                int beamnum = 1;
                for (int j = 0; j < beamnum; j++)
                {
                    beamdist = (outerscale * j + yscale) / 2f;
                    //coll = new Color(new Vector4(coll.R, coll.G, coll.B, coll.A) - new Vector4(5, 5, 5, 0));
                    spritebatch.Draw(parent.getTexture(textures.whitepixeltrans), centerpoint + diff * beamdist, null, /*parent.transform.color*/coll, testangle, centerTexture, scalevect, SpriteEffects.None, 0);
                    spritebatch.Draw(parent.getTexture(textures.whitepixeltrans), centerpoint - diff * beamdist, null, /*parent.transform.color*/coll, testangle, centerTexture, scalevect, SpriteEffects.None, 0);

                }

                //spritebatch.Draw(parent.getTexture(textures.whitepixel), centerpoint + diff, null, /*parent.transform.color*/coll, testangle, centerTexture, scalevect, SpriteEffects.None, 0);
                //spritebatch.Draw(parent.getTexture(textures.whitepixel), centerpoint - diff, null, /*parent.transform.color*/coll, testangle, centerTexture, scalevect, SpriteEffects.None, 0);

                //alternating *= -1;
                //diff *= alternating * 3;
                //spritebatch.Draw(parent.getTexture(textures.whitepixel), centerpoint + diff, null, /*parent.transform.color*/coll, testangle, centerTexture, scalevect, SpriteEffects.None, 0);
                count++;
            }
            
        }

    }
}
