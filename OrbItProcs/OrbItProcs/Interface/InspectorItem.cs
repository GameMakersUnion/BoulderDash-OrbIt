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
        obj,
        collection,
        array,
        list,
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
        //public InspectorArea inspectorArea;
        public Sidebar sidebar;
        //public treeitem itemtype;
        public int depth = 1;
        //public FieldInfo fieldInfo;
        //public PropertyInfo propertyInfo;
        public FPInfo fpinfo;
        //public comp component;

        public bool extended = false;
        public bool showValueToString = false;
        
        public List<InspectorItem> children;
        
        //public int childCount = 0;
        public object key;
        public String prefix;
        public String whitespace = "";

        public string ToolTip = "";

        public object obj;
        public object parentobj;
        private InspectorItem _parentItem;
        public InspectorItem parentItem { get { return _parentItem; } set { _parentItem = value; if (value != null) showValueToString = parentItem.showValueToString; } }
        public member_type membertype;
        public data_type datatype;

        public IList<object> masterList;

        //root item
        public InspectorItem(IList<object> masterList, object obj, Sidebar sidebar, bool showValueToString = false)
        {
            this.showValueToString = showValueToString;
            this.whitespace = "|";
            this.obj = obj;
            this.masterList = masterList; 
            //this.fpinfo = new FPInfo(propertyInfo);
            this.membertype = member_type.none;
            this.children = new List<InspectorItem>();
            //this.inspectorArea = insArea;
            this.sidebar = sidebar;
            CheckItemType();
            prefix = "" + ((char)164);
            //System.Console.WriteLine(obj);
            //children = GenerateList(obj, whitespace, this); 
            
        }
        //a property
        public InspectorItem(IList<object> masterList, InspectorItem parentItem, object obj, PropertyInfo propertyInfo)
        {
            this.membertype = member_type.property;
            this.fpinfo = new FPInfo(propertyInfo);
            FieldOrPropertyInitilize(masterList, parentItem, obj);
        }
        //a field
        public InspectorItem(IList<object> masterList, InspectorItem parentItem, object obj, FieldInfo fieldInfo)
        {
            this.membertype = member_type.field;
            this.fpinfo = new FPInfo(fieldInfo);
            FieldOrPropertyInitilize(masterList, parentItem, obj);
        }
        private void FieldOrPropertyInitilize(IList<object> masterList, InspectorItem parentItem, object obj)
        {
            this.whitespace = "|";
            if (parentItem != null) this.whitespace += parentItem.whitespace;
            this.obj = obj;
            this.parentItem = parentItem;
            this.masterList = masterList;
            this.children = new List<InspectorItem>();
            CheckItemType();
            prefix = "" + ((char)164);
            //this.inspectorArea = parentItem.inspectorArea;
            this.sidebar = parentItem.sidebar;
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
            this.children = new List<InspectorItem>();
            //this.inspectorArea = parentItem.inspectorArea;
            this.sidebar = parentItem.sidebar;

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
            this.children = new List<InspectorItem>();
            //this.inspectorArea = parentItem.inspectorArea;
            this.sidebar = parentItem.sidebar;

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

        public void GenerateChildren(bool GenerateFields = false)
        {
            children = GenerateList(obj, this, GenerateFields);
        }

        

        public void AddChildrenToMasterDeep()
        {
            foreach (object child in children.ToList())
            {
                InspectorItem item = (InspectorItem)child;
                if (masterList != null) masterList.Add(child);
                if (item.children.Count > 0 && item.extended)
                {
                    item.AddChildrenToMasterDeep();
                }
            }

        }

        public static List<InspectorItem> GenerateList(object parent, InspectorItem parentItem = null, bool GenerateFields = false)
        {
            List<InspectorItem> list = new List<InspectorItem>();
            //char a = (char)164;
            //System.Console.WriteLine(a);
            //string space = "|";
            //if (parentItem != null) space += parentItem.whitespace;
            //List<FieldInfo> fieldInfos = o.GetType().GetFields().ToList(); //just supporting properties for now
            data_type dt = data_type.obj; //if this item is the root, we should give it it's real type in.steam of assuming it's an object
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
            else if (dt == data_type.array)
            {
                dynamic array = parent;
                foreach (object o in array)
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
                ///// PROPERTIES
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

                UserLevel userlevel = Program.getGame().ui.sidebar.userLevel;

                foreach (PropertyInfo pinfo in propertyInfos)
                {
                    string tooltip = "";
                    object[] attributes = pinfo.GetCustomAttributes(false);
                    var abstractions = pinfo.GetCustomAttributes(typeof(Info), false);
                    if (abstractions.Length > 0)
                    {
                        Info info = (Info)abstractions[0];
                        if ((int)info.userLevel > (int)userlevel) continue;
                        tooltip = info.summary;

                    }
                    else if (userlevel != UserLevel.Debug)
                    {
                        continue;
                    }
                    //if (pinfo.Name.Equals("Item")) continue;
                    InspectorItem iitem = new InspectorItem(parentItem.masterList, parentItem, pinfo.GetValue(parent, null), pinfo);
                    if (tooltip.Length > 0) iitem.ToolTip = tooltip;
                    if (iitem.CheckForChildren()) iitem.prefix = "+";
                    InsertItemSorted(list, iitem);
                }
                ////// FIELDS
                    List<FieldInfo> fieldInfos;
                    //if the object isn't a component, then we only want to see the 'declared' properties (not inherited)
                    if (!(parent is Component || parent is Player || parent is Process))
                    {
                        fieldInfos = parent.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
                    }
                    else
                    {
                        fieldInfos = parent.GetType().GetFields().ToList();
                    }

                    foreach (FieldInfo finfo in fieldInfos)
                    {
                        //if (finfo.GetCustomAttributes(typeof(DoNotInspect), false).Length > 0) continue;
                        var abstractions = finfo.GetCustomAttributes(typeof(Info), false);
                        if (abstractions.Length > 0)
                        {
                            if ((int)(abstractions[0] as Info).userLevel > (int)userlevel) continue;
                        }
                        else if (userlevel != UserLevel.Debug)
                        {
                            continue;
                        }
                        InspectorItem iitem = new InspectorItem(parentItem.masterList, parentItem, finfo.GetValue(parent), finfo);
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
                //he tried..
            }
            if (datatype == data_type.array)
            {
                dynamic array = obj;

                if (array.Length > 0) return true;
                else return false;
                //he tried..
            }
            if (datatype == data_type.array)
            {
                dynamic array = obj;

                if (array.Length > 0) return true;
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

        public static void InsertItemSorted(List<InspectorItem> itemList, InspectorItem item)
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
                int itemweight = (int)((InspectorItem)itemList.ElementAt(i)).datatype;
                if (weight < itemweight)
                {
                    itemList.Insert(i, item);
                    return;
                }
            }
            itemList.Add(item);
            
        }

        public override string ToString()
        {
            //string result = whitespace + prefix;
            string result = "";

            if (obj is Vector2)
            {
                dynamic vect = obj;
                result += fpinfo.Name;
                if (showValueToString) result += string.Format(" : X: {0:0} | Y: {1:0}", vect.X, vect.Y);
                return result;

            }

            if (membertype == member_type.dictentry)
            {
                if (obj.GetType().IsSubclassOf(typeof(Component)))
                {
                    Component component = (Component) obj;
                    result += component.GetType();
                    if (showValueToString) result += " : " + component.active;
                    return result;
                }
                if (datatype == data_type.obj || datatype == data_type.none)
                {
                    string ts = obj.GetType().ToString().LastWord('.');
                    return result + key + "[" + ts + "]";
                }
                result += key;
                if (showValueToString) result += ":" + obj;
                return result;
            }
            if (membertype == member_type.collectionentry)
            {
                if (datatype == data_type.obj || datatype == data_type.none)
                {
                    PropertyInfo pinfo = obj.GetType().GetProperty("name");
                    if (pinfo != null) return pinfo.GetValue(obj, null).ToString();

                    if (obj is Link) return obj.ToString();

                    if (obj is Group) return result + (obj as Group).Name;

                    string ts = obj.GetType().ToString().LastWord('.');
                    return result + key + "[" + ts + "]";
                }
                return result + key + ":" + obj;
            }
            if (fpinfo != null)
            {
                if (datatype == data_type.dict)
                {
                    if (fpinfo.Name.Equals("comps")) return result + "components";

                    Type k = obj.GetType().GetGenericArguments()[0];
                    Type v = obj.GetType().GetGenericArguments()[1];
                    string ks = k.ToString().LastWord('.');
                    string vs = v.ToString().LastWord('.');
                    //System.Console.WriteLine(obj.GetType());
                    //System.Console.WriteLine(result + fpinfo.Name + " <" + ks + "," + vs + ">");
                    result += fpinfo.Name;
                    if (showValueToString) result += " <" + ks + "," + vs + ">";
                    return result;
                }
                if (datatype == data_type.array)
                {
                    //Type[] gen = obj.GetType().GetGenericArguments();
                    //obj.GetType()
                    string k = obj.GetType().GetElementType().ToString().LastWord('.');
                    result += fpinfo.Name;
                    if (showValueToString) result += " <" + k + ">";
                    return result;
                }
                if (datatype == data_type.collection)
                {
                    if (obj.GetType().GetGenericArguments().Length == 0) return result + fpinfo.Name;
                    string k = obj.GetType().GetGenericArguments()[0].ToString().LastWord('.');
                    //Type v = obj.GetType().GetGenericArguments()[1];
                    //string ks = k;
                    //string vs = v.ToString().Split('.').ToList().ElementAt(v.ToString().Split('.').ToList().Count - 1);
                    //System.Console.WriteLine(obj.GetType());
                    //System.Console.WriteLine(result + fpinfo.Name + " <" + ks + "," + vs + ">");
                    //return result + fpinfo.Name + " <" + ks + ">";
                    result += fpinfo.Name;
                    if (showValueToString) result += " <" + k + ">";
                    return result;
                }
                if (obj != null && obj.GetType().IsSubclassOf(typeof(Component)))
                {
                    Component component = (Component)obj;
                    result += component.GetType().ToString().ToUpper().LastWord('.');
                    if (showValueToString) result += " : " + component.active;
                    return result;
                }
                if (datatype == data_type.obj || datatype == data_type.none)
                {
                    string ts = obj.GetType().ToString().LastWord('.');
                    result += fpinfo.Name;
                    if (showValueToString) result += "[" + ts + "]";
                    return result;
                }
                result += fpinfo.Name;
                if (showValueToString) result += " : " + fpinfo.GetValue(parentItem.obj);
                return result;
            }
            if (obj == null)
            {
                return result + ": -null";
            }

            //return result + obj + " (" + obj.GetType() + ")";
            
            return result + obj;
            
            
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
            if (obj is Link)
            {
                return ((Link)obj).ToString();
            }
            return "error99";
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
            if (inspectorArea.sidebar.userLevel == UserLevel.User || inspectorArea.sidebar.userLevel == UserLevel.Advanced) return;

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
                            InspectorItem uplevel = new InspectorItem(masterList, "...", sidebar);
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
                if (masterList != null)
                {
                foreach (InspectorItem subitem in children)
                {
                    //masterList.Insert(position + i++, subitem);
                    masterList.Remove(subitem);
                }
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

            if (obj == null)
            {
                //Console.WriteLine("Object was null when checking inspector.item obj type.");
            }
            else if (obj is int)
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
            else if (obj.GetType().IsSubclassOf(typeof(Enum)))
            {
                dt = data_type.enm;
            }
            //else if (obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
            else if (obj.GetType().GetInterface("IDictionary") != null)
            {
                //System.Console.WriteLine("Dictionary found.");
                dt = data_type.dict;

                //Type keyType = t.GetGenericArguments()[0];
                //Type valueType = t.GetGenericArguments()[1];
            }
            else if (obj is Array)
            {
                dt = data_type.array;
            }
            else if (obj.GetType().GetInterfaces()
                .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
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
            if (membertype == member_type.dictentry)
            {
                //holy shit that's dynamic.
                dynamic dict = parentItem.obj;
                dynamic KEY = key;
                dynamic VALUE = value;

                if (!dict.GetType().IsGenericType || dict.GetType().GetGenericTypeDefinition() != typeof(Dictionary<,>))
                {
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
        public void SetValueSafe(object value)
        {
            object san = TrySanitize(value);
            if (san != null)
                SetValue(value);
            //todo: fix for dictionaries / lists
        }

        public object TrySanitize(object value)
        {
            if (value is string && fpinfo.FPType != typeof(string))
            {
                string s = value.ToString().Trim();

                if (fpinfo.FPType == typeof(int))
                {
                    int v;
                    if (Int32.TryParse(s, out v))
                    {
                        fpinfo.SetValue(v, parentItem.obj);
                        return v;
                    }
                    else return null;
                }
                else if (fpinfo.FPType == typeof(float))
                {
                    float v;
                    if (float.TryParse(s, out v))
                    {
                        fpinfo.SetValue(v, parentItem.obj);
                        return v;
                    }
                    else return null;
                }
                else if (fpinfo.FPType == typeof(double))
                {
                    double v;
                    if (double.TryParse(s, out v))
                    {
                        fpinfo.SetValue(v, parentItem.obj);
                        return v;
                    }
                    else return null;
                }
                else if (fpinfo.FPType == typeof(byte))
                {
                    byte v;
                    if (byte.TryParse(s, out v))
                    {
                        fpinfo.SetValue(v, parentItem.obj);
                        return v;
                    }
                    else return null;
                }
            }
            else if (value is string)
            {
                value = value.ToString().Trim();
            }
            return value;
        }

        public void BuildItemsPath(InspectorItem item, List<InspectorItem> itemspath)
        {
            InspectorItem temp = item;
            itemspath.Insert(0, temp);
            while (temp.parentItem != null)
            {
                temp = temp.parentItem;
                itemspath.Insert(0, temp);
            }
        }

        public void ApplyToAllNodes(Group group)
        {
            if (group == null) return;
            List<InspectorItem> itemspath = new List<InspectorItem>();
            InspectorItem item = this;
            object value = item.GetValue();

            BuildItemsPath(item, itemspath);

            group.ForEachAllSets(delegate(Node n)
            {
                if (n == itemspath.ElementAt(0).obj) return;
                InspectorItem temp = new InspectorItem(null, n, sidebar);
                int count = 0;
                foreach (InspectorItem pathitem in itemspath)
                {
                    if (temp.obj.GetType() != pathitem.obj.GetType())
                    {
                        Console.WriteLine("The paths did not match while applying to all. {0} != {1}", temp.obj.GetType(), pathitem.obj.GetType());
                        break;
                    }
                    if (count == itemspath.Count - 1) //last item
                    {
                        if (pathitem.membertype == member_type.dictentry)
                        {
                            dynamic dict = temp.parentItem.obj;
                            dynamic key = pathitem.key;
                            if (!dict.ContainsKey(key)) break;
                            if (dict[key] is Component)
                            {
                                dict[key].active = ((Component)value).active;
                            }
                            else if (temp.IsPanelType())
                            {
                                dict[key] = value;
                            }
                        }
                        else
                        {
                            if (value is Component)
                            {
                                ((Component)temp.obj).active = ((Component)value).active;
                            }
                            else if (temp.IsPanelType())
                            {
                                temp.fpinfo.SetValue(value, temp.parentItem.obj);
                            }
                        }
                    }
                    else
                    {
                        InspectorItem next = itemspath.ElementAt(count + 1);
                        if (next.membertype == member_type.dictentry)
                        {
                            dynamic dict = temp.obj;
                            dynamic key = next.key;
                            if (!dict.ContainsKey(key)) break;
                            temp = new InspectorItem(null, temp, dict[key], key);
                        }
                        else
                        {
                            temp = new InspectorItem(null, temp, next.fpinfo.GetValue(temp.obj), next.fpinfo.propertyInfo);
                        }
                    }
                    count++;
                }
            });
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
