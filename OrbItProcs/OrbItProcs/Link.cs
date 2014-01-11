using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OrbItProcs.Processes;
using OrbItProcs.Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace OrbItProcs
{
    public enum linktype
    {
        none,
        NodeToNode,
        NodeToGroup,
        GroupToGroup,

    }

    public enum updatetime
    {
        SourceUpdate,
        RoomUpdate,
    }

    public class Link
    {
        private bool _active = true;
        public bool active { get { return _active; } 
            set 
            {
                if (value && !_active && formation != null)
                {
                    formation.UpdateFormation();
                }
                _active = value; 
            } 
        }
        public Room room;
        public ILinkable linkComponent { get; set; }
        public HashSet<ILinkable> components { get; set; }

        public linktype ltype { get; set; }
        public updatetime updateTime { get; set; }
        //public Action UpdateAction { get; set; }
        public Formation formation { get; set; }
        private formationtype _FormationType;
        public formationtype FormationType { get { return _FormationType; }
            set {
                if (_FormationType != value && formation != null)
                {
                    _FormationType = value;
                    formation.UpdateFormation();
                }
                else
                {
                    _FormationType = value; 
                }
                
            } 
        }
        public bool Reversed { get; set; }
        public Node sourceNode { get; set; }
        public Node targetNode { get; set; }

        public ObservableHashSet<Node> sources { get; set; }
        public ObservableHashSet<Node> targets { get; set; }
        public ObservableHashSet<Node> exclusions { get; set; }

        public Group sourceGroup { get; set; }
        public Group targetGroup { get; set; }

        public bool _IsEntangled = false;
        public bool IsEntangled
        {
            get { return _IsEntangled; }
            set
            {
                linkComponent.parent = sourceNode;
                _IsEntangled = value;
            }
        }

        //blank link (for the palette)
        public Link(ILinkable linkComponent, formationtype ftype = formationtype.AllToAll)
        {
            this.room = Program.getRoom();
            this.linkComponent = linkComponent;
            this._FormationType = ftype;
            this.formation = new Formation(this, ftype, InitializeFormation: false);

        }

        private void Initialize(dynamic src, dynamic trg, ILinkable linkComponent, dynamic formation)
        {
            this.room = Program.getRoom();
            this.updateTime = updatetime.SourceUpdate;
            this.linkComponent = linkComponent;

            if (src is Node && trg is Node) this.ltype = linktype.NodeToNode;
            else if (src is Node && (trg is HashSet<Node> || trg is Group)) this.ltype = linktype.NodeToGroup;
            else this.ltype = linktype.GroupToGroup;

            bool EqualSets = false;

            //source
            if (src is Node)
            {
                this.sourceNode = src;
                this.sources = new ObservableHashSet<Node>() { sourceNode };
                sourceNode.SourceLinks.Add(this);
                linkComponent.parent = sourceNode;
                if (trg is Node)
                {
                    sourceNode.OnAffectOthers += NodeToNodeHandler;
                }
                else
                {
                    sourceNode.OnAffectOthers += NodeToGroupHandler;
                }

            }
            else if (src is HashSet<Node>)
            {
                Group ss = new Group();
                foreach (Node s in src)
                {
                    ss.entities.Add(s);
                }

                this.sourceGroup = ss;
                this.sourceGroup.SourceLinks.Add(this);
                this.sources = this.sourceGroup.fullSet;

                room.masterGroup.childGroups["Link Groups"].AddGroup(ss.Name, ss);

                foreach (Node s in this.sources)
                {
                    s.OnAffectOthers += NodeToGroupHandler;
                    s.SourceLinks.Add(this);
                }
                this.sources.CollectionChanged += sourceGroup_CollectionChanged;

                if (src == trg)
                {
                    EqualSets = true;
                    this.targetGroup = this.sourceGroup;
                    this.targetGroup.TargetLinks.Add(this);
                    this.targets = this.targetGroup.fullSet;
                    foreach (Node t in this.targets)
                    {
                        t.TargetLinks.Add(this);
                    }
                }

                
            }
            else if (src is Group)
            {
                this.sourceGroup = src;
                this.sources = this.sourceGroup.fullSet;

                this.sourceGroup.SourceLinks.Add(this);


                foreach (Node s in sources)
                {
                    s.OnAffectOthers += NodeToGroupHandler;
                    s.SourceLinks.Add(this);

                }
                this.sourceGroup.fullSet.CollectionChanged += sourceGroup_CollectionChanged;

                
            }
            else
            {
                Console.WriteLine("Unrecongized source type when creating link");
            }

            //target
            if (trg is Node)
            {
                this.targetNode = trg;
                this.targets = new ObservableHashSet<Node>() { targetNode };
                targetNode.TargetLinks.Add(this);
            }
            else if (trg is HashSet<Node> && !EqualSets)
            {
                Group ts = new Group();
                foreach (Node t in trg)
                {
                    ts.entities.Add(t);
                }
                this.targetGroup = ts;
                this.targets = this.targetGroup.fullSet;
                this.targetGroup.TargetLinks.Add(this);

                room.masterGroup.childGroups["Link Groups"].AddGroup(ts.Name, ts);

                foreach (Node t in this.targets)
                {
                    t.TargetLinks.Add(this);
                }
                this.targets.CollectionChanged += targetGroup_CollectionChanged;
            }
            else if (trg is Group)
            {
                this.targetGroup = trg;
                this.targets = this.targetGroup.fullSet;
                this.targetGroup.TargetLinks.Add(this);

                foreach (Node t in targets)
                {
                    t.TargetLinks.Add(this);
                }
                this.targetGroup.fullSet.CollectionChanged += targetGroup_CollectionChanged;
            }
            else
            {
                Console.WriteLine("Unrecongized target type when creating link");
            }

            if (formation == null)
            {
                this._FormationType = formationtype.AllToAll;
                this.formation = new Formation(this, formationtype.AllToAll);
            }
            else if (formation is formationtype)
            {
                this._FormationType = formation;
                this.formation = new Formation(this, formation);
            }
            else if (formation is Formation)
            {
                this._FormationType = formation.FormationType;
                this.formation = new Formation(this, formation);
            }

            
        }

        public Link(Node sourceNode, Node targetNode, ILinkable linkComponent = null, dynamic formation = null)
        {
            Initialize(sourceNode, targetNode, linkComponent, formation);

            /*
            this.room = Program.getRoom();
            this.sourceNode = sourceNode;
            this.targetNode = targetNode;
            this.ltype = linktype.NodeToNode;
            this.updateTime = updatetime.SourceUpdate;
            this.linkComponent = linkComponent;
            linkComponent.parent = sourceNode; //fix this down there too

            this.sources = new ObservableHashSet<Node>() { sourceNode };
            this.targets = new ObservableHashSet<Node>() { targetNode };

            //sourceNode.transform.color = Microsoft.Xna.Framework.Color.Blue;
            //targetNode.transform.color = Microsoft.Xna.Framework.Color.Blue;

            sourceNode.OnAffectOthers += NodeToNodeHandler;

            this._FormationType = ftype;
            formation = new Formation(this, ftype);
            //this.FormationType = formation.FormationType;

            sourceNode.SourceLinks.Add(this);
            targetNode.TargetLinks.Add(this);
            */
        }
        public Link(Node sourceNode, HashSet<Node> targets, ILinkable linkComponent = null, dynamic formation = null)
        {
            Initialize(sourceNode, targets, linkComponent, formation);
            /*
            this.room = Program.getRoom();
            this.sourceNode = sourceNode;

            this.ltype = linktype.NodeToGroup;
            this.updateTime = updatetime.SourceUpdate;
            Group ts = new Group();
            foreach (Node t in targets)
            {
                ts.entities.Add(t);
            }
            this.targetGroup = ts;
            this.targets = ts.entities;
            this.targetGroup.TargetLinks.Add(this);

            this.linkComponent = linkComponent;
            this.sources = new ObservableHashSet<Node>() { sourceNode };


            sourceNode.room.masterGroup.childGroups["Link Groups"].AddGroup(ts.Name, ts);

            this._FormationType = ftype;
            formation = new Formation(this, ftype);

            sourceNode.OnAffectOthers += NodeToGroupHandler;
            sourceNode.SourceLinks.Add(this);

            foreach (Node t in this.targets)
            {
                t.TargetLinks.Add(this);
            }
            this.targets.CollectionChanged += targetGroup_CollectionChanged;

            */
        }
        public Link(HashSet<Node> sources, Node targetNode, ILinkable linkComponent = null, dynamic formation = null)
        {
            Initialize(sources, targetNode, linkComponent, formation);
        }
        public Link(HashSet<Node> sources, HashSet<Node> targets, ILinkable linkComponent = null, dynamic formation = null)
        {
            Initialize(sources, targets, linkComponent, formation);
            /*
            this.room = Program.getRoom();
            Group ss = new Group();
            foreach (Node s in sources)
            {
                ss.entities.Add(s);
            }
            
            this.sourceGroup = ss;
            this.sourceGroup.SourceLinks.Add(this);

            this.linkComponent = linkComponent;
            Group ts = new Group();
            foreach (Node t in targets)
            {
                ts.entities.Add(t);
            }
            this.targetGroup = ts;
            this.targetGroup.TargetLinks.Add(this);

            this.sources = sourceGroup.entities;
            this.targets = targetGroup.entities;
            this.linkComponent = linkComponent;
            this.ltype = linktype.GroupToGroup;
            this.updateTime = updatetime.SourceUpdate;

            sourceNode.room.masterGroup.childGroups["Link Groups"].AddGroup(ss.Name, ss);
            sourceNode.room.masterGroup.childGroups["Link Groups"].AddGroup(ts.Name, ts);

            //sourceNode.OnAffectOthers += delegate { UpdateGroupToGroup(); };
            this._FormationType = ftype;
            formation = new Formation(this, ftype);
            //this.FormationType = formation.FormationType;

            foreach (Node s in this.sources)
            {
                //s.OnAffectOthers += (o, e) => UpdateNodeToGroup((Node)o);
                s.OnAffectOthers += NodeToGroupHandler;
                s.SourceLinks.Add(this);
            }
            this.sources.CollectionChanged += sourceGroup_CollectionChanged;

            foreach (Node t in this.targets)
            {
                t.TargetLinks.Add(this);
            }
            this.targets.CollectionChanged += targetGroup_CollectionChanged;
            */
        }
        public Link(HashSet<Node> sources, Group targetGroup, ILinkable linkComponent = null, dynamic formation = null)
        {
            Initialize(sources, targetGroup, linkComponent, formation);
        }
        public Link(Group sourceGroup, HashSet<Node> targets, ILinkable linkComponent = null, dynamic formation = null)
        {
            Initialize(sourceGroup, targets, linkComponent, formation);
        }


        public Link(Node sourceNode, Group targetGroup, ILinkable linkComponent = null, dynamic formation = null)
        {
            Initialize(sourceNode, targetGroup, linkComponent, formation);
            /*
            this.room = Program.getRoom();
            this.sourceNode = sourceNode;
            this.targetGroup = targetGroup;
            this.targets = targetGroup.entities;
            this.targetGroup.TargetLinks.Add(this);

            this.ltype = linktype.NodeToGroup;
            this.updateTime = updatetime.SourceUpdate;
            this.linkComponent = linkComponent;

            this.sources = new ObservableHashSet<Node>() { sourceNode };

            //sourceNode.OnAffectOthers += delegate { UpdateNodeToGroup(); }; //unused method

            sourceNode.OnAffectOthers += NodeToGroupHandler;
            sourceNode.SourceLinks.Add(this);

            this._FormationType = ftype;
            formation = new Formation(this, ftype);
            //this.FormationType = formation.FormationType;


            foreach (Node t in targets)
            {
                t.TargetLinks.Add(this);
            }
            this.targets.CollectionChanged += targetGroup_CollectionChanged;
            */
        }
        public Link(Group sourceGroup, Node targetNode, ILinkable linkComponent = null, dynamic formation = null)
        {
            Initialize(sourceGroup, targetNode, linkComponent, formation);
        }

        public Link(Group sourceGroup, Group targetGroup, ILinkable linkComponent = null, dynamic formation = null)
        {
            Initialize(sourceGroup, targetGroup, linkComponent, formation);
            /*
            this.room = Program.getRoom();
            this.sourceGroup = sourceGroup;
            this.targetGroup = targetGroup;
            this.sources = sourceGroup.fullSet;
            this.targets = targetGroup.fullSet;

            this.ltype = linktype.GroupToGroup;
            this.updateTime = updatetime.SourceUpdate;
            this.linkComponent = linkComponent;

            this.targetGroup.TargetLinks.Add(this);
            this.sourceGroup.SourceLinks.Add(this);

            this._FormationType = ftype;
            formation = new Formation(this, ftype);

            foreach (Node s in sources)
            {
                s.OnAffectOthers += NodeToGroupHandler;
                s.SourceLinks.Add(this);
                
            }
            sourceGroup.fullSet.CollectionChanged += sourceGroup_CollectionChanged;

            foreach (Node t in targets)
            {
                t.TargetLinks.Add(this);
            }
            targetGroup.fullSet.CollectionChanged += targetGroup_CollectionChanged;
            */

        }
        //handlers
        public void NodeToNodeHandler(object sender, EventArgs e)
        {
            UpdateNodeToNode();
        }
        public void NodeToGroupHandler(object sender, EventArgs e)
        {
            UpdateNodeToGroup((Node)sender);
        }
        

        void sourceGroup_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                
                foreach (Node n in e.NewItems)
                {
                    //n.OnAffectOthers += (o, ee) => UpdateNodeToGroup((Node)o);
                    n.OnAffectOthers += NodeToGroupHandler;
                    n.SourceLinks.Add(this);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (Node n in e.OldItems)
                {
                    //n.OnAffectOthers -= (o, ee) => UpdateNodeToGroup((Node)o);
                    n.OnAffectOthers -= NodeToGroupHandler;
                    n.SourceLinks.Remove(this);
                }
            }
        }

        void targetGroup_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                
                foreach (Node n in e.NewItems)
                {
                    n.TargetLinks.Add(this);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (Node n in e.OldItems)
                {
                    n.TargetLinks.Remove(this);
                }
            }
        }

        public void SetLinkComponent(ILinkable component)
        {
            this.linkComponent = component;
            if (sourceNode != null)
                this.linkComponent.parent = sourceNode;
        }

        public void UpdateNodeToNode()
        {
            if (!active) return;
            if (IsEntangled)
            {
                linkComponent.parent = sourceNode;
                linkComponent.AffectOther(targetNode);
                linkComponent.parent = targetNode;
                linkComponent.AffectOther(sourceNode);
            }
            else
            {
                //implied that sourceNode != targetNode
                linkComponent.parent = sourceNode;
                linkComponent.AffectOther(targetNode);
            }
        }
        //unused for now as well
        public void UpdateNodeToGroup()
        {
            if (!active) return;
            if (IsEntangled)
            {
                //this will affect the sourcenode for every target.. probably bad
                foreach (Node target in formation.AffectionSets[sourceNode])
                {
                    linkComponent.parent = sourceNode;
                    linkComponent.AffectOther(target);
                    linkComponent.parent = target;
                    linkComponent.AffectOther(sourceNode);
                }
            }
            else
            {
                linkComponent.parent = sourceNode;
                foreach (Node target in formation.AffectionSets[sourceNode])
                {
                    linkComponent.AffectOther(target);
                }
            }
        }

        public void UpdateNodeToGroup(Node source)
        {
            if (!active) return;
            if (IsEntangled)
            {
                if (!formation.AffectionSets.ContainsKey(source)) return;
                //this will affect the sourcenode for every target.. probably bad
                foreach (Node target in formation.AffectionSets[source])
                {
                    if (source == target) continue;

                    linkComponent.parent = source;
                    linkComponent.AffectOther(target);
                    linkComponent.parent = target;
                    linkComponent.AffectOther(source);
                }
            }
            else
            {
                if (!formation.AffectionSets.ContainsKey(source)) return;

                linkComponent.parent = source;
                foreach (Node target in formation.AffectionSets[source])
                {
                    if (source == target) continue;
                    linkComponent.AffectOther(target);
                }
            }
        }
        //unused for now
        public void UpdateGroupToGroup()
        {
            if (!active) return;
            if (IsEntangled)
            {
                //every sourcenode will affect every target, and vise-versa.. way too much going on
                foreach (Node source in sources)
                {
                    foreach (Node target in formation.AffectionSets[source])
                    {
                        linkComponent.parent = source;
                        linkComponent.AffectOther(target);
                        linkComponent.parent = target;
                        linkComponent.AffectOther(source);
                    }
                }
            }
            else
            {
                //every sourcenode will affect every target... probably too much going on.
                foreach (Node source in sources)
                {
                    linkComponent.parent = source;
                    foreach (Node target in formation.AffectionSets[source])
                    {
                        linkComponent.AffectOther(target);
                    }
                }
            }
            
        }

        public void GenericDraw(SpriteBatch spritebatch)
        {
            if (!active) return;
            if (!linkComponent.active)
                return;

            float mapzoom = room.mapzoom;


            Color col;
            /*
            if (linkComponent.activated)
                col = Color.Blue;
            else
                col = Color.White;
            */
            //col = Color.Blue;
            Component lcomp = (Component)linkComponent;
            col = Group.IntToColor[(int)lcomp.com % Group.IntToColor.Count];

            foreach (Node source in sources)
            {
                //col = source.transform.color;

                //spritebatch.Draw(source.getTexture(), source.transform.position / mapzoom, null, col, 0, source.TextureCenter(), (source.transform.scale / mapzoom) * 1.2f, SpriteEffects.None, 0);

                if (!formation.AffectionSets.ContainsKey(source)) continue;
                foreach (Node target in formation.AffectionSets[source])
                {
                    Vector2 diff = target.transform.position - source.transform.position;
                    Vector2 perp = new Vector2(diff.Y, -diff.X);
                    perp.Normalize();
                    perp *= 2;

                    Utils.DrawLine(spritebatch, source.transform.position, target.transform.position, 2f, col, room);

                    //Utils.DrawLine(spritebatch, source.transform.position + perp, target.transform.position + perp, 2f, col, room);
                    //Utils.DrawLine(spritebatch, source.transform.position - perp, target.transform.position - perp, 2f, col, room);

                    perp *= 20;

                    Vector2 center = (target.transform.position + source.transform.position) / 2;

                    Vector2 point = target.transform.position - (diff / 5);
                    Utils.DrawLine(spritebatch, point + perp, target.transform.position, 2f, col, room);
                    Utils.DrawLine(spritebatch, point - perp, target.transform.position, 2f, col, room);
                }
            }

        }

        public override string ToString()
        {
            string result = "[Link]";
            if (linkComponent != null) result += "[" + ((Component)linkComponent).com + "]";
            result += "[" + FormationType + "]";
            return result;
        }

        public void DeleteLink()
        {

            if (sourceNode != null)
            { 
                sourceNode.SourceLinks.Remove(this);
                sourceNode.OnAffectOthers -= NodeToNodeHandler;
            }
            if (targetNode != null) targetNode.TargetLinks.Remove(this);

            if (sources != null)
            {
                foreach (Node n in sources)
                {
                    n.OnAffectOthers -= NodeToGroupHandler;
                    n.SourceLinks.Remove(this);
                }
            }
            if (targets != null)
            {
                foreach (Node n in targets)
                {
                    n.TargetLinks.Remove(this);
                }
            }
            if (sourceGroup != null) sourceGroup.SourceLinks.Remove(this);
            if (targetGroup != null) targetGroup.TargetLinks.Remove(this);

            room.AllActiveLinks.Remove(this);
        }

        


    }
}
