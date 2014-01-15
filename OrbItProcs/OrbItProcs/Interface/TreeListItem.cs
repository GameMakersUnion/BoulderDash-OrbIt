using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TomShane.Neoforce.Controls;
using System.Reflection;
using Component = OrbItProcs.Component;

namespace OrbItProcs {

    public enum treeitem {
        fieldinfo,
        propertyinfo,
        objfieldinfo,
        objpropertyinfo,
        dictentry,
        component,
        obj,
    };

    public class TreeListItem {
        public treeitem itemtype;
        public int depth = 1;
        public FieldInfo fieldInfo;
        public PropertyInfo propertyInfo;
        public Node node;
        //public Dictionary<dynamic, dynamic> dict;
        public comp component;
        public bool hasChildren = false;
        public bool extended = false;
        public List<object> children;
        public int childCount = 0;
        public dynamic key;
        public String prefix;
        public String whitespace = "";
        public object obj;

        public TreeListItem(Node node, FieldInfo fieldInfo)
        {
            this.node = node;
            this.obj = node;
            this.fieldInfo = fieldInfo;
            itemtype = treeitem.fieldinfo;
            //check if this field has children, and convert to obj if so
            prefix = "=";
        }
        public TreeListItem(Node node, PropertyInfo propertyInfo)
        {
            this.node = node;
            this.obj = node;
            this.propertyInfo = propertyInfo;
            itemtype = treeitem.propertyinfo;
            prefix = "=";
        }
        public TreeListItem(Node node, comp c, String whiteSpace)
        {
            this.node = node;
            this.obj = node;
            component = c;
            itemtype = treeitem.component;
            prefix = "+";
            Component comp = node.comps[c];
            children = GenerateList(comp, whiteSpace);
            childCount = children.Count;
            hasChildren = true;
        }
        /*
        public TreeListItem(Node node, dynamic key)
        {
            this.node = node;
            this.key = key;
            itemtype = treeitem.dictentry;
            prefix = "~"; // should be "="
        }
        */
        public TreeListItem(FieldInfo fieldInfo, object obj)
        {
            this.obj = obj;
            this.fieldInfo = fieldInfo;
            itemtype = treeitem.objfieldinfo;
            //check if this field has children, and convert to obj if so
            prefix = "=";
        }
        public TreeListItem(PropertyInfo propertyInfo, object obj)
        {
            this.obj = obj;
            this.propertyInfo = propertyInfo;
            itemtype = treeitem.objpropertyinfo;
            //check if this field has children, and convert to obj if so
            prefix = "=";
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

            /*
            foreach (comp c in Enum.GetValues(typeof(comp)))
            {
                if (n.props.ContainsKey(c))
                {
                    TreeListItem item = new TreeListItem(n, c, whiteSpace);
                    item.whitespace = whiteSpace;
                    list.Add(item);
                }
            }
             */
            foreach (comp c in n.comps.Keys)
            {
                TreeListItem item = new TreeListItem(n, c, whiteSpace);
                item.whitespace = whiteSpace;
                list.Add(item);
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
                if (field.Name.Equals("_sentinel")) break;
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
                //if (!node.props.ContainsKey(component)) return "component not in node's properties dictionary";
                if (!node.comps.ContainsKey(component)) return "component not in node's components dictionary";
                result += "(C)" + component.ToString() + " " + node.isCompActive(component);
            }
            else if (itemtype == treeitem.objfieldinfo)
            {
                if (fieldInfo == null) return "fieldInfo is null";
                if (obj == null) return "parent object is nulltt";
                result += "(F)" + fieldInfo.Name + " " + fieldInfo.GetValue(obj);
            }
            else if (itemtype == treeitem.objpropertyinfo)
            {
                if (propertyInfo == null) return "propertyInfo is null";
                if (obj == null) return "parent object is nullttt";
                result += "(P)" + propertyInfo.Name + " " + propertyInfo.GetValue(obj,null);
            }
            return result;
            //Todo: implement cases for treeitem.dictionary and treeitem.obj later
        }

    }



    //remember to clean up the Type display format (it gets too long)
}
