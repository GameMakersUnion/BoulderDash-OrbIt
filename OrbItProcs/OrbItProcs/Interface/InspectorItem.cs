using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TomShane.Neoforce.Controls;
using System.Reflection;


namespace OrbItProcs.Interface {

    public enum member_type {
        field,
        property,
        none,
    };

    public enum data_type {
        integer,
        single,
        str,
        boolean,
        dict,
        list,
        obj,
        none,
    };


    public class InspectorItem {

        public treeitem itemtype;
        public int depth = 1;
        public FieldInfo fieldInfo;
        public PropertyInfo propertyInfo;
        public comp component;
        public bool hasChildren = false;
        public bool extended = false;
        
        public List<object> children;
        public int childCount = 0;
        public dynamic key;
        public String prefix;
        public String whitespace = "";

        public object obj;
        public object parentobj;
        public InspectorItem parentItem;
        public member_type membertype;
        public data_type datatype;

        public InspectorItem(object parent, FieldInfo fieldInfo, InspectorItem parentItem)
        {
            this.parentobj = parent;
            this.fieldInfo = fieldInfo;
            this.membertype = member_type.field;
            prefix = "=";

            CheckItemType();


        }


        public void CheckItemType()
        {
            Type t = null;
            if (fieldInfo != null)
            {
                t = fieldInfo.FieldType;
            }
            else if (propertyInfo != null)
            {
                t = propertyInfo.PropertyType;
            }

            string n = t.ToString();

            if (n.Equals("System.Int32"))
            {
                datatype = data_type.integer;
            }
            else if (n.Equals("System.Single"))
            {
                datatype = data_type.single;
            }
            else if (n.Equals("System.String"))
            {
                datatype = data_type.str;
            }
            else if (n.Equals("System.Boolean"))
            {
                datatype = data_type.boolean;
            }
            else if (n.Contains("Dictionary"))
            {
                System.Console.WriteLine("Dictionary found.");
                datatype = data_type.dict;

            }
            else if (n.Contains("List"))
            {
                System.Console.WriteLine("List found.");
                datatype = data_type.list;

            }
            else
            {
                datatype = data_type.none; //support more types in the future
            }


        }

        public object GetValue()
        {
            object result = null;

            if (parentobj == null)
            {
                System.Console.WriteLine("parent object is null");
                return null;
            }

            if (membertype == member_type.field)
            {
                result = fieldInfo.GetValue(parentobj);
            }
            else if (membertype == member_type.property)
            {
                result = propertyInfo.GetValue(parentobj, null);
            }

            return result;
        
        }
        
        public static List<object> GenerateList(Component c, String whiteSpace)
        {
            List<object> list = new List<object>();
            List<FieldInfo> fieldInfos = c.GetType().GetFields().ToList();

            List<PropertyInfo> propertyInfos = c.GetType().GetProperties().ToList();
            //extend this to check for properties as well
            foreach (PropertyInfo property in propertyInfos)
            {
                TreeListItem item = new TreeListItem(property, c);
                item.whitespace = whiteSpace + " |-->";
                item.prefix = "=";
                list.Add(item);
            }
            foreach (FieldInfo field in fieldInfos)
            {
                TreeListItem item = new TreeListItem(field,c);
                if (field.Name.Equals("sentinel")) break;
                item.whitespace = whiteSpace + " |-->";
                item.prefix = "=";
                list.Add(item);
            }
            return list;
        }

        public static List<object> GenerateList(Node n, String whiteSpace)
        {
            List<object> list = new List<object>();
            List<FieldInfo> fieldInfos = n.GetType().GetFields().ToList();
            List<PropertyInfo> propertyInfos = n.GetType().GetProperties().ToList();

            foreach (comp c in Enum.GetValues(typeof(comp)))
            {
                if (n.props.ContainsKey(c))
                {
                    TreeListItem item = new TreeListItem(n, c, whiteSpace);
                    item.whitespace = whiteSpace;
                    list.Add(item);
                }
            }
            foreach (PropertyInfo property in propertyInfos)
            {
                TreeListItem item = new TreeListItem(n, property);
                //System.Console.WriteLine(property.PropertyType);
                if (property.Name.Equals("sentinelp")) break;
                list.Add(item);
            }
            foreach (FieldInfo field in fieldInfos)
            {
                TreeListItem item = new TreeListItem(n, field);
                if (field.Name.Equals("sentinel")) break;
                list.Add(item);
            }
            
            return list;
        }

        public String getName()
        {
            String result = "";
            if (this.fieldInfo != null)
            {
                result = fieldInfo.Name;
            }
            else if (this.propertyInfo != null)
            {
                result = propertyInfo.Name;
            }
            else
            { 
                // add in other cases such as dictionary or Objects
            }


            return result;
        }

        /*
        public override String ToString()
        {
            String result = whitespace + prefix;

            if (itemtype == treeitem.objfieldinfo)
            {
                if (obj == null) return "obj is null";
            }
            else if (itemtype == treeitem.objpropertyinfo)
            {
                if (obj == null) return "obj is null";
            }
            else if (node == null) return "node is null";

            if (itemtype == treeitem.fieldinfo)
            {
                if (fieldInfo == null) return "fieldInfo is null";
                result += "(F)" + fieldInfo.Name + " " + fieldInfo.GetValue(node);


            }
            else if (itemtype == treeitem.propertyinfo)
            {
                if (propertyInfo == null) return "propertyInfo is null";
                result += "(P)" + propertyInfo.Name + " " + propertyInfo.GetValue(node,null);


            }
            else if (itemtype == treeitem.component)
            {
                if (!node.props.ContainsKey(component)) return "component not in node's properties dictionary";
                if (!node.comps.ContainsKey(component)) return "component not in node's components dictionary";
                result += "(C)" + component.ToString() + " " + node.props[component];
            }
            else if (itemtype == treeitem.objfieldinfo)
            {
                if (fieldInfo == null) return "fieldInfo is null";
                if (obj == null) return "parent object is null";
                result += "(F)" + fieldInfo.Name + " " + fieldInfo.GetValue(obj);
            }
            else if (itemtype == treeitem.objpropertyinfo)
            {
                if (propertyInfo == null) return "propertyInfo is null";
                if (obj == null) return "parent object is null";
                result += "(P)" + propertyInfo.Name + " " + propertyInfo.GetValue(obj,null);
            }
            return result;
            //Todo: implement cases for treeitem.dictionary and treeitem.obj later
        }
        */
    }
}
