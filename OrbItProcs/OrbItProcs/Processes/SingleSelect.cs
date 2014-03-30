using Microsoft.Xna.Framework;
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

        public Node SelectNode(Vector2 pos)
        {
            Node found = null;
            float shortedDistance = Int32.MaxValue;
            for (int i = room.masterGroup.fullSet.Count - 1; i >= 0; i--)
            {
                Node n = (Node)room.masterGroup.fullSet.ElementAt(i);
                // find node that has been clicked, starting from the most recently placed nodes
                float distsquared = Vector2.DistanceSquared(n.body.pos, pos);
                if (distsquared < n.body.radius * n.body.radius)
                {
                    if (distsquared < shortedDistance)
                    {
                        found = n;
                        shortedDistance = distsquared;
                    }

                }
            }
            return found;
        }

        public void SingleSel()
        {
            //if (buttonState == ButtonState.Released) return;
            Node found = SelectNode(UserInterface.WorldMousePos);
            Game1.ui.sidebar.SetTargetNode(found);

        }
        public void MakeLink()
        {
            //if (buttonState == ButtonState.Released) return;
            Node found = SelectNode(UserInterface.WorldMousePos);
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
            else
            {
            }
        }
        public void StartLink()
        {
            //if (buttonState == ButtonState.Released) return;
            Node found = SelectNode(UserInterface.WorldMousePos);
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
            else
            {

            }
        }
    }
}
