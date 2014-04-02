using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace OrbItProcs
{
    public class ItemPayload : Component
    {
        public const mtypes CompType = mtypes.draw;
        public override mtypes compType { get { return CompType; } set { } }

        public Dictionary<Type, Component> payload { get; set; }
        public CollisionDelegate OnCollision;
        public bool OverwriteComponents { get; set; }
        public bool DieOnDelivery { get; set; }
        public HashSet<Node> AlreadyDelivered;
        public ItemPayload() : this(null) { }
        public ItemPayload(Node parent)
        {
            this.parent = parent;
            payload = new Dictionary<Type, Component>();
            AlreadyDelivered = new HashSet<Node>();
            OverwriteComponents = false;
            DieOnDelivery = true;
            OnCollision = (s, o) =>
            {
                if (o.player == null) return;
                if (AlreadyDelivered.Contains(o)) return;
                foreach(Type t in payload.Keys)
                {
                    if (o.HasComp(t) && !OverwriteComponents) continue;
                    Component comp = payload[t];
                    Component clone = comp.CreateClone(o);
                    o.addComponent(clone, true, true);
                }
                if (DieOnDelivery)
                {
                    this.parent.OnDeath(null);//(o); to send the message that the node picking up the payload has 'killed' this payload
                    return;
                }
                AlreadyDelivered.Add(o);
            };
            //testing
            Gravity grav = new Gravity();
            grav.mode = Gravity.Mode.Strong;
            AddComponentItem(grav);
        }
        public override void AfterCloning()
        {
            foreach (Type t in payload.Keys.ToList())
            {
                Component comp = payload[t];
                Component clone = comp.CreateClone(parent);
                payload[t] = clone;
            }
        }
        public void AddComponentItem(Component component, bool overwrite = true)
        {
            Type t = component.GetType();
            if (payload.ContainsKey(t) && !overwrite) return;
            Component clone = component.CreateClone(parent);
            payload[t] = clone;
        }

        public override void OnSpawn()
        {
            parent.collision.isSolid = false;
            parent.body.OnCollisionEnter += OnCollision;
        }

        public override void Draw()
        {
            foreach(Component c in payload.Values)
            {
                c.Draw();
            }
        }

    }
}
