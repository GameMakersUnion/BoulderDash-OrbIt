using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;


namespace OrbItProcs
{
    public class Flow : Component, ILinkable
    {
        private Link _link = null;
        public Link link { get { return _link; } set { _link = value; } }
        public enum gate
        {
            None,
            AND,
            OR,
            NOT,
        }

        public bool active
        {
            get { return _active; }
            set
            {
                _active = value;
                if (!_active)
                {
                    foreach (Node n in outgoing.ToList())
                    {
                        outgoing.Remove(n);
                        n.comps[comp.flow].incoming.Remove(parent);
                    }
                    foreach (Node n in incoming.ToList())
                    {
                        incoming.Remove(n);
                        n.comps[comp.flow].outgoing.Remove(parent);
                    }
                }
            }
        }

        private HashSet<Node> _outgoing = new HashSet<Node>();
        [Polenter.Serialization.ExcludeFromSerialization]
        public HashSet<Node> outgoing { get { return _outgoing; } set { _outgoing = value; } }
        private HashSet<Node> _incoming = new HashSet<Node>();
        [Polenter.Serialization.ExcludeFromSerialization]
        public HashSet<Node> incoming { get { return _incoming; } set { _incoming = value; } }

        private bool _activated = false;
        public bool activated { get { return _activated; } set { _activated = value; } }

        private gate _gatetype = gate.AND;
        public gate gatetype { get { return _gatetype; } set { _gatetype = value; } }

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

            //parent.comps[comp.lifetime].immortal = true;


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
            if (!active) { return; }
        }
        public override void AffectSelf()
        {
            if (gatetype == gate.AND)
                AND_Gate();
            else if (gatetype == gate.OR)
                OR_Gate();
            else if (gatetype == gate.NOT)
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


            spritebatch.Draw(parent.getTexture(), parent.transform.position / mapzoom, null, col, 0, parent.TextureCenter(), (parent.transform.scale / mapzoom) * 1.2f, SpriteEffects.None, 0);

            foreach (Node receiver in outgoing)
            {
                /*
                Color tempcol;
                if (receiver.comps[comp.flow].gatetype == 2 && receiver.comps[comp.flow])
                {
                    //indexof wont work...
                }
                */

                Utils.DrawLine(spritebatch, parent.transform.position, receiver.transform.position, 2f, col, room);
                Vector2 center = (receiver.transform.position + parent.transform.position) / 2;
                Vector2 perp = new Vector2(center.Y, -center.X);
                perp.Normalize();
                perp *= 10;
                //center += perp;
                Utils.DrawLine(spritebatch, center + perp, receiver.transform.position, 2f, col, room);
                Utils.DrawLine(spritebatch, center - perp, receiver.transform.position, 2f, col, room);

                
                //count++;
            }

            string gatestring = "";
            if ((int)gatetype > 0) gatestring = gatetype.ToString();

            //spriteBatch.Begin();

            spritebatch.DrawString(room.game.font, gatestring, parent.transform.position/mapzoom, Color.White, 0f, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);
            //spriteBatch.DrawString(spriteFont, fps, new Vector2(1, 1), Color.White, 0f, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);
            //spritebatch.DrawString(room.game.font, gatestring, new Vector2(2, Game1.sHeight - 40), Color.White, 0f, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);

            //spritebatch.Draw(parent.getTexture(), parent.transform.position / mapzoom, null, parent.transform.color, 0, parent.TextureCenter(), parent.transform.scale / mapzoom, SpriteEffects.None, 0);

        }

        public void onCollision(Dictionary<dynamic, dynamic> args)
        {
        }

    }
}
