using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TomShane.Neoforce.Controls;
using OrbItProcs;

namespace TomShane.Neoforce.Controls
{
    public class InspectorBox : ListBox
    {
        private ScrollBar sbVert = null;
        public InspectorItem rootitem = null;

        public static string DeriveControlName(Control control)
        {
            if (control != null)
            {
                try
                {
                    string str = control.ToString();
                    int i = str.LastIndexOf(".");
                    return str.Remove(0, i + 1);
                }
                catch
                {
                    return control.ToString();
                }
            }
            return control.ToString();
        }

        //private List<object> Items = new List<object>();
        public int startChildren = -1, endChildren = -1;

        public InspectorBox(Manager manager)
            : base(manager)
        {

            if (Manager != null && Manager.Skin != null && Manager.Skin.Controls != null)
            {
                SkinControl s = Manager.Skin.Controls[DeriveControlName(new ListBox(manager))];
                if (s != null) Skin = new SkinControl(s);
                else Skin = new SkinControl(Manager.Skin.Controls["Control"]);
            }
            else
            {
                throw new Exception("Control skin cannot be initialized. No skin loaded.");
            }
        }

        public String prefix(int index)
        {
            if (index == startChildren - 1) return "- ";
            if (index >= startChildren && index <= endChildren) return "  >";
            if (index < startChildren || index > endChildren) return "+ ";
            return "~~dingus";

        }

        public void addRangeAt(List<object> list, int index)
        {
            int length = Items.Count;
            if (index > length || index == 0)
            {
                System.Console.WriteLine("Ya dingus");
                return;
            }
            for (int i = 0; i < list.Count; i++)
            {
                Items.Insert(i + index, (object)list.ElementAt(i));
            }
            startChildren = index + 1;
            endChildren = startChildren + list.Count;
            //return;
        }

        private void DrawPane(object sender, DrawEventArgs e)
        {
            if (Items != null && Items.Count > 0)
            {
                SkinText font = Skin.Layers["Control"].Text;
                SkinLayer sel = Skin.Layers["ListBox.Selection"];
                int h = (int)font.Font.Resource.MeasureString(Items[0].ToString()).Y;
                int v = (sbVert.Value / 10);
                int p = (sbVert.PageSize / 10);
                int d = (int)(((sbVert.Value % 10) / 10f) * h);
                int c = Items.Count;
                int s = ItemIndex;

                for (int i = v; i <= v + p + 1; i++)
                {
                    if (i < c)
                    {
                        //e.Renderer.DrawString(this, Skin.Layers["Control"], prefix(i) + Items[i].ToString(), new Rectangle(e.Rectangle.Left, e.Rectangle.Top - d + ((i - v) * h), e.Rectangle.Width, h), false);
                        e.Renderer.DrawString(this, Skin.Layers["Control"], Items[i].ToString(), new Rectangle(e.Rectangle.Left, e.Rectangle.Top - d + ((i - v) * h), e.Rectangle.Width, h), false);
                    }
                }
                if (s >= 0 && s < c && (Focused || !HideSelection))
                {
                    int pos = -d + ((s - v) * h);
                    if (pos > -h && pos < (p + 1) * h)
                    {
                        e.Renderer.DrawLayer(this, sel, new Rectangle(e.Rectangle.Left, e.Rectangle.Top + pos, e.Rectangle.Width, h));

                        //e.Renderer.DrawString(this, sel, prefix(s) + Items[s].ToString(), new Rectangle(e.Rectangle.Left, e.Rectangle.Top + pos, e.Rectangle.Width, h), false);
                        e.Renderer.DrawString(this, sel, Items[s].ToString(), new Rectangle(e.Rectangle.Left, e.Rectangle.Top + pos, e.Rectangle.Width, h), false);
                    }
                }
            }
        }

    }
}
