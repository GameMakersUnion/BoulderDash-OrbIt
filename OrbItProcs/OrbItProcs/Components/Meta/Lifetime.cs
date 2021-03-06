﻿using System;
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
    [Info(UserLevel.User, "This node will only exist for a short time.", CompType)]
    public class Lifetime : Component
    {
        public const mtypes CompType = mtypes.affectself;
        public override mtypes compType { get { return CompType; } set { } }
        /// <summary>
        /// If enabled, this node will be deleted when it's lifetime has reached or surpassed this number.
        /// </summary>
        [Info(UserLevel.User, "If enabled, this node will be deleted when it's lifetime has reached or surpassed this number.")]
        public Toggle<int> timeUntilDeath { get { return _timeUntilDeath; } set { _timeUntilDeath = value; if (value.enabled) lifeLived = 0; } }
        private Toggle<int> _timeUntilDeath;
        private int lifeLived = 0;
        /// <summary>
        /// How many milliseconds this node has been alive
        /// </summary>
        [Info(UserLevel.Developer, "How many milliseconds have passed since this node was spawned")]
        public int lifetime { get; set; }


        public Lifetime() : this(null) { }
        public Lifetime(Node parent = null)
        {
            if (parent != null) this.parent = parent;
            com = comp.lifetime; 
            timeUntilDeath = new Toggle<int>(5000, false);

        }

        public override void OnSpawn()
        {
 	         base.OnSpawn();
             lifetime = 0;
        }

        public override void AffectSelf()
        {
            int mill = OrbIt.gametime.ElapsedGameTime.Milliseconds;
            lifetime += mill;
            if (timeUntilDeath.enabled && lifetime > timeUntilDeath)
            {
                lifeLived += mill;
                if (lifeLived > timeUntilDeath)
                    parent.OnDeath(null);
            }
        }
    }
}
