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
        /// <summary>
        /// Sets the maximum amount of nodes that can be captured by the shovel at any time.
        /// </summary>
        [Info(UserLevel.User, "Sets the maximum amount of nodes that can be captured by the shovel at any time.")]
        public int maxShovelCapacity { get; set; }
        /// <summary>
        /// The maximum reach the shovel can have in order to pick up nodes. (The size of the shovel head's reach)
        /// </summary>
        [Info(UserLevel.User, "The maximum reach the shovel can have in order to pick up nodes. (The size of the shovel head's reach)")]
        public float scoopReach { get; set; }
        public enum ModeShovelPosition
        {
            AbsoluteStickPos,
            PhysicsBased,
        }
        /// <summary>
        /// Controls how the shovel will behave in terms of player control. Absoulte make it the exact stick position. Physics based deals with forces.
        /// </summary>
        [Info(UserLevel.User, "Controls how the shovel will behave in terms of player control. Absoulte make it the exact stick position. Physics based deals with forces.")]
        public ModeShovelPosition modeShovelPosition { get; set; }
        public enum ModePlayers
        {
            GrabOtherPlayers,
            GrabSelf,
            GrabBoth,
            GrabNone,
        }
        /// <summary>
        /// The modePlayers allows you to specific which players the shovel can pick up. You can shovel yourself, other players, both, or none.
        /// </summary>
        [Info(UserLevel.User, "The modePlayers allows you to specific which players the shovel can pick up. You can shovel yourself, other players, both, or none.")]
        public ModePlayers modePlayers { get; set; }
        public Shovel() : this(null) { }
        public Shovel(Node parent)
        {
            this.parent = parent;
            this.com = comp.shovel;
            shovelRadius = 15;
            shovelReach = 100;
            scoopReach = 60;
            maxShovelCapacity = 5;
            modePlayers = ModePlayers.GrabNone;
            modeShovelPosition = ModeShovelPosition.PhysicsBased;
            physicsDivisor = 8;

            Dictionary<dynamic, dynamic> props = new Dictionary<dynamic, dynamic>()
            {
                {comp.movement, true},
                {comp.basicdraw, true},
                {comp.collision, true},
            };

            shovelNode = new Node(parent.room, props);
            shovelNode.name = "shovel";
            shovelNode.body.radius = shovelRadius;
            shovelNode.body.ExclusionCheck += (c1, c2) => c2 == parent.body;
            shovelNode.body.mass = 0.001f;
            shovelNode.body.texture = textures.shoveltip;
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
            shovelNode.dataStore["shovelnodeparent"] = parent;
            shovelNode.body.pos = parent.body.pos;

            shovelNode.ExclusionCheck += (node) => node == parent;

            parent.room.itemGroup.IncludeEntity(shovelNode);
            shovelNode.OnSpawn();
            shovelNode.body.AddExclusionCheck(parent.body);
            Spring spring = new Spring();
            spring.restdist = 0;
            spring.springMode = Spring.mode.PullOnly;
            spring.active = true;
            spring.multiplier = 1000;

            Tether tether = new Tether();
            tether.mindist = 0;
            tether.maxdist = 20;
            tether.active = true;

            shovelLink = new Link(shovelNode, new HashSet<Node>(), spring);
            shovelLink.components.Add(tether);

            //to keep the shovel in reach for physics based control
            Tether rangeTether = new Tether();
            rangeTether.mindist = 0;
            rangeTether.maxdist = (int)shovelReach;
            rangeTether.active = true;
            rangeLink = new Link(parent, shovelNode, rangeTether);
            if (modeShovelPosition == ModeShovelPosition.PhysicsBased)
            {
                rangeLink.active = true;
            }

            //exclusionDel = delegate(Collider c1, Collider c2)
            //{
            //    return shovelLink.active && shovelLink.targets.Contains(c2.parent);
            //};
            //shovelNode.body.ExclusionCheck += exclusionDel;
            //parent.body.ExclusionCheck += exclusionDel;

        }
        Link rangeLink;
        Link shovelLink;
        bool shovelling = false;
        float deadzone = 0.5f;
        public float physicsDivisor { get; set; }
        Func<Collider, Collider, bool> exclusionDel;
        float compoundedMass = 0f;
        public override void PlayerControl(Controller controller)
        {
            if (controller is FullController)
            {
                FullController fc = (FullController)controller;
                Vector2 newstickpos = fc.newGamePadState.ThumbSticks.Right;
                newstickpos.Y *= -1;
                Vector2 pos = newstickpos * shovelReach;
                Vector2 worldStickPos = parent.body.pos + pos;
                Vector2 diff = worldStickPos - shovelNode.body.pos;
                //float angle = Utils.VectorToAngle(shovelNode.body.pos - parent.body.pos) + VMath.PIbyTwo % VMath.twoPI;
                Vector2 shovelDir = shovelNode.body.pos - parent.body.pos;
                shovelDir = new Vector2(shovelDir.Y, -shovelDir.X);
                shovelNode.body.SetOrientV2(shovelDir);

                if (modeShovelPosition == ModeShovelPosition.AbsoluteStickPos)
                {
                    shovelNode.body.pos = worldStickPos;
                }
                else if (modeShovelPosition == ModeShovelPosition.PhysicsBased)
                {
                    float len = diff.Length();
                    if (len < 1)
                    {
                        shovelNode.body.velocity = Vector2.Zero;

                    }
                    else
                    {
                        float velLen = shovelNode.body.velocity.Length();

                        Vector2 diffcopy = diff;
                        VMath.NormalizeSafe(ref diffcopy);

                        Vector2 normalizedVel = shovelNode.body.velocity;
                        VMath.NormalizeSafe(ref normalizedVel);

                        float result = 0;
                        Vector2.Dot(ref diffcopy, ref normalizedVel, out result);

                        diffcopy *= result;
                        Vector2 force = (diff / physicsDivisor);
                        if (shovelling && compoundedMass >= 1) force /= compoundedMass * 3;
                        shovelNode.body.velocity = diffcopy + force;
                        //shovelNode.body.ApplyForce(force);
                    }
                }

                if (shovelling)
                {
                    if (fc.newGamePadState.Triggers.Right < deadzone && fc.oldGamePadState.Triggers.Right > deadzone)
                    {
                        shovelling = false;
                        foreach(Node n in shovelLink.targets.ToList())
                        {
                            n.body.velocity = n.body.effvelocity;
                            n.collision.active = true;
                            shovelLink.targets.Remove(n);
                            n.body.ClearExclusionChecks();
                        }
                        shovelLink.formation.UpdateFormation();
                        shovelLink.active = false;
                        shovelNode.room.AllActiveLinks.Remove(shovelLink);
                        compoundedMass = 0f;
                    }
                }
                else
                {
                    if (fc.newGamePadState.Triggers.Right > deadzone && fc.oldGamePadState.Triggers.Right < deadzone)
                    {
                        shovelling = true;
                        ObservableHashSet<Node> capturedNodes = new ObservableHashSet<Node>();
                        int count = 0;
                        Action<Collider, Collider> del = delegate(Collider c1, Collider c2){
                            if (count >= maxShovelCapacity) return;
                            if (c2.parent.dataStore.ContainsKey("shovelnodeparent"))return;
                            if (modePlayers != ModePlayers.GrabBoth && c2.parent.IsPlayer)
                            {
                                if (modePlayers == ModePlayers.GrabNone) return;
                                if (modePlayers == ModePlayers.GrabSelf && c2.parent != parent) return;
                                if (modePlayers == ModePlayers.GrabOtherPlayers && c2.parent == parent) return;
                            }
                            float dist = Vector2.Distance(c1.pos, c2.pos);
                            if (dist <= scoopReach)
                            {
                                count++;
                                capturedNodes.Add(c2.parent);
                            }
                        };
                        shovelNode.room.gridsystemAffect.retrieveOffsetArraysAffect(shovelNode.body, del, scoopReach * 2);
                        shovelLink.targets = capturedNodes;
                        shovelLink.formation.UpdateFormation();
                        shovelLink.active = true;
                        shovelNode.room.AllActiveLinks.Add(shovelLink);
                        compoundedMass = 0f;
                        foreach(Node n in capturedNodes)
                        {
                            n.collision.active = false;
                            compoundedMass += n.body.mass;
                        }
                    }
                }

            }

        }
        public override void Draw()
        {
            Color col = Color.White;

            Vector2 position = shovelNode.body.pos;
            if (position == Vector2.Zero) position = parent.body.pos;
            else
            {
                parent.room.camera.DrawLine(position, parent.body.pos, 2f, col, Layers.Over3);
            }
        }

        public override void OnRemove(Node other)
        {
            shovelNode.OnDeath(other);
        }
    }
}