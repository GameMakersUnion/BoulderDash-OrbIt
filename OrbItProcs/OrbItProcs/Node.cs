﻿using System;
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
    }

    public class Node {
        public static int nodeCounter = 0;
        private Vector2 tempPosition = new Vector2(0, 0);
        private string _nodeHash = "";
        public List<string> nodeHistory = new List<string>();
        public string nodeHash { get { return _nodeHash; } set 
        {
            room.nodeHashes.Remove(_nodeHash);
            _nodeHash = value;
            room.nodeHashes.Add(value);
        } }

        private bool triggerSortComponentsUpdate = false, triggerSortComponentsDraw = false, triggerRemoveComponent = false;
        private Dictionary<Type, bool> tempCompActiveValues = new Dictionary<Type, bool>();

        private state _nodeState = state.on;
        public state nodeState { get { return _nodeState; } set { _nodeState = value; } }
        public bool IsAI { get; set; }

        private bool _active = true;
        public bool active
        {
            get { return _active; }
            set
            {
                if (_active && !value)
                {
                    foreach (Type t in comps.Keys.ToList())
                    {
                        tempCompActiveValues[t] = comps[t].active;
                        comps[t].active = false;
                    }
                    collision.RemoveCollidersFromSet();
                }
                else if (!_active && value)
                {
                    foreach (Type t in comps.Keys.ToList())
                    {
                        if (tempCompActiveValues.ContainsKey(t)) comps[t].active = tempCompActiveValues[t];
                        else comps[t].active = true;
                    }
                    if (collision != null) collision.UpdateCollisionSet();
                }
                _active = value;
            }
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
        //public bool IsDefault = false;
        //public int lifetime = -1;

        private string _name = "node";
        public string name { get { return _name; } 
            set 
            { 
                _name = value;
                if (value.Equals("Group115"))
                    Console.WriteLine("Group115");
            } 
        }
        public Room room;

        private Dictionary<Type, Component> _comps = new Dictionary<Type, Component>();
        public Dictionary<Type, Component> comps { get { return _comps; } set { _comps = value; } }

        private List<Type> aOtherProps = new List<Type>();
        private List<Type> aSelfProps = new List<Type>();
        private List<Type> drawProps = new List<Type>();
        private List<Type> playerProps = new List<Type>();
        private List<Type> aiProps = new List<Type>();
        private List<Type> compsToRemove = new List<Type>();
        private List<Type> compsToAdd = new List<Type>();

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
                    if (HasComp<Movement>())
                    {
                        comps.Remove(typeof(Movement));
                    }
                    comps.Add(typeof(Movement), value);
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
                    if (HasComp<Collision>())
                    {
                        comps.Remove(typeof(Collision));
                    }
                    comps.Add(typeof(Collision), value);
                }
            }
        }
        private BasicDraw _basicdraw;
        public BasicDraw basicdraw
        {
            get { return _basicdraw; }
            set
            {
                _basicdraw = value;
                if (comps != null && value != null)
                {
                    if (HasComp<BasicDraw>())
                    {
                        comps.Remove(typeof(BasicDraw));
                    }
                    comps.Add(typeof(BasicDraw), value);
                }
            }
        }
        private Meta _meta;
        public Meta meta
        {
            get { return _meta; }
            set
            {
                _meta = value;
                if (comps != null && value != null)
                {
                    if (HasComp<Meta>())
                    {
                        comps.Remove(typeof(Meta));
                    }
                    comps.Add(typeof(Meta), value);
                }
            }
        }
        public textures texture
        {
            get { return body.texture; }
            set { body.texture = value; }
        }
        private Player _player;
        public Player player { get { return _player; } set { _player = value; if (value != null) SortComponentListsUpdate(); } }
        [Info(UserLevel.Never)]
        public bool IsPlayer { get { return player != null; } }

        private ObservableHashSet<Link> _SourceLinks = new ObservableHashSet<Link>();
        [Polenter.Serialization.ExcludeFromSerialization]
        public ObservableHashSet<Link> SourceLinks { get { return _SourceLinks; } set { _SourceLinks = value; } }
        private ObservableHashSet<Link> _TargetLinks = new ObservableHashSet<Link>();
        [Polenter.Serialization.ExcludeFromSerialization]
        public ObservableHashSet<Link> TargetLinks { get { return _TargetLinks; } set { _TargetLinks = value; } }
        [Polenter.Serialization.ExcludeFromSerialization]
        public Group group
        {
            get { return _group; }
            set
            {
                _group = value;
            }
        }
        private Group _group;

        [Polenter.Serialization.ExcludeFromSerialization]
        [Info(UserLevel.Never)]
        public Delegator delegator
        {
            get
            {
                if (!HasComp<Delegator>())
                {
                    addComponent(comp.delegator, true);
                }
                return Comp<Delegator>();
            }
            set
            {
                comps[typeof(Delegator)] = value;
            }
        }
        [Polenter.Serialization.ExcludeFromSerialization]
        [Info(UserLevel.Never)]
        public Scheduler scheduler
        {
            get
            {
                if (!HasComp<Scheduler>())
                {
                    addComponent(comp.scheduler, true);
                }
                return Comp<Scheduler>();
            }
            set
            {
                comps[typeof(Scheduler)] = value;
            }
        }

        public bool IsDefault
        {
            get
            {
                if (group != null && this == group.defaultNode) return true;
                return false;
            }
        }

        public bool DebugFlag { get; set; }

        public bool PolenterHack
        {
            get { return true; }
            set 
            {
                foreach (Type c in comps.Keys.ToList())
                {
                    comps[c].parent = this;
                }
                body.parent = this;
                collision.parent = this;
                movement.parent = this;
                body.shape.body = body;
            }
        }

        [Info(UserLevel.Never)]
        public DataStore dataStore = new DataStore();

        public event EventHandler OnAffectOthers;

        public void storeInInstance(nodeE val, Dictionary<dynamic,dynamic> dict)
        {
            if (val == nodeE.active) active = dict[val];
            if (val == nodeE.name) name = dict[val];
            if (val == nodeE.position) body.pos = dict[val];
            if (val == nodeE.velocity) body.velocity = dict[val];
            if (val == nodeE.radius) body.radius = dict[val];
            if (val == nodeE.mass) body.mass = dict[val];
            if (val == nodeE.scale) body.scale = dict[val];
            if (val == nodeE.texture) body.texture = dict[val];
            if (val == nodeE.color) body.color = dict[val];
        }

        public Node() : this(null,ShapeType.eCircle) { }

        public Node(Room room, bool createHash = true) : this(room, ShapeType.eCircle, createHash) { }

        public Node(Room room, ShapeType shapetype, bool createHash = true)
        {
            this.room = room;
            //if (room == null) throw new NotImplementedException
            //    ("Polenter is benched for now. Everyone else must use the Parameterized constructor and pass a room reference.");
            //if (lifetime > 0) name = "temp|" + name + Guid.NewGuid().GetHashCode().ToString().Substring(0, 5);
            name = name + nodeCounter;
            if (createHash)
            {
                nodeHash = Utils.uniqueString(room.nodeHashes);
            }
            nodeCounter++;
            meta = new Meta(this);
            movement = new Movement(this);
            
            Shape shape;
            if (shapetype == ShapeType.eCircle)
            {
                shape = new Circle(15);
            }
            else if (shapetype == ShapeType.ePolygon)
            {
                shape = new Polygon();
            }
            else
            {
                shape = new Circle(15); //in case there are more shapes
            }

            body = new Body(shape: shape, parent: this);
            body.radius = 15;
            collision = new Collision(this);
            basicdraw = new BasicDraw(this);
            movement.active = true; collision.active = true; basicdraw.active = true;
            IsAI = false;
            //name = "blankname";

            //comps.Add(comp.body, body);
            //comps.Add(comp.movement, movement);
            //comps.Add(comp.collision, collision);

            //room.gridsystemAffect.retrieveOffsetArrays()
            affectAction = (source, other) =>
            {
                //todo: extend to check for every component for finer control if necessary
                if (source.parent.ExclusionCheck != null && source.parent.ExclusionCheck(other.parent)) return;
                foreach (Type t in source.parent.aOtherProps)
                {
                    if (!source.parent.comps[t].active) continue;
                    source.parent.comps[t].AffectOther(other.parent);
                }
            };
        }
        Action<Collider, Collider> affectAction;
        public Node(Room room, Dictionary<dynamic, dynamic> userProps, ShapeType shapetype = ShapeType.eCircle, bool createHash = true)
            : this(room, shapetype, createHash)
        {
            if (userProps != null)
            {
                // add the userProps to the props
                foreach (dynamic p in userProps.Keys)
                {
                    // if the key is a comp type, we need to add the component to comps dict
                    if (p is comp)
                    {
                        comp c = (comp)p;
                        fetchComponent(Utils.compTypes[c], userProps[c]);
                    }
                    // if the key is a node type, we need to update the instance variable value
                    else if (p is nodeE)
                    {
                        nodeE nn = (nodeE)p;
                        storeInInstance(nn, userProps);
                    }
                }
            }
            SortComponentLists();
        }

        public static Node ContructLineWall(Room room, Vector2 start, Vector2 end, int thickness, Dictionary<dynamic, dynamic> props = null, bool addToWallGroup = true)
        {
            float dist = Vector2.Distance(start, end);
            int halfheight = (int)(dist / 2);
            int halfwidth = thickness / 2;
            float angle = Utils.VectorToAngle(start - end);

            Node n = new Node(room, props, ShapeType.ePolygon);
            Polygon p = (Polygon)n.body.shape;
            n.body.orient = angle;
            p.SetBox(halfwidth, halfheight);
            n.body.pos = (start + end) / 2;
            n.body.DrawPolygonCenter = false;
            

            n.body.SetStatic();
            if (addToWallGroup)
            {
                room.masterGroup.childGroups["Wall Group"].IncludeEntity(n);
                n.OnSpawn();
            }
            return n;
        }

        public T Comp<T>() where T : Component
        {
            return (T)comps[typeof(T)];
        }
        public bool HasComp<T>() where T : Component
        {
            return comps.ContainsKey(typeof(T));
        }
        public bool HasComp(comp component)
        {
            return comps.ContainsKey(Utils.compTypes[component]);
        }
        public bool HasComp(Type componentType)
        {
            return comps.ContainsKey(componentType);
        }
        public bool HasActiveComponent(comp component)
        {
            Type t = Utils.compTypes[component];
            return comps.ContainsKey(t) && comps[t].active;
        }
        public void EnsureContains<T>(bool active = true) where T : Component
        {
            if (!HasComp<T>())
            {
                addComponent<T>(active);
            }
        }
        public bool HasActiveComponent<T>()
        {
            return comps.ContainsKey(typeof(T)) && comps[typeof(T)].active;
        }
        [Info(UserLevel.Never)]
        public dynamic this[comp component]
        {
            get
            {
                return comps[Utils.compTypes[component]];
            }
            set
            {
                comps[Utils.compTypes[component]] = value;
            }
        }
        public T CheckData<T>(string key)
        {
            if (dataStore.ContainsKey(key))
            {
                return dataStore[key];
            }
            else
            {
                return default(T);
            }
        }

        public void SetData(string key, dynamic data)
        {
            dataStore[key] = data;
        }

        public void AddTag(string tag)
        {
            tags.Add(tag);
        }
        public void RemoveTag(string tag)
        {
            tags.Remove(tag);
        }
        public Func<Node, bool> ExclusionCheck = null;
        public int affectionReach = 180;
        public virtual void Update(GameTime gametime)
        {
            if (IsPlayer)
            {
                body.angularVelocity = 0;
            }

            if (!movement.pushable && tempPosition != new Vector2(0, 0))
            {
                body.pos = tempPosition;
                body.velocity = Vector2.Zero;
            }
            body.effvelocity = body.pos - tempPosition;
            tempPosition = body.pos;
            
            //collision.ClearCollisionList();
            collision.ClearCollisionLists();
            if (nodeState == state.off || nodeState == state.drawOnly) return;

            if (aOtherProps.Count > 0)
            {
                if (room.affectAlgorithm == 1)
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
                    returnObjectsFinal = room.gridsystemAffect.retrieve(body, reach);
                    returnObjectsFinal.Remove(body);
                    if (ExclusionCheck == null)
                    {
                        foreach (Collider other in returnObjectsFinal)
                        {
                            if (room.ColorNodesInReach && this == room.targetNode && other is Body) (other as Body).color = Color.Purple;
                            if (other.parent.active)
                            {
                                foreach (Type t in aOtherProps)
                                {
                                    if (!comps[t].active) continue;
                                    comps[t].AffectOther(other.parent);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (Collider other in returnObjectsFinal)
                        {
                            if (room.ColorNodesInReach && this == room.targetNode && other is Body) (other as Body).color = Color.Purple;
                            if (other.parent.active)
                            {
                                if (ExclusionCheck(other.parent)) continue; //todo: extend to check for every component for finer control if necessary
                                foreach (Type t in aOtherProps)
                                {
                                    if (!comps[t].active) continue;
                                    comps[t].AffectOther(other.parent);
                                }
                            }
                        }
                    }
                }
                else if (room.affectAlgorithm == 2)
                {
                    if (meta.IgnoreAffectGrid)
                    {
                        foreach(Node n in room.masterGroup.fullSet)
                        {
                            affectAction(body, n.body);
                        }
                    }
                    else
                    {
                        room.gridsystemAffect.retrieveOffsetArraysAffect(body, affectAction, affectionReach);
                    }
                }
            }
            if (OnAffectOthers != null) OnAffectOthers.Invoke(this, null);

            foreach (Component component in comps.Values)
            {
                component.CaluclateDecay();
                Type t = component.GetType();
                if (aSelfProps.Contains(t))
                    component.AffectSelf();
            }

            if (IsPlayer)
            {
                player.controller.UpdateNewState();
                foreach (Type c in playerProps)
                {
                    comps[c].PlayerControl(player.controller);
                }
                player.controller.UpdateOldState();
            }
            //AI execution
            if (IsAI)
            {
                foreach (Type c in aiProps)
                {
                    comps[c].AIControl(AIMode.Agro);
                }
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
        //may implement this optimization if there are more affect others compoenents based on grabbing surrounding nodes from buckets
        //public void ReorderAffectOthersList()
        //{
        //    List<Type> affectOthers = aOtherProps.ToList();
        //    //affectOthers.Sort((a, b) => )
        //
        //}


        public void Draw()
        {
            if (nodeState == state.off || nodeState == state.updateOnly) return;

            foreach (Type c in drawProps)
            {
                if (!comps[c].CallDraw) continue;
                if (!comps[c].active) continue;
                comps[c].Draw();
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

        public void setCompActive(Type c, bool Active)
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
        public bool isCompActive(Type c)
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
                    Type t = Utils.compTypes[p];
                    fetchComponent(t, userProps[p]);
                    if (HasComp(t)) comps[t].active = userProps[p];
                }
            }
            SortComponentLists();
        }

        public void addComponent(comp c, bool active, bool overwrite = false)
        {
            bool fetch = fetchComponent(Utils.compTypes[c], active, overwrite);
            if (fetch) SortComponentLists();
        }
        public void addComponent(Type t, bool active, bool overwrite = false)
        {
            bool fetch = fetchComponent(t, active, overwrite);
            if (fetch) SortComponentLists();
        }
        public void addComponent<T>(bool active, bool overwrite = false) where T : Component
        {
            bool fetch = fetchComponent(typeof(T), active, overwrite);
            if (fetch) SortComponentLists();
        }
        public void addComponent(Component component, bool active, bool overwrite = false)
        {
            component.parent = this;
            if (comps.ContainsKey(component.GetType()) && !overwrite) return;
            comps[component.GetType()] = component;
            component.active = active;

            component.Initialize(this);
            SortComponentLists();
            if (IsPlayer && component.IsItem())
            {
                player.AddItem(component);
            }
        }

        public bool fetchComponent(Type t, bool active, bool overwrite = false)
        {
            if (t == typeof(Movement))//todo: add more essentials here
            {
                movement.active = active;
                return false;
            }
            else if (t == typeof(Collision))
            {
                collision.active = active;
                return false;
            }
            else if (t == typeof(BasicDraw))
            {
                basicdraw.active = active;
                return false;
            }
            else if (t == typeof(Meta))
            {
                meta.active = active;
                return false;
            }
            
            if (overwrite)
            {

                Component component = MakeComponent(t, active, this);
                if (HasComp(t))
                {
                    comps.Remove(t);
                }
                comps.Add(t, component);
                if (IsPlayer && component.IsItem())
                {
                    player.AddItem(component);
                }

            }
            else
            {
                if (!HasComp(t))
                {
                    Component component = MakeComponent(t, active, this);
                    comps.Add(t, component);
                    if (IsPlayer && component.IsItem())
                    {
                        player.AddItem(component);
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
            
        }

        public static Component MakeComponent(Type t, bool active, Node parent)
        {
            Component component;

            component = Component.GenerateComponent(t, parent);
            //component.parent = this;
            component.active = active;
            component.AfterCloning();

            return component;
        }

        public void RemoveComponent(comp c)
        {
            Type t = Utils.compTypes[c];
            if (!comps.ContainsKey(t))
            {
                Console.WriteLine("Component already removed or doesn't exist.");
                return;
            }
            comps[t].active = false;
            compsToRemove.Add(t);
            if (!room.masterGroup.fullSet.Contains(this))
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
            List<Type> toremove = new List<Type>();
            List<Type> toaddremove = new List<Type>();
            foreach (Type c in compsToRemove)
            {
                if (comps.ContainsKey(c))
                {
                    if (!drawProps.Contains(c) && !aSelfProps.Contains(c) && !aOtherProps.Contains(c))
                    {
                        //we should call a 'destroy component' method here, instead of just hoping it gets garabage collected
                        if (IsPlayer)
                        {
                            player.RemoveItem(comps[c]);
                        }
                        comps[c].OnRemove(null);
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
            foreach (Type c in compsToAdd)
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
            aOtherProps = new List<Type>();
            aSelfProps = new List<Type>();
            playerProps = new List<Type>();
            aiProps = new List<Type>();

            var clist = comps.Keys.ToList();
            Comparison<Type> typeComparer = delegate(Type t1, Type t2)
            {
                string s1 = t1.ToString().LastWord('.');
                string s2 = t2.ToString().LastWord('.');
                return s1.CompareTo(s2);
            };
            clist.Sort(typeComparer);

            foreach (Type c in clist)
            {
                if (c == typeof(Movement) || c == typeof(Collision)) continue;
                if (comps.ContainsKey(c) && isCompActive(c) && ((comps[c].compType & mtypes.affectother) == mtypes.affectother))
                {
                    aOtherProps.Add(c);
                }
            }
            foreach (Type c in clist)
            {
                if (c == typeof(Movement)) continue;
                if (comps.ContainsKey(c) && isCompActive(c) && ((comps[c].compType & mtypes.affectself) == mtypes.affectself))
                {
                    aSelfProps.Add(c);
                }
            }
            if (IsPlayer)
            {
                foreach (Type c in clist)
                {
                    if (comps.ContainsKey(c) && isCompActive(c) && ((comps[c].compType & mtypes.playercontrol) == mtypes.playercontrol))
                    {
                        playerProps.Add(c);
                    }
                }
            }
            //if (meta.AImode != AIMode.None && meta.AImode != AIMode.Player)
            //{
                foreach (Type c in clist)
                {
                    if (comps.ContainsKey(c) && isCompActive(c) && ((comps[c].compType & mtypes.aicontrol) == mtypes.aicontrol))
                    {
                        aiProps.Add(c);
                    }
                }
            //}
        }

        public void SortComponentListsDraw()
        {
            drawProps = new List<Type>();

            var clist = comps.Keys.ToList();
            Comparison<Type> typeComparer = delegate(Type t1, Type t2)
            {
                string s1 = t1.ToString().LastWord('.');
                string s2 = t2.ToString().LastWord('.');
                return s1.CompareTo(s2);
            };
            clist.Sort(typeComparer);

            foreach (Type c in clist)
            {
                if (comps.ContainsKey(c) && isCompActive(c) && ((comps[c].compType & mtypes.minordraw) == mtypes.minordraw))
                {
                    drawProps.Add(c);
                }
            }
            foreach (Type c in clist)
            {
                if (comps.ContainsKey(c) && isCompActive(c) && ((comps[c].compType & mtypes.draw) == mtypes.draw))
                {
                    drawProps.Add(c);
                }
            }
        }
        public Texture2D getTexture()
        {
            return Assets.textureDict[body.texture];
        }
        public Texture2D getTexture(textures t)
        {
            return Assets.textureDict[t];
        }
        public Vector2 TextureCenter()
        { 
            Texture2D tx = Assets.textureDict[body.texture];
            return new Vector2(tx.Width / 2f, tx.Height / 2f); // TODO: maybe cast to floats to make sure it's the exact center.
        }
        
        public void SetColor(Color c)
        {
            body.color = c;
            body.permaColor = c;
            basicdraw.UpdateColor();
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
            foreach (Type key in comps.Keys.ToList())
            {
                Component component = comps[key];
                //MethodInfo mInfo = component.GetType().GetMethod("OnSpawn");
                //if (mInfo != null
                //    && mInfo.DeclaringType == component.GetType())
                //{
                    component.OnSpawn();
                //}
            }
        }

        public void OnDeath(Node other, bool delete = true)
        {
            foreach (Type key in comps.Keys.ToList())
            {
                if (key == typeof(Meta)) continue;
                Component component = comps[key];
                //MethodInfo mInfo = component.GetType().GetMethod("OnRemove");
                //if (mInfo != null
                //    && mInfo.DeclaringType == component.GetType())
                //{
                    component.OnRemove(other);
                //}
            }
            meta.OnRemove(other);
            if (group != null && delete)
            {
                group.DeleteEntity(this);
            }
        }

        public void OnDelete()
        {
            room.nodeHashes.Remove(nodeHash);

            //active = false;

            //if (this == room.targetNode) room.targetNode = null;
            //if (this == Game1.ui.sidebar.inspectorArea.editNode) Game1.ui.sidebar.inspectorArea.editNode = null; //todo: social design pattern
            //if (this == Game1.ui.spawnerNode) Game1.ui.spawnerNode = null;

            //if (room.masterGroup != null && room.masterGroup.fullSet.Contains(this))
            //{
            //    room.masterGroup.DiscludeEntity(this);
            //}
            OnDeath(null, false);
        }

        public Node CreateClone(Room room = null, bool CloneHash = false)
        {
            Room r = room ?? this.room;
            Node newNode = new Node(r, !CloneHash);
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
                    Dictionary<Type, Component> dict = sourceNode.comps;
                    foreach (Type key in dict.Keys)
                    {
                        if (key == typeof(Movement) || key == typeof(Collision)) continue;
                        destNode.addComponent(key, sourceNode.comps[key].active);
                        Component.CloneComponent(dict[key], destNode.comps[key]);
                        destNode.comps[key].Initialize(destNode);
                    }
                    foreach (Type key in destNode.comps.Keys.ToList())
                    {
                        if (key == typeof(Movement) || key == typeof(Collision)) continue;
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
            }
        }

        internal void clearData(string p)
        {
            dataStore.Remove(p);
        }
    }
}