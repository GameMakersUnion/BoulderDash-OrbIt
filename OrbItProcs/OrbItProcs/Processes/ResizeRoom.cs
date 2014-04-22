using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public class ResizeRoom : Process
    {
        public ResizeRoom()
            : base()
        {
            addProcessKeyAction("Resize", KeyCodes.LeftClick, OnPress: resize);

        }
        public void resize()
        {
            OrbIt.game.room.resize(UserInterface.WorldMousePos);
        }

    }
}
