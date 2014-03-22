using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.CSharp;
using System.Reflection;



using System.Collections.Specialized;
using System.Collections;



namespace OrbItProcs {

    public enum nodeE {
        active,
        position,
        velocity,
        radius,
        mass,
        scale,
        texture,
        name,
        lifetime,
        color,
    };

    public enum state
    {
        off,
        updateOnly,
        drawOnly,
        on,
    }
    public delegate void CollisionDelegate(Node source, Node target);

    public class DataStore : Dictionary<string, dynamic>
    {
        public DataStore() : base() { }

        //public DataStore (params Tuple<string, dynamic>[] items) : base(items.Length)
        //{
        //    foreach(var item in items)
        //    {
        //        this[item.Item1] = item.Item2;
        //    }
        //}
            }

    public class Node {
        private string _nodeHash = "";
        public string nodeHash { get { return _nodeHash; } set 
        {
            room.nodeHashes.Remove(_nodeHash);
            _nodeHash = value;
            room.nodeHashes.Add(value);
        } }

        public static int nodeCounter = 0;

        public T GetComponent<T>() //todo: make Component dictionary (not dynamic) to see if casting to (T) is faster than dynamic (probably casts anyway)
        {
            return comps[Utils.compEnums[typeof(T)]];
        }
        public bool HasComponent(comp component)
        {
            return comps.ContainsKey(component);
        }
        public bool HasActiveComponent(comp component)
        {
            return comps.ContainsKey(component) && comps[component].active;
        }
        [Info(UserLevel.Never)]
        public dynamic this[comp component]
        {
            get
            {
                return comps[component];
            }
            set
            {
                comps[component] = value;
            }
        }

        private bool triggerSortComponentsUpdate = false, triggerSortComponentsDraw = false, triggerRemoveComponent = false;
        private Dictionary<comp, bool> tempCompActiveValues = new Dictionary<comp, bool>();

        private state _nodeState = state.on;
        public state nodeState { get { return _nodeState; } set { _nodeState = value; } }

        private bool _active = true;
        public bool active
        {
            get { return _active; }
            set
            {
                if (_active && !value)
                {
                    foreach (comp c in comps.Keys.ToList())
                    {
                        if (comps[c] is Component)
                        {
                            tempCompActiveValues[c] = comps[c].active;
                            comps[c].active = false;
                        }
                    }
                    collision.RemoveCollidersFromSet();
                }
                else if (!_active && value)
                {
                    foreach (comp c in comps.Keys.ToList())
                    {
                        if (comps[c] is Component)
                        {
                            if (tempCompActiveValues.ContainsKey(c)) comps[c].active = tempCompActiveValues[c];
                            else comps[c].active = true;
                        }
                    }
                    if (collision != null) collision.UpdateCollisionSet();
                }
                _active = value;
            }
        }
        ///<summary>
        ///Warning: Do not call in update Loop. BE CAREFUL
        ///</summary>
        public IEnumerable<Collider> GetColliders()
        {
            foreach(Collider c in collision.colliders.Values)
            {
                yield return c;
            }
            yield return body;
        }

        private bool _IsDeleted = false;
        public bool IsDeleted
        {
            get { return _IsDeleted; }
            set
            {
                if (!_IsDeleted && value)
                {
                    OnDelete();
                }
                _IsDeleted = value;
            }
        }



        public bool IsDefault = false;

        public int lifetime = -1;

        private string _name = "node";
        public string name { get { return _name; } set { _name = value; } }

        private int _sentinel = -10;
        public int sentinelp { get { return _sentinel; } set { _sentinel = value; } }

        public Room room = Program.getRoom();

        public Dictionary<comp, dynamic> _comps = new Dictionary<comp, dynamic>();
        public Dictionary<comp, dynamic> comps { get { return _comps; } set { _comps = value; } }

        public List<comp> aOtherProps = new List<comp>();
        public List<comp> aSelfProps = new List<comp>();
        public List<comp> drawProps = new List<comp>();

        public List<comp> compsToRemove = new List<comp>();
        public List<comp> compsToAdd = new List<comp>();

