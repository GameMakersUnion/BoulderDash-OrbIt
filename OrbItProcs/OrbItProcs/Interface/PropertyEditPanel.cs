﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using TomShane.Neoforce.Controls;
using Component = OrbItProcs.Components.Component;

namespace OrbItProcs.Interface
{
    public class PropertyEditPanel
    {
        public Sidebar sidebar;
        public GroupPanel grouppanel;
        public Dictionary<string, Control> panelControls;
        private InspectorItem activeInspectorItem;
        public Type editType;


        private int HeightCounter = 0;
        private int LeftPadding = 10;
        private bool triggerResizeSlider = true;

        public PropertyEditPanel(GroupPanel grouppanel, Sidebar sidebar)
        {
            this.sidebar = sidebar;
            this.grouppanel = grouppanel;
            panelControls = new Dictionary<string, Control>();

        }

        public void DisableControls()
        {
            List<string> list = panelControls.Keys.ToList(); // for some reason this isn't updated if you click quickly
            foreach (string key in list)
            {
                grouppanel.Remove(panelControls[key]);
                panelControls.Remove(key);
            }
        }

        public void UpdatePanel(InspectorItem inspectorItem)
        {
            if (activeInspectorItem == inspectorItem) return;

            if (panelControls.Keys.Count > 0) DisableControls();

            activeInspectorItem = inspectorItem;

            /*
            if (inspectorItem.itemtype == treeitem.component)
            {
                //System.Console.WriteLine("Component, run boolean code");
                CheckBox chkbox = new CheckBox(manager);
                chkbox.Init();
                chkbox.Parent = grouppanel;
                chkbox.Left = LeftPadding;
                chkbox.Top = 10;
                chkbox.Width = 120;
                chkbox.Checked = (bool)treeItem.node.isCompActive(treeItem.component);
                chkbox.Text = treeItem.component + " (" + treeItem.node.isCompActive(treeItem.component) + ")";
                chkbox.CheckedChanged += new TomShane.Neoforce.Controls.EventHandler(chkbox_CheckedChanged);

                panelControls.Add("chkbox", chkbox);

                return;
            }
            */


            //grouppanel.Text = activeInspectorItem.ToString(); //.Name();
            grouppanel.Text = activeInspectorItem.Name();

            if (!activeInspectorItem.HasPanelElements()) return;

            editType = activeInspectorItem.obj.GetType();

            if (editType == typeof(int) || editType == typeof(Single) || editType == typeof(string))
            {
                //System.Console.WriteLine("It's an int or float.");
                TextBox txtbox = new TextBox(grouppanel.Manager);
                txtbox.Init();
                txtbox.Parent = grouppanel;
                txtbox.Left = LeftPadding;
                txtbox.Top = 10;
                txtbox.Width = 80;
                txtbox.Height = txtbox.Height + 3;

                //txtbox.BackColor = Color.Green;

                //txtbox.DrawBorders = true;
                txtbox.Text = activeInspectorItem.obj.ToString();

                Button btnModify = new Button(grouppanel.Manager);
                btnModify.Init();
                btnModify.Parent = grouppanel;
                btnModify.Left = LeftPadding * 2 + txtbox.Width;
                btnModify.Top = 10;
                btnModify.Width = 80;
                btnModify.Text = "Modify";
                btnModify.Click += new TomShane.Neoforce.Controls.EventHandler(btnModify_Click);


                panelControls.Add("txtbox", txtbox);
                panelControls.Add("btnModify", btnModify);

                if (editType == typeof(int) || editType == typeof(Single))
                {
                    TrackBar trkMain = new TrackBar(grouppanel.Manager);
                    trkMain.Init();
                    trkMain.Parent = grouppanel;
                    trkMain.Left = LeftPadding;
                    trkMain.Top = 20 + btnModify.Height;
                    trkMain.Width = txtbox.Width + btnModify.Width + LeftPadding;
                    trkMain.Anchor = Anchors.Left | Anchors.Top | Anchors.Right;
                    int val = Convert.ToInt32(activeInspectorItem.obj);
                    trkMain.Value = val;
                    if (val == 0) val = 5;
                    trkMain.Range = val * 2;

                    //trkMain.
                    trkMain.ValueChanged += new TomShane.Neoforce.Controls.EventHandler(trkMain_ValueChanged);
                    //trkMain.btnSlider.MouseUp += new TomShane.Neoforce.Controls.MouseEventHandler(trkMain_MouseUp);
                    panelControls.Add("trkMain", trkMain);
                }

            }
            else if (editType == typeof(bool))
            {
                //System.Console.WriteLine("It's a boolean.");
                CheckBox chkbox = new CheckBox(grouppanel.Manager);
                chkbox.Init();
                chkbox.Parent = grouppanel;
                chkbox.Left = LeftPadding;
                chkbox.Top = 10;
                chkbox.Width = 120;
                chkbox.Checked = (bool)activeInspectorItem.obj;
                chkbox.Text = activeInspectorItem.Name() + " (" + activeInspectorItem.obj + ")";
                chkbox.CheckedChanged += new TomShane.Neoforce.Controls.EventHandler(chkbox_CheckedChanged);
                panelControls.Add("chkbox", chkbox);

            }



        }

