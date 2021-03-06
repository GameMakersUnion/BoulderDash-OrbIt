﻿using System;
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
        public const mtypes CompType = mtypes.draw | mtypes.tracer;
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
                if (parent != null && parent.HasComp<Queuer>() && parent.Comp<Queuer>().queuecount < value)
                {
                    parent.Comp<Queuer>().queuecount = value;
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
        public float thickness { get; set; }
        /// <summary>
        /// The ratio of the white core to the coloured outer parts of the laser.
        /// </summary>
        [Info(UserLevel.Advanced, "The ratio of the white core to the coloured outer parts of the laser.")]
        public float beamRatio { get; set; }
        /// <summary>
        /// The amount of laser beams to draw beside eachother, resulting in a thicker beam.
        /// </summary>
        [Info(UserLevel.User, "The amount of laser beams to draw beside eachother, resulting in a thicker beam.")]
        public int beamCount { get; set; }

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
            thickness = 5f;
            beamRatio = 0.7f;
            IsColorByAngle = true;
            beamCount = 1;
        }

        public override void AfterCloning()
        {
            //if (!parent.HasComp<Queuer>()) parent.addComponent(comp.queuer, true);
            //parent.Comp<Queuer>().qs = parent.Comp<Queuer>().qs | queues.scale | queues.position;// | queues.angle;
        }
        private Vector2 prevPos = Vector2.Zero;
        private int counter = 0, onceEveryAmount = 3;
        public override void Draw()
        {
            if (++counter % onceEveryAmount != 0) return; 
            Vector2 start = parent.body.pos;
            if (prevPos == Vector2.Zero)
            {
                prevPos = start;
                return;
            }
            //don't draw lines from screen edge to edge if screenwrapping
            if (parent.movement.mode == movemode.screenwrap)
            {
                float diffx = prevPos.X - start.X;
                if (diffx > parent.room.worldWidth / 2)
                {
                    start.X += parent.room.worldWidth;
                }
                else if (diffx < -parent.room.worldWidth / 2)
                {
                    start.X -= parent.room.worldWidth;
                }
                float diffy = prevPos.Y - start.Y;
                if (diffy > parent.room.worldHeight / 2)
                {
                    start.Y += parent.room.worldHeight;
                }
                else if (diffy < -parent.room.worldHeight / 2)
                {
                    start.Y -= parent.room.worldHeight;
                }
            }

            Vector2 diff = (prevPos - start);
            Vector2 centerpoint = (prevPos + start) / 2;
            float len = diff.Length();
            Vector2 scalevect;
            float xscale = len;
            float yscale = thickness;
            //float outerscale = yscale;
            //float beamdist = 1f;
            if (brightness)
            {
                xscale = brightness;
            }
            //if (thickness)
            //{
            //    yscale = thickness;
            //    outerscale = yscale * beamRatio;
            //}

            scalevect = new Vector2(xscale, yscale);

            float testangle = (float)(Math.Atan2(diff.Y, diff.X));

            VMath.NormalizeSafe(ref diff);
            diff = new Vector2(-diff.Y, diff.X);

            //uncommet later when not using direction based color shit
            Color coll;
            int alpha = 255;//i * (255 / min);
            //Console.WriteLine(alpha);
            if (IsColorByAngle)
            {
                coll = ColorChanger.getColorFromHSV((testangle + (float)Math.PI) * (float)(180 / Math.PI));
            }
            else
            {
                coll = new Color(parent.body.color.R, parent.body.color.G, parent.body.color.B, alpha);
            }
            parent.room.camera.AddPermanentDraw(textures.whitepixel, centerpoint, parent.body.color, scalevect, testangle, laserLength);
            prevPos = start;
        }
    }
}
