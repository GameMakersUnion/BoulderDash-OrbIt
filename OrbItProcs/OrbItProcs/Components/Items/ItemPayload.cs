using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace OrbItProcs
{
    /// <summary>
    /// A payload of items or components to add to the node that picks up this payload.
    /// </summary>
    [Info(UserLevel.User, "A payload of items or components to add to the node that picks up this payload.", CompType)]
    public class ItemPayload : Component
    {
        public const mtypes CompType = mtypes.draw | mtypes.affectself | mtypes.item;
        public override mtypes compType { get { return CompType; } set { } }

        //public Dictionary<Type, Component> payload { get; set; }
        /// <summary>
        /// The payload node contains all the components that will be picked up by the colliding player/node.
        /// </summary>
        [Info(UserLevel.User, "The payload node contains all the components that will be picked up by the colliding player/node.")]
        [CopyNodeProperty]
        public Node payloadNode { get; set; }
        public CollisionDelegate OnCollision;
        /// <summary>
        /// If enabled, components picked up will overwrite existing components.
        /// </summary>
        [Info(UserLevel.User, "If enabled, components picked up will overwrite existing components.")]
        public bool OverwriteComponents { get; set; }
        /// <summary>
        /// If enabled, the payload node will be erased after it is picked up.
        /// </summary>
        [Info(UserLevel.User, "If enabled, the payload node will be erased after it is picked up.")]
        public bool DieOnDelivery { get; set; }
        public HashSet<Node> AlreadyDelivered;
        public List<ParticlePack> particlePacks;
        /// <summary>
        /// The amount of particle packs to draw beneath the payload.
        /// </summary>
        [Info(UserLevel.User, "The amount of particle packs to draw beneath the payload.")]
        public int packCount = 1;
        /// <summary>
        /// The amount of time in seconds until the items in the payload disappear.
        /// </summary>
        [Info(UserLevel.User, "The amount of time in seconds until the items in the payload disappear.")]
        public Toggle<int> timeLimit { get; set; }
        public ItemPayload() : this(null) { }
        public ItemPayload(Node parent)
        {
            this.parent = parent;
            //payload = new Dictionary<Type, Component>();
            timeLimit = new Toggle<int>(5, false);
            AlreadyDelivered = new HashSet<Node>();
            OverwriteComponents = false;
            DieOnDelivery = true;
            OnCollision = (s, o) =>
            {
                if (o.player == null) return;
                if (AlreadyDelivered.Contains(o)) return;
                foreach (Type t in payloadNode.comps.Keys)
                {
                    if (o.HasComp(t) && !OverwriteComponents) continue;
                    Component comp = payloadNode.comps[t];
                    Component clone = comp.CreateClone(o);
                    o.addComponent(clone, true, true);
                    clone.OnSpawn();
                }
                if (DieOnDelivery)
                {
                    this.parent.OnDeath(null);//(o); to send the message that the node picking up the payload has 'killed' this payload
                    return;
                }
                AlreadyDelivered.Add(o);
            };

            if (parent == null) return;
            payloadNode = new Node(parent.room);
            payloadNode.comps.Remove(typeof(Movement));
            payloadNode.comps.Remove(typeof(Collision));
            payloadNode.comps.Remove(typeof(BasicDraw));
            payloadNode.comps.Remove(typeof(Meta));
            //testing
            //Gravity grav = new Gravity();
            //grav.mode = Gravity.Mode.Strong;
            //AddComponentItem(grav);
        }
        public override void AfterCloning()
        {
            Dictionary<Type, Component> newPayload = new Dictionary<Type, Component>();
            foreach (Type t in payloadNode.comps.Keys.ToList())
            {
                Component comp = payloadNode.comps[t];
                Component clone = comp.CreateClone(parent);
                newPayload[t] = clone;
                if (timeLimit.enabled)
                    clone.SetDecayMaxTime(timeLimit.value);
                
            }
            payloadNode.comps = newPayload;
        }
        public void AddComponentItem(Component component, bool overwrite = true)
        {
            Type t = component.GetType();
            if (payloadNode.comps.ContainsKey(t) && !overwrite) return;
            Component clone = component.CreateClone(parent);
            payloadNode.comps[t] = clone;
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
            foreach (Component c in payloadNode.comps.Values)
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
