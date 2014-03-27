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
    /// Draws one or two waves behind the node in a trail. The wave is customizable in terms of amplitude, period, wave size, and so on.
    /// </summary>
    [Info(UserLevel.User, "Draws one or two waves behind the node in a trail. The wave is customizable in terms of amplitude, period, wave size, and so on.", CompType)]
    public class Waver : Component
    {
        public const mtypes CompType = mtypes.affectself | mtypes.draw | mtypes.tracer;
        public override mtypes compType { get { return CompType; } set { } }
        [Info(UserLevel.Developer)]
        public Queue<Vector2> metapositions = new Queue<Vector2>();
        private Queue<Vector2> reflectpositions = new Queue<Vector2>();

        public int _waveLength;
        /// <summary>
        /// Sets the length of the wave.
        /// </summary>
        [Info(UserLevel.User, "Sets the length of the wave. ")]
        [Polenter.Serialization.ExcludeFromSerialization]
        public int waveLength 
        { 
            get 
            {
                return _waveLength; 
            } 
            set 
            {
                _waveLength = value;
            } 
        }

        /// <summary>
        /// Sets the scale of the circles left by the wave trail
        /// </summary>
        [Info(UserLevel.User, "Sets the scale of the circles left by the wave trail")]
        public float waveScale { get; set; }

        /// <summary>
        /// How wide the wave will be at its maximum point.
        /// </summary>
        [Info(UserLevel.User, "How wide the wave will be at its maximum point.")]
        public float amp { get; set; }

        /// <summary>
        /// The frequency of the hills and valleys of the wave.
        /// </summary>
        [Info(UserLevel.User, "The frequency of the hills and valleys of the wave.")]
        public float period { get; set; }
        /// <summary>
        /// If set, two waves will appear instead of one, each a reflection of the other across the axis that is the node's path.
        /// </summary>
        [Info(UserLevel.User, "If set, two waves will appear instead of one, each a reflection of the other across the axis that is the node's path.")]
        public bool reflective { get; set; }
        /// <summary>
        /// The composite level of the sine wave. Google "Composite Sine Wave"  to understand this.
        /// </summary>
        [Info(UserLevel.Advanced, "The composite level of the sine wave. Google 'Composite Sine Wave'  to understand this.")]
        public int composite{get; set;}

        private float vshift = 0f;
        private float yval = 0f;


        public Waver() : this(null) { }
        public Waver(Node parent = null)
        {
            if (parent != null)
            {
                this.parent = parent;
            }
            com = comp.waver;
            amp = 20;
            period = 1000;
            composite = 1;
            waveScale = 0.3f;
            waveLength = 50;
        }

        public override void AfterCloning()
        {
            parent.addComponent(comp.lifetime, true);

            //if (!parent.comps.ContainsKey(comp.modifier)) 
            //    parent.addComponent(comp.modifier, true);

            /*//MODIFIER
            ModifierInfo modinfo = new ModifierInfo();
            modinfo.AddFPInfoFromString("v1", "pos", parent.body);
            modinfo.AddFPInfoFromString("m1", "timer", parent.comps[comp.lifetime]);

            modinfo.args.Add("amp", amp);
            modinfo.args.Add("period", period);
            modinfo.args.Add("composite", composite);
            modinfo.args.Add("vshift", 0.0f);
            modinfo.args.Add("yval", 0.0f);

            //modinfo.delegateName = "Mod";
            //modinfo.delegateName = "Triangle";
            //modinfo.delegateName = "VelocityToOutput";
            //modinfo.delegateName = "VectorSine";
            modinfo.delegateName = "VectorSineComposite";

            parent.comps[comp.modifier].modifierInfos["waver"] = modinfo;*/
        }

        public override void OnSpawn()
        {
            if (!active) return;
            if (parent.HasComp<Queuer>())
                parent.Comp<Queuer>().positions = metapositions;
        }

        public override void InitializeLists()
        {
            metapositions = new Queue<Vector2>();
            reflectpositions = new Queue<Vector2>();
        }
        public override void AffectSelf()
        {
            /*float yval = 0;
            if (parent.comps[comp.modifier].modifierInfos["waver"].args.ContainsKey("yval"))
            {
                yval = parent.comps[comp.modifier].modifierInfos["waver"].args["yval"];
            }*/
            float time = 0;
            if (parent.HasComp<Lifetime>())
            {
                time = parent.Comp<Lifetime>().lifetime;
            }

            yval = DelegateManager.SineComposite(time, amp, period, vshift, composite);

            Vector2 metapos = new Vector2(parent.body.velocity.Y, -parent.body.velocity.X);
            VMath.NormalizeSafe(ref metapos);
            metapos *= yval;
            Vector2 metaposfinal = parent.body.pos + metapos;

            if (metapositions.Count > waveLength)
            {
                metapositions.Dequeue();
            }
            metapositions.Enqueue(metaposfinal);

            if (reflective)
            {
                Vector2 reflectfinal = parent.body.pos - metapos;
                if (reflectpositions.Count > waveLength)
                {
                    reflectpositions.Dequeue();
                }
                reflectpositions.Enqueue(reflectfinal);
            }

        }

        public override void Draw()
        {
            float time = 0;
            if (parent.HasComp<Lifetime>())
            {
                time = parent.Comp<Lifetime>().lifetime;
            }
            else return;

            yval = DelegateManager.SineComposite(time, amp, period, vshift, composite);

            Vector2 metapos = new Vector2(parent.body.velocity.Y, -parent.body.velocity.X);
            VMath.NormalizeSafe(ref metapos);
            metapos *= yval;
            Vector2 metaposfinal = parent.body.pos + metapos;

            parent.room.camera.AddPermanentDraw(parent.texture, metaposfinal, parent.body.color, parent.body.scale * waveScale, 0, waveLength);
            //metapositions.Enqueue(metaposfinal);

            if (reflective)
            {
                Vector2 reflectfinal = parent.body.pos - metapos;
                parent.room.camera.AddPermanentDraw(parent.texture, reflectfinal, parent.body.color, parent.body.scale * waveScale, 0, waveLength);
                //reflectpositions.Enqueue(reflectfinal);
            }
        }

        public void DrawOld()
        {
            Room room = parent.room;
            //float mapzoom = room.zoom;

            foreach (Vector2 metapos in metapositions)
            {
                room.camera.Draw(parent.texture, metapos, parent.body.color, parent.body.scale * waveScale);
            }
            if (reflective)
            {
                foreach (Vector2 relectpos in reflectpositions)
                {
                    room.camera.Draw(parent.texture, relectpos, parent.body.color, parent.body.scale * waveScale);
                }
            }
        }

        public void onCollision(Dictionary<dynamic, dynamic> args)
        {
        }

    }
}
