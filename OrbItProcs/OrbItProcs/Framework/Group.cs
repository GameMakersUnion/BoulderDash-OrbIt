using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace OrbItProcs
{
    public enum GroupState { off, drawingOnly, updatingOnly, on };

    public class Group
    {
        public static int GroupNumber = 2;
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

        private string _groupHash = "";
        public string groupHash
        {
            get { return _groupHash; }
            set
            {
                room.groupHashes.Remove(_groupHash);
                _groupHash = value;

                if (room.groupHashes.Contains(value))
                    room.findGroupByHash(value).groupHash =
                        Utils.uniqueString(room.groupHashes);

                room.groupHashes.Add(value);
            }
        }
        
        public int GroupId { get; set; }
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
                    room.nodeHashes.Remove(n.nodeHash);
                    Node newNode = n.CreateClone(true);
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
        private Node _defaultNode = null;
        public Node defaultNode { get { return _defaultNode; } 
            set 
            { 
                _defaultNode = value;
                if (value != null)
                {
                    value.group = this;
                    value.collision.RemoveCollidersFromSet();
                }
            } 
        }
        public Room room;
        private string _Name;
        public string Name { get { return _Name; } set { if (_Name != null && _Name.Equals("master")) return; _Name = value; } } //cannot rename main group
        public bool Spawnable { get; set; }
        public GroupState groupState { get; set; }

        private bool _Disabled = false;
        public bool Disabled
        {
            get { return _Disabled; }
            set
            {
                _Disabled = value;
                if (value)
                {
                    if (parentGroup != null)
                    {
                        foreach (Node n in entities)
                        {
                            if (parentGroup.inherited.Contains(n)) parentGroup.inherited.Remove(n);
                        }
                        foreach (Node n in inherited)
                        {
                            if (parentGroup.inherited.Contains(n)) parentGroup.inherited.Remove(n);
                        }
                    }
                }
                else
                {
                    if (parentGroup != null)
                    {
                        foreach (Node n in entities)
                        {
                            parentGroup.inherited.Add(n);
                        }
                        foreach (Node n in inherited)
                        {
                            parentGroup.inherited.Add(n);
                        }
                    }
                }
            }
        }

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
                    n.group = this;
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
        public Group(Node defaultNode = null, Group parentGroup = null, GroupState groupState = GroupState.on, string Name = "", bool Spawnable = true, ObservableHashSet<Node> entities = null)
        {
            if (parentGroup != null) room = parentGroup.room;
            else  room = Program.getRoom();

            GroupId = -1;
            groupHash = Utils.uniqueString(room.groupHashes);
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
                Name = "Group" + GroupNumber; //maybe a check that the name is unique
                GroupNumber++; 
            }
            this.Name = Name;

            /*threads[0] = new Thread(new ThreadStart(ThreadStartAction));
            threads[1] = new Thread(new ThreadStart(ThreadStartAction));
            threads[2] = new Thread(new ThreadStart(ThreadStartAction));
            threads[3] = new Thread(new ThreadStart(ThreadStartAction));*/
            //threads[0] = new Thread(new ThreadStart(Thread1));
            //threads[1] = new Thread(new ThreadStart(Thread2));
            //threads[2] = new Thread(new ThreadStart(Thread3));

            //threads[4] = new Thread(new ThreadStart(ThreadStartAction));
            //threads[5] = new Thread(new ThreadStart(ThreadStartAction));

            if (parentGroup != null)
            {
                parentGroup.AddGroup(this.Name, this);
            }
        }


        void entities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                bool ui = room.game.ui != null && room.game.ui.sidebar.cbListPicker != null;
                foreach (Node n in e.NewItems)
                {
                    if (ui)
                    {
                        if (room.game.ui.sidebar.cbListPicker.Text.Equals(Name))
                        {
                            room.game.ui.sidebar.lstMain.Items.Add(n);
                        }
                    }
                    if (parentGroup != null && !parentGroup.entities.Contains(n) && !Disabled)
                    {
                        parentGroup.inherited.Add(n);
                    }
                    fullSet.Add(n);
                }
                if (ui) room.game.ui.sidebar.SyncTitleNumber(this);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (Node n in e.OldItems)
                {
                    if (room.game.ui != null)
                    {
                        if (room.game.ui.sidebar.cbListPicker.Text.Equals(Name))
                        {
                            room.game.ui.sidebar.lstMain.Items.Remove(n);
                            room.game.ui.sidebar.SyncTitleNumber(this);
                        }
                    }
                    if (!entities.Contains(n) && !inherited.Contains(n))
                        fullSet.Remove(n);
                    if (parentGroup != null && parentGroup.inherited.Contains(n))
                    {
                        parentGroup.inherited.Remove(n);
                    }
                    if (n.group == this) n.group = null;
                }
            }
        }

        public void EmptyGroup()
        {
            foreach(Node n in fullSet.ToList())
            {
                DeleteEntity(n);
            }
        }

        public void ForEachFullSet(Action<Node> action)
        {
            //fullSet.ToList().ForEach(action);
            foreach(var n in fullSet) // ToList()
            {
                action(n);
            }
        }

        


        //adds entity to current group and all parent groups
        public void IncludeEntity(Node entity)
        {
            entities.Add(entity);
            entity.group = this;
            if (entity.collision.active)
            {
                //room.CollisionSet.Add(entity);
                entity.collision.UpdateCollisionSet();
            }
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
            Group root = this;
            while (root.parentGroup != null)
            {
                root = root.parentGroup;
            }
            root.DiscludeEntity(entity);
            entity.collision.RemoveCollidersFromSet();
            //room.CollisionSet.Remove(entity);
            entity.OnDelete();
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

        public void GroupNamesToList(List<object> list, bool addSelf = true)
        {
            if (addSelf) list.Add(Name);
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
                n.group = null;
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


        public Node FindNodeByHash(string value)
        {
            foreach(Node n in fullSet)
            {
                if (n.nodeHash.Equals(value)) return n;
            }
            return null;
        }

        public void FindNodeByHashes(ObservableHashSet<string> hashes, ObservableHashSet<Node> nodeSet)
        {
            ObservableHashSet<string> alreadyFound = new ObservableHashSet<string>();
            foreach(Node n in fullSet)
            {
                if (hashes.Contains(n.nodeHash) && !alreadyFound.Contains(n.nodeHash))
                {
                    nodeSet.Add(n);
                    alreadyFound.Add(n.nodeHash);
                }
            }
            Console.WriteLine("hashes:{0}  |  alreadyfound:{1}", hashes.Count, alreadyFound.Count);
        }
    }
}

