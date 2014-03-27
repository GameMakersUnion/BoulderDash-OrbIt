using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace OrbItProcs
{
    public class Shooter : Component
    {
        public static Node bulletNode;
        public const mtypes CompType = mtypes.playercontrol;// | mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }
        public int bulletLife { get; set; }
        public int shootingRate { get; set; }
        private int shootingRateCount = 0;
        public float speed { get; set; }
        public Shooter() : this(null) { }
        public Shooter(Node parent)
        {
            this.parent = parent;
            bulletLife = 1000;
            shootingRate = 5;
            speed = 15f;
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
        }
        public void FireNode(Vector2 dir)
        {
            Node n = bulletNode.CreateClone();
            n.Comp<Lifetime>().timeUntilDeath.value = bulletLife;
            dir.Y *= -1;
            n.body.velocity = dir * speed;
            n.body.pos = parent.body.pos;
            parent.room.game.spawnNode(n);
        }
    }
}
