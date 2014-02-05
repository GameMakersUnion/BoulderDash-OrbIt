using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TomShane.Neoforce.Controls;
using System.Reflection;


using Component = OrbItProcs.Component;
using Console = System.Console;
using System.Collections.ObjectModel;
using System.Drawing;
using Color = Microsoft.Xna.Framework.Color;
using SysColor = System.Drawing.Color;

namespace OrbItProcs {

    public enum member_type {
        none,
        field,
        property,
        dictentry,
        collectionentry,
        previouslevel,
        unimplemented,
        
    };
    //listed in the order that they will appear in the listbox
    public enum data_type {
        none,
        dict,
        collection,
        list,
        obj,
        enm,
        boolean,
        integer,
        single,
        tbyte,
        str,
    };

    public class InspectorItem {

        public static List<Type> ValidTypes = new List<Type>()
        {
            typeof(Node),
            typeof(Player),
            typeof(Component),
            typeof(ModifierInfo),
            typeof(Vector2),
            typeof(Color),
            typeof(Game1),
            typeof(Room),
            typeof(GridSystem),
            typeof(Group),
            typeof(Link),
            typeof(Formation),
            typeof(ProcessManager),
            typeof(Process),
            typeof(ILinkable),
        };
        public static List<Type> PanelTypes = new List<Type>()
        {
            typeof(int),
            typeof(float),
            typeof(double),
            typeof(string),
            typeof(bool),
            typeof(Enum),
            typeof(byte),
        };
        //public treeitem itemtype;
        public int depth = 1;
        //public FieldInfo fieldInfo;
        //public PropertyInfo propertyInfo;
        public FPInfo fpinfo;
        public comp component;

        public bool extended = false;
        
        public List<object> children;
        
        //public int childCount = 0;
        public object key;
        public String prefix;
        public String whitespace = "";

        public object obj;
        public object parentobj;
        //public object parentdictionary;
        public InspectorItem parentItem;
        public member_type membertype;
        public data_type datatype;

        public IList<object> masterList;

        //root item
        public InspectorItem(IList<object> masterList, object obj)
        {
            this.whitespace = "|";
            this.obj = obj;
            this.masterList = masterList; 
            //this.fpinfo = new FPInfo(propertyInfo);
            this.membertype = member_type.none;
            this.children = new List<object>();
            CheckItemType();
            prefix = "" + ((char)164);
            //System.Console.WriteLine(obj);
            //children = GenerateList(obj, whitespace, this); 
            
        }
        //a property
        public InspectorItem(IList<object> masterList, InspectorItem parentItem, object obj, PropertyInfo propertyInfo)
        {
            this.whitespace = "|";
            if (parentItem != null) this.whitespace += parentItem.whitespace;
            this.obj = obj;
            this.parentItem = parentItem;
            //this.fieldInfo = fieldInfo;
            this.fpinfo = new FPInfo(propertyInfo);
            this.masterList = masterList; 
            this.membertype = member_type.property;
            this.children = new List<object>();
            CheckItemType();
            prefix = "" + ((char)164);
            //System.Console.WriteLine(this);
            //children = GenerateList(obj, whitespace, this);

        }
        //a dictionary entry
        public InspectorItem(IList<object> masterList, InspectorItem parentItem, object obj, object key) //obj = null
        {
            this.whitespace = "|";
            if (parentItem != null) this.whitespace += parentItem.whitespace;
            this.parentItem = parentItem;
            this.obj = obj;
            this.masterList = masterList;
            this.fpinfo = null;
            this.children = new List<object>();
            Type t = parentItem.obj.GetType();
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                membertype = member_type.dictentry;
                this.key = key;
                CheckItemType();
                prefix = "" + ((char)164);
                //System.Console.WriteLine(this);
                //children = GenerateList(obj, whitespace, this);
            }
            else
            {
                System.Console.WriteLine("Unexpected: InspectorItem with no obj reference was not a dictionary entry");
                membertype = member_type.unimplemented;
            }
        }
        //a IEnumberable entry
        public InspectorItem(IList<object> masterList, InspectorItem parentItem, object obj) //obj = null
        {
            this.whitespace = "|";
            if (parentItem != null) this.whitespace += parentItem.whitespace;
            this.parentItem = parentItem;
            this.obj = obj;
            this.masterList = masterList;
            this.fpinfo = null;
            this.children = new List<object>();
            Type t = parentItem.obj.GetType();

            if (t.GetInterfaces()
                .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                //Console.WriteLine("IEnumerable : {0}", obj.GetType());
                membertype = member_type.collectionentry;
                CheckItemType();
                prefix = "" + ((char)164);
            }
            else
            {
                System.Console.WriteLine("Unexpected: InspectorItem with no obj reference was not a collection entry");
                membertype = member_type.unimplemented;
            }
        }
        /*
        public InspectorItem(object parent, FieldInfo fieldInfo, InspectorItem parentItem)
        {
            this.parentobj = parent;
            //this.fieldInfo = fieldInfo;
            this.fpinfo = new FPInfo(fieldInfo);
            this.membertype = member_type.field;
            prefix = "=";
            //CheckItemType();
        }
        */

