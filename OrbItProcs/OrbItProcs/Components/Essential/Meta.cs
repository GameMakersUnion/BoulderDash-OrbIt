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
        public Toggle<float> score { get; set; }
        public Toggle<float> maxHealth { get; set; }
        public float currentHealth { get; set; }
        public Toggle<float> worth { get; set; }
        public Toggle<float> armour { get; set; }
        public DamageMode damageMode { get; set; }
        public AIMode aimode { get; set; }
        public bool deadly { get; set; }
        public Action<Node, Node> OnDeath { get; set; }
        public Action<Node, Node, int> OnTakeDamage { get; set; }

        public Meta() : this(null) { }
        public Meta(Node parent)
        {
            this.parent = parent;
            score = new Toggle<float>(0, false);
            maxHealth = new Toggle<float>(100, false);
            currentHealth = maxHealth.value;
            worth = new Toggle<float>(1, false);
            armour = new Toggle<float>(1, false);
            damageMode = DamageMode.Nothing;
            aimode = AIMode.None;
            deadly = false;
            OnDeath = Die;
        }
        public override void AffectSelf()
        {
            
        }
        public void TakeDamage(Node n, Node other, int damage)
        {

        }
        public void Die(Node n, Node other)
        {

        }
    }
}
