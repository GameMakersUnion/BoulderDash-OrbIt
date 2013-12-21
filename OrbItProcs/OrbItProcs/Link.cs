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

    public class Link
    {
        public ILinkable linkComponent;
        public linktype ltype;
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

            if (linkComponent == null)
                UpdateAction = EmptyUpdate;
            else
                UpdateAction = UpdateNodeToNode;
        }
        public Link(Node sourceNode, HashSet<Node> targets, ILinkable linkComponent = null)
        {
            this.sourceNode = sourceNode;
            
            this.ltype = linktype.NodeToGroup;
            Group ts = new Group();
            foreach (Node t in targets)
            {
                ts.entities.Add(t);
            }
            this.targetGroup = ts;
            this.targets = ts.entities;

            if (linkComponent == null)
                UpdateAction = EmptyUpdate;
            else
            {
                SetLinkComponent(linkComponent);
                UpdateAction = UpdateNodeToGroup;
            }
        }
        public Link(HashSet<Node> sources, HashSet<Node> targets, ILinkable linkComponent = null)
        {
            Group ss = new Group();
            foreach (Node t in targets)
            {
                ss.entities.Add(t);
            }
            
            this.sourceGroup = ss;

            Group ts = new Group();
            foreach (Node t in targets)
            {
                ts.entities.Add(t);
            }
            this.targetGroup = ts;


            this.sources = sourceGroup.entities;
            this.targets = targetGroup.entities;

            this.ltype = linktype.GroupToGroup;

            if (linkComponent == null)
                UpdateAction = EmptyUpdate;
            else
            {
                SetLinkComponent(linkComponent);
                UpdateAction = UpdateGroupToGroup;
            }
        }
        public Link(Node sourceNode, Group targetGroup, ILinkable linkComponent = null)
        {
            this.sourceNode = sourceNode;
            this.targetGroup = targetGroup;
            this.targets = targetGroup.entities;

            this.ltype = linktype.NodeToGroup;

            if (linkComponent == null)
                UpdateAction = EmptyUpdate;
            else
            {
                SetLinkComponent(linkComponent);
                UpdateAction = UpdateNodeToGroup;
            }
        }
        public Link(Group sourceGroup, Group targetGroup, ILinkable linkComponent = null)
        {
            this.sourceGroup = sourceGroup;
            this.targetGroup = targetGroup;
            this.sources = sourceGroup.entities;
            this.targets = targetGroup.entities;

            this.ltype = linktype.GroupToGroup;

            if (linkComponent == null)
                UpdateAction = EmptyUpdate;
            else
            {
                SetLinkComponent(linkComponent);
                UpdateAction = UpdateGroupToGroup;
            }
        }

        public void SetLinkComponent(ILinkable component)
        {
            this.linkComponent = component;
            if (sourceNode != null)
                this.linkComponent.parent = sourceNode;
        }

        public void EmptyUpdate() { }

        public void Update()
        {
            
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
                foreach (Node target in targets)
                {
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
