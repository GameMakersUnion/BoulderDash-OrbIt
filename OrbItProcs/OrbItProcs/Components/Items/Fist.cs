﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace OrbItProcs
{
    /// <summary>
    /// The fist allows you to punch other players and nodes, sometimes with magical properties.
    /// </summary>
    [Info(UserLevel.User, "The fist allows you to punch other players and nodes, sometimes with magical properties.", CompType)]
    public class Fist : Component
    {
        /// <summary>
        /// The fist node that will be held and swung.
        /// </summary>
        [Info(UserLevel.User, "The sword node that will be held and swung.")]
        [CopyNodeProperty]
        public Node fistNode { get; set; }

        public const mtypes CompType = mtypes.playercontrol | mtypes.minordraw | mtypes.item;// | mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }
        /// <summary>
        /// The distance from the player the fist can reach.
        /// </summary>
        [Info(UserLevel.User, "The distance from the player the sword will swing at.")]
        public float fistReach { get; set; }
        /// <summary>
        /// The radius of the fist.
        /// </summary>
        [Info(UserLevel.User, "The radius of the fist.")]
        public float fistRadius { get; set; }
        /// <summary>
        /// The amount of damage the fist will do.
        /// </summary>
        [Info(UserLevel.User, "The amount of damage the fist will do.")]
        public float damageMultiplier { get; set; }
        /// <summary>
        /// The force at which to push the other node back when clashing fist.
        /// </summary>
        [Info(UserLevel.User, "The force at which to push the other node back when clashing swords.")]
        public float parryKnockback { get; set; }
        /// <summary>
        /// The force at which to push the other node back after a direct hit to the other node.
        /// </summary>
        [Info(UserLevel.User, "The force at which to push the other node back after a direct hit to the other node.")]
        public float nodeKnockback { get; set; }
        private bool movingStick = false;

        Vector2 target;
        public Fist() : this(null) { }
        public Fist(Node parent)
        {
            this.parent = parent;
            fistReach = 60;
            fistRadius = 15;
            damageMultiplier = 10f;
            parryKnockback = 20f;
            nodeKnockback = 500f;
            

            Dictionary<dynamic, dynamic> props = new Dictionary<dynamic, dynamic>()
            {
                {comp.movement, true},
                {comp.basicdraw, true},
                {comp.collision, true},
                //{comp.waver, true},
            };

            fistNode = new Node(parent.room, props);
            fistNode.name = "fist";
            fistNode.body.radius = fistRadius;
        }

        public override void AfterCloning()
        {
            if (fistNode == null) return;
            fistNode = fistNode.CreateClone(parent.room);
            //sword = new Node(parent.room, props);
        }

        public override void OnSpawn()
        {
            //Node.cloneNode(parent.Game1.ui.sidebar.ActiveDefaultNode, sword);
            //parent.body.texture = textures.orientedcircle;
            fistNode.Kawasaki["fistnodeparent"] = parent;
            fistNode.body.pos = parent.body.pos;
            

            parent.room.itemGroup.IncludeEntity(fistNode);
            fistNode.OnSpawn();
            fistNode.body.AddExclusionCheck(parent.body);
            fistNode.body.ExclusionCheck += delegate(Collider p, Collider o) { return !movingStick; };
            fistNode.body.OnCollisionEnter += (p, o) =>
            {
                if (o.Kawasaki.ContainsKey("swordnodeparent"))
                {
                    Node otherparent = o.Kawasaki["swordnodeparent"];
                    Vector2 f = otherparent.body.pos - parent.body.pos;
                    VMath.NormalizeSafe(ref f);
                    f *= parryKnockback;
                    otherparent.body.ApplyForce(f);
                }
                else if (o.Kawasaki.ContainsKey("fistnodeparent"))
                {
                    Node otherparent = o.Kawasaki["fistnodeparent"];
                    Vector2 f = otherparent.body.pos - parent.body.pos;
                    VMath.NormalizeSafe(ref f);
                    f *= parryKnockback;
                    otherparent.body.ApplyForce(f);
                }
                else if (o.player != null)
                {

                    o.player.node.meta.CalculateDamage(parent, damageMultiplier);

                }
            };
            //sword.body.exclusionList.Add(parent.body);
            //
            //parent.body.exclusionList.Add(sword.body);
        }
        public override void AffectSelf()
        {
        }
        public override void PlayerControl(Controller controller)
        {
            if (controller is FullController)
            {
                FullController fc = (FullController)controller;
                //fistNode.movement.active = false;
                //fistNode.body.velocity = fistNode.body.effvelocity * nodeKnockback;

                bool atReach = Vector2.Distance(fistNode.body.pos, parent.body.pos) > fistReach;

                if (fc.newGamePadState.ThumbSticks.Right.LengthSquared() > 0.2 * 0.2)
                {
                    if (!atReach)
                    {
                        movingStick = true;
                        target = fc.newGamePadState.ThumbSticks.Right * fistReach;
                        target.Y *= -1;
                        target = parent.body.pos + target;
                        Vector2 force = target - fistNode.body.pos;
                        fistNode.body.ApplyForce(force / 10);
                        //fistNode.body.velocity += force;
                        //Console.WriteLine(force.X + " : " + force.Y);
                    }
                }
                else
                {
                    target = parent.body.pos;
                    //movingStick = false;
                    //Vector2 restPos = new Vector2(parent.body.radius, 0).Rotate(parent.body.orient) + parent.body.pos;
                    //fistNode.body.pos = Vector2.Lerp(fistNode.body.pos, restPos, 0.1f);
                    //fistNode.body.orient = Utils.AngleLerp(fistNode.body.orient, parent.body.orient, 0.1f);
                }

                if (atReach)
                {
                    Vector2 direction = fistNode.body.pos - parent.body.pos;
                    VMath.NormalizeSafe(ref direction);
                    direction *= fistReach;
                    fistNode.body.pos = parent.body.pos + direction;
                }
            }
        }

        public override void Draw()
        {
            Vector2 position = fistNode.body.pos;
            if (position == Vector2.Zero) position = parent.body.pos;
            Utils.DrawLine(parent.room, position, parent.body.pos, 2f, parent.body.color);
            Utils.DrawLine(parent.room, target, parent.body.pos, 2f, Color.Red);
            parent.room.camera.Draw(textures.ring, position, parent.body.color, fistNode.body.scale, fistNode.body.orient);

            
        }
        public override void Death(Node other)
        {
            fistNode.OnDeath(other);
        }
    }
}