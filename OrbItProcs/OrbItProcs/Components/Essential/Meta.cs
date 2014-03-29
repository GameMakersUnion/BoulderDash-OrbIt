using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace OrbItProcs
{
    public enum AIMode
    {
        Agro,
        Friendly,
        Neutral,
        Player,
        None,
    }
    /// <summary>
    /// The Meta component hold information about the node such as health, shields, and player/ai modes.
    /// </summary>
    [Info(UserLevel.User, "The Meta component hold information about the node such as health, shields, and player/ai modes.")]
    public class Meta : Component
    {
        public enum DamageMode
        {
            OnlyPlayers,
            OnlyNonPlayers,
            Everything,
            Nothing,
        }
        public const mtypes CompType = mtypes.affectself | mtypes.essential;//mtypes.affectother | mtypes.minordraw;
        public override mtypes compType { get { return CompType; } set { } }
        /// <summary>
        /// The score of the player. If enabled and the player reaches the max score, the game ends.
        /// </summary>
        [Info(UserLevel.User, "The score of the player. If enabled and the player reaches the max score, the game ends.")]
        public Toggle<float> score { get; set; }
        /// <summary>
        /// The maximum health the node. When it reaches 0, the node dies.
        /// </summary>
        [Info(UserLevel.User, "The maximum health the node. When it reaches 0, the node dies.")]
        public Toggle<float> maxHealth { get; set; }
        /// <summary>
        /// The current health of the node.
        /// </summary>
        [Info(UserLevel.User, "The current health of the node.")]
        public float currentHealth { get; set; }
        /// <summary>
        /// The worth of the node represents how likely it is that the node will drop valuable loot upon death.
        /// </summary>
        [Info(UserLevel.User, "The worth of the node represents how likely it is that the node will drop valuable loot upon death.")]
        public Toggle<float> worth { get; set; }
        /// <summary>
        /// Represents the amount of damage mitigation upon losing health.
        /// </summary>
        [Info(UserLevel.User, "Represents the amount of damage mitigation upon losing health.")]
        public Toggle<float> armour { get; set; }
        /// <summary>
        /// The damageMode describes who this node can hurt.
        /// </summary>
        [Info(UserLevel.User, "The damageMode describes who this node can hurt.")]
        public DamageMode damageMode { get; set; }
        /// <summary>
        /// If this node is controlled by an AI, this will determine it's behaviour.
        /// </summary>
        [Info(UserLevel.User, "If this node is controlled by an AI, this will determine it's behaviour.")]
        public AIMode AImode { get; set; }
        /// <summary>
        /// If the node is deadly, this node will kill other nodes on contact.
        /// </summary>
        [Info(UserLevel.User, "If the node is deadly, this node will kill other nodes on contact.")]
        public bool deadly { get; set; }

        /// <summary>
        /// The radius of the node.
        /// </summary>
        [Info(UserLevel.User, "The radius of the node.")]
        public float radius { get { return parent != null ? parent.body.radius : 25; } set { if (parent != null) parent.body.radius = value; } }

        public Action<Node, Node> OnDeath { get; set; }
        public Action<Node, Node, int> OnTakeDamage { get; set; }
        public Meta() : this(null) { }
        public Meta(Node parent)
        {
            this.parent = parent;
            score = new Toggle<float>(0, false);
            maxHealth = new Toggle<float>(100, true);
            currentHealth = maxHealth.value;
            worth = new Toggle<float>(1, false);
            armour = new Toggle<float>(1, false);
            damageMode = DamageMode.Everything;
            AImode = AIMode.None;
            deadly = false;
            active = true;
        }
        public override void AffectSelf()
        {
            
        }
        public void TakeDamage(Node other, float damage)
        {
            if (maxHealth.enabled)
            {
                currentHealth = (float)Math.Max(currentHealth - damage, 0);
                currentHealth = (float)Math.Min(currentHealth, maxHealth.value);
                float percent = 0.65f;
                parent.body.color = parent.body.permaColor * ((currentHealth / maxHealth.value) * percent + (1f - percent));
                if (currentHealth <= 0)
                {
                    //Die(other);
                    parent.OnDeath(other);
                }
            }
        }
        public override void Death(Node other)
        {
            if (OnDeath != null) OnDeath(parent, other);
        }
    }
}

