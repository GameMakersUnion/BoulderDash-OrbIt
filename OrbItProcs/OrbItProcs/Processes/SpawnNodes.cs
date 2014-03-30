using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace OrbItProcs
{
    public class SpawnNodes : Process
    {
        private Vector2 spawnPos;
        int rightClickCount = 0;//
        int rightClickMax = 1;//
        public int batchSpawnNum { get; set; }

        public SpawnNodes() : base()
        {
            batchSpawnNum = 5;

            addProcessKeyAction("SpawnNode", KeyCodes.LeftClick, OnPress: SpawnNode);
            //addProcessKeyAction("SetSpawnPosition", KeyCodes.LeftShift, OnPress: SetSpawnPosition);
            addProcessKeyAction("BatchSpawn", KeyCodes.RightClick, OnHold: BatchSpawn);
            //addProcessKeyAction("DirectionalLaunch", KeyCodes.LeftShift, KeyCodes.RightClick, OnHold: DirectionalLaunch);
            
            addProcessKeyAction("testing", KeyCodes.OemPipe, OnPress: TestingStuff);

            addProcessKeyAction("serial1", KeyCodes.F1, OnPress: Serailize_Entities);
            addProcessKeyAction("deserial2", KeyCodes.F2, OnPress: Deserailize_Entities);
            addProcessKeyAction("serial1", KeyCodes.F3, OnPress: Serailize_Group);
            addProcessKeyAction("deserial2", KeyCodes.F4, OnPress: Deserailize_Group);
            addProcessKeyAction("serial1", KeyCodes.F5, OnPress: Serailize_Link);
            addProcessKeyAction("deserial2", KeyCodes.F6, OnPress: Deserailize_Link);
            addProcessKeyAction("serial1", KeyCodes.F7, OnPress: Serialize_Room);
            addProcessKeyAction("deserial2", KeyCodes.F8, OnPress: Deserialize_Room);
            addProcessKeyAction("serial1", KeyCodes.F9, OnPress: Serialize_Room_Binary);
            addProcessKeyAction("deserial2", KeyCodes.F10, OnPress: Deserialize_Room_Binary);
        }
        public void SpawnNode()
        {
            ///
            room.spawnNode((int)UserInterface.WorldMousePos.X, (int)UserInterface.WorldMousePos.Y);
        }

        public void TestingStuff()
        {
            //
            //room.game.testing.TestOnClick();
            //room.game.testing.TestHashSet();
            //room.game.testing.WhereTest();
            //room.game.testing.ForLoops();
            //room.game.testing.ColorsTest();
            //room.game.testing.NormalizeTest();
            //DateTime before = DateTime.Now;
            //room.game.testing.LoopTesting();
            //
            //int diff = DateTime.Now.Millisecond - before.Millisecond;
            //if (diff < 0)  diff += 1000;
            //Console.WriteLine("DIFF:" + diff);
            //

            //testing.TriangleTest2();
            //testing.TestRedirect();
            //room.game.testing.Gridsystem();
            //room.gridsystem.GenerateReachOffsets();
            //room.gridsystem.PrintOffsets(10);
            //long totalMemory = GC.GetTotalMemory(true);
            //Console.WriteLine(totalMemory);
            //room.gridsystem.GenerateAllReachOffsetsPerCoord(300);
            //long totalMemoryNew = GC.GetTotalMemory(true);
            //Console.WriteLine(totalMemoryNew);
            //
            //float m1 = Testing.ByteToMegabyte((int)totalMemory);
            //float m2 = Testing.ByteToMegabyte((int)totalMemoryNew);
            //Console.WriteLine("{0} - {1} = {2}", m2, m1, m2 - m1);
            //room.gridsystem.PrintOffsets(max: 300, x: 0, y: 0);
            //Testing.IndexsGridsystem();
            Testing.TestCountArray();
        }

        
        public void SetSpawnPosition()
        {
            spawnPos = UserInterface.WorldMousePos;
        }

        

        public void BatchSpawn()
        {
            int rad = 100;
            for (int i = 0; i < batchSpawnNum; i++)
            {
                int rx = Utils.random.Next(rad * 2) - rad;
                int ry = Utils.random.Next(rad * 2) - rad;
                room.spawnNode((int)UserInterface.WorldMousePos.X + rx, (int)UserInterface.WorldMousePos.Y + ry);
            }
        }

        public void DirectionalLaunch()
        {
            rightClickCount++;
            if (rightClickCount % rightClickMax == 0)
            {
                //Vector2 positionToSpawn = new Vector2(Game1.sWidth, Game1.sHeight);
                Vector2 positionToSpawn = spawnPos;
                //positionToSpawn /= (game.room.mapzoom * 2);
                //positionToSpawn /= (2);
                Vector2 diff = UserInterface.WorldMousePos;
                //diff *= room.zoom;
                diff = diff - positionToSpawn;
                //diff.Normalize();

                //new node(s)
                Dictionary<dynamic, dynamic> userP = new Dictionary<dynamic, dynamic>() {
                                
                            //{ node.texture, textures.whitecircle },
                            //{ node.radius, 12 },
                            { comp.movement, true },
                            //{ comp.randvelchange, true },
                            //{ comp.gravity, false },
                            { comp.lifetime, true },
                            //{ comp.transfer, true },
                            //{ comp.lasertimers, true },
                            //{ comp.laser, true },
                            //{ comp.wideray, true },
                            //{ comp.hueshifter, true },
                            //{ comp.phaseorb, true },
                            //{ comp.collision, false },
                            { nodeE.position, positionToSpawn },
                            { nodeE.velocity, diff },
                        };

                if (UserInterface.oldKeyBState.IsKeyDown(Keys.LeftControl))
                {
                    Action<Node> after = delegate(Node n) { 
                        n.body.velocity = diff;
                        if (n.body.velocity.IsFucked()) System.Diagnostics.Debugger.Break();
                    
                    }; 
                    room.spawnNode(userP, after);
                }
                else
                {
                    room.spawnNode(userP);
                }
                rightClickCount = 0;
            }
        }

        public void Serailize_Entities()//F1
        {
            string filename = "Presets//Rooms//entities.xml";
            room.game.serializer = new Polenter.Serialization.SharpSerializer();
            room.game.serializer.Serialize(room.masterGroup.fullSet, filename);

        }
        public void Deserailize_Entities()//F2
        {
            string filename = "Presets//Rooms//entities.xml";
            room.game.serializer = new Polenter.Serialization.SharpSerializer();
            ObservableHashSet<Node> ents = (ObservableHashSet<Node>)room.game.serializer.Deserialize(filename);

            Group g = OrbIt.ui.sidebar.ActiveGroup;
            //g.EmptyGroup();

            foreach (Node n in ents)
            {
                Node newNode = n.CreateClone(true);
                g.IncludeEntity(newNode);
            }
        }
        public void Serailize_Group()//F3
        {
            string filename = "Presets//Rooms//group1.xml";
            room.game.serializer = new Polenter.Serialization.SharpSerializer();
            room.game.serializer.Serialize(room.masterGroup, filename);

        }
        public void Deserailize_Group()//F4
        {
            string filename = "Presets//Rooms//group1.xml";
            room.game.serializer = new Polenter.Serialization.SharpSerializer();
            room.masterGroup.EmptyGroup();
            Group master = (Group)room.game.serializer.Deserialize(filename);
            room.masterGroup = master;

            OrbIt.ui.sidebar.UpdateGroupComboBoxes();
        }

        public void Serailize_Link()//F5
        {
            string filename = "Presets//Rooms//link1.xml";
            room.game.serializer = new Polenter.Serialization.SharpSerializer();
            room.game.serializer.Serialize(room._AllActiveLinks.ElementAt(0), filename);
        }
        public void Deserailize_Link()//F6
        {
            string filename = "Presets//Rooms//link1.xml";
            room.game.serializer = new Polenter.Serialization.SharpSerializer();
            try
            {
                Link lnk = (Link)room.game.serializer.Deserialize(filename);
                //room._AllActiveLinks = new ObservableHashSet<Link>();
                room._AllActiveLinks.Add(lnk);
                Console.WriteLine(room._AllActiveLinks.Count);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //if (e.InnerException != null)
                //{
                //    if (e.InnerException.InnerException != null)
                //    {
                //        throw e.InnerException.InnerException;
                //    }
                //    else
                //    {
                //        throw e.InnerException;
                //    }
                //}
                //else
                //{
                //    throw e;
                //}
            }
        }
        public void Serialize_Room()//F7
        {
            string filename = "Presets//Rooms//room1.xml";
            room.game.serializer = new Polenter.Serialization.SharpSerializer();
            room.game.serializer.Serialize(room, filename);
        }
        public void Deserialize_Room()//F8
        {
            Group.GroupNumber = 0;
            Node.nodeCounter = 0;

            string filename = "Presets//Rooms//room1.xml";
            room.game.serializer = new Polenter.Serialization.SharpSerializer();
            Room rm = (Room)room.game.serializer.Deserialize(filename);
            room.game.room = rm;
            

        }
        public void Serialize_Room_Binary()//F9
        {
            string filename = "Presets//Rooms//room1.bin";
            room.game.serializer = new Polenter.Serialization.SharpSerializer(true);
            room.game.serializer.Serialize(room, filename);
        }
        public void Deserialize_Room_Binary()//F10
        {
            string filename = "Presets//Rooms//room1.bin";
            room.game.serializer = new Polenter.Serialization.SharpSerializer(true);
            Room rm = (Room)room.game.serializer.Deserialize(filename);
            room.game.room = rm;

        }
        //==============================================================================

        
    }
}
