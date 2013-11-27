﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.CSharp;
using System.Reflection;

using OrbItProcs.Processes;
using OrbItProcs.Components;
using System.Collections.Specialized;



namespace OrbItProcs {

    public enum node {
        active,
        position,
        velocity,
        velMultiplier,
        multiplier,
        effectiveRadius,
        radius,
        mass,
        scale,
        texture,
        name,
        lifetime,
        collidable,
        color,
    
    };

    //public delegate void MyEventHandler (object sender, EventArgs e);

    public class Node {

        public static int nodeCounter = 0;

        public event ProcessMethod Collided;
        public Dictionary<dynamic, dynamic> CollideArgs;

        static Dictionary<dynamic, dynamic> defaultProps = new Dictionary<dynamic, dynamic>()
        {
            { node.active,                      true },
            { node.position,                    new Vector2(0,0) },
            { node.velocity,                    new Vector2(0,0) },
            { node.velMultiplier,               1f  },
            { node.multiplier,                  1f },
            { node.effectiveRadius,             100f  },
            { node.radius,                      25f  },
            { node.mass,                        10f  },
            { node.scale,                       1f  },
            { node.texture,                     null  },
            { node.name,                        "node" },
            { node.lifetime,                    -1 },
            { node.collidable,                  true  },
            { node.color,                       new Color(255,255,255) },
            //{ comp.movement,                    true },
            
        };

        private bool triggerSortComponentsUpdate = false, triggerSortComponentsDraw = false;
        //public bool active = true;
        public bool _collidable = true;
        public bool collidable { get { return _collidable; } set { _collidable = value; } }

        
        private float radius = 25f;
        public float Radius
        {
            get { return radius; }
            set {
                radius = value;
                if (texture != null)
                    scale = value / (getTexture().Width / 2);
                else
                    scale = value / (50 / 2);
            }
        }
        public float _mass = 10f;
        public float mass { get { return _mass; } set { _mass = value; } }
        
        public float _multiplier = 1f;
        public float multiplier { get { return _multiplier; } set { _multiplier = value; } }
        public Vector2 position = new Vector2(0,0);
        public Vector2 velocity = new Vector2(0,0);
        public int lifetime = -1;
        public float _effectiveRadius = 100f;
        public float effectiveRadius { get { return _effectiveRadius; } set { _effectiveRadius = value; } }

        public float _scale = 1f; // TODO: make the setter change the radius -e harely
        public float scale { get { return _scale; } 
            set { _scale = value; } 
        }
        
        public string _name = "node";
        public string name { get { return _name; } set { _name = value; } }

        public Color color = new Color(255,255,255);

        public int _sentinel = -10;
        
        public int sentinelp { get; set; }

        public Room room = Program.getRoom();
        public textures _texture = textures.whitecircle;
        public textures texture { get { return _texture; } set { _texture = value; } }

        public float _velMultiplier = 1f;
        public float velMultiplier { get { return _velMultiplier; } set { _velMultiplier = value; } }


        public Dictionary<dynamic, bool> props = new Dictionary<dynamic, bool>();
        public Dictionary<dynamic, bool> propsProperty { get { return props; } set { props = value; } }
    
        public Dictionary<comp, dynamic> comps = new Dictionary<comp, dynamic>();
        public Dictionary<comp, dynamic> compsProperty { get { return comps; } set { comps = value; } }
        //public OrderedDictionary ocomps;

        public List<comp> aOtherProps = new List<comp>();
        public List<comp> aSelfProps = new List<comp>();
        public List<comp> drawProps = new List<comp>();

        public void storeInInstance(node val, Dictionary<dynamic,dynamic> dict)
        {
            //if (val == node.active)             active          = dict[val];
            if (val == node.position)           position        = dict[val];
            if (val == node.velocity)           velocity        = dict[val];
            if (val == node.velMultiplier)      velMultiplier   = dict[val];
            if (val == node.multiplier)         multiplier      = dict[val];
            if (val == node.effectiveRadius)    effectiveRadius = dict[val];
            if (val == node.radius)             radius          = dict[val];
            if (val == node.mass)               mass            = dict[val];
            if (val == node.scale)              scale           = dict[val];
            if (val == node.texture)            texture         = dict[val];
            if (val == node.name)               name            = dict[val];
            if (val == node.lifetime)           lifetime        = dict[val];
            if (val == node.collidable)         collidable      = dict[val];
            if (val == node.color)              color           = dict[val];

        }

