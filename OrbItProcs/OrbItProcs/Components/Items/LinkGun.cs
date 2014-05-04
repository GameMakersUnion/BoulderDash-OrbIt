﻿using System;
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
                if (shootNode != null)
                {
                    shootNode.active = value;
                }
            }
        }
        /// <summary>
        /// The shovel node that will be held and swung.
        /// </summary>
        [Info(UserLevel.User, "The shovel node that will be held and swung.")]
        [CopyNodeProperty]
        public Node shootNode { get; set; }

        public const mtypes CompType = mtypes.playercontrol | mtypes.minordraw | mtypes.item;// | mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }
        /// <summary>
        /// The radius of the fist.
        /// </summary>
        [Info(UserLevel.User, "The radius of the shovel.")]
        public float shootNodeRadius { get; set; }
        public float shootNodeSpeed { get; set; }
        public bool linkToPlayers { get; set; }
        private Link shootLink, parentLink;
        public LinkGun() : this(null) { }
        public LinkGun(Node parent)
        {
            this.parent = parent;
            //this.com = comp.shovel;
            shootNodeRadius = 25; //fill in property later
            linkToPlayers = true;
            shootNodeSpeed = 5f;

            Dictionary<dynamic, dynamic> props = new Dictionary<dynamic, dynamic>()
            {
                {comp.movement, true},
                {comp.basicdraw, true},
                {comp.collision, true},
            };

            shootNode = new Node(parent.room, props);
            shootNode.name = "linknode";
            shootNode.body.radius = shootNodeRadius;
            shootNode.body.ExclusionCheck += (c1, c2) => c2 == parent.body;
            shootNode.body.mass = 2f;
            shootNode.body.texture = textures.cage;
        }

        public override void AfterCloning()
        {
            if (shootNode == null) return;
            shootNode = shootNode.CreateClone(parent.room);
        }

        public override void OnSpawn()
        {
            //Node.cloneNode(parent.Game1.ui.sidebar.ActiveDefaultNode, sword);
            //parent.body.texture = textures.orientedcircle;
            shootNode.dataStore["linknodeparent"] = parent;
            shootNode.body.pos = parent.body.pos;
            shootNode.addComponent<ColorChanger>(true);

            shootNode.AffectExclusionCheck += (node) => node == parent;

            

            parent.room.itemGroup.IncludeEntity(shootNode);
            shootNode.OnSpawn();
            shootNode.body.AddExclusionCheck(parent.body);
            shootNode.active = false;
            shootNode.movement.maxVelocity.value = 50f;

            spring = new Spring();
            spring.restdist = 0;
            spring.springMode = Spring.mode.PushOnly;
            spring.active = true;
            spring.multiplier = 400;

            //Tether tether = new Tether();
            //tether.mindist = 0;
            //tether.maxdist = 20;
            //tether.active = true;

            //shootNodeLink = new Link(parent, shootNode, spring);
            //shootNodeLink.IsEntangled = true;
            ////shovelLink.components.Add(tether);

            grav = new Gravity();
            grav.active = true;
            grav.mode = Gravity.Mode.ConstantForce;
            //grav.Repulsive = true;
            grav.multiplier = 100;

            shootLink = new Link(parent, shootNode, grav);
            shootLink.AddLinkComponent(spring, true);
            
            Gravity attachGrav = new Gravity();
            attachGrav.active = true;
            attachGrav.multiplier = 100;
            attachGrav.mode = Gravity.Mode.ConstantForce;

            Tether aTether = new Tether();
            aTether.maxdist = 100;
            aTether.mindist = 100;
            aTether.activated = true;

            attachedNodesQueue = new Queue<Node>();
            attachLink = new Link(parent, new HashSet<Node>(), attachGrav);
            attachLink.AddLinkComponent(aTether, true);

            shootNode.body.OnCollisionEnter += (n, other) =>
                {
                    if (other == parent) return;
                    //shootLink.targetNode = o;
                    //shootLink.active = true;
                    if (!attachLink.targets.Contains(other))
                    {
                        attachLink.targets.Add(other);
                        attachedNodesQueue.Enqueue(other);
                        attachLink.active = true;

                    }
                };

            parent.body.ExclusionCheck += (c1, c2) =>
                attachLink.targets.Contains(c2.parent);

        }

        public enum GunState
        {
            inactive,
            extending,
            retracting,
            //attached,
        }
        Gravity grav;
        Spring spring;
        GunState state = GunState.inactive;
        Queue<Node> attachedNodesQueue;
        Link attachLink;
        //bool deployed = false;
        public override void PlayerControl(Controller controller)
        {
            if (controller is FullController)
            {
                FullController fc = (FullController)controller;

                if (state == GunState.inactive)
                {
                    if (fc.newGamePadState.Triggers.Right > 0.5 && fc.oldGamePadState.Triggers.Right < 0.5)
                    {
                        state = GunState.extending;
                        Vector2 dir = fc.GetRightStick().NormalizeSafe() * shootNodeSpeed + parent.body.velocity;
                        shootNode.body.pos = parent.body.pos + dir * 5;
                        shootNode.body.velocity = dir;
                        shootNode.active = true;

                        grav.active = false;
                        spring.active = true;
                        shootLink.active = true;
                        
                    }
                }
                else if (state == GunState.extending)
                {
                    if (fc.newGamePadState.Triggers.Right < 0.5 && fc.oldGamePadState.Triggers.Right > 0.5)
                    {
                        state = GunState.retracting;
                        grav.active = true;
                        spring.active = false;
                    }
                }
                else if (state == GunState.retracting)
                {
                    shootNode.body.velocity = VMath.Redirect(shootNode.body.velocity, parent.body.pos - shootNode.body.pos);
                    float catchZone = 20f; //1f for bipedal action
                    if ((parent.body.pos - shootNode.body.pos).Length() < catchZone)
                    {
                        state = GunState.inactive;
                        shootNode.active = false;
                        shootLink.active = false;
                    }
                }

                if (fc.newGamePadState.Buttons.RightShoulder == ButtonState.Pressed && fc.oldGamePadState.Buttons.RightShoulder == ButtonState.Released)
                {
                    if (attachedNodesQueue.Count > 0)
                    {
                        Node n = attachedNodesQueue.Dequeue();
                        if (attachLink.targets.Contains(n))
                        {
                            attachLink.targets.Remove(n);
                        }
                        if (attachedNodesQueue.Count == 0)
                        {
                            attachLink.active = false;
                        }
                        attachLink.formation.UpdateFormation();
                    }
                }

                if (fc.newGamePadState.Buttons.LeftShoulder == ButtonState.Pressed && fc.oldGamePadState.Buttons.LeftShoulder == ButtonState.Released)
                {
                    if (attachedNodesQueue.Count > 0)
                    {
                        if (attachedNodesQueue.Count != 0)
                            attachedNodesQueue = new Queue<Node>();
                        if (attachLink.targets.Count != 0)
                            attachLink.targets = new ObservableHashSet<Node>();

                        attachLink.active = false;
                    }
                }

            }
        }
        public override void Draw()
        {
            //Color col = Color.White;
            //
            //Vector2 position = shootNode.body.pos;
            //if (position == Vector2.Zero)
            //{
            //    position = parent.body.pos;
            //    return;
            //}
            //if (deployed)
            //{
            //    //parent.room.camera.DrawLine(position, parent.body.pos, 2f, col, Layers.Under2);
            //}
        }

        public override void OnRemove(Node other)
        {
            shootNode.OnDeath(other);
        }
    }
}