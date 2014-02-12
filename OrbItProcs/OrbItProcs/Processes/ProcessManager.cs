using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OrbItProcs
{
    public enum proc
    {
        spawnnodes,
        singleselect,
        groupselect,
        randomizer,
        axismovement,
        polygonspawner,

    }

    public struct MouseArgs
    {
        //MouseButtons mouseButtons;
        public ButtonState buttonState;
        public Vector2 mousePosition;
        public int scrollDirection;

        public MouseArgs(ButtonState buttonState, Vector2 mousePosition, int scrollDirection = 0)
        {
            this.buttonState = buttonState;
            this.mousePosition = mousePosition;
            this.scrollDirection = scrollDirection;
        }
    }

    public class ProcessManager
    {
        

        //public enum MouseButtons
        //{
        //    LeftButton,
        //    RightButton,
        //    MiddleButton,
        //    ScrollValue,
        //}

        public Dictionary<proc, Process> processDict { get; set; }
        public Process activeInputProcess;
        public HashSet<Process> processes;
        public Room room;
        public static MouseState mouseState, oldMouseState;
        public KeyboardState keyState, oldKeyState;

        private int ScrollPosition = 0;

        public ProcessManager(Room room)
        {
            this.room = room;
            this.processDict = new Dictionary<proc, Process>();
            this.processes = new HashSet<Process>();

            processDict.Add(proc.spawnnodes, new SpawnNodes());
            processDict.Add(proc.randomizer, new Randomizer());
            processDict.Add(proc.singleselect, new SingleSelect());
            processDict.Add(proc.groupselect, new GroupSelect());
            processDict.Add(proc.polygonspawner, new PolygonSpawner());

            activeInputProcess = processDict[proc.spawnnodes];

            //room.game.ui.
        }
        /*
        public void PollKeyboard()
        {
            if (activeInputProcess != null)
            {
                activeInputProcess.InvokeKeyEvent(null);
            }
        }
        
        public void PollMouse(MouseState mS, MouseState oMS)
        {
            //return;
            //left button
            if (mS.LeftButton == ButtonState.Pressed && oMS.LeftButton == ButtonState.Released)
            {
                if (activeInputProcess != null)
                    activeInputProcess.InvokeLeftClick(ButtonState.Pressed);
            }
            else if (mS.LeftButton == ButtonState.Released && oMS.LeftButton == ButtonState.Pressed)
            {
                if (activeInputProcess != null)
                    activeInputProcess.InvokeLeftClick(ButtonState.Released);
            }
            //right button
            if (mS.RightButton == ButtonState.Pressed && oMS.RightButton == ButtonState.Released)
            {
                if (activeInputProcess != null)
                    activeInputProcess.InvokeRightClick(ButtonState.Pressed);
            }
            else if (mS.RightButton == ButtonState.Released && oMS.RightButton == ButtonState.Pressed)
            {
                if (activeInputProcess != null)
                    activeInputProcess.InvokeRightClick(ButtonState.Released);
            }
            //middle button
            if (mS.MiddleButton == ButtonState.Pressed && oMS.MiddleButton == ButtonState.Released)
            {
                if (activeInputProcess != null)
                    activeInputProcess.InvokeMiddleClick(ButtonState.Pressed);
            }
            else if (mS.MiddleButton == ButtonState.Released && oMS.MiddleButton == ButtonState.Pressed)
            {
                if (activeInputProcess != null)
                    activeInputProcess.InvokeMiddleClick(ButtonState.Released);
            }
            //scroll
            if (mS.ScrollWheelValue < oMS.ScrollWheelValue)
            {
                if (activeInputProcess != null)
                    activeInputProcess.InvokeScroll(1);
            }
            else if (mS.ScrollWheelValue > oMS.ScrollWheelValue)
            {
                if (activeInputProcess != null)
                    activeInputProcess.InvokeScroll(-1);
            }
        }
        */

        /*public void SetInputStates(MouseState mS, MouseState oMS, KeyboardState kS, KeyboardState oKS)
        {
            mouseState = mS;
            oldMouseState = oMS;
            keyState = kS;
            oldKeyState = oKS;
        }*/

        public void Update()
        {
            foreach (Process p in processes)
            {
                p.OnUpdate();
            }
            if (activeInputProcess != null)
                activeInputProcess.OnUpdate();

        }

        //this will start it.... for now...
        public void Add(Process p)
        {
            if (!processes.Contains(p))
            {
                bool isIn = processes.Any((x) => x.GetType() == p.GetType());
                if (isIn) return;

                processes.Add(p);
                p.OnCreate();
            }
            //System.Console.WriteLine("heyo pre-emptive strike");
        }

        public void Remove(Process p)
        {
            p.OnDestroy();
            processes.Remove(p);
        }
    }
}