        public Node()
        {
            nodeCounter++;
            room = Program.getRoom();
            
        }

        public Node(Room room1, Dictionary<dynamic, dynamic> userProps = null)
        {
            nodeCounter++;    

            // add the userProps to the props
            foreach (dynamic p in userProps.Keys)
            {
                // if the value is bool, it is a property and is added to props dict
                //3if (p is comp && userProps[p] is bool)
                //3    props.Add(p, userProps[p]);
                // if the key is a comp type, we need to add the component to comps dict
                if (p is comp)
                {
                    fetchComponent(p);
                    if (comps.ContainsKey(p)) comps[p].active = userProps[p];
                }
                // if the key is a node type, (and not a bool) we need to update the instance variable value
                if (p is node)// && !(userProps[p] is bool))
                    storeInInstance(p, userProps);

            }
            // fill in remaining defaultProps
            foreach (dynamic p in defaultProps.Keys)
            {
                if (!userProps.ContainsKey(p) && defaultProps[p] is bool)
                {

                    if (p is comp)
                    {
                        fetchComponent(p);
                        if (comps.ContainsKey(p)) comps[p].active = defaultProps[p];
                    }
                    else
                    {
                        props.Add(p, defaultProps[p]); // not adding comps to props anymore
                    }
                }

                //don't need to add non-bools to instance values: they've been set with the same values as defualt (upon definition)
                    
            }
            SortComponentLists();
            //room = room1;
            room = Program.getRoom();
            if (room == null) Console.WriteLine("null");
            if (Program.getGame() == null) Console.WriteLine("gnull");
            if (lifetime > 0) name = "temp|" + name + Guid.NewGuid().GetHashCode().ToString().Substring(0,5);
            else name = name + nodeCounter;
            
        }

        public override string ToString()
        {
            //return base.ToString();
            return name;
        }

        public void setCompActive(comp c, bool Active)
        {
            if (comps.ContainsKey(c))
            {
                comps[c].active = Active;
                //ensure that the componenent delegate lists are updated to only include active components
                //triggerSortLists() is called from the 'active' property in the component class 
                //(to ensure that if active is altered and this method isn't used, the update will still occur)
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
                //old method using comp enum values in props
                /*
                // if the value is bool, it is a property and is added to props dict
                if (p is comp && userProps[p] is bool)
                    props.Add(p, userProps[p]);
                // if the key is a comp type, we need to add the component to comps dict
                if (p is comp)
                    fetchComponent(p);
                // if the key is a node type, (and not a bool) we need to update the instance variable value
                if (p is node)// && !(userProps[p] is bool))
                    storeInInstance(p, userProps);
                */

                // if the key is a node type, (and not a bool) we need to update the instance variable value
                if (p is node)// && !(userProps[p] is bool))
                    storeInInstance(p, userProps);
                // if the key is a comp type, we need to add the component to comps dict
                if (p is comp)
                {
                    fetchComponent(p);
                    if (comps.ContainsKey(p)) comps[p].active = userProps[p];
                }
                
            }
        }

        public void addComponent(comp c, bool active)
        {
            if (!fetchComponent(c))
                return; // component not found.

            //if (!props.ContainsKey(c)) 
            //    props.Add(c, active);

            SortComponentLists();
        }

        //I believe this is unneccessary 
        //public void checkForComponentMethods(Component component)


        public void addComponent(Component component, bool active)
        { 
            //if (comps.ContainsKey(component.com))
            //component.active = active;                                      //move 'active' to base call Component and take comps out of props dict
            component.parent = this;
            comps.Add(component.com, component);
            props.Add(component.com, active);

            //checkForComponentMethods(component);
            component.Initialize(this);
            //if (component.hasMethod("InitializeLists")) component.InitializeLists();
            SortComponentLists();
        }



        public bool fetchComponent(comp c)
        {
            Component component;

            component = Game1.GenerateComponent(c);

            component.Initialize(this);

            if (!comps.ContainsKey(c))
            {
                comps.Add(c, component);
            }

            return true; // success
        }

