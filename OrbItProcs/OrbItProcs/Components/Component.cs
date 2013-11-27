using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

using Component = OrbItProcs.Components.Component;

namespace OrbItProcs.Components
{
    public abstract class Component {
        public Dictionary<dynamic, dynamic> compProps = new Dictionary<dynamic, dynamic>();

        private bool _active = true;
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
        public Node parent;
        public comp _com;
        public comp com { get { return _com; } set { _com = value; } }

       public abstract void Initialize(Node parent);
       public abstract void AffectOther(Node other);
       public abstract void AffectSelf();
       public abstract void Draw(SpriteBatch spritebatch);
       public abstract bool hasMethod(string methodName);

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
           List<PropertyInfo> properties = sourceComp.GetType().GetProperties().ToList();
           foreach (PropertyInfo property in properties)
           {
               property.SetValue(destComp, property.GetValue(sourceComp, null), null);
           }
           foreach (FieldInfo field in fields)
           {
               //Console.WriteLine("fieldtype: " + field.FieldType);
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

               destComp.InitializeLists();

               //field.SetValue(newobj, field.GetValue(obj));
           }
       }
    }

    
}
