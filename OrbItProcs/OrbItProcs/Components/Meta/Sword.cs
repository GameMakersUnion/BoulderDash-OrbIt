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
        public Node sword;

        public enum swordState
        {
            sheathed,
            stabbing,
            swinging,
            cooldown
        }

        public const mtypes CompType = mtypes.playercontrol|mtypes.minordraw;// | mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }

        public float distance { get; set; }
        public float length{get;set;}
        Vector2 target;
        bool enabled; 
        public float swordLength { get; set; }

        public float swordWidth { get; set; }
        public int swingRate { get; set; }
        private int swingRateCount = 0;

        public int speed { get; set; }
        public Sword() : this(null) { }
        public Sword(Node parent)
        {
            this.parent = parent;
            swingRate = 5;
            distance = 60;
            swordLength = 40;
            swordWidth = 5;
            speed = 3;



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
            //Node.cloneNode(parent.Game1.ui.sidebar.ActiveDefaultNode, sword);
            parent.body.texture = textures.orientedcircle;
            Polygon poly = new Polygon();
            poly.body = sword.body;
            poly.SetBox(swordWidth, swordLength);
            
            sword.body.shape = poly;
            sword.body.pos = parent.body.pos;
            sword.body.DrawCircle = false;
            ///parent.room.game.spawnNode(sword);


            parent.room.itemGroup.IncludeEntity(sword);
            sword.OnSpawn();
            sword.body.AddExclusionCheck(parent.body);
            //sword.body.exclusionList.Add(parent.body);
            //
            //parent.body.exclusionList.Add(sword.body);
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
                sword.movement.active = false;
                //sword.body.velocity = Utils.AngleToVector(sword.body.orient + (float)Math.PI/2) * 100;
                sword.body.velocity = sword.body.effvelocity * 500;

                if (fc.newGamePadState.ThumbSticks.Right.LengthSquared() > 0.9 * 0.9)
                {
                    target = fc.newGamePadState.ThumbSticks.Right;
                    enabled = true;
                    target.Normalize();
                    target *= distance;
                    target *= new Vector2(1, -1);
                    target += parent.body.pos;
                    sword.body.pos = Vector2.Lerp(sword.body.pos, target, 0.1f);
                    //sword.body.pos = target + parent.body.pos;
                    Vector2 result = sword.body.pos - parent.body.pos;
                    sword.body.SetOrientV2(result);
                    
                }
                else
                {
                    enabled = false;
                    Vector2 restPos = new Vector2(parent.body.radius, 0).Rotate(parent.body.orient) + parent.body.pos;
                    sword.body.pos = Vector2.Lerp(sword.body.pos, restPos, 0.1f);
                    sword.body.orient = Utils.AngleLerp(sword.body.orient, parent.body.orient, 0.1f);
                }

                //sword.body.pos = position;
                
            }
        }

        public override void Draw()
        {
            base.Draw();
            parent.room.camera.Draw(textures.sword, sword.body.pos, parent.body.color,sword.body.scale*2, sword.body.orient);
        }
        public override void Death(Node other)
        {
            sword.OnDeath(other);
        }
    }
}
