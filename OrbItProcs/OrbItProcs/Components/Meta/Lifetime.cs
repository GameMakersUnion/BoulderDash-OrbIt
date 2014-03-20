using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
namespace OrbItProcs
{
    /// <summary>
    /// This node will only exist for a short time.
    /// </summary>
    [Info(UserLevel.User, "This node will only exist for a short time.", mtypes.affectself)]
    public class Lifetime : Component
    {
        public const mtypes CompType = mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }
        /// <summary>
        /// If enabled, this node will be deleted when it's lifetime has reached or surpassed this number.
        /// </summary>
        [Info(UserLevel.User, "If enabled, this node will be deleted when it's lifetime has reached or surpassed this number.")]
        public Toggle<int> timeOfDeath { get; set; }

        /// <summary>
        /// How many seconds this node has been alive
        /// </summary>
        [Info(UserLevel.User, "How many seconds have passed since this node was spawned")]
        public int lifetime = 0;


        public Lifetime() : this(null) { }
        public Lifetime(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.lifetime; 
            timeOfDeath = new Toggle<int>(5000);
        }

        public override void OnSpawn()
        {
 	         base.OnSpawn();
             lifetime = 0;
        }

        public override void AffectSelf()
        {
            int mill = Game1.GlobalGameTime.ElapsedGameTime.Milliseconds;
            lifetime += mill;
            if (timeOfDeath.enabled && lifetime > timeOfDeath)
            {
                Die();
            }

        }

        public void Die()
        {
            parent.active = false;
            parent.IsDeleted = true;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
        }

    }
}