        private HashSet<string> _tags = new HashSet<string>();
        public HashSet<string> tags { get { return _tags; } set { _tags = value; } }

        private Body _body;
        public Body body
        {
            get { return _body; }
            set
            {
                _body = value;
            } 
        }

        private Movement _movement;
        public Movement movement
        {
            get { return _movement; }
            set
            {
                _movement = value;
                if (comps != null && value != null)
                {
                    if (comps.ContainsKey(comp.movement))
                    {
                        comps.Remove(comp.movement);
                    }
                    comps.Add(comp.movement, value);
                }
            }
        }

        private Collision _collision;
        public Collision collision
        {
            get { return _collision; }
            set
            {
                _collision = value;
                if (comps != null && value != null)
                {
                    if (comps.ContainsKey(comp.collision))
                    {
                        comps.Remove(comp.collision);
                    }
                    comps.Add(comp.collision, value);
                }
            }
        }

        public textures texture
        {
            get { return body.texture; }
            set { body.texture = value; }
        }

        private ObservableHashSet<Link> _SourceLinks = new ObservableHashSet<Link>();
        [Polenter.Serialization.ExcludeFromSerialization]
        public ObservableHashSet<Link> SourceLinks { get { return _SourceLinks; } set { _SourceLinks = value; } }

        private ObservableHashSet<Link> _TargetLinks = new ObservableHashSet<Link>();
        [Polenter.Serialization.ExcludeFromSerialization]
        public ObservableHashSet<Link> TargetLinks { get { return _TargetLinks; } set { _TargetLinks = value; } }
        [Polenter.Serialization.ExcludeFromSerialization]
        public ObservableHashSet<Group> Groups { get; set; }

        [Polenter.Serialization.ExcludeFromSerialization]
        [Info(UserLevel.Never)]
        public Delegator delegator
        {
            get
            {
                if (!comps.ContainsKey(comp.delegator))
                {
                    addComponent(comp.delegator, true);
                }
                return comps[comp.delegator];
            }
            set
            {
                comps[comp.delegator] = value;
            }
        }
        [Polenter.Serialization.ExcludeFromSerialization]
        [Info(UserLevel.Never)]
        public Scheduler scheduler
        {
            get
            {
                if (!comps.ContainsKey(comp.scheduler))
                {
                    addComponent(comp.scheduler, true);
                }
                return comps[comp.scheduler];
            }
            set
            {
                comps[comp.scheduler] = value;
            }
        }


        public bool DebugFlag { get; set; }

        public bool PolenterHack
        {
            get { return true; }
            set 
            {
                foreach (comp c in comps.Keys.ToList())
                {
                    ((Component)comps[c]).parent = this;
                }
                body.parent = this;
                collision.parent = this;
                movement.parent = this;
                body.shape.body = body;
            }
        }

        [Info(UserLevel.Never)]
        public DataStore Kawasaki = new DataStore();

        public event EventHandler OnAffectOthers;

        public void storeInInstance(nodeE val, Dictionary<dynamic,dynamic> dict)
        {
            if (val == nodeE.active)             active                      = dict[val];
            if (val == nodeE.position)           body.pos          = dict[val];
            if (val == nodeE.velocity)           body.velocity          = dict[val];
            if (val == nodeE.radius)             body.radius            = dict[val];
            if (val == nodeE.mass)               body.mass              = dict[val];
            if (val == nodeE.scale)              body.scale             = dict[val];
            if (val == nodeE.texture)            body.texture           = dict[val];
            if (val == nodeE.name)               name                        = dict[val];
            if (val == nodeE.lifetime)           lifetime                    = dict[val];
            if (val == nodeE.color)              body.color             = dict[val];
        }

        public Node() : this(ShapeType.eCircle) { }

        public Node(bool createHash) : this(ShapeType.eCircle, createHash) { }

