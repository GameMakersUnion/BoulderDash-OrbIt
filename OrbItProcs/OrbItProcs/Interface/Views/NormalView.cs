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
    public class NormalView : ListView<ViewItem>
    {
        public event Action<ViewItem> OnSelectionChanged;
        public NormalView(Sidebar sidebar, Control parent, int Left, int Top, bool Init = true, int? Height = null)
            : base(sidebar, parent, Left, Top, Init, Height)
        {
            viewItems = new List<ViewItem>();
            backColor = UserInterface.TomDark;
            textColor = Color.Black;
        }
        public void AddObject(object o)
        {
            if (o == null) return;
            int height = 0;
            if (viewItems.Count > 0)
            {
                height = (viewItems[0].itemHeight + 2) * viewItems.Count;
            }
            ViewItem newItem = new ViewItem(manager, o, backPanel, height, LeftPadding);
            newItem.textColor = textColor;
            newItem.backColor = backColor;
            newItem.RefreshColor();

            viewItems.Add(newItem);
            SetupScroll(newItem);
            newItem.OnSelect += delegate
            {
                if (this != null)
                {
                    SelectItem(newItem);
                    InvokeOnSelectionChanged(newItem);
                }
            };
        }
        public void InvokeOnSelectionChanged(ViewItem item)
        {
            if (OnSelectionChanged != null)
            {
                OnSelectionChanged(item);
            }
        }
    }
}
