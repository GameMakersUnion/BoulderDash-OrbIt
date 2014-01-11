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
        public event ProcessMethod Update;
        public event ProcessMethod Create;
        public event ProcessMethod Destroy;
        public event ProcessMethod Collision;
        public event ProcessMethod OutOfBounds;

        public List<Process> procs = new List<Process>();
        //Process parentprocess; //I bet you a coke -Dante
        public Dictionary<dynamic, dynamic> pargs;
        public Dictionary<dynamic, ProcessMethod> pmethods;

        //public ProcessMethod Update;// { get; set; }
        //public ProcessMethod Create { get; set; }
        //public ProcessMethod Destroy { get; set; }
        //public ProcessMethod Collision { get; set; }
        //public ProcessMethod OutOfBounds { get; set; }

        //public Dictionary<dynamic, dynamic> UpdateArgs { get; set; }
        //public Dictionary<dynamic, dynamic> CreateArgs { get; set; }
        //public Dictionary<dynamic, dynamic> DestroyArgs { get; set; }
        //public Dictionary<dynamic, dynamic> CollisionArgs { get; set; }
        //public Dictionary<dynamic, dynamic> OutOfBoundsArgs { get; set; }
        
        //CollisionListener collisionListener;
        //OutOfBoundsListener outofboundsListener;

        /*
        public Process(Dictionary<ProcessMethod,Dictionary<dynamic,dynamic>> methods)
        {
            //Update = methods.ElementAt(0).Key;
            //Create = methods.ElementAt(1).Key;
            //Destroy = methods.ElementAt(2).Key;
            //Collision = methods.ElementAt(3).Key;
            //OutOfBounds = methods.ElementAt(4).Key;

            //UpdateArgs    = methods.ElementAt(0).Value;
            //CreateArgs  = methods.ElementAt(1).Value;
            //DestroyArgs        = methods.ElementAt(2).Value;
            //CollisionArgs      = methods.ElementAt(3).Value;
            //OutOfBoundsArgs    = methods.ElementAt(4).Value;
        }
        */

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
            if (Update != null) Update(pargs);
        }

        public void Add(Process p)
        {
            procs.Add(p);
            p.OnCreate();

        }

        public void OnCreate()
        {
            if (Create != null) Create(pargs);
        }

        public void OnCollision(Dictionary<dynamic,dynamic> args)
        {
            if (Collision != null)
            {
                Collision(args);
            }
        }

        public void Remove(Process p)
        {
            p.OnDestroy();
            procs.Remove(p);
        }

        public void OnDestroy()
        {
            if (Destroy != null) Destroy(pargs);
        }
        
    }
}
