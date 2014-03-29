using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public enum ControlSide { left, right }
    public enum FullPadMode { mirrorMode, spellMode }
  
    public struct Stick
    {
        #region //Directional Vector Constants//
        static Vector2 v2Up = new Vector2(0, 1);
        static Vector2 v2UpLeft = new Vector2(-Utils.invRootOfTwo, Utils.invRootOfTwo);
        static Vector2 v2Left = new Vector2(-1, 0);
        static Vector2 v2DownLeft = new Vector2(-Utils.invRootOfTwo, -Utils.invRootOfTwo);
        static Vector2 v2Down = new Vector2(0, -1);
        static Vector2 v2DownRight = new Vector2(Utils.invRootOfTwo, -Utils.invRootOfTwo);
        static Vector2 v2Right = new Vector2(1, 0);
        static Vector2 v2UpRight = new Vector2(Utils.invRootOfTwo, Utils.invRootOfTwo);
        #endregion

        public Vector2 v2;
        public ButtonState up;
        public ButtonState down;
        public ButtonState left;
        public ButtonState right;

        public Stick(Vector2 sourceStick)
        {
            v2 = Vector2.Zero;
            up = ButtonState.Released;
            down = ButtonState.Released;
            left = ButtonState.Released;
            right = ButtonState.Released;

            v2 = sourceStick;
            if (v2.LengthSquared() < Controller.deadZone * Controller.deadZone) return;

            double angle = Math.Atan2(sourceStick.Y, sourceStick.X);
            int octant = ((int)Math.Round(8 * angle / (2 * Math.PI) + 9)) % 8; // TODO: test & clarify

            switch (octant)
            {
                case 0: up = ButtonState.Pressed; right = ButtonState.Pressed; break;
                case 1: up = ButtonState.Pressed; break;
                case 2: left = ButtonState.Pressed; up = ButtonState.Pressed; break;
                case 3: left = ButtonState.Pressed; break;
                case 4: down = ButtonState.Pressed; left = ButtonState.Pressed; break;
                case 5: down = ButtonState.Pressed; break;
                case 6: right = ButtonState.Pressed; down = ButtonState.Pressed; break;
                case 7: right = ButtonState.Pressed; break;
            }

        }
        public Stick(ButtonState up, ButtonState down, ButtonState left, ButtonState right)
        {
            v2 = Vector2.Zero;
            this.up = ButtonState.Released;
            this.down = ButtonState.Released;
            this.left = ButtonState.Released;
            this.right = ButtonState.Released;

            this.up = up; this.down = down; this.left = left; this.right = right;
            if (isCentered()) return;
            if (up == ButtonState.Pressed && down == ButtonState.Released)
            {
                if (right == ButtonState.Pressed && left == ButtonState.Released)
                {
                    v2 = v2UpRight; return;
                }
                else if (left == ButtonState.Pressed && right == ButtonState.Released)
                {
                    v2 = v2UpLeft; return;
                }
                else
                {
                    v2 = v2Up; return;
                }
            }
            else if (down == ButtonState.Pressed && up == ButtonState.Released)
            {
                if (right == ButtonState.Pressed && left == ButtonState.Released)
                {
                    v2 = v2DownRight; return;
                }
                else if (left == ButtonState.Pressed && right == ButtonState.Released)
                {
                    v2 = v2DownLeft; return;
                }
                else
                {
                    v2 = v2Down; return;
                }
            }
            else if (right == ButtonState.Pressed && left == ButtonState.Released)
            {
                v2 = v2Right; return;
            }
            else if (left == ButtonState.Pressed && right == ButtonState.Released)
            {
                v2 = v2Left; return;
            }
            else
            {
                v2 = Vector2.Zero;
            }
        }
        public static implicit operator Vector2(Stick s)
        {
            return s.v2;
        }
        public bool isCentered()
        {
            if (up == ButtonState.Released &&
                down == ButtonState.Released &&
                left == ButtonState.Released &&
                right == ButtonState.Released) return true;
            else return false;
        }
    }

    public struct HalfPadState
    {
        /// <summary>
        /// HalfPadLeft: LStick. HalfPadRight: RStick. SpellMode: LStick.
        /// </summary>
        public Stick stick1;
        /// <summary>
        /// HalfPadLeft: Dpad. HalfPadRight: ABXY. SpellMode: Dpad and RStick.
        /// </summary>
        public Stick stick2;
        /// <summary>
        /// HalfPadLeft: LBumper. HalfPadRight: RBumper. SpellMode: LBumper, RBumper and A.
        /// </summary>
        public ButtonState Btn1;
        /// <summary>
        /// HalfPadLeft: LTrigger. HalfPadRight: RTrigger. SpellMode: B, LTrigger and RTrigger 
        /// For Pressure Sensitive analog triggers, use Btn2AsTrigger.
        /// </summary>
        public ButtonState Btn2;
        /// <summary>
        /// HalfPadLeft: LTrigger. HalfPadRight: RTrigger. SpellMode: LTrigger and RTrigger.
        /// </summary>
        public float Btn2AsTrigger;
        /// <summary>
        /// HalfPadLeft: LStick press. HalfPadRight: RStick press. SpellMode: LStickPress, RStickPress and X.
        /// </summary>
        public ButtonState Btn3;
        /// <summary>
        /// HalfPadLeft: Select. HalfPadRight: Start. SpellMode: Select and Start
        /// </summary>
        public ButtonState BtnStart;

        public static HalfPadState NullPadState = new HalfPadState();


        public HalfPadState(ControlSide side, PlayerIndex controllerIndex)
        {
            if (side == ControlSide.left)
            {
                stick1 = new Stick(GamePad.GetState(controllerIndex).ThumbSticks.Left);
                stick2 = new Stick(GamePad.GetState(controllerIndex).DPad.Up,
                                   GamePad.GetState(controllerIndex).DPad.Down,
                                   GamePad.GetState(controllerIndex).DPad.Left,
                                   GamePad.GetState(controllerIndex).DPad.Right);

                Btn1 = GamePad.GetState(controllerIndex).Buttons.LeftShoulder;
                Btn2 = (GamePad.GetState(controllerIndex).Triggers.Left < Controller.deadZone) ? ButtonState.Released : ButtonState.Pressed; //TODO: test
                Btn2AsTrigger = GamePad.GetState(controllerIndex).Triggers.Left;
                Btn3 = GamePad.GetState(controllerIndex).Buttons.LeftStick;

                BtnStart = GamePad.GetState(controllerIndex).Buttons.Back;
            }
            else //if (side == ControlSide.right) 
            {
                stick1 = new Stick(GamePad.GetState(controllerIndex).ThumbSticks.Right);
                stick2 = new Stick(GamePad.GetState(controllerIndex).Buttons.Y,
                                   GamePad.GetState(controllerIndex).Buttons.A,
                                   GamePad.GetState(controllerIndex).Buttons.X,
                                   GamePad.GetState(controllerIndex).Buttons.B);

                Btn1 = GamePad.GetState(controllerIndex).Buttons.RightShoulder;
                Btn2 = (GamePad.GetState(controllerIndex).Triggers.Right < Controller.deadZone) ? ButtonState.Released : ButtonState.Pressed; //TODO: test
                Btn2AsTrigger = GamePad.GetState(controllerIndex).Triggers.Right;
                Btn3 = GamePad.GetState(controllerIndex).Buttons.RightStick;

                BtnStart = GamePad.GetState(controllerIndex).Buttons.Start;
            }
        }

        public HalfPadState(FullPadMode mode, PlayerIndex controllerIndex)
        {
            if (mode == FullPadMode.mirrorMode)
            {
                stick1 = new Stick(GamePad.GetState(controllerIndex).Buttons.Y,
                                       GamePad.GetState(controllerIndex).Buttons.A,
                                       GamePad.GetState(controllerIndex).Buttons.X,
                                       GamePad.GetState(controllerIndex).Buttons.B);
                if (stick1.isCentered())
                {
                    stick1 = new Stick(GamePad.GetState(controllerIndex, GamePadDeadZone.Circular).ThumbSticks.Left);
                }

                stick2 = new Stick(GamePad.GetState(controllerIndex, GamePadDeadZone.Circular).ThumbSticks.Right);
                if (stick2.isCentered())
                {
                    stick2 = new Stick(GamePad.GetState(controllerIndex).DPad.Up,
                                       GamePad.GetState(controllerIndex).DPad.Down,
                                       GamePad.GetState(controllerIndex).DPad.Left,
                                       GamePad.GetState(controllerIndex).DPad.Right);
                }

                Btn1 = GamePad.GetState(controllerIndex).Buttons.RightShoulder;
                if (Btn1 == ButtonState.Released) Btn1 = GamePad.GetState(controllerIndex).Buttons.LeftShoulder;

                Btn2 = (GamePad.GetState(controllerIndex).Triggers.Right < Controller.deadZone) ? ButtonState.Released : ButtonState.Pressed; //TODO: test
                if (Btn2 == ButtonState.Released) Btn2 = (GamePad.GetState(controllerIndex).Triggers.Left < Controller.deadZone) ? ButtonState.Released : ButtonState.Pressed;

                Btn2AsTrigger = Math.Max(GamePad.GetState(controllerIndex).Triggers.Right, GamePad.GetState(controllerIndex).Triggers.Left);

                Btn3 = GamePad.GetState(controllerIndex).Buttons.RightStick;
                if (Btn3 == ButtonState.Released) Btn3 = GamePad.GetState(controllerIndex).Buttons.LeftStick;

                BtnStart = GamePad.GetState(controllerIndex).Buttons.Start;
                if (BtnStart == ButtonState.Released) BtnStart = GamePad.GetState(controllerIndex).Buttons.Back;
            }
            else
            {
                stick1 = new Stick(GamePad.GetState(controllerIndex, GamePadDeadZone.Circular).ThumbSticks.Left);
                stick2 = new Stick(GamePad.GetState(controllerIndex, GamePadDeadZone.Circular).ThumbSticks.Right);
                if (stick2.isCentered())
                {
                    stick2 = new Stick(GamePad.GetState(controllerIndex).DPad.Up,
                                       GamePad.GetState(controllerIndex).DPad.Down,
                                       GamePad.GetState(controllerIndex).DPad.Left,
                                       GamePad.GetState(controllerIndex).DPad.Right);
                }

                Btn1 = GamePad.GetState(controllerIndex).Buttons.RightShoulder;
                if (Btn1 == ButtonState.Released) Btn1 = GamePad.GetState(controllerIndex).Buttons.LeftShoulder;
                if (Btn1 == ButtonState.Released) Btn1 = GamePad.GetState(controllerIndex).Buttons.A;

                Btn2 = (GamePad.GetState(controllerIndex).Triggers.Right < Controller.deadZone) ? ButtonState.Released : ButtonState.Pressed; //TODO: test
                if (Btn2 == ButtonState.Released) Btn2 = (GamePad.GetState(controllerIndex).Triggers.Left < Controller.deadZone) ? ButtonState.Released : ButtonState.Pressed;
                if (Btn2 == ButtonState.Released) Btn2 = GamePad.GetState(controllerIndex).Buttons.B;
                if (Btn2 == ButtonState.Released) Btn2 = GamePad.GetState(controllerIndex).Buttons.Y;

                Btn2AsTrigger = Math.Max(GamePad.GetState(controllerIndex).Triggers.Right, GamePad.GetState(controllerIndex).Triggers.Left);
                if (GamePad.GetState(controllerIndex).Buttons.B == ButtonState.Pressed ||
                    GamePad.GetState(controllerIndex).Buttons.Y == ButtonState.Pressed) Btn2AsTrigger = 1.0f;

                Btn3 = GamePad.GetState(controllerIndex).Buttons.RightStick;
                if (Btn3 == ButtonState.Released) Btn3 = GamePad.GetState(controllerIndex).Buttons.LeftStick;
                if (Btn3 == ButtonState.Released) Btn3 = GamePad.GetState(controllerIndex).Buttons.X;

                BtnStart = GamePad.GetState(controllerIndex).Buttons.Start;
                if (BtnStart == ButtonState.Released) BtnStart = GamePad.GetState(controllerIndex).Buttons.Back;

            }
        }

    }
    static class ControllerExtensions {
        public static bool isAvailable(this Controller.ControllerCodes code) { return (Controller.availableControllers & code) != 0; }
        public static bool isAvailable(this int code) { return ((int)Controller.availableControllers & code) != 0; }
    }
    public abstract class Controller
    {
        [Flags]
        public enum ControllerCodes
        {
            None = 0,                                           //  00000000
            FirstLeft = 1,                                      //  00000001
            FirstRight = 1 << 1,                                //  00000010
            SecondLeft = 1 << 2,                                //  00000100
            SecondRight = 1 << 3,                               //  00001000
            ThirdLeft = 1 << 4,                                 //  00010000
            ThirdRight = 1 << 5,                                //  00100000
            FourthLeft = 1 << 6,                                //  01000000
            FourthRight = 1 << 7,                               //  10000000
            First = FirstLeft | FirstRight,                     //  00000011
            Second = SecondLeft | SecondRight,                  //  00001100
            Third = ThirdLeft | ThirdRight,                     //  00110000
            Fourth = FourthLeft | FourthRight,                  //  11000000
            All = First | Second | Third | Fourth               //  11111111
        }
        public static List<HalfController> halfControllers = new List<HalfController>();
        public static List<FullController> fullControllers = new List<FullController>();
        public static ControllerCodes availableControllers = ControllerCodes.All;
        public const float deadZone = 0.2f;
        public const int maxControllers = 4;

        public int playerNum;
        protected PlayerIndex controllerIndex;
        protected ControllerCodes controllerCode;
        public bool enabled = true;

        public static void ResetControllers()
        {
            availableControllers = ControllerCodes.All;
            halfControllers = new List<HalfController>();
            fullControllers = new List<FullController>();
        }

        public static Dictionary<int, PlayerIndex> intToPlayerIndex =
            new Dictionary<int, PlayerIndex>(){
            {1, PlayerIndex.One},
            {2, PlayerIndex.Two},
            {3, PlayerIndex.Three},
            {4, PlayerIndex.Four}};
        public abstract Vector2 getLeftStick();
        public abstract Vector2 getRightStick();
        public static int connectedControllers()
        {
            for (int i = 1; i <= 4; i++)
                if (!GamePad.GetState(intToPlayerIndex[i]).IsConnected)
                    return i - 1;
            return 0;
        }
        protected void assign(ControllerCodes controller)
        {
            controllerCode = controller;
            availableControllers ^= controller;
        }
        public virtual void unassign()
        {
            availableControllers = availableControllers | controllerCode;
            controllerCode = ControllerCodes.None;
        }
        public virtual void UpdateNewState() { }
        public virtual void UpdateOldState() { }
    }

    public class FullController : Controller
    {
        public GamePadState newGamePadState;
        public GamePadState oldGamePadState;
        public static FullController GetNew(int player)
        {
            bool win = false;
            FullController f = new FullController(player, ref win);
            return win ? f : null;
        }
        private FullController(int player, ref bool success) {
            fullControllers.Add(this);
            
            this.playerNum = player;
            if (player > connectedControllers())
            {
                success = false;
                return;
            }

            if (ControllerCodes.First.isAvailable())
            {
                controllerIndex = PlayerIndex.One;
                assign(ControllerCodes.First);
            }
            else if (ControllerCodes.Second.isAvailable())
            {
                controllerIndex = PlayerIndex.Two;
                assign(ControllerCodes.Second);
            }
            else if (ControllerCodes.Third.isAvailable())
            {
                controllerIndex = PlayerIndex.Three;
                assign(ControllerCodes.Third);
            }
            else if (ControllerCodes.Fourth.isAvailable())
            {
                controllerIndex = PlayerIndex.Four;
                assign(ControllerCodes.Fourth);
            }
            else
            {
                PopUp.Toast("Insufficient controllers! Player "+ player +" will not work!");
                enabled = false;
                success = false;
                return;
            }
            success = true;
        }
        public GamePadState getState()
        {
            if (enabled == false) return new GamePadState();
            return GamePad.GetState(controllerIndex, GamePadDeadZone.Circular);;

             
        }
        public override Vector2 getRightStick()
        {
            return newGamePadState.ThumbSticks.Right * new Vector2(1,-1);
        }
        public override Vector2 getLeftStick()
        {
            return newGamePadState.ThumbSticks.Left * new Vector2(1, -1);
        }
        public override void UpdateNewState()
        {
            newGamePadState = getState();
        }
        public override void UpdateOldState()
        {
            oldGamePadState = newGamePadState;
        }
    }

    public class HalfController : Controller
    {
        public HalfPadState newHalfPadState;
        public HalfPadState oldHalfPadState;
        public ControlSide side;
        public FullPadMode fullPadMode;
        bool fullControllerAvailable;
        public static HalfController GetNew(int player, FullPadMode mode = FullPadMode.spellMode)
        {
            bool win = false;
            HalfController h = new HalfController(player, ref win, mode);
            return win ? h : null;
        }

        private HalfController(int player, ref bool assign, FullPadMode mode = FullPadMode.spellMode)
        {
            halfControllers.Add(this);
            this.fullPadMode = mode;
            this.playerNum = player;
            assign = reassign();
        }
        public override Vector2 getRightStick()
        {
            return newHalfPadState.stick1 * new Vector2(1, -1);
        }
        public override Vector2 getLeftStick()
        {
            return newHalfPadState.stick2 * new Vector2(1, -1);
        }
        public HalfPadState getState()
        {
            if (enabled == false) return HalfPadState.NullPadState;
            if (fullControllerAvailable == true)
            {
                    return new HalfPadState(fullPadMode, controllerIndex);
            }
            return new HalfPadState(side, controllerIndex);
        }
        public override void UpdateNewState()
        {
            newHalfPadState = getState();
        }
        public override void UpdateOldState()
        {
            oldHalfPadState = newHalfPadState;
        }
        public override void unassign(){
            if (side == ControlSide.right)
                halfControllers.First(x => x.controllerCode == (ControllerCodes)((int)controllerCode << 1)).fullControllerAvailable = true;
            base.unassign();
        }
        public bool reassign()
        {
            for (int j = 0; j < maxControllers*2; j++)
            {
                Console.WriteLine("j = " + j);
                int i = (j * 2) % ((maxControllers*2 - 1) + j / (maxControllers*2 - 1)); //magic
                if (i >= connectedControllers() * 2)
                {
                    if (j < maxControllers)
                    {
                        j = maxControllers - 1;
                        continue;
                    }
                    else
                    {
                        PopUp.Toast("Insufficient controllers! Player " + playerNum + " will not work!");
                        enabled = false;

                        return false;
                    }
                }

                if ((1 << i).isAvailable())
                {
                    controllerIndex = intToPlayerIndex[(i / 2) + 1];
                    if ((i % 2) == 0)
                    {
                        side = ControlSide.left;
                    }
                    else
                    {
                        side = ControlSide.right;
                        fullControllerAvailable = false;
                        halfControllers.First(x => x.controllerCode == (ControllerCodes)((int)controllerCode >> 1)).fullControllerAvailable = false;
                    }
                    assign((ControllerCodes)(1 << i));
                    return true;
                }
            }
            if (!GamePad.GetState(controllerIndex).IsConnected)
                PopUp.Toast("Warning: Player " + playerNum + " is disconnected.");
            return false;
        }

    }
}
