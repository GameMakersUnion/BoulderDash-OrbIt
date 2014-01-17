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

        public SpawnNodes() : base()
        {
            LeftClick += LeftC;
            KeyEvent += KeyEv;
            RightHold += RightH;
        }

        public void KeyEv (Dictionary<dynamic,dynamic> args)
        {
            if (UserInterface.keybState.IsKeyDown(Keys.LeftShift) && !UserInterface.oldKeyBState.IsKeyDown(Keys.LeftShift))
            {
                spawnPos = UserInterface.WorldMousePos;
            }

        }

        public void LeftC(ButtonState buttonState)
        {
            if (buttonState == ButtonState.Released) return;

            if (!UserInterface.keybState.IsKeyDown(Keys.LeftShift))
            {
                room.game.spawnNode((int)UserInterface.WorldMousePos.X, (int)UserInterface.WorldMousePos.Y);
            }
        }

        public void RightH()
        {
            if (UserInterface.keybState.IsKeyDown(Keys.LeftShift))
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
                        Action<Node> after = delegate(Node n) { n.transform.velocity = diff; }; room.game.spawnNode(userP, after);
                    }
                    else
                    {
                        room.game.spawnNode(userP);
                    }




                    rightClickCount = 0;
                }
            }
            else
            {
                if (rightClickCount > rightClickMax)
                {
                    //new node(s)
                    int rad = 100;
                    for (int i = 0; i < 10; i++)
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
}
