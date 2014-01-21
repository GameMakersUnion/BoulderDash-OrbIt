using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



using System.Reflection;
using System.Collections;
using System.Collections.ObjectModel;

namespace OrbItProcs
{
    public class Testing
    {
        public Room room;
        public Redirector redirector;
        public DateTime before;
        public bool timerStarted = false;

        public Testing()
        {
            room = Program.getRoom();

            Redirector.PopulateDelegatesAll();
            redirector = new Redirector();

            //obints.CollectionChanged += (s, e) => { };
        }

        public ObservableHashSet<int> obints = new ObservableHashSet<int>();
        public HashSet<int> hints = new HashSet<int>();
        public List<int> lints = new List<int>();
        public ObservableCollection<int> oblist = new ObservableCollection<int>();


        public void IntToColor(int i)
        {
            int r = (i / (255 * 255)) % 255;
            int g = (i / 255) % 255;
            int b = i % 255;

            string s = string.Format("{0}\t{1}\t{2}", r, g, b);
            Console.WriteLine(s);

        }

        public void ColorsTest()
        {
            int max = 255 * 255 * 255;
            for(int i = max/2; i<max;i++)
            {
                if (i % 100 == 0) IntToColor(i);
            }
        }

        public void ForLoops()
        {
            HashSet<int> ints = new HashSet<int>();
            for(int i = 0; i< 100000; i++)
            {
                ints.Add(i);
            }
            int count = 0;
            StartTimer();
            foreach (int i in ints)
            {
                count += i;
            }
            StopTimer("foreach:");
            Console.WriteLine(count);
            StartTimer();
            ints.ToList().ForEach(i => count += i);
            StopTimer("tolist:");
            Console.WriteLine(count);
        }

        public void KeyPresses(KeyManager kbs)
        {
            StartTimer();
            kbs.Update();
            StopTimer("keypresses:");
        }

        public void TestHashSet()
        {
            StartTimer();
            for (int i = 0; i < 100000; i++)
            {
                hints.Add(i);
            }
            StopTimer("HashSetAdd");
            StartTimer();
            for (int i = 0; i < 100000; i++)
            {
                obints.Add(i);
            }
            StopTimer(" ObsHashSetAdd");
            
            StartTimer();
            for (int i = 0; i < 100000; i++)
            {
                lints.Add(i);
            }
            StopTimer(" ListAdd");

            StartTimer();
            for (int i = 0; i < 100000; i++)
            {
                oblist.Add(i);
            }
            StopTimer(" ObsListAdd");
            Console.WriteLine("");
            
            ///
            StartTimer();
            for (int i = 0; i < 100000; i++)
            {
                hints.Contains(i);
            }
            StopTimer("HashSetContains");
            StartTimer();
            for (int i = 0; i < 100000; i++)
            {
                obints.Contains(i);
            }
            StopTimer(" ObsHashSetContains");
            /*
            StartTimer();
            for (int i = 0; i < 100000; i++)
            {
                //lints.Contains(i);
            }
            StopTimer("ListContains");
            */
            Console.WriteLine("");

            ///
            StartTimer();
            for (int i = 0; i < 100000; i++)
            {
                hints.Remove(i);
            }
            StopTimer("HashSetRemove");
            StartTimer();
            for (int i = 0; i < 100000; i++)
            {
                obints.Remove(i);
            }
            StopTimer(" ObsHashSetRemove");
            /*
            StartTimer();
            for (int i = 0; i < 100000; i++)
            {
                //lints.Remove(i);
            }
            StopTimer("ListRemove");
            */
            Console.WriteLine("");

            HashSet<int> excludeset = new HashSet<int>() { 5 };
            HashSet<int> newset;
            List<int> excludeInt = new List<int>() { 5 };
            StartTimer();
            for (int i = 0; i < 1000; i++)
            {
                //hints.ExceptWith(excludeset);
            }

        }

        public void WhereTest()
        {
            var originalSet = new HashSet<int>(){ 1, 2, 3, 4, 5 };
            var excludeListForA = new HashSet<int>() { 1, 2, 3 }; // <-- Note that A is in it's own list!

            // To update D and E:
            var setToActOn = originalSet.Where(element => !excludeListForA.Contains(element));
            foreach (int i in setToActOn)
            {
                Console.WriteLine(i);
            }
        }