        public Node(ShapeType shapetype, bool createHash = true)
        {
            room = Program.getRoom();
            if (createHash)
            {
                nodeHash = Utils.uniqueString(room.nodeHashes);
            }
            Groups = new ObservableHashSet<Group>();
            nodeCounter++;
            movement = new Movement(this);
            collision = new Collision(this);
            Shape shape;
            if (shapetype == ShapeType.eCircle)
            {
                shape = new Circle(25);
            }
            else if (shapetype == ShapeType.ePolygon)
            {
                shape = new Polygon();
            }
            else
            {
                shape = new Circle(25); //in case there are more shapes
            }

            body = new Body(shape: shape, parent: this);
            name = "blankname";

            //comps.Add(comp.body, body);
            //comps.Add(comp.movement, movement);
            //comps.Add(comp.collision, collision);
            
        }

        public Node(Dictionary<dynamic, dynamic> userProps, ShapeType shapetype = ShapeType.eCircle, bool createHash = true)
            : this(shapetype, createHash)
        {
            // add the userProps to the props
            foreach (dynamic p in userProps.Keys)
            {
                // if the key is a comp type, we need to add the component to comps dict
                if (p is comp)
                {
                    fetchComponent(p, userProps[p]);
                }
                // if the key is a node type, we need to update the instance variable value
                else if (p is nodeE)
                    storeInInstance(p, userProps);
            }
            SortComponentLists();
            //room = room1;
            //room = Program.getRoom();
            //if (room == null) Console.WriteLine("null");
            //if (Program.getGame() == null) Console.WriteLine("gnull");
            if (lifetime > 0) name = "temp|" + name + Guid.NewGuid().GetHashCode().ToString().Substring(0,5);
            else name = name + nodeCounter;
        }

        public T CheckData<T>(string key)
        {
            if (Kawasaki.ContainsKey(key))
            {
                return Kawasaki[key];
            }
            else
            {
                return default(T);
            }
        }

        public void SetData(string key, dynamic data)
        {
            Kawasaki[key] = data;
        }

        public void AddTag(string tag)
        {
            tags.Add(tag);
        }
        public void RemoveTag(string tag)
        {
            tags.Remove(tag);
        }
        public virtual void Update(GameTime gametime)
        {
            //collision.ClearCollisionList();
            collision.ClearCollisionLists();
            if (nodeState == state.off || nodeState == state.drawOnly) return;

            if (aOtherProps.Count > 0)
            {
                List<Collider> returnObjectsFinal = new List<Collider>();

                int reach; //update later based on cell size and radius (or polygon size.. maybe based on it's AABB)
                if (body.shape is Polygon)
                {
                    reach = 20;
                }
                else
                {
                    //reach = (int)(body.radius * 5) / room.gridsystem.cellWidth;
                    reach = 10;
                }

                ///*
                returnObjectsFinal = room.gridsystem.retrieve(body, reach);
                //int cellReach = (int)(body.radius * 2) / room.gridsystem.cellWidth * 2;
                if (comps.ContainsKey(comp.flow) && comps[comp.flow].active)
                {
                    returnObjectsFinal = new List<Collider>();
                    if (comps[comp.flow].activated)
                    {
                        returnObjectsFinal = comps[comp.flow].outgoing.ToList();
                    }
                }
                returnObjectsFinal.Remove(body);

                foreach (Collider other in returnObjectsFinal)
                {
                    if (other.parent.active)
                    {
                        foreach (comp c in aOtherProps)
                        {
                            if (!comps[c].active) continue;
                            comps[c].AffectOther(other.parent);
                        }
                    }
                }
                //*/
                /*
                var buckets = room.gridsystem.retrieveBuckets(this, 115);
                if (buckets != null)
                {
                    foreach (var bucket in buckets)
                    {
                        foreach (var nn in bucket)
                        {
                            if (nn.active)
                            {
                                foreach (comp c in aOtherProps)
                                {
                                    comps[c].AffectOther(nn);
                                }
                            }
                        }
                    }
                }
                //*/

            }

            

            if (OnAffectOthers != null) OnAffectOthers.Invoke(this, null);

            foreach (comp c in aSelfProps)
            {
                comps[c].AffectSelf();
            }

            if (movement.active) movement.AffectSelf(); //temporary until make movement list to update at the correct time

            if (triggerSortComponentsUpdate)
            {
                SortComponentListsUpdate();
                triggerSortComponentsUpdate = false;
            }

            if (triggerRemoveComponent)
            {
                RemoveComponentTriggered();
            }

        }

