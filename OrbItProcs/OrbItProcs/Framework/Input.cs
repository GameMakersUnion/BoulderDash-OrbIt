using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OrbItProcs
{
    public struct InputState
    {
        public readonly Stick LeftStick_WASD, RightStick_Mouse;
        public readonly ButtonState A_1, X_2, B_3, Y_4;
        public readonly ButtonState Dpad_UpArrow, Dpad_DownArrow, Dpad_RightArrow, Dpad_LeftArrow;
        public readonly ButtonState Select_TAB, Start_ESC;
        public readonly ButtonState LeftBumper_Q, RightBumper_E;
        public readonly ButtonState LeftTrigger_Mouse2, RightTrigger_Mouse1;
        public readonly float LeftTriggerAnalog, RightTriggerAnalog;

        /*public InputState(Stick LeftStick_WASD, Stick RightStick_Mouse,
                          ButtonState A_1, ButtonState X_2, ButtonState B_3, ButtonState Y_4,
                          ButtonState Dpad_UpArrow, ButtonState Dpad_DownArrow, ButtonState Dpad_RightArrow, ButtonState Dpad_LeftArrow,
                          ButtonState Select_TAB, ButtonState Start_ESC,
                          ButtonState LeftBumper_Q, ButtonState RightBumper_E,
                          ButtonState LeftTrigger_Mouse2, ButtonState RightTrigger_Mouse1,
                          float LeftTriggerAnalog = 0, float RightTriggerAnalog = 0)
        {
            this.LeftStick_WASD = LeftStick_WASD;
            this.RightStick_Mouse = RightStick_Mouse;
            this.A_1 = A_1;
            this.X_2 = X_2;
            this.B_3 = B_3;
            this.Y_4 = Y_4;
            this.Dpad_UpArrow = Dpad_UpArrow;
            this.Dpad_DownArrow = Dpad_DownArrow;
            this.Dpad_RightArrow = Dpad_RightArrow;
            this.Dpad_LeftArrow = Dpad_LeftArrow;
            this.Select_TAB = Select_TAB;
            this.Start_ESC = Start_ESC;
            this.LeftBumper_Q = LeftBumper_Q;
            this.RightBumper_E = RightBumper_E;
            this.LeftTrigger_Mouse2 = LeftTrigger_Mouse2;
            this.RightTrigger_Mouse1 = RightTrigger_Mouse1;
            this.LeftTriggerAnalog = LeftTriggerAnalog;
            this.RightTriggerAnalog = RightTriggerAnalog;
        }*/
        //keyboard/mouse
        public InputState(Stick LeftStick_WASD, Stick RightStick_Mouse,
                          ButtonState LeftTrigger_Mouse2, ButtonState RightTrigger_Mouse1,
                          bool A_1, bool X_2, bool B_3, bool Y_4,
                          bool Dpad_UpArrow, bool Dpad_DownArrow, bool Dpad_RightArrow, bool Dpad_LeftArrow,
                          bool Select_TAB, bool Start_ESC,
                          bool LeftBumper_Q, bool RightBumper_E,
                          float LeftTriggerAnalog = 0, float RightTriggerAnalog = 0)
        {
            this.LeftStick_WASD = LeftStick_WASD;
            this.RightStick_Mouse = RightStick_Mouse;
            this.LeftTrigger_Mouse2 = LeftTrigger_Mouse2;
            this.RightTrigger_Mouse1 = RightTrigger_Mouse1;
            this.A_1 = A_1 ? ButtonState.Pressed : ButtonState.Released;
            this.X_2 = X_2 ? ButtonState.Pressed : ButtonState.Released;
            this.B_3 = B_3 ? ButtonState.Pressed : ButtonState.Released;
            this.Y_4 = Y_4 ? ButtonState.Pressed : ButtonState.Released;
            this.Dpad_UpArrow = Dpad_UpArrow ? ButtonState.Pressed : ButtonState.Released;
            this.Dpad_DownArrow = Dpad_DownArrow ? ButtonState.Pressed : ButtonState.Released;
            this.Dpad_RightArrow = Dpad_RightArrow ? ButtonState.Pressed : ButtonState.Released;
            this.Dpad_LeftArrow = Dpad_LeftArrow ? ButtonState.Pressed : ButtonState.Released;
            this.Select_TAB = Select_TAB ? ButtonState.Pressed : ButtonState.Released;
            this.Start_ESC = Start_ESC ? ButtonState.Pressed : ButtonState.Released;
            this.LeftBumper_Q = LeftBumper_Q ? ButtonState.Pressed : ButtonState.Released;
            this.RightBumper_E = RightBumper_E ? ButtonState.Pressed : ButtonState.Released;
            this.LeftTriggerAnalog = LeftTriggerAnalog;
            this.RightTriggerAnalog = RightTriggerAnalog;
        }
        //controller
        public InputState(ref GamePadState state, float triggerDeadZone)
        {
            this.LeftStick_WASD = new Stick(state.ThumbSticks.Left);
            this.LeftStick_WASD.v2.Y *= -1; //todo: fix directional buttonstates?
            this.RightStick_Mouse = new Stick(state.ThumbSticks.Right);
            this.RightStick_Mouse.v2.Y *= -1;
            this.LeftTrigger_Mouse2 = state.Triggers.Left > triggerDeadZone ? ButtonState.Pressed : ButtonState.Released;
            this.RightTrigger_Mouse1 = state.Triggers.Right > triggerDeadZone ? ButtonState.Pressed : ButtonState.Released;
            this.A_1 = state.Buttons.A;
            this.X_2 = state.Buttons.X;
            this.B_3 = state.Buttons.B;
            this.Y_4 = state.Buttons.Y;
            this.Dpad_UpArrow = state.DPad.Up;
            this.Dpad_DownArrow = state.DPad.Down;
            this.Dpad_RightArrow = state.DPad.Right;
            this.Dpad_LeftArrow = state.DPad.Left;
            this.Select_TAB = state.Buttons.Back;
            this.Start_ESC = state.Buttons.Start;
            this.LeftBumper_Q = state.Buttons.LeftShoulder;
            this.RightBumper_E = state.Buttons.RightShoulder;
            this.LeftTriggerAnalog = state.Triggers.Left;
            this.RightTriggerAnalog = state.Triggers.Right;
        }

    }


    public abstract class Input
    {
        public InputState newInputState, oldInputState;
        public Player player;
        public abstract InputState GetState();
        public virtual Vector2 GetLeftStick()
        {
            return newInputState.LeftStick_WASD.v2;
        }
        public virtual Vector2 GetRightStick()
        {
            return newInputState.RightStick_Mouse.v2;
        }
        public virtual void SetOldState()
        {
            oldInputState = newInputState;
        }
    }
    public class PcFullInput : Input
    {
        public KeyboardState oldKeyState, newKeyState;
        public MouseState oldMouseState, newMouseState;
        
        public float mouseStickRadius;

        public PcFullInput(Player player, float mouseStickRadius)
        {
            this.player = player;
            this.mouseStickRadius = mouseStickRadius;
        }
        public override InputState GetState()
        {
            newKeyState = Keyboard.GetState();
            newMouseState = Mouse.GetState();
            Stick LeftStick_WASD = new Stick(newKeyState.IsKeyDown(Keys.Up), newKeyState.IsKeyDown(Keys.Down), newKeyState.IsKeyDown(Keys.Left), newKeyState.IsKeyDown(Keys.Right));
            
            Vector2 mousePos = new Vector2(newMouseState.X, newMouseState.Y);
            Vector2 playerPos = player.node.body.pos * player.room.camera.zoom; // divide?
            Vector2 dir = mousePos - playerPos;
            float lensqr = dir.LengthSquared();
            if (lensqr > mouseStickRadius * mouseStickRadius)
            {
                VMath.NormalizeSafe(ref dir);
                dir *= mouseStickRadius;
            }
            else
            {
                dir /= mouseStickRadius;
            }
            Stick RightStick_Mouse = new Stick(dir);
            newInputState = new InputState(LeftStick_WASD, RightStick_Mouse, newMouseState.RightButton, newMouseState.LeftButton,
                                           newKeyState.IsKeyDown(Keys.D1), newKeyState.IsKeyDown(Keys.D2), newKeyState.IsKeyDown(Keys.D3), newKeyState.IsKeyDown(Keys.D4),
                                           newKeyState.IsKeyDown(Keys.Up), newKeyState.IsKeyDown(Keys.Down), newKeyState.IsKeyDown(Keys.Right), newKeyState.IsKeyDown(Keys.Left),
                                           newKeyState.IsKeyDown(Keys.Tab), newKeyState.IsKeyDown(Keys.Escape), newKeyState.IsKeyDown(Keys.Q), newKeyState.IsKeyDown(Keys.E));
            return newInputState;
        }
        public override void SetOldState()
        {
            base.SetOldState();
            oldKeyState = newKeyState;
            oldMouseState = newMouseState;
        }
    }


    public class ControllerFullInput : Input
    {
        public GamePadState newGamePadState, oldGamePadState;
        public PlayerIndex playerIndex = 0;
        public float triggerDeadZone = 0.5f;
        public override InputState GetState()
        {
            newGamePadState = GamePad.GetState(playerIndex, GamePadDeadZone.Circular);
            
            newInputState = new InputState(ref newGamePadState, triggerDeadZone);
            return newInputState;
        }
        public override void SetOldState()
        {
            base.SetOldState();
            oldGamePadState = newGamePadState;
        }
    }
}
