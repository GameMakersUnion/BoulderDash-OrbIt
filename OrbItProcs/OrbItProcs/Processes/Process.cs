using OrbItProcs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs.Processes
{
    public delegate void ProcessMethod (Dictionary<dynamic,dynamic> args); // to be 'classoverloaded' later
    

    public class Process
    {
        public List<Process> procs = new List<Process>();
        //Process parentprocess; //I bet you a coke -Dante
        public Dictionary<dynamic, dynamic> pargs;
        public Dictionary<dynamic, ProcessMethod> pmethods;

        public ProcessMethod Update;// { get; set; }
        public Dictionary<dynamic, dynamic> UpdateArgs { get; set; }
        public ProcessMethod Create { get; set; }
        public Dictionary<dynamic, dynamic> CreateArgs { get; set; }
        public ProcessMethod Destroy { get; set; }
        public Dictionary<dynamic, dynamic> DestroyArgs { get; set; }
        public ProcessMethod Collision { get; set; }
        public Dictionary<dynamic, dynamic> CollisionArgs { get; set; }
        public ProcessMethod OutOfBounds { get; set; }
        public Dictionary<dynamic, dynamic> OutOfBoundsArgs { get; set; }

        //CollisionListener collisionListener;
        //OutOfBoundsListener outofboundsListener;

        public Process(Dictionary<ProcessMethod,Dictionary<dynamic,dynamic>> methods)
        {
            Update        = methods.ElementAt(0).Key;
            UpdateArgs    = methods.ElementAt(0).Value;
            Create       = methods.ElementAt(1).Key;
            CreateArgs  = methods.ElementAt(1).Value;
            Destroy         = methods.ElementAt(2).Key;
            DestroyArgs        = methods.ElementAt(2).Value;
            Collision          = methods.ElementAt(3).Key;
            CollisionArgs      = methods.ElementAt(3).Value;
            OutOfBounds        = methods.ElementAt(4).Key;
            OutOfBoundsArgs    = methods.ElementAt(4).Value;
        }

        public Process()
        { 
            // / // / //
        }

        public void OnUpdate()
        {
            foreach (Process p in procs)
            {
                p.OnUpdate();
            }
            if (Update != null) Update(UpdateArgs);
        }

        public void Add(Process p)
        {
            procs.Add(p);
            p.OnCreate();

        }

        public void OnCreate()
        {
            if (Create != null) Create(CreateArgs);

            if (Collision != null)
            {
                //CollisionListener collisionListener = new CollisionListener(CollisionArgs["trigger"]);
                Node n = (Node)CollisionArgs["trigger"];
                n.Collided += Collision;
                n.CollideArgs = CollisionArgs;
            }

            if (OutOfBounds != null)
            {
                //OutOfBoundsListener outofboundsListener = new OutOfBoundsListener(OutOfBoundsArgs["trigger"]);
            }
        }

        public void Remove(Process p)
        {
            p.OnDestroy();
            procs.Remove(p);
        }

        public void OnDestroy()
        {
            if (Destroy != null) Destroy(DestroyArgs);
        }
        
    }
    /*
    public class EventListener
    {
        public EventListener(ProcessMethod method)
        { 
            
        }
    
    }
     */
}