        public void TestOnClick()
        {
            //testing to see how long it takes to generate all the getter/setter delegates

            object transformobj = room.defaultNode.transform;
            dynamic nodedynamic = room.defaultNode;
            List<Func<Node, float>> delList = new List<Func<Node, float>>();
            float total = 0;
            MethodInfo minfo = typeof(Transform).GetProperty("mass").GetGetMethod();
            Func<Transform, float> getDel = (Func<Transform, float>)Delegate.CreateDelegate(typeof(Func<Transform, float>), minfo);

            Movement movement = new Movement();

            //redirector.TargetObject = movement;
            //redirector.PropertyToObject["active"] = movement;
            redirector.AssignObjectToPropertiesAll(movement);
            PropertyInfo pinfo = movement.GetType().GetProperty("active");
            //Action<object, object> movementsetter = redirector.setters[typeof(Movement)]["active"];
            //Console.WriteLine(":::" + movement.active);
            //bool a = redirector.active;
            bool a = false;

            StartTimer();
            for (int i = 0; i < 100000; i++)
            {
                //if (i > 0) if (i > 1) if (i > 2) if (i > 3) if (i > 4) total++;

                //delList.Add(getDel);
                //float slow = (float)minfo.Invoke((Transform)transformobj, new object[] { });
                //float mass = getDel(room.defaultNode);
                //float mass2 = getDel((Transform)transformobj); //doesn't work because it's of type Object at compile time
                //float mass2 = getDel(nodedynamic);
                //total += mass;
                //gotten = room.defaultNode.GetComponent<Movement>(); //generic method to grab components
                //gotten = room.defaultNode.comps[comp.movement];
                //bool act = gotten.active;
                //gotten.active = true;
                redirector.active = false; //21m(impossible)... 24m(new) ... 19m (newer) ... 16m(newest)
                //a = redirector.active;
                //pinfo.SetValue(movement, false, null); //34m
                //movementsetter(movement, false); //4m(old)......... 6m(new)
                //movement.active = false;

            }
            //Movement move = room.defaultNode.comps[comp.movement];
            StopTimer();
            //Console.WriteLine(total);
            /* //this code won't run right now, but it represents the ability to make a specific generic method based on type variables from another generic method, and then invoke it... (this is slow)
            MethodInfo method = GetType().GetMethod("DoesEntityExist")
                             .MakeGenericMethod(new Type[] { typeof(Type) });
            method.Invoke(this, new object[] { dt, mill });
            */

            //gotten.fallOff();

            /////////////////////////////////////////////////////////////////////////////
        }

        public void StartTimer()
        {
            timerStarted = true;
            before = DateTime.Now;
        }
        public void StopTimer(string message = "")
        {
            DateTime after = DateTime.Now;
            if (!timerStarted)
            {
                Console.WriteLine("Timer was not previously started, so timer cannot be stopped.");
                return;
            }
            if (before == null) return;
            int mill = after.Millisecond - before.Millisecond;
            if (mill < 0) mill += 1000;
            Console.WriteLine(" {0}: {1}", message, mill);
            timerStarted = false;
        }

        public void OldTests()
        {
            //////////////////////////////////////////////////////////////////////////////////////
            List<int> ints = new List<int> { 1, 2, 3 };
            ints.ForEach(delegate(int i) { if (i == 2) ints.Remove(i); }); //COOL: NO ENUMERATION WAS MODIFIED ERROR
            ints.ForEach(delegate(int i) { Console.WriteLine(i); });

            MethodInfo testmethod = room.GetType().GetMethod("test");
            Action<Room, int, float, string> del = (Action<Room, int, float, string>)Delegate.CreateDelegate(typeof(Action<Room, int, float, string>), testmethod);
            del(room, 1, 0.3f, "Action worked.");

            Action<int, float, string> del2 = (Action<int, float, string>)Delegate.CreateDelegate(typeof(Action<int, float, string>), room, testmethod);
            //target is bound to 'room' in this example due to the overload of CreateDelegate used.
            del2(2, 3.3f, "Action worked again.");

            PropertyInfo pinfo = typeof(Component).GetProperty("active");
            MethodInfo minfo = pinfo.GetGetMethod();
            Console.WriteLine("{0}", minfo.ReturnType);

            Movement tester = new Movement();
            tester.active = true;

            bool ret = (bool)minfo.Invoke(tester, new object[] { }); //VERY expensive (slow)
            Console.WriteLine("{0}", ret);



            Func<Component, bool> delGet = (Func<Component, bool>)Delegate.CreateDelegate(typeof(Func<Component, bool>), minfo);
            Console.WriteLine("{0}", delGet(tester)); //very fast, and no cast or creation of empty args array required

            minfo = pinfo.GetSetMethod();
            //Console.WriteLine("{0} {1}", minfo.ReturnType, minfo.GetParameters()[0].ParameterType);

            Action<Component, bool> delSet = (Action<Component, bool>)Delegate.CreateDelegate(typeof(Action<Component, bool>), minfo);
            delSet(tester, false);
            Console.WriteLine("Here we go: {0}", delGet(tester));
            delSet(tester, true);
            /////////////////////////////////////////////////////////////////////////////////////////
            /*
            //gets all types that are a subclass of Component
            List<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(assembly => assembly.GetTypes())
                       .Where(type => type.IsSubclassOf(typeof(Component))).ToList();
            foreach (Type t in types) Console.WriteLine(t);
            */

            //room.defaultNode.Update(new GameTime()); //for testing

            //MODIFIER ADDITION
            /*
            room.defaultNode.addComponent(comp.modifier, true); //room.defaultNode.comps[comp.modifier].active = false;
            ModifierInfo modinfo = new ModifierInfo();
            modinfo.AddFPInfoFromString("o1", "scale", room.defaultNode);
            modinfo.AddFPInfoFromString("m1", "position", room.defaultNode);
            modinfo.AddFPInfoFromString("v1", "position", room.defaultNode);

            modinfo.args.Add("mod", 4.0f);
            modinfo.args.Add("times", 3.0f);
            modinfo.args.Add("test", 3.0f);
            
            //modinfo.delegateName = "Mod";
            //modinfo.delegateName = "Triangle";
            //modinfo.delegateName = "VelocityToOutput";
            //modinfo.delegateName = "VectorSine";
            modinfo.delegateName = "VectorSineComposite";

            room.defaultNode.comps[comp.modifier].modifierInfos["sinecomposite"] = modinfo;
            */

            ObservableHashSet<int> obints = new ObservableHashSet<int>();
            obints.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (int i in e.NewItems)
                    {
                        Console.WriteLine("Added:" + i);
                    }
                }
            };
            obints.Add(6);
        }
    }
}
