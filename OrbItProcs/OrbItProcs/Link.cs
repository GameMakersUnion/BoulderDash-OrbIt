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
        single,
        group,
        grouptogroup,

    }

    public class Link
    {
        public ILinkable linkComponent;

        public Node source;
        public Node target;

        public HashSet<Node> targets;
        public HashSet<Node> exclusions;

        public Group sourceGroup;
        public Group targetGroup;

        public bool IsEntangled = false;

        public Link(Group targetGroup = null)
        {
            //the targets set is now synced with the group's entities
            if (targetGroup != null)
                targets = targetGroup.entities.hashSet;

        }
    }
}
