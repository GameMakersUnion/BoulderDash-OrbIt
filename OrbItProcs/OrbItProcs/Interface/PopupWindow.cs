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

namespace OrbItProcs.Interface
{

    public class PopupWindow
    {
        public enum PopUpType { alert, prompt, textBox, dropDown };
        public delegate void confirmDelegate(bool confirm);//, object answer = null);

        private PopUpType type;
        private string message;
        private confirmDelegate action;

        private UserInterface ui;
        private Manager manager;
        private string windowtype;

        private Dialog window;
        private ComboBox cbBox;
        private Button btnOk, btnCancel, btnSave;
        private TextBox tbName;

        public int HeightCounter = 10;
        public int VertPadding = 10;

        public PopupWindow(UserInterface ui, PopUpType windowType, string message = "You shouldn't have come here!")
        {
            this.ui = ui;
            this.manager = ui.game.Manager;
            this.type = windowType;
            this.message = message;

            if (windowType == PopUpType.textBox || windowType == PopUpType.dropDown)
            {
                if (ui.editNode == null)
                {
                    PopupWindow nothingSelected = new PopupWindow(ui, PopUpType.alert, "You haven't selected a Node.");
                    return;
                }
            }

            Initialize(windowType);
            int a;

        }

        public void Initialize(PopUpType windowType)
        {
            if (windowType == PopUpType.alert)
            {
            window = new Dialog(manager);
                window.Init();
                window.ShowModal();
                window.Caption.Height = 1;   //hack to remove caption
                window.Caption.Width = 1;    //hack to remove caption
                window.Description.Top = window.Caption.Top;
                window.Description.Text = "";
                window.Description.Height = 1000;
                message = message.wordWrap(25);
                window.Description.Text = message;

                window.Anchor = Anchors.None;
                window.Width = 200;
                //window.Passive = true;
                window.Height = Game1.sHeight / 4;
                //window.Center();
                window.Visible = true;
                window.Resizable = false;
                window.Movable = false;
                //window.Parent = sidebar;

                window.BorderVisible = true;
                //window.MaximumHeight = Game1.sHeight;
                //window.MinimumHeight = Game1.sHeight;
                //window.MaximumWidth = 300;
                //window.MinimumWidth = 200;
                window.Alpha = 255;
                window.SetPosition(Game1.sWidth - 210, Game1.sHeight / 4);

                //window.FocusLost += new TomShane.Neoforce.Controls.EventHandler(window_FocusLost);
                window.Closing += new WindowClosingEventHandler(window_FocusLost);

                window.StayOnTop = true;

                manager.Add(window);

                Label lblname = new Label(manager);
                lblname.Init();
                lblname.Parent = window;
                lblname.Top = VertPadding;
                lblname.Text = "";
                lblname.Left = VertPadding;
                lblname.Width = window.Width - VertPadding * 2;
                //lblname.Height = 200;
                HeightCounter += VertPadding * 2 + lblname.Height + 10;
                lblname.Anchor = Anchors.Left;

                btnOk = new Button(manager);
                btnOk.Init();
                btnOk.Parent = window;
                btnOk.Top = HeightCounter;
                HeightCounter += VertPadding * 2;
                btnOk.Text = "Ok";
                btnOk.Left = VertPadding * 2;
                btnOk.Click += new TomShane.Neoforce.Controls.EventHandler(closeWindow);
                //btnCancel.Click += new TomShane.Neoforce.Controls.EventHandler();

            }
            if (windowType == PopUpType.dropDown)
            {
                window = new Dialog(manager);

                window.Init();
                window.ShowModal();
                window.Caption.Height = 1;
                window.Caption.Width = 1;
                window.Description.Top = window.Caption.Top;

                window.Description.Text = "Add to component to:\n" + ui.editNode.name;
                //window.TopPanel.Visible = true;
                window.Text = "Choose Component";
                window.Width = 200;
                //window.Passive = true;
                window.Height = Game1.sHeight / 4;
                //window.Center();
                window.Visible = true;
                window.Resizable = false;
                window.Movable = false;
                //window.Parent = sidebar;
                window.Anchor = Anchors.Top | Anchors.Right | Anchors.Bottom;
                window.BorderVisible = true;
                //window.MaximumHeight = Game1.sHeight;
                //window.MinimumHeight = Game1.sHeight;
                //window.MaximumWidth = 300;
                //window.MinimumWidth = 200;
                window.Alpha = 255;
                window.SetPosition(Game1.sWidth - 210, Game1.sHeight / 4);

                //window.FocusLost += new TomShane.Neoforce.Controls.EventHandler(window_FocusLost);
                window.Closing += new WindowClosingEventHandler(window_FocusLost);

                window.StayOnTop = true;

                manager.Add(window);

                HeightCounter += VertPadding * 2 + 20;
                //System.Console.WriteLine(HeightCounter);
                cbBox = new ComboBox(manager);
                cbBox.Init();
                cbBox.Width = 160;
                cbBox.Left = 10;
                cbBox.Top = HeightCounter; HeightCounter += VertPadding * 2 + cbBox.Height;
                cbBox.Parent = window;

                //foreach (comp.)
                foreach (comp c in Enum.GetValues(typeof(comp)))
                {
                    if (!ui.editNode.comps.ContainsKey(c))
                        cbBox.Items.Add(c);
                }

                btnSave = new Button(manager);
                btnSave.Init();
                btnSave.Parent = window;
                btnSave.Top = HeightCounter;
                //HeightCounter += VertPadding * 2 + btnSave.Height;
                btnSave.Text = "Add Component";
                btnSave.Left = VertPadding;
                btnSave.Click += new TomShane.Neoforce.Controls.EventHandler(closeWindow);
                btnSave.Click += new TomShane.Neoforce.Controls.EventHandler(btnAddComponent_Click);


                btnCancel = new Button(manager);
                btnCancel.Init();
                btnCancel.Parent = window;
                btnCancel.Top = HeightCounter;
                HeightCounter += VertPadding * 2 + btnSave.Height;
                btnCancel.Text = "Cancel";
                btnCancel.Left = VertPadding * 2 + btnSave.Width;
                btnCancel.Click += new TomShane.Neoforce.Controls.EventHandler(closeWindow);

            }
            if (windowType == PopUpType.textBox)
            {
                window = new Dialog(manager);

                window.Caption.Text = "Choose preset name:";
                window.Description.Text = "";
                window.Init();
                window.ShowModal();
                //window.TopPanel.Visible = true;
                window.Text = "Save Node Preset";
                window.Width = 200;
                //window.Passive = true;
                window.Height = Game1.sHeight / 4;
                //window.Center();
                window.Visible = true;
                window.Resizable = false;
                window.Movable = false;
                //window.Parent = sidebar;
                window.Anchor = Anchors.Top | Anchors.Right | Anchors.Bottom;
                window.BorderVisible = true;
                window.Alpha = 255;
                window.SetPosition(Game1.sWidth - 210, Game1.sHeight / 4);
                window.Closing += new WindowClosingEventHandler(window_FocusLost);
                window.StayOnTop = true;

                manager.Add(window);

                Label lblname = new Label(manager);
                lblname.Init();
                lblname.Parent = window;
                lblname.Top = VertPadding;
                lblname.Text = "";
                lblname.Left = VertPadding;
                HeightCounter += VertPadding * 2 + lblname.Height;
                lblname.Anchor = Anchors.Left;

                tbName = new TextBox(manager);
                tbName.Init();
                tbName.Parent = window;
                tbName.Top = HeightCounter;
                HeightCounter += VertPadding * 2 + lblname.Height;
                tbName.Text = ui.editNode.name;
                tbName.Left = VertPadding;

                btnSave = new Button(manager);
                btnSave.Init();
                btnSave.Parent = window;
                btnSave.Top = HeightCounter;
                //HeightCounter += VertPadding * 2 + btnSave.Height;
                btnSave.Text = "Save as Preset";
                btnSave.Left = VertPadding;
                btnSave.Click += new TomShane.Neoforce.Controls.EventHandler(closeWindow);
                btnSave.Click += new TomShane.Neoforce.Controls.EventHandler(btnSave_Click);


                btnCancel = new Button(manager);
                btnCancel.Init();
                btnCancel.Parent = window;
                btnCancel.Top = HeightCounter;
                HeightCounter += VertPadding * 2 + btnSave.Height;
                btnCancel.Text = "Cancel";
                btnCancel.Left = VertPadding * 2 + btnSave.Width;
                btnCancel.Click += new TomShane.Neoforce.Controls.EventHandler(closeWindow);

            }
        }