//Thread experiment=============================

//private int counter = 0;
//private static int ThreadCount = 3;
//private static bool ThreadStarted = false;
//public Node[] nodes;
//Thread[] threads = new Thread[ThreadCount];
///*Thread t1;
//Thread t2;
//Thread t3;
//Thread t4;
//Thread t5;
//Thread t6;*/

/*public void ForEachThreading(GameTime gameTime)
{
    nodes = fullSet.ToArray();

    / *foreach(Node n in nodes)
    {
        if (n.active)
        {
            n.Update(gameTime);
        }
    }
    return;* /

    //Thread t1 = new Thread(new ThreadStart(ThreadStartAction));
    //Thread t2 = new Thread(new ThreadStart(ThreadStartAction));
    //Thread t3 = new Thread(new ThreadStart(ThreadStartAction));
    //Thread t4 = new Thread(new ThreadStart(ThreadStartAction));
    //Thread t5 = new Thread(new ThreadStart(ThreadStartAction));
    //Thread t6 = new Thread(new ThreadStart(ThreadStartAction));
    //Thread t7 = new Thread(new ThreadStart(ThreadStartAction));
    //Thread t8 = new Thread(new ThreadStart(ThreadStartAction));

            
    / *for (int i = 0; i < ThreadCount; i++)
    {
        threads[i] = new Thread(new ThreadStart(ThreadStartAction));
    }* /

    / *if (!ThreadStarted)
    {
        for (int i = 0; i < ThreadCount; i++)
        {
            threads[i].Start();
        }
        ThreadStarted = true;
    }
    else
    {
        for (int i = 0; i < ThreadCount; i++)
        {
            Thread.Sleep(1);
            //threads[i].
        }
    }* /

    if (!ThreadStarted)
    {

        threads[0].Start();
        threads[1].Start();
        threads[2].Start();

        ThreadStarted = true;
    }

    / *for (int i = 0; i < ThreadCount; i++)
    {
        threads[i].Join();
    }* /

}


public void Thread1()
{
    for(int i = 0; i < 100; i++)
    {
        Console.WriteLine("{0} : {1}", 1, i);
    }
}
public void Thread2()
{
    for (int i = 0; i < 100; i++)
    {
        Console.WriteLine("{0} : {1}", 2, i);
    }
}
public void Thread3()
{
    for (int i = 0; i < 100; i++)
    {
        Console.WriteLine("{0} : {1}", 3, i);
    }
}

public void ThreadStartAction()
{
    int min = (nodes.Length / ThreadCount) * counter;
    int max = (nodes.Length / ThreadCount + 1) * counter;

    counter++;
    //for (int i = min; i <= max - (nodes.Length % ThreadCount) - 1; i++)
    //for (int i = min; i <= max - (nodes.Length % ThreadCount) - 1; i++)
    while(true)
    {
        Console.WriteLine("COUNT1:" + counter);
        if (nodes[counter - 1].active)
        {
            Console.WriteLine("COUNT2:" + counter);
            lock (nodes[counter - 1])
            {
                Console.WriteLine("COUNT3:" + counter);
                nodes[counter - 1].Update(Game1.GlobalGameTime);
            }
        }
    }
    //counter++; 
}*/