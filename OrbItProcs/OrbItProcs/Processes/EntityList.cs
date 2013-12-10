using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace OrbItProcs.Processes
{
    public class EntityList
    {
        public enum ListState { off, drawingOnly, updatingOnly, on };
        public ObservableCollection<Node> parentList { get; set; }
        public ObservableCollection<Node> entities { get; set; }
        
        public Node defaultNode { get; set; }
        public Room room;

        public string Name { get; set; }
        public ListState listState { get; set; }

        public EntityList() : this(null)
        {
        }
        public EntityList(Node defaultNode = null, ObservableCollection<Node> entities = null, ObservableCollection<Node> parentList = null, ListState listState = ListState.on, string Name = "newlist")
        {
            room = Program.getRoom();
            this.defaultNode = defaultNode ?? room.defaultNode;
            this.entities = entities ?? new ObservableCollection<Node>();
            this.parentList = parentList ?? new ObservableCollection<Node>();
            this.listState = ListState.on;
            this.Name = Name;

            entities.CollectionChanged += entities_CollectionChanged;

        }

        void entities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (Node n in e.NewItems)
                {
                    if (!parentList.Contains(n))
                    {
                        parentList.Add(n);
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (Node n in e.OldItems)
                {
                    if (parentList.Contains(n))
                    {
                        parentList.Remove(n);
                    }
                }
            }
        }

        public void Update(GameTime gametime)
        {
            if (listState == ListState.on || listState == ListState.updatingOnly)
            {
                foreach (Node n in entities.ToList())
                {
                    n.Update(gametime);
                }
            }
        }

        public void Draw(SpriteBatch spritebatch)
        {
            if (listState == ListState.on || listState == ListState.drawingOnly)
            {
                foreach (Node n in entities.ToList())
                {
                    n.Draw(spritebatch);
                }
            }
        }

        public string ToString()
        {
            return Name;
        }


    }
}