        public void addDelegate(confirmDelegate exec)
        {
            action = exec;
            Button btnCancel = new Button(manager);
            btnCancel.Init();
            btnCancel.Parent = window;
            btnCancel.Top = btnOk.Top;
            btnCancel.Text = "Cancel";
            btnCancel.Left = btnOk.Width + VertPadding * 3;
            btnOk.Click += new TomShane.Neoforce.Controls.EventHandler(
                delegate(object sender, TomShane.Neoforce.Controls.EventArgs e) { action(true); });
            btnCancel.Click += new TomShane.Neoforce.Controls.EventHandler(
                delegate(object sender, TomShane.Neoforce.Controls.EventArgs e) { action(false); });
            btnCancel.Click += new TomShane.Neoforce.Controls.EventHandler(closeWindow);
        }
        void btnSave_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            confirmDelegate pass = delegate(bool c)
            {
                if (c)
                {
                    bool updatePresetList = true;
                    ui.editNode.name = tbName.Text.Trim();

                    string filepath = "Presets//Nodes";
                    DirectoryInfo d = new DirectoryInfo(filepath);
                    if (!d.Exists) d.Create();
                    List<FileInfo> filesWithName = d.GetFiles(ui.editNode.name + ".xml").ToList();
                    if (filesWithName.Count > 0) //we must be overwriting, therefore don't update the live presetList
                    {
                        updatePresetList = false;
                    }

                    string filename = "Presets//Nodes//" + ui.editNode.name + ".xml";
                    //System.Console.WriteLine("OUTRAGEOUS. TRULY OUTREAGEOUS.");

                    Node serializenode = new Node();
                    Node.cloneObject(ui.editNode, serializenode);

                    ui.room.serializer.Serialize(serializenode, filename);



                    //System.Console.WriteLine("name ::: " + d.FullName);
                    if (updatePresetList)
                    {
                        foreach (FileInfo file in d.GetFiles(ui.editNode.name + ".xml"))
                        {
                            string fname = file.Name;
                            //System.Console.WriteLine(filename);
                            //string path = file.FullName;
                            fname = "Presets//Nodes//" + fname;
                            ui.game.NodePresets.Add((Node)ui.room.serializer.Deserialize(fname));
                            ui.game.presetFileInfos.Add(file);
                            break;
                        }
                    }

                }
            };

