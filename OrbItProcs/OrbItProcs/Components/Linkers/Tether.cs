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
    public class Tether : Component, ILinkable
    {
        new public bool active
        {
            get { return _active; }
            set
            {
                _active = value;
                if (!_active)
                {
                    foreach (Node other in outgoing.ToList())
                    {
                        outgoing.Remove(other);
                        other.comps[comp.tether].incoming.Remove(parent);
                    }
                    foreach (Node other in incoming.ToList())
                    {
                        
                        other.comps[comp.tether].outgoing.Remove(parent);
                        incoming.Remove(other);
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
            //methods = mtypes.affectother | mtypes.draw | mtypes.minordraw;
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

        public override void AffectOther(Node other) // called when used as a link
        {
            if (activated)
            {
                Vector2 diff = other.transform.position - parent.transform.position;
                float len;
                if (locked)
                {
                    len = lockedVals[other];
                }
                else
                {
                    len = diff.Length();
                }

                if (len > maxdist)
                {
                    if (confining)
                    {
                        other.transform.position = parent.transform.position + confiningVects[other] * maxdist;
                    }
                    else
                    {
                        float percent = maxdist / len;
                        diff *= percent;
                        other.transform.position = parent.transform.position + diff;
                    }
                }
                else if (len < mindist)
                {
                    if (confining)
                    {
                        other.transform.position = parent.transform.position + confiningVects[other] * mindist;
                    }
                    else
                    {
                        float percent = mindist / len;
                        diff *= percent;
                        other.transform.position = parent.transform.position + diff;
                    }
                }
                else
                {
                    if (confining)
                    {
                        other.transform.position = parent.transform.position + confiningVects[other] * len;
                        //Console.WriteLine("{0}, {1}, {2}", confiningVects[other], other.transform.position, len);
                    }
                }
                //diff = other.transform.position - parent.transform.position;
                //Console.WriteLine(diff.Length());
                
            }
        }
        public override void AffectSelf()
        {
            if (activated)
            {
                foreach (Node other in outgoing)
                {
                    Vector2 diff = other.transform.position - parent.transform.position;
                    float len;
                    if (locked)
                    {
                        len = lockedVals[other];
                    }
                    else
                    {
                        len = diff.Length();
                    }

                    if (len > maxdist)
                    {
                        if (confining)
                        {
                            other.transform.position = parent.transform.position + confiningVects[other] * maxdist;
                        }
                        else
                        {
                            float percent;
                            if (locked) percent = len / diff.Length();
                            else percent = maxdist / len;
                            diff *= percent;
                            other.transform.position = parent.transform.position + diff;
                        }
                    }
                    else if (len < mindist)
                    {
                        if (confining)
                        {
                            other.transform.position = parent.transform.position + confiningVects[other] * mindist;
                        }
                        else
                        {
                            float percent;
                            if (locked) percent = len / diff.Length();
                            else percent = mindist / len;
                            diff *= percent;
                            other.transform.position = parent.transform.position + diff;
                        }
                    }
                    else
                    {
                        if (confining)
                        {
                            other.transform.position = parent.transform.position + confiningVects[other] * len;
                            //Console.WriteLine("{0}, {1}, {2}", confiningVects[other], other.transform.position, len);
                        }
                        if (locked)
                        {
                            float percent = len / diff.Length();
                            //else percent = mindist / len;
                            diff *= percent;
                            other.transform.position = parent.transform.position + diff;
                        }
                    }

                    
                    
                    //diff = other.transform.position - parent.transform.position;
                    //Console.WriteLine(diff.Length());
                }
            }
        }

        public void Confine()
        {
            if (parent == null) return;
            confiningVects = new Dictionary<Node, Vector2>();
            foreach (Node other in outgoing.ToList())
            {
                Vector2 len = other.transform.position - parent.transform.position;
                len.Normalize();
                confiningVects[other] = len;
            }
        }

        public void Lock()
        {
            if (parent == null) return;
            lockedVals = new Dictionary<Node, int>();
            foreach (Node other in outgoing.ToList())
            {
                Vector2 len = other.transform.position - parent.transform.position;
                lockedVals[other] = (int)len.Length();
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
                        Vector2 v = (node.transform.position - parent.transform.position); v.Normalize();
                        confiningVects.Add(node, v);
                    }
                    if (locked && !lockedVals.ContainsKey(node)) lockedVals.Add(node, (int)(node.transform.position - parent.transform.position).Length());
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
                col = parent.transform.color;
            else
                col = Color.White;

            spritebatch.Draw(parent.getTexture(), parent.transform.position / mapzoom, null, col, 0, parent.TextureCenter(), (parent.transform.scale / mapzoom) * 1.2f, SpriteEffects.None, 0);

            foreach (Node receiver in outgoing)
            {
                Vector2 diff = receiver.transform.position - parent.transform.position;
                Vector2 perp = new Vector2(diff.Y, -diff.X);
                perp.Normalize();
                perp *= 2;

                Utils.DrawLine(spritebatch, parent.transform.position, receiver.transform.position, 2f, col, room);

                Utils.DrawLine(spritebatch, parent.transform.position + perp, receiver.transform.position + perp, 2f, Color.Red, room);
                Utils.DrawLine(spritebatch, parent.transform.position - perp, receiver.transform.position - perp, 2f, Color.Green, room);
                
                perp *= 20;

                Vector2 center = (receiver.transform.position + parent.transform.position) / 2;
                
                //Utils.DrawLine(spritebatch, center + perp, receiver.transform.position, 1f, col, room);
                //Utils.DrawLine(spritebatch, center - perp, receiver.transform.position, 1f, col, room);

                Vector2 point = receiver.transform.position - (diff / 5);
                Utils.DrawLine(spritebatch, point + perp, receiver.transform.position, 2f, col, room);
                Utils.DrawLine(spritebatch, point - perp, receiver.transform.position, 2f, col, room);
            }

            //spritebatch.DrawString(room.game.font, gatestring, parent.transform.position / mapzoom, Color.White, 0f, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);
            //spriteBatch.DrawString(spriteFont, fps, new Vector2(1, 1), Color.White, 0f, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);
            //spritebatch.DrawString(room.game.font, gatestring, new Vector2(2, Game1.sHeight - 40), Color.White, 0f, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);

            //spritebatch.Draw(parent.getTexture(), parent.transform.position / mapzoom, null, parent.transform.color, 0, parent.TextureCenter(), parent.transform.scale / mapzoom, SpriteEffects.None, 0);

        }

        public void onCollision(Dictionary<dynamic, dynamic> args)
        {
        }

    }
}
