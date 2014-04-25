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
        graphdata,
        cameracontrol,
        directedspawn,
        removenodes,
        gridspawn,
        roomResize,
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
        public static MouseState mouseState, oldMouseState;
        public KeyboardState keyState, oldKeyState;

        //private int ScrollPosition = 0;

        public ProcessManager(OrbIt game)
        {
            this.processDict = new Dictionary<proc, Process>();
            this.processes = new HashSet<Process>();

            processDict.Add(proc.spawnnodes, new SpawnNodes());
            processDict.Add(proc.randomizer, new Randomizer());
            processDict.Add(proc.singleselect, new SingleSelect());
            processDict.Add(proc.groupselect, new GroupSelect());
            processDict.Add(proc.polygonspawner, new PolygonSpawner());
            processDict.Add(proc.mapeditor, new MapEditor());
            processDict.Add(proc.graphdata, new GraphData());
            processDict.Add(proc.cameracontrol, new CameraControl());
            processDict.Add(proc.directedspawn, new DirectedSpawn());
            processDict.Add(proc.removenodes, new RemoveNodes());
            processDict.Add(proc.gridspawn, new GridSpawn());
            processDict.Add(proc.roomResize, new ResizeRoom());

            activeInputProcess = processDict[proc.spawnnodes];
            OrbIt.ui.groupSelectSet = (processDict[proc.groupselect] as GroupSelect).groupSelectSet;
        }

        Action enableKeyBinds(proc p)
        {
            return delegate
            {
                OrbIt.ui.keyManager.AddProcess(processDict[p]);
            };
        }

        public void SetProcessKeybinds()
        {
            ToolWindow toolbar = OrbIt.ui.sidebar.toolWindow;
            KeyManager Keybindset = OrbIt.ui.keyManager;
            

            Keybindset.Add("spawnnodes", new KeyBundle(KeyCodes.D1, KeyCodes.LeftShift), enableKeyBinds(proc.spawnnodes));
            toolbar.AddButton("spawn",enableKeyBinds(proc.spawnnodes), "Spawn node of selected group. RightClick to spawn many" );
            toolbar.AddButton("remove", enableKeyBinds(proc.removenodes), "Remove nodes: leftclick single, rightclick drag, middleclick remove all.");
            
            Keybindset.Add("groupselect", new KeyBundle(KeyCodes.D3, KeyCodes.LeftShift), enableKeyBinds(proc.groupselect));
            Keybindset.Add("singleselect", new KeyBundle(KeyCodes.D4, KeyCodes.LeftShift), enableKeyBinds(proc.singleselect));
            toolbar.AddButton("select", enableKeyBinds(proc.singleselect), "Click to select a node, drag to select many");

            Keybindset.Add("mapeditor", new KeyBundle(KeyCodes.D5, KeyCodes.LeftShift), enableKeyBinds(proc.mapeditor)); 
            toolbar.AddButton("level",enableKeyBinds(proc.mapeditor), "Click to set static colidable polygons." );

            Keybindset.Add("randomizer", new KeyBundle(KeyCodes.D2, KeyCodes.LeftShift), enableKeyBinds(proc.randomizer));
            toolbar.AddButton("random",enableKeyBinds(proc.randomizer), "Click to spawn a random node, right click to spawn a copy of the previous random node." );

            toolbar.AddButton("forceSpawn", enableKeyBinds(proc.directedspawn), "Spawn nodes in a direction using left and right click.");
            toolbar.AddButton("forcePush", Utils.notImplementedException,"Take a hike.");
            toolbar.AddButton("control", Utils.notImplementedException,"Take a hike.");
            toolbar.AddButton("static", enableKeyBinds(proc.gridspawn), "Spawn nodes statically to the grid.");
            toolbar.AddButton("resize", enableKeyBinds(proc.roomResize), "Change the size of the Room");
            
            Keybindset.Add("resetplayers", new KeyBundle(KeyCodes.Home), delegate { Player.ResetPlayers(OrbIt.game.mainRoom); });

            Keybindset.Add("pausegame", new KeyBundle(KeyCodes.F, KeyCodes.LeftShift), delegate { OrbIt.ui.IsPaused = !OrbIt.ui.IsPaused; });


            Keybindset.Add("graphdata", new KeyBundle(KeyCodes.D6, KeyCodes.LeftShift), enableKeyBinds(proc.graphdata)); 

            Keybindset.Add("polygonspawner", new KeyBundle(KeyCodes.D9, KeyCodes.LeftShift), enableKeyBinds(proc.polygonspawner));
            

            Keybindset.AddProcess(processDict[proc.cameracontrol], false);
            Keybindset.AddProcess(processDict[proc.spawnnodes]);
        }

        public void Update()
        {
            /*foreach (Process p in processes)
            {
                if (p.active)
                {
                    p.OnUpdate();
                }
            }
            if (activeInputProcess != null)
                activeInputProcess.OnUpdate();*/



        }
        
        public void Draw()
        {
            foreach (Process p in processes)
            {
                if (p.active)
                {
                    p.OnDraw();
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
