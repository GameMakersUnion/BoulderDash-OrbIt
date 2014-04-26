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
    public class Shovel : Component
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
                if (shovelNode != null)
                {
                    shovelNode.active = value;
                }
            }
        }
        /// <summary>
        /// The shovel node that will be held and swung.
        /// </summary>
        [Info(UserLevel.User, "The shovel node that will be held and swung.")]
        [CopyNodeProperty]
        public Node shovelNode { get; set; }

        public const mtypes CompType = mtypes.playercontrol | mtypes.minordraw | mtypes.item;// | mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }
        /// <summary>
        /// The radius of the fist.
        /// </summary>
        [Info(UserLevel.User, "The radius of the shovel.")]
        public float shovelRadius { get; set; }
        ///// <summary>
        ///// The multiplier affects the strength of the torch, which will make to more gravitating, or more repulsive.
        ///// </summary>
        //[Info(UserLevel.User, "The multiplier affects the strength of the torch, which will make to more gravitating, or more repulsive.")]
        //public float shovelMultiplier { get; set; }
        /// <summary>
        /// Represents the max distance the torch can reach from the player.
        /// </summary>
        [Info(UserLevel.User, "Represents the max distance the shovel can reach from the player.")]
        public float shovelReach { get; set; }

        public Shovel() : this(null) { }
        public Shovel(Node parent)
        {
            this.parent = parent;
            shovelRadius = 15;
            shovelReach = 200;

            Dictionary<dynamic, dynamic> props = new Dictionary<dynamic, dynamic>()
            {
                {comp.movement, true},
                {comp.basicdraw, true},
                {comp.collision, false},
            };

            shovelNode = new Node(parent.room, props);
            shovelNode.name = "shovel";
            shovelNode.body.radius = shovelRadius;
        }

        public override void AfterCloning()
        {
            if (shovelNode == null) return;
            shovelNode = shovelNode.CreateClone(parent.room);
        }

        public override void OnSpawn()
        {
            //Node.cloneNode(parent.Game1.ui.sidebar.ActiveDefaultNode, sword);
            //parent.body.texture = textures.orientedcircle;
            shovelNode.Kawasaki["shovelnodeparent"] = parent;
            shovelNode.body.pos = parent.body.pos;

            shovelNode.ExclusionCheck += (node) => node == parent;

            parent.room.itemGroup.IncludeEntity(shovelNode);
            shovelNode.OnSpawn();
            shovelNode.body.AddExclusionCheck(parent.body);
            Spring spring = new Spring();
            spring.restdist = 100;
            spring.springMode = Spring.mode.PullOnly;

            shovelLink = new Link(shovelNode, new HashSet<Node>(), spring);
            

        }
        Link shovelLink;
        bool shovelling = false;
        float deadzone = 0.5f;
        public override void PlayerControl(Controller controller)
        {
            if (controller is FullController)
            {
                FullController fc = (FullController)controller;
                Vector2 newstickpos = fc.newGamePadState.ThumbSticks.Right;
                newstickpos.Y *= -1;
                Vector2 pos = newstickpos * shovelReach;
                shovelNode.body.pos = parent.body.pos + pos;

                if (shovelling)
                {
                    if (fc.newGamePadState.Triggers.Right < deadzone && fc.oldGamePadState.Triggers.Right > deadzone)
                    {
                        shovelling = false;

                    }
                    else
                    {

                    }
                }
                else
                {
                    if (fc.newGamePadState.Triggers.Right > deadzone && fc.oldGamePadState.Triggers.Right < deadzone)
                    {
                        shovelling = true;
                        //parent.room.gridsystemAffect.retrieveBuckets()
                    }
                    else
                    {

                    }
                }
            }
        }


        public override void OnRemove(Node other)
        {
            shovelNode.OnDeath(other);
        }
    }
}