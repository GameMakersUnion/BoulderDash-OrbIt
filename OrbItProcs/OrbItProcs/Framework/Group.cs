using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public enum GroupState { off, drawingOnly, updatingOnly, on };

    public class Group
    {
        public static HashSet<string> groupHashes = new HashSet<string>();

        public string _groupHash = "";
        public string groupHash { get { return _groupHash; } set {

            groupHashes.Remove(_groupHash);
            _groupHash = value;

            if (groupHashes.Contains(value))
                room.findGroupByHash(value).groupHash =
                    Utils.uniqueString(groupHashes);

            groupHashes.Add(value);
        } }

        public static int GroupNumber = 0;
        public static Dictionary<int, Color> IntToColor = new Dictionary<int, Color>()
        {
            { 0, Color.White },
            { 1, Color.Green },
            { 2, Color.Red },
            { 3, Color.Blue },
            { 4, Color.Purple },
            { 5, Color.RosyBrown },
            { 6, Color.YellowGreen },
            { 7, Color.DarkGreen },
            { 8, Color.LightBlue },
            { 9, Color.Violet },

        };

        public int GroupId = -1;

        [Polenter.Serialization.ExcludeFromSerialization]
        public Group parentGroup { get; set; }
        //
        public ObservableHashSet<Node> fullSet;
        [Polenter.Serialization.ExcludeFromSerialization]
        public ObservableHashSet<Node> fullSetP
        {
            get { return fullSet; }
            set {
                fullSet = value; ;
            }
        }

        public ObservableHashSet<Node> entities;// { get; set; }
        public ObservableHashSet<Node> inherited;// { get; set; }

        public ObservableHashSet<Node> entitiesP { get { return entities; }
            set
            {
                if (entities == null || entities.Count > 0) throw new SystemException("Entities was null was setting entitiesP");

                foreach (Node n in value.ToList()) // another coke -dante
                {
                    Node newNode = n.CreateClone();
                    //entities.Add(newNode);
                    IncludeEntity(newNode);
                }
            }
        }
        public ObservableHashSet<Node> inheritedP
        {
            get { return inherited; }
            set
            {
                return;
                /*if (inherited == null || inherited.Count > 0) throw new SystemException("inherited was null was setting inheritedP");
                foreach (Node n in value.ToList())
                {
                    Node newNode = n.CreateClone();
                    inherited.Add(newNode);
                }*/
            }
        }
        
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
                foreach(Group g in _childGroups.Values)
                {
                    g.parentGroup = this;
                }
            }
        }
        public Node defaultNode { get; set; }
        public Room room;
        private string _Name;
        public string Name { get { return _Name; } set { if (_Name != null && _Name.Equals("master")) return; _Name = value; } } //cannot rename main group
        public bool Spawnable { get; set; }
        public GroupState groupState { get; set; }

        private ObservableHashSet<Link> _SourceLinks = new ObservableHashSet<Link>();
        [Polenter.Serialization.ExcludeFromSerialization]
        public ObservableHashSet<Link> SourceLinks { get { return _SourceLinks; } set { _SourceLinks = value; } }

        private ObservableHashSet<Link> _TargetLinks = new ObservableHashSet<Link>();
        [Polenter.Serialization.ExcludeFromSerialization]
        public ObservableHashSet<Link> TargetLinks { get { return _TargetLinks; } set { _TargetLinks = value; } }

        public bool PolenterHack { get { return true; }
            set
            {
                if (fullSet == null) fullSet = new ObservableHashSet<Node>();
                foreach(Node n in entities)
                {
                    fullSet.Add(n);
                }
                foreach(Node n in inherited)
                {
                    fullSet.Add(n);
                }
                foreach(Group g in childGroups.Values)
                {
                    foreach(Node n in g.fullSet)
                    {
                        fullSet.Add(n);
                    }
                }
            }
        }

        public Group() : this(null)
        {
        }
        public Group(Node defaultNode = null, ObservableHashSet<Node> entities = null, Group parentGroup = null, GroupState groupState = GroupState.on, string Name = "", bool Spawnable = true)
        {
            groupHash = Utils.uniqueString(groupHashes);
            room = Program.getRoom();
            this.defaultNode = defaultNode ?? room.defaultNode;
            this.entities = entities ?? new ObservableHashSet<Node>();
            this.inherited = new ObservableHashSet<Node>();
            this.fullSet = new ObservableHashSet<Node>();
            if (entities != null)
            {
                foreach (Node e in entities)
                {
                    fullSet.Add(e);
                }
            }
            this.parentGroup = parentGroup;
            this.groupState = groupState;
            this.Spawnable = Spawnable;
            this.childGroups = new Dictionary<string, Group>();
            this.entities.CollectionChanged += entities_CollectionChanged;
            this.inherited.CollectionChanged += entities_CollectionChanged;

            if (Name.Equals("")) 
            {
                this.GroupId = GroupNumber;
                Name = "[G" + GroupNumber + "]"; //maybe a check that the name is unique
                GroupNumber++; 
            }
            this.Name = Name;

        }

        void entities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (Node n in e.NewItems)
                {
                    if (room.game.ui.sidebar.cbListPicker.Text.Equals(Name))
                    {
                        room.game.ui.sidebar.lstMain.Items.Add(n);
                        room.game.ui.sidebar.SyncTitleNumber(this);
                    }
                    if (parentGroup != null && !parentGroup.entities.Contains(n))
                    {
                        parentGroup.inherited.Add(n);
                    }
                    fullSet.Add(n);
                }
                room.game.ui.sidebar.SyncTitleNumber(this);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (Node n in e.OldItems)
                {
                    if (room.game.ui.sidebar.cbListPicker.Text.Equals(Name))
                    {
                        room.game.ui.sidebar.lstMain.Items.Remove(n);
                        room.game.ui.sidebar.SyncTitleNumber(this);
                    }
                    if (!entities.Contains(n) && !inherited.Contains(n))
                        fullSet.Remove(n);
                }
            }
        }

        public void EmptyGroup()
        {

        }

        public void ForEachFullSet(Action<Node> action)
        {
            fullSet.ToList().ForEach(action);
            /*
            foreach (Node n in fullSet)
            {
                action(n);
            }
            */
        }

        //adds entity to current group and all parent groups
        public void IncludeEntity(Node entity)
        {
            entities.Add(entity);
            if (entity.collision.active)
                room.CollisionSet.Add(entity);

            //if (parentGroup != null)
            //    parentGroup.IncludeEntity(entity);
        }
        //removes entity from current group and all child groups
        public void DiscludeEntity(Node entity)
        {
            if (entity is Node && parentGroup == null) ((Node)entity).active = false;

            entities.Remove(entity);
            inherited.Remove(entity);
            fullSet.Remove(entity);

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

            room.CollisionSet.Remove(entity);
        }

        public Group FindGroup(string name)
        {
            Group root = this;
            while (root.parentGroup != null)
            {
                root = root.parentGroup;
            }
            Group result = root.FindGroupRecurse(name);
            if (result != null) return result;
            return root;
        }

        private Group FindGroupRecurse(string name)
        {
            if (Name.Equals(name)) return this;
            if (childGroups.Count == 0) return null;

            foreach (Group g in childGroups.Values)
            {
                Group result = g.FindGroupRecurse(name);
                if (result != null) return result;
            }
            return null;
        }

        
        public void ForEachAllSets(Action<Node> action)
        {
            entities.ToList().ForEach(action);
            inherited.ToList().ForEach(action);
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
        /*
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
        */
        public void AddGroup(string name, Group group)
        {
            if (childGroups.ContainsKey(name))
            {
                throw new SystemException("Error: One of the childGroups with the same key was already present in this Group.");
            }
            childGroups.Add(name, group);
        }

        public void GroupNamesToList(List<object> list)
        {
            list.Add(Name);
            foreach (Group g in childGroups.Values)
            {
                g.GroupNamesToList(list);
            }
        }

        public void DeleteGroup()
        {
            foreach (Group g in childGroups.Values)
                g.DeleteGroup();

            foreach (Node n in entities.ToList())
            {
                DeleteEntity(n);
            }
            if (parentGroup == null) throw new SystemException("Don't delete orphans");
            parentGroup.childGroups.Remove(Name);
        }

        /*
        public void UpdateComboBox()
        {
            room.game.ui.sidebar.cbListPicker.ItemIndex = 0;
            List<object> list = room.game.ui.sidebar.cbListPicker.Items;
            list.ToList().ForEach((o) => list.Remove(o));

            GroupNamesToList(list);
            list.Add("Other Objects");
        }
        */
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


        public Node findNodeByHash(string value)
        {
            throw new NotImplementedException();
        }
    }
}