        public void Draw(SpriteBatch spritebatch)
        {
            if (nodeState == state.off || nodeState == state.updateOnly) return;

            foreach (comp c in drawProps)
            {
                if (!comps[c].CallDraw) continue;
                if (!comps[c].active) continue;
                comps[c].Draw(spritebatch);
                //if (!comps[c].compType.HasFlag(mtypes.minordraw))
                //    break; //only executes the most significant draw component
            }

            if (triggerSortComponentsDraw)
            {
                SortComponentListsDraw();
                triggerSortComponentsDraw = false;
            }

            if (triggerRemoveComponent)
            {
                RemoveComponentTriggered();
            }
        }

        public override string ToString()
        {
            //return base.ToString();
            string ret = name;
            if (IsDefault) ret += "(DEF)";
            return ret;
        }

        public void setCompActive(comp c, bool Active)
        {
            if (comps.ContainsKey(c))
            {
                comps[c].active = Active;
            }
            else
            {
                Console.WriteLine("Component not found in dictionary");
            }
        }

        //assuming caller knows that c is contained in comps (to prevent a very frequent comparison) 
        //(probably called from a foreach of comps.keys anyway)
        public bool isCompActive(comp c)
        {
            return comps[c].active;
        }

        //lists will be sorted once at a safe place, and then these will be set to false;
        public void triggerSortLists()
        {
            triggerSortComponentsUpdate = true;
            triggerSortComponentsDraw = true;
        }

        public void acceptUserProps(Dictionary<dynamic, dynamic> userProps)
        {
            foreach (dynamic p in userProps.Keys)
            {
                // if the key is a node type, (and not a bool) we need to update the instance variable value
                if (p is nodeE)// && !(userProps[p] is bool))
                    storeInInstance(p, userProps);
                // if the key is a comp type, we need to add the component to comps dict
                if (p is comp)
                {
                    fetchComponent(p, userProps[p]);
                    if (comps.ContainsKey(p)) comps[p].active = userProps[p];
                }
            }
            SortComponentLists();
        }

        public void addComponent(comp c, bool active, bool overwrite = false)
        {
            bool fetch = fetchComponent(c, active, overwrite);
            if (fetch) SortComponentLists();
        }

        public void addComponentSafe(comp c)
        {
            if (comps.ContainsKey(c)) { Console.WriteLine("AddComponentSafe didn't perform: key already exists."); return; }

            compsToAdd.Add(c);
            triggerRemoveComponent = true;

        }

        public void addComponent(Component component, bool active)
        {
            component.parent = this;
            comps.Add(component.com, component);

            component.Initialize(this);
            SortComponentLists();
        }



