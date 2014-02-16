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
            addProcessKeyAction("SpawnNode", KeyCodes.LeftClick, OnPress: SpawnNode);
            addProcessKeyAction("SetSpawnPosition", KeyCodes.LeftShift, OnPress: SetSpawnPosition);
            addProcessKeyAction("BatchSpawn", KeyCodes.RightClick, OnHold: BatchSpawn);
            addProcessKeyAction("DirectionalLaunch", KeyCodes.LeftShift, KeyCodes.RightClick, OnHold: DirectionalLaunch);
            batchSpawnNum = 50;

            addProcessKeyAction("serial", KeyCodes.O, OnPress: Testing3);
            addProcessKeyAction("deserial", KeyCodes.P, OnPress: Testing4);
        }

        public void SetSpawnPosition ()
        {
            spawnPos = UserInterface.WorldMousePos;
        }
        
        /*public void Testing()
        {
            string filename = "Presets//Rooms//entities.xml";
            room.serializer.Serialize(room.masterGroup.fullSet, filename);

        }
        public void Testing2()
        {
            string filename = "Presets//Rooms//entities.xml";
            ObservableHashSet<Node> ents = (ObservableHashSet<Node>)room.serializer.Deserialize(filename);
            room.serializer = new Polenter.Serialization.SharpSerializer();
            
            //room.masterGroup.EmptyGroup();
            Group g = room.game.ui.sidebar.ActiveGroupFirst;
            //g.EmptyGroup();

            foreach (Node n in ents)
            {
                Node newNode = n.CreateClone();
                g.IncludeEntity(newNode);
            }
        }*/
        //public void Testing3()
        //{
        //    string filename = "Presets//Rooms//group1.xml";
        //    room.serializer.Serialize(room.masterGroup, filename);
        //
        //}
        //public void Testing4()
        //{
        //    string filename = "Presets//Rooms//group1.xml";
        //    Group master = (Group)room.serializer.Deserialize(filename);
        //    room.serializer = new Polenter.Serialization.SharpSerializer();
        //    room.masterGroup = master;
        //
        //    room.game.ui.sidebar.UpdateGroupComboBoxes();
        //}

        public void Testing3()
        {
            string filename = "Presets//Rooms//link1.xml";
            room.serializer.Serialize(room._AllActiveLinks.ElementAt(0), filename);

        }
        public void Testing4()
        {
            string filename = "Presets//Rooms//link1.xml";
            Link lnk = (Link)room.serializer.Deserialize(filename);
            room.serializer = new Polenter.Serialization.SharpSerializer();
            room._AllActiveLinks = new ObservableHashSet<Link>();
            room._AllActiveLinks.Add(lnk);
        }

        public void SpawnNode()
        {
            room.game.spawnNode((int)UserInterface.WorldMousePos.X, (int)UserInterface.WorldMousePos.Y);
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
                Vector2 diff = UserInterface.MousePos;
                diff *= room.mapzoom;
                diff = diff - positionToSpawn;
                //diff.Normalize();

                //new node(s)
                Dictionary<dynamic, dynamic> userP = new Dictionary<dynamic, dynamic>() {
                                
                            //{ node.texture, textures.whitecircle },
                            //{ node.radius, 12 },
                            { comp.randcolor, true },
                            { comp.movement, true },
                            //{ comp.randvelchange, true },
                            { comp.randinitialvel, true },
                            //{ comp.gravity, false },
                            { comp.lifetime, true },
                            //{ comp.transfer, true },
                            //{ comp.lasertimers, true },
                            //{ comp.laser, true },
                            //{ comp.wideray, true },
                            //{ comp.hueshifter, true },
                            //{ comp.phaseorb, true },
                            //{ comp.collision, false },
                            { node.position, positionToSpawn },
                            { node.velocity, diff },
                        };

                if (UserInterface.oldKeyBState.IsKeyDown(Keys.LeftControl))
                {
                    Action<Node> after = delegate(Node n) { 
                        n.body.velocity = diff;
                        if (n.body.velocity.IsFucked()) System.Diagnostics.Debugger.Break();
                    
                    }; 
                    room.game.spawnNode(userP, after);
                }
                else
                {
                    room.game.spawnNode(userP);
                }
                rightClickCount = 0;
            }
        }

        public void BatchSpawn()
        {
            if (rightClickCount > rightClickMax)
            {
                //new node(s)
                int rad = 100;
                for (int i = 0; i < batchSpawnNum; i++)
                {
                    int rx = Utils.random.Next(rad * 2) - rad;
                    int ry = Utils.random.Next(rad * 2) - rad;
                    room.game.spawnNode((int)UserInterface.WorldMousePos.X + rx, (int)UserInterface.WorldMousePos.Y + ry);
                }

                rightClickCount = 0;
            }
            else
            {
                rightClickCount++;
            }
        }
    }
}
