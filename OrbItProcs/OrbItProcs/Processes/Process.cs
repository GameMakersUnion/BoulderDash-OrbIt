using Microsoft.Xna.Framework.Input;
using OrbItProcs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbItProcs
{
    public delegate void ProcessMethod (Dictionary<dynamic,dynamic> args); // to be 'classoverloaded' later
    

    public class Process
    {
        public Room room;

        public event ProcessMethod Update;
        public event ProcessMethod Create;
        public event ProcessMethod Destroy;
        public event ProcessMethod Collision;
        public event ProcessMethod OutOfBounds;

        public event Action<ButtonState> LeftClick;
        public event Action<ButtonState> RightClick;
        public event Action<ButtonState> MiddleClick;
        public event Action<int> Scroll;

        public event Action LeftHold;
        public event Action RightHold;
        public event Action MiddleHold;

        

        public event ProcessMethod KeyEvent;

        private bool leftHold = false, rightHold = false, middleHold = false;

        public List<Process> procs = new List<Process>();
        //Process parentprocess; //I bet you a coke -Dante
        public Dictionary<dynamic, dynamic> pargs;
        public Dictionary<dynamic, ProcessMethod> pmethods;

        public Process()
        { 
            // / // / //
            room = Program.getRoom();
        }

        public void OnUpdate()
        {
            foreach (Process p in procs)
            {
                p.OnUpdate();
            }
            if (Update != null) Update(pargs);

            if (leftHold) InvokeLeftHold();
            if (rightHold) InvokeRightHold();
            if (middleHold) InvokeMiddleHold();
            

        }

        public void InvokeKeyEvent(Dictionary<dynamic,dynamic> args)
        {
            if (KeyEvent != null)
            {
                KeyEvent(args);
            }
        }

        public void InvokeLeftClick(ButtonState buttonState)
        {
            if (buttonState == ButtonState.Pressed) leftHold = true;
            else leftHold = false;

            if (LeftClick != null)
            {
                LeftClick(buttonState);
            }
            //probably update all the child process clicks, but make sure they aren't already being handled by the manager
        }
        public void InvokeRightClick(ButtonState buttonState)
        {
            if (buttonState == ButtonState.Pressed) rightHold = true;
            else rightHold = false;

            if (RightClick != null)
            {
                RightClick(buttonState);
            }
        }
        public void InvokeMiddleClick(ButtonState buttonState)
        {
            if (buttonState == ButtonState.Pressed) middleHold = true;
            else middleHold = false;

            if (MiddleClick != null)
            {
                MiddleClick(buttonState);
            }
        }
        public void InvokeScroll(int scrollval)
        {
            if (Scroll != null)
            {
                Scroll(scrollval);
            }
        }
        public void InvokeLeftHold()
        {
            if (LeftHold != null)
            {
                LeftHold();
            }
            //probably update all the child process clicks, but make sure they aren't already being handled by the manager
        }
        public void InvokeRightHold()
        {
            if (RightHold != null)
            {
                RightHold();
            }
        }
        public void InvokeMiddleHold()
        {
            if (MiddleHold != null)
            {
                MiddleHold();
            }
        }

        public void Add(Process p)
        {
            procs.Add(p);
            p.OnCreate();

        }

        public void OnCreate()
        {
            if (Create != null) Create(pargs);
        }

        public void OnCollision(Dictionary<dynamic,dynamic> args)
        {
            if (Collision != null)
            {
                Collision(args);
            }
        }

        public void Remove(Process p)
        {
            p.OnDestroy();
            procs.Remove(p);
        }

        public void OnDestroy()
        {
            if (Destroy != null) Destroy(pargs);
        }

        public bool DetectKeyPress(Keys key)
        {
            return UserInterface.keybState.IsKeyDown(key) && UserInterface.oldKeyBState.IsKeyUp(key);
        }
        public bool DetectKeyRelease(Keys key)
        {
            return UserInterface.keybState.IsKeyUp(key) && UserInterface.oldKeyBState.IsKeyDown(key);
        }
        public bool DetectKeyDown(Keys key)
        {
            return UserInterface.keybState.IsKeyDown(key);
        }
        
    }
}
