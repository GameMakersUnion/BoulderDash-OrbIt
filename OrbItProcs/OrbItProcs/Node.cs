using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.CSharp;
using System.Reflection;



using System.Collections.Specialized;



namespace OrbItProcs {

    public enum node {
        active,
        position,
        velocity,
        multiplier,
        effectiveRadius,
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
    //public delegate void MyEventHandler (object sender, EventArgs e);
    public delegate void CollisionDelegate(Node source, Node target);

    public class Node {
        private string _nodeHash = "";
        public string nodeHash { get { return _nodeHash; } set 
        { 
            //if (room.nodeHashes.Contains(_nodeHash)) Console.WriteLine("Total Hashes:{0} , removed hash:{1}", room.nodeHashes.Count, _nodeHash);
            room.nodeHashes.Remove(_nodeHash);
            _nodeHash = value;
            /*if (room.nodeHashes.Contains(value))
            {
                string unqStr = Utils.uniqueString(room.nodeHashes);
                Group mstrGrp = room.masterGroup;

                Node nd = mstrGrp.FindNodeByHash(value);
                if (nd == null) System.Diagnostics.Debugger.Break();
                nd.nodeHash = unqStr;

            }*/
            room.nodeHashes.Add(value);
            //Console.WriteLine("Total Hashes:{0} , added hash:{1}, with name: {2} ", room.nodeHashes.Count, value, this.name);
        } }

        public static int nodeCounter = 0;

        public event EventHandler OnAffectOthers;

        public event CollisionDelegate OnCollision;
        //public event CollisionDelegate CollidedFrame;
        public event CollisionDelegate OnCollisionStart;
        public event CollisionDelegate OnCollisionEnd;

        public Dictionary<string, dynamic> DataStore = new Dictionary<string, dynamic>();

        //public Dictionary<dynamic, dynamic> CollideArgs;
        //static Dictionary<dynamic, dynamic> defaultProps = new Dictionary<dynamic, dynamic>() { };
        public T GetComponent<T>()
        {
            //return comps[Game1.compEnums[typeof(T)]];
            return comps[Utils.compEnums[typeof(T)]];
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
                OnAffectOthers += Node_OnAffectOthers;
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
                }
                _active = value;
            }
        }

