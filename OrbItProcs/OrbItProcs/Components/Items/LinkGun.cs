using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace OrbItProcs
{
    /// <summary>
    /// The shovel allows you to pick up nodes, hold them, and throw them away.
    /// </summary>
    [Info(UserLevel.User, "The shovel allows you to pick up nodes, hold them, and throw them away.", CompType)]
    public class LinkGun : Component
    {
        public override bool active
        {
            get
            {
                return base.active;
            }
            set
            {
                base.active = value;
                if (linkNode != null)
                {
                    linkNode.active = value;
                }
            }
        }
        /// <summary>
        /// The shovel node that will be held and swung.
        /// </summary>
        [Info(UserLevel.User, "The shovel node that will be held and swung.")]
        [CopyNodeProperty]
        public Node linkNode { get; set; }

        public const mtypes CompType = mtypes.playercontrol | mtypes.minordraw | mtypes.item;// | mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }
        /// <summary>
        /// The radius of the fist.
        /// </summary>
        [Info(UserLevel.User, "The radius of the shovel.")]
        public float linkNodeRadius { get; set; }
        public bool linkToPlayers { get; set; }

        public LinkGun() : this(null) { }
        public LinkGun(Node parent)
        {
            this.parent = parent;
            this.com = comp.shovel;
            linkNodeRadius = 15;
            linkToPlayers = true;

            linkNode = new Node(parent.room);
            linkNode.name = "shovel";
            linkNode.body.radius = linkNodeRadius;
            linkNode.body.ExclusionCheck += (c1, c2) => c2 == parent.body;
            linkNode.body.mass = 0.001f;
            linkNode.body.texture = textures.shoveltip;
        }

        public override void AfterCloning()
        {
            if (linkNode == null) return;
            linkNode = linkNode.CreateClone(parent.room);
        }

        public override void OnSpawn()
        {
            //Node.cloneNode(parent.Game1.ui.sidebar.ActiveDefaultNode, sword);
            //parent.body.texture = textures.orientedcircle;
            linkNode.dataStore["linknodeparent"] = parent;
            linkNode.body.pos = parent.body.pos;

            linkNode.ExclusionCheck += (node) => node == parent;

            parent.room.itemGroup.IncludeEntity(linkNode);
            linkNode.OnSpawn();
            linkNode.body.AddExclusionCheck(parent.body);
            Spring spring = new Spring();
            spring.restdist = 0;
            spring.springMode = Spring.mode.PullOnly;
            spring.active = true;
            spring.multiplier = 1000;

            Tether tether = new Tether();
            tether.mindist = 0;
            tether.maxdist = 20;
            tether.active = true;

            shovelLink = new Link(linkNode, new HashSet<Node>(), spring);
            shovelLink.components.Add(tether);


        }
        Link rangeLink;
        Link shovelLink;
        public override void PlayerControl(Controller controller)
        {
            if (controller is FullController)
            {

            }
        }
        public override void Draw()
        {
            Color col = Color.White;

            Vector2 position = linkNode.body.pos;
            if (position == Vector2.Zero) position = parent.body.pos;
            else
            {
                parent.room.camera.DrawLine(position, parent.body.pos, 2f, col, Layers.Over3);
            }
        }

        public override void OnRemove(Node other)
        {
            linkNode.OnDeath(other);
        }
    }
}