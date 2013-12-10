using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TomShane.Neoforce.Controls;
using System.Reflection;

using OrbItProcs.Interface;
using System.IO;
using System.Collections;

namespace OrbItProcs
{

    public static class PopUp
    {
        private enum PopUpType { alert, prompt, textBox, dropDown };
        public delegate void ConfirmDelegate(bool confirm, object answer = null);
        private const int MAX_CHARS_PER_LINE = 25;
        private static int VertPadding = 10;

        private static void makePopup(UserInterface ui, PopUpType windowType, string message = "", string title = "Hey! Listen!",ConfirmDelegate action = null, IEnumerable list = null)
        {
            bool confirmed = false;
            object answer = null;
            action = action ?? delegate(bool c, object a){};
            Manager manager = ui.game.Manager;
            message = message.wordWrap(MAX_CHARS_PER_LINE);

            Dialog window = new Dialog(manager);
            window.Text = title;
            window.Init();
            window.ShowModal();
            window.Caption.Text = "";
            window.Description.Top = window.Caption.Top;
            window.Description.Text = message;
            window.Width = 200;
            window.Height = 200;
            window.SetPosition(Game1.sWidth - 220, Game1.sHeight / 4);

            Button btnOk = new Button(manager);
            btnOk.Top = window.Description.Top + window.Description.Height;
            btnOk.Anchor = Anchors.Top;
            btnOk.Parent = window;
            btnOk.Text = "Ok";
            btnOk.Left = VertPadding;
            btnOk.Click += delegate { window.Close(); };
            btnOk.Init();


            if (windowType == PopUpType.alert)
            {
                btnOk.Left = window.Width / 2 - btnOk.Width / 2;
            }

            if (windowType == PopUpType.dropDown)
            {
                
                ComboBox cbBox = new ComboBox(manager);
                cbBox.Init();
                cbBox.Parent = window;
                cbBox.MaxItems = 20;
                cbBox.Width = window.Width - VertPadding * 5;
                cbBox.Left = VertPadding;
                cbBox.Top = btnOk.Top;
                btnOk.Top = cbBox.Top + cbBox.Height;
                foreach (object o in list) cbBox.Items.Add(o);
                cbBox.ItemIndexChanged += delegate { answer = cbBox.Items.ElementAt(cbBox.ItemIndex); };
            }
            if (windowType == PopUpType.textBox)
            {
                TextBox tbName = new TextBox(manager);
                tbName.Init();
                tbName.Parent = window;
                tbName.Width = window.Width - VertPadding * 5;
                tbName.Left = VertPadding;
                tbName.Top = btnOk.Top;
                btnOk.Top = tbName.Top + tbName.Height;

                tbName.TextChanged += delegate {answer = tbName.Text;};
            }

            if (windowType.In(PopUpType.prompt, PopUpType.textBox, PopUpType.dropDown))
            {
                btnOk.Click += delegate { confirmed = true; action(true, answer); };

                Button btnCancel = new Button(manager);
                btnCancel.Init();
                btnCancel.Parent = window;
                btnCancel.Top = btnOk.Top;
                btnCancel.Text = "Cancel";
                btnCancel.Left = VertPadding * 2 + btnOk.Width;
                btnCancel.Click += delegate { window.Close(); };

            }
            window.Closed += delegate { if (confirmed == false) action(false, answer); };
            window.Height = (btnOk.Top) + 70;
            manager.Add(window);

        }

        public static void Toast(UserInterface ui, string message = "", string title = "Hey! Listen!")
        {makePopup(ui, PopUpType.alert, message, title);}
        public static void Prompt(UserInterface ui, string message = "", string title = "Hey! Listen!",ConfirmDelegate action = null)
        {makePopup(ui, PopUpType.prompt, message, title, action);}
        public static void Select(UserInterface ui, string message = "", string title = "Hey! Listen!",ConfirmDelegate action = null, IEnumerable list = null)
        {makePopup(ui, PopUpType.dropDown, message, title, action, list);}
        public static void Text(UserInterface ui, string message = "", string title = "Hey! Listen!",ConfirmDelegate action = null)
        { makePopup(ui, PopUpType.textBox, message, title, action); }

    }

}
