using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


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
        //public ILinkable linkComponent { get; set; }
        //[Polenter.Serialization.ExcludeFromSerialization]
        public ObservableHashSet<ILinkable> components { get; set; }
        /*public HashSet<Component> componentsP
        {
            get
            {
                HashSet<Component> result = new HashSet<Component>();
                components.ToList().ForEach(w => result.Add((Component)w));
                return result;
            }
            set{
                HashSet<ILinkable> result = new HashSet<ILinkable>();
                value.ToList().ForEach(w => result.Add((ILinkable)w));
                components = result;
            }
        }*/

        public linktype ltype { get; set; }
        public updatetime _UpdateTime = updatetime.SourceUpdate;
        public updatetime UpdateTime { get { return _UpdateTime; } set { _UpdateTime = value; } }
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
        public bool _Reversed = false;
        public bool Reversed { get { return _Reversed; } set { _Reversed = value; } }
        public bool _DrawTips = false;
        public bool DrawTips { get { return _DrawTips; } set { _DrawTips = value; } }
        public float _AngleInc = 0.2f;
        public float AngleInc { get { return _AngleInc; } set { _AngleInc = value; } }
        [Polenter.Serialization.ExcludeFromSerialization]
        public Node sourceNode { get; set; }
        [Polenter.Serialization.ExcludeFromSerialization]
        public Node targetNode { get; set; }
        [Polenter.Serialization.ExcludeFromSerialization]
        public ObservableHashSet<Node> sources { get; set; }
        [Polenter.Serialization.ExcludeFromSerialization]
        public ObservableHashSet<Node> targets { get; set; }
        [Polenter.Serialization.ExcludeFromSerialization]
        public ObservableHashSet<Node> exclusions { get; set; }
        [Polenter.Serialization.ExcludeFromSerialization]
        public Group sourceGroup { get; set; }
        [Polenter.Serialization.ExcludeFromSerialization]
        public Group targetGroup { get; set; }

        public bool _IsEntangled = false;
        public bool IsEntangled
        {
            get { return _IsEntangled; }
            set
            {
                //linkComponent.parent = sourceNode;
                _IsEntangled = value;
            }
        }

        private float anglestep = 0;

        public Link()
        {
            //..
            this.room = Program.getRoom();
            this.components = new ObservableHashSet<ILinkable>();
        }

        //blank link (for the palette)
        public Link(ILinkable linkComponent, formationtype ftype = formationtype.AllToAll)
        {
            this.room = Program.getRoom();
            this.components = new ObservableHashSet<ILinkable>();
            this.components.Add(linkComponent);
            this._FormationType = ftype;
            this.formation = new Formation(this, ftype, InitializeFormation: false);
        }

        public Link(Link link, dynamic source, dynamic target)
        {
            this.room = Program.getRoom();
            this.components = new ObservableHashSet<ILinkable>();

            this.UpdateTime = link.UpdateTime;
            this.IsEntangled = link.IsEntangled;
            this.Reversed = link.Reversed;
            this.AngleInc = link.AngleInc;
            this.DrawTips = link.DrawTips;

            foreach (ILinkable component in link.components)
            {
                dynamic newComponent = Activator.CreateInstance(component.GetType());
                Component.CloneComponent((Component)component, newComponent);
                newComponent.active = true;
                newComponent.link = this;
                if (newComponent.GetType().GetProperty("activated") != null) newComponent.activated = true;

                this.components.Add(newComponent);
            }

            Initialize(source, target, null, link.formation);

            

        }

        private void Initialize(dynamic src, dynamic trg, ILinkable linkComponent, dynamic formation)
        {
            this.room = Program.getRoom();
            
            if (components == null)
            {
                this.components = new ObservableHashSet<ILinkable>();
            }
            if (linkComponent != null)
            {
                linkComponent.link = this;
                this.components.Add(linkComponent);
            }


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
                //linkComponent.parent = sourceNode;
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
                room.game.ui.sidebar.UpdateGroupComboBoxes();

                foreach (Node s in this.sources)
                {
                    s.OnAffectOthers += NodeToGroupHandler;
                    s.SourceLinks.Add(this);
                }
                this.sources.CollectionChanged += sourceGroup_CollectionChanged;

                if (trg is HashSet<Node> && src == trg)
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
                room.game.ui.sidebar.UpdateGroupComboBoxes();

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
        //constructors
        public Link(Node sourceNode, Node targetNode, ILinkable linkComponent = null, dynamic formation = null)
        {
            Initialize(sourceNode, targetNode, linkComponent, formation);
        }
        public Link(Node sourceNode, HashSet<Node> targets, ILinkable linkComponent = null, dynamic formation = null)
        {
            Initialize(sourceNode, targets, linkComponent, formation);
        }
        public Link(HashSet<Node> sources, Node targetNode, ILinkable linkComponent = null, dynamic formation = null)
        {
            Initialize(sources, targetNode, linkComponent, formation);
        }
        public Link(HashSet<Node> sources, HashSet<Node> targets, ILinkable linkComponent = null, dynamic formation = null)
        {
            Initialize(sources, targets, linkComponent, formation);
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
        }
        public Link(Group sourceGroup, Node targetNode, ILinkable linkComponent = null, dynamic formation = null)
        {
            Initialize(sourceGroup, targetNode, linkComponent, formation);
        }
        public Link(Group sourceGroup, Group targetGroup, ILinkable linkComponent = null, dynamic formation = null)
        {
            Initialize(sourceGroup, targetGroup, linkComponent, formation);
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
                    if (_FormationType == formationtype.AllToAll && formation != null && !formation.AffectionSets.ContainsKey(n))
                    {
                        formation.AffectionSets[n] = targets;
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (Node n in e.OldItems)
                {
                    //n.OnAffectOthers -= (o, ee) => UpdateNodeToGroup((Node)o);
                    n.OnAffectOthers -= NodeToGroupHandler;
                    n.SourceLinks.Remove(this);
                    if (formation != null && formation.AffectionSets.ContainsKey(n))
                    {
                        formation.AffectionSets.Remove(n);
                    }
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

        public void AddLinkComponent(ILinkable linkC, bool active = true)
        {
            linkC.link = this;
            linkC.active = active;
            components.Add(linkC);
        }

        public void UpdateNodeToNode()
        {
            if (!active) return;

            if (IsEntangled)
            {
                foreach (ILinkable link in components)
                {
                    link.parent = sourceNode;
                    link.AffectOther(targetNode);
                    link.parent = targetNode;
                    link.AffectOther(sourceNode);
                }
            }
            else
            {
                foreach (ILinkable link in components)
                {
                    link.parent = sourceNode;
                    link.AffectOther(targetNode);
                }
            }
        }
        
        public void UpdateNodeToGroup(Node source)
        {
            if (!active) return;
            if (IsEntangled)
            {
                if (!formation.AffectionSets.ContainsKey(source)) return;
                foreach (ILinkable link in components)
                {
                    foreach (Node target in formation.AffectionSets[source])
                    {
                        if (source == target) continue;

                        link.parent = source;
                        link.AffectOther(target);
                        link.parent = target;
                        link.AffectOther(source);
                    }
                }
            }
            else
            {
                if (!formation.AffectionSets.ContainsKey(source)) return;

                foreach (ILinkable link in components)
                {
                    link.parent = source;
                    foreach (Node target in formation.AffectionSets[source])
                    {
                        if (source == target) continue;
                        link.AffectOther(target);
                    }
                }
            }
        }
        //unused for now as well
        /*
        public void UpdateNodeToGroup()
        {
            if (!active) return;
            if (IsEntangled)
            {
                foreach (ILinkable link in components)
                {
                    foreach (Node target in formation.AffectionSets[sourceNode])
                    {
                        link.parent = sourceNode;
                        link.AffectOther(target);
                        link.parent = target;
                        link.AffectOther(sourceNode);
                    }
                }
            }
            else
            {
                foreach (ILinkable link in components)
                {
                    link.parent = sourceNode;
                    foreach (Node target in formation.AffectionSets[sourceNode])
                    {
                        link.AffectOther(target);
                    }
                }
            }
        }
        */
        //unused for now
        /*
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
        */
        public void GenericDraw(SpriteBatch spritebatch)
        {
            if (!active) return;
            //if (!linkComponent.active)
                //return;

            float mapzoom = room.mapzoom;


            Color col;
            /*
            if (linkComponent.activated)
                col = Color.Blue;
            else
                col = Color.White;
            */
            col = Color.Blue;

            //float increment = 0.5f;
            anglestep = 0;
            
            //Component lcomp = (Component)linkComponent;
            //col = Group.IntToColor[(int)lcomp.com % Group.IntToColor.Count];

            foreach (Node source in sources)
            {
                //col = source.transform.color;

                //spritebatch.Draw(source.getTexture(), source.transform.position / mapzoom, null, col, 0, source.TextureCenter(), (source.transform.scale / mapzoom) * 1.2f, SpriteEffects.None, 0);

                if (!formation.AffectionSets.ContainsKey(source)) continue;
                foreach (Node target in formation.AffectionSets[source])
                {
                    anglestep += AngleInc;
                    
                    int[] cs = HueShifter.getColorsFromAngle(anglestep % 360);
                    Color color1 = new Color(cs[0], cs[1], cs[2]);

                    Vector2 diff = target.body.pos - source.body.pos;
                    Vector2 perp = new Vector2(diff.Y, -diff.X);
                    perp.Normalize();
                    perp *= 2;

                    Utils.DrawLine(room, source.body.pos, target.body.pos, 2f, color1);

                    //Utils.DrawLine(spritebatch, source.transform.position + perp, target.transform.position + perp, 2f, col, room);
                    //Utils.DrawLine(spritebatch, source.transform.position - perp, target.transform.position - perp, 2f, col, room);

                    if (!DrawTips) continue;
                    perp *= 20;

                    Vector2 center = (target.body.pos + source.body.pos) / 2;

                    Vector2 point = target.body.pos - (diff / 5);
                    Utils.DrawLine(room, point + perp, target.body.pos, 2f, color1);
                    Utils.DrawLine(room, point - perp, target.body.pos, 2f, color1);
                }
            }

        }

        public override string ToString()
        {
            string result = "[L]";
            if (components != null)
            {
                foreach(ILinkable link in components)
                {
                    Component c = (Component)link;
                    result += c.com.ToString().Substring(0, 4) + "|";
                }
            }
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
                sources.CollectionChanged -= sourceGroup_CollectionChanged;
                foreach (Node n in sources)
                {
                    n.OnAffectOthers -= NodeToGroupHandler;
                    n.SourceLinks.Remove(this);
                }
            }
            if (targets != null)
            {
                targets.CollectionChanged -= targetGroup_CollectionChanged;
                foreach (Node n in targets)
                {
                    n.TargetLinks.Remove(this);
                }
            }
            if (sourceGroup != null) sourceGroup.SourceLinks.Remove(this);
            if (targetGroup != null) targetGroup.TargetLinks.Remove(this);

            

            room.AllActiveLinks.Remove(this);
        }

        public static void GetILinkableEnumVals(List<object> list)
        {
            foreach (comp key in Enum.GetValues(typeof(comp)))
            {
                Type compType = Game1.compTypes[key];

                if (!typeof(ILinkable).IsAssignableFrom(compType)) continue;

                list.Add(key);

                /*
                MethodInfo mInfo = compType.GetMethod("AffectOther");
                if (mInfo != null
                    && mInfo.DeclaringType == compType)
                {
                    list.Add(key);
                }
                */
            }
        }


    }
}
