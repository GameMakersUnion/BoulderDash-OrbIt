using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;



using Component = OrbItProcs.Component;

namespace OrbItProcs
{
    [Flags]
    public enum mtypes
    {
        none = 0,
        initialize = 1,
        affectother = 2,
        affectself = 4,
        draw = 8,
        minordraw = 16,
        exclusiveLinker = 32,
        essential = 64,
        tracer = 128,
    };

    public abstract class Component {
        public virtual mtypes compType { get; set; }

        protected bool _active = false;
        [Info(UserLevel.Developer)]
        public virtual bool active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                if (parent != null && parent.comps.ContainsKey(com))
                {
                    parent.triggerSortLists();
                }
            }
        }
        //*
        [Polenter.Serialization.ExcludeFromSerialization]
        public Node parent { get; set; }

        //*/
        //flag as not editable in InspectorBox
        private comp _com;
        public comp com { get { return _com; } set { } }
        //flag as not editable in InspectorBox
        private bool _CallDraw = true;
        public bool CallDraw { get { return _CallDraw; } set { _CallDraw = value; } }

        public HashSet<Node> exclusions = new HashSet<Node>();

        public bool isEssential()
        {
            return (compType & mtypes.essential) == mtypes.essential;
        }

        protected virtual void GetCompEnum()
        {
            string s = this.GetType().ToString().ToLower().LastWord('.');
            foreach(string name in Enum.GetNames(typeof(comp)))
            {
                if (name.Equals(s))
                {
                    _com = (comp)Enum.Parse(typeof(comp), name);
                    break;
                }
            }
        }

        public Component()
        {
            GetCompEnum();
        }

        public virtual void Initialize(Node parent) { this.parent = parent; }
        public virtual void AfterCloning() { }
        public virtual void OnSpawn() { }
        public virtual void AffectOther(Node other) { }
        public virtual void AffectSelf() { }
        public virtual void Draw() { }

        public virtual void InitializeLists()
        {
        }

        public virtual Texture2D getTexture()
        {
            if (parent != null)
            {
                return parent.getTexture();
            }
            return null;
        }

        public static Component GenerateComponent(comp c)
        {
            Component component = (Component)Activator.CreateInstance(Utils.GetComponentTypeFromEnum(c));
            return component;
        }
        

       public static void CloneComponent(Component sourceComp, Component destComp)
       {
           List<FieldInfo> fields = sourceComp.GetType().GetFields().ToList();
           fields.AddRange(sourceComp.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).ToList());
           List<PropertyInfo> properties = sourceComp.GetType().GetProperties().ToList();
           foreach (PropertyInfo property in properties)
           {
               if (property.PropertyType == typeof(ModifierInfo)) continue;
               if (property.PropertyType == typeof(Node)) continue;

                property.SetValue(destComp, property.GetValue(sourceComp, null), null);
           }
           foreach (FieldInfo field in fields)
           {
               if (field.Name.Equals("shape")) continue;
               if (field.FieldType == typeof(Dictionary<string,ModifierInfo>))
               {
                   Modifier mod = (Modifier) sourceComp;
                   
                   Dictionary<string, ModifierInfo> newmodinfos = new Dictionary<string, ModifierInfo>();
                   foreach (KeyValuePair<string, ModifierInfo> kvp in mod.modifierInfos)
                   {
                       string key = kvp.Key;
                       ModifierInfo modifierInfo = kvp.Value;
                       Dictionary<string, FPInfo> newFpInfos = new Dictionary<string, FPInfo>();
                       Dictionary<string, object> newFpInfosObj = new Dictionary<string, object>();
                       foreach (string key2 in modifierInfo.fpInfos.Keys)
                       {
                           FPInfo fpinfo = new FPInfo(modifierInfo.fpInfos[key2]);

                           newFpInfos.Add(key2, fpinfo);
                           newFpInfosObj.Add(key2, null);
                       }
                       Dictionary<string, dynamic> newargs = new Dictionary<string, dynamic>();
                       foreach (string key2 in modifierInfo.args.Keys)
                       {
                           newargs.Add(key2, modifierInfo.args[key2]); //by reference (for now)
                       }

                       ModifierInfo modInfo = new ModifierInfo(newFpInfos, newFpInfosObj, newargs, modifierInfo.modifierDelegate);
                       modInfo.delegateName = modifierInfo.delegateName;
                       newmodinfos.Add(key, modInfo);
                   }
                   field.SetValue(destComp, newmodinfos);

               }
               //no longer checking for dictionaries, parent(Node)
               if ((field.FieldType == typeof(int))
                   || (field.FieldType == typeof(Single))
                   || (field.FieldType == typeof(bool))
                   || (field.FieldType == typeof(string)))
               {
                   field.SetValue(destComp, field.GetValue(sourceComp));
               }
               else if (field.FieldType == typeof(Vector2))
               {
                   Vector2 vect = (Vector2)field.GetValue(sourceComp);
                   Vector2 newvect = new Vector2(vect.X, vect.Y);
                   field.SetValue(destComp, newvect);
               }
               else if (field.FieldType == typeof(Color))
               {
                   Color col = (Color)field.GetValue(sourceComp);
                   Color newcol = new Color(col.R, col.G, col.B, col.A);
                   field.SetValue(destComp, newcol);
               }
               else
               {
                   //this would be an object field
                   if (field.Name.Equals("room"))
                   {
                       field.SetValue(destComp, field.GetValue(sourceComp));
                   }
               }
               //field.SetValue(newobj, field.GetValue(obj));
           }
           destComp.InitializeLists();
           destComp.AfterCloning();
       }


       public static void CloneObject(object sourceObject, object destObject)
       {
           List<FieldInfo> fields = sourceObject.GetType().GetFields().ToList();
           fields.AddRange(sourceObject.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).ToList());
           List<PropertyInfo> properties = sourceObject.GetType().GetProperties().ToList();
           foreach (PropertyInfo property in properties)
           {
               if (property.PropertyType == typeof(ModifierInfo)) continue;
               if (property.PropertyType == typeof(Node)) continue;

               property.SetValue(destObject, property.GetValue(sourceObject, null), null);
           }
           foreach (FieldInfo field in fields)
           {
               if (field.Name.Equals("shape")) continue;
               //no longer checking for dictionaries, parent(Node)
               if ((field.FieldType == typeof(int))
                   || (field.FieldType == typeof(Single))
                   || (field.FieldType == typeof(bool))
                   || (field.FieldType == typeof(string)))
               {
                   field.SetValue(destObject, field.GetValue(sourceObject));
               }
               else if (field.FieldType == typeof(Vector2))
               {
                   Vector2 vect = (Vector2)field.GetValue(sourceObject);
                   Vector2 newvect = new Vector2(vect.X, vect.Y);
                   field.SetValue(destObject, newvect);
               }
               else if (field.FieldType == typeof(Color))
               {
                   Color col = (Color)field.GetValue(sourceObject);
                   Color newcol = new Color(col.R, col.G, col.B, col.A);
                   field.SetValue(destObject, newcol);
               }
               else if (field.FieldType == typeof(Room))
               {
                    field.SetValue(destObject, field.GetValue(sourceObject));
               }
           }

           MethodInfo mInfo = destObject.GetType().GetMethod("InitializeLists");
           if (mInfo != null) mInfo.Invoke(destObject, null);
           mInfo = destObject.GetType().GetMethod("AfterCloning");
           if (mInfo != null) mInfo.Invoke(destObject, null);
           
           //destObject.InitializeLists();
           //destObject.AfterCloning();
       }
    }

    
}
