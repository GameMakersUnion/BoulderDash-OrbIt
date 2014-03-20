using System;
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

        public DetailedView(Sidebar sidebar, Control parent, int Left, int Top)
            : base(sidebar, parent, Left, Top)
        {
            viewItems = new List<DetailedItem>();
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