        public bool fetchComponent(comp c, bool active, bool overwrite = false)
        {
            if (c == comp.movement)
            {
                movement.active = active;
                return false;
            }
            else if (c == comp.collision)
            {
                collision.active = active;
                return false;
            }
            
            if (overwrite)
            {
                Component component = MakeComponent(c, active, this);
                if (comps.ContainsKey(c))
                {
                    comps.Remove(c);
                }
                comps.Add(c, component);
            }
            else
            {
                if (!comps.ContainsKey(c))
                {
                    Component component = MakeComponent(c, active, this);
                    comps.Add(c, component);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public Component MakeComponent(comp c, bool active, Node parent)
        {
            Component component;

            component = Component.GenerateComponent(c);
            component.parent = this;
            component.active = active;
            component.AfterCloning();

            return component;
        }

        public void RemoveComponent(comp c)
        {
            if (!comps.ContainsKey(c))
            {
                //Console.WriteLine("Component already removed or doesn't exist.");
                return;
            }
            comps[c].active = false;
            compsToRemove.Add(c);
            if (!room.masterGroup.entities.Contains(this))
            {
                SortComponentLists();
                RemoveComponentTriggered();
            }
            else
            {
                triggerSortLists();
                triggerRemoveComponent = true;
            }
        }

        public void RemoveComponentTriggered()
        {
            List<comp> toremove = new List<comp>();
            List<comp> toaddremove = new List<comp>();
            foreach (comp c in compsToRemove)
            {
                if (comps.ContainsKey(c))
                {
                    if (!drawProps.Contains(c) && !aSelfProps.Contains(c) && !aOtherProps.Contains(c))
                    {
                        //we should call a 'destroy component' method here, instead of just hoping it gets garabage collected
                        comps.Remove(c);
                        toremove.Add(c);
                        triggerRemoveComponent = false;
                        triggerSortLists();
                    }
                    else
                    {
                        triggerSortLists();
                    }
                }
            }
            foreach (comp c in compsToAdd)
            {
                if (comps.ContainsKey(c)) continue;

                addComponent(c, true);
                toaddremove.Add(c);
            }
            int cc = toremove.Count;
            for (int i = 0; i < cc; i++)
            {
                compsToRemove.Remove(toremove.ElementAt(0));
            }
            cc = toaddremove.Count;
            for (int i = 0; i < cc; i++)
            {
                compsToAdd.Remove(toaddremove.ElementAt(0));
            }
        }

        public void SortComponentLists()
        {
            SortComponentListsUpdate();
            SortComponentListsDraw();
        }

        public void SortComponentListsUpdate()
        {
            aOtherProps = new List<comp>();
            aSelfProps = new List<comp>();

            var clist = comps.Keys.ToList();
            clist.Sort();

            foreach (comp c in clist)
            {
                if (c == comp.movement || c == comp.collision) continue;
                if (comps.ContainsKey(c) && isCompActive(c) && ((comps[c].compType & mtypes.affectother) == mtypes.affectother))
                {
                    aOtherProps.Add(c);
                }
            }
            foreach (comp c in clist)
            {
                if (c == comp.movement) continue;
                if (comps.ContainsKey(c) && isCompActive(c) && ((comps[c].compType & mtypes.affectself) == mtypes.affectself))
                {
                    aSelfProps.Add(c);
                }
            }
        }

        public void SortComponentListsDraw()
        {
            drawProps = new List<comp>();

            var clist = comps.Keys.ToList();
            clist.Sort();

            foreach (comp c in clist)
            {
                if (comps.ContainsKey(c) && isCompActive(c) && ((comps[c].compType & mtypes.minordraw) == mtypes.minordraw))
                {
                    drawProps.Add(c);
                }
            }
            foreach (comp c in clist)
            {
                if (comps.ContainsKey(c) && isCompActive(c) && ((comps[c].compType & mtypes.draw) == mtypes.draw))
                {
                    drawProps.Add(c);
                }
            }
        }
        public Texture2D getTexture()
        {
            return room.game.textureDict[body.texture];
        }
        public Texture2D getTexture(textures t)
        {
            return room.game.textureDict[t];
        }
        public Vector2 TextureCenter()
        { 
            Texture2D tx = room.game.textureDict[body.texture];
            return new Vector2(tx.Width / 2f, tx.Height / 2f); // TODO: maybe cast to floats to make sure it's the exact center.
        }
        
        

        public void changeRadius(float newRadius)
        {
            body.radius = newRadius;
            body.scale = body.radius / (getTexture().Width / 2);
        }
        public float diameter()
        {
            return body.radius * 2;
        }

        public void OnSpawn()
        {
            foreach (comp key in comps.Keys.ToList())
            {
                Component component = comps[key];
                MethodInfo mInfo = component.GetType().GetMethod("OnSpawn");
                if (mInfo != null
                    && mInfo.DeclaringType == component.GetType())
                {
                    component.OnSpawn();
                }
            }
        }

        public void OnDelete()
        {
            //Console.WriteLine("{0} - deleting:      {1}", name, nodeHash);
            //if (room.nodeHashes.Contains(nodeHash)) Console.WriteLine("Total Hashes:{0} , removed hash:{1}", room.nodeHashes.Count, _nodeHash);
            //Console.WriteLine("---------------");
            room.nodeHashes.Remove(nodeHash);

            active = false;
            if (this == room.targetNode) room.targetNode = null;
            if (this == room.game.ui.sidebar.inspectorArea.editNode) room.game.ui.sidebar.inspectorArea.editNode = null; //todo: social design pattern
            if (this == room.game.ui.spawnerNode) room.game.ui.spawnerNode = null;
            if (room.masterGroup != null && room.masterGroup.fullSet.Contains(this))
            {
                room.masterGroup.DeleteEntity(this);
            }
        }

        public Node CreateClone(bool CloneHash = false)
        {
            Node newNode = new Node(!CloneHash);
            cloneNode(this, newNode, CloneHash);
            return newNode;
        }

        public static void cloneNode(Node sourceNode, Node destNode, bool CloneHash = false) //they must be the same type
        {
            //dynamic returnval;
            List<FieldInfo> fields = sourceNode.GetType().GetFields().ToList();
            fields.AddRange(sourceNode.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).ToList());
            List<PropertyInfo> properties = sourceNode.GetType().GetProperties().ToList();
            /*
            foreach (PropertyInfo property in properties)
            {
                //if (property.Name.Equals("compsProp")) continue;
                property.SetValue(destNode, property.GetValue(sourceNode, null), null);
            
            }
            //*/
            //do not copy parent field
            foreach (FieldInfo field in fields)
            {
                if (field.Name.Equals("_nodeHash"))
                {
                    if (!CloneHash)
                    {
                        continue;
                    }
                }
                if (field.Name.Equals("_comps"))
                {
                    Dictionary<comp, dynamic> dict = sourceNode.comps;
                    foreach (comp key in dict.Keys)
                    {
                        if (key == comp.collision || key == comp.movement) continue;
                        destNode.addComponent(key, sourceNode.comps[key].active);
                        Component.CloneComponent(dict[key], destNode.comps[key]);
                        destNode.comps[key].Initialize(destNode);
                    }
                    foreach (comp key in destNode.comps.Keys.ToList())
                    {
                        if (key == comp.collision || key == comp.movement) continue;
                        Component component = destNode.comps[key];
                        MethodInfo mInfo = component.GetType().GetMethod("AfterCloning");
                        if (mInfo != null
                            && mInfo.DeclaringType == component.GetType())
                        {
                            component.AfterCloning();
                        }

                    }
                }
                else if ((field.FieldType == typeof(int))
                   || (field.FieldType == typeof(Single))
                   || (field.FieldType == typeof(bool))
                   || (field.FieldType == typeof(string)))
                {
                    if (!field.Name.Equals("IsDefault"))
                        field.SetValue(destNode, field.GetValue(sourceNode));
                }
                else if (field.FieldType == typeof(Vector2))
                {
                    Vector2 vect = (Vector2)field.GetValue(sourceNode);
                    Vector2 newvect = new Vector2(vect.X, vect.Y);
                    field.SetValue(destNode, newvect);
                }
                else if (field.FieldType == typeof(Color))
                {
                    Color col = (Color)field.GetValue(sourceNode);
                    Color newcol = new Color(col.R, col.G, col.B, col.A);
                    field.SetValue(destNode, newcol);
                }
                else if (field.FieldType == (typeof(Collision)))
                {
                    Component.CloneComponent(sourceNode.collision, destNode.collision);
                    destNode.collision.parent = destNode;
                    destNode.collision.AfterCloning();
                }
                else if (field.FieldType == (typeof(Movement)))
                {
                    Component.CloneComponent(sourceNode.movement, destNode.movement);
                    destNode.movement.parent = destNode;
                    destNode.movement.AfterCloning();
                }
                else if (field.FieldType == (typeof(Body)))
                {
                    //Component.CloneComponent(sourceNode.body, destNode.body);

                    Component.CloneObject(sourceNode.body, destNode.body);
                    destNode.body.parent = destNode;
                    destNode.body.shape.body = destNode.body;
                    destNode.body.AfterCloning();
                }
                else if (field.FieldType == typeof(Room))
                {
                    field.SetValue(destNode, field.GetValue(sourceNode));
                }
            }
        }
    }
}