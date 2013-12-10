using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TomShane.Neoforce.Controls;
using System.Reflection;
using OrbItProcs.Processes;
using OrbItProcs.Components;
using Component = OrbItProcs.Components.Component;
using Console = System.Console;
using System.Collections.ObjectModel;

namespace OrbItProcs.Interface {

    public enum member_type {
        none,
        field,
        property,
        dictentry,
        unimplemented,
        
    };
    //listed in the order that they will appear in the listbox
    public enum data_type {
        none,
        dict,
        list,
        enm,
        boolean,
        integer,
        single,
        str,
        obj,
    };

    public class InspectorItem {

        public static List<Type> ValidTypes = new List<Type>()
        {
            typeof(Node),
            typeof(Component),
            typeof(ModifierInfo),
            typeof(Vector2),
            typeof(Color),
            typeof(Game1),
            typeof(Room),
            typeof(GridSystem),
        };
        public static List<Type> PanelTypes = new List<Type>()
        {
            typeof(int),
            typeof(float),
            typeof(double),
            typeof(string),
            typeof(bool),
            typeof(Enum),
        };


        public treeitem itemtype;
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
        public InspectorItem(IList<object> masterList, object obj, string whitespace)
        {
            this.whitespace = whitespace;
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

        public InspectorItem(IList<object> masterList, object obj, PropertyInfo propertyInfo, InspectorItem parentItem, string whitespace)
        {
            this.whitespace = whitespace;
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
        public InspectorItem(IList<object> masterList, InspectorItem parentItem, string whitespace, object key, object obj = null)
        {
            this.whitespace = whitespace;
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
                System.Console.WriteLine("Unexpected: InspectorItem with no obj refernce was not a dictionary entry");
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
            /*
            if (ReferenceExists(parentItem, obj))
            {
                children = new List<object>();
                return;
            }
            */
            //System.Console.WriteLine(this);
            children = GenerateList(obj, whitespace, this);
        }

        public static List<object> GenerateList(object parent, String whiteSpace, InspectorItem parentItem = null)
        {
            List<object> list = new List<object>();
            List<int> weightsList = new List<int>();
            //char a = (char)164;
            //System.Console.WriteLine(a);
            string space = "|" + whiteSpace;
            //List<FieldInfo> fieldInfos = o.GetType().GetFields().ToList(); //just supporting properties for now
            data_type dt = data_type.obj; //if this item is the root, we should give it it's real type insteam of assuming it's an object
            if (parentItem != null) dt = parentItem.datatype;

            if (dt == data_type.list)
            {
                //do nothing for now
            }
            else if (dt == data_type.dict)
            {
                //dynamic dict = iitem.fpinfo.GetValue(iitem.parentItem);
                dynamic dict = parent;
                foreach (dynamic key in dict.Keys)
                {
                    //System.Console.WriteLine(key.ToString());
                    InspectorItem iitem = new InspectorItem(parentItem.masterList, parentItem, space, key, dict[key]);
                    //iitem.GenerateChildren();
                    //list.Add(iitem);
                    if (iitem.CheckForChildren()) iitem.prefix = "+";
                    InsertItemSorted(list, weightsList, iitem);
                }
            }
            else if (dt == data_type.obj)
            {
                List<PropertyInfo> propertyInfos;
                //if the object isn't a component, then we only want to see the 'declared' properties (not inherited)
                if (!(parent is Component))
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

                    InspectorItem iitem = new InspectorItem(parentItem.masterList, pinfo.GetValue(parent, null), pinfo, parentItem, space);
                    //iitem.GenerateChildren();
                    //list.Add(iitem);
                    if (iitem.CheckForChildren()) iitem.prefix = "+";
                    InsertItemSorted(list, weightsList, iitem);
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
            if (datatype == data_type.obj)
            {
                List<PropertyInfo> propertyInfos = obj.GetType().GetProperties().ToList();
                if (propertyInfos.Count > 0) return true;
                else return false;
            }
            return false;
        }

        public static void InsertItemSorted(List<object> objList, List<int> weightsList, InspectorItem item)
        {
            int length = objList.Count;
            int weight = (int)item.datatype;

            if (weight == 0)
            {
                weightsList.Add(weight);
                objList.Add(item);
                return;
            }
            
            for (int i = 0; i < length; i++)
            {
                if (weight < weightsList.ElementAt(i))
                {
                    weightsList.Insert(i, weight);
                    objList.Insert(i, item);
                    return;
                }
            }
            weightsList.Add(weight);
            objList.Add(item);
            
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
                
                //listComp.Items.Remove(subitem);
            }
        }

        public void ClickItem(int position)
        {
            if (hasChildren())
            {
                if (extended)
                {
                    prefix = "+";
                    RemoveChildren();
                    /*
                    foreach (InspectorItem subitem in children.ToList())
                    {
                        masterList.Remove(subitem);
                        //listComp.Items.Remove(subitem);
                    }
                    */
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
                            //listComp.Items.Insert(listComp.ItemIndex + i++, subitem);
                        }
                    }
                }
                extended = !extended;
            }

        }

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
            else if (obj == null)
            {
                Console.WriteLine("Object was null when checking inspectoritem obj type.");
            }
            else if (obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                //System.Console.WriteLine("Dictionary found.");
                dt = data_type.dict;

                //Type keyType = t.GetGenericArguments()[0];
                //Type valueType = t.GetGenericArguments()[1];
            }
            //might need to be more specific than List
            else if (obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(List<>))
            {
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
                foreach (Type type in ValidTypes)
                {
                    if (obj.GetType() == type)
                    {
                        dt = data_type.obj;
                    }
                }
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

            /*
            if (children != null)
            {
                return (children.Count > 0);
            }
            return false;
            */
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
