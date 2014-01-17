using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public class GroupSelect : Process
    {
        private Vector2 groupSelectionBoxOrigin;
        public HashSet<Node> groupSelectSet;

        public GroupSelect() : base()
        {
            LeftHold += LeftH;
            LeftClick += LeftC;

            //
            /*
            if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
            {
                groupSelectionBoxOrigin = new Vector2(mouseState.X, mouseState.Y);
                groupSelectionBoxOrigin *= room.mapzoom;
            }
            else if (mouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
            {
                bool ctrlDown = oldKeyBState.IsKeyDown(Keys.LeftControl);
                bool altDown = oldKeyBState.IsKeyDown(Keys.LeftAlt);
                if (altDown) ctrlDown = false;

                Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);
                mousePos *= room.mapzoom;
                //groupSelectionBoxOrigin *= room.mapzoom;

                float lowerx = Math.Min(mousePos.X, groupSelectionBoxOrigin.X);
                float upperx = Math.Max(mousePos.X, groupSelectionBoxOrigin.X);
                float lowery = Math.Min(mousePos.Y, groupSelectionBoxOrigin.Y);
                float uppery = Math.Max(mousePos.Y, groupSelectionBoxOrigin.Y);

                if (!ctrlDown && !altDown) groupSelectSet = new HashSet<Node>();

                foreach (Node n in room.masterGroup.fullSet.ToList())
                {
                    float xx = n.transform.position.X;
                    float yy = n.transform.position.Y;

                    if (xx >= lowerx && xx <= upperx
                     && yy >= lowery && yy <= uppery)
                    {
                        if (altDown)
                        {
                            if (groupSelectSet.Contains(n)) groupSelectSet.Remove(n);
                            else groupSelectSet.Add(n);
                        }
                        else
                        {
                            groupSelectSet.Add(n);
                        }
                    }
                }
                //System.Console.WriteLine(groupSelectSet.Count);

                room.addRectangleLines(lowerx, lowery, upperx, uppery);
            }

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);
                mousePos *= room.mapzoom;

                float lowerx = Math.Min(mousePos.X, groupSelectionBoxOrigin.X);
                float upperx = Math.Max(mousePos.X, groupSelectionBoxOrigin.X);
                float lowery = Math.Min(mousePos.Y, groupSelectionBoxOrigin.Y);
                float uppery = Math.Max(mousePos.Y, groupSelectionBoxOrigin.Y);

                room.addRectangleLines(lowerx, lowery, upperx, uppery);
            }
            */
        }

        public void LeftH()
        {
            Vector2 mousePos = UserInterface.WorldMousePos;

            float lowerx = Math.Min(mousePos.X, groupSelectionBoxOrigin.X);
            float upperx = Math.Max(mousePos.X, groupSelectionBoxOrigin.X);
            float lowery = Math.Min(mousePos.Y, groupSelectionBoxOrigin.Y);
            float uppery = Math.Max(mousePos.Y, groupSelectionBoxOrigin.Y);

            room.addRectangleLines(lowerx, lowery, upperx, uppery);
            //Console.WriteLine(mousePos.X + " " + glob.X);
        }

        public void LeftC(ButtonState buttonState)
        {
            if (buttonState == ButtonState.Pressed)
            {
                groupSelectionBoxOrigin = UserInterface.WorldMousePos;
            }
            else if (buttonState == ButtonState.Released)
            {
                bool ctrlDown = UserInterface.oldKeyBState.IsKeyDown(Keys.LeftControl);
                bool altDown = UserInterface.oldKeyBState.IsKeyDown(Keys.LeftAlt);
                if (altDown) ctrlDown = false;

                Vector2 mousePos = UserInterface.WorldMousePos;

                float lowerx = Math.Min(mousePos.X, groupSelectionBoxOrigin.X);
                float upperx = Math.Max(mousePos.X, groupSelectionBoxOrigin.X);
                float lowery = Math.Min(mousePos.Y, groupSelectionBoxOrigin.Y);
                float uppery = Math.Max(mousePos.Y, groupSelectionBoxOrigin.Y);

                if (!ctrlDown && !altDown) groupSelectSet = new HashSet<Node>();
                

                foreach (Node n in room.masterGroup.fullSet.ToList())
                {
                    float xx = n.transform.position.X;
                    float yy = n.transform.position.Y;

                    if (xx >= lowerx && xx <= upperx
                     && yy >= lowery && yy <= uppery)
                    {
                        if (altDown)
                        {
                            if (groupSelectSet.Contains(n)) groupSelectSet.Remove(n);
                            else groupSelectSet.Add(n);
                        }
                        else
                        {
                            groupSelectSet.Add(n);
                        }
                    }
                }
                //System.Console.WriteLine(groupSelectSet.Count);

                room.addRectangleLines(lowerx, lowery, upperx, uppery);
            }
        }
    }
}
