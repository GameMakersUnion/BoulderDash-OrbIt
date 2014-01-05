using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OrbItProcs.Interface
{
    public class KeybindSet
    {
        public UserInterface ui;

        public Dictionary<Keys[], Action> _Keybinds = new Dictionary<Keys[], Action>();
        public Dictionary<Keys[], Action> Keybinds { get { return _Keybinds; } set { _Keybinds = value; } }

        public Stack<Keys> PressedKeys = new Stack<Keys>(); //max of three keys detected at a time

        public HashSet<Keys> AllKeys = new HashSet<Keys>();

        public KeyboardState newKeyboardState, oldKeyboardState;

        public KeybindSet(UserInterface ui, Dictionary<Keys[],Action> Keybinds = null)
        {
            this.ui = ui;
            this.newKeyboardState = Keyboard.GetState();
            this.oldKeyboardState = Keyboard.GetState();

            if (Keybinds != null)
            {
                this.Keybinds = Keybinds;
            }
            /*
            AllKeys = new HashSet<Keys>() {
                Keys.D1,
                Keys.D2,
                Keys.D3,
                Keys.D4,
                Keys.D5,
                Keys.D6,
                Keys.D7,
                Keys.D8,
                Keys.D9,
                Keys.D0,
            };
            foreach (Keys[] keyarr in this.Keybinds.Keys)
            {
                for(int i = 0; i < keyarr.Length; i++)
                {
                    AllKeys.Add(keyarr[i]);
                }
            }
            */
            

            foreach (Keys k in Enum.GetValues(typeof(Keys)))
            {
                AllKeys.Add(k);
            }
            

        }

        public void Add(Keys[] keyarr, Action action)
        {
            Keybinds.Add(keyarr, action);
        }

        public void Update()
        {
            this.newKeyboardState = Keyboard.GetState();

            foreach(Keys k in AllKeys.ToList())
            {
                if (KeyPressEvent(k))
                {
                    if (!PressedKeys.Contains(k) && PressedKeys.Count < 3)
                    {
                        PressedKeys.Push(k);
                    }
                    PrintPressedKeys();
                }
            }
            
            for (int i = PressedKeys.Count - 1; i >= 0 ; i--)
            {
                Keys key = PressedKeys.ElementAt(i);
                if (KeyReleaseEvent(key))
                {
                    while (PressedKeys.Peek() != key) PressedKeys.Pop();
                    PressedKeys.Pop();

                    PrintPressedKeys(true);
                    break;
                }
            }

            this.oldKeyboardState = Keyboard.GetState();
        }

        public void PrintPressedKeys(bool released = false)
        {
            string s = "";
            if (released) s += "Release: "; else s += "Pressed: ";

            for (int i = PressedKeys.Count - 1; i >= 0; i--)
            {
                s += PressedKeys.ElementAt(i) + " ";
            }

            Console.WriteLine(s);
        }

        public bool KeyPressEvent(Keys key)
        {
            return newKeyboardState.IsKeyDown(key) && !oldKeyboardState.IsKeyDown(key);
        }
        public bool KeyReleaseEvent(Keys key)
        {
            return !newKeyboardState.IsKeyDown(key) && oldKeyboardState.IsKeyDown(key);
        }


    }
}