        public bool ReferenceExists(InspectorItem parent, object reference)
        {
            if (parent == null)
            {
                return false;
            }
            if (parent.obj == reference)
            {
                return true;
            }
            return ReferenceExists(parent.parentItem, reference);
        }

        public void GenerateChildren()
        {
            children = GenerateList(obj, this);
        }

        

        public void AddChildrenToMasterDeep()
        {
            foreach (object child in children.ToList())
            {
                InspectorItem item = (InspectorItem)child;
                masterList.Add(child);
                if (item.children.Count > 0 && item.extended)
                {
                    item.AddChildrenToMasterDeep();
                }
            }

        }
        /*
        public void RefrestMasterList()
        {
            AddMissingChildren();
            foreach (object o in masterList.ToList())
            {
                masterList.Remove(o);
            }
            AddChildrenToMasterDeep();

        }
        public void AddMissingChildren()
        {
            if (datatype == data_type.dict)
            {
                dynamic dict = obj;
                foreach (dynamic key in dict.Keys)
                {
                    //System.Console.WriteLine(key.ToString());
                    bool found = false;
                    foreach (object child in children.ToList())
                    {
                        InspectorItem item = (InspectorItem)child;
                        if (item.key.Equals(key))
                        {
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        InspectorItem iitem = new InspectorItem(masterList, this, dict[key], key);
                        if (iitem.CheckForChildren()) iitem.prefix = "+";
                        InsertItemSorted(children, iitem);
                    }

                }
            }
            else if (datatype == data_type.obj)
            {
                List<PropertyInfo> propertyInfos;
                //if the object isn't a component, then we only want to see the 'declared' properties (not inherited)
                if (!(this.obj is Component || this.obj is Player))
                {
                    propertyInfos = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
                }
                else
                {
                    propertyInfos = obj.GetType().GetProperties().ToList();
                }

                foreach (PropertyInfo pinfo in propertyInfos)
                {
                    //if (pinfo.PropertyType == typeof(Node)) continue; //don't infinitely recurse on nodes
                    bool found = false;
                    foreach (object child in children)
                    {
                        if (((InspectorItem)child).fpinfo.Name.Equals(pinfo.Name))
                        {
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        InspectorItem iitem = new InspectorItem(parentItem.masterList, this, pinfo.GetValue(obj, null), pinfo);
                        if (iitem.CheckForChildren()) iitem.prefix = "+";
                        InsertItemSorted(children, iitem);
                    }
                }
                foreach (object child in children)
                {
                    InspectorItem item = (InspectorItem)child;
                    if (item.extended)
                    {
                        item.AddMissingChildren();
                    }
                }
            }
        }
        */

