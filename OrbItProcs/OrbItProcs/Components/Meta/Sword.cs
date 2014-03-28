using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace OrbItProcs
{
    /// <summary>
        /// This node has a nifty sword he can swing to attack enemies. 
        /// </summary>
        [Info(UserLevel.User, "This node has a nifty sword he can swing to attack enemies. ", CompType)]
    public class Sword : Component
    {
        private Node sword;

        public enum swordState
        {
            sheathed,
            stabbing,
            swinging,
            cooldown
        }

        public const mtypes CompType = mtypes.playercontrol;// | mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }

        public float distance { get; set; }
        public float length{get;set;}
        Vector2 target;
        bool enabled; 
        public float swordLength { get; set; }

        public float swordWidth { get; set; }
        public int swingRate { get; set; }
        private int swingRateCount = 0;
        public Sword() : this(null) { }
        public Sword(Node parent)
        {
            this.parent = parent;
            swingRate = 5;
            distance = 60;
            swordLength = 40;
            swordWidth = 5;


            Dictionary<dynamic, dynamic> props = new Dictionary<dynamic, dynamic>()
            {
                {comp.movement, true},
                {comp.basicdraw, true},
                {comp.collision, true},
                //{comp.waver, true},
            };


            sword = new Node(props);


           // Polygon poly = new Polygon();
           // poly.body = sword.body;
           // poly.SetBox(100, 110);
           // sword.body.shape = poly;
           // //sword.body.pos = position;

            //Node newNode = new Node();

            //room.game.spawnNode(newNode);
            

        }

        public override void OnSpawn()
        {
            //Node.cloneNode(parent.room.game.ui.sidebar.ActiveDefaultNode, sword);
            Polygon poly = new Polygon();
            poly.body = sword.body;
            poly.SetBox(swordWidth, swordLength);
            
            sword.body.shape = poly;
            sword.body.pos = parent.body.pos;
            sword.body.DrawCircle = false;
            parent.room.game.spawnNode(sword);
            sword.body.exclusionList.Add(parent.body);
            parent.body.exclusionList.Add(sword.body);
        }
        public override void AffectSelf()
        {
            base.AffectSelf();
        }
        public override void PlayerControl(Controller controller)
        {
            if (controller is FullController)
            {
                FullController fc = (FullController)controller;
                target = fc.newGamePadState.ThumbSticks.Right;
                if (target.LengthSquared() > 0.5 * 0.5)
                {
                    enabled = true;
                    target.Normalize();
                    target *= distance;
                    target *= new Vector2(1, -1);
                    sword.body.pos = target + parent.body.pos;

                }
                else
                {
                    enabled = false;
                    sword.body.pos = parent.body.pos;
                }

                //sword.body.pos = position;
                
            }
        }
        //public void FireNode(Vector2 dir)
        //{
        //    Node n = bulletNode.CreateClone();
        //    n.Comp<Lifetime>().timeUntilDeath.value = bulletLife;
        //    dir.Y *= -1;
        //    n.body.velocity = dir * speed;
        //    n.body.pos = parent.body.pos;
        //    parent.room.game.spawnNode(n);
        //}


    }
}
