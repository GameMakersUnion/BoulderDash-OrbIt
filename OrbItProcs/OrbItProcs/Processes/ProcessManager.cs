using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs.Processes
{
    

    public class ProcessManager
    {

        public List<Process> processes;
        public Room room;

        public ProcessManager(Room room)
        {
            this.room = room;
            this.processes = new List<Process>();

        }

        public void Update()
        {
            foreach (Process p in processes)
            {
                p.OnUpdate();
            }
        }

        //this will start it.... for now...
        public void Add(Process p)
        {
            processes.Add(p);
            p.OnCreate();
            //System.Console.WriteLine("heyo pre-emptive strike");
        }

        public void Remove(Process p)
        {
            p.OnDestroy();
            processes.Remove(p);
        }
    }
}
