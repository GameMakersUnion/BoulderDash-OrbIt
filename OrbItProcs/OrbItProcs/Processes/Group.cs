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
        public ObservableCollection<object> entities { get; set; }
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
        public Group(Node defaultNode = null, ObservableCollection<object> entities = null, Group parentGroup = null, GroupState groupState = GroupState.on, string Name = "newlist")
        {
            room = Program.getRoom();
            this.defaultNode = defaultNode ?? room.defaultNode;
            this.entities = entities ?? new ObservableCollection<object>();
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
                        room.game.ui.sidebar.lstMain.Items.Add(n);
                    if (parentGroup != null && !parentGroup.entities.Contains(n))
                    {
                        parentGroup.entities.Add(n);
                        
                    }
                }
                room.game.ui.sidebar.SyncTitleNumber(this);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (Node n in e.OldItems)
                {
                    if (room.game.ui.sidebar.cmbListPicker.Text.Equals(Name)) 
                        room.game.ui.sidebar.lstMain.Items.Remove(n);
                    if (parentGroup != null && parentGroup.entities.Contains(n))
                    {
                        n.active = false;
                        parentGroup.entities.Remove(n);
                    }

                    RemoveFromChildrenDeep(n);
                }
                room.game.ui.sidebar.SyncTitleNumber(this);
            }
        }

        public void RemoveFromChildrenDeep(object toremove)
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
                entities.ToList().ForEach(delegate(object n) { ((Node)n).Update(gametime); });
            }
        }

        public void Draw(SpriteBatch spritebatch)
        {
            if (groupState.In(GroupState.on, GroupState.drawingOnly))
            {
                entities.ToList().ForEach(delegate(object n) { ((Node)n).Draw(spritebatch); });
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
            HashSet<object> hashset = new HashSet<object>();
            dict.Keys.ToList().ForEach(delegate(string key) {
                Group g = dict[key];

                g.entities.ToList().ForEach(delegate(object o) { 
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
