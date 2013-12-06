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

using OrbItProcs;
using System.IO;
using System.Collections;

namespace OrbItProcs.Interface
{

    public class PopupWindow
    {
        public enum PopUpType { alert, prompt, textBox, dropDown };
        public delegate void ConfirmDelegate(bool confirm, object answer = null);
        public const int MAX_CHARS_PER_LINE = 25;

        private UserInterface ui;
        private PopUpType type;
        private string message;
        private ConfirmDelegate action = delegate(bool c, object a){};

        private Manager manager;
        private Dialog window;
        private ComboBox cbBox;
        private Button btnOk, btnCancel;
        private TextBox tbName;

        private int VertPadding = 10;

        private bool confirmed = false;
        private object answer;

        public PopupWindow(UserInterface ui, PopUpType windowType, string message = "", string title = "Hey! Listen!",ConfirmDelegate action = null, IEnumerable list = null)
        {
            this.ui = ui;
            this.action = action ?? this.action;
            this.manager = ui.game.Manager;
            this.type = windowType;
            this.message = message.wordWrap(MAX_CHARS_PER_LINE);

            window = new Dialog(manager);
            window.Text = title;
            window.Init();
            window.ShowModal();
            window.Caption.Text = "";
            window.Description.Top = window.Caption.Top;
            window.Description.Text = message;
            window.Width = 200;
            window.SetPosition(Game1.sWidth - 210, Game1.sHeight / 4);

            btnOk = new Button(manager);
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
                cbBox = new ComboBox(manager);
                cbBox.Init();
                cbBox.Parent = window;
                cbBox.Width = window.Width - VertPadding * 2;
                cbBox.Left = VertPadding ;
                cbBox.Top = btnOk.Top;
                btnOk.Top = cbBox.Top + cbBox.Height;
                foreach (object o in list) cbBox.Items.Add(o);
                cbBox.ItemIndexChanged += delegate { answer = cbBox.Items.ElementAt(cbBox.ItemIndex); };
            }
            if (windowType == PopUpType.textBox)
            {
                tbName = new TextBox(manager);
                tbName.Init();
                tbName.Parent = window;
                tbName.Width = window.Width - VertPadding * 2;
                tbName.Left = VertPadding;
                tbName.Top = btnOk.Top;
                btnOk.Top = tbName.Top + tbName.Height;

                tbName.TextChanged += delegate {answer = tbName.Text;};
            }

            if (windowType.In(PopUpType.prompt, PopUpType.textBox, PopUpType.dropDown))
            {
                btnOk.Click += affirmative;

                btnCancel = new Button(manager);
                btnCancel.Init();
                btnCancel.Parent = window;
                btnCancel.Top = btnOk.Top;
                btnCancel.Text = "Cancel";
                btnCancel.Left = VertPadding * 2 + btnOk.Width;
                btnCancel.Click += delegate { window.Close(); };

            }
            window.Closed += negatory;
            window.Height = (btnOk.Top) + 70;
            manager.Add(window);

        }

        private void affirmative(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            confirmed = true; action(true, answer); 
        }

        private void negatory(object sender, WindowClosedEventArgs e)
        {
            if (confirmed  == false) action(false, answer);
        }
    }

}
