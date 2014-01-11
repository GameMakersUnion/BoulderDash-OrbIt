using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OrbItProcs.Processes;

namespace OrbItProcs.Interface
{
    public enum MouseEvents
    {
        LeftClick,
        LeftPress,
        LeftRelease,
        MiddleClick,
        MiddlePress,
        MiddleRelease,
        RightClick,
        RightPress,
        RightRelease,
    }

    public struct KeyBundle
    {
        public Keys? first;
        public Keys? second;
        public Keys? third;

        public KeyBundle (Keys? first, Keys? second = null, Keys? third = null)
        {
            this.first = first;
            this.second = second;

            if (second == null && third != null) third = null;
            this.third = third;
        }

        public KeyBundle(Stack<Keys> stack)
        {
            first = second = third = null;
            for (int i = stack.Count - 1; i >= 0; i--)
            {
                if (first == null) first = stack.ElementAt(i);
                else if (second == null) second = stack.ElementAt(i);
                else if (third == null) third = stack.ElementAt(i);
            }
        }
    }

    public class KeybindSet
    {
        public UserInterface ui;

        public Dictionary<KeyBundle, dynamic> _Keybinds = new Dictionary<KeyBundle, dynamic>();
        public Dictionary<KeyBundle, dynamic> Keybinds { get { return _Keybinds; } set { _Keybinds = value; } }

        public Stack<Keys> PressedKeys = new Stack<Keys>(); //max of three keys detected at a time

        public HashSet<Keys> AllKeys = new HashSet<Keys>();

        public KeyboardState newKeyboardState, oldKeyboardState;

        public MouseState newMouseState, oldMouseState;

        public Dictionary<MouseEvents, dynamic> _Mousebinds = new Dictionary<MouseEvents, dynamic>();
        public Dictionary<MouseEvents, dynamic> Mousebinds { get { return _Mousebinds; } set { _Mousebinds = value; } }


        public KeybindSet(UserInterface ui, Dictionary<KeyBundle, dynamic> Keybinds = null)
        {
            this.ui = ui;
            this.newKeyboardState = Keyboard.GetState();
            this.oldKeyboardState = Keyboard.GetState();

            if (Keybinds != null)
            {
                this.Keybinds = Keybinds;
                
            }

            MouseState m = Mouse.GetState();
            
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


            Add(new KeyBundle(Keys.LeftControl, Keys.R), delegate { Console.WriteLine("Ctrl+R!!"); }, true);
            Add(new KeyBundle(Keys.LeftControl, Keys.LeftAlt, Keys.Q), delegate { Console.WriteLine("Ctrl+Alt+Q!!"); }, true);


            int i = 0;
            Action a = delegate { i++; };
            Mousebinds.Add(MouseEvents.LeftPress, a);
            

            foreach (Keys k in Enum.GetValues(typeof(Keys)))
            {
                AllKeys.Add(k);
            }
            

        }

        

        public void Add(KeyBundle keyarr, Process process, bool AddBothCombinations = false)
        {
            Keybinds.Add(keyarr, process);
            if (AddBothCombinations && keyarr.third != null) //adds the process to both combinations (123, 213)
            {
                KeyBundle kb = new KeyBundle(keyarr.second, keyarr.first, keyarr.third);
                Keybinds.Add(kb, process);
            }
        }

        public void Add(KeyBundle keyarr, Action action, bool AddBothCombinations = false)
        {
            Keybinds.Add(keyarr, action);
            if (AddBothCombinations && keyarr.third != null) //adds the action to both combinations (123, 213)
            {
                KeyBundle kb = new KeyBundle(keyarr.second, keyarr.first, keyarr.third);
                Keybinds.Add(kb, action);
            }
        }

        public void Update()
        {
            newKeyboardState = Keyboard.GetState();
            newMouseState = Mouse.GetState();

            ProcessKeyboard();
            ProcessMouse();

            oldKeyboardState = Keyboard.GetState();
            oldMouseState = Mouse.GetState();
        }

        public void ProcessMouse()
        {
            DetectMouseButton(newMouseState.LeftButton, oldMouseState.LeftButton, MouseEvents.LeftPress, MouseEvents.LeftClick, MouseEvents.LeftRelease);
            DetectMouseButton(newMouseState.RightButton, oldMouseState.RightButton, MouseEvents.RightPress, MouseEvents.RightClick, MouseEvents.RightRelease);
            DetectMouseButton(newMouseState.MiddleButton, oldMouseState.MiddleButton, MouseEvents.MiddlePress, MouseEvents.MiddleClick, MouseEvents.MiddleRelease);
        }

        public void DetectMouseButton(ButtonState newButtonState, ButtonState oldButtonState, MouseEvents press, MouseEvents click, MouseEvents release)
        {
            if (newButtonState == ButtonState.Pressed && oldButtonState == ButtonState.Pressed)
            {
                TryMouseAction(press);
            }
            else if (newButtonState == ButtonState.Pressed && oldButtonState == ButtonState.Released)
            {
                TryMouseAction(click);
            }
            else if (newButtonState == ButtonState.Released && oldButtonState == ButtonState.Pressed)
            {
                TryMouseAction(release);
            }
        }

        public void TryMouseAction(MouseEvents mouseEvent)
        {
            if (Mousebinds.ContainsKey(mouseEvent))
            {
                dynamic value = Mousebinds[mouseEvent];
                if (value != null)
                {
                    if (value is Process)
                    {
                        Process p = value;
                        //execute procsses  p.OnStart(); ?
                    }
                    else if (value is Action)
                    {
                        Action a = value;
                        a();
                    }
                }
            }
        }


        public void ProcessKeyboard()
        {
            foreach (Keys k in AllKeys.ToList())
            {
                if (KeyPressEvent(k))
                {
                    if (!PressedKeys.Contains(k) && PressedKeys.Count < 3)
                    {
                        PressedKeys.Push(k);

                    }
                    //PrintPressedKeys();
                    TryKeyboardAction();
                }
            }

            for (int i = PressedKeys.Count - 1; i >= 0; i--)
            {
                Keys key = PressedKeys.ElementAt(i);
                if (KeyReleaseEvent(key))
                {
                    while (PressedKeys.Peek() != key) PressedKeys.Pop();
                    PressedKeys.Pop();

                    //PrintPressedKeys(true);
                    break;
                }
            }
        }

        public void TryKeyboardAction()
        {
            KeyBundle kb = new KeyBundle(PressedKeys);
            if (Keybinds.ContainsKey(kb))
            {
                dynamic value = Keybinds[kb];
                if (value != null)
                {
                    if (value is Process)
                    {
                        Process p = value;
                        //execute procsses  p.OnStart(); ?
                    }
                    else if (value is Action)
                    {
                        Action a = value;
                        a();
                    }
                }
            }
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
