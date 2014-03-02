using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace OrbItProcs
{
    public class CameraControl : Process
    {
        Camera camera;
        public float velocity = 5f;
        public CameraControl(Camera camera)
        {
            this.camera = camera;
            Update += update;
        }

        public void update()
        {
            KeyboardState state = KeyManager.newKeyboardState;
            KeyboardState oldstate = KeyManager.oldKeyboardState;
            bool up = state.IsKeyDown(Keys.Up);// && !oldstate.IsKeyDown(Keys.Up);
            bool down = state.IsKeyDown(Keys.Down);// && !oldstate.IsKeyDown(Keys.Down);
            bool left = state.IsKeyDown(Keys.Left);// && !oldstate.IsKeyDown(Keys.Left);
            bool right = state.IsKeyDown(Keys.Right);// && !oldstate.IsKeyDown(Keys.Right);
            Vector2 vel = Vector2.Zero;

            if (up)
            {
                vel.Y -= velocity;
            }
            if (down)
            {
                vel.Y += velocity;
            }
            if (left)
            {
                vel.X -= velocity;
            }
            if (right)
            {
                vel.X += velocity;
            }
            if (vel.Length() > velocity)
            {
                vel *= 0.70710678118f;
            }

            camera.pos += vel;


        }

    }
}
