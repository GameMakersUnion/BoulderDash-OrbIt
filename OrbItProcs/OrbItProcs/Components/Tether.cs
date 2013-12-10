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
    public class Tether : Component
    {
        new public bool active
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
                        n.comps[comp.tether].incoming.Remove(parent);
                    }
                    foreach (Node n in incoming.ToList())
                    {
                        incoming.Remove(n);
                        n.comps[comp.tether].outgoing.Remove(parent);
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

        private bool _confining = false;
        public bool confining { get { return _confining; } set { if (!_confining && value) Confine(); _confining = value; } }
        private Dictionary<Node, Vector2> confiningVects;

        private bool _locked = false;
        public bool locked { get { return _locked; } set { if (!_locked && value) Lock(); _locked = value; } }

        private Dictionary<Node, int> lockedVals;

        public int _maxdist = 300;
        public int maxdist { get { return _maxdist; } set { _maxdist = value; if (_maxdist < _mindist) _maxdist = _mindist; } }
        public int _mindist = 100;
        public int mindist { get { return _mindist; } set { _mindist = value; if (_mindist > _maxdist) _mindist = _maxdist; } }

        public Tether() : this(null) { }
        public Tether(Node parent = null)
        {
            
            if (parent != null)
            {
                this.parent = parent;
            }
            com = comp.tether;
            methods = mtypes.affectself | mtypes.draw | mtypes.minordraw;
            //InitializeLists();
        }

        public override void AfterCloning()
        {
            //if (!parent.comps.ContainsKey(comp.queuer)) parent.addComponent(comp.queuer, true);
            if (!parent.comps.ContainsKey(comp.lifetime))
                parent.addComponent(comp.lifetime, true);

            parent.comps[comp.lifetime].immortal = true;

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
            if (activated)
            {
                
                foreach (Node n in outgoing)
                {
                    Vector2 diff = n.position - parent.position;
                    float len;
                    if (locked)
                    {
                        len = lockedVals[n];
                    }
                    else
                    {
                        len = diff.Length();
                    }

                    if (len > maxdist)
                    {
                        if (confining)
                        {
                            n.position = parent.position + confiningVects[n] * maxdist;
                        }
                        else
                        {
                            float percent = maxdist / len;
                            diff *= percent;
                            n.position = parent.position + diff;
                        }
                    }
                    else if (len < mindist)
                    {
                        if (confining)
                        {
                            n.position = parent.position + confiningVects[n] * mindist;
                        }
                        else
                        {
                            float percent = mindist / len;
                            diff *= percent;
                            n.position = parent.position + diff;
                        }
                    }
                    else
                    {
                        if (confining)
                        {
                            n.position = parent.position + confiningVects[n] * len;
                        }
                    }

                    
                    
                    //diff = n.position - parent.position;
                    //Console.WriteLine(diff.Length());
                }
            }
        }

        public void Confine()
        {
            if (parent == null) return;
            confiningVects = new Dictionary<Node, Vector2>();
            foreach (Node n in outgoing.ToList())
            {
                Vector2 len = n.position - parent.position;
                len.Normalize();
                confiningVects[n] = len;
            }
        }

        public void Lock()
        {
            if (parent == null) return;
            lockedVals = new Dictionary<Node, int>();
            foreach (Node n in outgoing.ToList())
            {
                Vector2 len = n.position - parent.position;
                lockedVals[n] = (int)len.Length();
            }
        }

        public void AddToOutgoing(Node node)
        {
            if (node != parent && node.comps.ContainsKey(comp.tether))
            {
                if (outgoing.Contains(node))
                {
                    outgoing.Remove(node);
                    node.comps[comp.tether].incoming.Remove(parent);
                    if (confining && confiningVects.ContainsKey(node)) confiningVects.Remove(node);
                    if (locked && lockedVals.ContainsKey(node)) lockedVals.Remove(node);
                }
                else
                {
                    outgoing.Add(node);
                    node.comps[comp.tether].incoming.Add(parent);
                    if (confining && !confiningVects.ContainsKey(node))
                    {
                        Vector2 v = (node.position - parent.position); v.Normalize();
                        confiningVects.Add(node, v);
                    }
                    if (locked && !lockedVals.ContainsKey(node)) lockedVals.Add(node, (int)(node.position - parent.position).Length());
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
                col = Color.Blue;
            else
                col = Color.White;

            spritebatch.Draw(parent.getTexture(), parent.position / mapzoom, null, col, 0, parent.TextureCenter(), (parent.scale / mapzoom) * 1.2f, SpriteEffects.None, 0);

            foreach (Node receiver in outgoing)
            {
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

            //spritebatch.DrawString(room.game.font, gatestring, parent.position / mapzoom, Color.White, 0f, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);
            //spriteBatch.DrawString(spriteFont, fps, new Vector2(1, 1), Color.White, 0f, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);
            //spritebatch.DrawString(room.game.font, gatestring, new Vector2(2, Game1.sHeight - 40), Color.White, 0f, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);

            //spritebatch.Draw(parent.getTexture(), parent.position / mapzoom, null, parent.color, 0, parent.TextureCenter(), parent.scale / mapzoom, SpriteEffects.None, 0);

        }

        public void onCollision(Dictionary<dynamic, dynamic> args)
        {
        }

    }
}
