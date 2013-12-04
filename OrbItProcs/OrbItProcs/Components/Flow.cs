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
    public class Flow : Component
    {

        public HashSet<Node> outgoing = new HashSet<Node>();
        public HashSet<Node> incoming = new HashSet<Node>();

        private bool _activated = false;
        public bool activated { get { return _activated; } set { _activated = value; } }

        private int _gatetype = 0;
        public int gatetype { get { return _gatetype; } set { _gatetype = value; } }

        public Flow() : this(null) { }
        public Flow(Node parent = null)
        {
            if (parent != null)
            {
                this.parent = parent;


            }
            com = comp.flow;
            methods = mtypes.affectself | mtypes.draw | mtypes.minordraw;
            //InitializeLists();

        }

        public override void AfterCloning()
        {
            //if (!parent.comps.ContainsKey(comp.queuer)) parent.addComponent(comp.queuer, true);
            if (!parent.comps.ContainsKey(comp.lifetime))
                parent.addComponent(comp.lifetime, true);

            parent.comps[comp.lifetime].immortal = true;

            /*
            if (!parent.comps.ContainsKey(comp.modifier))
                parent.addComponent(comp.modifier, true);
            //if (parent.comps.ContainsKey(comp.queuer)) 
            //parent.comps[comp.queuer].qs = parent.comps[comp.queuer].qs | queues.scale | queues.position;

            

            //MODIFIER
            ModifierInfo modinfo = new ModifierInfo();
            //modinfo.AddFPInfoFromString("o1", "scale", parent);
            //modinfo.AddFPInfoFromString("m1", "position", parent);
            modinfo.AddFPInfoFromString("v1", "position", parent);
            modinfo.AddFPInfoFromString("m1", "timer", parent.comps[comp.lifetime]);

            //modinfo.args.Add("mod", 4.0f);
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

            parent.comps[comp.modifier].modifierInfos["waver"] = modinfo;
            */


        }

        public override void InitializeLists()
        {
            outgoing = new HashSet<Node>();
            incoming = new HashSet<Node>();
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
            if (gatetype == 0)
                AND_Gate();
            else if (gatetype == 1)
                OR_Gate();
            else if (gatetype == 2)
                NOT_Gate();
        }

        public void AND_Gate()
        {
            if (incoming.Count == 0)
            {
                //activated = false;
                return;
            }
            foreach (Node input in incoming)
            {
                if (input.comps.ContainsKey(comp.flow))
                {
                    if (!input.comps[comp.flow].activated)
                    {
                        activated = false;
                        return;
                    }
                }
            }
            activated = true;
        }

        public void OR_Gate()
        {
            if (incoming.Count == 0)
            {
                //activated = false;
                return;
            }
            foreach (Node input in incoming)
            {
                if (input.comps.ContainsKey(comp.flow))
                {
                    if (input.comps[comp.flow].activated)
                    {
                        activated = true;
                        return;
                    }
                }
            }
            activated = false;
        }

        public void NOT_Gate()
        {
            if (incoming.Count == 0)
            {
                activated = false;
                return;
            }
            foreach (Node input in incoming)
            {
                if (input.comps.ContainsKey(comp.flow))
                {
                    activated = !input.comps[comp.flow].activated;
                    return;
                }
            }
            activated = false;
        }

        public void AddToOutgoing(Node node)
        {
            
            if (node != parent && node.comps.ContainsKey(comp.flow))
            {
                if (outgoing.Contains(node))
                {
                    outgoing.Remove(node);
                    node.comps[comp.flow].incoming.Remove(parent);
                }
                else
                {
                    outgoing.Add(node);
                    node.comps[comp.flow].incoming.Add(parent);
                }
            }
            
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            Room room = parent.room;
            float mapzoom = room.mapzoom;

            //int count = 0;
            //Queue<float> scales = parent.comps[comp.queuer].scales;
            //Queue<Vector2> positions = ((Queue<Vector2>)(parent.comps[comp.queuer].positions));
            Color col;
            if (activated)
                col = Color.Green;
            else
                col = Color.Red;


            spritebatch.Draw(parent.getTexture(), parent.position / mapzoom, null, col, 0, parent.TextureCenter(), (parent.scale / mapzoom) * 1.2f, SpriteEffects.None, 0);

            foreach (Node receiver in outgoing)
            {
                /*
                Color tempcol;
                if (receiver.comps[comp.flow].gatetype == 2 && receiver.comps[comp.flow])
                {
                    //indexof wont work...
                }
                */

                Utils.DrawLine(spritebatch, parent.position, receiver.position, 2f, col, room);
                Vector2 center = (receiver.position + parent.position) / 2;
                Vector2 perp = new Vector2(center.Y, -center.X);
                perp.Normalize();
                perp *= 10;
                //center += perp;
                Utils.DrawLine(spritebatch, center + perp, receiver.position, 2f, col, room);
                Utils.DrawLine(spritebatch, center - perp, receiver.position, 2f, col, room);

                
                //count++;
            }

            string gatestring = "";
            if (gatetype == 0) gatestring = "AND";
            else if (gatetype == 1) gatestring = "OR";
            else if (gatetype == 2) gatestring = "NOT";
            //spriteBatch.Begin();

            spritebatch.DrawString(room.game.font, gatestring, parent.position/mapzoom, Color.White, 0f, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);
            //spriteBatch.DrawString(spriteFont, fps, new Vector2(1, 1), Color.White, 0f, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);
            //spritebatch.DrawString(room.game.font, gatestring, new Vector2(2, Game1.sHeight - 40), Color.White, 0f, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);

            //spritebatch.Draw(parent.getTexture(), parent.position / mapzoom, null, parent.color, 0, parent.TextureCenter(), parent.scale / mapzoom, SpriteEffects.None, 0);

        }

        public void onCollision(Dictionary<dynamic, dynamic> args)
        {
        }

    }
}
