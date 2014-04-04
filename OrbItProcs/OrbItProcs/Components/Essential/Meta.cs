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
    [Flags]
    public enum ItemSlots
    {
        None = 0,
        A_Green = 1,
        B_Red = 2,
        X_Blue = 4,
        Y_Yellow = 8,
        All = 15,
    }
    /// <summary>
    /// The Meta component hold information about the node such as health, shields, and player/ai modes.
    /// </summary>
    [Info(UserLevel.User, "The Meta component hold information about the node such as health, shields, and player/ai modes.", CompType)]
    public class Meta : Component
    {
        public enum DamageMode
        {
            OnlyPlayers,
            OnlyNonPlayers,
            Everything,
            Nothing,
        }
        public enum HealthBarMode
        {
            Fade,
            Bar,
            none,
        }
        public const mtypes CompType = mtypes.affectself | mtypes.essential | mtypes.minordraw;//mtypes.affectother 
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
        /// Select how to indicate health
        /// </summary>
        [Info(UserLevel.User, "Select how to indicate health")]
        public HealthBarMode healthBar { get; set; }
        //[Info(UserLevel.Developer)]
        //public ItemSlots itemSlots { get; set; }
        /// <summary>
        /// If enabled, the health bar will rotate.
        /// </summary>
        [Info(UserLevel.Advanced, "If enabled, the health bar will rotate.")]
        public bool rotateHealthBar { get; set; }
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
        /// This affects the amount of damage this node will do when attacking other nodes.
        /// </summary>
        [Info(UserLevel.User, "This affects the amount of damage this node will do when attacking other nodes.")]
        public float damageMultiplier { get; set; }

        /// <summary>
        /// The radius of the node.
        /// </summary>
        [Info(UserLevel.User, "The radius of the node.")]
        public float radius { get { return parent != null ? parent.body.radius : 25; } set { if (parent != null) parent.body.radius = value; } }
        private float lightRotation = 0f;
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
            damageMode = DamageMode.OnlyPlayers;
            AImode = AIMode.None;
            deadly = false;
            active = true;
            damageMultiplier = 1f;
        }
        public override void AffectSelf()
        {
            
        }
        public void CalculateDamage(Node other, float damage)
        {
            if (maxHealth.enabled)
            {
                float resultingDamage = damage * damageMultiplier;
                currentHealth = (float)Math.Max(currentHealth - resultingDamage, 0);
                currentHealth = (float)Math.Min(currentHealth, maxHealth.value);
                float percent = 0.65f;
                if (healthBar == HealthBarMode.Fade) parent.body.color = parent.body.permaColor * ((currentHealth / maxHealth.value) * percent + (1f - percent));
                if (currentHealth <= 0)
                {
                    //Die(other);
                    parent.OnDeath(other);
                }
            }
        }
        public override void Draw()
        {
            if (healthBar == HealthBarMode.Bar) {
                
                float healthRatio = (float)currentHealth / (float)maxHealth;
                float baseRotation = rotateHealthBar ? parent.body.orient : 0f;
                float rotation = baseRotation + (1f - healthRatio) * VMath.twoPI;
                float rotation2 = baseRotation;
                Color c; 
                Layers hideLayer = Layers.Over1;
                if (healthRatio > 0.75f) c = Color.Lime;
                else if (healthRatio > 0.5f) c = Color.GreenYellow;
                else if (healthRatio > 0.25f) { c = Color.Orange; hideLayer = Layers.Over3; rotation2 = rotation - VMath.PI; }
                else { c = Color.Red; hideLayer = Layers.Over3; rotation2 = rotation - VMath.PI; }

                parent.room.camera.Draw(textures.outerL, parent.body.pos, Color.Black, parent.body.scale, baseRotation, hideLayer);
                parent.room.camera.Draw(textures.outerR, parent.body.pos, Color.Black, parent.body.scale, baseRotation, Layers.Over1);
                parent.room.camera.Draw(textures.innerL, parent.body.pos, c, parent.body.scale, rotation, Layers.Over2);
                parent.room.camera.Draw(textures.innerR, parent.body.pos, c, parent.body.scale,rotation2, Layers.Over2);

            }
            if (parent.player != null)
            {
                parent.room.camera.Draw(textures.pointer, parent.body.pos, Color.Gold, parent.body.scale, parent.body.orient, Layers.Over4);
                ItemSlots itemSlots = parent.player.occupiedSlots;
                textures A, B, X, Y;
                A = (itemSlots & ItemSlots.A_Green) == ItemSlots.A_Green ? textures.itemLight : textures.itemWhisper;
                B = (itemSlots & ItemSlots.B_Red) == ItemSlots.B_Red ? textures.itemLight : textures.itemWhisper;
                X = (itemSlots & ItemSlots.X_Blue) == ItemSlots.X_Blue ? textures.itemLight : textures.itemWhisper;
                Y = (itemSlots & ItemSlots.Y_Yellow) == ItemSlots.Y_Yellow ? textures.itemLight : textures.itemWhisper;

                parent.room.camera.Draw(A, parent.body.pos, Color.Green, parent.body.scale * 1.7f, lightRotation, Layers.Under2);
                parent.room.camera.Draw(B, parent.body.pos, Color.Red, parent.body.scale * 1.7f, lightRotation + VMath.PIbyTwo, Layers.Under2);
                parent.room.camera.Draw(X, parent.body.pos, Color.Blue, parent.body.scale * 1.7f, lightRotation + VMath.PI, Layers.Under2);
                parent.room.camera.Draw(Y, parent.body.pos, Color.Yellow, parent.body.scale * 1.7f, lightRotation + VMath.PI + VMath.PIbyTwo, Layers.Under2);

                lightRotation += 0.1f;
            }

        }
        public override void OnRemove(Node other)
        {
            if (OnDeath != null) OnDeath(parent, other);
        }


    }
}

