using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace OrbItProcs
{
    public class ItemPayload : Component
    {
        public const mtypes CompType = mtypes.draw | mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }

        public Dictionary<Type, Component> payload { get; set; }
        public CollisionDelegate OnCollision;
        public bool OverwriteComponents { get; set; }
        public bool DieOnDelivery { get; set; }
        public HashSet<Node> AlreadyDelivered;
        public List<ParticlePack> particlePacks;
        public int packCount = 1;
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
            //Gravity grav = new Gravity();
            //grav.mode = Gravity.Mode.Strong;
            //AddComponentItem(grav);
        }
        public override void AfterCloning()
        {
            Dictionary<Type, Component> newPayload = new Dictionary<Type, Component>();
            foreach (Type t in payload.Keys.ToList())
            {
                Component comp = payload[t];
                Component clone = comp.CreateClone(parent);
                newPayload[t] = clone;
            }
            payload = newPayload;
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

            particlePacks = new List<ParticlePack>();
            for(int i = 0; i < packCount; i++)
            {
                particlePacks.Add(new ParticlePack(textures.randompixels, 1.0f, 0.4f));
            }
        }
        public override void AffectSelf()
        {
            foreach(var pack in particlePacks)
            {
                pack.Update();
            }
        }
        public override void Draw()
        {
            foreach (var pack in particlePacks)
            {
                pack.Draw(parent.room, parent.body.pos, parent.body.color);
            }
            foreach(Component c in payload.Values)
            {
                c.Draw();
            }
        }
    }
    public class ParticlePack
    {
        textures texture;
        float scale;
        float scaleRate;
        float scaleMax;
        float rotation;
        float rotationRate;
        float alpha;
        
        public ParticlePack(textures texture, float scaleMax, float alpha)
        {
            this.texture = texture;
            this.scaleMax = scaleMax;
            this.alpha = alpha;

            this.scale = (float)Utils.random.NextDouble();
            this.scaleRate = (float)Utils.random.NextDouble() * 0.10f + 0.05f; //#magic
            this.rotation = (float)(Utils.random.NextDouble() * Math.PI * 2.0);
            this.rotationRate = (float)Utils.random.NextDouble() * 1f;
        }

        public void Update()
        {
            scale = (scale + scaleRate) % scaleMax;
            rotation = (rotation + rotationRate) % (float)(Math.PI * 2.0);
        }

        public void Draw(Room room, Vector2 position, Color color)
        {
            room.camera.AddPermanentDraw(texture, position, color * alpha, scale, rotation, 20);
        }

    }
}
