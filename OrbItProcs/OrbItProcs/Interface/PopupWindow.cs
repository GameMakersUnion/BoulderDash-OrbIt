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

namespace OrbItProcs.Interface {

    public delegate void confirmDelegate(bool confirm);

    public class PopupWindow {
        public Manager manager;
        public string windowtype;
        public Game1 game;
        public Dialog window;
        public ComboBox cbBox;
        public Button btnAdd, btnOk, btnCancel;
        public Label lbl;
        
        public confirmDelegate exc;
        public TextBox tbName;
        public Button btnSave;

        Node activeNode;

        public int HeightCounter = 10;
        public int VertPadding = 10;
        public string message;

        public PopupWindow(Game1 game, string windowType, string message = "You shouldn't have come here!")
        {
            this.game = game;
            this.manager = game.Manager;
            this.windowtype = windowType;
            this.message = message;

            if (windowType.Equals("saveNode") || windowType.Equals("addComponent"))
            {
                //System.Console.WriteLine("ya:" + game.ui.lstMain.ItemIndex);
                
                //if (game.ui.lstMain.ItemIndex == -1 || game.ui.lstMain.Items.Count < 1)
                if (game.ui.editNode == null)
                {
                    PopupWindow nothingSelected = new PopupWindow(game, "showMessage", "You haven't selected a Node.");
                    return;
                }
                //activeNode = (Node)game.ui.lstMain.Items.ElementAt(game.ui.lstMain.ItemIndex);
                activeNode = game.ui.editNode;
            }
            
            Initialize(windowType);
            int a;
            
        }

        public void Initialize(string windowType)
        {
            if (windowType.Equals("showMessage"))
            {
                window = new Dialog(manager);

                window.Init();
                window.ShowModal();
                //window.Caption.Text = "";
                window.Caption.Height = 1;
                window.Caption.Width = 1;
                window.Description.Top = window.Caption.Top;
                window.Description.Text = "";
                window.Description.Height = 1000;
                //if (window.Skin.Layers.ElementAt(0).Text.Font.Resource.MeasureString(message).X < window.Description.Width) for (int i = 0; i < message.Length; i++) if (message[(message.Length / 2 - (int)Math.Cos(Math.PI * i) * (i + 1) / 2)].Equals(" ")) { message.Insert((message.Length / 2 - (int)Math.Cos(Math.PI * i) * (i + 1) / 2), "\n"); break; }
                //window.Caption.Width = (int)window.Skin.Layers.ElementAt(0).Text.Font.Resource.MeasureString(message).X;

                int chars = 25;
                for (int i = 1; i <= 4; i++)
                {
                    if (message.Length > chars * i)
                    {
                        for (int j = chars * i; j > (chars * i) -chars; j--)
                        {
                            if (message.ElementAt(j).Equals(' '))
                            {
                                message = message.Insert(j+1, "\n");
                                
                                break;
                            }
                        }
                        //System.Console.WriteLine("{0} ::: {1}", message.Length, chars * i);
                    }

                    
                }
                
                //System.Console.WriteLine("{0}", message);
                window.Description.Text = message;
                //window.Description.Text = "adfsa\ndsafsa";

                //window.TopPanel.Visible = true;
                //window.Text = ;
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
            if (windowType.Equals("addComponent"))
            {
                window = new Dialog(manager);

                window.Init();
                window.ShowModal();
                window.Caption.Height = 1;
                window.Caption.Width = 1;
                window.Description.Top = window.Caption.Top;

                window.Description.Text = "Add to component to:\n" + activeNode.name;
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
                    if (!game.ui.editNode.comps.ContainsKey(c))
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
            if (windowType.Equals("saveNode"))
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
                tbName.Text = activeNode.name;
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
            exc = exec;
            Button btnCancel = new Button(manager);
            btnCancel.Init();
            btnCancel.Parent = window;
            btnCancel.Top = btnOk.Top;
            btnCancel.Text = "Cancel";
            btnCancel.Left = btnOk.Width + VertPadding * 3;
            btnOk.Click += new TomShane.Neoforce.Controls.EventHandler(
                delegate(object sender, TomShane.Neoforce.Controls.EventArgs e) { exc(true); });
            btnCancel.Click += new TomShane.Neoforce.Controls.EventHandler(
                delegate(object sender, TomShane.Neoforce.Controls.EventArgs e) { exc(false); });
            btnCancel.Click += new TomShane.Neoforce.Controls.EventHandler(closeWindow);
        }
        void btnSave_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            confirmDelegate pass = delegate(bool c)
            {
                if (c)
                {
                    bool updatePresetList = true;
                    activeNode.name = tbName.Text.Trim();

                    string filepath = "Presets//Nodes";
                    DirectoryInfo d = new DirectoryInfo(filepath);
                    if (!d.Exists) d.Create();
                    List<FileInfo> filesWithName = d.GetFiles(activeNode.name + ".xml").ToList();
                    if (filesWithName.Count > 0) //we must be overwriting, therefore don't update the live presetList
                    {
                        updatePresetList = false; 
                    }
                    
                    string filename = "Presets//Nodes//" + activeNode.name + ".xml";
                    //System.Console.WriteLine("OUTRAGEOUS. TRULY OUTREAGEOUS.");

                    Node serializenode = new Node();
                    Node.cloneObject(activeNode, serializenode);

                    game.room.serializer.Serialize(serializenode, filename);

                    
                    
                    //System.Console.WriteLine("name ::: " + d.FullName);
                    if (updatePresetList)
                    {
                        foreach (FileInfo file in d.GetFiles(activeNode.name + ".xml"))
                        {
                            string fname = file.Name;
                            //System.Console.WriteLine(filename);
                            //string path = file.FullName;
                            fname = "Presets//Nodes//" + fname;
                            game.NodePresets.Add((Node)game.room.serializer.Deserialize(fname));
                            game.presetFileInfos.Add(file);
                            break;
                        }
                    }

                }
            };

            foreach (Node preset in game.NodePresets)
            {
                if (preset.name == tbName.Text)
                {
                    //copyname exists
                    PopupWindow failure = new PopupWindow(game, "showMessage", "A preset already has that name\nOverwrite anyways?");
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
                PopupWindow fail = new PopupWindow(game, "showMessage", "You haven't selected a component.");
                return;
            }

            if (game.ui.editNode == null)
            {
                PopupWindow fail = new PopupWindow(game, "showMessage", "EditNode is null.");
                return;
            }

            confirmDelegate overwriteComp = delegate(bool c)
            {
                if (c)
                {
                    game.ui.editNode.addComponent((comp)cbBox.Items.ElementAt(cbBox.ItemIndex), true, true);
                    if (game.ui.sidebar.panelControls.Keys.Count > 0) game.ui.sidebar.DisableControls(game.ui.sidebar.groupPanel); //TODO

                    game.ui.sidebar.compLst = TreeListItem.GenerateList(game.ui.editNode, "");
                }
            };

            if (game.ui.editNode.comps.ContainsKey((comp)cbBox.Items.ElementAt(cbBox.ItemIndex)))
            {
                PopupWindow fail = new PopupWindow(game, "showMessage", "The node already contains this component. Overwrite to default component?");
                fail.addDelegate(overwriteComp);
                return;
            }
            overwriteComp(true);
        }
    }

}