            foreach (Node preset in ui.game.NodePresets)
            {
                if (preset.name == tbName.Text)
                {
                    //copyname exists
                    PopupWindow failure = new PopupWindow(ui, PopUpType.prompt, "A preset already has that name\nOverwrite anyways?");
                    //failure.addDelegate(pass);
                    //List<int> li = new List<int>();
                    //li.ForEach()

                    return;
                }
            }
            pass(true);

        }

        void window_FocusLost(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            Dialog window = (Dialog)sender;
            //window.Focused = true;
            //manager.FocusedControl = window;
            //manager.AutoUnfocus = false;
            //manager.GetControl("win1").Invalidate();
            //System.Console.WriteLine("Dfdf");
        }

        void closeWindow(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {

            window.Close();
        }

        void btnAddComponent_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            if (cbBox.ItemIndex == -1)
            {
                PopupWindow fail = new PopupWindow(ui, PopUpType.alert, "You haven't selected a component.");
                return;
            }

            if (ui.editNode == null)
            {
                PopupWindow fail = new PopupWindow(ui, PopUpType.alert, "EditNode is null.");
                return;
            }

            confirmDelegate overwriteComp = delegate(bool c)
            {
                if (c)
                {
                    ui.editNode.addComponent((comp)cbBox.Items.ElementAt(cbBox.ItemIndex), true, true);
                    if (ui.sidebar.panelControls.Keys.Count > 0) ui.sidebar.DisableControls(ui.sidebar.groupPanel); //TODO

                    ui.sidebar.compLst = TreeListItem.GenerateList(ui.editNode, "");
                }
            };

            if (ui.editNode.comps.ContainsKey((comp)cbBox.Items.ElementAt(cbBox.ItemIndex)))
            {
                PopupWindow fail = new PopupWindow(ui, PopUpType.prompt, "The node already contains this component. Overwrite to default component?");
                fail.addDelegate(overwriteComp);
                return;
            }
            overwriteComp(true);
        }
    }

}
