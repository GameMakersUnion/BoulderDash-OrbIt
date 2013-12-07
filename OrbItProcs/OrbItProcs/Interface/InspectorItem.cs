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
using System.Collections.ObjectModel;

namespace OrbItProcs.Interface {

    public enum member_type {
        none,
        field,
        property,
        dictentry,
        unimplemented,
        
    };

    public enum data_type {
        none,
        integer,
        single,
        str,
        boolean,
        dict,
        list,
        obj,
        enm,
    };


    public class InspectorItem {

        public static List<Type> ValidTypes = new List<Type>()
        {
            typeof(Node),
            typeof(Component),
            typeof(ModifierInfo),
            typeof(Vector2),
            typeof(Color),
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
            CheckItemType();
            prefix = "=";
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
            CheckItemType();
            prefix = "=";
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
            Type t = parentItem.obj.GetType();
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                membertype = member_type.dictentry;
                this.key = key;
                CheckItemType();
                prefix = "=";
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
            if (ReferenceExists(parentItem, obj))
            {
                children = new List<object>();
                return;
            }
            //System.Console.WriteLine(this);
            children = GenerateList(obj, whitespace, this);
        }

        public static List<object> GenerateList(object parent, String whiteSpace, InspectorItem parentItem = null)
        {
            List<object> list = new List<object>();
            string space = "|" + whiteSpace;
            //List<FieldInfo> fieldInfos = o.GetType().GetFields().ToList(); //just supporting properties for now
            data_type dt = data_type.obj;
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
                    iitem.GenerateChildren();
                    list.Add(iitem);
                }
            }
            else if (dt == data_type.obj)
            {
                List<PropertyInfo> propertyInfos = parent.GetType().GetProperties().ToList();

                foreach (PropertyInfo pinfo in propertyInfos)
                {
                    if (pinfo.PropertyType == typeof(Node)) continue; //don't infinitely recurse on nodes

                    InspectorItem iitem = new InspectorItem(parentItem.masterList, pinfo.GetValue(parent, null), pinfo, parentItem, space);
                    iitem.GenerateChildren();
                    list.Add(iitem);
                }
            }
            //if it's just a normal primitive, it will return an empty list
            return list;
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

                return result + key + ":" + obj;
            }

            if (fpinfo != null)
            {
                return result + fpinfo.Name + " : " + fpinfo.GetValue(parentItem.obj);
            }
            if (obj == null)
            {
                return result + ": obj is null";
            }

            return result + obj + " (" + obj.GetType() + ")";
            
            
            
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

        public void ClickItem(int position)
        {
            if (hasChildren())
            {
                if (extended)
                {
                    prefix = "+";
                    foreach (InspectorItem subitem in children)
                    {
                        masterList.Remove(subitem);
                        //listComp.Items.Remove(subitem);
                    }
                }
                else
                {
                    GenerateChildren();
                    prefix = "-";
                    int i = 1;
                    foreach (InspectorItem subitem in children)
                    {
                        masterList.Insert(position + i++, subitem);
                        //listComp.Items.Insert(listComp.ItemIndex + i++, subitem);
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

        public object GetValue()
        {
            object result = null;

            if (parentobj == null)
            {
                System.Console.WriteLine("parent object is null");
                return null;
            }

            result = fpinfo.GetValue(parentobj);

            return result;

        }

        public void SetValue(object value)
        {
            //if (HasPanelElements()) //must be primitive or enum

            if (membertype == member_type.dictentry)
            {
                dynamic dict = parentItem.obj;
                dict[key] = value;
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
            if (children != null)
            {
                return (children.Count > 0);
            }
            return false;
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
