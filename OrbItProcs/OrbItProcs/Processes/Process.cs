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

        //Fuck no
        //
        //public event Action<ButtonState> LeftClick;
        //public event Action<ButtonState> RightClick;
        //public event Action<ButtonState> MiddleClick;
        //public event Action<int> Scroll;
        //
        //public event Action LeftHold;
        //public event Action RightHold;
        //public event Action MiddleHold;
        //
        //public event ProcessMethod KeyEvent;
        //
        //private bool leftHold = false, rightHold = false, middleHold = false;

        public Dictionary<KeyAction, KeyBundle> processKeyActions = new Dictionary<KeyAction,KeyBundle>();

        public List<Process> procs = new List<Process>();
        //Process parentprocess; //I bet you a coke -Dante (resolved section 33.32 of the skeet bible studies)
        public Dictionary<dynamic, dynamic> pargs;
        public Dictionary<dynamic, ProcessMethod> pmethods;

        public Process()
        { 
            // / // / //
            room = Program.getRoom();
        }

        protected void addProcessKeyAction(String name, KeyCodes k1, KeyCodes? k2 = null, KeyCodes? k3 = null, Action OnPress = null, Action OnRelease = null, Action OnHold = null)
        {
            KeyBundle keyBundle;
            if (k2 == null) keyBundle = new KeyBundle(k1);
            else if (k3 == null) keyBundle = new KeyBundle((KeyCodes)k2, k1);
            else keyBundle = new KeyBundle((KeyCodes)k3, (KeyCodes) k2, k1);

            var keyAction = new KeyAction(name, OnPress, new HashSet<KeyBundle>() { keyBundle });

            keyAction.releaseAction = OnRelease;
            keyAction.holdAction = OnHold;

            processKeyActions.Add(keyAction, keyBundle);
        }

        

        public void OnUpdate()
        {
            foreach (Process p in procs)
            {
                p.OnUpdate();
            }
            if (Update != null) Update(pargs);
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
