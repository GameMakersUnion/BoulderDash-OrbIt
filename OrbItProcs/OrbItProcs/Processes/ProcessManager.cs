using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

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
        mapeditor,

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
            processDict.Add(proc.mapeditor, new MapEditor(room.level));

            activeInputProcess = processDict[proc.spawnnodes];

        }

        public void SetProcessKeybinds(KeyManager Keybindset)
        {
            //
            Keybindset.Add("spawnnodes", new KeyBundle(KeyCodes.D1), delegate
            {
                Keybindset.AddProcess(processDict[proc.spawnnodes]);//, KeySwitchMethod.Overwrite);
            });
            //
            Keybindset.Add("randomizer", new KeyBundle(KeyCodes.D2), delegate
            {
                Keybindset.AddProcess(processDict[proc.randomizer]);//, KeySwitchMethod.Overwrite);
            });
            //
            Keybindset.Add("groupselect", new KeyBundle(KeyCodes.D3), delegate
            {
                Keybindset.AddProcess(processDict[proc.groupselect]);//, KeySwitchMethod.Overwrite);
            });
            //
            Keybindset.Add("singleselect", new KeyBundle(KeyCodes.D4), delegate
            {
                Keybindset.AddProcess(processDict[proc.singleselect]);//, KeySwitchMethod.Overwrite);
            });
            //
            Keybindset.Add("polygonspawner", new KeyBundle(KeyCodes.D9), delegate
            {
                Keybindset.AddProcess(processDict[proc.polygonspawner]);//, KeySwitchMethod.Overwrite);
            });
            //
            Keybindset.Add("axismovement", new KeyBundle(KeyCodes.D0), delegate
            {
                Keybindset.AddProcess(processDict[proc.axismovement]);//, KeySwitchMethod.Overwrite);
            });
            //
            Keybindset.Add("mapeditor", new KeyBundle(KeyCodes.D5), delegate
            {
                Keybindset.AddProcess(processDict[proc.mapeditor]);//, KeySwitchMethod.Overwrite);
            });
        }

        public void Update()
        {
            foreach (Process p in processes)
            {
                if (p.active)
                {
                    p.OnUpdate();
                }
            }
            if (activeInputProcess != null)
                activeInputProcess.OnUpdate();

        }
        
        public void Draw(SpriteBatch batch)
        {
            foreach (Process p in processes)
            {
                if (p.active)
                {
                    p.OnDraw(batch);
                }
            }
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
