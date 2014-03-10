using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace OrbItProcs
{
    public class TripSpawnOnCollide : Process
    {
        public int colCount = 0;
        public Node triggerNode { get; set; }

        public TripSpawnOnCollide(Node node) : base()
        {
            this.triggerNode = node;

            Collision += CollisionEvent;
            triggerNode.OnCollision += OnCollision;
        }

        

        public void CollisionEvent(Node me, Node it)
        {
            if (me == null) return;
            Console.WriteLine("event1");
            colCount++;
            if (colCount > 10)
            {
                //Collision -= CollisionEvent;
                me.OnCollision -= OnCollision;
                Console.WriteLine("yes");
            }
            Node n1 = new Node(), n2 = new Node(), n3 = new Node();
            Node.cloneNode(me, n1); // take params (...)
            Node.cloneNode(me, n2);
            Node.cloneNode(me, n3);
            //CollisionArgs["trigger"].Collided -= Collision;
            n1.OnCollision -= OnCollision;
            n2.OnCollision -= OnCollision;
            n3.OnCollision -= OnCollision;
            n1.body.pos.X -= 150;
            n2.body.pos.X += 150;
            n3.body.pos.Y -= 150;



            Group g = room.masterGroup.FindGroup("[G0]");
            g.IncludeEntity(n1);
            g.IncludeEntity(n2);
            g.IncludeEntity(n3);

            //n1.room.nodesToAdd.Enqueue(n1);
            //n1.room.nodesToAdd.Enqueue(n2);
            //n1.room.nodesToAdd.Enqueue(n3);
            
            //System.Console.WriteLine("Heyo");
        }
        
    }
}
