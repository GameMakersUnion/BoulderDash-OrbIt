﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace OrbItProcs
{
    public enum ShootMode
    {
        Rapid,
        Single,
        Burst,
        Auto,
    }
    /// <summary>
    /// Shoots out damaging lasers that are automatic, single fire or rapid firing.
    /// </summary>
    [Info(UserLevel.User, "Shoots out damaging lasers that are automatic, single fire or rapid firing.", CompType)]
    public class Shooter : Component
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
                if (bulletNode != null)
                {
                    bulletNode.active = value;
                }
            }
        }
        public static Node bulletNode;
        public const mtypes CompType = mtypes.playercontrol | mtypes.minordraw | mtypes.item | mtypes.aicontrol;// | mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }
        /// <summary>
        /// The mode in which to fire nodes. This will change the way input is handled.
        /// </summary>
        [Info(UserLevel.User, "The mode in which to fire nodes. This will change the way input is handled.")]
        public ShootMode shootMode { get; set; }
        /// <summary>
        /// The amount of time the bullet will live before disappearing.
        /// </summary>
        [Info(UserLevel.User, "The amount of time the bullet will live before disappearing.")]
        public int bulletLife { get; set; }

        /// <summary>
        /// The higher the delay, the slower you will shoot.
        /// </summary>
        [Info(UserLevel.User, "The higher the delay, the slower you will shoot.")]
        public int shootingDelay { get; set; }
        private int shootingRateCount = 0;
        /// <summary>
        /// The speed at which bullets will travel.
        /// </summary>
        [Info(UserLevel.User, "The speed at which bullets will travel.")]
        public float speed { get; set; }
        /// <summary>
        /// The amount of damage each bullet will inflict on the node it collides with.
        /// </summary>
        [Info(UserLevel.User, "The amount of damage each bullet will inflict on the node it collides with.")]
        public float damage { get; set; }
        /// <summary>
        /// If enabled, the bullet's velocity will be determined by the controller stick's distance from the center of the stick.
        /// </summary>
        [Info(UserLevel.User, "If enabled, the bullet's velocity will be determined by the controller stick's distance from the center of the stick.")]
        public bool useStickVelocity { get; set; }
        private Toggle<int> _maxAmmo;
        /// <summary>
        /// Max ammo
        /// </summary>
        [Info(UserLevel.User, "Max ammo")]
        public Toggle<int> maxAmmo { get { return _maxAmmo; } set { _maxAmmo = value; ammo = value; } }
        private int ammo;
        /// <summary>
        /// If the shooter is a turret, it will fire at players.
        /// </summary>
        [Info(UserLevel.User, "If the shooter is a turret, it will fire at players.")]
        public bool isTurret
        {
            get { return _isTurret; }
            set
            {
                _isTurret = value;
                if (value)
                {
                    parent.IsAI = true;
                }
                else
                {
                    parent.IsAI = false;
                }
            }
        }
        private bool _isTurret = false;
        /// <summary>
        /// The time interval between which the AI will shoot a bullet from the turret.
        /// </summary>
        [Info(UserLevel.User, "The time interval between which the AI will shoot a bullet from the turret.")]
        public float TurretTimerSeconds { get; set; }
        private float tempTurretTimer = 0;
        public Shooter() : this(null) { }
        public Shooter(Node parent)
        {
            this.parent = parent;
            bulletLife = 700;
            shootingDelay = 50;
            speed = 15f;
            damage = 10f;
            shootMode = ShootMode.Auto;
            useStickVelocity = false;
            maxAmmo = new Toggle<int>(50, true);
            TurretTimerSeconds = 1;
            
        }
        public override void OnSpawn()
        {
            AppointmentDelegate ap = (n,d) => 
                {
                    if (ammo < maxAmmo) ammo++;
                };
            parent.scheduler.AddAppointment(new Appointment(ap, 5000, infinite: true));
        }
        public static void MakeBullet(Room room)
        {
            Dictionary<dynamic, dynamic> props = new Dictionary<dynamic, dynamic>()
            {
                { nodeE.radius, 5f},
                { comp.movement, true},
                { comp.basicdraw, true},
                { comp.collision, true},
                { comp.laser, true},
                { comp.colorchanger, true},
                { comp.lifetime, true},
                //{comp.waver, true},
            };
            bulletNode = new Node(room, props);
            bulletNode.Comp<Collision>().isSolid = false;
            bulletNode.body.isSolid = true;
            bulletNode.body.restitution = 1f;
            bulletNode.Comp<ColorChanger>().colormode = ColorChanger.ColorMode.hueShifter;
            bulletNode.Comp<Lifetime>().timeUntilDeath.enabled = true;
            bulletNode.Comp<Laser>().thickness = 5f;
            bulletNode.Comp<Laser>().laserLength = 20;
            bulletNode.Comp<Movement>().randInitialVel.enabled = false;
            bulletNode.group = room.bulletGroup;
            
            
        }
        public override void PlayerControl(Controller controller)
        {
            if (controller is FullController)
            {
                FullController fc = (FullController)controller;
                if (shootMode == ShootMode.Rapid)
                {
                    if (fc.newGamePadState.IsButtonDown(Buttons.RightTrigger))
                    {
                        if (shootingRateCount++ % shootingDelay == 0)
                        {
                            FireNode(fc.newGamePadState.ThumbSticks.Right);
                        }
                    }
                    else
                    {
                        shootingRateCount = 0;
                    }
                }
                else if (shootMode == ShootMode.Single)
                {
                    //if (fc.newGamePadState.IsButtonDown(Buttons.RightTrigger) && fc.oldGamePadState.IsButtonUp(Buttons.RightTrigger))
                    if (fc.newGamePadState.Triggers.Right > 0.5 && fc.oldGamePadState.Triggers.Right < 0.5)
                    {
                        if (shootingRateCount++ % shootingDelay == 0)
                        {
                            FireNode(fc.newGamePadState.ThumbSticks.Right);
                        }
                    }
                    else
                    {
                        shootingRateCount = 0;
                    }
                }
                else if (shootMode == ShootMode.Auto)
                {
                    //if (fc.newGamePadState.IsButtonDown(Buttons.RightTrigger) && fc.oldGamePadState.IsButtonUp(Buttons.RightTrigger))
                    if (fc.newGamePadState.ThumbSticks.Right != Vector2.Zero)
                    {
                        if (shootingRateCount++ % shootingDelay == 0)
                        {
                            FireNode(fc.newGamePadState.ThumbSticks.Right);
                        }
                    }
                    else
                    {
                        shootingRateCount = 0;
                    }
                }
            }
        }
        
        public override void AIControl(AIMode aiMode)
        {
            //if (aiMode == AIMode.)
            tempTurretTimer += OrbIt.gametime.ElapsedGameTime.Milliseconds;
            if (tempTurretTimer > TurretTimerSeconds * 1000)
            {
                
                float nearest = float.MaxValue;
                Node nearNode = null;
                foreach (Node n in parent.room.playerGroup.entities)
                {
                    float dist = Vector2.Distance(parent.body.pos, n.body.pos);
                    if (dist < nearest)
                    {
                        nearNode = n;
                        nearest = dist;
                    }
                }
                if (nearNode != null)
                {
                    Vector2 dir = nearNode.body.pos - parent.body.pos;
                    VMath.NormalizeSafe(ref dir);
                    dir.Y *= -1;
                    FireNode(dir);
                    tempTurretTimer = 0;
                }
            }
        }
        public override void Draw()
        {
            Meta.drawBar(parent, 0.5f, (float)ammo / (float)maxAmmo, true, Color.Goldenrod);
        }
        public void FireNode(Vector2 dir)
        {
            ammo--;
            if (!useStickVelocity) VMath.NormalizeSafe(ref dir);
            Node n = bulletNode.CreateClone(parent.room);
            n.Comp<Lifetime>().timeUntilDeath.value = bulletLife;
            dir.Y *= -1;
            n.body.velocity = dir * speed;
            n.body.pos = parent.body.pos;
            n.body.AddExclusionCheck(parent.body);
            //n.body.AddExclusion(parent.body);
            if (parent.HasComp<Sword>())
            {
                n.body.AddExclusionCheck(parent.Comp<Sword>().swordNode.body);
                //n.body.AddExclusion(parent.Comp<Sword>().sword.body);
            }
            if (parent.IsPlayer)
            {
                n.Comp<ColorChanger>().colormode = ColorChanger.ColorMode.none;
                n.SetColor(parent.player.pColor);
            }
            parent.room.spawnNode(n, g: parent.room.bulletGroup);
            CollisionDelegate bulletHit = (n1, n2) =>
            {
                Node bullet, them;
                if (n1 == n)
                {
                    bullet = n1;
                    them = n2;
                }
                else if (n2 == n)
                {
                    bullet = n2;
                    them = n1;
                }
                else
                {
                    return;
                }
                if (parent.meta.damageMode == Meta.DamageMode.OnlyPlayers)
                {
                    if (!them.IsPlayer) return;
                }
                else if (parent.meta.damageMode == Meta.DamageMode.OnlyNonPlayers)
                {
                    if (them.IsPlayer) return;
                }
                else if (parent.meta.damageMode == Meta.DamageMode.Nothing)
                {
                    return;
                }
                them.meta.CalculateDamage(parent, damage);
                bullet.OnDeath(null);
            };
            n.body.OnCollisionEnter += bulletHit;
            //n.body.isSolid = false;
            if (ammo <= 0)
            {
                parent.RemoveComponent(com);
            }
        }

    }
}