        //not called currently, but we should edit the access to this event in the trackbar class in neoforce again.
        void trkMain_MouseUp(object sender, TomShane.Neoforce.Controls.MouseEventArgs e)
        {
            //TrackBar trkbar = (TrackBar)sender;
            //System.Console.WriteLine("yeah");
            TrackBar trkbar = (TrackBar)panelControls["trkMain"];

            if (trkbar.Value == trkbar.Range) trkbar.Range *= 2;
        }

        void trkMain_ValueChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            TrackBar trkbar = (TrackBar)sender;
            //GroupPanel gp = (GroupPanel)(trkbar.Parent.Parent);

            if (activeInspectorItem.obj is int)
            {
                activeInspectorItem.SetValue(trkbar.Value);
                panelControls["txtbox"].Text = "" + trkbar.Value;
            }
            else if (activeInspectorItem.obj is Single)
            {
                activeInspectorItem.SetValue((Single)trkbar.Value);
                panelControls["txtbox"].Text = "" + trkbar.Value;
            }

            //attempt to stop the slider range from changing rapidly
            if (trkbar.Value != trkbar.Range)
            {
                triggerResizeSlider = true;
            }

            if (triggerResizeSlider && trkbar.Value == trkbar.Range)
            {
                trkbar.Range *= 2;
                triggerResizeSlider = false;
            }
            
        }

        void chkbox_CheckedChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;

            /*
            if (item.itemtype == treeitem.component)
            {
                item.node.setCompActive(item.component, checkbox.Checked);
                checkbox.Text = item.component + " (" + item.node.isCompActive(item.component) + ")";
            }
            */

            activeInspectorItem.SetValue(checkbox.Checked);
            checkbox.Text = activeInspectorItem.Name() + " (" + activeInspectorItem.GetValue() + ")";


        }

        void btnModify_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            String str;
            Type t = activeInspectorItem.obj.GetType();

            if (t == typeof(int))
            {
                str = panelControls["txtbox"].Text.Trim();
                int integer;
                if (str.Length < 1) return;
                if (Int32.TryParse(str, out integer))
                    activeInspectorItem.SetValue(integer);
                else
                    return;
            }
            if (t == typeof(Single))
            {
                str = panelControls["txtbox"].Text.Trim();
                float f;
                if (str.Length < 1) return;
                if (float.TryParse(str, out f))
                    activeInspectorItem.SetValue(f);
                else
                    return;
            }
            if (t == typeof(string))
            {
                str = panelControls["txtbox"].Text;
                if (str.Length < 1) return;
                activeInspectorItem.SetValue(str);
                return;
            }

        }


    }
}