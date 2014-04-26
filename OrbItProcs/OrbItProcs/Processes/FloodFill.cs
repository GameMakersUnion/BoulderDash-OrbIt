using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    class FloodFill: Process
    {
        static Dictionary<Vector2, List<Node>> spawnedNodes = new Dictionary<Vector2, List<Node>>();
        static Dictionary<Vector2, int[]> spawnPoints = new Dictionary<Vector2, int[]>();
        public FloodFill()
            : base()
        {
            addProcessKeyAction("Fill", KeyCodes.LeftClick, OnPress: mouseFill);

        }
        public void mouseFill()
        {
            startFill(UserInterface.WorldMousePos);

        }
        public void startFill(Vector2 pos)
        {
            if (!spawnPoints.Keys.Contains(pos)) spawnPoints[pos] = new int[2];
        }
        public override void OnUpdate()
        {
            List<Vector2> removeLst = new List<Vector2>();
            foreach (var v in spawnPoints.Keys)
            {

                int Spawned = spawnPoints[v][0];
                int cols = spawnPoints[v][1] / 3;
                Console.WriteLine(Spawned + " <> " + cols);
                if (Spawned >= cols )
                {
                    float ratio = (float)Utils.random.NextDouble() + .5f;

                    Dictionary<dynamic, dynamic> standardDictionary = new Dictionary<dynamic, dynamic>(){
                    { nodeE.position, v },
                    { nodeE.radius, 25f * (ratio)},
                    //{ comp.gravity, true },
                };
                        spawnPoints[v][0]++;
                        Node n = room.spawnNode(standardDictionary);
                        spawnedNodes.GetOrAdd(v).Add(n);
                        n.SetData("Filling", v);
                        n.body.OnCollisionEnter += add;
                        n.body.OnCollisionExit += rem;
                    
                }
                else
                {
                    removeLst.Add(v);
                }
            }
            foreach (var v in removeLst)
            {
                finish(v);
            }
            base.OnUpdate();
        }

        private void finish(Vector2 v)
        {
            spawnPoints.Remove(v);
            foreach (var n in spawnedNodes[v])
            {
                n.clearData("Filling");
                n.body.OnCollisionEnter -= add;
                n.body.OnCollisionExit -= rem;
            }
        }
        CollisionDelegate add = delegate(Node Source, Node Dest)
        {
            if (Source.dataStore.Keys.Contains("Filling") && Dest.dataStore.Keys.Contains("Filling"))
                spawnPoints[Source.CheckData<Vector2>("Filling")][1]++;
        };

        CollisionDelegate rem = delegate(Node Source, Node Dest)
        {
            if (Source.dataStore.Keys.Contains("Filling") && Dest.dataStore.Keys.Contains("Filling"))
                spawnPoints[Source.CheckData<Vector2>("Filling")][1]--;
        };

    }
}
