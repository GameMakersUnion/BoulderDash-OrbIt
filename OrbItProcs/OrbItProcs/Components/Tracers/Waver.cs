using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;


namespace OrbItProcs
{
    public class Waver : Component
    {
        private bool _reflective = false;
        public bool reflective { get { return _reflective; } set { _reflective = value; } }
        public Queue<Vector2> metapositions = new Queue<Vector2>();
        public Queue<Vector2> reflectpositions = new Queue<Vector2>();
        [Polenter.Serialization.ExcludeFromSerialization]
        public int queuecount { get { if (parent != null && parent.HasComponent(comp.queuer)) return parent[comp.queuer].queuecount; else return 10; } set { if (parent != null && parent.HasComponent(comp.queuer)) parent[comp.queuer].queuecount = value; } }

        private float _amp = 100;
        public float amp
        {
            get { return _amp; }
            set 
            {
                _amp = value;
            } 
        }
        private float _period = 30;
        public float period
        {
            get { return _period; }
            set
            {
                _period = value;
            }
        }
        private int _composite = 1;
        public int composite
        {
            get { return _composite; }
            set
            {
                _composite = value;
            }
        }
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
            methods = mtypes.affectself | mtypes.draw;
            //InitializeLists();
        }

        public override void AfterCloning()
        {
            if (!parent.comps.ContainsKey(comp.lifetime)) 
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
            if (parent.comps.ContainsKey(comp.queuer))
                parent.comps[comp.queuer].positions = metapositions;
        }

        public override void InitializeLists()
        {
            metapositions = new Queue<Vector2>();
            reflectpositions = new Queue<Vector2>();
        }

        public override void AffectOther(Node other)
        {

        }
        public override void AffectSelf()
        {
            /*float yval = 0;
            if (parent.comps[comp.modifier].modifierInfos["waver"].args.ContainsKey("yval"))
            {
                yval = parent.comps[comp.modifier].modifierInfos["waver"].args["yval"];
            }*/
            float time = 0;
            if (parent.comps.ContainsKey(comp.lifetime))
            {
                time = parent.comps[comp.lifetime].mseconds;
            }

            yval = DelegateManager.SineComposite(time, amp, period, vshift, composite);

            Vector2 metapos = new Vector2(parent.body.velocity.Y, -parent.body.velocity.X);
            metapos.Normalize();
            metapos *= yval;
            Vector2 metaposfinal = parent.body.pos + metapos;


            if (metapositions.Count > queuecount)
            {
                metapositions.Dequeue();
            }
            metapositions.Enqueue(metaposfinal);

            if (reflective)
            {
                Vector2 reflectfinal = parent.body.pos - metapos;
                if (reflectpositions.Count > queuecount)
                {
                    reflectpositions.Dequeue();
                }
                reflectpositions.Enqueue(reflectfinal);
            }

        }

        public override void Draw(SpriteBatch spritebatch)
        {
            Room room = parent.room;
            float mapzoom = room.zoom;

            int count = 0;
            //Queue<float> scales = parent.comps[comp.queuer].scales;
            //Queue<Vector2> positions = ((Queue<Vector2>)(parent.comps[comp.queuer].positions));

            foreach (Vector2 metapos in metapositions)
            {
                spritebatch.Draw(parent.getTexture(), metapos * mapzoom, null, parent.body.color, 0, parent.TextureCenter(), parent.body.scale * mapzoom, SpriteEffects.None, 0);

                //if (metapos == reflectpositions.ElementAt(count)) Console.WriteLine("YEA");
                count++;
                
            }
            count = 0;
            foreach (Vector2 relectpos in reflectpositions)
            {
                spritebatch.Draw(parent.getTexture(), relectpos * mapzoom, null, parent.body.color, 0, parent.TextureCenter(), parent.body.scale * mapzoom, SpriteEffects.None, 0);
                count++;
            }

            //spritebatch.Draw(parent.getTexture(), parent.transform.position / mapzoom, null, parent.transform.color, 0, parent.TextureCenter(), parent.transform.scale / mapzoom, SpriteEffects.None, 0);

        }

        public void onCollision(Dictionary<dynamic, dynamic> args)
        {
        }

    }
}
