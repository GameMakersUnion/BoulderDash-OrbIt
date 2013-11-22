using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OrbItProcs.Processes;

namespace OrbItProcs.Processes
{
    public class TripSpawnOnCollide : Process
    {
        public int colCount = 0;
        public TripSpawnOnCollide(Node node) : base()
        {
            CollisionArgs = new Dictionary<dynamic, dynamic>();
            CollisionArgs["trigger"] = node;
            CollisionArgs["room"] = node.room;
            Collision += CollisionEvent;
            Collision += CollisionEvent2;
            
            //Collision = null;
        }

        public void CollisionEvent2(Dictionary<dynamic, dynamic> dict)
        {
            Console.WriteLine("event2");
        }

        public void CollisionEvent(Dictionary<dynamic,dynamic> dict)
        {
            Console.WriteLine("event1");
            colCount++;
            if (colCount > 10)
            {
                //Collision -= CollisionEvent;
                CollisionArgs["trigger"].Collided -= Collision;
                Console.WriteLine("yes");
            }
            Node n1 = new Node(), n2 = new Node(), n3 = new Node();
            Node.cloneObject(CollisionArgs["trigger"], n1); // take params (...)
            Node.cloneObject(CollisionArgs["trigger"], n2);
            Node.cloneObject(CollisionArgs["trigger"], n3);
            //CollisionArgs["trigger"].Collided -= Collision;
            n1.Collided -= Collision;
            n2.Collided -= Collision;
            n3.Collided -= Collision;
            n1.position.X -= 150;
            n2.position.X += 150;
            n3.position.Y -= 150;
            

            if (n1.room.nodesToAdd.Count < 1)
            {
                n1.room.nodesToAdd.Enqueue(n1);
                n1.room.nodesToAdd.Enqueue(n2);
                n1.room.nodesToAdd.Enqueue(n3);
            }
            //System.Console.WriteLine("Heyo");
        }
        
    }
}
