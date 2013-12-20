using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace OrbItProcs.Processes
{
    public enum GroupState { off, drawingOnly, updatingOnly, on };

    public class Group
    {
        public Group parentGroup { get; set; }
        public ObservableHashSet<Node> entities { get; set; }
        public ObservableHashSet<Node> foreigners { get; set; }
        public ObservableHashSet<Node> fullSet { get; set; }
        private Dictionary<string, Group> _childGroups;
        public Dictionary<string, Group> childGroups
        {
            get
            {
                return _childGroups;
            }
            set
            {
                _childGroups = value;
            }
        }
        public Node defaultNode { get; set; }
        public Room room;
        private string _Name;
        public string Name { get { return _Name; } set { if (_Name != null && _Name.Equals("master")) return; _Name = value; } } //cannot rename main group
        public GroupState groupState { get; set; }

        public Group() : this(null)
        {
        }
        public Group(Node defaultNode = null, ObservableHashSet<Node> entities = null, Group parentGroup = null, GroupState groupState = GroupState.on, string Name = "newlist")
        {
            room = Program.getRoom();
            this.defaultNode = defaultNode ?? room.defaultNode;
            this.entities = entities ?? new ObservableHashSet<Node>();
            this.foreigners = new ObservableHashSet<Node>();
            this.fullSet = new ObservableHashSet<Node>();
            this.parentGroup = parentGroup;
            this.groupState = groupState;
            this.Name = Name;
            this.childGroups = new Dictionary<string, Group>();
            this.entities.CollectionChanged += entities_CollectionChanged;
        }

        void entities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                
                foreach (Node n in e.NewItems)
                {
                    if (room.game.ui.sidebar.cmbListPicker.Text.Equals(Name))
                    {
                        room.game.ui.sidebar.lstMain.Items.Add(n);
                        room.game.ui.sidebar.SyncTitleNumber(this);
                    }
                    if (parentGroup != null && !parentGroup.entities.Contains(n))
                    {
                        //Console.WriteLine("Adding {0} to {1}", n.name, Name);
                        parentGroup.foreigners.Add(n);
                    }
                    fullSet.Add(n);
                }
                room.game.ui.sidebar.SyncTitleNumber(this);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (Node n in e.OldItems)
                {
                    if (room.game.ui.sidebar.cmbListPicker.Text.Equals(Name))
                    {
                        room.game.ui.sidebar.lstMain.Items.Remove(n);
                        room.game.ui.sidebar.SyncTitleNumber(this);
                    }
                    fullSet.Remove(n);
                }
                
            }
        }
        //adds entity to current group and all parent groups
        public void IncludeEntity(Node entity)
        {
            //if (!entities.Contains(entity))
                entities.Add(entity);

            if (parentGroup != null)
                parentGroup.IncludeEntity(entity);
        }
        //removes entity from current group and all child groups
        public void DiscludeEntity(Node entity)
        {
            if (entity is Node && parentGroup == null) ((Node)entity).active = false;

            ///if (entities.Contains(entity))
                entities.Remove(entity);

            if (childGroups.Count > 0)
            {
                foreach (Group childgroup in childGroups.Values.ToList())
                {
                    childgroup.DiscludeEntity(entity);
                }
            }
        }
        //removes entity from all groups, starting from the highest root
        public void DeleteEntity(Node entity)
        {
            if (entity is Node) ((Node)entity).IsDeleted = true;

            Group root = this;
            while (root.parentGroup != null)
            {
                root = root.parentGroup;
            }
            root.DiscludeEntity(entity);
        }

        public void ForEachAll(Action<Node> action)
        {
            entities.ToList().ForEach(action);
            foreigners.ToList().ForEach(action);
        }

        public void TraverseGroups()
        {
            foreach (Group g in childGroups.Values.ToList())
            {
                g.TraverseGroups();
            }
        }

        //dunno about this
        public void RemoveFromChildrenDeep(Node toremove)
        {
            if (entities.Contains(toremove)) entities.Remove(toremove);
            if (childGroups.Count == 0) return;

            foreach(string s in childGroups.Keys.ToList())
            {
                childGroups[s].RemoveFromChildrenDeep(toremove);
            }
        }

        public void Update(GameTime gametime)
        {
            if (groupState.In(GroupState.on, GroupState.updatingOnly))
            {
                entities.ToList().ForEach(delegate(Node n) { ((Node)n).Update(gametime); });
            }
        }

        public void Draw(SpriteBatch spritebatch)
        {
            if (groupState.In(GroupState.on, GroupState.drawingOnly))
            {
                entities.ToList().ForEach(delegate(Node n) { ((Node)n).Draw(spritebatch); });
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public Group FindGroup(string name)
        {
            if (name.Equals(Name)) return this;

            foreach (string s in childGroups.Keys.ToList())
            {
                if (name.Equals(s)) return childGroups[s];
            }
            if (parentGroup != null) return parentGroup;
            return this;
        }

        public void AddGroup(string name, Group group, bool updateCmb = true)
        {
            if (childGroups.ContainsKey(name))
            {
                Console.WriteLine("Error: One of the childGroups with the same key was already present in this Group.");
                return;
            }
            childGroups.Add(name, group);

            if (updateCmb) UpdateComboBox();
        }

        public void UpdateComboBox()
        {
            List<object> list = room.game.ui.sidebar.cmbListPicker.Items;
            foreach (object o in list.ToList())
            {
                list.Remove(o);
            }
            list.Add(Name);
            foreach (string s in childGroups.Keys.ToList())
            {
                list.Add(s);
            }
            list.Add("Other Objects");
        }

        //unfortunately I'm not sure it makes sense to use this awesome method
        public static void ForEachDictionary (Dictionary<string,Group> dict, Action<object> action)
        {
            //dict.Values.ToList().Select(x => x.entities).Aggregate((x, y) => (ObservableCollection<object>)x.Union(y)).ToList().ForEach(action);
            HashSet<object> hashset = new HashSet<object>();
            dict.Keys.ToList().ForEach(delegate(string key) {
                Group g = dict[key];

                g.entities.ToList().ForEach(delegate(Node o)
                { 
                    if (!hashset.Contains(o))
                    {
                        hashset.Add(o);
                        action(o);
                    }
                });
            });
        }

    }
}
