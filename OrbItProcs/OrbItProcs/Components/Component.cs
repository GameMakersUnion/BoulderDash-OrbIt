using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using OrbItProcs.Processes;
using OrbItProcs.Interface;

using Component = OrbItProcs.Components.Component;

namespace OrbItProcs.Components
{
    [Flags]
    public enum mtypes
    {
        none = 0x00,
        initialize = 0x01,
        affectother = 0x02,
        affectself = 0x04,
        draw = 0x08,
        changereference = 0x10,
    };


    public abstract class Component {

        protected bool _active = false;
        public bool active
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

        public int sentinel = -10;
        protected Node _parent;
        //*
        [Polenter.Serialization.ExcludeFromSerialization]
        public Node parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
            }
        }
        //*/
        public comp _com;
        public comp com { get { return _com; } set { _com = value; } }
        public mtypes methods;

       public abstract void Initialize(Node parent);
       public virtual void AfterCloning() { }
       public abstract void AffectOther(Node other);
       public abstract void AffectSelf();
       public abstract void Draw(SpriteBatch spritebatch);

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

       public static void CloneComponent(Component sourceComp, Component destComp)
       {
           List<FieldInfo> fields = sourceComp.GetType().GetFields().ToList();
           fields.AddRange(sourceComp.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).ToList());
           List<PropertyInfo> properties = sourceComp.GetType().GetProperties().ToList();
           foreach (PropertyInfo property in properties)
           {
               if (property.PropertyType == typeof(ModifierInfo)) continue;
               if (property.PropertyType == typeof(Node)) continue;
               //Console.WriteLine(destComp.GetType().ToString() + property.Name);
               property.SetValue(destComp, property.GetValue(sourceComp, null), null);
           }
           foreach (FieldInfo field in fields)
           {
               //Console.WriteLine("fieldtype: " + field.FieldType);
               if (field.FieldType == typeof(ModifierInfo))
               {
                   Modifier mod = (Modifier) sourceComp;
                   if (mod.modifierInfo == null)
                   {
                       field.SetValue(destComp, null);
                       continue;
                   }
                   Dictionary<string, FPInfo> newFpInfos = new Dictionary<string, FPInfo>();
                   Dictionary<string, object> newFpInfosObj = new Dictionary<string, object>();
                   foreach(string key in mod.modifierInfo.fpInfos.Keys)
                   {
                       FPInfo fpinfo = new FPInfo(mod.modifierInfo.fpInfos[key]);
                       
                       newFpInfos.Add(key, fpinfo);
                       newFpInfosObj.Add(key, null);
                   }
                   Dictionary<string, dynamic> newargs = new Dictionary<string, dynamic>();
                   foreach(string key in mod.modifierInfo.args.Keys)
                   {
                       newargs.Add(key, mod.modifierInfo.args[key]); //by reference (for now)
                   }


                   ModifierInfo modInfo = new ModifierInfo(newFpInfos, newFpInfosObj, newargs, mod.modifierInfo.modifierDelegate);
                   field.SetValue(destComp, modInfo);

               }

               if (field.FieldType.ToString().Contains("Dictionary"))
               {
                   //Console.WriteLine(field.Name + " is a dictionary.");
               }
               else if ((field.FieldType.ToString().Equals("System.Int32"))
                   || (field.FieldType.ToString().Equals("System.Single"))
                   || (field.FieldType.ToString().Equals("System.Boolean"))
                   || (field.FieldType.ToString().Equals("System.String")))
               {
                   field.SetValue(destComp, field.GetValue(sourceComp));
               }
               else if (field.FieldType.ToString().Equals("Microsoft.Xna.Framework.Vector2"))
               {
                   //Console.WriteLine("VECTOR: {0}", field.FieldType.ToString());
                   Vector2 vect = (Vector2)field.GetValue(sourceComp);
                   Vector2 newvect = new Vector2(vect.X, vect.Y);
                   field.SetValue(destComp, newvect);
               }
               else if (field.FieldType.ToString().Equals("Microsoft.Xna.Framework.Color"))
               {
                   Color col = (Color)field.GetValue(sourceComp);
                   Color newcol = new Color(col.R, col.G, col.B, col.A);
                   field.SetValue(destComp, newcol);
               }
               else if (field.Name.Equals("parent"))
               {
                   //do nothing, we don't want to overwrite the reference to the new parent
               }
               else
               {
                   //this would be an object field
                   //Console.WriteLine(field.Name + " is an object of some kind.");
                   if (field.Name.Equals("room") || field.Name.Equals("texture"))
                   {
                       field.SetValue(destComp, field.GetValue(sourceComp));
                   }

                   //NO IDEA IF THIS WILL FUCK SHIT UP
                   //field.SetValue(destComp, field.GetValue(sourceComp));
               }

               

               //field.SetValue(newobj, field.GetValue(obj));
           }
           destComp.InitializeLists();
       }
    }

    
}
