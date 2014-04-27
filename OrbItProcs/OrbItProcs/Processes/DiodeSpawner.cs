using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
namespace OrbItProcs
{
    public class DiodeSpawner : Process
    {
        public int diodeThickness = 50;
        public DiodeSpawner()
        {
            this.addProcessKeyAction("PlaceDiode", KeyCodes.LeftClick, OnPress: PlaceDiode);
            this.addProcessKeyAction("ChangeDiodeMode", KeyCodes.MiddleClick, OnPress: ChangeDiodeMode);
        }
        bool firstclick = true;
        Vector2 firstPos = Vector2.Zero;
        Node lastSpawnedDiode = null;
        public void PlaceDiode()
        {
            Vector2 mousePos = UserInterface.WorldMousePos;
            if (firstclick)
            {
                firstclick = false;
                firstPos = mousePos;
            }
            else
            {
                firstclick = true;
                var dict = new Dictionary<dynamic, dynamic>() { { comp.diode, true } };
                lastSpawnedDiode = Node.ContructLineWall(room, firstPos, mousePos, diodeThickness, dict);
                Console.WriteLine(lastSpawnedDiode.body.orient);
            }
        }

        public void ChangeDiodeMode()
        {
            Vector2 pos = UserInterface.WorldMousePos;
            Node found = null;
            float shortedDistance = Int32.MaxValue;
            for (int i = room.masterGroup.fullSet.Count - 1; i >= 0; i--)
            {
                Node n = (Node)room.masterGroup.fullSet.ElementAt(i);
                if (!n.HasComp<Diode>()) continue;
                // find node that has been clicked, starting from the most recently placed nodes
                float distsquared = Vector2.DistanceSquared(n.body.pos, pos);
                //if (distsquared < n.body.radius * n.body.radius)
                //{
                    if (distsquared < shortedDistance)
                    {
                        found = n;
                        shortedDistance = distsquared;
                    }
                //}
            }
            if (found != null)
            {
                Diode d = found.Comp<Diode>();
                int mode = (int)d.mode;
                int countModes = Enum.GetValues(typeof(Diode.Mode)).Length;
                d.mode = (Diode.Mode)((mode + 1) % countModes);
            }
        }
        
    }
}
