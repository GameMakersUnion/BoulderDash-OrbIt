﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public class SingleSelect : Process
    {
        

        public SingleSelect() : base()
        {
            //LeftClick += LeftC;
            //RightClick += RightC;
            //MiddleClick += MiddleC;
            addProcessKeyAction("SingleSel", KeyCodes.LeftClick, OnPress: SingleSel);
            addProcessKeyAction("MakeLink", KeyCodes.RightClick, OnPress: MakeLink);
            addProcessKeyAction("StartLink", KeyCodes.MiddleClick, OnPress: StartLink);

        }

        

        public void SingleSel()
        {
            //if (buttonState == ButtonState.Released) return;
            Node found = room.SelectNodeAt(UserInterface.WorldMousePos);
            OrbIt.ui.sidebar.SetTargetNode(found);

        }
        public void MakeLink()
        {
            //if (buttonState == ButtonState.Released) return;
            Node found = room.SelectNodeAt(UserInterface.WorldMousePos);
            if (found != null)
            {
                if (room.targetNode != null && room.targetNode.HasComp<Flow>())
                {
                    room.targetNode.Comp<Flow>().AddToOutgoing(found);
                }
                if (room.targetNode != null && room.targetNode.HasComp<Tether>())
                {
                    room.targetNode.Comp<Tether>().AddToOutgoing(found);
                }
            }
        }
        public void StartLink()
        {
            //if (buttonState == ButtonState.Released) return;
            Node found = room.SelectNodeAt(UserInterface.WorldMousePos);
            if (found != null)
            {
                if (found.HasComp<Flow>())
                {
                    found.Comp<Flow>().activated = !found.Comp<Flow>().activated;
                }
                if (found.HasComp<Tether>())
                {
                    found.Comp<Tether>().activated = !found.Comp<Tether>().activated;
                }
            }
        }
    }
}