        public void SortComponentLists()
        {
            SortComponentListsUpdate();
            SortComponentListsDraw();

            /*
            aOtherProps = new List<comp>();
            aSelfProps = new List<comp>();
            drawProps = new List<comp>();

            var clist = comps.Keys.ToList();
            clist.Sort();

            foreach (comp c in clist)
            {
                if (props.ContainsKey(c) && props[c] && comps[c].hasMethod("affectother"))
                {
                    aOtherProps.Add(c);
                }
            }
            foreach (comp c in clist)
            {
                if (props.ContainsKey(c) && props[c] && comps[c].hasMethod("affectself"))
                {
                    //compsASC.Add(asc[p]);
                    aSelfProps.Add(c);
                }
            }
            foreach (comp c in clist)
            {
                if (props.ContainsKey(c) && props[c] && comps[c].hasMethod("draw"))
                {
                    //compsDC.Add(dc[p]);
                    drawProps.Add(c);
                }
            }
            */

            
        }

        public void SortComponentListsUpdate()
        {
            aOtherProps = new List<comp>();
            aSelfProps = new List<comp>();

            var clist = comps.Keys.ToList();
            clist.Sort();

            foreach (comp c in clist)
            {
                if (comps.ContainsKey(c) && isCompActive(c) && comps[c].hasMethod("affectother"))
                {
                    aOtherProps.Add(c);
                }
            }
            foreach (comp c in clist)
            {
                if (comps.ContainsKey(c) && isCompActive(c) && comps[c].hasMethod("affectself"))
                {
                    //compsASC.Add(asc[p]);
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
                if (comps.ContainsKey(c) && isCompActive(c) && comps[c].hasMethod("draw"))
                {
                    //compsDC.Add(dc[p]);
                    drawProps.Add(c);
                }
            }
        }

        
        
        public void Update(GameTime gametime)
        {

            List<Node> returnObjectsFinal = new List<Node>();
            List<Node> collisionList = new List<Node>();
            returnObjectsFinal = room.gridsystem.retrieve(this);

            int cellReach = (int)(Radius * 2) / room.gridsystem.cellwidth * 2;

            if (collidable) collisionList = room.gridsystem.retrieve(this, cellReach);
            HashSet<Node> hashlist = new HashSet<Node>();
            hashlist.UnionWith(returnObjectsFinal);//bad
            hashlist.UnionWith(collisionList);
            hashlist.Remove(this);

            //List<Node> gravList = room.gridsystem.retrieve(this, (int)(comps[comp.gravity].radius / room.gridsystem.cellwidth));
            if (collidable || aOtherProps.Count > 0)
            {
                //Console.WriteLine("yep");
                foreach (Node other in hashlist)
                //foreach (Node other in room.nodes)
                {
                    // if both are collidable
                    //if (other != this && collidable && other.collidable && other.props[node.active])
                    if (collisionList.Contains(other) && collidable && other.collidable && other.props[node.active])
                    //if (collidable && other.collidable && other.props[node.active]) 
                    {
                        //if (Utils.checkCollision(this, other))
                        //{
                        //    OnCollideInvoker();
                        //    other.OnCollideInvoker();
                        //    Utils.resolveCollision(this, other);
                        //}
                        //depreciated in version 17.4
                    }
                    if (returnObjectsFinal.Contains(other) && other.props[node.active] && true)
                    {
                        // iterate over components that contain 'effectOthers' method (and only call that method)
                        //if (other == room.game1.targetNode) Console.WriteLine("affecting target");
                        foreach (comp c in aOtherProps)
                        {
                            //if (props[c])
                            //if (isCompActive(c)) // we don't need this because we're assuming that any components in the aOtherProps list is active.
                            comps[c].AffectOther(other);
                            //a useless check except to make sure this isn't an error
                            //if (!isCompActive(c)) Console.WriteLine("a component that is not active is in the aOtherProps list.(bad)");
                        }
                    }
                }
            }

            // ONLY ENTER IF THERE IS AT LEAST ONE COMPONENT THAT HAS 'AFFECTOTHER' METHOD
            if (props[node.active] && aOtherProps.Count > 0 && false)
            {
                foreach (Node other in room.nodes)
                {
                    if (other != this && other.props[node.active])
                    {
                        // iterate over components that contain 'effectOthers' method (and only call that method)
                        foreach (comp c in aOtherProps)
                        {
                            //if (props[c])
                            //if (isCompActive(c)) // we don't need this because we're assuming that any components in the aOtherProps list is active.
                                comps[c].AffectOther(other);
                        }
                    }
                }
            }
            //yo
            // iterate over components that contain 'effectSelf' (and only call that method)
            foreach (comp c in aSelfProps)
            {
                //if (props[c])
                //if (isCompActive(c)) // we don't need this because we're assuming that any components in the aSelfProps list is active.
                    comps[c].AffectSelf();
                    //a useless check except to make sure this isn't an error
                    //if (!isCompActive(c)) Console.WriteLine("a component that is not active is in the aSelfProps list.(bad)");
            }
            /*foreach (ASC del in compsASC)
            {
                del(this);
            }*/

            //position += velocity;

            if (triggerSortComponentsUpdate)
            {
                SortComponentListsUpdate();
                triggerSortComponentsUpdate = false;
            }
            
        }

        public void OnCollidePublic()
        {
            OnCollideInvoker();
        }

        protected virtual void OnCollideInvoker()
        {
            if (Collided != null)
            {
                Collided(CollideArgs);
            }
        }

        public Texture2D getTexture()
        {
            return room.game1.textureDict[texture];
        }
        public Texture2D getTexture(textures t)
        {
            return room.game1.textureDict[t];
        }
        public Vector2 TextureCenter()
        { 
            Texture2D tx = room.game1.textureDict[texture];
            return new Vector2(tx.Width / 2f, tx.Height / 2f); // TODO: maybe cast to floats to make sure it's the exact center.
        }
        
        public void Draw(SpriteBatch spritebatch)
        {
            //Console.WriteLine(drawProps.Count);
            foreach (comp c in drawProps)
            {
                //if (props[c])
                //{
                    comps[c].Draw(spritebatch);
                //useless check expect to check for errors
                    //if (!isCompActive(c)) Console.WriteLine("a component that is not active is in the drawProps list.(bad)");
                    return;
                //}
            }
            /*foreach (DC del in compsDC)
            {
                del(this,spritebatch);
                return;
            }*/

            if (triggerSortComponentsDraw)
            {
                SortComponentListsDraw();
                triggerSortComponentsDraw = false;
            }
        }

        public void changeRadius(float newRadius)
        {
            radius = newRadius;
            scale = radius / (getTexture().Width / 2);
        }
        public float diameter()
        {
            return radius * 2;
        }

        public static void cloneObject(Node sourceNode, Node destNode) //they must be the same type
        {
            //dynamic returnval;
            List<FieldInfo> fields = sourceNode.GetType().GetFields().ToList();
            List<PropertyInfo> properties = sourceNode.GetType().GetProperties().ToList();
            //foreach (PropertyInfo property in properties)
            //{
            //    //if (property.Name.Equals("compsProp")) continue;
            //    property.SetValue(destNode, property.GetValue(sourceNode, null), null);
            //}
            foreach (FieldInfo field in fields)
            {
                //Console.WriteLine("fieldtype: " + field.FieldType);
                if (field.FieldType.ToString().Contains("Dictionary"))
                {
                    //Console.WriteLine(field.Name + " is a dictionary.");
                    if (field.FieldType.ToString().Contains("[System.Object,System.Boolean]"))//must be props
                    {
                        //Console.WriteLine("PROPS");
                        //dynamic node = sourceNode;

                        Dictionary<dynamic, bool> dict = sourceNode.props;
                        Dictionary<dynamic, bool> newdict = new Dictionary<dynamic, bool>();
                        foreach (dynamic key in dict.Keys)
                        {
                            newdict.Add(key, dict[key]);
                            //Console.WriteLine("key: {0}, value: {1}", key, dict[key]);
                        }
                        field.SetValue(destNode, newdict);

                    }
                    else if (field.FieldType.ToString().Contains("[OrbItProcs.comp,System.Object]"))//must be comps
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
                            

                            //TODO: how can I check if there is a *non-empty* override of the onCollision method in this component
                            // and if so, add it to the node's Collided event...
                        }
                        //Console.WriteLine(newNode.comps[comp.randinitialvel].multiplier);
                    }
                }
                else if ((field.FieldType.ToString().Equals("System.Int32"))
                    || (field.FieldType.ToString().Equals("System.Single"))
                    || (field.FieldType.ToString().Equals("System.Boolean"))
                    || (field.FieldType.ToString().Equals("System.String")))
                {
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