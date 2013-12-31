using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OrbItProcs.Processes;
using OrbItProcs.Components;

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
        public ILinkable linkComponent;
        public linktype ltype;
        public updatetime updateTime;
        public Action UpdateAction;

        public Node sourceNode;
        public Node targetNode;

        public ObservableHashSet<Node> sources;
        public ObservableHashSet<Node> targets;
        public ObservableHashSet<Node> exclusions;

        public Group sourceGroup;
        public Group targetGroup;

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

        public Link(Node sourceNode, Node targetNode, ILinkable linkComponent = null)
        {
            this.sourceNode = sourceNode;
            this.targetNode = targetNode;
            this.ltype = linktype.NodeToNode;
            this.updateTime = updatetime.SourceUpdate;
            this.linkComponent = linkComponent;
            linkComponent.parent = sourceNode; //fix this down there too

            sourceNode.transform.color = Microsoft.Xna.Framework.Color.Blue;
            targetNode.transform.color = Microsoft.Xna.Framework.Color.Blue;

            //sourceNode.OnAffectOthers += delegate { linkComponent.AffectOther(targetNode); };
            sourceNode.OnAffectOthers += delegate { UpdateNodeToNode(); };

            //sourceNode.room.AfterIteration += delegate { linkComponent.AffectOther(targetNode); };
            
        }
        public Link(Node sourceNode, HashSet<Node> targets, ILinkable linkComponent = null)
        {
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
            this.linkComponent = linkComponent;
            this.sources = new ObservableHashSet<Node>() { sourceNode };

            sourceNode.room.masterGroup.childGroups["Link Groups"].AddGroup(ts.Name, ts);

            sourceNode.OnAffectOthers += delegate { UpdateNodeToGroup(); };
        }
        public Link(HashSet<Node> sources, HashSet<Node> targets, ILinkable linkComponent = null)
        {
            Group ss = new Group();
            foreach (Node s in sources)
            {
                ss.entities.Add(s);
            }
            
            this.sourceGroup = ss;
            this.linkComponent = linkComponent;
            Group ts = new Group();
            foreach (Node t in targets)
            {
                ts.entities.Add(t);
            }
            this.targetGroup = ts;


            this.sources = sourceGroup.entities;
            this.targets = targetGroup.entities;
            this.linkComponent = linkComponent;
            this.ltype = linktype.GroupToGroup;
            this.updateTime = updatetime.SourceUpdate;

            sourceNode.room.masterGroup.childGroups["Link Groups"].AddGroup(ss.Name, ss);
            sourceNode.room.masterGroup.childGroups["Link Groups"].AddGroup(ts.Name, ts);

            //sourceNode.OnAffectOthers += delegate { UpdateGroupToGroup(); };

            foreach (Node s in sourceGroup.fullSet)
            {
                s.OnAffectOthers += (o, e) => UpdateNodeToGroup((Node)o);
            }
            sourceGroup.fullSet.CollectionChanged += sourceGroup_CollectionChanged;

        }
        public Link(Node sourceNode, Group targetGroup, ILinkable linkComponent = null)
        {
            this.sourceNode = sourceNode;
            this.targetGroup = targetGroup;
            this.targets = targetGroup.entities;

            this.ltype = linktype.NodeToGroup;
            this.updateTime = updatetime.SourceUpdate;
            this.linkComponent = linkComponent;

            this.sources = new ObservableHashSet<Node>() { sourceNode };

            sourceNode.OnAffectOthers += delegate { UpdateNodeToGroup(); };
        }
        public Link(Group sourceGroup, Group targetGroup, ILinkable linkComponent = null)
        {
            this.sourceGroup = sourceGroup;
            this.targetGroup = targetGroup;
            this.sources = sourceGroup.entities;
            this.targets = targetGroup.entities;

            this.ltype = linktype.GroupToGroup;
            this.updateTime = updatetime.SourceUpdate;
            this.linkComponent = linkComponent;

            foreach (Node s in sourceGroup.fullSet)
            {
                s.OnAffectOthers += (o, e) => UpdateNodeToGroup((Node)o);
            }
            sourceGroup.fullSet.CollectionChanged += sourceGroup_CollectionChanged;
        }

        void sourceGroup_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (Node n in e.NewItems)
                {
                    n.OnAffectOthers += (o, ee) => UpdateNodeToGroup((Node)o);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (Node n in e.OldItems)
                {
                    n.OnAffectOthers -= (o, ee) => UpdateNodeToGroup((Node)o);
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
            if (IsEntangled)
            {
                linkComponent.parent = sourceNode;
                linkComponent.AffectOther(targetNode);
                linkComponent.parent = targetNode;
                linkComponent.AffectOther(sourceNode);
            }
            else
            {
                linkComponent.parent = sourceNode;
                linkComponent.AffectOther(targetNode);
            }
        }

        public void UpdateNodeToGroup()
        {
            if (IsEntangled)
            {
                //this will affect the sourcenode for every target.. probably bad
                foreach (Node target in targets)
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
                foreach (Node target in targets)
                {
                    linkComponent.AffectOther(target);
                }
            }
        }

        public void UpdateNodeToGroup(Node source)
        {
            if (IsEntangled)
            {
                //this will affect the sourcenode for every target.. probably bad
                foreach (Node target in targets)
                {
                    linkComponent.parent = source;
                    linkComponent.AffectOther(target);
                    linkComponent.parent = target;
                    linkComponent.AffectOther(source);
                }
            }
            else
            {
                foreach (Node target in targets)
                {
                    linkComponent.parent = source;
                    linkComponent.AffectOther(target);
                }
            }
        }

        public void UpdateGroupToGroup()
        {
            if (IsEntangled)
            {
                //every sourcenode will affect every target, and vise-versa.. way too much going on
                foreach (Node source in sources)
                {
                    foreach (Node target in targets)
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
                    foreach (Node target in targets)
                    {
                        linkComponent.AffectOther(target);
                    }
                }
            }
            
        }




        


    }
}
