using System;
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
    public class Shooter : Component
    {
        public static Node bulletNode;
        public const mtypes CompType = mtypes.playercontrol;// | mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }
        public ShootMode shootMode { get; set; }
        public int bulletLife { get; set; }
        
        
        public int shootingRate { get; set; }
        private int shootingRateCount = 0;
        public float speed { get; set; }
        public float damage { get; set; }
        public bool useStickVelocity { get; set; }
        public Shooter() : this(null) { }
        public Shooter(Node parent)
        {
            this.parent = parent;
            bulletLife = 700;
            shootingRate = 50;
            speed = 15f;
            damage = 10f;
            shootMode = ShootMode.Auto;
            useStickVelocity = false;
        }
        public static void MakeBullet()
        {
            Dictionary<dynamic, dynamic> props = new Dictionary<dynamic, dynamic>()
            {
                {nodeE.radius, 5f},
                {comp.movement, true},
                {comp.basicdraw, true},
                {comp.collision, true},
                {comp.laser, true},
                {comp.colorchanger, true},
                {comp.lifetime, true},
                //{comp.waver, true},
            };
            bulletNode = new Node(props);
            bulletNode.Comp<Collision>().isSolid = false;
            bulletNode.body.isSolid = false;
            bulletNode.Comp<ColorChanger>().colormode = ColorChanger.ColorMode.hueShifter;
            bulletNode.Comp<Lifetime>().timeUntilDeath.enabled = true;
            bulletNode.Comp<Laser>().thickness = 5f;
            bulletNode.Comp<Laser>().laserLength = 20;
            bulletNode.Comp<Movement>().randInitialVel.enabled = false;
            
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
                        if (shootingRateCount++ % shootingRate == 0)
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
                        if (shootingRateCount++ % shootingRate == 0)
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
                        if (shootingRateCount++ % shootingRate == 0)
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
        public void FireNode(Vector2 dir)
        {
            if (!useStickVelocity) VMath.NormalizeSafe(ref dir);
            Node n = bulletNode.CreateClone();
            n.Comp<Lifetime>().timeUntilDeath.value = bulletLife;
            dir.Y *= -1;
            n.body.velocity = dir * speed;
            n.body.pos = parent.body.pos;
            n.body.AddExclusion(parent.body);
            if (parent.HasComp<Sword>())
            {
                n.body.AddExclusion(parent.Comp<Sword>().sword.body);
            }
            if (parent.player != null)
            {
                n.Comp<ColorChanger>().colormode = ColorChanger.ColorMode.none;
                n.SetColor(parent.player.pColor);
            }
            parent.room.game.spawnNode(n);
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
                    if (them.player == null) return;
                }
                else if (parent.meta.damageMode == Meta.DamageMode.OnlyNonPlayers)
                {
                    if (them.player != null) return;
                }
                else if (parent.meta.damageMode == Meta.DamageMode.Nothing)
                {
                    return;
                }
                them.meta.TakeDamage(parent, damage);
                bullet.OnDeath(null);
            };
            n.body.OnCollisionEnter += bulletHit;
        }

    }
}