        void Node_OnAffectOthers(object sender, EventArgs e)
        {
            
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

        private float _multiplier = 1f;
        public float multiplier { get { return _multiplier; } set { _multiplier = value; } }

        public int lifetime = -1;
        private float _effectiveRadius = 100f;
        public float effectiveRadius { get { return _effectiveRadius; } set { _effectiveRadius = value; } }

        private string _name = "node";
        public string name { get { return _name; } set { _name = value; } }

        private int _sentinel = -10;
        public int sentinelp { get { return _sentinel; } set { _sentinel = value; } }

        public Room room = Program.getRoom();

        private Dictionary<comp, dynamic> _comps = new Dictionary<comp, dynamic>();
        public Dictionary<comp, dynamic> comps { get { return _comps; } set { _comps = value; } }

        public List<comp> aOtherProps = new List<comp>();
        public List<comp> aSelfProps = new List<comp>();
        public List<comp> drawProps = new List<comp>();

        public List<comp> compsToRemove = new List<comp>();
        public List<comp> compsToAdd = new List<comp>();

        public Body body;
        public Body BODY
        {
            get { return body; }
            set
            {
                body = value;
                if (comps != null && value != null)
                {
                    if (comps.ContainsKey(comp.body))
                    {
                        comps.Remove(comp.body);
                    }
                    comps.Add(comp.body, value);
                }
            } 
        }

        public Movement movement;
        public Movement MOVEMENT
        {
            get { return movement; }
            set
            {
                movement = value;
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

        public Collision collision;
        public Collision COLLISION
        {
            get { return collision; }
            set
            {
                collision = value;
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

        private ObservableHashSet<Link> _SourceLinks = new ObservableHashSet<Link>();
        [Polenter.Serialization.ExcludeFromSerialization]
        public ObservableHashSet<Link> SourceLinks { get { return _SourceLinks; } set { _SourceLinks = value; } }

        private ObservableHashSet<Link> _TargetLinks = new ObservableHashSet<Link>();
        [Polenter.Serialization.ExcludeFromSerialization]
        public ObservableHashSet<Link> TargetLinks { get { return _TargetLinks; } set { _TargetLinks = value; } }
        [Polenter.Serialization.ExcludeFromSerialization]
        public ObservableHashSet<Group> Groups { get; set; }

        

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

        public bool HasCollision()
        {
            return OnCollision != null;
        }
        public bool HasCollisionStart()
        {
            return OnCollisionStart != null;
        }
        public bool HasCollisionEnd()
        {
            return OnCollisionEnd != null;
        }
        public void OnCollisionInvoke(Node target)
        {
            if (OnCollision != null)
            {
                OnCollision(this, target);
            }
        }
        public void OnCollisionStartInvoke(Node target)
        {
            if (OnCollisionStart != null)
            {
                OnCollisionStart(this, target);
            }
        }
        public void OnCollisionEndInvoke(Node target)
        {
            if (OnCollisionEnd != null)
            {
                OnCollisionEnd(this, target);
            }
        }

        public void storeInInstance(node val, Dictionary<dynamic,dynamic> dict)
        {
            if (val == node.active)             active                      = dict[val];
            if (val == node.position)           body.pos          = dict[val];
            if (val == node.velocity)           body.velocity          = dict[val];
            if (val == node.multiplier)         multiplier                  = dict[val];
            if (val == node.effectiveRadius)    effectiveRadius             = dict[val];
            if (val == node.radius)             body.radius            = dict[val];
            if (val == node.mass)               body.mass              = dict[val];
            if (val == node.scale)              body.scale             = dict[val];
            if (val == node.texture)            body.texture           = dict[val];
            if (val == node.name)               name                        = dict[val];
            if (val == node.lifetime)           lifetime                    = dict[val];
            if (val == node.color)              body.color             = dict[val];
        }
        //these comes will allow eachother's draws to be called (all 4 could draw at once)
        public static List<comp> drawPropsSuper = new List<comp>()
        {
            comp.waver,
            comp.wideray,
            comp.laser,
            comp.phaseorb
        };

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

            comps.Add(comp.body, body);
            comps.Add(comp.movement, movement);
            comps.Add(comp.collision, collision);
            
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
                else if (p is node)
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
            if (DataStore.ContainsKey(key))
            {
                return DataStore[key];
            }
            else
            {
                return default(T);
            }
        }

        public void SetData(string key, dynamic data)
        {
            DataStore[key] = data;
        }

        public virtual void Update(GameTime gametime)
        {
            if (nodeState == state.off || nodeState == state.drawOnly) return;

            if (aOtherProps.Count > 0)
            {
                List<Node> returnObjectsFinal = new List<Node>();

                returnObjectsFinal = room.gridsystem.retrieve(this);

                int cellReach = (int)(body.radius * 2) / room.gridsystem.cellWidth * 2;


                if (comps.ContainsKey(comp.flow) && comps[comp.flow].active)
                {
                    returnObjectsFinal = new List<Node>();
                    if (comps[comp.flow].activated)
                    {
                        returnObjectsFinal = comps[comp.flow].outgoing.ToList();
                    }
                }
                returnObjectsFinal.Remove(this);

                foreach (Node other in returnObjectsFinal)
                {
                    if (other.active)
                    {
                        foreach (comp c in aOtherProps)
                        {
                            comps[c].AffectOther(other);
                        }
                    }
                }
            }
            collision.ClearCollisionList();

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

            int numOfSupers = 0;
            foreach (comp c in drawProps)
            {
                if (!comps[c].CallDraw) continue;
                if (!comps[c].active) continue;
                if (drawPropsSuper.Contains(c))
                {
                    numOfSupers++;
                    comps[c].Draw(spritebatch);
                }
                else
                {
                    if (numOfSupers > 0) break;
                    comps[c].Draw(spritebatch);
                    if (!comps[c].methods.HasFlag(mtypes.minordraw))
                        break; //only executes the most significant draw component
                }
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
                if (p is node)// && !(userProps[p] is bool))
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
            fetchComponent(c, active, overwrite);
            SortComponentLists();
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
                return true;
            }
            else if (c == comp.collision)
            {
                collision.active = active;
                return true;
            }

            Component component = MakeComponent(c, active, this);

            
            if (overwrite)
            {
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
                    comps.Add(c, component);
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
                Console.WriteLine("Component already removed or doesn't exist.");
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
                if (c == comp.movement || c == comp.body || c == comp.collision) continue;
                if (comps.ContainsKey(c) && isCompActive(c) && ((comps[c].methods & mtypes.affectother) == mtypes.affectother))
                {
                    aOtherProps.Add(c);
                }
            }
            foreach (comp c in clist)
            {
                if (c == comp.movement || c == comp.body || c == comp.collision) continue;
                if (comps.ContainsKey(c) && isCompActive(c) && ((comps[c].methods & mtypes.affectself) == mtypes.affectself))
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
                if (comps.ContainsKey(c) && isCompActive(c) && ((comps[c].methods & mtypes.draw) == mtypes.draw))
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
            if (this == room.game.targetNode) room.game.targetNode = null;
            if (this == room.game.ui.sidebar.inspectorArea.editNode) room.game.ui.sidebar.inspectorArea.editNode = null;
            if (this == room.game.ui.spawnerNode) room.game.ui.spawnerNode = null;
            if (room.masterGroup != null && room.masterGroup.fullSet.Contains(this))
            {
                room.masterGroup.DeleteEntity(this);
            }
        }

        public Node CreateClone(bool CloneHash = false)
        {
            Node newNode = new Node(!CloneHash);
            cloneObject(this, newNode, CloneHash);
            return newNode;
        }

        public static void cloneObject(Node sourceNode, Node destNode, bool CloneHash = false) //they must be the same type
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
            foreach (FieldInfo field in fields)
            {
                if (field.Name.Equals("_nodeHash"))
                {
                    if (!CloneHash)
                    {
                        continue;
                    }
                }
                //Console.WriteLine("fieldtype: " + field.FieldType);
                if (field.FieldType.ToString().Contains("Dictionary"))
                {
                    if (field.FieldType.ToString().Contains("[OrbItProcs.comp,System.Object]"))//must be comps
                    {
                        //Console.WriteLine("COMPS");
                        //dynamic node = sourceNode;
                        Dictionary<comp, dynamic> dict = sourceNode.comps;
                        //dynamic newNode = destNode;
                        foreach (comp key in dict.Keys)
                        {
                            destNode.addComponent(key, sourceNode.comps[key].active);
                            //cloneObject<Component>(dict[key], destNode.comps[key]);
                            Component.CloneComponent(dict[key], destNode.comps[key]);

                            destNode.comps[key].Initialize(destNode);

                            //MethodInfo mInfo = sourceNode.comps[key].GetType().GetMethod("InitializeLists");
                            //if (mInfo != null &&
                            //   mInfo.DeclaringType == sourceNode.comps[key].GetType()) Console.WriteLine(sourceNode.comps[key].GetType()+"initializelist exists");
                            //else Console.WriteLine(sourceNode.comps[key].GetType()+"doesn't have initilizelists");

                            //TODO: how can I check if there is a *non-empty* override of the onCollision method in this component
                            // and if so, add it to the node's Collided event...
                        }
                        //Console.WriteLine(newNode.comps[comp.randinitialvel].multiplier);

                        foreach (comp key in destNode.comps.Keys.ToList())
                        {
                            Component component = destNode.comps[key];
                            MethodInfo mInfo = component.GetType().GetMethod("AfterCloning");
                            if (mInfo != null
                                && mInfo.DeclaringType == component.GetType())
                            {
                                component.AfterCloning();
                            }

                        }
                    }
                }
                else if ((field.FieldType.ToString().Equals("System.Int32"))
                    || (field.FieldType.ToString().Equals("System.Single"))
                    || (field.FieldType.ToString().Equals("System.Boolean"))
                    || (field.FieldType.ToString().Equals("System.String")))
                {
                    if (!field.Name.Equals("IsDefault"))
                        field.SetValue(destNode, field.GetValue(sourceNode));
                }
                else if (field.FieldType.ToString().Contains("Vector2"))
                {
                    Vector2 vect = (Vector2)field.GetValue(sourceNode);
                    Vector2 newvect = new Vector2(vect.X, vect.Y);
                    field.SetValue(destNode, newvect);
                }
                else if (field.FieldType.ToString().Equals("Microsoft.Xna.Framework.Color"))
                {
                    Color col = (Color)field.GetValue(sourceNode);
                    Color newcol = new Color(col.R, col.G, col.B, col.A);
                    field.SetValue(destNode, newcol);
                }
                else if (field.Name.Equals("parent"))
                {
                    //do nothing
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
                    Component.CloneComponent(sourceNode.body, destNode.body);
                    destNode.body.parent = destNode;
                    destNode.body.shape.body = destNode.body;
                    destNode.body.AfterCloning();
                }
                else
                {
                    //this would be an object field
                    //Console.WriteLine(field.Name + " is an object of some kind.");
                    if (field.Name.Equals("room") || field.Name.Equals("_texture"))
                    {
                        field.SetValue(destNode, field.GetValue(sourceNode));
                    }
                }
                //field.SetValue(newobj, field.GetValue(obj));
                //Console.WriteLine("{0}", field.FieldType);
            }
        }
        
        


        /*
         * - need to make sure that different components don't make the same calculations (grav and repel will both calculate: (dist, angle, force, etc))
         *     however, it is important to note that sometimes values must be recalculated (if the calculation involves a value from the component's properties dictionary
         * - somehow manage a priority list of which components are applied first
         * - better to keep components in seperate dictionary from properties (if looping through components, this will be faster)
         * - components will make use of their own properties (you want an object's gravComponent to have a multiplier of 20f,
         *     but it's repelComponent to have a multiplier of 50f)
         * - the component could be initialized with, for example 2/5 properites. 
         *     the 2 will be used, and the missing 3 will be taken from the object's properties (rather than the compoent's properties)
         * - the component's initialization must happen seperately from it's function, otherwise you'd need to check that everything was initialized every frame
         * - need to make sure that using a properties dictionary isn't slower than using normal instance variables (due to lookup overhead) 
         *     if so, system might need rethinking
         * - each component will have certain 'meta properties' that describe what kind of component it is.
         *      for example, some components will 'iterateOverAllNodes' (two of these together will perform calculations all in one loop)
         * - during the Node intialization, the priority list of components will be referenced, and using the existing components attached to the node 
         * (in the node's component dictionary) it will create an ordering of component execution (this list contains Only the present components)
         *  this list will also be updated upon the AddComponent method, to support adding components at runtime and updating the priority list
         *  - the components can be added to several ordered lists. all components that 'iterateOverAllNodes' will be put in that list (and ordered based 
         *   on priority.) All components that 'changeSelf' will be thrown in that list also. The list will only contain references to the components
         *   (I think...), so they can still be referenced any time from the componentDictionary of that Node.
         * 
         * 
        */

        //make a tree node (in fact, make a node based off every data structure

        //---------------------------------------------------------------------------------START SHIT

    }
}