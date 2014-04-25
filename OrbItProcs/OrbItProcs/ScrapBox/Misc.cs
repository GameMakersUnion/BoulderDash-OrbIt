using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OrbItProcs;
using Microsoft.Xna.Framework;

namespace OrbItProcs.ScrapBox
{
    class Misc
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


        //public Stick(ButtonState up, ButtonState down, ButtonState left, ButtonState right)
        //{
        //    v2 = Vector2.Zero;
        //    //this.up = ButtonState.Released;
        //    //this.down = ButtonState.Released;
        //    //this.left = ButtonState.Released;
        //    //this.right = ButtonState.Released;
        //
        //    this.up = up; this.down = down; this.left = left; this.right = right;
        //    if (isCentered()) return;
        //    if (up == ButtonState.Pressed && down == ButtonState.Released)
        //    {
        //        if (right == ButtonState.Pressed && left == ButtonState.Released)
        //        {
        //            v2 = v2UpRight; return;
        //        }
        //        else if (left == ButtonState.Pressed && right == ButtonState.Released)
        //        {
        //            v2 = v2UpLeft; return;
        //        }
        //        else
        //        {
        //            v2 = v2Up; return;
        //        }
        //    }
        //    else if (down == ButtonState.Pressed && up == ButtonState.Released)
        //    {
        //        if (right == ButtonState.Pressed && left == ButtonState.Released)
        //        {
        //            v2 = v2DownRight; return;
        //        }
        //        else if (left == ButtonState.Pressed && right == ButtonState.Released)
        //        {
        //            v2 = v2DownLeft; return;
        //        }
        //        else
        //        {
        //            v2 = v2Down; return;
        //        }
        //    }
        //    else if (right == ButtonState.Pressed && left == ButtonState.Released)
        //    {
        //        v2 = v2Right; return;
        //    }
        //    else if (left == ButtonState.Pressed && right == ButtonState.Released)
        //    {
        //        v2 = v2Left; return;
        //    }
        //    else
        //    {
        //        v2 = Vector2.Zero;
        //    }
        //}

        public void AssignColor(Group activegroup, Node newNode)
        {
            if (Group.IntToColor.ContainsKey(activegroup.GroupId))
            {
                newNode.body.color = Group.IntToColor[activegroup.GroupId];
            }
            else
            {
                int Enumsize = Enum.GetValues(typeof(System.Drawing.KnownColor)).Length;

                //int rand = Utils.random.Next(size - 1);
                int index = 0;
                foreach (char c in activegroup.Name.ToCharArray().ToList())
                {
                    index += (int)c;
                }
                index = index % (Enumsize - 1);

                System.Drawing.Color syscolor = System.Drawing.Color.FromKnownColor((System.Drawing.KnownColor)index);
                Color xnacol = new Color(syscolor.R, syscolor.G, syscolor.B, syscolor.A);
                newNode.body.color = xnacol;
            }
        }
    }
}