        public static List<object> GenerateList(object parent, InspectorItem parentItem = null)
        {
            List<object> list = new List<object>();
            //char a = (char)164;
            //System.Console.WriteLine(a);
            //string space = "|";
            //if (parentItem != null) space += parentItem.whitespace;
            //List<FieldInfo> fieldInfos = o.GetType().GetFields().ToList(); //just supporting properties for now
            data_type dt = data_type.obj; //if this item is the root, we should give it it's real type insteam of assuming it's an object
            if (parentItem != null) dt = parentItem.datatype;

            if (dt == data_type.collection)
            {
                dynamic collection = parent;
                foreach (object o in collection)
                {
                    InspectorItem iitem = new InspectorItem(parentItem.masterList, parentItem, o);
                    if (iitem.CheckForChildren()) iitem.prefix = "+";
                    InsertItemSorted(list, iitem);
                }

            }
            else if (dt == data_type.dict)
            {
                //dynamic dict = iitem.fpinfo.GetValue(iitem.parentItem);
                dynamic dict = parent;
                foreach (dynamic key in dict.Keys)
                {
                    //System.Console.WriteLine(key.ToString());
                    InspectorItem iitem = new InspectorItem(parentItem.masterList, parentItem, dict[key], key);
                    //iitem.GenerateChildren();
                    //list.Add(iitem);
                    if (iitem.CheckForChildren()) iitem.prefix = "+";
                    InsertItemSorted(list, iitem);
                }
            }
            else if (dt == data_type.obj)
            {
                List<PropertyInfo> propertyInfos;
                //if the object isn't a component, then we only want to see the 'declared' properties (not inherited)
                if (!(parent is Component || parent is Player || parent is Process))
                {
                    propertyInfos = parent.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
                }
                else
                {
                    propertyInfos = parent.GetType().GetProperties().ToList();
                }

                foreach (PropertyInfo pinfo in propertyInfos)
                {
                    //if (pinfo.PropertyType == typeof(Node)) continue; //don't infinitely recurse on nodes

                    InspectorItem iitem = new InspectorItem(parentItem.masterList, parentItem, pinfo.GetValue(parent, null), pinfo);
                    //iitem.GenerateChildren();
                    //list.Add(iitem);
                    if (iitem.CheckForChildren()) iitem.prefix = "+";
                    InsertItemSorted(list, iitem);
                }
            }
            //if it's just a normal primitive, it will return an empty list
            if (list.Count > 0) parentItem.prefix = "+";
            return list;
        }

        public bool CheckForChildren()
        {
            if (datatype == data_type.dict)
            {
                dynamic dict = obj;
                if (dict.Count > 0) return true;
                else return false;
            }
            if (datatype == data_type.collection)
            {
                dynamic collection = obj;
                if (collection.Count > 0) return true;
                else return false;
            }
            if (datatype == data_type.obj)
            {
                List<PropertyInfo> propertyInfos = obj.GetType().GetProperties().ToList();
                if (propertyInfos.Count > 0) return true;
                else return false;
            }
            return false;
        }

        public static void InsertItemSorted(List<object> itemList, InspectorItem item)
        {
            int length = itemList.Count;
            int weight = (int)item.datatype;

            if (weight == 0)
            {
                itemList.Add(item);
                return;
            }
            for (int i = 0; i < length; i++)
            {
                if (weight < (int)((InspectorItem)itemList.ElementAt(i)).datatype)
                {
                    itemList.Insert(i, item);
                    return;
                }
            }
            itemList.Add(item);
            
        }

        public override string ToString()
        {
            string result = whitespace + prefix;

            if (membertype == member_type.dictentry)
            {
                if (obj.GetType().IsSubclassOf(typeof(Component)))
                {
                    Component component = (Component) obj;
                    return result + key.ToString().ToUpper() + " : " + component.active;
                }
                if (datatype == data_type.obj || datatype == data_type.none)
                {
                    string ts = obj.GetType().ToString().Split('.').ToList().ElementAt(obj.GetType().ToString().Split('.').ToList().Count - 1);
                    return result + key + "[" + ts + "]";
                }
                return result + key + ":" + obj;
            }
            if (membertype == member_type.collectionentry)
            {
                if (datatype == data_type.obj || datatype == data_type.none)
                {
                    PropertyInfo pinfo = obj.GetType().GetProperty("name");
                    if (pinfo != null) return pinfo.GetValue(obj, null).ToString();

                    if (obj is Link) return obj.ToString();

                    string ts = obj.GetType().ToString().Split('.').ToList().ElementAt(obj.GetType().ToString().Split('.').ToList().Count - 1);
                    return result + key + "[" + ts + "]";
                }
                return result + key + ":" + obj;
            }

            if (fpinfo != null)
            {
                if (datatype == data_type.dict)
                {
                    Type k = obj.GetType().GetGenericArguments()[0];
                    Type v = obj.GetType().GetGenericArguments()[1];
                    string ks = k.ToString().Split('.').ToList().ElementAt(k.ToString().Split('.').ToList().Count-1);
                    string vs = v.ToString().Split('.').ToList().ElementAt(v.ToString().Split('.').ToList().Count-1);
                    //System.Console.WriteLine(obj.GetType());
                    //System.Console.WriteLine(result + fpinfo.Name + " <" + ks + "," + vs + ">");
                    return result + fpinfo.Name + " <" + ks + "," + vs + ">";
                }
                if (datatype == data_type.collection)
                {
                    Type k = obj.GetType().GetGenericArguments()[0];
                    //Type v = obj.GetType().GetGenericArguments()[1];
                    string ks = k.ToString().Split('.').ToList().ElementAt(k.ToString().Split('.').ToList().Count - 1);
                    //string vs = v.ToString().Split('.').ToList().ElementAt(v.ToString().Split('.').ToList().Count - 1);
                    //System.Console.WriteLine(obj.GetType());
                    //System.Console.WriteLine(result + fpinfo.Name + " <" + ks + "," + vs + ">");
                    return result + fpinfo.Name + " <" + ks + ">";
                }

                return result + fpinfo.Name + " : " + fpinfo.GetValue(parentItem.obj);
            }
            if (obj == null)
            {
                return result + ": obj is null";
            }

            //return result + obj + " (" + obj.GetType() + ")";
            if (datatype == data_type.obj || datatype == data_type.none)
            {
                
                string ts = obj.GetType().ToString().Split('.').ToList().ElementAt(obj.GetType().ToString().Split('.').ToList().Count - 1);
                return result + "[" + ts + "]";
            }
            return result + key + ":" + obj;
            
            
            //return result + obj.ToString();
        }

