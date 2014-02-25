using Microsoft.Xna.Framework.Graphics;
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
        public bool active { get; set; }

        public event ProcessMethod Update;
        public event Action<SpriteBatch> Draw;
        public event ProcessMethod Create;
        public event ProcessMethod Destroy;
        public event Action<Node,Node> Collision;
        public event ProcessMethod OutOfBounds;

        public Dictionary<KeyAction, KeyBundle> processKeyActions = new Dictionary<KeyAction,KeyBundle>();

        public List<Process> procs = new List<Process>();
        //Process parentprocess; //I bet you a coke -Dante (resolved section 33.32 of the skeet bible studies)
        public Dictionary<dynamic, dynamic> pargs;
        public Dictionary<dynamic, ProcessMethod> pmethods;

        public Process()
        { 
            // / // / //
            room = Program.getRoom();
            active = true;
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
                if (p.active)
                {
                    p.OnUpdate();
                }
            }
            if (Update != null) Update(pargs);
        }
        public void OnDraw(SpriteBatch batch)
        {
            foreach (Process p in procs)
            {
                if (p.active)
                {
                    p.OnDraw(batch);
                }
            }
            if (Draw != null) Draw(batch);
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

        public void OnCollision(Node me, Node it)
        {
            if (Collision != null)
            {
                Collision(me, it);
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
