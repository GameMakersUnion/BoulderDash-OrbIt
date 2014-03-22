﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomShane.Neoforce.Controls;
using Component = OrbItProcs.Component;
using Console = System.Console;
using EventHandler = TomShane.Neoforce.Controls.EventHandler;
using EventArgs = TomShane.Neoforce.Controls.EventArgs;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;

namespace OrbItProcs
{
    public class DetailedView : ListView<DetailedItem>
    {
        public Action<DetailedItem, object> ItemCreator;
        public Action<Control, DetailedItem, EventArgs> OnItemEvent;

        public DetailedView(Sidebar sidebar, Control parent, int Left, int Top, bool Init = true)
            : base(sidebar, parent, Left, Top, Init)
        {
            viewItems = new List<DetailedItem>();
            sidebar.ui.detailedViews.Add(this);
        }
        public void Setup(Action<DetailedItem, object> ItemCreator, Action<Control, DetailedItem, EventArgs> OnItemEvent)
        {
            this.ItemCreator = ItemCreator;
            this.OnItemEvent = OnItemEvent;
        }

        public void InvokeOnItemEvent(Control control, DetailedItem item, EventArgs eventArgs)
        {
            if (OnItemEvent != null)
            {
                OnItemEvent(control, item, eventArgs);
            }
        }
        
        public virtual void ClearView()
        {
            selectedItem = null;
            foreach(DetailedItem i in viewItems.ToList())
            {
                backPanel.Remove(i.textPanel);
                viewItems.Remove(i);
            }
        }
        public virtual void Refresh()
        {
            if (viewItems != null)
            {
                foreach (DetailedItem item in viewItems)
                {
                    if (item.obj == null) continue;
                    if (item.obj is InspectorItem)
                    {
                        object o = (item.obj as InspectorItem).obj;
                        if (o != null && o is Component || o is Node || o is Body)
                        {
                            continue;
                        }
                    }
                    item.label.Text = item.obj.ToString().LastWord('.');
                }
            }
        }
        public void SetButtonBool(Button button, bool b)
        {
            if (b)
            {
                button.Text = "On";
                button.TextColor = UserInterface.TomShanePuke;
            }
            else
            {
                button.Text = "Off";
                button.TextColor = Color.Red;
            }
        }
        public bool GetButtonBool(Button button, bool toggle = true)
        {
            if (button.Text.Equals("On"))
            {
                if (toggle) { SetButtonBool(button, false); return false; }
                return true;
            }
            else
            {
                if (toggle) { SetButtonBool(button, true); return true; }
                return false;
            }
        }
    }

    public class DetailedItem : ViewItem
    {
        public DetailedView detailedView;
        public Dictionary<string, Control> itemControls;

        public override bool isSelected
        {
            get
            {
                return base.isSelected;
            }
            set
            {
                base.isSelected = value;
                RefreshColor();
            }
        }

        public DetailedItem(Manager manager, DetailedView detailedView, object obj, Control parent, int Top, int Left, int Width)
            : base(manager, obj, parent, Top, Left, Width)
        {
            this.detailedView = detailedView;
            textPanel.Width = detailedView.backPanel.Width - 4;
            textColor = detailedView.textColor;
            backColor = detailedView.backColor;
            RefreshColor();
            textPanel.Click += delegate
            {
                if (detailedView != null)
                {
                    detailedView.SelectItem(this);
                }
            };

            itemControls = new Dictionary<string, Control>();
            if (detailedView.ItemCreator != null)
            {
                detailedView.ItemCreator(this, obj);
            }
        }

        public void AddControl(Control control)
        {
            if (detailedView == null || control == null || control.Name.Equals("")) return;

            if (itemControls.ContainsKey(control.Name))
            {
                RemoveControl(control.Name);
            }
            itemControls[control.Name] = control;
            if (control is ComboBox)
            {
                (control as ComboBox).ItemIndexChanged += (s, e) =>
                {
                    detailedView.InvokeOnItemEvent(control, this, e);
                };
                return;
            }
            control.Click += (s, e) =>
            {
                detailedView.InvokeOnItemEvent(control, this, e);
            };
            control.KeyUp += (s, e) =>
            {
                detailedView.InvokeOnItemEvent(control, this, e);
            };
            
            //todo: add more handlers as necessary
        }
        public void RemoveControl(string name)
        {

        }
    }
}