        public string Name()
        {
            if (membertype == member_type.dictentry)
            {
                return key.ToString();
            }
            if (fpinfo != null)
            {
                return fpinfo.Name;
            }
            if (obj is Node)
            {
                return ((Node)obj).ToString();
            }
            return "error_Name_99";
        }

        public void RemoveChildren()
        {
            if (masterList == null) return;
            foreach (InspectorItem subitem in children.ToList())
            {
                masterList.Remove(subitem);
                foreach (InspectorItem subsub in subitem.children.ToList())
                {
                    if (masterList.Contains(subsub))
                    {
                        subitem.RemoveChildren();
                        break;
                    }
                }
                //Color c = new Color();
                //System.Drawing.Color c;
                //listComp.Items.Remove(subitem);
                //System.Drawing.Color cc = new System.Drawing.Color();
                //System.Drawing.Color.FromKnownColor(KnownColor.ActiveBorder);
                //System.Drawing.Drawing2D.
            }
        }
        public void DoubleClickItem(InspectorArea inspectorArea)
        {
            bool haschildren = hasChildren();
            if (haschildren)
            {
                if (extended)
                {
                    prefix = "+";
                    RemoveChildren();
                }
                else
                {
                    GenerateChildren();
                    prefix = "-";
                    
                    if (masterList != null)
                    {
                        inspectorArea.ActiveInspectorParent = this;
                        foreach (object item in masterList.ToList())
                        {
                            masterList.Remove(item);
                        }
                        if (parentItem != null)
                        {
                            InspectorItem uplevel = new InspectorItem(masterList, "...");
                            uplevel.parentItem = this;
                            uplevel.membertype = member_type.previouslevel;
                            masterList.Add(uplevel);
                        }
                        foreach (InspectorItem subitem in children)
                        {
                            //masterList.Insert(position + i++, subitem);
                            masterList.Add(subitem);
                        }
                    }
                }
                //extended = !extended;
            }
            else if (membertype == member_type.previouslevel)
            {
                if (parentItem != null && parentItem.parentItem != null)
                {
                    parentItem.parentItem.DoubleClickItem(inspectorArea);
                }
            }
            else if (!haschildren)
            {
                foreach (InspectorItem subitem in children)
                {
                    //masterList.Insert(position + i++, subitem);
                    masterList.Remove(subitem);
                }
            }

        }

        /*
        public void ClickItem(int position)
        {
            if (hasChildren())
            {
                if (extended)
                {
                    prefix = "+";
                    RemoveChildren();
                }
                else
                {
                    GenerateChildren();
                    prefix = "-";
                    int i = 1;
                    if (masterList != null)
                    {
                        foreach (InspectorItem subitem in children)
                        {
                            masterList.Insert(position + i++, subitem);
                        }
                    }
                }
                extended = !extended;
            }

        }
        */

