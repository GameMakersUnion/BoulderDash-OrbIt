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

        public static float between0and2pi(this float value)
        {
            if (value > 2 * VMath.PI) value = value % (2 * VMath.PI);
            if (value < 0)
                value = 2 * VMath.PI + value;
            return value;
        }

        public const float rootOfTwo = 1.41421356237f;
        public const float invRootOfTwo = 0.70710678118f;

        public static float Lerp(float start, float end, float amount)
        {
            if (amount > 1f) amount = 1f;
            if (amount < 0f) amount = 0f;
            if (start > end)
            {
                float temp = start;
                start = end;
                end = start;
            }
            return start + (end - start) * amount;
        }
        public static Color ToColor(this Vector4 v)
        {
            return new Color(v.X, v.Y, v.Z, v.W);
        }
        public static Color ToColor(this Vector3 v, byte alpha)
        {
            return new Color(v.X, v.Y, v.Z, (float)alpha / 255f);
        }
        public static string Name(this Type t)
        {
            return t.ToString().LastWord('.');
        }
        public static Vector2 AngleToVector(float angle)
        {
            return new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle));
        }
        public static float CircularDistance(float x, float v, int t = 360)
        {
            int half = t / 2;
            if (x == v) return 0;
            if (x > v)
            {
                if (v > x - half)
                {
                    return x - v;
                }
                else
                {
                    return t - x + v;
                }
            }
            else
            {
                if (v < x + half)
                {
                    return v - x;
                }
                else
                {
                    return x - t + v;
                }
            }
        }

        public static float VectorToAngle(Vector2 vector)
        {
            float value = (float)Math.Atan2(vector.X, -vector.Y);
            if (value > 2f * VMath.PI)
                value = value % (2 * VMath.PI);
            if (value < 0)
                value = 2f * VMath.PI + value;

            return value;
        }
        public static Texture2D Crop(this Texture2D image, Rectangle source)
        {
            var graphics = image.GraphicsDevice;
            var ret = new RenderTarget2D(graphics, source.Width, source.Height);
            var sb = new SpriteBatch(graphics);

            graphics.SetRenderTarget(ret); // draw to image
            graphics.Clear(new Color(0, 0, 0, 0));

            sb.Begin();
            sb.Draw(image, Vector2.Zero, source, Color.White);
            sb.End();

            graphics.SetRenderTarget(null); // set back to main window

            return (Texture2D)ret;
        }
        public static Texture2D[,] sliceSpriteSheet(this Texture2D spritesheet, int columnsX, int rowsY)
        {
           Texture2D[,] result = new Texture2D[columnsX,rowsY];
            int width = spritesheet.Bounds.Width / columnsX;
            int height = spritesheet.Bounds.Height / rowsY;
           for (int x = 0; x < columnsX; x++)
           {
               for (int y = 0; y < rowsY; y++)
               {
                   result[x, y] = spritesheet.Crop(new Rectangle(x * width, y * height, width, height));
               }
           }
           return result;
           
            
        }
        public static void notImplementedException() { PopUp.Toast("Zack and Dante are lazy.", "NotImplementedException"); }
        public static object parsePrimitive(Type primitiveType, String value)
        {

            string s = value.ToString().Trim();

            if (primitiveType == typeof(int))
            {
                int v;
                if (Int32.TryParse(s, out v))
                {
                    //fpinfo.SetValue(v, parentItem.obj);
                    return v;
                }
                else return null;
            }
            else if (primitiveType == typeof(float))
            {
                float v;
                if (float.TryParse(s, out v))
                {
                    //fpinfo.SetValue(v, parentItem.obj);
                    return v;
                }
                else return null;
            }
            else if (primitiveType == typeof(double))
            {
                double v;
                if (double.TryParse(s, out v))
                {
                    //fpinfo.SetValue(v, parentItem.obj);
                    return v;
                }
                else return null;
            }
            else if (primitiveType == typeof(byte))
            {
                byte v;
                if (byte.TryParse(s, out v))
                {
                    //fpinfo.SetValue(v, parentItem.obj);
                    return v;
                }
                else return null;
            }
            else if (primitiveType.IsEnum)
            {
                foreach (var val in Enum.GetValues(primitiveType))
                {
                    if (val.ToString().ToLower().Equals(s.ToLower()))
                    {
                        return val;
                    }
                }
            }
            return null;
        }

        public static string increment(this String origin, Type primitiveType, NumBoxMode n)
        {
            dynamic number = parsePrimitive(primitiveType, origin);
            if (number == null) return "0";
            switch (n){
                case NumBoxMode.byOne:
                    return (number + 1).ToString();
                case NumBoxMode.byTen:
                    return (number + 10).ToString();
                case NumBoxMode.quadratic:
                    return (number * 2).ToString();
                default: return "0";
            }
        }
        public static string decrement(this String origin, Type primitiveType, NumBoxMode n)
        {
            dynamic number = parsePrimitive(primitiveType, origin);
            if (number == null) return "0";
            switch (n)
            {
                case NumBoxMode.byOne:
                    return (number - 1).ToString();
                case NumBoxMode.byTen:
                    return (number - 10).ToString();
                case NumBoxMode.quadratic:
                    return (number / 2).ToString();
                default: return "0";
            }
        }
        public static Dictionary<comp, Type> compTypes;
        public static Dictionary<Type, comp> compEnums;

        public static Type GetComponentTypeFromEnum(comp c)
        {
            if (compTypes.ContainsKey(c)) return compTypes[c];
            return null;
        }
        public static comp GetComponentCompFromType(Type t)
        {
            if (compEnums.ContainsKey(t)) throw new SystemException("Type was not found in compEnums dictionary.");
            return compEnums[t];
        }
        public static mtypes GetCompTypes(Type t)
        {
            FieldInfo pinfo = t.GetField("CompType");
            if (pinfo == null || pinfo.FieldType != typeof(mtypes)) return mtypes.none;
            return (mtypes)pinfo.GetValue(null);
        }

        public static Info GetInfoType(Type t)
        {
            var infos = t.GetCustomAttributes(typeof(Info), false);
            if (infos != null && infos.Length > 0)
            {
                return (Info)infos.ElementAt(0);
            }
            return null;
        }
        public static Info GetInfoClass(object o)
        {
            return GetInfoType(o.GetType());
        }
        public static Info GetInfoProperty(PropertyInfo pinfo)
        {
            var infos = pinfo.GetCustomAttributes(typeof(Info), false);
            if (infos != null && infos.Length > 0)
            {
                return (Info)infos.ElementAt(0);
            }
            return null;
        }

        public static bool isGenericType(Type genericType, Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }
            return false;
        }
        public static bool isToggle(object o)
        {
            return isGenericType(typeof(Toggle<>), o.GetType());
        }
        public static bool isToggle(Type t)
        {
            return isGenericType(typeof(Toggle<>), t);
        }

        public static void PopulateComponentTypesDictionary()
        {
            compTypes = new Dictionary<comp, Type>();

            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(assembly => assembly.GetTypes())
                       .Where(type => type.IsSubclassOf(typeof(Component)));


            foreach(Type t in types)
            {
                string name = t.ToString().ToLower();
                name = name.LastWord('.');
                comp c;
                if (Enum.TryParse<comp>(name, out c))
                {
                    compTypes.Add(c, t);
                }
                else
                {
                    Console.WriteLine("{0} did not have an equivalent enum value", t.ToString());
                }
            }
            Dictionary<comp, Type> ct = new Dictionary<comp, Type>();
            foreach(comp c in Enum.GetValues(typeof(comp)))
            {
                if (!compTypes.ContainsKey(c)) { Console.WriteLine("Class didn't exist for enum value " + c); continue; }
                ct[c] = compTypes[c];
            }
            compTypes = ct;

            compEnums = new Dictionary<Type, comp>(); //move this later
            foreach (comp key in Utils.compTypes.Keys.ToList())
            {
                compEnums.Add(Utils.GetComponentTypeFromEnum(key), key);
            }
        }

        public static int Sign(int i)
        {
            return (i > 0).ToInt() - (i < 0).ToInt();
        }

        public static int ToInt(this bool b)
        {
            return b ? 1 : 0;
        }

        public static string LastWord(this string s, char delim)
        {
            return s.Substring(s.LastIndexOf(delim) + 1);
        }

        public static void Break()
        {
            System.Diagnostics.Debugger.Break();
        }

        public static string uniqueString(ObservableHashSet<string> hashSet = null)
        {
            string GuidString = randomString();

            if (hashSet != null)
            {
                while (hashSet.Contains(GuidString))
                {
                    GuidString = randomString();
                }
            }
            return GuidString;
        }

        public static string randomString()
        {
            Guid g = Guid.NewGuid();
            string GuidString = Convert.ToBase64String(g.ToByteArray());
            GuidString = GuidString.Replace("=", "");
            GuidString = GuidString.Replace("+", "");
            return GuidString;
        }
        public static bool IsFucked(this Vector2 v)
        {
            if (float.IsInfinity(v.X) || float.IsNaN(v.X) || float.IsInfinity(v.Y) || float.IsNaN(v.Y)) return true;
            return false;
        }


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
        
        public static float[] toFloatArray(this Vector2 v2)
        {
            float[] result = new float[2];
            result[0] = v2.X; result[1] = v2.Y;
            return result;
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

        

        public static T Pop<T>(this List<T> list)
        {
            T item = list.ElementAt(list.Count);
            list.Remove(item);
            return item;
        }

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
        public static Vector2 CENTER_TEXTURE = new Vector2(0.5f, 0.5f);
        public static void DrawLine(Room room, Vector2 start, Vector2 end, float thickness, Color color)
        {
            if (thickness * room.zoom < 1) thickness = 1 / room.zoom;
            Vector2 diff = (end - start);// *mapzoom;
            Vector2 centerpoint = (end + start) / 2;
            //centerpoint *= mapzoom;
            float len = diff.Length();
            //thickness *= 2f * mapzoom;
            Vector2 scalevect = new Vector2(len, thickness);
            float angle = (float)(Math.Atan2(diff.Y, diff.X));
            room.camera.Draw(textures.whitepixel, centerpoint, null, color, angle, Assets.textureCenters[textures.whitepixel], scalevect, SpriteEffects.None, 0);
        }

        public static bool checkCollision(Node o1, Node o2)
        {
            if (Vector2.DistanceSquared(o1.body.pos, o2.body.pos) <= ((o1.body.radius + o2.body.radius) * (o1.body.radius + o2.body.radius)))
            {
                return true;
            }
            return false;
        }

        public static void resolveCollision(Node o1, Node o2)
        {
            float distanceOrbs = (float)Vector2.Distance(o1.body.pos, o2.body.pos);
            if (distanceOrbs < 10) distanceOrbs = 10; //prevent /0 error
            Vector2 normal = (o2.body.pos - o1.body.pos) / distanceOrbs;
            float pvalue = 2 * (o1.body.velocity.X * normal.X + o1.body.velocity.Y * normal.Y - o2.body.velocity.X * normal.X - o2.body.velocity.Y * normal.Y) / (o1.body.mass + o2.body.mass);

            o1.body.velocity.X = o1.body.velocity.X - pvalue * normal.X * o2.body.mass;
            o1.body.velocity.Y = o1.body.velocity.Y - pvalue * normal.Y * o2.body.mass;
            o2.body.velocity.X = o2.body.velocity.X + pvalue * normal.X * o1.body.mass;
            o2.body.velocity.Y = o2.body.velocity.Y + pvalue * normal.Y * o1.body.mass;
            //float loss1 = 0.98f;
            //float loss2 = 0.98f;
            //o1.transform.velocity *= loss1;
            //o2.transform.velocity *= loss2;
            fixCollision(o1, o2);
        }

        //make sure that if the orbs are stuck together, they are separated.
        public static void fixCollision(Node o1, Node o2)
        {
            //float orbRadius = 25.0f; //integrate this into the orb class
            //if the orbs are still within colliding distance after moving away (fix radius variables)
            //if (Vector2.DistanceSquared(o1.transform.position + o1.transform.velocity, o2.transform.position + o2.transform.velocity) <= ((o1.transform.radius * 2) * (o2.transform.radius * 2)))
            if (Vector2.DistanceSquared(o1.body.pos + o1.body.velocity, o2.body.pos + o2.body.velocity) <= ((o1.body.radius + o2.body.radius) * (o1.body.radius + o2.body.radius)))
            {

                Vector2 difference = o1.body.pos - o2.body.pos; //get the vector between the two orbs
                float length = Vector2.Distance(o1.body.pos, o2.body.pos);//get the length of that vector
                difference = difference / length;//get the unit vector
                //fix the below statement to get the radius' from the orb objects
                length = (o1.body.radius + o2.body.radius) - length; //get the length that the two orbs must be moved away from eachother
                difference = difference * length; // produce the vector from the length and the unit vector
                if (o1.movement.active && o1.movement.pushable
                    && o2.movement.active && o2.movement.pushable)
                {
                    o1.body.pos += difference / 2;
                    o2.body.pos -= difference / 2;
                }
                else if (o1.movement.active && !o1.movement.pushable)
                {
                    o2.body.pos -= difference;
                }
                else if (o2.movement.active && !o2.movement.pushable)
                {
                    o1.body.pos += difference;
                }
            }
            else return;
        }

        public static void Infect(Node newNode)
        {
            if (Utils.random.Next(50000) == 0)
            {
                newNode.body.color = Color.Red;
                CollisionDelegate evil = null;
                Action<Node> doAfter = delegate(Node n)
                {
                    n.body.color = Color.Red;
                    n.body.OnCollisionStay += evil;
                };


                evil = delegate(Node source, Node target)
                {
                    if (target == null) return;
                    if (target.CheckData<bool>("infected")) return;
                    if (target.HasComp<Scheduler>())
                    {
                        target.Comp<Scheduler>().doAfterXMilliseconds(doAfter, Utils.random.Next(5000));
                        target.SetData("infected", true);
                    }
                };
                newNode.body.OnCollisionStay += evil;
            }
        }


        internal static float AngleLerp(float source, float dest, float p)
        {

            float result = 0f;
            float pi = VMath.PI;


            if (source < pi / 2 && dest > (3 * pi) / 2)
            {
                result = MathHelper.Lerp(source, dest - (2 * pi), p);
            }
            else if (source > (3 * pi / 2) && dest < pi / 2)
            {
                result = MathHelper.Lerp(source, dest + (2 * pi), p);
            }
            else
            {
                result = MathHelper.Lerp(source, dest, p);
            }
            return result;
        }
    } // end of class.
}
