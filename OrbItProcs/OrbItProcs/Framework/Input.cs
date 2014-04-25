using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OrbItProcs
{
    public enum PadSupport
    {
        supported,
        notSupported,
        notImplemented,
    }

    public abstract class Input
    {
        protected abstract PadSupport SupportsHalfPad { get; }
        protected abstract PadSupport SupportsFullPad { get; }
        protected abstract PadSupport SupportsPointer { get; }
        protected virtual PadSupport SupportsGesture { get { return PadSupport.notImplemented; } }

        public FullPadState newInputState, oldInputState;

        public Player player;
        public abstract FullPadState GetState();
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
        public override FullPadState GetState()
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
            newInputState = new FullPadState(LeftStick_WASD, RightStick_Mouse, newMouseState.RightButton, newMouseState.LeftButton,
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
        public override FullPadState GetState()
        {
            newGamePadState = GamePad.GetState(playerIndex, GamePadDeadZone.Circular);
            
            newInputState = new FullPadState(ref newGamePadState, triggerDeadZone);
            return newInputState;
        }
        public override void SetOldState()
        {
            base.SetOldState();
            oldGamePadState = newGamePadState;
        }
    }
}
