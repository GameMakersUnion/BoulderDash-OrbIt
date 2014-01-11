using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace OrbItProcs {
    public static class Utils {

        public static string wordWrap(this string message, int maxCharsPerLine)
        {

            int chars = maxCharsPerLine;
                for (int i = 1; i <= 4; i++)
                    if (message.Length > chars * i)
                        for (int j = chars * i; j > (chars * i) - chars; j--)
                            if (message.ElementAt(j).Equals(' ') || message.ElementAt(j).Equals('/'))
                            {
                                message = message.Insert(j + 1, "\n");

                                break;
                            };
                return message;
        }
        //even distribution of colors between 0 and 16.5 million (total number of possible colors, excluding alphas)
        public static Color IntToColor(int i, int alpha = 255)
        {
            int r = (i / (255 * 255)) % 255;
            int g = (i / 255) % 255;
            int b = i % 255;

            string s = string.Format("{0}\t{1}\t{2}", r, g, b);
            //Console.WriteLine(s);
            //Console.WriteLine(i);
            return new Color(r, g, b, alpha);
        }

        public static int CurrentMilliseconds()
        {
            DateTime dt = DateTime.Now;
            int total = dt.Millisecond + (dt.Second * 1000) + (dt.Minute * 60 * 1000);
            return total;
        }

        public static bool In<T>(this T x, params T[] args) where T : struct, IConvertible {return args.Contains(x);}

        public static string SelectedItem(this TomShane.Neoforce.Controls.ComboBox cb)
        {
            if (cb == null || cb.ItemIndex == -1) return null;
            return cb.Items.ElementAt(cb.ItemIndex).ToString();
        }


        public static object selected(this TomShane.Neoforce.Controls.ListBox c) { return c.Items.ElementAt(c.ItemIndex); }

        public static void syncToOCDelegate(this ICollection<object> lst, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                foreach (object o in e.NewItems)
                    lst.Add(o);
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                foreach (object o in e.OldItems)
                    lst.Remove(o);
        }
        public static void syncTo(this ObservableCollection<object> oc, ICollection<object> from)
        {
            oc.CollectionChanged += delegate(object s, NotifyCollectionChangedEventArgs e) { (from as ObservableCollection<object>).syncToOCDelegate(e); };
        }
        public static void reset(this ObservableCollection<object> oc)
        {
            foreach (object o in oc) oc.Remove(o);
        }
        public static void AddRange(this ObservableCollection<object> oc, ICollection<object> from)
        { foreach (object o in from) oc.Add(o); }
        public static void RemoveRange(this ObservableCollection<object> oc, ICollection<object> from)
        { foreach (object o in from) if (oc.Contains(o)) oc.Remove(o); }

        /*
        public static void cloneObject<T>(T obj, T newobj) //they must be the same type
        {
            //dynamic returnval;
            List<FieldInfo> fields = obj.GetType().GetFields().ToList();
            List<PropertyInfo> properties = obj.GetType().GetProperties().ToList();
            foreach (PropertyInfo property in properties)
            {
                property.SetValue(newobj, property.GetValue(obj, null), null);
            }
            foreach (FieldInfo field in fields)
            {
                //Console.WriteLine("fieldtype: " + field.FieldType);
                if (field.FieldType.ToString().Contains("Dictionary"))
                {
                    //Console.WriteLine(field.Name + " is a dictionary.");
                    if (field.FieldType.ToString().Contains("[System.Object,System.Boolean]"))//must be props
                    {
                        //Console.WriteLine("PROPS");
                        dynamic node = obj;
                        
                        Dictionary<dynamic, bool> dict = node.props;
                        Dictionary<dynamic, bool> newdict = new Dictionary<dynamic, bool>();
                        foreach (dynamic key in dict.Keys)
                        {
                            newdict.Add(key, dict[key]);
                        }
                        field.SetValue(newobj, newdict);

                    }
                    else if (field.FieldType.ToString().Contains("[OrbItProc.comp,System.Object]"))//must be comps
                    {
                        //Console.WriteLine("COMPS");
                        dynamic node = obj;
                        Dictionary<comp,dynamic> dict = node.comps;
                        dynamic newNode = newobj;
                        foreach (comp key in dict.Keys)
                        {
                            newNode.addComponent(key, true);
                            cloneObject<Component>(dict[key], newNode.comps[key]);
                            
                        }
                        //Console.WriteLine(newNode.comps[comp.randinitialvel].multiplier);
                    }
                }
                else if ((field.FieldType.ToString().Equals("System.Int32"))
                    || (field.FieldType.ToString().Equals("System.Single"))
                    || (field.FieldType.ToString().Equals("System.Boolean"))
                    || (field.FieldType.ToString().Equals("System.String")))
                {
                    field.SetValue(newobj, field.GetValue(obj));
                }
                else if (field.FieldType.ToString().Contains("Vector2"))
                {
                    Vector2 vect = (Vector2)field.GetValue(obj);
                    Vector2 newvect = new Vector2(vect.X, vect.Y);
                    field.SetValue(newobj, newvect);
                }
                else if (field.Name.Equals("parent"))
                {
                    //do nothing
                }
                else
                { 
                    //this would be an object field
                    //Console.WriteLine(field.Name + " is an object of some kind.");
                    if (field.Name.Equals("room") || field.Name.Equals("texture"))
                    {
                        field.SetValue(newobj, field.GetValue(obj));
                    }
                }

                //field.SetValue(newobj, field.GetValue(obj));
            }
            
        }
        */

        public static Random random = new Random((int)DateTime.Now.Millisecond);

        public static Color randomColor()
        {
            return new Color((float)Utils.random.Next(255) / (float)255, (float)Utils.random.Next(255) / (float)255, (float)Utils.random.Next(255) / (float)255);
        }

        public static void printDictionary(Dictionary<dynamic,dynamic> dict, string s = "")
        {
            if (dict == null)
            { //Console.WriteLine("Dict is null"); return; }
            }
            Console.WriteLine(s);
            foreach (KeyValuePair<dynamic, dynamic> kvp in dict)
            {
                //Console.WriteLine("Key = {0}, Value = {1}",
                //    kvp.Key, kvp.Value);
            }
        }

        public static void ensureContains(Dictionary<dynamic, dynamic> props, Dictionary<dynamic, dynamic> defProps)
        {
            foreach (dynamic p in defProps.Keys)
            {
                if (!props.ContainsKey(p)) props.Add(p, defProps[p]);
                else props[p] = props[p] ?? defProps[p];
            }
            
        }

        public static void DrawLine(SpriteBatch spritebatch, Vector2 start, Vector2 end, float thickness, Color color, Room room)
        {
            float mapzoom = room.mapzoom;
            
            Vector2 centerTexture = new Vector2(0.5f, 0.5f);
            Vector2 diff = (end - start) / mapzoom;
            Vector2 centerpoint = (end + start) / 2;
            centerpoint /= mapzoom;
            float len = diff.Length();
            Vector2 scalevect = new Vector2(len, thickness);
            float testangle = (float)(Math.Atan2(diff.Y, diff.X));// + (Math.PI / 2));

            //diff.Normalize();
            //diff = new Vector2(diff.Y, diff.X);

            spritebatch.Draw(room.game.textureDict[textures.whitepixel], centerpoint, null, color, testangle, centerTexture, scalevect, SpriteEffects.None, 0);

        }

        public static bool checkCollision(Node o1, Node o2)
        {

            if (Vector2.DistanceSquared(o1.transform.position, o2.transform.position) <= ((o1.transform.radius + o2.transform.radius) * (o1.transform.radius + o2.transform.radius)))
            {
                return true;
            }
            return false;
        }

        public static void resolveCollision(Node o1, Node o2)
        {

            /*Console.WriteLine("Collision Occured.");
            o1.IsActive = false;
            o2.IsActive = false;
             */

            //Console.WriteLine(o1.transform.mass + " " + o2.transform.mass);
            
            //ELASTIC COLLISION RESOLUTION --- FUCK YEAH
            //float orbimass = 1, orbjmass = 1;
            //float orbRadius = 25.0f; //integrate this into the orb class
            float distanceOrbs = (float)Vector2.Distance(o1.transform.position, o2.transform.position);
            if (distanceOrbs < 10) distanceOrbs = 10; //prevent /0 error
            Vector2 normal = (o2.transform.position - o1.transform.position) / distanceOrbs;
            float pvalue = 2 * (o1.transform.velocity.X * normal.X + o1.transform.velocity.Y * normal.Y - o2.transform.velocity.X * normal.X - o2.transform.velocity.Y * normal.Y) / (o1.transform.mass + o2.transform.mass);
            //if (!test) 
            //return;
            o1.transform.velocity.X = o1.transform.velocity.X - pvalue * normal.X * o2.transform.mass;
            o1.transform.velocity.Y = o1.transform.velocity.Y - pvalue * normal.Y * o2.transform.mass;
            o2.transform.velocity.X = o2.transform.velocity.X + pvalue * normal.X * o1.transform.mass;
            o2.transform.velocity.Y = o2.transform.velocity.Y + pvalue * normal.Y * o1.transform.mass;

            float loss1 = 0.98f;
            float loss2 = 0.98f;
            //o1.transform.velocity *= loss1;
            //o2.transform.velocity *= loss2;

            //if (game.fixCollisionOn)
            fixCollision(o1, o2);


        }

        //make sure that if the orbs are stuck together, they are separated.
        public static void fixCollision(Node o1, Node o2)
        {
            //float orbRadius = 25.0f; //integrate this into the orb class
            //if the orbs are still within colliding distance after moving away (fix radius variables)
            //if (Vector2.DistanceSquared(o1.transform.position + o1.transform.velocity, o2.transform.position + o2.transform.velocity) <= ((o1.transform.radius * 2) * (o2.transform.radius * 2)))
            if (Vector2.DistanceSquared(o1.transform.position + o1.transform.velocity, o2.transform.position + o2.transform.velocity) <= ((o1.transform.radius + o2.transform.radius) * (o1.transform.radius + o2.transform.radius)))
            {

                Vector2 difference = o1.transform.position - o2.transform.position; //get the vector between the two orbs
                float length = Vector2.Distance(o1.transform.position, o2.transform.position);//get the length of that vector
                difference = difference / length;//get the unit vector
                //fix the below statement to get the radius' from the orb objects
                length = (o1.transform.radius + o2.transform.radius) - length; //get the length that the two orbs must be moved away from eachother
                difference = difference * length; // produce the vector from the length and the unit vector
                if (o1.comps.ContainsKey(comp.movement) && o1.comps[comp.movement].pushable
                    && o2.comps.ContainsKey(comp.movement) && o2.comps[comp.movement].pushable)
                {
                    o1.transform.position += difference / 2;
                    o2.transform.position -= difference / 2;
                }
                else if (o1.comps.ContainsKey(comp.movement) && !o1.comps[comp.movement].pushable)
                {
                    o2.transform.position -= difference;
                }
                else if (o2.comps.ContainsKey(comp.movement) && !o2.comps[comp.movement].pushable)
                {
                    o1.transform.position += difference;
                }
            }
            else return;
        }



    } // end of class
}