        public void CheckItemType()
        {
            //Type t = fpinfo.FPType();
            //Type t = obj.GetType();
            data_type dt = data_type.none;

            if (obj is int)
            {
                dt = data_type.integer;
            }
            else if (obj is Single)
            {
                dt = data_type.single;
            }
            else if (obj is String)
            {
                dt = data_type.str;
            }
            else if (obj is bool)
            {
                dt = data_type.boolean;
            }
            else if (obj is byte)
            {
                dt = data_type.tbyte;
            }
            else if (obj == null)
            {
                //Console.WriteLine("Object was null when checking inspectoritem obj type.");
            }
            else if (obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                //System.Console.WriteLine("Dictionary found.");
                dt = data_type.dict;

                //Type keyType = t.GetGenericArguments()[0];
                //Type valueType = t.GetGenericArguments()[1];
            }
            else if (obj.GetType().GetInterfaces()
                .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                //Console.WriteLine("IEnumerable : {0}", obj.GetType());
                dt = data_type.collection;
            }
            //might need to be more specific than List
            else if (obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(List<>))
            {
                Console.WriteLine("List(What?)");
                //System.Console.WriteLine("List found.");
                dt = data_type.list;

                //Type valueType = t.GetGenericArguments()[0];

            }
            else if (obj is Component)
            {
                dt = data_type.obj;
            }
            else
            {

                //foreach (Type type in ValidTypes)
                //{
                //    if (obj.GetType() == type)
                //    {
                //        dt = data_type.obj;
                //    }
                //}
                dt = data_type.obj; //this should be ok (we don't need to specific validtypes anymore)


                //datatype = data_type.none; //support more types in the future
            }

            datatype = dt;

        }

        public bool IsPanelType()
        {
            foreach (Type type in PanelTypes)
            {
                if (obj.GetType() == type)
                {
                    return true;
                }
                if (obj.GetType().IsSubclassOf(type))
                {
                    return true;
                }
            }
            return false;
        }

        public object GetValue()
        {
            object result = null;

            if (membertype == member_type.dictentry)
            {
                dynamic dict = parentItem.obj;
                dynamic KEY = key;
                return dict[KEY];
            }

            if (fpinfo == null)
            {
                System.Console.WriteLine("parent object is null");
                return null;
            }

            result = fpinfo.GetValue(parentItem.obj);

            return result;

        }

        public void SetValue(object value)
        {
            //if (HasPanelElements()) //must be primitive or enum

            if (membertype == member_type.dictentry)
            {
                
                
                //holy shit that's dynamic.
                dynamic dict = parentItem.obj;
                dynamic KEY = key;
                dynamic VALUE = value;

                if (!dict.GetType().IsGenericType || dict.GetType().GetGenericTypeDefinition() != typeof(Dictionary<,>))
                {
                    System.Console.WriteLine("Error: The parentItem wasn't a dictionary.");
                    return;
                }
                Type keytype = dict.GetType().GetGenericArguments()[0];
                Type valuetype = dict.GetType().GetGenericArguments()[1];
                if ((KEY.GetType() != keytype && !KEY.GetType().IsSubclassOf(keytype)) && keytype != typeof(object))
                {
                    System.Console.WriteLine("Error: The key type didn't match the parent dictionary. ({0} != {1})", KEY.GetType(), keytype);
                    return;
                }
                if ((VALUE.GetType() != valuetype && !VALUE.GetType().IsSubclassOf(valuetype)) && valuetype != typeof(object))
                {
                    System.Console.WriteLine("Error: The value type didn't match the parent dictionary. ({0} != {1})", VALUE.GetType(), valuetype);
                    return;
                }
                dict[KEY] = VALUE;
                obj = VALUE;

            }
            else if (fpinfo != null)
            {
                fpinfo.SetValue(value, parentItem.obj);
            }
            else
            {
                System.Console.WriteLine("Error while SetValue() in InspectorItem.");
            }

        }

        

        public bool HasPanelElements()
        {
            if (obj == null)
            {
                Console.WriteLine("object was null when checking if InspectorItem HasPanelElements()");
                return false;
            }

            Type itemtype = obj.GetType();
            foreach (Type type in PanelTypes)
            {
                if (itemtype == type)
                {
                    return true;
                }
                if (itemtype.IsSubclassOf(type))
                {
                    return true;
                }
            }
            return false;
        }

        public bool hasChildren()
        {
            return CheckForChildren();
        }

        public int childCount()
        {
            if (children != null)
            {
                return children.Count;
            }
            return 0;
        }

    }
}
