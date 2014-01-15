using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace OrbItProcs
{
    public class TripSpawnOnCollide : Process
    {
        public Room room;
        public int colCount = 0;
        public Node triggerNode { get; set; }

        public TripSpawnOnCollide(Node node) : base()
        {
            //CollisionArgs = new Dictionary<dynamic, dynamic>();
            //CollisionArgs["trigger"] = node;
            //CollisionArgs["room"] = node.room;
            //pargs["trigger"] = node;

            this.triggerNode = node;
            room = Program.getRoom();

            Collision += CollisionEvent;
            triggerNode.Collided += OnCollision;
            
            //Collision = null;
        }

        

        public void CollisionEvent(Dictionary<dynamic,dynamic> dict)
        {
            if (dict != null && dict.ContainsKey("collidee") && dict["collidee"] != null)
            {
                Console.WriteLine("Collided with node.");
                Node n = dict["collidee"];
                n.transform.color = Microsoft.Xna.Framework.Color.LightSkyBlue;
            }
            else
            {
                return;
            }

            Console.WriteLine("event1");
            colCount++;
            if (colCount > 10)
            {
                //Collision -= CollisionEvent;
                triggerNode.Collided -= OnCollision;
                Console.WriteLine("yes");
            }
            Node n1 = new Node(), n2 = new Node(), n3 = new Node();
            Node.cloneObject(triggerNode, n1); // take params (...)
            Node.cloneObject(triggerNode, n2);
            Node.cloneObject(triggerNode, n3);
            //CollisionArgs["trigger"].Collided -= Collision;
            n1.Collided -= OnCollision;
            n2.Collided -= OnCollision;
            n3.Collided -= OnCollision;
            n1.transform.position.X -= 150;
            n2.transform.position.X += 150;
            n3.transform.position.Y -= 150;



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
